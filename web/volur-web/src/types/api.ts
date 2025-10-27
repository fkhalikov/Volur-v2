// API response types matching the .NET backend

export interface ExchangeDto {
  code: string
  name: string
  country: string
  currency: string
  operatingMic?: string
}

export interface SymbolDto {
  ticker: string
  fullSymbol: string
  name: string
  type?: string
  currency?: string
  isin?: string
  isActive: boolean
  
  // Fundamental data (optional - may be null if not available)
  marketCap?: number
  trailingPE?: number
  dividendYield?: number
  currentPrice?: number
  changePercent?: number
  sector?: string
  industry?: string
  fundamentalsFetchedAt?: string
}

export interface CacheMetadata {
  source: string
  ttlSeconds: number
}

export interface ExchangesResponse {
  count: number
  items: ExchangeDto[]
  fetchedAt: string
  cache: CacheMetadata
}

export interface PaginationMetadata {
  page: number
  pageSize: number
  total: number
  hasNext: boolean
}

export interface SymbolsResponse {
  exchange: ExchangeDto
  pagination: PaginationMetadata
  items: SymbolDto[]
  fetchedAt: string
  cache: CacheMetadata
}

export interface StockQuoteDto {
  ticker: string
  currentPrice?: number
  previousClose?: number
  change?: number
  changePercent?: number
  open?: number
  high?: number
  low?: number
  volume?: number
  averageVolume?: number
  lastUpdated: string
}

export interface StockHighlightsDto {
  marketCapitalization?: number
  marketCapitalizationMln?: number
  ebitda?: number
  peRatio?: number
  pegRatio?: number
  wallStreetTargetPrice?: number
  bookValue?: number
  dividendShare?: number
  dividendYield?: number
  earningsShare?: number
  epsEstimateCurrentYear?: number
  epsEstimateNextYear?: number
  epsEstimateNextQuarter?: number
  epsEstimateCurrentQuarter?: number
  mostRecentQuarter?: string
  profitMargin?: number
  operatingMarginTtm?: number
  returnOnAssetsTtm?: number
  returnOnEquityTtm?: number
  revenueTtm?: number
  revenuePerShareTtm?: number
  quarterlyRevenueGrowthYoy?: number
  grossProfitTtm?: number
  dilutedEpsTtm?: number
  quarterlyEarningsGrowthYoy?: number
}

export interface StockValuationDto {
  trailingPe?: number
  forwardPe?: number
  priceSalesTtm?: number
  priceBookMrq?: number
  enterpriseValue?: number
  enterpriseValueRevenue?: number
  enterpriseValueEbitda?: number
}

export interface StockTechnicalsDto {
  beta?: number
  fiftyTwoWeekHigh?: number
  fiftyTwoWeekLow?: number
  fiftyDayMa?: number
  twoHundredDayMa?: number
}

export interface StockSplitsDividendsDto {
  payoutRatio?: number
  dividendDate?: number
  exDividendDate?: number
  dividendPerShare?: number
  dividendYield?: number
  numberDividendsByYear?: number
}

export interface StockEarningsDto {
  history?: { [key: string]: StockEarningHistoryDto }
  trend?: { [key: string]: StockEarningTrendDto }
  annual?: StockEarningAnnualDto
}

export interface StockEarningHistoryDto {
  reportDate?: string
  date?: string
  beforeAfterMarket?: string
  currency?: string
  epsEstimate?: number
  epsActual?: number
  difference?: number
  percent?: number
}

export interface StockEarningTrendDto {
  date?: string
  period?: string
  growth?: string
  earningsEstimateAvg?: number
  earningsEstimateLow?: number
  earningsEstimateHigh?: number
  earningsEstimateYearAgoEps?: number
  earningsEstimateNumberOfAnalysts?: number
  earningsEstimateGrowth?: number
  revenueEstimateAvg?: number
  revenueEstimateLow?: number
  revenueEstimateHigh?: number
  revenueEstimateYearAgoEps?: number
  revenueEstimateNumberOfAnalysts?: number
  revenueEstimateGrowth?: number
}

export interface StockEarningAnnualDto {
  earnings?: { [key: string]: StockEarningAnnualDataDto }
}

export interface StockEarningAnnualDataDto {
  reportDate?: string
  epsActual?: number
}

export interface StockFinancialsDto {
  balanceSheet?: StockBalanceSheetDto
  cashFlow?: StockCashFlowDto
  incomeStatement?: StockIncomeStatementDto
}

export interface StockBalanceSheetDto {
  quarterly?: { [key: string]: StockBalanceSheetDataDto }
  yearly?: { [key: string]: StockBalanceSheetDataDto }
}

export interface StockCashFlowDto {
  quarterly?: { [key: string]: StockCashFlowDataDto }
  yearly?: { [key: string]: StockCashFlowDataDto }
}

export interface StockIncomeStatementDto {
  quarterly?: { [key: string]: StockIncomeStatementDataDto }
  yearly?: { [key: string]: StockIncomeStatementDataDto }
}

