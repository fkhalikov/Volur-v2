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
    [property: JsonPropertyName("General")] EodhdGeneralInfoDto General,
    [property: JsonPropertyName("Highlights")] EodhdHighlightDto Highlights,
    [property: JsonPropertyName("Valuation")] EodhdValuationDto Valuation,
    [property: JsonPropertyName("Technicals")] EodhdTechnicalsDto Technicals,
    [property: JsonPropertyName("SplitsDividends")] EodhdSplitsDividendsDto SplitsDividends,
    [property: JsonPropertyName("Earnings")] EodhdEarningsDto Earnings,
    [property: JsonPropertyName("Financials")] EodhdFinancialsDto Financials
);

public sealed record EodhdGeneralInfoDto(
    [property: JsonPropertyName("Code")] string Code,
    [property: JsonPropertyName("Type")] string Type,
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Exchange")] string Exchange,
    [property: JsonPropertyName("CurrencyCode")] string CurrencyCode,
    [property: JsonPropertyName("CurrencyName")] string CurrencyName,
    [property: JsonPropertyName("CurrencySymbol")] string CurrencySymbol,
    [property: JsonPropertyName("CountryName")] string CountryName,
    [property: JsonPropertyName("CountryISO")] string CountryIso,
    [property: JsonPropertyName("ISIN")] string? Isin,
    [property: JsonPropertyName("Sector")] string Sector,
    [property: JsonPropertyName("Industry")] string Industry,
    [property: JsonPropertyName("GicSector")] string? GicSector,
    [property: JsonPropertyName("GicGroup")] string? GicGroup,
    [property: JsonPropertyName("GicIndustry")] string? GicIndustry,
    [property: JsonPropertyName("GicSubIndustry")] string? GicSubIndustry,
    [property: JsonPropertyName("Description")] string Description,
    [property: JsonPropertyName("Address")] string? Address,
    [property: JsonPropertyName("WebURL")] string? WebUrl,
    [property: JsonPropertyName("LogoURL")] string? LogoUrl,
    [property: JsonPropertyName("FullTimeEmployees")] int? FullTimeEmployees,
    [property: JsonPropertyName("UpdatedAt")] string UpdatedAt
);

public sealed record EodhdHighlightDto(
    [property: JsonPropertyName("MarketCapitalization")] long? MarketCapitalization,
    [property: JsonPropertyName("MarketCapitalizationMln")] double? MarketCapitalizationMln,
    [property: JsonPropertyName("EBITDA")] long? Ebitda,
    [property: JsonPropertyName("PERatio")] double? PeRatio,
    [property: JsonPropertyName("PEGRatio")] double? PegRatio,
    [property: JsonPropertyName("WallStreetTargetPrice")] double? WallStreetTargetPrice,
    [property: JsonPropertyName("BookValue")] double? BookValue,
    [property: JsonPropertyName("DividendShare")] double? DividendShare,
    [property: JsonPropertyName("DividendYield")] double? DividendYield,
    [property: JsonPropertyName("EarningsShare")] double? EarningsShare,
    [property: JsonPropertyName("EPSEstimateCurrentYear")] double? EpsEstimateCurrentYear,
    [property: JsonPropertyName("EPSEstimateNextYear")] double? EpsEstimateNextYear,
    [property: JsonPropertyName("EPSEstimateNextQuarter")] double? EpsEstimateNextQuarter,
    [property: JsonPropertyName("EPSEstimateCurrentQuarter")] double? EpsEstimateCurrentQuarter,
    [property: JsonPropertyName("MostRecentQuarter")] string? MostRecentQuarter,
    [property: JsonPropertyName("ProfitMargin")] double? ProfitMargin,
    [property: JsonPropertyName("OperatingMarginTTM")] double? OperatingMarginTtm,
    [property: JsonPropertyName("ReturnOnAssetsTTM")] double? ReturnOnAssetsTtm,
    [property: JsonPropertyName("ReturnOnEquityTTM")] double? ReturnOnEquityTtm,
    [property: JsonPropertyName("RevenueTTM")] long? RevenueTtm,
    [property: JsonPropertyName("RevenuePerShareTTM")] double? RevenuePerShareTtm,
    [property: JsonPropertyName("QuarterlyRevenueGrowthYOY")] double? QuarterlyRevenueGrowthYoy,
    [property: JsonPropertyName("GrossProfitTTM")] long? GrossProfitTtm,
    [property: JsonPropertyName("DilutedEpsTTM")] double? DilutedEpsTtm,
    [property: JsonPropertyName("QuarterlyEarningsGrowthYOY")] double? QuarterlyEarningsGrowthYoy
);

