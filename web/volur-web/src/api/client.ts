import { ExchangesResponse, SymbolsResponse, StockDetailsResponse, BulkFetchFundamentalsResponse } from '../types/api'

const API_BASE_URL = '/api'

class ApiError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public code?: string
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}))
    throw new ApiError(
      errorData.error?.message || response.statusText,
      response.status,
      errorData.error?.code
    )
  }
  return response.json()
}

export const api = {
  async getExchanges(forceRefresh = false): Promise<ExchangesResponse> {
    const url = new URL(`${API_BASE_URL}/exchanges`, window.location.origin)
    if (forceRefresh) {
      url.searchParams.set('forceRefresh', 'true')
    }
    const response = await fetch(url.toString())
    return handleResponse<ExchangesResponse>(response)
  },

  async getSymbols(
    exchangeCode: string,
    params: {
      page?: number
      pageSize?: number
      q?: string
      type?: string
      forceRefresh?: boolean
    } = {}
  ): Promise<SymbolsResponse> {
    const url = new URL(
      `${API_BASE_URL}/exchanges/${exchangeCode}/symbols`,
      window.location.origin
    )
    
    if (params.page) url.searchParams.set('page', params.page.toString())
    if (params.pageSize) url.searchParams.set('pageSize', params.pageSize.toString())
    if (params.q) url.searchParams.set('q', params.q)
    if (params.type) url.searchParams.set('type', params.type)
    if (params.forceRefresh) url.searchParams.set('forceRefresh', 'true')

    const response = await fetch(url.toString())
    return handleResponse<SymbolsResponse>(response)
  },

  async refreshSymbols(exchangeCode: string): Promise<void> {
    const response = await fetch(
      `${API_BASE_URL}/exchanges/${exchangeCode}/symbols/refresh`,
      { method: 'POST' }
    )
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      throw new ApiError(
        errorData.error?.message || response.statusText,
        response.status,
        errorData.error?.code
      )
    }
  },

  async getStockDetails(
    ticker: string,
    forceRefresh = false
  ): Promise<StockDetailsResponse> {
    const url = new URL(`${API_BASE_URL}/stocks/${ticker}/details`, window.location.origin)
    if (forceRefresh) {
      url.searchParams.set('forceRefresh', 'true')
    }
    const response = await fetch(url.toString())
    return handleResponse<StockDetailsResponse>(response)
  },

  async bulkFetchFundamentals(
    exchangeCode: string,
    batchSize = 3000
  ): Promise<BulkFetchFundamentalsResponse> {
    const url = new URL(
      `${API_BASE_URL}/exchanges/${exchangeCode}/symbols/bulk-fetch-fundamentals`,
      window.location.origin
    )
    if (batchSize !== 3000) {
      url.searchParams.set('batchSize', batchSize.toString())
    }
    const response = await fetch(url.toString(), {
      method: 'POST',
    })
    return handleResponse<BulkFetchFundamentalsResponse>(response)
  },
}

export { ApiError }

