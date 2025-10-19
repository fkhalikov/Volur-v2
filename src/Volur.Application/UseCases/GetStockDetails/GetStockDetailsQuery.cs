namespace Volur.Application.UseCases.GetStockDetails;

/// <summary>
/// Query to get combined stock details (symbol + quote + fundamentals).
/// </summary>
public sealed record GetStockDetailsQuery(
    string Ticker,
    bool ForceRefresh = false
);
