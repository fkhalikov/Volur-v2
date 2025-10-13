# Changelog

All notable changes to the Volur project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-13

### Added - MVP Release

#### Backend (.NET 8)
- **Clean Architecture** implementation with Domain, Application, Infrastructure, and API layers
- **Exchange Endpoints**
  - `GET /api/exchanges` - List all exchanges with optional force refresh
  - `GET /api/exchanges/{code}/symbols` - Get symbols by exchange with pagination and search
  - `POST /api/exchanges/{code}/symbols/refresh` - Force refresh symbols for an exchange
- **Health Check Endpoints**
  - `GET /api/health` - Liveness probe
  - `GET /api/ready` - Readiness probe with dependency checks
- **MongoDB Integration**
  - TTL-based caching with automatic index creation
  - Optimized queries with pagination and text search
  - Compound indexes for performance
- **EODHD Provider Client**
  - Resilient HTTP client with Polly (retry, circuit breaker, timeout)
  - Rate limit handling (429 responses)
  - Structured error handling
- **Logging & Observability**
  - Structured logging with Serilog
  - Request/response logging with correlation IDs
  - Cache hit/miss tracking
  - Provider latency metrics
- **Validation & Error Handling**
  - FluentValidation for request validation
  - Global exception handling middleware
  - Consistent error response format
- **API Documentation**
  - OpenAPI/Swagger integration
  - XML documentation comments

#### Frontend (React + TypeScript)
- **Modern Stack**
  - React 18 with TypeScript
  - Vite for fast development and optimized builds
  - TanStack Query for intelligent data fetching
  - React Router for client-side routing
  - Tailwind CSS for responsive UI
- **Pages**
  - Exchanges list page with client-side search
  - Symbols page with server-side pagination and search
  - Responsive design for mobile and desktop
- **Features**
  - Debounced search (300ms)
  - Loading and error states
  - Empty state handling
  - Copy-to-clipboard for tickers
  - Cache metadata display
- **Developer Experience**
  - TypeScript type safety
  - ESLint for code quality
  - Hot module replacement (HMR)

#### Infrastructure
- **Docker Support**
  - Multi-stage Docker builds for API and Web
  - Docker Compose configuration
  - MongoDB container with health checks
  - Nginx configuration for production
- **Testing**
  - Unit tests with xUnit and FluentAssertions
  - Integration tests with WebApplicationFactory
  - Test coverage for mappers and validators
- **CI/CD**
  - GitHub Actions workflow for CI
  - Automated testing on push/PR
  - Docker image building

#### Documentation
- Comprehensive README with architecture overview
- DEVELOPMENT.md with detailed development guide
- QUICKSTART.md for quick setup
- Setup scripts for Windows (PowerShell) and Unix (Bash)
- API documentation via Swagger

### Configuration Options
- Configurable cache TTL (exchanges and symbols)
- CORS configuration
- MongoDB connection settings
- EODHD API configuration
- Environment-based settings

### Security
- API token stored server-side only
- CORS protection
- Input validation
- Secure headers in nginx
- No sensitive data in logs

## [Unreleased]

### Planned Features
- [ ] Background scheduled refresh with Quartz.NET
- [ ] User authentication and authorization
- [ ] Symbol detail page with fundamentals/quotes
- [ ] Export to CSV functionality
- [ ] WebSocket support for real-time updates
- [ ] Internationalization (i18n)
- [ ] Advanced filtering (multiple types, date ranges)
- [ ] Favorites/watchlist functionality
- [ ] Historical data charting
- [ ] Audit trail for admin actions
- [ ] Metrics endpoint for Prometheus
- [ ] E2E tests with Playwright

### Known Issues
- None reported

---

For more details on any release, please refer to the [README.md](README.md) or the Git commit history.

