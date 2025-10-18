import { useState, useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router-dom'
import { api } from '../api/client'
import LoadingSpinner from '../components/LoadingSpinner'
import ErrorMessage from '../components/ErrorMessage'
import EmptyState from '../components/EmptyState'
import { useDebounce } from '../hooks/useDebounce'

export default function SymbolsPage() {
  const { code } = useParams<{ code: string }>()
  const [searchTerm, setSearchTerm] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(50)
  const [isRefreshing, setIsRefreshing] = useState(false)
  
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

  const handleCopyTicker = (ticker: string) => {
    navigator.clipboard.writeText(ticker)
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
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Currency
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    ISIN
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="bg-slate-800 divide-y divide-slate-700">
                {data.items.map((symbol) => (
                  <tr key={symbol.fullSymbol} className="hover:bg-slate-700">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <button
                        onClick={() => handleCopyTicker(symbol.fullSymbol)}
                        className="text-sm font-medium text-blue-400 hover:text-blue-300 cursor-pointer"
                        title="Click to copy"
                      >
                        {symbol.fullSymbol}
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-white">{symbol.name}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-slate-300">{symbol.type || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-slate-300">{symbol.currency || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-slate-300 font-mono">{symbol.isin || '-'}</div>
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
    </div>
  )
}

