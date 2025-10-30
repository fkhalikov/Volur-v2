namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for stock highlights information.
/// </summary>
public sealed record StockHighlightsDto(
    long? MarketCapitalization,
    double? MarketCapitalizationMln,
    long? Ebitda,
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
    string? MostRecentQuarter,
    double? ProfitMargin,
    double? OperatingMarginTtm,
    double? ReturnOnAssetsTtm,
    double? ReturnOnEquityTtm,
    long? RevenueTtm,
    double? RevenuePerShareTtm,
    double? QuarterlyRevenueGrowthYoy,
    long? GrossProfitTtm,
    double? DilutedEpsTtm,
    double? QuarterlyEarningsGrowthYoy
);

/// <summary>
/// Data transfer object for stock valuation information.
/// </summary>
public sealed record StockValuationDto(
    double? TrailingPe,
    double? ForwardPe,
    double? PriceSalesTtm,
    double? PriceBookMrq,
    long? EnterpriseValue,
    double? EnterpriseValueRevenue,
    double? EnterpriseValueEbitda
);

/// <summary>
/// Data transfer object for stock technicals information.
/// </summary>
public sealed record StockTechnicalsDto(
    double? Beta,
    double? FiftyTwoWeekHigh,
    double? FiftyTwoWeekLow,
    double? FiftyDayMa,
    double? TwoHundredDayMa
);

/// <summary>
/// Data transfer object for stock shares statistics information.
/// </summary>
public sealed record StockSharesStatsDto(
    long? SharesOutstanding,
    long? SharesFloat,
    double? PercentInsiders,
    double? PercentInstitutions,
    long? SharesShort,
    long? SharesShortPriorMonth,
    double? ShortRatio,
    double? ShortPercentOutstanding,
    double? ShortPercentFloat
);

/// <summary>
/// Data transfer object for stock splits and dividends information.
/// </summary>
public sealed record StockSplitsDividendsDto(
    double? PayoutRatio,
    string? DividendDate,
    string? ExDividendDate,
    double? DividendPerShare,
    double? DividendYield,
    int? NumberDividendsByYear
);

/// <summary>
/// Data transfer object for stock earnings information.
/// </summary>
public sealed record StockEarningsDto(
    Dictionary<string, StockEarningHistoryDto>? History,
    Dictionary<string, StockEarningTrendDto>? Trend,
    StockEarningAnnualDto? Annual
);

/// <summary>
/// Data transfer object for stock earning history.
/// </summary>
public sealed record StockEarningHistoryDto(
    string? ReportDate,
    string? Date,
    string? BeforeAfterMarket,
    string? Currency,
    double? EpsEstimate,
    double? EpsActual,
    double? Difference,
    double? Percent
);

/// <summary>
/// Data transfer object for stock earning trends.
/// </summary>
public sealed record StockEarningTrendDto(
    string? Date,
    string? Period,
    string? Growth,
    double? EarningsEstimateAvg,
    double? EarningsEstimateLow,
    double? EarningsEstimateHigh,
    double? EarningsEstimateYearAgoEps,
    int? EarningsEstimateNumberOfAnalysts,
    double? EarningsEstimateGrowth,
    double? RevenueEstimateAvg,
    double? RevenueEstimateLow,
    double? RevenueEstimateHigh,
    double? RevenueEstimateYearAgoEps,
    int? RevenueEstimateNumberOfAnalysts,
    double? RevenueEstimateGrowth
);

/// <summary>
/// Data transfer object for stock annual earnings.
/// </summary>
public sealed record StockEarningAnnualDto(
    Dictionary<string, StockEarningAnnualDataDto>? Earnings
);

/// <summary>
/// Data transfer object for annual earnings data.
/// </summary>
public sealed record StockEarningAnnualDataDto(
    string? ReportDate,
    double? EpsActual
);

/// <summary>
/// Data transfer object for stock financials information.
/// </summary>
public sealed record StockFinancialsDto(
    StockBalanceSheetDto? BalanceSheet,
    StockCashFlowDto? CashFlow,
    StockIncomeStatementDto? IncomeStatement
);

/// <summary>
/// Data transfer object for balance sheet information.
/// </summary>
public sealed record StockBalanceSheetDto(
    Dictionary<string, StockBalanceSheetDataDto>? Quarterly,
    Dictionary<string, StockBalanceSheetDataDto>? Yearly
);

/// <summary>
/// Data transfer object for cash flow information.
/// </summary>
public sealed record StockCashFlowDto(
    Dictionary<string, StockCashFlowDataDto>? Quarterly,
    Dictionary<string, StockCashFlowDataDto>? Yearly
);

