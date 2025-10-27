using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volur.Application.Common;
using Volur.Application.Configuration;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Application.Mappers;
using Volur.Domain.Entities;
using Volur.Shared;

namespace Volur.Application.UseCases.GetSymbols;

/// <summary>
/// Handler for GetSymbolsQuery.
/// Implements read-through cache pattern with pagination and search.
/// </summary>
public sealed class GetSymbolsHandler
{
    private readonly ISymbolRepository _symbolRepository;
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IStockDataRepository _stockDataRepository;
    private readonly IStockDataProvider _stockDataProvider;
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<GetSymbolsHandler> _logger;
    private readonly CacheTtlOptions _cacheTtl;

    public GetSymbolsHandler(
        ISymbolRepository symbolRepository,
        IExchangeRepository exchangeRepository,
        IStockDataRepository stockDataRepository,
        IStockDataProvider stockDataProvider,
        IEodhdClient eodhdClient,
        ILogger<GetSymbolsHandler> logger,
        IOptions<CacheTtlOptions> cacheTtl)
    {
        _symbolRepository = symbolRepository;
        _exchangeRepository = exchangeRepository;
        _stockDataRepository = stockDataRepository;
        _stockDataProvider = stockDataProvider;
        _eodhdClient = eodhdClient;
        _logger = logger;
        _cacheTtl = cacheTtl.Value;
    }

