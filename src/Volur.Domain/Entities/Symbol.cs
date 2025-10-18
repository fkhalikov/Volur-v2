namespace Volur.Domain.Entities;

/// <summary>
/// Represents a tradable security/instrument on an exchange.
/// </summary>
public sealed record Symbol(
    string Ticker,
    string ExchangeCode,
    string ParentExchange,
    string Name,
    string? Type,
    string? Isin,
    string? Currency,
    bool IsActive
)
{
    /// <summary>
    /// Gets the fully qualified symbol identifier in format {Ticker}.{ExchangeCode}
    /// </summary>
    public string FullSymbol => $"{Ticker}.{ExchangeCode}";
}

