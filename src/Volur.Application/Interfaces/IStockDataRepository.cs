using Volur.Application.DTOs;

namespace Volur.Application.Interfaces;

/// <summary>
/// Repository interface for stock data caching operations.
/// </summary>
public interface IStockDataRepository
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
}
