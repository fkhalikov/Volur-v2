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
}

