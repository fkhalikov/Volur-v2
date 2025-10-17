using Volur.Application.DTOs.Provider;
using Volur.Shared;

namespace Volur.Application.Interfaces;

/// <summary>
/// Client for EODHD market data provider.
/// </summary>
public interface IEodhdClient
{
    /// <summary>
    /// Fetches all exchanges from EODHD.
    /// </summary>
    Task<Result<IReadOnlyList<EodhdExchangeDto>>> GetExchangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all symbols for a given exchange from EODHD.
    /// </summary>
    Task<Result<IReadOnlyList<EodhdSymbolDto>>> GetSymbolsAsync(string exchangeCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets real-time stock quote.
    /// </summary>
    Task<Result<EodhdStockQuoteDto>> GetStockQuoteAsync(string ticker, string exchange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical price data.
    /// </summary>
    Task<Result<IReadOnlyList<EodhdHistoricalPriceDto>>> GetHistoricalPricesAsync(
        string ticker, 
        string exchange, 
        DateTime from, 
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets fundamental data.
    /// </summary>
    Task<Result<EodhdFundamentalDto>> GetFundamentalsAsync(string ticker, string exchange, CancellationToken cancellationToken = default);
}

