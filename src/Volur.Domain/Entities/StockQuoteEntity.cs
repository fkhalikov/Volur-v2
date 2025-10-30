namespace Volur.Domain.Entities;

/// <summary>
/// SQL Server entity for Stock Quote.
/// </summary>
public sealed class StockQuoteEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ticker symbol (unique).
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    public double? CurrentPrice { get; set; }
    public double? PreviousClose { get; set; }
    public double? Change { get; set; }
    public double? ChangePercent { get; set; }
    public double? Open { get; set; }
    public double? High { get; set; }
    public double? Low { get; set; }
    public double? Volume { get; set; }
    public double? AverageVolume { get; set; }
    public DateTime LastUpdated { get; set; }
}

