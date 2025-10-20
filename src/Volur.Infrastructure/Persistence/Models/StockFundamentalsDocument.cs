using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Volur.Infrastructure.Persistence.Models;

/// <summary>
/// MongoDB document for caching stock fundamental data.
/// </summary>
public sealed class StockFundamentalsDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ticker")]
    public required string Ticker { get; set; }

    [BsonElement("companyName")]
    public string? CompanyName { get; set; }

    [BsonElement("sector")]
    public string? Sector { get; set; }

    [BsonElement("industry")]
    public string? Industry { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("website")]
    public string? Website { get; set; }

    [BsonElement("logoUrl")]
    public string? LogoUrl { get; set; }

    [BsonElement("currencyCode")]
    public string? CurrencyCode { get; set; }

    [BsonElement("currencySymbol")]
    public string? CurrencySymbol { get; set; }

    [BsonElement("currencyName")]
    public string? CurrencyName { get; set; }

    [BsonElement("marketCap")]
    public double? MarketCap { get; set; }

    [BsonElement("enterpriseValue")]
    public double? EnterpriseValue { get; set; }

    [BsonElement("trailingPE")]
    public double? TrailingPE { get; set; }

    [BsonElement("forwardPE")]
    public double? ForwardPE { get; set; }

    [BsonElement("peg")]
    public double? PEG { get; set; }

    [BsonElement("priceToSales")]
    public double? PriceToSales { get; set; }

    [BsonElement("priceToBook")]
    public double? PriceToBook { get; set; }

    [BsonElement("enterpriseToRevenue")]
    public double? EnterpriseToRevenue { get; set; }

    [BsonElement("enterpriseToEbitda")]
    public double? EnterpriseToEbitda { get; set; }

    [BsonElement("profitMargins")]
    public double? ProfitMargins { get; set; }

    [BsonElement("grossMargins")]
    public double? GrossMargins { get; set; }

    [BsonElement("operatingMargins")]
    public double? OperatingMargins { get; set; }

    [BsonElement("returnOnAssets")]
    public double? ReturnOnAssets { get; set; }

    [BsonElement("returnOnEquity")]
    public double? ReturnOnEquity { get; set; }

    [BsonElement("revenue")]
    public double? Revenue { get; set; }

    [BsonElement("revenuePerShare")]
    public double? RevenuePerShare { get; set; }

    [BsonElement("quarterlyRevenueGrowth")]
    public double? QuarterlyRevenueGrowth { get; set; }

    [BsonElement("quarterlyEarningsGrowth")]
    public double? QuarterlyEarningsGrowth { get; set; }

    [BsonElement("totalCash")]
    public double? TotalCash { get; set; }

    [BsonElement("totalCashPerShare")]
    public double? TotalCashPerShare { get; set; }

    [BsonElement("totalDebt")]
    public double? TotalDebt { get; set; }

    [BsonElement("debtToEquity")]
    public double? DebtToEquity { get; set; }

    [BsonElement("currentRatio")]
    public double? CurrentRatio { get; set; }

    [BsonElement("bookValue")]
    public double? BookValue { get; set; }

    [BsonElement("priceToBookValue")]
    public double? PriceToBookValue { get; set; }

    [BsonElement("dividendRate")]
    public double? DividendRate { get; set; }

    [BsonElement("dividendYield")]
    public double? DividendYield { get; set; }

    [BsonElement("payoutRatio")]
    public double? PayoutRatio { get; set; }

    [BsonElement("beta")]
    public double? Beta { get; set; }

    [BsonElement("fiftyTwoWeekLow")]
    public double? FiftyTwoWeekLow { get; set; }

    [BsonElement("fiftyTwoWeekHigh")]
    public double? FiftyTwoWeekHigh { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("fetchedAt")]
    public DateTime FetchedAt { get; set; }
}
