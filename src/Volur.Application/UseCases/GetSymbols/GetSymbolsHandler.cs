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
    private readonly IStockAnalysisRepository _stockAnalysisRepository;
    private readonly IStockDataRepositoryFactory _stockDataRepositoryFactory;
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<GetSymbolsHandler> _logger;
    private readonly CacheTtlOptions _cacheTtl;

    public GetSymbolsHandler(
        ISymbolRepository symbolRepository,
        IExchangeRepository exchangeRepository,
        IStockDataRepository stockDataRepository,
        IStockDataProvider stockDataProvider,
        IStockAnalysisRepository stockAnalysisRepository,
        IStockDataRepositoryFactory stockDataRepositoryFactory,
        IEodhdClient eodhdClient,
        ILogger<GetSymbolsHandler> logger,
        IOptions<CacheTtlOptions> cacheTtl)
    {
        _symbolRepository = symbolRepository;
        _exchangeRepository = exchangeRepository;
        _stockDataRepository = stockDataRepository;
        _stockDataProvider = stockDataProvider;
        _stockAnalysisRepository = stockAnalysisRepository;
        _stockDataRepositoryFactory = stockDataRepositoryFactory;
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

        // Use type filter only if explicitly specified (don't default to "Common Stock" as it may not match all exchanges)
        var typeFilter = query.TypeFilter; // Allow null to show all types
        
        // Default to sorting by P/E in descending order when no sort is specified
        var sortBy = !string.IsNullOrWhiteSpace(query.SortBy) ? query.SortBy : "pe";
        var sortDirection = !string.IsNullOrWhiteSpace(query.SortDirection) ? query.SortDirection : "desc";
        
        _logger.LogInformation("Processing symbols for {ExchangeCode} with sortBy={SortBy}, sortDirection={SortDirection}", 
            query.ExchangeCode, sortBy, sortDirection);
        
        // Try cache first unless force refresh
        if (!query.ForceRefresh)
        {
            var cached = await _symbolRepository.GetByExchangeAsync(
                query.ExchangeCode,
                query.Page,
                query.PageSize,
                query.SearchQuery,
                typeFilter,
                sortBy,
                sortDirection,
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
                    if (IsEnrichedFieldSort(sortBy))
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
                            var (symbolsSubset, subsetTotalCount, _) = allSymbolsResult.Value;
                            var enrichedSymbols = await EnrichSymbolsWithFundamentalDataAsync(symbolsSubset, cancellationToken);
                            var sortedSymbols = ApplyClientSideSorting(enrichedSymbols, sortBy, sortDirection);
                            
                            _logger.LogInformation("Sorted {Count} symbols by {SortBy} {SortDirection}", sortedSymbols.Count, sortBy, sortDirection);
                            
                            var hasNext = (query.Page * query.PageSize) < totalCount;
                            var skip = (query.Page - 1) * query.PageSize;
                            var pagedSymbols = sortedSymbols.Skip(skip).Take(query.PageSize).ToList();

                            return Result.Success(new SymbolsResponse(
                                Exchange: exchange.ToDto(),
                                Pagination: new PaginationMetadata(query.Page, query.PageSize, totalCount, hasNext),
                                Items: pagedSymbols,
                                FetchedAt: fetchedAt.Value,
                                Cache: new CacheMetadata("sql", ttlRemaining)
                            ));
                        }
                    }

                    return await BuildSuccessResponseAsync(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "sql", ttlRemaining, sortBy, sortDirection, cancellationToken);
                }

                // If cache expired but we have a search query, return expired cache data instead of refreshing
                if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                {
                    _logger.LogInformation("Symbols cache expired for {ExchangeCode}, but returning expired cache for search query", query.ExchangeCode);
                    return await BuildSuccessResponseAsync(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "sql", 0, sortBy, sortDirection, cancellationToken);
                }

                _logger.LogInformation("Symbols cache expired for {ExchangeCode}, fetching from provider", query.ExchangeCode);
            }
            else if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                // No cached data but we have a search query - return empty results instead of refreshing
                _logger.LogInformation("No cached symbols for {ExchangeCode} and search query provided, returning empty results", query.ExchangeCode);
                return await BuildSuccessResponseAsync(exchange, Array.Empty<Symbol>(), query.Page, query.PageSize, 0, DateTime.UtcNow, "none", 0, sortBy, sortDirection, cancellationToken);
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
        _logger.LogInformation("Fetched {Count} symbols from provider for {ExchangeCode}", 
            providerSymbols?.Count ?? 0, query.ExchangeCode);
        
        // Apply type filter only if explicitly specified
        var filteredProviderSymbols = providerSymbols;
        if (!string.IsNullOrWhiteSpace(query.TypeFilter))
        {
            filteredProviderSymbols = providerSymbols
                .Where(s => s.Type?.Equals(query.TypeFilter, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
            
            _logger.LogInformation("Filtered {OriginalCount} symbols to {FilteredCount} of type {Type} for {ExchangeCode}", 
                providerSymbols.Count, filteredProviderSymbols.Count, query.TypeFilter, query.ExchangeCode);
        }
        else
        {
            _logger.LogInformation("No type filter applied for {ExchangeCode}, showing all {Count} symbols", 
                query.ExchangeCode, filteredProviderSymbols?.Count ?? 0);
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
            _logger.LogError(ex, "Failed to cache symbols for {ExchangeCode} in SQL Server", query.ExchangeCode);
            // Continue - we have the data from provider
        }

        // For enriched field sorting, we need to get a larger subset of data, sort it, then paginate
        if (IsEnrichedFieldSort(sortBy))
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
            var sortedSymbols = ApplyClientSideSorting(enrichedSymbols, sortBy, sortDirection);
            
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
                sortBy,
                sortDirection,
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
            return await BuildSuccessResponseAsync(exchange, filteredSymbols, query.Page, query.PageSize, finalTotalCount, fetchedAtUtc, "provider", (int)ttl.TotalSeconds, sortBy, sortDirection, cancellationToken);
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

        // No client-side sorting needed - all sorting is done server-side at SQL Server
        // P/E sorting uses SQL Server query with nulls always last
        // Other fields are sorted directly in SQL Server query

        return Result.Success(new SymbolsResponse(
            Exchange: exchange.ToDto(),
            Pagination: new PaginationMetadata(page, pageSize, totalCount, hasNext),
            Items: symbolDtos,
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
                            // Use a separate repository instance to avoid DbContext concurrency issues
                            try
                            {
                                using var stockDataRepo = _stockDataRepositoryFactory.Create();
                                await stockDataRepo.UpsertQuoteAsync(freshQuote, cancellationToken);
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

                // Check for NoBuy status in stock analysis
                var hasNoBuyStatus = false;
                try
                {
                    var keyValues = await _stockAnalysisRepository.GetKeyValuesAsync(symbol.Ticker, symbol.ExchangeCode, cancellationToken);
                    hasNoBuyStatus = keyValues.Any(kv => kv.Key == "Status" && kv.Value == "NoBuy");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check NoBuy status for {Ticker}", symbol.Ticker);
                }

                return symbol.ToDto(
                    quote: quoteResult?.quote,
                    fundamentals: fundamentalsResult?.fundamentals,
                    fundamentalsFetchedAt: fundamentalsResult?.fetchedAt,
                    hasNoBuyStatus: hasNoBuyStatus
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
                ? symbols.OrderByDescending(x => x.TrailingPE ?? -999999).ThenBy(x => x.Ticker).ToList()
                : symbols.OrderBy(x => x.TrailingPE ?? double.MaxValue).ThenBy(x => x.Ticker).ToList(),
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
        // These fields are now denormalized in SymbolDocument and can be sorted at database level
        // No client-side sorting needed for these fields anymore
        return false;
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