export interface StockBalanceSheetDataDto {
  date?: string
  filing_date?: string
  currency_symbol?: string
  totalAssets?: string
  totalCurrentAssets?: string
  cashAndCashEquivalentsAtCarryingValue?: string
  cashAndShortTermInvestments?: string
  netReceivables?: string
  inventory?: string
  otherCurrentAssets?: string
  totalNonCurrentAssets?: string
  propertyPlantEquipment?: string
  accumulatedDepreciation?: string
  otherAssets?: string
  deferredLongTermAssetCharges?: string
  totalLiab?: string
  totalCurrentLiabilities?: string
  currentPortionOfLongTermDebt?: string
  shortTermDebt?: string
  accountsPayable?: string
  otherCurrentLiab?: string
  totalNonCurrentLiabilities?: string
  totalLongTermDebt?: string
  otherLiab?: string
  deferredLongTermLiabilityCharges?: string
  minorityInterest?: string
  negativeGoodwill?: string
  warrants?: string
  preferredStockRedeemable?: string
  capitalSurplus?: string
  retainedEarnings?: string
  treasuryStock?: string
  capitalStock?: string
  otherStockholderEquity?: string
  totalStockholderEquity?: string
  netTangibleAssets?: string
}

export interface StockCashFlowDataDto {
  date?: string
  filing_date?: string
  currency_symbol?: string
  investments?: string
  changeToLiabilities?: string
  totalCashflowsFromInvestingActivities?: string
  netBorrowings?: string
  totalCashFromFinancingActivities?: string
  changeToOperatingActivities?: string
  netIncome?: string
  changeInCash?: string
  beginPeriodCashFlow?: string
  endPeriodCashFlow?: string
  totalCashFromOperatingActivities?: string
  issuanceOfCapitalStock?: string
  depreciation?: string
  otherCashflowsFromInvestingActivities?: string
  dividendsPaid?: string
  changeToInventory?: string
  changeToAccountReceivables?: string
  salePurchaseOfStock?: string
  otherCashflowsFromFinancingActivities?: string
  changeToNetincome?: string
  capitalExpenditures?: string
  changeReceivables?: string
  cashFlowsOtherOperating?: string
  exchangeRateChanges?: string
  cashAndCashEquivalentsChanges?: string
}

export interface StockIncomeStatementDataDto {
  date?: string
  filing_date?: string
  currency_symbol?: string
  researchDevelopment?: string
  effectOfAccountingCharges?: string
  incomeBeforeTax?: string
  minorityInterest?: string
  netIncome?: string
  sellingGeneralAdministrative?: string
  sellingAndMarketingExpenses?: string
  grossProfit?: string
  reconciledDepreciation?: string
  ebit?: string
  ebitda?: string
  depreciationAndAmortization?: string
  nonOperatingIncomeNetOther?: string
  operatingIncome?: string
  otherOperatingExpenses?: string
  interestExpense?: string
  taxProvision?: string
  interestIncome?: string
  netInterestIncome?: string
  extraordinaryItems?: string
  nonRecurring?: string
  otherItems?: string
  incomeTaxExpense?: string
  totalRevenue?: string
  totalOperatingExpenses?: string
  costOfRevenue?: string
  totalOtherIncomeExpenseNet?: string
  discontinuedOperations?: string
  netIncomeFromContinuingOps?: string
  netIncomeApplicableToCommonShares?: string
  preferredStockAndOtherAdjustments?: string
}

export interface StockFundamentalsDto {
  ticker: string
  companyName?: string
  sector?: string
  industry?: string
  description?: string
  website?: string
  logoUrl?: string
  currencyCode?: string
  currencySymbol?: string
  currencyName?: string
  highlights?: StockHighlightsDto
  valuation?: StockValuationDto
  technicals?: StockTechnicalsDto
  splitsDividends?: StockSplitsDividendsDto
  earnings?: StockEarningsDto
  financials?: StockFinancialsDto
  marketCap?: number
  enterpriseValue?: number
  trailingPE?: number
  forwardPE?: number
  peg?: number
  priceToSales?: number
  priceToBook?: number
  enterpriseToRevenue?: number
  enterpriseToEbitda?: number
  profitMargins?: number
  grossMargins?: number
  operatingMargins?: number
  returnOnAssets?: number
  returnOnEquity?: number
  revenue?: number
  revenuePerShare?: number
  quarterlyRevenueGrowth?: number
  quarterlyEarningsGrowth?: number
  totalCash?: number
  totalCashPerShare?: number
  totalDebt?: number
  debtToEquity?: number
  currentRatio?: number
  bookValue?: number
  priceToBookValue?: number
  dividendRate?: number
  dividendYield?: number
  payoutRatio?: number
  beta?: number
  fiftyTwoWeekLow?: number
  fiftyTwoWeekHigh?: number
  lastUpdated: string
}

export interface StockDetailsResponse {
  symbol: SymbolDto
  quote?: StockQuoteDto
  fundamentals?: StockFundamentalsDto
  quoteFetchedAt?: string
  fundamentalsFetchedAt?: string
  requestedAt: string
}

export interface BulkFetchFundamentalsResponse {
  exchangeCode: string
  totalSymbols: number
  symbolsWithoutData: number
  skippedNoDataSymbols: number
  processedSymbols: number
  successfulFetches: number
  failedFetches: number
  rateLimitHits: number
  dailyLimitHit: boolean
  totalWaitTime: string
  batchesProcessed: number
  startedAt: string
  completedAt: string
}

export interface ErrorResponse {
  error: {
    code: string
    message: string
    traceId: string
  }
}

