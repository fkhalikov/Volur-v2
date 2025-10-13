namespace Volur.Application.UseCases.GetStockQuote;

/// <summary>
/// Query to get real-time stock quote.
/// </summary>
public sealed record GetStockQuoteQuery(string Ticker);
