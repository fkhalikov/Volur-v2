import { useState, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { api } from '../api/client'
import LoadingSpinner from '../components/LoadingSpinner'
import ErrorMessage from '../components/ErrorMessage'
import EmptyState from '../components/EmptyState'
import StockDetailsModal from '../components/StockDetailsModal'
import { useDebounce } from '../hooks/useDebounce'
import { StockDetailsResponse } from '../types/api'

// Helper functions for formatting values
const formatMarketCap = (value?: number): string => {
  if (!value) return '-'
  if (value >= 1e12) return `$${(value / 1e12).toFixed(1)}T`
  if (value >= 1e9) return `$${(value / 1e9).toFixed(1)}B`
  if (value >= 1e6) return `$${(value / 1e6).toFixed(1)}M`
  return `$${value.toLocaleString()}`
}

const formatPrice = (value?: number): string => {
  if (!value) return '-'
  return `$${value.toFixed(2)}`
}

const formatPercentage = (value?: number): string => {
  if (!value) return '-'
  return `${value.toFixed(2)}%`
}

const formatRatio = (value?: number): string => {
  if (!value) return '-'
  return value.toFixed(1)
}

export default function SymbolsPage() {
  const { code } = useParams<{ code: string }>()
  const [searchTerm, setSearchTerm] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(50)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [isRefreshingFundamentals, setIsRefreshingFundamentals] = useState(false)
  
  // Stock details modal state
  const [selectedStock, setSelectedStock] = useState<StockDetailsResponse | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [isLoadingStockDetails, setIsLoadingStockDetails] = useState(false)
  const [stockDetailsError, setStockDetailsError] = useState<string | null>(null)
  const [shouldRefreshGridOnClose, setShouldRefreshGridOnClose] = useState(false)
  
  const debouncedSearch = useDebounce(searchTerm, 300)

  // Reset page when search changes
  useEffect(() => {
    setPage(1)
  }, [debouncedSearch])

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['symbols', code, page, pageSize, debouncedSearch],
    queryFn: () =>
      api.getSymbols(code!, {
        page,
        pageSize,
        q: debouncedSearch || undefined,
      }),
    enabled: !!code,
  })

  if (!code) {
    return <ErrorMessage message="Exchange code is required" />
  }

  if (isLoading) {
    return <LoadingSpinner />
  }

  if (error) {
    return (
      <ErrorMessage
        message={error instanceof Error ? error.message : 'Failed to load symbols'}
        onRetry={() => refetch()}
      />
    )
  }

  const handleCopyTicker = (ticker: string, event: React.MouseEvent) => {
    event.stopPropagation() // Prevent row click when copying
    navigator.clipboard.writeText(ticker)
  }

  const handleSymbolClick = async (ticker: string) => {
    setIsLoadingStockDetails(true)
    setStockDetailsError(null)
    setIsModalOpen(true)
    setShouldRefreshGridOnClose(false) // Reset the flag
    
    try {
      const stockDetails = await api.getStockDetails(ticker)
      setSelectedStock(stockDetails)
      
      // If we successfully loaded stock details (which means we may have cached new data),
      // mark that we should refresh the grid when the modal closes
      if (stockDetails.quote || stockDetails.fundamentals) {
        setShouldRefreshGridOnClose(true)
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to load stock details'
      setStockDetailsError(errorMessage)
      console.error('Failed to fetch stock details:', error)
    } finally {
      setIsLoadingStockDetails(false)
    }
  }

  const handleCloseModal = async () => {
    setIsModalOpen(false)
    setSelectedStock(null)
    setStockDetailsError(null)
    
    // Only refresh the grid if we loaded new data in the modal
    if (shouldRefreshGridOnClose && code) {
      try {
        // Force refresh to get updated fundamental data
        await api.getSymbols(code, {
          page,
          pageSize,
          q: debouncedSearch || undefined,
          forceRefresh: true
        })
        // Then refresh the query cache
        await refetch()
      } catch (error) {
        console.error('Failed to refresh grid after closing modal:', error)
        // Don't show error to user as this is a background operation
      }
    }
    
    setShouldRefreshGridOnClose(false) // Reset the flag
  }

  const handleRefresh = async () => {
    if (!code) return
    setIsRefreshing(true)
    try {
      // Call the refresh endpoint
      const response = await fetch(`http://localhost:5000/api/exchanges/${code}/symbols/refresh`, {
        method: 'POST',
      })
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Failed to refresh' }))
        throw new Error(errorData.message || `Server returned ${response.status}`)
      }
      
      // Refetch the data
      await refetch()
      
      // Show success message (optional)
      alert('Symbols refreshed successfully!')
    } catch (err) {
      console.error('Failed to refresh symbols:', err)
      alert(`Failed to refresh symbols: ${err instanceof Error ? err.message : 'Unknown error'}`)
    } finally {
      setIsRefreshing(false)
    }
  }

  const handleRefreshFundamentals = async () => {
    if (!code) return
    setIsRefreshingFundamentals(true)
    try {
      // Force refresh symbols with fundamentals data
      await api.getSymbols(code, {
        page,
        pageSize,
        q: debouncedSearch || undefined,
        forceRefresh: true
      })
      
      // Update the query cache with the new data
      await refetch()
      alert('Fundamental data refreshed successfully!')
    } catch (err) {
      console.error('Failed to refresh fundamental data:', err)
      alert(`Failed to refresh fundamental data: ${err instanceof Error ? err.message : 'Unknown error'}`)
    } finally {
      setIsRefreshingFundamentals(false)
    }
  }

  return (
    <div>
      {/* Breadcrumb */}
      <nav className="mb-4 text-sm text-slate-400">
        <Link to="/exchanges" className="hover:text-blue-400">
          Exchanges
        </Link>
        <span className="mx-2">/</span>
        <span className="text-white">{data?.exchange.name}</span>
      </nav>

      {/* Header */}
      <div className="mb-6 flex justify-between items-start">
        <div>
          <h1 className="text-3xl font-bold text-white mb-2">
            {data?.exchange.name}
          </h1>
          <p className="text-slate-300">
            {data?.exchange.country} • {data?.exchange.currency}
          </p>
        </div>
        <div className="flex gap-3">
          <button
            onClick={handleRefreshFundamentals}
            disabled={isRefreshingFundamentals}
            className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg font-medium disabled:bg-slate-600 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
          >
            <svg
              className={`w-5 h-5 ${isRefreshingFundamentals ? 'animate-spin' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
              />
            </svg>
            {isRefreshingFundamentals ? 'Refreshing...' : 'Refresh Fundamentals'}
          </button>
          <button
            onClick={handleRefresh}
            disabled={isRefreshing}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium disabled:bg-slate-600 disabled:cursor-not-allowed transition-colors flex items-center gap-2"
          >
            <svg
              className={`w-5 h-5 ${isRefreshing ? 'animate-spin' : ''}`}
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
            {isRefreshing ? 'Refreshing...' : 'Refresh Cache'}
          </button>
        </div>
      </div>

      {/* Cache info and pagination summary */}
      <div className="mb-4 flex justify-between items-center text-sm text-slate-400">
        <div>
          {data?.cache && (
            <span>
              Cached from {data.cache.source} • TTL: {Math.round(data.cache.ttlSeconds / 60)} minutes
            </span>
          )}
        </div>
        <div>
          {data?.pagination && (
            <span>
              Showing {((data.pagination.page - 1) * data.pagination.pageSize) + 1} -{' '}
              {Math.min(data.pagination.page * data.pagination.pageSize, data.pagination.total)}{' '}
              of {data.pagination.total} symbols
            </span>
          )}
        </div>
      </div>

      {/* Search */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Search symbols by ticker or name..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 bg-slate-800 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Table */}
      {data?.items && data.items.length > 0 ? (
        <>
          <div className="bg-slate-800 shadow-lg rounded-lg overflow-hidden mb-6 border border-slate-700">
            <table className="min-w-full divide-y divide-slate-700">
              <thead className="bg-slate-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Symbol
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Price
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Change %
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Market Cap
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                    P/E Ratio
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Div Yield
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="bg-slate-800 divide-y divide-slate-700">
                {data.items.map((symbol) => (
                  <tr 
                    key={symbol.fullSymbol} 
                    className="hover:bg-slate-700 cursor-pointer transition-colors"
                    onClick={() => handleSymbolClick(symbol.ticker)}
                    title="Click to view stock details"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <button
                        onClick={(e) => handleCopyTicker(symbol.fullSymbol, e)}
                        className="text-sm font-medium text-blue-400 hover:text-blue-300 cursor-pointer"
                        title="Click to copy"
                      >
                        {symbol.fullSymbol}
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-white">{symbol.name}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="text-sm text-white font-mono">
                        {formatPrice(symbol.currentPrice)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className={`text-sm font-mono ${
                        symbol.changePercent && symbol.changePercent > 0 
                          ? 'text-green-400' 
                          : symbol.changePercent && symbol.changePercent < 0 
                          ? 'text-red-400' 
                          : 'text-slate-300'
                      }`}>
                        {formatPercentage(symbol.changePercent)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="text-sm text-slate-300 font-mono">
                        {formatMarketCap(symbol.marketCap)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="text-sm text-slate-300 font-mono">
                        {formatRatio(symbol.trailingPE)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <div className="text-sm text-slate-300 font-mono">
                        {formatPercentage(symbol.dividendYield ? symbol.dividendYield * 100 : undefined)}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-slate-300">{symbol.type || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          symbol.isActive
                            ? 'bg-green-600 text-white'
                            : 'bg-slate-600 text-slate-300'
                        }`}
                      >
                        {symbol.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {data.pagination && (
            <div className="flex justify-between items-center">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-4 py-2 border border-slate-600 rounded-md text-sm font-medium text-slate-300 bg-slate-800 hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-sm text-slate-300">
                Page {page} of {Math.ceil(data.pagination.total / data.pagination.pageSize)}
              </span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={!data.pagination.hasNext}
                className="px-4 py-2 border border-slate-600 rounded-md text-sm font-medium text-slate-300 bg-slate-800 hover:bg-slate-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          )}
        </>
      ) : (
        <EmptyState message="No symbols found matching your search." />
      )}

      {/* Stock Details Modal */}
      <StockDetailsModal
        isOpen={isModalOpen}
        onClose={handleCloseModal}
        stockDetails={selectedStock}
        isLoading={isLoadingStockDetails}
        error={stockDetailsError}
      />
    </div>
  )
}

