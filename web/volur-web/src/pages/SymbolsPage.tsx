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

  return (
    <div>
      {/* Breadcrumb */}
      <nav className="mb-4 text-sm text-gray-500">
        <Link to="/exchanges" className="hover:text-blue-600">
          Exchanges
        </Link>
        <span className="mx-2">/</span>
        <span className="text-gray-900">{data?.exchange.name}</span>
      </nav>

      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          {data?.exchange.name}
        </h1>
        <p className="text-gray-600">
          {data?.exchange.country} • {data?.exchange.currency}
        </p>
      </div>

      {/* Cache info and pagination summary */}
      <div className="mb-4 flex justify-between items-center text-sm text-gray-500">
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
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Table */}
      {data?.items && data.items.length > 0 ? (
        <>
          <div className="bg-white shadow rounded-lg overflow-hidden mb-6">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Ticker
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Type
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Currency
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    ISIN
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {data.items.map((symbol) => (
                  <tr key={symbol.ticker} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <button
                        onClick={() => handleCopyTicker(symbol.ticker)}
                        className="text-sm font-medium text-blue-600 hover:text-blue-900 cursor-pointer"
                        title="Click to copy"
                      >
                        {symbol.ticker}
                      </button>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">{symbol.name}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500">{symbol.type || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500">{symbol.currency || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500 font-mono">{symbol.isin || '-'}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          symbol.isActive
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
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
                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-sm text-gray-700">
                Page {page} of {Math.ceil(data.pagination.total / data.pagination.pageSize)}
              </span>
              <button
                onClick={() => setPage((p) => p + 1)}
                disabled={!data.pagination.hasNext}
                className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
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

