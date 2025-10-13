namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for real-time stock quote.
/// </summary>
public sealed record StockQuoteDto(
    string Ticker,
    double? CurrentPrice,
    double? PreviousClose,
    double? Change,
    double? ChangePercent,
    double? Open,
    double? High,
    double? Low,
    double? Volume,
    double? AverageVolume,
    DateTime LastUpdated
);

/// <summary>
/// Response for stock quote data.
/// </summary>
public sealed record StockQuoteResponse(
    StockQuoteDto Quote,
    DateTime FetchedAt
);
