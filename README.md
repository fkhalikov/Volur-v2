# Volur - Market Data Platform

A modern, scalable financial market data platform built with .NET 8, React, TypeScript, and MongoDB. Volur fetches and caches exchange and symbol data from EODHD API with intelligent TTL-based caching.

## 🏗️ Architecture

### Backend (.NET 8)
- **Clean Architecture** with clear separation of concerns
- **Domain Layer**: Core business entities and logic
- **Application Layer**: Use cases, DTOs, and business rules
- **Infrastructure Layer**: Data access, external APIs, caching
- **API Layer**: RESTful endpoints with OpenAPI/Swagger

### Frontend (React + TypeScript)
- **Vite** for fast development and optimized builds
- **TanStack Query** for intelligent data fetching and caching
- **React Router** for client-side routing
- **Tailwind CSS** for modern, responsive UI

### Data Layer
- **MongoDB** with TTL-based caching
- Automatic index creation on startup
- Optimized queries with pagination and search

## 📋 Features

- ✅ List all financial exchanges worldwide
- ✅ Browse symbols by exchange with pagination
- ✅ Server-side search with debouncing
- ✅ Intelligent caching with configurable TTL
- ✅ Resilient HTTP client with Polly (retry, circuit breaker)
- ✅ Health checks and readiness probes
- ✅ Structured logging with Serilog
- ✅ OpenAPI/Swagger documentation
- ✅ CORS configuration
- ✅ Docker support with Docker Compose
- ✅ Comprehensive test coverage

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [MongoDB](https://www.mongodb.com/try/download/community) (or use Docker)
- [EODHD API Token](https://eodhd.com/) (free tier available)

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Volur-v2
   ```

2. **Set up environment variables**
   ```bash
   cd deploy
   cp .env.example .env
   # Edit .env and add your EODHD_API_TOKEN
   ```

3. **Run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

4. **Access the application**
   - Web UI: http://localhost:3000
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger

### Option 2: Local Development

#### Backend Setup

1. **Configure API settings**
   ```bash
   cd src/Volur.Api
   cp appsettings.Development.json.example appsettings.Development.json
   # Edit appsettings.Development.json and add your EODHD API token
   ```

   Or use .NET User Secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Eodhd:ApiToken" "YOUR_TOKEN_HERE"
   ```

2. **Start MongoDB** (if not using Docker)
   ```bash
   mongod --dbpath /path/to/data
   ```

3. **Run the API**
   ```bash
   dotnet restore
   dotnet run --project src/Volur.Api
   ```

   The API will be available at http://localhost:5000

#### Frontend Setup

1. **Install dependencies**
   ```bash
   cd web/volur-web
   npm install
   ```

2. **Run the development server**
   ```bash
   npm run dev
   ```

   The web app will be available at http://localhost:5173

## 🔧 Configuration

### API Configuration (appsettings.json)

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "volur"
  },
  "Eodhd": {
    "ApiToken": "YOUR_TOKEN",
    "BaseUrl": "https://eodhd.com/",
    "TimeoutSeconds": 10
  },
  "CacheTtl": {
    "ExchangesHours": 24,
    "SymbolsHours": 24
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `EODHD_API_TOKEN` | EODHD API token (required) | - |
| `Mongo__ConnectionString` | MongoDB connection string | `mongodb://localhost:27017` |
| `Mongo__Database` | Database name | `volur` |
| `CacheTtl__ExchangesHours` | Exchange cache TTL in hours | `24` |
| `CacheTtl__SymbolsHours` | Symbol cache TTL in hours | `24` |

## 📚 API Documentation

### Endpoints

#### `GET /api/exchanges`
Get all exchanges with optional force refresh.

**Query Parameters:**
- `forceRefresh` (bool, optional): Bypass cache and fetch fresh data

**Response:**
```json
{
  "count": 92,
  "items": [
    {
      "code": "US",
      "name": "US Stocks",
      "country": "United States",
      "currency": "USD",
      "operatingMic": "XNYS"
    }
  ],
  "fetchedAt": "2025-10-13T17:30:12Z",
  "cache": {
    "source": "mongo",
    "ttlSeconds": 86340
  }
}
```

#### `GET /api/exchanges/{code}/symbols`
Get symbols for a specific exchange with pagination and search.

**Path Parameters:**
- `code` (string, required): Exchange code

**Query Parameters:**
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 50, max: 500): Items per page
- `q` (string, optional): Search query (ticker or name)
- `type` (string, optional): Filter by symbol type
- `forceRefresh` (bool, optional): Bypass cache

**Response:**
```json
{
  "exchange": {
    "code": "LSE",
    "name": "London Stock Exchange",
    "country": "United Kingdom",
    "currency": "GBP"
  },
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "total": 4231,
    "hasNext": true
  },
  "items": [
    {
      "ticker": "BP.L",
      "name": "BP p.l.c.",
      "type": "Common Stock",
      "currency": "GBP",
      "isin": "GB0007980591",
      "isActive": true
    }
  ],
  "fetchedAt": "2025-10-13T17:33:00Z",
  "cache": {
    "source": "mongo",
    "ttlSeconds": 86340
  }
}
```

#### `POST /api/exchanges/{code}/symbols/refresh`
Force refresh symbols for a specific exchange (admin endpoint).

**Response:** `204 No Content`

#### `GET /api/health`
Health check endpoint (liveness probe).

#### `GET /api/ready`
Readiness check endpoint with dependency status.

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test tests/Volur.UnitTests
```

### Run Integration Tests
```bash
dotnet test tests/Volur.IntegrationTests
```

### Run All Tests
```bash
dotnet test
```

## 🏗️ Project Structure

```
Volur-v2/
├── src/
│   ├── Volur.Domain/           # Domain entities and business logic
│   ├── Volur.Application/      # Use cases, DTOs, interfaces
│   ├── Volur.Infrastructure/   # Data access, external APIs
│   ├── Volur.Shared/          # Shared utilities (Result, Error)
│   └── Volur.Api/             # API controllers and configuration
├── web/
│   └── volur-web/             # React + TypeScript frontend
├── tests/
│   ├── Volur.UnitTests/       # Unit tests
│   └── Volur.IntegrationTests/ # Integration tests
├── deploy/
│   ├── docker-compose.yml     # Docker Compose configuration
│   ├── Dockerfile.api         # API Dockerfile
│   └── Dockerfile.web         # Web Dockerfile
└── README.md
```

## 🔒 Security

- ✅ API token stored server-side only (never exposed to browser)
- ✅ CORS configured with allowed origins
- ✅ Input validation with FluentValidation
- ✅ Rate limiting awareness (429 handling)
- ✅ Secure headers in nginx
- ✅ No sensitive data in logs

## 🚢 Deployment

### Docker Deployment

1. Build images:
   ```bash
   cd deploy
   docker-compose build
   ```

2. Run services:
   ```bash
   docker-compose up -d
   ```

3. View logs:
   ```bash
   docker-compose logs -f
   ```

4. Stop services:
   ```bash
   docker-compose down
   ```

### Production Considerations

- Use a secrets manager for API tokens (Azure Key Vault, AWS Secrets Manager)
- Set up proper MongoDB backups
- Configure application insights/monitoring (Prometheus, Grafana)
- Use a reverse proxy (nginx, Traefik) for SSL termination
- Implement rate limiting on the API gateway
- Set up CI/CD pipeline (GitHub Actions example included)

## 📊 Monitoring

### Health Checks

- **Liveness:** `GET /api/health` - Returns 200 if process is running
- **Readiness:** `GET /api/ready` - Checks MongoDB connectivity and upstream availability

### Logging

Structured logging with Serilog:
- Console output (JSON in production)
- Correlation with `traceId` per request
- Request/response logging
- Provider latency tracking
- Cache hit/miss metrics

## 🔄 CI/CD

Example GitHub Actions workflow (create `.github/workflows/ci.yml`):

```yaml
name: CI/CD

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal

  build:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker images
        run: |
          cd deploy
          docker-compose build
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Market data provided by [EODHD](https://eodhd.com/)
- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- UI powered by [React](https://react.dev/) and [Tailwind CSS](https://tailwindcss.com/)

## 📧 Support

For issues and questions:
- Open an issue on GitHub
- Check the [API documentation](http://localhost:5000/swagger) when running locally

---

**Built with ❤️ following SOLID principles and clean architecture**

