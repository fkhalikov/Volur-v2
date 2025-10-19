using System.Text.Json.Serialization;

namespace Volur.Application.DTOs.Provider;

/// <summary>
/// EODHD real-time stock quote response.
/// </summary>
public sealed record EodhdStockQuoteDto(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("timestamp")] long Timestamp,
    [property: JsonPropertyName("gmtoffset")] int GmtOffset,
    [property: JsonPropertyName("open")] double Open,
    [property: JsonPropertyName("high")] double High,
    [property: JsonPropertyName("low")] double Low,
    [property: JsonPropertyName("close")] double Close,
    [property: JsonPropertyName("volume")] long Volume,
    [property: JsonPropertyName("previousClose")] double PreviousClose,
    [property: JsonPropertyName("change")] double Change,
    [property: JsonPropertyName("change_p")] double ChangeP
);

/// <summary>
/// EODHD historical price data response.
/// </summary>
public sealed record EodhdHistoricalPriceDto(
    DateTime Date,
    double Open,
    double High,
    double Low,
    double Close,
    long Volume,
    double? AdjustedClose
);

/// <summary>
/// EODHD fundamental data response.
/// </summary>
public sealed record EodhdFundamentalDto(
    string Code,
    string Exchange,
    string? Name,
    string? Type,
    string? Country,
    string? Currency,
    string? Isin,
    string? Cusip,
    string? Sector,
    string? Industry,
    string? Description,
    string? Website,
    string? LogoUrl,
    double? MarketCapitalization,
    double? Employees,
    double? FullTimeEmployees,
    double? UpdatedAt,
    EodhdGeneralInfoDto? General,
    EodhdHighlightDto? Highlights,
    EodhdValuationDto? Valuation,
    EodhdTechnicalsDto? Technicals
);

public sealed record EodhdGeneralInfoDto(
    string? Code,
    string? Type,
    string? Name,
    string? Exchange,
    string? CurrencyCode,
    string? CurrencyName,
    string? CurrencySymbol,
    string? CountryName,
    string? CountryIso,
    string? Isin,
    string? Cusip,
    string? Cik,
    string? EmployerIdNumber,
    string? FiscalYearEnd,
    string? IpoDate,
    string? InternationalDomestic,
    string? Sector,
    string? Industry,
    string? GicSector,
    string? GicGroup,
    string? GicIndustry,
    string? GicSubIndustry,
    string? Description,
    string? Address,
    string? AddressData,
    string? Listings,
    string? Officers,
    string? Phone,
    string? WebUrl,
    string? LogoUrl,
    string? FullTimeEmployees,
    string? UpdatedAt
);

public sealed record EodhdHighlightDto(
    double? MarketCapitalization,
    double? MarketCapitalizationMln,
    double? Ebitda,
    double? PeRatio,
    double? PegRatio,
    double? WallStreetTargetPrice,
    double? BookValue,
    double? DividendShare,
    double? DividendYield,
    double? EarningsShare,
    double? EpsEstimateCurrentYear,
    double? EpsEstimateNextYear,
    double? EpsEstimateNextQuarter,
    double? EpsEstimateCurrentQuarter,
    double? MostRecentQuarter,
    double? ProfitMargin,
    double? OperatingMarginTtm,
    double? ReturnOnAssetsTtm,
    double? ReturnOnEquityTtm,
    double? RevenueTtm,
    double? RevenuePerShareTtm,
    double? QuarterlyRevenueGrowthYoy,
    double? GrossProfitTtm,
    double? DilutedEpsTtm,
    double? QuarterlyEarningsGrowthYoy
);

public sealed record EodhdValuationDto(
    double? TrailingPe,
    double? ForwardPe,
    double? PriceSalesTtm,
    double? PriceBookMrq,
    double? EnterpriseValueRevenue,
    double? EnterpriseValueEbitda
);

public sealed record EodhdTechnicalsDto(
    double? Beta,
    double? FiftyTwoWeekHigh,
    double? FiftyTwoWeekLow,
    double? FiftyDayMa,
    double? TwoHundredDayMa
);
