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
        string? sortBy = null,
        string? sortDirection = null,
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

    /// <summary>
    /// Gets all symbols for an exchange without pagination.
    /// </summary>
    Task<(IReadOnlyList<Symbol> symbols, int totalCount, DateTime? fetchedAt)?> GetAllByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates denormalized sorting fields for a symbol when fundamentals/quote data changes.
    /// </summary>
    Task UpdateDenormalizedFieldsAsync(
        string ticker, 
        double? trailingPE = null,
        double? marketCap = null,
        double? currentPrice = null,
        double? changePercent = null,
        double? dividendYield = null,
        string? sector = null,
        string? industry = null,
        CancellationToken cancellationToken = default);
}