/// <summary>
/// Data transfer object for income statement information.
/// </summary>
public sealed record StockIncomeStatementDto(
    Dictionary<string, StockIncomeStatementDataDto>? Quarterly,
    Dictionary<string, StockIncomeStatementDataDto>? Yearly
);

/// <summary>
/// Data transfer object for balance sheet data.
/// </summary>
public sealed record StockBalanceSheetDataDto(
    string? Date,
    string? Filing_date,
    string? Currency_symbol,
    string? TotalAssets,
    string? TotalCurrentAssets,
    string? CashAndCashEquivalentsAtCarryingValue,
    string? CashAndShortTermInvestments,
    string? NetReceivables,
    string? Inventory,
    string? OtherCurrentAssets,
    string? TotalNonCurrentAssets,
    string? PropertyPlantEquipment,
    string? AccumulatedDepreciation,
    string? OtherAssets,
    string? DeferredLongTermAssetCharges,
    string? TotalLiab,
    string? TotalCurrentLiabilities,
    string? CurrentPortionOfLongTermDebt,
    string? ShortTermDebt,
    string? AccountsPayable,
    string? OtherCurrentLiab,
    string? TotalNonCurrentLiabilities,
    string? TotalLongTermDebt,
    string? OtherLiab,
    string? DeferredLongTermLiabilityCharges,
    string? MinorityInterest,
    string? NegativeGoodwill,
    string? Warrants,
    string? PreferredStockRedeemable,
    string? CapitalSurplus,
    string? RetainedEarnings,
    string? TreasuryStock,
    string? CapitalStock,
    string? OtherStockholderEquity,
    string? TotalStockholderEquity,
    string? NetTangibleAssets
);

/// <summary>
/// Data transfer object for cash flow data.
/// </summary>
public sealed record StockCashFlowDataDto(
    string? Date,
    string? Filing_date,
    string? Currency_symbol,
    string? Investments,
    string? ChangeToLiabilities,
    string? TotalCashflowsFromInvestingActivities,
    string? NetBorrowings,
    string? TotalCashFromFinancingActivities,
    string? ChangeToOperatingActivities,
    string? NetIncome,
    string? ChangeInCash,
    string? BeginPeriodCashFlow,
    string? EndPeriodCashFlow,
    string? TotalCashFromOperatingActivities,
    string? IssuanceOfCapitalStock,
    string? Depreciation,
    string? OtherCashflowsFromInvestingActivities,
    string? DividendsPaid,
    string? ChangeToInventory,
    string? ChangeToAccountReceivables,
    string? SalePurchaseOfStock,
    string? OtherCashflowsFromFinancingActivities,
    string? ChangeToNetincome,
    string? CapitalExpenditures,
    string? ChangeReceivables,
    string? CashFlowsOtherOperating,
    string? ExchangeRateChanges,
    string? CashAndCashEquivalentsChanges
);

/// <summary>
/// Data transfer object for income statement data.
/// </summary>
public sealed record StockIncomeStatementDataDto(
    string? Date,
    string? Filing_date,
    string? Currency_symbol,
    string? ResearchDevelopment,
    string? EffectOfAccountingCharges,
    string? IncomeBeforeTax,
    string? MinorityInterest,
    string? NetIncome,
    string? SellingGeneralAdministrative,
    string? SellingAndMarketingExpenses,
    string? GrossProfit,
    string? ReconciledDepreciation,
    string? Ebit,
    string? Ebitda,
    string? DepreciationAndAmortization,
    string? NonOperatingIncomeNetOther,
    string? OperatingIncome,
    string? OtherOperatingExpenses,
    string? InterestExpense,
    string? TaxProvision,
    string? InterestIncome,
    string? NetInterestIncome,
    string? ExtraordinaryItems,
    string? NonRecurring,
    string? OtherItems,
    string? IncomeTaxExpense,
    string? TotalRevenue,
    string? TotalOperatingExpenses,
    string? CostOfRevenue,
    string? TotalOtherIncomeExpenseNet,
    string? DiscontinuedOperations,
    string? NetIncomeFromContinuingOps,
    string? NetIncomeApplicableToCommonShares,
    string? PreferredStockAndOtherAdjustments
);

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
    string? CurrencyCode,
    string? CurrencySymbol,
    string? CurrencyName,
    StockHighlightsDto? Highlights,
    StockValuationDto? Valuation,
    StockSharesStatsDto? SharesStats,
    StockTechnicalsDto? Technicals,
    StockSplitsDividendsDto? SplitsDividends,
    StockEarningsDto? Earnings,
    StockFinancialsDto? Financials,
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
