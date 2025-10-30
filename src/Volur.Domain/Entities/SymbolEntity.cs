namespace Volur.Domain.Entities;

/// <summary>
/// SQL Server entity for Symbol.
/// </summary>
public sealed class SymbolEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ticker symbol.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange code.
    /// </summary>
    public string ExchangeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent exchange.
    /// </summary>
    public string ParentExchange { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full symbol in format {Ticker}.{ExchangeCode}.
    /// </summary>
    public string FullSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the ISIN identifier.
    /// </summary>
    public string? Isin { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the symbol is active.
    /// </summary>
    public bool IsActive { get; set; }

    // Denormalized fields for efficient sorting (updated when fundamentals/quotes change)
    public double? TrailingPE { get; set; }
    public double? MarketCap { get; set; }
    public double? CurrentPrice { get; set; }
    public double? ChangePercent { get; set; }
    public double? DividendYield { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
}

