namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for stock fundamental information.
/// </summary>
public sealed record StockFundamentalsDto(
    string Ticker,
    string? CompanyName,
    string? Sector,
    string? Industry,
    string? Description,
    string? Website,
    string? LogoUrl,
    double? MarketCap,
    double? EnterpriseValue,
    double? TrailingPE,
    double? ForwardPE,
    double? PEG,
    double? PriceToSales,
    double? PriceToBook,
    double? EnterpriseToRevenue,
    double? EnterpriseToEbitda,
    double? ProfitMargins,
    double? GrossMargins,
    double? OperatingMargins,
    double? ReturnOnAssets,
    double? ReturnOnEquity,
    double? Revenue,
    double? RevenuePerShare,
    double? QuarterlyRevenueGrowth,
    double? QuarterlyEarningsGrowth,
    double? TotalCash,
    double? TotalCashPerShare,
    double? TotalDebt,
    double? DebtToEquity,
    double? CurrentRatio,
    double? BookValue,
    double? PriceToBookValue,
    double? DividendRate,
    double? DividendYield,
    double? PayoutRatio,
    double? Beta,
    double? FiftyTwoWeekLow,
    double? FiftyTwoWeekHigh,
    DateTime LastUpdated
);

/// <summary>
/// Response for stock fundamental data.
/// </summary>
public sealed record StockFundamentalsResponse(
    StockFundamentalsDto Fundamentals,
    DateTime FetchedAt
);
