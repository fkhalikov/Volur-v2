namespace Volur.Application.UseCases.GetStockFundamentals;

/// <summary>
/// Query to get stock fundamental data.
/// </summary>
public sealed record GetStockFundamentalsQuery(string Ticker);
