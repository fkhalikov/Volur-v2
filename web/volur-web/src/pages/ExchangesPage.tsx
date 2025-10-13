import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import LoadingSpinner from '../components/LoadingSpinner'
import ErrorMessage from '../components/ErrorMessage'
import EmptyState from '../components/EmptyState'

export default function ExchangesPage() {
  const [searchTerm, setSearchTerm] = useState('')

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['exchanges'],
    queryFn: () => api.getExchanges(),
  })

  const filteredExchanges = data?.items.filter(
    (exchange) =>
      exchange.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exchange.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exchange.country.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (isLoading) {
    return <LoadingSpinner />
  }

  if (error) {
    return (
      <ErrorMessage
        message={error instanceof Error ? error.message : 'Failed to load exchanges'}
        onRetry={() => refetch()}
      />
    )
  }

  return (
    <div>
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Exchanges</h1>
        <p className="text-gray-600">
          Browse {data?.count} financial exchanges worldwide
        </p>
      </div>

      {/* Cache info */}
      {data?.cache && (
        <div className="mb-4 text-sm text-gray-500">
          Cached from {data.cache.source} • TTL: {Math.round(data.cache.ttlSeconds / 60)} minutes
        </div>
      )}

      {/* Search */}
      <div className="mb-6">
        <input
          type="text"
          placeholder="Search by code, name, or country..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Table */}
      {filteredExchanges && filteredExchanges.length > 0 ? (
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Code
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Name
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Country
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Currency
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredExchanges.map((exchange) => (
                <tr key={exchange.code} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {exchange.code}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">{exchange.name}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-500">{exchange.country}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-500">{exchange.currency}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <Link
                      to={`/exchanges/${exchange.code}/symbols`}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      View Symbols →
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <EmptyState message="No exchanges found matching your search." />
      )}
    </div>
  )
}