public sealed record EodhdValuationDto(
    [property: JsonPropertyName("TrailingPE")] double? TrailingPe,
    [property: JsonPropertyName("ForwardPE")] double? ForwardPe,
    [property: JsonPropertyName("PriceSalesTTM")] double? PriceSalesTtm,
    [property: JsonPropertyName("PriceBookMRQ")] double? PriceBookMrq,
    [property: JsonPropertyName("EnterpriseValue")] long? EnterpriseValue,
    [property: JsonPropertyName("EnterpriseValueRevenue")] double? EnterpriseValueRevenue,
    [property: JsonPropertyName("EnterpriseValueEbitda")] double? EnterpriseValueEbitda
);

public sealed record EodhdTechnicalsDto(
    [property: JsonPropertyName("Beta")] double? Beta,
    [property: JsonPropertyName("52WeekHigh")] double? FiftyTwoWeekHigh,
    [property: JsonPropertyName("52WeekLow")] double? FiftyTwoWeekLow,
    [property: JsonPropertyName("50DayMA")] double? FiftyDayMa,
    [property: JsonPropertyName("200DayMA")] double? TwoHundredDayMa
);

public sealed record EodhdSplitsDividendsDto(
    [property: JsonPropertyName("ForwardAnnualDividendRate")] double? ForwardAnnualDividendRate,
    [property: JsonPropertyName("ForwardAnnualDividendYield")] double? ForwardAnnualDividendYield,
    [property: JsonPropertyName("PayoutRatio")] double? PayoutRatio,
    [property: JsonPropertyName("DividendDate")] string? DividendDate,
    [property: JsonPropertyName("ExDividendDate")] string? ExDividendDate,
    [property: JsonPropertyName("LastSplitFactor")] string? LastSplitFactor,
    [property: JsonPropertyName("LastSplitDate")] string? LastSplitDate
);

public sealed record EodhdEarningsDto(
    [property: JsonPropertyName("History")] Dictionary<string, EodhdEarningHistoryDto> History,
    [property: JsonPropertyName("Trend")] Dictionary<string, EodhdEarningTrendDto> Trend,
    [property: JsonPropertyName("Annual")] Dictionary<string, EodhdEarningAnnualDto> Annual
);

public sealed record EodhdEarningHistoryDto(
    [property: JsonPropertyName("reportDate")] string reportDate,
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("epsActual")] double? epsActual,
    [property: JsonPropertyName("epsEstimate")] double? epsEstimate,
    [property: JsonPropertyName("epsDifference")] double? epsDifference,
    [property: JsonPropertyName("surprisePercent")] double? surprisePercent
);

public sealed record EodhdEarningTrendDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("period")] string? period,
    [property: JsonPropertyName("growth")] string? growth,
    [property: JsonPropertyName("earningsEstimateAvg")] string? earningsEstimateAvg,
    [property: JsonPropertyName("earningsEstimateLow")] string? earningsEstimateLow,
    [property: JsonPropertyName("earningsEstimateHigh")] string? earningsEstimateHigh,
    [property: JsonPropertyName("earningsEstimateYearAgoEps")] string? earningsEstimateYearAgoEps,
    [property: JsonPropertyName("earningsEstimateNumberOfAnalysts")] string? earningsEstimateNumberOfAnalysts,
    [property: JsonPropertyName("earningsEstimateGrowth")] string? earningsEstimateGrowth,
    [property: JsonPropertyName("revenueEstimateAvg")] string? revenueEstimateAvg,
    [property: JsonPropertyName("revenueEstimateLow")] string? revenueEstimateLow,
    [property: JsonPropertyName("revenueEstimateHigh")] string? revenueEstimateHigh,
    [property: JsonPropertyName("revenueEstimateYearAgoEps")] string? revenueEstimateYearAgoEps,
    [property: JsonPropertyName("revenueEstimateNumberOfAnalysts")] string? revenueEstimateNumberOfAnalysts,
    [property: JsonPropertyName("revenueEstimateGrowth")] string? revenueEstimateGrowth,
    [property: JsonPropertyName("epsTrendCurrent")] string? epsTrendCurrent,
    [property: JsonPropertyName("epsTrend7daysAgo")] string? epsTrend7daysAgo,
    [property: JsonPropertyName("epsTrend30daysAgo")] string? epsTrend30daysAgo,
    [property: JsonPropertyName("epsTrend60daysAgo")] string? epsTrend60daysAgo,
    [property: JsonPropertyName("epsTrend90daysAgo")] string? epsTrend90daysAgo,
    [property: JsonPropertyName("epsRevisionsUpLast7days")] string? epsRevisionsUpLast7days,
    [property: JsonPropertyName("epsRevisionsUpLast30days")] string? epsRevisionsUpLast30days,
    [property: JsonPropertyName("epsRevisionsDownLast7days")] string? epsRevisionsDownLast7days,
    [property: JsonPropertyName("epsRevisionsDownLast30days")] string? epsRevisionsDownLast30days
);

