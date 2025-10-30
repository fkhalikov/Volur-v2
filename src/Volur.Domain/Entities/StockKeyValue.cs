namespace Volur.Domain.Entities;

/// <summary>
/// Represents a dated key-value pair for a stock symbol.
/// </summary>
public sealed class StockKeyValue : BaseEntity
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string ExchangeCode { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
