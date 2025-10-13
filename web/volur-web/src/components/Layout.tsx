import { Outlet, Link, useLocation } from 'react-router-dom'

export default function Layout() {
  const location = useLocation()

  return (
    <div className="min-h-screen bg-slate-900">
      {/* Header */}
      <header className="bg-slate-800 shadow-lg border-b border-slate-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-8">
              <Link to="/" className="text-2xl font-bold text-blue-400">
                Volur
              </Link>
              <nav className="flex space-x-4">
                <Link
                  to="/exchanges"
                  className={`px-3 py-2 rounded-md text-sm font-medium ${
                    location.pathname === '/exchanges' || location.pathname === '/'
                      ? 'bg-blue-600 text-white'
                      : 'text-slate-300 hover:bg-slate-700'
                  }`}
                >
                  Exchanges
                </Link>
              </nav>
            </div>
            <div className="text-sm text-slate-400">
              Market Data Platform
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Outlet />
      </main>

      {/* Footer */}
      <footer className="bg-slate-800 border-t border-slate-700 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <p className="text-center text-sm text-slate-400">
            &copy; 2025 Volur. Market data provided by EODHD.
          </p>
        </div>
      </footer>
    </div>
  )
}