public sealed record EodhdEarningAnnualDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("epsActual")] double? epsActual
);

public sealed record EodhdFinancialsDto(
    [property: JsonPropertyName("Balance_Sheet")] EodhdBalanceSheetDto BalanceSheet,
    [property: JsonPropertyName("Cash_Flow")] EodhdCashFlowDto CashFlow,
    [property: JsonPropertyName("Income_Statement")] EodhdIncomeStatementDto IncomeStatement
);

public sealed record EodhdBalanceSheetDto(
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("quarterly")] Dictionary<string, EodhdBalanceSheetQuarterlyDto> quarterly,
    [property: JsonPropertyName("yearly")] Dictionary<string, EodhdBalanceSheetYearlyDto> yearly
);

public sealed record EodhdBalanceSheetQuarterlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("totalAssets")] string? totalAssets,
    [property: JsonPropertyName("intangibleAssets")] string? intangibleAssets,
    [property: JsonPropertyName("earningAssets")] string? earningAssets,
    [property: JsonPropertyName("otherCurrentAssets")] string? otherCurrentAssets,
    [property: JsonPropertyName("totalLiab")] string? totalLiab,
    [property: JsonPropertyName("totalStockholderEquity")] string? totalStockholderEquity,
    [property: JsonPropertyName("deferredLongTermLiab")] string? deferredLongTermLiab,
    [property: JsonPropertyName("otherCurrentLiab")] string? otherCurrentLiab,
    [property: JsonPropertyName("commonStock")] string? commonStock,
    [property: JsonPropertyName("capitalStock")] string? capitalStock,
    [property: JsonPropertyName("retainedEarnings")] string? retainedEarnings,
    [property: JsonPropertyName("otherLiab")] string? otherLiab,
    [property: JsonPropertyName("goodWill")] string? goodWill,
    [property: JsonPropertyName("otherAssets")] string? otherAssets,
    [property: JsonPropertyName("cash")] string? cash,
    [property: JsonPropertyName("cashAndEquivalents")] string? cashAndEquivalents,
    [property: JsonPropertyName("totalCurrentLiabilities")] string? totalCurrentLiabilities,
    [property: JsonPropertyName("currentDeferredRevenue")] string? currentDeferredRevenue,
    [property: JsonPropertyName("netDebt")] string? netDebt,
    [property: JsonPropertyName("shortTermDebt")] string? shortTermDebt,
    [property: JsonPropertyName("shortLongTermDebt")] string? shortLongTermDebt,
    [property: JsonPropertyName("shortLongTermDebtTotal")] string? shortLongTermDebtTotal,
    [property: JsonPropertyName("otherStockholderEquity")] string? otherStockholderEquity,
    [property: JsonPropertyName("propertyPlantEquipment")] string? propertyPlantEquipment,
    [property: JsonPropertyName("totalCurrentAssets")] string? totalCurrentAssets,
    [property: JsonPropertyName("longTermInvestments")] string? longTermInvestments,
    [property: JsonPropertyName("netTangibleAssets")] string? netTangibleAssets,
    [property: JsonPropertyName("shortTermInvestments")] string? shortTermInvestments,
    [property: JsonPropertyName("netReceivables")] string? netReceivables,
    [property: JsonPropertyName("longTermDebt")] string? longTermDebt,
    [property: JsonPropertyName("inventory")] string? inventory,
    [property: JsonPropertyName("accountsPayable")] string? accountsPayable,
    [property: JsonPropertyName("totalPermanentEquity")] string? totalPermanentEquity,
    [property: JsonPropertyName("noncontrollingInterestInConsolidatedEntity")] string? noncontrollingInterestInConsolidatedEntity,
    [property: JsonPropertyName("temporaryEquityRedeemableNoncontrollingInterests")] string? temporaryEquityRedeemableNoncontrollingInterests,
    [property: JsonPropertyName("accumulatedOtherComprehensiveIncome")] string? accumulatedOtherComprehensiveIncome,
    [property: JsonPropertyName("additionalPaidInCapital")] string? additionalPaidInCapital,
    [property: JsonPropertyName("commonStockTotalEquity")] string? commonStockTotalEquity,
    [property: JsonPropertyName("preferredStockTotalEquity")] string? preferredStockTotalEquity,
    [property: JsonPropertyName("retainedEarningsTotalEquity")] string? retainedEarningsTotalEquity,
    [property: JsonPropertyName("treasuryStock")] string? treasuryStock,
    [property: JsonPropertyName("accumulatedAmortization")] string? accumulatedAmortization,
    [property: JsonPropertyName("nonCurrrentAssetsOther")] string? nonCurrrentAssetsOther,
    [property: JsonPropertyName("deferredLongTermAssetCharges")] string? deferredLongTermAssetCharges,
    [property: JsonPropertyName("nonCurrentAssetsTotal")] string? nonCurrentAssetsTotal,
    [property: JsonPropertyName("capitalLeaseObligations")] string? capitalLeaseObligations,
    [property: JsonPropertyName("longTermDebtTotal")] string? longTermDebtTotal,
    [property: JsonPropertyName("nonCurrentLiabilitiesOther")] string? nonCurrentLiabilitiesOther,
    [property: JsonPropertyName("nonCurrentLiabilitiesTotal")] string? nonCurrentLiabilitiesTotal,
    [property: JsonPropertyName("negativeGoodwill")] string? negativeGoodwill,
    [property: JsonPropertyName("warrants")] string? warrants,
    [property: JsonPropertyName("preferredStockRedeemable")] string? preferredStockRedeemable,
    [property: JsonPropertyName("capitalSurpluse")] string? capitalSurpluse,
    [property: JsonPropertyName("liabilitiesAndStockholdersEquity")] string? liabilitiesAndStockholdersEquity,
    [property: JsonPropertyName("cashAndShortTermInvestments")] string? cashAndShortTermInvestments,
    [property: JsonPropertyName("propertyPlantAndEquipmentGross")] string? propertyPlantAndEquipmentGross,
    [property: JsonPropertyName("propertyPlantAndEquipmentNet")] string? propertyPlantAndEquipmentNet,
    [property: JsonPropertyName("accumulatedDepreciation")] string? accumulatedDepreciation,
    [property: JsonPropertyName("netWorkingCapital")] string? netWorkingCapital,
    [property: JsonPropertyName("netInvestedCapital")] string? netInvestedCapital,
    [property: JsonPropertyName("commonStockSharesOutstanding")] string? commonStockSharesOutstanding
);

public sealed record EodhdBalanceSheetYearlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("totalAssets")] string? totalAssets,
    [property: JsonPropertyName("intangibleAssets")] string? intangibleAssets,
    [property: JsonPropertyName("earningAssets")] string? earningAssets,
    [property: JsonPropertyName("otherCurrentAssets")] string? otherCurrentAssets,
    [property: JsonPropertyName("totalLiab")] string? totalLiab,
    [property: JsonPropertyName("totalStockholderEquity")] string? totalStockholderEquity,
    [property: JsonPropertyName("deferredLongTermLiab")] string? deferredLongTermLiab,
    [property: JsonPropertyName("otherCurrentLiab")] string? otherCurrentLiab,
    [property: JsonPropertyName("commonStock")] string? commonStock,
    [property: JsonPropertyName("capitalStock")] string? capitalStock,
    [property: JsonPropertyName("retainedEarnings")] string? retainedEarnings,
    [property: JsonPropertyName("otherLiab")] string? otherLiab,
    [property: JsonPropertyName("goodWill")] string? goodWill,
    [property: JsonPropertyName("otherAssets")] string? otherAssets,
    [property: JsonPropertyName("cash")] string? cash,
    [property: JsonPropertyName("cashAndEquivalents")] string? cashAndEquivalents,
    [property: JsonPropertyName("totalCurrentLiabilities")] string? totalCurrentLiabilities,
    [property: JsonPropertyName("currentDeferredRevenue")] string? currentDeferredRevenue,
    [property: JsonPropertyName("netDebt")] string? netDebt,
    [property: JsonPropertyName("shortTermDebt")] string? shortTermDebt,
    [property: JsonPropertyName("shortLongTermDebt")] string? shortLongTermDebt,
    [property: JsonPropertyName("shortLongTermDebtTotal")] string? shortLongTermDebtTotal,
    [property: JsonPropertyName("otherStockholderEquity")] string? otherStockholderEquity,
    [property: JsonPropertyName("propertyPlantEquipment")] string? propertyPlantEquipment,
    [property: JsonPropertyName("totalCurrentAssets")] string? totalCurrentAssets,
    [property: JsonPropertyName("longTermInvestments")] string? longTermInvestments,
    [property: JsonPropertyName("netTangibleAssets")] string? netTangibleAssets,
    [property: JsonPropertyName("shortTermInvestments")] string? shortTermInvestments,
    [property: JsonPropertyName("netReceivables")] string? netReceivables,
    [property: JsonPropertyName("longTermDebt")] string? longTermDebt,
    [property: JsonPropertyName("inventory")] string? inventory,
    [property: JsonPropertyName("accountsPayable")] string? accountsPayable,
    [property: JsonPropertyName("totalPermanentEquity")] string? totalPermanentEquity,
    [property: JsonPropertyName("noncontrollingInterestInConsolidatedEntity")] string? noncontrollingInterestInConsolidatedEntity,
    [property: JsonPropertyName("temporaryEquityRedeemableNoncontrollingInterests")] string? temporaryEquityRedeemableNoncontrollingInterests,
    [property: JsonPropertyName("accumulatedOtherComprehensiveIncome")] string? accumulatedOtherComprehensiveIncome,
    [property: JsonPropertyName("additionalPaidInCapital")] string? additionalPaidInCapital,
    [property: JsonPropertyName("commonStockTotalEquity")] string? commonStockTotalEquity,
    [property: JsonPropertyName("preferredStockTotalEquity")] string? preferredStockTotalEquity,
    [property: JsonPropertyName("retainedEarningsTotalEquity")] string? retainedEarningsTotalEquity,
    [property: JsonPropertyName("treasuryStock")] string? treasuryStock,
    [property: JsonPropertyName("accumulatedAmortization")] string? accumulatedAmortization,
    [property: JsonPropertyName("nonCurrrentAssetsOther")] string? nonCurrrentAssetsOther,
    [property: JsonPropertyName("deferredLongTermAssetCharges")] string? deferredLongTermAssetCharges,
    [property: JsonPropertyName("nonCurrentAssetsTotal")] string? nonCurrentAssetsTotal,
    [property: JsonPropertyName("capitalLeaseObligations")] string? capitalLeaseObligations,
    [property: JsonPropertyName("longTermDebtTotal")] string? longTermDebtTotal,
    [property: JsonPropertyName("nonCurrentLiabilitiesOther")] string? nonCurrentLiabilitiesOther,
    [property: JsonPropertyName("nonCurrentLiabilitiesTotal")] string? nonCurrentLiabilitiesTotal,
    [property: JsonPropertyName("negativeGoodwill")] string? negativeGoodwill,
    [property: JsonPropertyName("warrants")] string? warrants,
    [property: JsonPropertyName("preferredStockRedeemable")] string? preferredStockRedeemable,
    [property: JsonPropertyName("capitalSurpluse")] string? capitalSurpluse,
    [property: JsonPropertyName("liabilitiesAndStockholdersEquity")] string? liabilitiesAndStockholdersEquity,
    [property: JsonPropertyName("cashAndShortTermInvestments")] string? cashAndShortTermInvestments,
    [property: JsonPropertyName("propertyPlantAndEquipmentGross")] string? propertyPlantAndEquipmentGross,
    [property: JsonPropertyName("propertyPlantAndEquipmentNet")] string? propertyPlantAndEquipmentNet,
    [property: JsonPropertyName("accumulatedDepreciation")] string? accumulatedDepreciation,
    [property: JsonPropertyName("netWorkingCapital")] string? netWorkingCapital,
    [property: JsonPropertyName("netInvestedCapital")] string? netInvestedCapital,
    [property: JsonPropertyName("commonStockSharesOutstanding")] string? commonStockSharesOutstanding
);

