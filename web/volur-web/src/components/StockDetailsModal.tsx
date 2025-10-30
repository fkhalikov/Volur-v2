import { Fragment } from 'react'
import { Dialog, Transition, Tab } from '@headlessui/react'
import { XMarkIcon } from '@heroicons/react/24/outline'
import { StockDetailsResponse } from '../types/api'
import LoadingSpinner from './LoadingSpinner'
import StockAnalysisTab from './StockAnalysisTab'

interface StockDetailsModalProps {
  isOpen: boolean
  onClose: () => void
  stockDetails: StockDetailsResponse | null
  isLoading: boolean
  error: string | null
}

export default function StockDetailsModal({
  isOpen,
  onClose,
  stockDetails,
  isLoading,
  error
}: StockDetailsModalProps) {
  const formatCurrency = (value?: number, currencyCode?: string, currencySymbol?: string) => {
    if (value === undefined || value === null) return 'N/A'
    
    // If we have a currency symbol, use it for display
    if (currencySymbol) {
      return `${currencySymbol}${new Intl.NumberFormat('en-US').format(value)}`
    }
    
    // Fall back to currency code if available
    if (currencyCode) {
      try {
        return new Intl.NumberFormat('en-US', {
          style: 'currency',
          currency: currencyCode
        }).format(value)
      } catch {
        // If currency code is invalid, fall back to symbol prefix
        return `${currencyCode} ${new Intl.NumberFormat('en-US').format(value)}`
      }
    }
    
    // Default to USD if no currency info available
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value)
  }

  const formatNumber = (value?: number) => {
    if (value === undefined || value === null) return 'N/A'
    return new Intl.NumberFormat('en-US').format(value)
  }

  const formatPercent = (value?: number) => {
    if (value === undefined || value === null) return 'N/A'
    return `${value.toFixed(2)}%`
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A'
    return new Date(dateString).toLocaleString()
  }

  function classNames(...classes: string[]) {
    return classes.filter(Boolean).join(' ')
  }

  return (
    <Transition appear show={isOpen} as={Fragment}>
      <Dialog as="div" className="relative z-10" onClose={onClose}>
        <Transition.Child
          as={Fragment}
          enter="ease-out duration-300"
          enterFrom="opacity-0"
          enterTo="opacity-100"
          leave="ease-in duration-200"
          leaveFrom="opacity-100"
          leaveTo="opacity-0"
        >
          <div className="fixed inset-0 bg-black bg-opacity-25" />
        </Transition.Child>

        <div className="fixed inset-0 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4 text-center">
            <Transition.Child
              as={Fragment}
              enter="ease-out duration-300"
              enterFrom="opacity-0 scale-95"
              enterTo="opacity-100 scale-100"
              leave="ease-in duration-200"
              leaveFrom="opacity-100 scale-100"
              leaveTo="opacity-0 scale-95"
            >
              <Dialog.Panel className="w-full max-w-4xl transform overflow-hidden rounded-2xl bg-slate-800 p-6 text-left align-middle shadow-xl transition-all border border-slate-700">
                <div className="flex justify-between items-start mb-6">
                  <Dialog.Title
                    as="h3"
                    className="text-2xl font-bold leading-6 text-white"
                  >
                    {stockDetails?.symbol.name || 'Stock Details'}
                  </Dialog.Title>
                  <button
                    type="button"
                    className="text-slate-400 hover:text-white"
                    onClick={onClose}
                  >
                    <XMarkIcon className="h-6 w-6" />
                  </button>
                </div>

                {isLoading && (
                  <div className="flex justify-center py-8">
                    <LoadingSpinner />
                  </div>
                )}

                {error && (
                  <div className="bg-red-900/50 border border-red-700 rounded-lg p-4 mb-4">
                    <p className="text-red-200">Error: {error}</p>
                  </div>
                )}

                {stockDetails && !isLoading && (
                  <div className="space-y-6">
                    {/* Symbol Information */}
                    <div className="bg-slate-700 rounded-lg p-4">
                      <h4 className="text-lg font-semibold text-white mb-3">Symbol Information</h4>
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                        <div>
                          <span className="text-slate-400">Ticker:</span>
                          <p className="text-white font-medium">{stockDetails.symbol.ticker}</p>
                        </div>
                        <div>
                          <span className="text-slate-400">Full Symbol:</span>
                          <p className="text-white font-medium">{stockDetails.symbol.fullSymbol}</p>
                        </div>
                        <div>
                          <span className="text-slate-400">Type:</span>
                          <p className="text-white">{stockDetails.symbol.type || 'N/A'}</p>
                        </div>
                        <div>
                          <span className="text-slate-400">Currency:</span>
                          <p className="text-white">{stockDetails.symbol.currency || 'N/A'}</p>
                        </div>
                        <div>
                          <span className="text-slate-400">ISIN:</span>
                          <p className="text-white font-mono text-xs">{stockDetails.symbol.isin || 'N/A'}</p>
                        </div>
                        <div>
                          <span className="text-slate-400">Status:</span>
                          <span className={`px-2 py-1 rounded-full text-xs font-semibold ${
                            stockDetails.symbol.isActive 
                              ? 'bg-green-600 text-white' 
                              : 'bg-slate-600 text-slate-300'
                          }`}>
                            {stockDetails.symbol.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </div>
                      </div>
                    </div>

                    {/* Price Information */}
                    <div className="bg-slate-700 rounded-lg p-4">
                      <div className="flex justify-between items-center mb-3">
                        <h4 className="text-lg font-semibold text-white">Price Information</h4>
                        {stockDetails.quoteFetchedAt && (
                          <span className="text-xs text-slate-400">
                            Last updated: {formatDate(stockDetails.quoteFetchedAt)}
                          </span>
                        )}
                      </div>
                      
                      {stockDetails.quote ? (
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                          <div>
                            <span className="text-slate-400">Current Price:</span>
                            <p className="text-white font-bold text-lg">{formatCurrency(stockDetails.quote.currentPrice, stockDetails.symbol.currency, stockDetails.fundamentals?.currencySymbol)}</p>
                          </div>
                          <div>
                            <span className="text-slate-400">Previous Close:</span>
                            <p className="text-white">{formatCurrency(stockDetails.quote.previousClose, stockDetails.symbol.currency, stockDetails.fundamentals?.currencySymbol)}</p>
                          </div>
                          <div>
                            <span className="text-slate-400">Change:</span>
                            <p className={`font-medium ${
                              (stockDetails.quote.change || 0) >= 0 ? 'text-green-400' : 'text-red-400'
                            }`}>
                              {stockDetails.quote.change !== undefined ? 
                                `${stockDetails.quote.change >= 0 ? '+' : ''}${stockDetails.quote.change.toFixed(2)}` : 'N/A'}
                            </p>
                          </div>
                          <div>
                            <span className="text-slate-400">Change %:</span>
                            <p className={`font-medium ${
                              (stockDetails.quote.changePercent || 0) >= 0 ? 'text-green-400' : 'text-red-400'
                            }`}>
                              {stockDetails.quote.changePercent !== undefined ? 
                                `${stockDetails.quote.changePercent >= 0 ? '+' : ''}${formatPercent(stockDetails.quote.changePercent)}` : 'N/A'}
                            </p>
                          </div>
                          <div>
                            <span className="text-slate-400">Open:</span>
                            <p className="text-white">{formatCurrency(stockDetails.quote.open, stockDetails.symbol.currency, stockDetails.fundamentals?.currencySymbol)}</p>
                          </div>
                          <div>
                            <span className="text-slate-400">High:</span>
                            <p className="text-white">{formatCurrency(stockDetails.quote.high, stockDetails.symbol.currency, stockDetails.fundamentals?.currencySymbol)}</p>
                          </div>
                          <div>
                            <span className="text-slate-400">Low:</span>
                            <p className="text-white">{formatCurrency(stockDetails.quote.low, stockDetails.symbol.currency, stockDetails.fundamentals?.currencySymbol)}</p>
                          </div>
                          <div>
                            <span className="text-slate-400">Volume:</span>
                            <p className="text-white">{formatNumber(stockDetails.quote.volume)}</p>
                          </div>
                        </div>
                      ) : (
                        <p className="text-slate-400 italic">No price data available</p>
                      )}
                    </div>

                    {/* Fundamentals Information with Tabs */}
                    <div className="bg-slate-700 rounded-lg p-4">
                      <div className="flex justify-between items-center mb-3">
                        <h4 className="text-lg font-semibold text-white">Fundamental Data</h4>
                        {stockDetails.fundamentalsFetchedAt && (
                          <span className="text-xs text-slate-400">
                            Last updated: {formatDate(stockDetails.fundamentalsFetchedAt)}
                          </span>
                        )}
                      </div>
                      
                      {stockDetails.fundamentals ? (
                        <Tab.Group>
                          <Tab.List className="flex flex-wrap gap-1 rounded-lg bg-slate-800 p-1 mb-4">
                            {['Overview', 'Description', 'Highlights', 'Valuation', 'Technicals', 'Splits & Dividends', 'Earnings', 'Financials', 'Analysis'].map((tabName) => (
                              <Tab
                                key={tabName}
                                className={({ selected }) =>
                                  classNames(
                                    'flex-1 min-w-0 rounded-md py-2 px-2 text-xs font-medium leading-5 text-white text-center',
                                    'ring-white ring-opacity-60 ring-offset-2 ring-offset-slate-700 focus:outline-none focus:ring-2',
                                    selected
                                      ? 'bg-slate-600 shadow'
                                      : 'text-slate-300 hover:bg-slate-700 hover:text-white'
                                  )
                                }
                              >
                                {tabName}
                              </Tab>
                            ))}
                          </Tab.List>
                          
                          <Tab.Panels className="mt-2">
                            {/* Overview Tab */}
                            <Tab.Panel className="space-y-4">
                              {/* Company Info */}
                              {(stockDetails.fundamentals.companyName || stockDetails.fundamentals.sector || stockDetails.fundamentals.industry) && (
                                <div>
                                  <h5 className="text-white font-medium mb-2">Company Information</h5>
                                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                                    <div>
                                      <span className="text-slate-400">Company:</span>
                                      <p className="text-white">{stockDetails.fundamentals.companyName || 'N/A'}</p>
                                    </div>
                                    <div>
                                      <span className="text-slate-400">Sector:</span>
                                      <p className="text-white">{stockDetails.fundamentals.sector || 'N/A'}</p>
                                    </div>
                                    <div>
                                      <span className="text-slate-400">Industry:</span>
                                      <p className="text-white">{stockDetails.fundamentals.industry || 'N/A'}</p>
                                    </div>
                                  </div>
                                </div>
                              )}

                              {/* Key Metrics */}
                              <div>
                                <h5 className="text-white font-medium mb-2">Key Metrics</h5>
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                                  <div>
                                    <span className="text-slate-400">Market Cap:</span>
                                    <p className="text-white">{formatCurrency(stockDetails.fundamentals.marketCap, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">P/E Ratio:</span>
                                    <p className="text-white">{stockDetails.fundamentals.trailingPE?.toFixed(2) || 'N/A'}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">Beta:</span>
                                    <p className="text-white">{stockDetails.fundamentals.beta?.toFixed(2) || 'N/A'}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">Dividend Yield:</span>
                                    <p className="text-white">{formatPercent(stockDetails.fundamentals.dividendYield)}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">52W Low:</span>
                                    <p className="text-white">{formatCurrency(stockDetails.fundamentals.fiftyTwoWeekLow, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">52W High:</span>
                                    <p className="text-white">{formatCurrency(stockDetails.fundamentals.fiftyTwoWeekHigh, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">Revenue:</span>
                                    <p className="text-white">{formatCurrency(stockDetails.fundamentals.revenue, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                  </div>
                                  <div>
                                    <span className="text-slate-400">Debt/Equity:</span>
                                    <p className="text-white">{stockDetails.fundamentals.debtToEquity?.toFixed(2) || 'N/A'}</p>
                                  </div>
                                </div>
                              </div>
                            </Tab.Panel>

                            {/* Description Tab */}
                            <Tab.Panel>
                              <div className="space-y-4">
                                <div>
                                  <h5 className="text-white font-medium mb-2">Company Description</h5>
                                  <div className="bg-slate-700 rounded-lg p-4">
                                    <p className="text-slate-200 leading-relaxed text-sm">
                                      {stockDetails.fundamentals.description || 'No company description available'}
                                    </p>
                                  </div>
                                </div>
                                
                                {/* Additional Company Details */}
                                {(stockDetails.fundamentals.website || stockDetails.fundamentals.companyName) && (
                                  <div>
                                    <h5 className="text-white font-medium mb-2">Company Details</h5>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                                      {stockDetails.fundamentals.companyName && (
                                        <div>
                                          <span className="text-slate-400">Company Name:</span>
                                          <p className="text-white">{stockDetails.fundamentals.companyName}</p>
                                        </div>
                                      )}
                                      {stockDetails.fundamentals.website && (
                                        <div>
                                          <span className="text-slate-400">Website:</span>
                                          <p className="text-white">
                                            <a 
                                              href={stockDetails.fundamentals.website} 
                                              target="_blank" 
                                              rel="noopener noreferrer"
                                              className="text-blue-400 hover:text-blue-300 underline"
                                            >
                                              {stockDetails.fundamentals.website}
                                            </a>
                                          </p>
                                        </div>
                                      )}
                                      {stockDetails.fundamentals.sector && (
                                        <div>
                                          <span className="text-slate-400">Sector:</span>
                                          <p className="text-white">{stockDetails.fundamentals.sector}</p>
                                        </div>
                                      )}
                                      {stockDetails.fundamentals.industry && (
                                        <div>
                                          <span className="text-slate-400">Industry:</span>
                                          <p className="text-white">{stockDetails.fundamentals.industry}</p>
                                        </div>
                                      )}
                                    </div>
                                  </div>
                                )}
                              </div>
                            </Tab.Panel>

                            {/* Highlights Tab */}
                            <Tab.Panel>
                              {stockDetails.fundamentals.highlights ? (
                                <div className="space-y-4">
                                  {/* Financial Highlights */}
                                  <div>
                                    <h5 className="text-white font-medium mb-2">Financial Highlights</h5>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                                      <div>
                                        <span className="text-slate-400">Market Cap:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.marketCapitalization, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">EBITDA:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.ebitda, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Revenue TTM:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.revenueTtm, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Gross Profit TTM:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.grossProfitTtm, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Book Value:</span>
                                        <p className="text-white">{stockDetails.fundamentals.highlights.bookValue?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Revenue Per Share TTM:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.revenuePerShareTtm, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">P/E Ratio:</span>
                                        <p className="text-white">{stockDetails.fundamentals.highlights.peRatio?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">PEG Ratio:</span>
                                        <p className="text-white">{stockDetails.fundamentals.highlights.pegRatio?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Profit Margin:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.profitMargin)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Operating Margin TTM:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.operatingMarginTtm)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">ROA TTM:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.returnOnAssetsTtm)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">ROE TTM:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.returnOnEquityTtm)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Earnings Per Share:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.earningsShare, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Diluted EPS TTM:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.dilutedEpsTtm, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Quarterly Revenue Growth YoY:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.quarterlyRevenueGrowthYoy)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Quarterly Earnings Growth YoY:</span>
                                        <p className={`${
                                          (stockDetails.fundamentals.highlights.quarterlyEarningsGrowthYoy || 0) >= 0 
                                            ? 'text-green-400' 
                                            : 'text-red-400'
                                        }`}>
                                          {formatPercent(stockDetails.fundamentals.highlights.quarterlyEarningsGrowthYoy)}
                                        </p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Most Recent Quarter:</span>
                                        <p className="text-white">{stockDetails.fundamentals.highlights.mostRecentQuarter || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Dividend Share:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.highlights.dividendShare, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Dividend Yield:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.highlights.dividendYield)}</p>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              ) : (
                                <p className="text-slate-400 italic">No highlights data available</p>
                              )}
                            </Tab.Panel>

                            {/* Valuation Tab */}
                            <Tab.Panel>
                              {stockDetails.fundamentals.valuation ? (
                                <div className="space-y-4">
                                  <div>
                                    <h5 className="text-white font-medium mb-2">Valuation Metrics</h5>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                                      <div>
                                        <span className="text-slate-400">Trailing P/E:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.trailingPe?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Forward P/E:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.forwardPe?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Price/Sales TTM:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.priceSalesTtm?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Price/Book MRQ:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.priceBookMrq?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Enterprise Value:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.valuation.enterpriseValue, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">EV/Revenue:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.enterpriseValueRevenue?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">EV/EBITDA:</span>
                                        <p className="text-white">{stockDetails.fundamentals.valuation.enterpriseValueEbitda?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              ) : (
                                <p className="text-slate-400 italic">No valuation data available</p>
                              )}
                            </Tab.Panel>

                            {/* Technicals Tab */}
                            <Tab.Panel>
                              {stockDetails.fundamentals.technicals ? (
                                <div className="space-y-4">
                                  <div>
                                    <h5 className="text-white font-medium mb-2">Technical Indicators</h5>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                                      <div>
                                        <span className="text-slate-400">Beta:</span>
                                        <p className="text-white">{stockDetails.fundamentals.technicals.beta?.toFixed(2) || 'N/A'}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">52W High:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.technicals.fiftyTwoWeekHigh, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">52W Low:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.technicals.fiftyTwoWeekLow, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">50-Day MA:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.technicals.fiftyDayMa, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">200-Day MA:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.technicals.twoHundredDayMa, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              ) : (
                                <p className="text-slate-400 italic">No technicals data available</p>
                              )}
                            </Tab.Panel>

                            {/* Splits & Dividends Tab */}
                            <Tab.Panel>
                              {stockDetails.fundamentals.splitsDividends ? (
                                <div className="space-y-4">
                                  <div>
                                    <h5 className="text-white font-medium mb-2">Dividend Information</h5>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                                      <div>
                                        <span className="text-slate-400">Payout Ratio:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.splitsDividends.payoutRatio)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Dividend Per Share:</span>
                                        <p className="text-white">{formatCurrency(stockDetails.fundamentals.splitsDividends.dividendPerShare, stockDetails.fundamentals.currencyCode, stockDetails.fundamentals.currencySymbol)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Dividend Yield:</span>
                                        <p className="text-white">{formatPercent(stockDetails.fundamentals.splitsDividends.dividendYield)}</p>
                                      </div>
                                      <div>
                                        <span className="text-slate-400">Dividends Per Year:</span>
                                        <p className="text-white">{stockDetails.fundamentals.splitsDividends.numberDividendsByYear || 'N/A'}</p>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              ) : (
                                <p className="text-slate-400 italic">No splits & dividends data available</p>
                              )}
                            </Tab.Panel>

                            {/* Earnings Tab */}
                            <Tab.Panel>
                              <div className="text-center py-8">
                                <p className="text-slate-400 italic">Earnings data visualization coming soon...</p>
                                <p className="text-slate-500 text-sm mt-2">This will include earnings history, trends, and estimates</p>
                              </div>
                            </Tab.Panel>

                            {/* Financials Tab */}
                            <Tab.Panel>
                              <div className="text-center py-8">
                                <p className="text-slate-400 italic">Financial statements coming soon...</p>
                                <p className="text-slate-500 text-sm mt-2">This will include balance sheet, income statement, and cash flow data</p>
                              </div>
                            </Tab.Panel>

                            {/* Analysis Tab - User Notes and Key-Values */}
                            <Tab.Panel>
                              <StockAnalysisTab 
                                ticker={stockDetails.symbol.ticker} 
                                exchangeCode={stockDetails.symbol.fullSymbol.split('.')[1] || 'US'} 
                              />
                            </Tab.Panel>
                          </Tab.Panels>
                        </Tab.Group>
                      ) : (
                        <p className="text-slate-400 italic">No fundamental data available</p>
                      )}
                    </div>

                    {/* Footer with timestamps */}
                    <div className="text-xs text-slate-500 border-t border-slate-600 pt-4">
                      <p>Requested at: {formatDate(stockDetails.requestedAt)}</p>
                    </div>
                  </div>
                )}

                <div className="mt-6 flex justify-end">
                  <button
                    type="button"
                    className="px-4 py-2 bg-slate-600 hover:bg-slate-500 text-white rounded-lg font-medium transition-colors"
                    onClick={onClose}
                  >
                    Close
                  </button>
                </div>
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </div>
      </Dialog>
    </Transition>
  )
}
