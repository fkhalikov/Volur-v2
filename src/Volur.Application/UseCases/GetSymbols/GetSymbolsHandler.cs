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
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<GetSymbolsHandler> _logger;
    private readonly CacheTtlOptions _cacheTtl;

    public GetSymbolsHandler(
        ISymbolRepository symbolRepository,
        IExchangeRepository exchangeRepository,
        IEodhdClient eodhdClient,
        ILogger<GetSymbolsHandler> logger,
        IOptions<CacheTtlOptions> cacheTtl)
    {
        _symbolRepository = symbolRepository;
        _exchangeRepository = exchangeRepository;
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

                    return BuildSuccessResponse(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "mongo", ttlRemaining);
                }

                // If cache expired but we have a search query, return expired cache data instead of refreshing
                if (!string.IsNullOrWhiteSpace(query.SearchQuery))
                {
                    _logger.LogInformation("Symbols cache expired for {ExchangeCode}, but returning expired cache for search query", query.ExchangeCode);
                    return BuildSuccessResponse(exchange, symbols, query.Page, query.PageSize, totalCount, fetchedAt.Value, "mongo", 0);
                }

                _logger.LogInformation("Symbols cache expired for {ExchangeCode}, fetching from provider", query.ExchangeCode);
            }
            else if (!string.IsNullOrWhiteSpace(query.SearchQuery))
            {
                // No cached data but we have a search query - return empty results instead of refreshing
                _logger.LogInformation("No cached symbols for {ExchangeCode} and search query provided, returning empty results", query.ExchangeCode);
                return BuildSuccessResponse(exchange, Array.Empty<Symbol>(), query.Page, query.PageSize, 0, DateTime.UtcNow, "none", 0);
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

        // Re-query with filters and pagination
        var finalResult = await _symbolRepository.GetByExchangeAsync(
            query.ExchangeCode,
            query.Page,
            query.PageSize,
            query.SearchQuery,
            typeFilter,
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
        return BuildSuccessResponse(exchange, filteredSymbols, query.Page, query.PageSize, finalTotalCount, fetchedAtUtc, "provider", (int)ttl.TotalSeconds);
    }

    private static Result<SymbolsResponse> BuildSuccessResponse(
        Domain.Entities.Exchange exchange,
        IReadOnlyList<Domain.Entities.Symbol> symbols,
        int page,
        int pageSize,
        int totalCount,
        DateTime fetchedAt,
        string cacheSource,
        int ttlSeconds)
    {
        var hasNext = (page * pageSize) < totalCount;

        return Result.Success(new SymbolsResponse(
            Exchange: exchange.ToDto(),
            Pagination: new PaginationMetadata(page, pageSize, totalCount, hasNext),
            Items: symbols.Select(s => s.ToDto()).ToList(),
            FetchedAt: fetchedAt,
            Cache: new CacheMetadata(cacheSource, ttlSeconds)
        ));
    }
}

