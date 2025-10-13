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
        <h1 className="text-3xl font-bold text-white mb-2">Exchanges</h1>
        <p className="text-slate-300">
          Browse {data?.count} financial exchanges worldwide
        </p>
      </div>

      {/* Cache info */}
      {data?.cache && (
        <div className="mb-4 text-sm text-slate-400">
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
          className="w-full px-4 py-2 bg-slate-800 border border-slate-600 rounded-lg text-white placeholder-slate-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>

      {/* Table */}
      {filteredExchanges && filteredExchanges.length > 0 ? (
        <div className="bg-slate-800 shadow-lg rounded-lg overflow-hidden border border-slate-700">
          <table className="min-w-full divide-y divide-slate-700">
            <thead className="bg-slate-700">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                  Code
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                  Name
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                  Country
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-slate-300 uppercase tracking-wider">
                  Currency
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-slate-300 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-slate-800 divide-y divide-slate-700">
              {filteredExchanges.map((exchange) => (
                <tr key={exchange.code} className="hover:bg-slate-700">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-white">
                      {exchange.code}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-white">{exchange.name}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-slate-300">{exchange.country}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-slate-300">{exchange.currency}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <Link
                      to={`/exchanges/${exchange.code}/symbols`}
                      className="text-blue-400 hover:text-blue-300"
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

