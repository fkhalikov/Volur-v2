namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for stock price information.
/// </summary>
public sealed record StockPriceDto(
    string Ticker,
    double? CurrentPrice,
    double? OpenPrice,
    double? HighPrice,
    double? LowPrice,
    double? PreviousClose,
    double? Change,
    double? ChangePercent,
    long? Volume,
    long? AverageVolume,
    double? MarketCap,
    DateTime LastUpdated
);

/// <summary>
/// Data transfer object for historical price data.
/// </summary>
public sealed record HistoricalPriceDto(
    DateTime Date,
    double Open,
    double High,
    double Low,
    double Close,
    long Volume
);

/// <summary>
/// Response containing historical price data.
/// </summary>
public sealed record HistoricalPriceResponse(
    string Ticker,
    IReadOnlyList<HistoricalPriceDto> Prices,
    DateTime FetchedAt
);