public sealed record EodhdCashFlowDto(
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("quarterly")] Dictionary<string, EodhdCashFlowQuarterlyDto> quarterly,
    [property: JsonPropertyName("yearly")] Dictionary<string, EodhdCashFlowYearlyDto> yearly
);

public sealed record EodhdCashFlowQuarterlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("investments")] string? investments,
    [property: JsonPropertyName("changeToLiabilities")] string? changeToLiabilities,
    [property: JsonPropertyName("totalCashflowsFromInvestingActivities")] string? totalCashflowsFromInvestingActivities,
    [property: JsonPropertyName("netBorrowings")] string? netBorrowings,
    [property: JsonPropertyName("totalCashFromFinancingActivities")] string? totalCashFromFinancingActivities,
    [property: JsonPropertyName("changeToOperatingActivities")] string? changeToOperatingActivities,
    [property: JsonPropertyName("netIncome")] string? netIncome,
    [property: JsonPropertyName("changeInCash")] string? changeInCash,
    [property: JsonPropertyName("beginPeriodCashFlow")] string? beginPeriodCashFlow,
    [property: JsonPropertyName("endPeriodCashFlow")] string? endPeriodCashFlow,
    [property: JsonPropertyName("totalCashFromOperatingActivities")] string? totalCashFromOperatingActivities,
    [property: JsonPropertyName("issuanceOfCapitalStock")] string? issuanceOfCapitalStock,
    [property: JsonPropertyName("depreciation")] string? depreciation,
    [property: JsonPropertyName("otherCashflowsFromInvestingActivities")] string? otherCashflowsFromInvestingActivities,
    [property: JsonPropertyName("dividendsPaid")] string? dividendsPaid,
    [property: JsonPropertyName("changeToInventory")] string? changeToInventory,
    [property: JsonPropertyName("changeToAccountReceivables")] string? changeToAccountReceivables,
    [property: JsonPropertyName("salePurchaseOfStock")] string? salePurchaseOfStock,
    [property: JsonPropertyName("otherCashflowsFromFinancingActivities")] string? otherCashflowsFromFinancingActivities,
    [property: JsonPropertyName("changeToNetincome")] string? changeToNetincome,
    [property: JsonPropertyName("capitalExpenditures")] string? capitalExpenditures,
    [property: JsonPropertyName("changeReceivables")] string? changeReceivables,
    [property: JsonPropertyName("cashFlowsOtherOperating")] string? cashFlowsOtherOperating,
    [property: JsonPropertyName("exchangeRateChanges")] string? exchangeRateChanges,
    [property: JsonPropertyName("cashAndCashEquivalentsChanges")] string? cashAndCashEquivalentsChanges,
    [property: JsonPropertyName("changeInWorkingCapital")] string? changeInWorkingCapital,
    [property: JsonPropertyName("stockBasedCompensation")] string? stockBasedCompensation,
    [property: JsonPropertyName("otherNonCashItems")] string? otherNonCashItems,
    [property: JsonPropertyName("freeCashFlow")] string? freeCashFlow
);

