import { Outlet, Link, useLocation } from 'react-router-dom'

export default function Layout() {
  const location = useLocation()

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-8">
              <Link to="/" className="text-2xl font-bold text-blue-600">
                Volur
              </Link>
              <nav className="flex space-x-4">
                <Link
                  to="/exchanges"
                  className={`px-3 py-2 rounded-md text-sm font-medium ${
                    location.pathname === '/exchanges' || location.pathname === '/'
                      ? 'bg-blue-100 text-blue-700'
                      : 'text-gray-700 hover:bg-gray-100'
                  }`}
                >
                  Exchanges
                </Link>
              </nav>
            </div>
            <div className="text-sm text-gray-500">
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
      <footer className="bg-white border-t mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <p className="text-center text-sm text-gray-500">
            &copy; 2025 Volur. Market data provided by EODHD.
          </p>
        </div>
      </footer>
    </div>
  )
}

