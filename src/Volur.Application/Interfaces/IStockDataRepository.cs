using Volur.Application.DTOs;

namespace Volur.Application.Interfaces;

/// <summary>
/// Repository interface for stock data caching operations.
/// </summary>
public interface IStockDataRepository : IDisposable
{
    /// <summary>
    /// Gets cached stock quote data for a ticker.
    /// </summary>
    Task<(StockQuoteDto quote, DateTime fetchedAt)?> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches stock quote data for a ticker.
    /// </summary>
    Task UpsertQuoteAsync(StockQuoteDto quote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached stock fundamentals data for a ticker.
    /// </summary>
    Task<(StockFundamentalsDto fundamentals, DateTime fetchedAt)?> GetFundamentalsAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches stock fundamentals data for a ticker.
    /// </summary>
    Task UpsertFundamentalsAsync(StockFundamentalsDto fundamentals, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a stock is marked as having no fundamental data available.
    /// </summary>
    Task<bool> IsNoDataAvailableAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a stock as having no fundamental data available.
    /// </summary>
    Task MarkAsNoDataAvailableAsync(string ticker, string exchangeCode, string? errorMessage = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a stock from the no-data-available list (when data becomes available).
    /// </summary>
    Task RemoveNoDataAvailableAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stocks that are marked as having no data available for a given exchange.
    /// </summary>
    Task<IReadOnlyList<(string ticker, string exchangeCode)>> GetNoDataAvailableForExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default);
}