public sealed record EodhdCashFlowYearlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("investments")] string? investments,
    [property: JsonPropertyName("changeToLiabilities")] string? changeToLiabilities,
    [property: JsonPropertyName("totalCashflowsFromInvestingActivities")] string? totalCashflowsFromInvestingActivities,
    [property: JsonPropertyName("netBorrowings")] string? netBorrowings,
    [property: JsonPropertyName("totalCashFromFinancingActivities")] string? totalCashFromFinancingActivities,
    [property: JsonPropertyName("changeToOperatingActivities")] string? changeToOperatingActivities,
    [property: JsonPropertyName("netIncome")] string? netIncome,
    [property: JsonPropertyName("changeInCash")] string? changeInCash,
    [property: JsonPropertyName("beginPeriodCashFlow")] string? beginPeriodCashFlow,
    [property: JsonPropertyName("endPeriodCashFlow")] string? endPeriodCashFlow,
    [property: JsonPropertyName("totalCashFromOperatingActivities")] string? totalCashFromOperatingActivities,
    [property: JsonPropertyName("issuanceOfCapitalStock")] string? issuanceOfCapitalStock,
    [property: JsonPropertyName("depreciation")] string? depreciation,
    [property: JsonPropertyName("otherCashflowsFromInvestingActivities")] string? otherCashflowsFromInvestingActivities,
    [property: JsonPropertyName("dividendsPaid")] string? dividendsPaid,
    [property: JsonPropertyName("changeToInventory")] string? changeToInventory,
    [property: JsonPropertyName("changeToAccountReceivables")] string? changeToAccountReceivables,
    [property: JsonPropertyName("salePurchaseOfStock")] string? salePurchaseOfStock,
    [property: JsonPropertyName("otherCashflowsFromFinancingActivities")] string? otherCashflowsFromFinancingActivities,
    [property: JsonPropertyName("changeToNetincome")] string? changeToNetincome,
    [property: JsonPropertyName("capitalExpenditures")] string? capitalExpenditures,
    [property: JsonPropertyName("changeReceivables")] string? changeReceivables,
    [property: JsonPropertyName("cashFlowsOtherOperating")] string? cashFlowsOtherOperating,
    [property: JsonPropertyName("exchangeRateChanges")] string? exchangeRateChanges,
    [property: JsonPropertyName("cashAndCashEquivalentsChanges")] string? cashAndCashEquivalentsChanges,
    [property: JsonPropertyName("changeInWorkingCapital")] string? changeInWorkingCapital,
    [property: JsonPropertyName("stockBasedCompensation")] string? stockBasedCompensation,
    [property: JsonPropertyName("otherNonCashItems")] string? otherNonCashItems,
    [property: JsonPropertyName("freeCashFlow")] string? freeCashFlow
);