    public async Task<Result<SymbolsResponse>> HandleAsync(GetSymbolsQuery query, CancellationToken cancellationToken = default)
    {
        // Validate exchange exists
        var exchange = await _exchangeRepository.GetByCodeAsync(query.ExchangeCode, cancellationToken);
        if (exchange == null)
        {
            return Result.Failure<SymbolsResponse>(Error.BadExchangeCode(query.ExchangeCode));
        }

        var ttl = TimeSpan.FromHours(_cacheTtl.SymbolsHours);

        // Use "Common Stock" as default type filter if none specified
        var typeFilter = !string.IsNullOrWhiteSpace(query.TypeFilter) ? query.TypeFilter : "Common Stock";
        
        // Try cache first unless force refresh
        if (!query.ForceRefresh)
        {
            var cached = await _symbolRepository.GetByExchangeAsync(
                query.ExchangeCode,
                query.Page,
                query.PageSize,
                query.SearchQuery,
                typeFilter,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

            if (cached.HasValue)
            {
                var (symbols, totalCount, fetchedAt) = cached.Value;
                var expiresAt = fetchedAt!.Value.Add(ttl);
                var ttlRemaining = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;

                if (ttlRemaining > 0)
                {
                    _logger.LogInformation("Symbols cache hit for {ExchangeCode}. TTL remaining: {TtlSeconds}s", 
                        query.ExchangeCode, ttlRemaining);

                    // For enriched field sorting, we need to get a larger subset of data and sort it properly
                    if (IsEnrichedFieldSort(query.SortBy))
                    {
                        // Get a larger subset of symbols from cache for proper sorting
                        var sortingPageSize = Math.Max(500, query.PageSize * 10);
                        var allSymbolsResult = await _symbolRepository.GetByExchangeAsync(
                            query.ExchangeCode,
                            1,
                            sortingPageSize,
                            query.SearchQuery,
                            typeFilter,
                            null, // No sorting at database level for enriched fields
                            null,
                            cancellationToken);
                        
                        if (allSymbolsResult.HasValue)
                        {
                            var (symbolsSubset, _, _) = allSymbolsResult.Value;
                            var enrichedSymbols = await EnrichSymbolsWithFundamentalDataAsync(symbolsSubset, cancellationToken);
                            var sortedSymbols = ApplyClientSideSorting(enrichedSymbols, query.SortBy, query.SortDirection);
                            
                            var hasNext = (query.Page * query.PageSize) < sortedSymbols.Count;
                            var skip = (query.Page - 1) * query.PageSize;
                            var pagedSymbols = sortedSymbols.Skip(skip).Take(query.PageSize).ToList();

                            return Result.Success(new SymbolsResponse(
                                Exchange: exchange.ToDto(),
                                Pagination: new PaginationMetadata(query.Page, query.PageSize, sortedSymbols.Count, hasNext),
                                Items: pagedSymbols,
                                FetchedAt: fetchedAt.Value,
                                Cache: new CacheMetadata("mongo", ttlRemaining)
                            ));
                        }
                    }

                    return await BuildSuccessResponseAsync(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "mongo", ttlRemaining, query.SortBy, query.SortDirection, cancellationToken);
                }

                // If cache expired but we have a search query, return expired cache data instead of refreshing
                if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                {
                    _logger.LogInformation("Symbols cache expired for {ExchangeCode}, but returning expired cache for search query", query.ExchangeCode);
                    return await BuildSuccessResponseAsync(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "mongo", 0, query.SortBy, query.SortDirection, cancellationToken);
                }

                _logger.LogInformation("Symbols cache expired for {ExchangeCode}, fetching from provider", query.ExchangeCode);
            }
            else if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                // No cached data but we have a search query - return empty results instead of refreshing
                _logger.LogInformation("No cached symbols for {ExchangeCode} and search query provided, returning empty results", query.ExchangeCode);
                return await BuildSuccessResponseAsync(exchange, Array.Empty<Symbol>(), query.Page, query.PageSize, 0, DateTime.UtcNow, "none", 0, query.SortBy, query.SortDirection, cancellationToken);
            }
        }
        else
        {
            _logger.LogInformation("Force refresh requested for {ExchangeCode}", query.ExchangeCode);
        }

        // Fetch from provider
        var providerResult = await _eodhdClient.GetSymbolsAsync(query.ExchangeCode, cancellationToken);
        if (providerResult.IsFailure)
        {
            _logger.LogWarning("Failed to fetch symbols for {ExchangeCode} from provider: {Error}", 
                query.ExchangeCode, providerResult.Error);
            return Result.Failure<SymbolsResponse>(providerResult.Error!);
        }

        var providerSymbols = providerResult.Value;
        
        // Filter for common stocks by default (unless a specific type filter is requested)
        var filteredProviderSymbols = providerSymbols;
        if (string.IsNullOrWhiteSpace(query.TypeFilter))
        {
            filteredProviderSymbols = providerSymbols
                .Where(s => s.Type?.Equals("Common Stock", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            
            _logger.LogInformation("Filtered {OriginalCount} symbols to {FilteredCount} common stocks for {ExchangeCode}", 
                providerSymbols.Count, filteredProviderSymbols.Count, query.ExchangeCode);
        }
        
        var domainSymbols = filteredProviderSymbols.Select(s => s.ToDomain(query.ExchangeCode)).ToList();
        var fetchedAtUtc = DateTime.UtcNow;

        // Update cache
        try
        {
            await _symbolRepository.UpsertManyAsync(query.ExchangeCode, domainSymbols, fetchedAtUtc, ttl, cancellationToken);
            _logger.LogInformation("Cached {Count} symbols for {ExchangeCode} with TTL {Hours}h", 
                domainSymbols.Count, query.ExchangeCode, _cacheTtl.SymbolsHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache symbols for {ExchangeCode} in MongoDB", query.ExchangeCode);
            // Continue - we have the data from provider
        }

        // For enriched field sorting, we need to get a larger subset of data, sort it, then paginate
        if (IsEnrichedFieldSort(query.SortBy))
        {
            // Get a larger subset of symbols for enriched field sorting (limit to prevent timeouts)
            // We'll get 10x the page size to have enough data for proper sorting
            var sortingPageSize = Math.Max(500, query.PageSize * 10); // Minimum 500 symbols for sorting
            
            var allSymbolsResult = await _symbolRepository.GetByExchangeAsync(
                query.ExchangeCode,
                1, // Start from page 1
                sortingPageSize, // Get a larger subset
                query.SearchQuery,
                typeFilter,
                null, // No sorting at database level for enriched fields
                null,
                cancellationToken);
            
            if (!allSymbolsResult.HasValue)
            {
                return Result.Success(new SymbolsResponse(
                    Exchange: exchange.ToDto(),
                    Pagination: new PaginationMetadata(query.Page, query.PageSize, 0, false),
                    Items: Array.Empty<SymbolDto>(),
                    FetchedAt: fetchedAtUtc,
                    Cache: new CacheMetadata("provider", (int)ttl.TotalSeconds)
                ));
            }

            var (symbolsSubset, totalCount, _) = allSymbolsResult.Value;
            
            // Enrich the subset of symbols with fundamental data
            var enrichedSymbols = await EnrichSymbolsWithFundamentalDataAsync(symbolsSubset, cancellationToken);
            
            // Apply client-side sorting to the enriched data
            var sortedSymbols = ApplyClientSideSorting(enrichedSymbols, query.SortBy, query.SortDirection);
            
            // Apply pagination to the sorted results
            var hasNext = (query.Page * query.PageSize) < sortedSymbols.Count;
            var skip = (query.Page - 1) * query.PageSize;
            var pagedSymbols = sortedSymbols.Skip(skip).Take(query.PageSize).ToList();

            return Result.Success(new SymbolsResponse(
                Exchange: exchange.ToDto(),
                Pagination: new PaginationMetadata(query.Page, query.PageSize, sortedSymbols.Count, hasNext),
                Items: pagedSymbols,
                FetchedAt: fetchedAtUtc,
                Cache: new CacheMetadata("provider", (int)ttl.TotalSeconds)
            ));
        }
        else
        {
            // For database fields, use the existing database-level sorting with pagination
            var finalResult = await _symbolRepository.GetByExchangeAsync(
                query.ExchangeCode,
                query.Page,
                query.PageSize,
                query.SearchQuery,
                typeFilter,
                query.SortBy,
                query.SortDirection,
                cancellationToken);

            if (!finalResult.HasValue)
            {
                // Shouldn't happen, but handle gracefully
                return Result.Success(new SymbolsResponse(
                    Exchange: exchange.ToDto(),
                    Pagination: new PaginationMetadata(query.Page, query.PageSize, 0, false),
                    Items: Array.Empty<SymbolDto>(),
                    FetchedAt: fetchedAtUtc,
                    Cache: new CacheMetadata("provider", (int)ttl.TotalSeconds)
                ));
            }

            var (filteredSymbols, finalTotalCount, _) = finalResult.Value;
            return await BuildSuccessResponseAsync(exchange, filteredSymbols, query.Page, query.PageSize, finalTotalCount, fetchedAtUtc, "provider", (int)ttl.TotalSeconds, query.SortBy, query.SortDirection, cancellationToken);
        }
    }

    private async Task<Result<SymbolsResponse>> BuildSuccessResponseAsync(
        Domain.Entities.Exchange exchange,
        IReadOnlyList<Domain.Entities.Symbol> symbols,
        int page,
        int pageSize,
        int totalCount,
        DateTime fetchedAt,
        string cacheSource,
        int ttlSeconds,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken)
    {
        var hasNext = (page * pageSize) < totalCount;

        // Fetch fundamental data for all symbols in parallel
        var symbolDtos = await EnrichSymbolsWithFundamentalDataAsync(symbols, cancellationToken);

        // Apply client-side sorting for enriched fields if needed
        var sortedSymbols = ApplyClientSideSorting(symbolDtos, sortBy, sortDirection);

        return Result.Success(new SymbolsResponse(
            Exchange: exchange.ToDto(),
            Pagination: new PaginationMetadata(page, pageSize, totalCount, hasNext),
            Items: sortedSymbols,
            FetchedAt: fetchedAt,
            Cache: new CacheMetadata(cacheSource, ttlSeconds)
        ));
    }

    private async Task<List<SymbolDto>> EnrichSymbolsWithFundamentalDataAsync(
        IReadOnlyList<Domain.Entities.Symbol> symbols,
        CancellationToken cancellationToken)
    {
        if (!symbols.Any())
            return new List<SymbolDto>();

        _logger.LogInformation("Enriching {Count} symbols with fundamental data", symbols.Count);

        // Create tasks to fetch quote and fundamental data for each symbol
        var enrichmentTasks = symbols.Select(async symbol =>
        {
            try
            {
                _logger.LogDebug("Fetching quote and fundamental data for symbol {Ticker}", symbol.Ticker);
                
                // Fetch cached quote and fundamentals data in parallel
                var quoteTask = _stockDataRepository.GetQuoteAsync(symbol.Ticker, cancellationToken);
                var fundamentalsTask = _stockDataRepository.GetFundamentalsAsync(symbol.Ticker, cancellationToken);

                await Task.WhenAll(quoteTask, fundamentalsTask);

                var quoteResult = await quoteTask;
                var fundamentalsResult = await fundamentalsTask;

                // If no cached quote data exists, try to fetch fresh data from provider
                if (quoteResult?.quote == null)
                {
                    try
                    {
                        _logger.LogDebug("No cached quote for {Ticker}, fetching from provider", symbol.Ticker);
                        var freshQuoteResult = await _stockDataProvider.GetQuoteAsync(symbol.FullSymbol, cancellationToken);
                        
                        if (freshQuoteResult.IsSuccess)
                        {
                            var freshQuote = freshQuoteResult.Value;
                            
                            // Cache the fresh quote data for future requests
                            try
                            {
                                await _stockDataRepository.UpsertQuoteAsync(freshQuote, cancellationToken);
                                _logger.LogDebug("Cached fresh quote for {Ticker}", symbol.Ticker);
                            }
                            catch (Exception cacheEx)
                            {
                                _logger.LogWarning(cacheEx, "Failed to cache quote for {Ticker}, continuing with fresh data", symbol.Ticker);
                            }

                            quoteResult = (freshQuote, DateTime.UtcNow);
                        }
                        else
                        {
                            _logger.LogDebug("Failed to fetch fresh quote for {Ticker}: {Error}", symbol.Ticker, freshQuoteResult.Error?.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching fresh quote for {Ticker}", symbol.Ticker);
                    }
                }

                _logger.LogDebug("Symbol {Ticker}: Quote={HasQuote}, Fundamentals={HasFundamentals}", 
                    symbol.Ticker, quoteResult?.quote != null, fundamentalsResult?.fundamentals != null);

                return symbol.ToDto(
                    quote: quoteResult?.quote,
                    fundamentals: fundamentalsResult?.fundamentals,
                    fundamentalsFetchedAt: fundamentalsResult?.fetchedAt
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich symbol {Ticker} with fundamental data", symbol.Ticker);
                // Return basic symbol data without enrichment
                return symbol.ToDto();
            }
        });

        var enrichedSymbols = await Task.WhenAll(enrichmentTasks);
        
        var enrichedCount = enrichedSymbols.Count(s => s.MarketCap.HasValue || s.CurrentPrice.HasValue);
        _logger.LogInformation("Successfully enriched {EnrichedCount} of {TotalCount} symbols with fundamental data", 
            enrichedCount, symbols.Count);

        return enrichedSymbols.ToList();
    }

    private List<SymbolDto> ApplyClientSideSorting(List<SymbolDto> symbols, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || string.IsNullOrWhiteSpace(sortDirection))
        {
            return symbols;
        }

        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "price" => isDescending 
                ? symbols.OrderByDescending(x => x.CurrentPrice ?? 0).ToList()
                : symbols.OrderBy(x => x.CurrentPrice ?? 0).ToList(),
            "change" => isDescending 
                ? symbols.OrderByDescending(x => x.ChangePercent ?? 0).ToList()
                : symbols.OrderBy(x => x.ChangePercent ?? 0).ToList(),
            "marketcap" => isDescending 
                ? symbols.OrderByDescending(x => x.MarketCap ?? 0).ToList()
                : symbols.OrderBy(x => x.MarketCap ?? 0).ToList(),
            "pe" => isDescending 
                ? symbols.OrderByDescending(x => x.TrailingPE ?? 0).ToList()
                : symbols.OrderBy(x => x.TrailingPE ?? 0).ToList(),
            "dividend" => isDescending 
                ? symbols.OrderByDescending(x => x.DividendYield ?? 0).ToList()
                : symbols.OrderBy(x => x.DividendYield ?? 0).ToList(),
            "sector" => isDescending 
                ? symbols.OrderByDescending(x => x.Sector ?? "").ToList()
                : symbols.OrderBy(x => x.Sector ?? "").ToList(),
            "industry" => isDescending 
                ? symbols.OrderByDescending(x => x.Industry ?? "").ToList()
                : symbols.OrderBy(x => x.Industry ?? "").ToList(),
            _ => symbols // No client-side sorting needed for database fields
        };
    }

    private bool IsEnrichedFieldSort(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return false;

        return sortBy.ToLowerInvariant() switch
        {
            "price" or "change" or "marketcap" or "pe" or "dividend" or "sector" or "industry" => true,
            _ => false
        };
    }

    private List<SymbolDto> ApplySearchFilter(List<SymbolDto> symbols, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return symbols;

        var query = searchQuery.ToLowerInvariant();
        return symbols.Where(s => 
            s.Ticker.ToLowerInvariant().Contains(query) ||
            s.Name.ToLowerInvariant().Contains(query) ||
            s.FullSymbol.ToLowerInvariant().Contains(query)
        ).ToList();
    }
}

