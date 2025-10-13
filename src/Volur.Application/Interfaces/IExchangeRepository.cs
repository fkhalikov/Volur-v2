using Volur.Domain.Entities;

namespace Volur.Application.Interfaces;

/// <summary>
/// Repository for Exchange entities.
/// </summary>
public interface IExchangeRepository
{
    /// <summary>
    /// Gets all cached exchanges.
    /// </summary>
    Task<(IReadOnlyList<Exchange> Exchanges, DateTime? FetchedAt)?> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single exchange by code.
    /// </summary>
    Task<Exchange?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts exchanges into cache.
    /// </summary>
    Task UpsertManyAsync(IReadOnlyList<Exchange> exchanges, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default);
}