public sealed record EodhdIncomeStatementDto(
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("quarterly")] Dictionary<string, EodhdIncomeStatementQuarterlyDto> quarterly,
    [property: JsonPropertyName("yearly")] Dictionary<string, EodhdIncomeStatementYearlyDto> yearly
);

public sealed record EodhdIncomeStatementQuarterlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("researchDevelopment")] string? researchDevelopment,
    [property: JsonPropertyName("effectOfAccountingCharges")] string? effectOfAccountingCharges,
    [property: JsonPropertyName("incomeBeforeTax")] string? incomeBeforeTax,
    [property: JsonPropertyName("minorityInterest")] string? minorityInterest,
    [property: JsonPropertyName("netIncome")] string? netIncome,
    [property: JsonPropertyName("sellingGeneralAdministrative")] string? sellingGeneralAdministrative,
    [property: JsonPropertyName("sellingAndMarketingExpenses")] string? sellingAndMarketingExpenses,
    [property: JsonPropertyName("grossProfit")] string? grossProfit,
    [property: JsonPropertyName("reconciledDepreciation")] string? reconciledDepreciation,
    [property: JsonPropertyName("ebit")] string? ebit,
    [property: JsonPropertyName("ebitda")] string? ebitda,
    [property: JsonPropertyName("depreciationAndAmortization")] string? depreciationAndAmortization,
    [property: JsonPropertyName("nonOperatingIncomeNetOther")] string? nonOperatingIncomeNetOther,
    [property: JsonPropertyName("operatingIncome")] string? operatingIncome,
    [property: JsonPropertyName("otherOperatingExpenses")] string? otherOperatingExpenses,
    [property: JsonPropertyName("interestExpense")] string? interestExpense,
    [property: JsonPropertyName("taxProvision")] string? taxProvision,
    [property: JsonPropertyName("interestIncome")] string? interestIncome,
    [property: JsonPropertyName("netInterestIncome")] string? netInterestIncome,
    [property: JsonPropertyName("extraordinaryItems")] string? extraordinaryItems,
    [property: JsonPropertyName("nonRecurring")] string? nonRecurring,
    [property: JsonPropertyName("otherItems")] string? otherItems,
    [property: JsonPropertyName("incomeTaxExpense")] string? incomeTaxExpense,
    [property: JsonPropertyName("totalRevenue")] string? totalRevenue,
    [property: JsonPropertyName("totalOperatingExpenses")] string? totalOperatingExpenses,
    [property: JsonPropertyName("costOfRevenue")] string? costOfRevenue,
    [property: JsonPropertyName("totalOtherIncomeExpenseNet")] string? totalOtherIncomeExpenseNet,
    [property: JsonPropertyName("discontinuedOperations")] string? discontinuedOperations,
    [property: JsonPropertyName("netIncomeFromContinuingOps")] string? netIncomeFromContinuingOps,
    [property: JsonPropertyName("netIncomeApplicableToCommonShares")] string? netIncomeApplicableToCommonShares,
    [property: JsonPropertyName("preferredStockAndOtherAdjustments")] string? preferredStockAndOtherAdjustments
);

public sealed record EodhdIncomeStatementYearlyDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("filing_date")] string? filing_date,
    [property: JsonPropertyName("currency_symbol")] string? currency_symbol,
    [property: JsonPropertyName("researchDevelopment")] string? researchDevelopment,
    [property: JsonPropertyName("effectOfAccountingCharges")] string? effectOfAccountingCharges,
    [property: JsonPropertyName("incomeBeforeTax")] string? incomeBeforeTax,
    [property: JsonPropertyName("minorityInterest")] string? minorityInterest,
    [property: JsonPropertyName("netIncome")] string? netIncome,
    [property: JsonPropertyName("sellingGeneralAdministrative")] string? sellingGeneralAdministrative,
    [property: JsonPropertyName("sellingAndMarketingExpenses")] string? sellingAndMarketingExpenses,
    [property: JsonPropertyName("grossProfit")] string? grossProfit,
    [property: JsonPropertyName("reconciledDepreciation")] string? reconciledDepreciation,
    [property: JsonPropertyName("ebit")] string? ebit,
    [property: JsonPropertyName("ebitda")] string? ebitda,
    [property: JsonPropertyName("depreciationAndAmortization")] string? depreciationAndAmortization,
    [property: JsonPropertyName("nonOperatingIncomeNetOther")] string? nonOperatingIncomeNetOther,
    [property: JsonPropertyName("operatingIncome")] string? operatingIncome,
    [property: JsonPropertyName("otherOperatingExpenses")] string? otherOperatingExpenses,
    [property: JsonPropertyName("interestExpense")] string? interestExpense,
    [property: JsonPropertyName("taxProvision")] string? taxProvision,
    [property: JsonPropertyName("interestIncome")] string? interestIncome,
    [property: JsonPropertyName("netInterestIncome")] string? netInterestIncome,
    [property: JsonPropertyName("extraordinaryItems")] string? extraordinaryItems,
    [property: JsonPropertyName("nonRecurring")] string? nonRecurring,
    [property: JsonPropertyName("otherItems")] string? otherItems,
    [property: JsonPropertyName("incomeTaxExpense")] string? incomeTaxExpense,
    [property: JsonPropertyName("totalRevenue")] string? totalRevenue,
    [property: JsonPropertyName("totalOperatingExpenses")] string? totalOperatingExpenses,
    [property: JsonPropertyName("costOfRevenue")] string? costOfRevenue,
    [property: JsonPropertyName("totalOtherIncomeExpenseNet")] string? totalOtherIncomeExpenseNet,
    [property: JsonPropertyName("discontinuedOperations")] string? discontinuedOperations,
    [property: JsonPropertyName("netIncomeFromContinuingOps")] string? netIncomeFromContinuingOps,
    [property: JsonPropertyName("netIncomeApplicableToCommonShares")] string? netIncomeApplicableToCommonShares,
    [property: JsonPropertyName("preferredStockAndOtherAdjustments")] string? preferredStockAndOtherAdjustments
);
