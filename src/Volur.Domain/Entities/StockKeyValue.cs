namespace Volur.Domain.Entities;

/// <summary>
/// Represents a dated key-value pair for a stock symbol.
/// </summary>
public sealed class StockKeyValue
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string ExchangeCode { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
