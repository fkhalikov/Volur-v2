namespace Volur.Domain.Entities;

/// <summary>
/// SQL Server entity for Stock Fundamentals.
/// </summary>
public sealed class StockFundamentalsEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ticker symbol (unique).
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    public string? CompanyName { get; set; }
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencySymbol { get; set; }
    public string? CurrencyName { get; set; }

    public double? MarketCap { get; set; }
    public double? EnterpriseValue { get; set; }
    public double? TrailingPE { get; set; }
    public double? ForwardPE { get; set; }
    public double? PEG { get; set; }
    public double? PriceToSales { get; set; }
    public double? PriceToBook { get; set; }
    public double? EnterpriseToRevenue { get; set; }
    public double? EnterpriseToEbitda { get; set; }
    public double? ProfitMargins { get; set; }
    public double? GrossMargins { get; set; }
    public double? OperatingMargins { get; set; }
    public double? ReturnOnAssets { get; set; }
    public double? ReturnOnEquity { get; set; }
    public double? Revenue { get; set; }
    public double? RevenuePerShare { get; set; }
    public double? QuarterlyRevenueGrowth { get; set; }
    public double? QuarterlyEarningsGrowth { get; set; }
    public double? TotalCash { get; set; }
    public double? TotalCashPerShare { get; set; }
    public double? TotalDebt { get; set; }
    public double? DebtToEquity { get; set; }
    public double? CurrentRatio { get; set; }
    public double? BookValue { get; set; }
    public double? PriceToBookValue { get; set; }
    public double? DividendRate { get; set; }
    public double? DividendYield { get; set; }
    public double? PayoutRatio { get; set; }
    public double? Beta { get; set; }
    public double? FiftyTwoWeekLow { get; set; }
    public double? FiftyTwoWeekHigh { get; set; }
    public DateTime LastUpdated { get; set; }
}

