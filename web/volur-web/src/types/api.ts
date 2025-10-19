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

export interface StockFundamentalsDto {
  ticker: string
  companyName?: string
  sector?: string
  industry?: string
  description?: string
  website?: string
  logoUrl?: string
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
  processedSymbols: number
  successfulFetches: number
  failedFetches: number
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

