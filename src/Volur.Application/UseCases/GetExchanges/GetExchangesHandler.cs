using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volur.Application.Common;
using Volur.Application.Configuration;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Application.Mappers;
using Volur.Shared;

namespace Volur.Application.UseCases.GetExchanges;

/// <summary>
/// Handler for GetExchangesQuery.
/// Implements read-through cache pattern.
/// </summary>
public sealed class GetExchangesHandler
{
    private readonly IExchangeRepository _repository;
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<GetExchangesHandler> _logger;
    private readonly CacheTtlOptions _cacheTtl;

    public GetExchangesHandler(
        IExchangeRepository repository,
        IEodhdClient eodhdClient,
        ILogger<GetExchangesHandler> logger,
        IOptions<CacheTtlOptions> cacheTtl)
    {
        _repository = repository;
        _eodhdClient = eodhdClient;
        _logger = logger;
        _cacheTtl = cacheTtl.Value;
    }

    public async Task<Result<ExchangesResponse>> HandleAsync(GetExchangesQuery query, CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromHours(_cacheTtl.ExchangesHours);

        // Try cache first unless force refresh
        if (!query.ForceRefresh)
        {
            var cached = await _repository.GetAllAsync(cancellationToken);
            if (cached.HasValue)
            {
                var (exchanges, fetchedAt) = cached.Value;
                var expiresAt = fetchedAt!.Value.Add(ttl);
                var ttlRemaining = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;

                if (ttlRemaining > 0)
                {
                    _logger.LogInformation("Exchanges cache hit. TTL remaining: {TtlSeconds}s", ttlRemaining);
                    
                    return Result.Success(new ExchangesResponse(
                        Count: exchanges.Count,
                        Items: exchanges.Select(e => e.ToDto()).ToList(),
                        FetchedAt: fetchedAt.Value,
                        Cache: new CacheMetadata("mongo", ttlRemaining)
                    ));
                }

                _logger.LogInformation("Exchanges cache expired, fetching from provider");
            }
        }
        else
        {
            _logger.LogInformation("Force refresh requested, bypassing cache");
        }

        // Fetch from provider
        var providerResult = await _eodhdClient.GetExchangesAsync(cancellationToken);
        if (providerResult.IsFailure)
        {
            _logger.LogWarning("Failed to fetch exchanges from provider: {Error}", providerResult.Error);
            return Result.Failure<ExchangesResponse>(providerResult.Error!);
        }

        var providerExchanges = providerResult.Value;
        var domainExchanges = providerExchanges.Select(e => e.ToDomain()).ToList();
        var fetchedAtUtc = DateTime.UtcNow;

        // Update cache (fire-and-forget with error logging)
        try
        {
            await _repository.UpsertManyAsync(domainExchanges, fetchedAtUtc, ttl, cancellationToken);
            _logger.LogInformation("Cached {Count} exchanges with TTL {Hours}h", domainExchanges.Count, _cacheTtl.ExchangesHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache exchanges in MongoDB");
            // Continue - we have the data from provider
        }

        return Result.Success(new ExchangesResponse(
            Count: domainExchanges.Count,
            Items: domainExchanges.Select(e => e.ToDto()).ToList(),
            FetchedAt: fetchedAtUtc,
            Cache: new CacheMetadata("provider", (int)ttl.TotalSeconds)
        ));
    }
}

