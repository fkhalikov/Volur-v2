namespace Volur.Domain.Entities;

/// <summary>
/// Represents a tradable security/instrument on an exchange.
/// </summary>
public sealed record Symbol(
    string Ticker,
    string ExchangeCode,
    string Name,
    string? Type,
    string? Isin,
    string? Currency,
    bool IsActive
);

