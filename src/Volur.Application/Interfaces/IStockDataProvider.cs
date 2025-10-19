using Volur.Application.DTOs;
using Volur.Shared;

namespace Volur.Application.Interfaces;

/// <summary>
/// Provider for stock market data including prices and fundamentals.
/// </summary>
public interface IStockDataProvider
{
    /// <summary>
    /// Gets real-time stock quote for a ticker.
    /// </summary>
    Task<Result<StockQuoteDto>> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets real-time stock quote for a ticker on a specific exchange.
    /// </summary>
    Task<Result<StockQuoteDto>> GetQuoteAsync(string ticker, string exchange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical price data for a ticker.
    /// </summary>
    Task<Result<IReadOnlyList<HistoricalPriceDto>>> GetHistoricalPricesAsync(
        string ticker, 
        DateTime startDate, 
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets fundamental data for a ticker.
    /// </summary>
    Task<Result<StockFundamentalsDto>> GetFundamentalsAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets fundamental data for a ticker on a specific exchange.
    /// </summary>
    Task<Result<StockFundamentalsDto>> GetFundamentalsAsync(string ticker, string exchange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple stock quotes in a single request.
    /// </summary>
    Task<Result<IReadOnlyList<StockQuoteDto>>> GetQuotesAsync(IReadOnlyList<string> tickers, CancellationToken cancellationToken = default);
}
