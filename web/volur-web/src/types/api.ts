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
  name: string
  type?: string
  currency?: string
  isin?: string
  isActive: boolean
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

export interface ErrorResponse {
  error: {
    code: string
    message: string
    traceId: string
  }
}

