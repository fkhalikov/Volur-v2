namespace Volur.Application.UseCases.GetHistoricalPrices;

/// <summary>
/// Query to get historical price data.
/// </summary>
public sealed record GetHistoricalPricesQuery(
    string Ticker,
    DateTime StartDate,
    DateTime EndDate
);
