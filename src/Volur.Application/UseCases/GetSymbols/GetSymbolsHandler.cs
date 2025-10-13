using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volur.Application.Common;
using Volur.Application.Configuration;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Application.Mappers;
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

        // Try cache first unless force refresh
        if (!query.ForceRefresh)
        {
            var cached = await _symbolRepository.GetByExchangeAsync(
                query.ExchangeCode,
                query.Page,
                query.PageSize,
                query.SearchQuery,
                query.TypeFilter,
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

                _logger.LogInformation("Symbols cache expired for {ExchangeCode}, fetching from provider", query.ExchangeCode);
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
        var domainSymbols = providerSymbols.Select(s => s.ToDomain()).ToList();
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
            query.TypeFilter,
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

