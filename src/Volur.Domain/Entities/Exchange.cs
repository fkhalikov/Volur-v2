namespace Volur.Domain.Entities;

/// <summary>
/// Represents a financial exchange/market.
/// </summary>
public sealed record Exchange(
    string Code,
    string Name,
    string? OperatingMic,
    string Country,
    string Currency,
    DateTime? TradingHoursUpdatedAt = null
);

