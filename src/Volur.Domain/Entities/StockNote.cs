namespace Volur.Domain.Entities;

/// <summary>
/// Represents a dated note or analysis for a stock symbol.
/// </summary>
public sealed class StockNote
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string ExchangeCode { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
