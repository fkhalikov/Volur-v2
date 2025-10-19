using Volur.Domain.Entities;

namespace Volur.Application.Interfaces;

/// <summary>
/// Repository for Symbol entities.
/// </summary>
public interface ISymbolRepository
{
    /// <summary>
    /// Gets symbols for an exchange with pagination and filtering.
    /// </summary>
    Task<(IReadOnlyList<Symbol> Symbols, int TotalCount, DateTime? FetchedAt)?> GetByExchangeAsync(
        string exchangeCode,
        int page,
        int pageSize,
        string? searchQuery = null,
        string? typeFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts symbols into cache for a specific exchange.
    /// </summary>
    Task UpsertManyAsync(string exchangeCode, IReadOnlyList<Symbol> symbols, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all symbols for an exchange (for refresh scenarios).
    /// </summary>
    Task DeleteByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a symbol by ticker (searches across all exchanges).
    /// </summary>
    Task<Symbol?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
}

