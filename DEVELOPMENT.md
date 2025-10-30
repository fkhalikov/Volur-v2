# Development Guide

This guide covers everything you need to know to develop and contribute to Volur.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [MongoDB 7.0+](https://www.mongodb.com/try/download/community)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- IDE: Visual Studio 2022, VS Code, or Rider

## Getting Started

### 1. Clone and Setup

```bash
git clone <repository-url>
cd Volur-v2
```

### 2. Backend Setup

#### Install dependencies
```bash
dotnet restore
```

#### Configure User Secrets (Recommended for Development)
```bash
cd src/Volur.Api
dotnet user-secrets init
dotnet user-secrets set "Eodhd:ApiToken" "YOUR_EODHD_API_TOKEN"
dotnet user-secrets set "Mongo:ConnectionString" "mongodb://localhost:27017"
dotnet user-secrets set "Mongo:Database" "volur_dev"
```

#### Or use appsettings.Development.json
```bash
cp appsettings.Development.json.example appsettings.Development.json
# Edit the file and add your API token
```

#### Run MongoDB locally
```bash
# Using Docker
docker run -d -p 27017:27017 --name volur-mongo mongo:7.0

# Or install MongoDB locally and run
mongod --dbpath /path/to/data
```

#### Run the API
```bash
cd src/Volur.Api
dotnet run
```

API will be available at:
- HTTP: http://localhost:5000
- Swagger: http://localhost:5000/swagger

### 3. Frontend Setup

```bash
cd web/volur-web
npm install
npm run dev
```

Frontend will be available at http://localhost:5173

## Project Structure

```
Volur-v2/
├── src/
│   ├── Volur.Domain/           # Core domain entities
│   │   └── Entities/           # Domain models (Exchange, Symbol)
│   ├── Volur.Shared/          # Shared utilities
│   │   ├── Result.cs          # Result pattern implementation
│   │   └── Error.cs           # Error types
│   ├── Volur.Application/     # Application logic
│   │   ├── UseCases/          # Business use cases
│   │   ├── DTOs/              # Data transfer objects
│   │   ├── Interfaces/        # Repository and service interfaces
│   │   ├── Mappers/           # Entity to DTO mappers
│   │   └── Validators/        # FluentValidation validators
│   ├── Volur.Infrastructure/  # External concerns
│   │   ├── Persistence/       # MongoDB repositories
│   │   │   ├── Models/        # MongoDB documents
│   │   │   └── Repositories/  # Repository implementations
│   │   └── ExternalProviders/ # EODHD HTTP client
│   └── Volur.Api/            # Web API
│       ├── Controllers/       # API controllers
│       ├── Middleware/        # Custom middleware
│       └── Program.cs         # Application entry point
├── web/volur-web/            # React frontend
│   ├── src/
│   │   ├── api/              # API client
│   │   ├── components/       # Reusable components
│   │   ├── pages/           # Page components
│   │   ├── hooks/           # Custom React hooks
│   │   └── types/           # TypeScript types
└── tests/
    ├── Volur.UnitTests/      # Unit tests
    └── Volur.IntegrationTests/ # Integration tests
```

## Development Workflow

### Backend Development

#### Run with watch mode
```bash
dotnet watch --project src/Volur.Api
```

#### Run tests
```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/Volur.UnitTests

# Integration tests only
dotnet test tests/Volur.IntegrationTests

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

#### Code formatting
```bash
dotnet format
```

#### Add a new migration (if using EF Core in future)
```bash
dotnet ef migrations add <MigrationName> --project src/Volur.Infrastructure --startup-project src/Volur.Api
```

### Frontend Development

#### Run dev server
```bash
cd web/volur-web
npm run dev
```

#### Lint code
```bash
npm run lint
```

#### Build for production
```bash
npm run build
```

#### Preview production build
```bash
npm run preview
```

## Architecture Patterns

### Clean Architecture Layers

1. **Domain Layer** (innermost)
   - No dependencies on other layers
   - Contains core business entities
   - Pure C# with no framework dependencies

2. **Application Layer**
   - Depends on Domain and Shared
   - Contains use cases and business logic
   - Defines interfaces for external dependencies

3. **Infrastructure Layer**
   - Implements Application interfaces
   - Handles data persistence, external APIs, etc.
   - Contains framework-specific code

4. **API Layer** (outermost)
   - Depends on Application and Infrastructure
   - HTTP concerns only
   - Controllers, middleware, configuration

### Key Patterns

#### Result Pattern
```csharp
// Instead of throwing exceptions
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _repository.GetAsync(id);
    if (user == null)
        return Result.Failure<User>(Error.NotFound("User", id.ToString()));
    
    return Result.Success(user);
}

// Usage
var result = await _handler.GetUserAsync(1);
if (result.IsFailure)
{
    // Handle error
    return BadRequest(result.Error);
}
return Ok(result.Value);
```

#### Repository Pattern
```csharp
public interface IExchangeRepository
{
    Task<Exchange?> GetByCodeAsync(string code, CancellationToken ct);
    Task UpsertManyAsync(IReadOnlyList<Exchange> exchanges, DateTime fetchedAt, TimeSpan ttl, CancellationToken ct);
}
```

#### Use Case Handler Pattern
```csharp
public class GetExchangesHandler
{
    public async Task<Result<ExchangesResponse>> HandleAsync(
        GetExchangesQuery query, 
        CancellationToken ct)
    {
        // Implementation
    }
}
```

## Coding Standards

### C# Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **records** for DTOs and immutable entities
- Use **nullable reference types** (enabled by default)
- Prefer **async/await** for I/O operations
- Use **dependency injection** for all services
- Keep methods **small and focused** (SRP)

### TypeScript Guidelines

- Use **strict mode** (enabled in tsconfig.json)
- Prefer **functional components** with hooks
- Use **TypeScript interfaces** for all props
- Avoid `any` type
- Use **async/await** over promises

### Testing Guidelines

- Follow **AAA pattern** (Arrange, Act, Assert)
- Use **descriptive test names**
- One assertion per test (when possible)
- Mock external dependencies
- Test edge cases and error scenarios

Example:
```csharp
[Fact]
public async Task GetExchanges_WhenCacheExpired_ShouldFetchFromProvider()
{
    // Arrange
    var expiredDate = DateTime.UtcNow.AddHours(-25);
    _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync((new List<Exchange>(), expiredDate));
    
    // Act
    var result = await _handler.HandleAsync(new GetExchangesQuery(), CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    _eodhdClient.Verify(c => c.GetExchangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

## Debugging

### Backend Debugging

#### Visual Studio / Rider
1. Set `Volur.Api` as startup project
2. Press F5 or click Debug

#### VS Code
1. Install C# extension
2. Open the project folder
3. Press F5 (launch.json is configured)

#### Debug logs
Set log level in appsettings.Development.json:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### Reading Log Files for Backend Issues

The backend writes log files automatically when running. This is essential for debugging MongoDB issues, API errors, and data loading problems.

#### Log File Location

**Development/Local:**
- Path: `src/Volur.Api/bin/Debug/net8.0/logs/volur-YYYYMMDD.log`
- Format: One file per day (rolling daily)
- Example: `volur-20251029.log` for October 29, 2025

**Production/Docker:**
- Path: `logs/volur-YYYYMMDD.log` (inside container)
- Access via: `docker exec -it volur-api cat logs/volur-$(date +%Y%m%d).log`

#### How to Read Logs

**Windows (PowerShell):**
```powershell
# Read the most recent log file
Get-Content "src\Volur.Api\bin\Debug\net8.0\logs\volur-*.log" -Tail 100

# Follow log file in real-time (like tail -f)
Get-Content "src\Volur.Api\bin\Debug\net8.0\logs\volur-*.log" -Wait -Tail 50

# Search for errors
Select-String -Path "src\Volur.Api\bin\Debug\net8.0\logs\volur-*.log" -Pattern "ERR"

# Search for MongoDB errors specifically
Select-String -Path "src\Volur.Api\bin\Debug\net8.0\logs\volur-*.log" -Pattern "MongoDB|Failed to get"
```

**Linux/Mac:**
```bash
# Read the most recent log file
tail -100 src/Volur.Api/bin/Debug/net8.0/logs/volur-*.log

# Follow log file in real-time
tail -f src/Volur.Api/bin/Debug/net8.0/logs/volur-*.log

# Search for errors
grep -i "ERR" src/Volur.Api/bin/Debug/net8.0/logs/volur-*.log

# Search for MongoDB errors
grep -i "MongoDB\|Failed to get" src/Volur.Api/bin/Debug/net8.0/logs/volur-*.log
```

#### Testing Endpoints and Checking Logs

**Step 1: Test the endpoint**
```powershell
# Test LSE symbols endpoint
Invoke-RestMethod -Uri "http://localhost:5000/api/exchanges/LSE/symbols?page=1&pageSize=50" -Method Get

# Or use curl
curl http://localhost:5000/api/exchanges/LSE/symbols?page=1&pageSize=50
```

**Step 2: Check logs immediately after**
```powershell
# Get the latest log entries
Get-Content "src\Volur.Api\bin\Debug\net8.0\logs\volur-*.log" -Tail 50
```

#### Common MongoDB Error Patterns

**1. MongoDB LINQ Expression Not Supported**
```
MongoDB.Driver.Linq.ExpressionNotSupportedException: Expression not supported: (x.Field ?? defaultValue)
```
- **Cause**: Using C# null-coalescing operator (`??`) in MongoDB LINQ expressions
- **Fix**: Remove `??` from sort/filter expressions. MongoDB handles nulls naturally:
  - Ascending: nulls sort first
  - Descending: nulls sort last

**2. Connection Issues**
```
MongoDB.Driver.MongoConnectionException: Unable to connect to server
```
- **Check**: MongoDB is running (`docker ps` or `mongosh`)
- **Check**: Connection string in `appsettings.json` is correct
- **Check**: MongoDB port 27017 is accessible

**3. No Documents Found**
```
[WRN] No symbols found in MongoDB for LSE with filters
[WRN] Raw count (no filters, ParentExchange=LSE): 0
```
- **Cause**: No data in MongoDB for that exchange
- **Action**: Check if symbols were fetched from provider
- **Fix**: Use `forceRefresh=true` to fetch from provider, or check data in MongoDB directly

**4. Query Returning 0 Despite Count > 0**
```
[INF] MongoDB query returned totalCount=6201 for LSE
[ERR] Failed to get symbols for LSE from MongoDB
```
- **Cause**: Sort expression error (like the `??` operator issue)
- **Fix**: Check the sort definition in repository, remove unsupported expressions

#### Log File Structure

Each log entry contains:
- **Timestamp**: `2025-10-29 20:16:10.166 +00:00`
- **Log Level**: `[INF]` (Information), `[WRN]` (Warning), `[ERR]` (Error), `[DBG]` (Debug)
- **Message**: Human-readable log message with structured properties

Example log entry:
```
2025-10-29 20:16:10.146 +00:00 [INF] Querying MongoDB for symbols with ParentExchange=LSE (normalized to LSE), TypeFilter=none, SortBy=pe, SearchQuery=none
2025-10-29 20:16:10.166 +00:00 [INF] MongoDB query returned totalCount=3146 for LSE
2025-10-29 20:16:10.212 +00:00 [ERR] Failed to get symbols for LSE from MongoDB
```

#### Debugging Workflow

1. **Reproduce the issue**: Make the API call that's failing
2. **Check console output**: Look for errors in the running application console
3. **Read log file**: Check the most recent log file for detailed errors
4. **Search for specific patterns**: Use grep/Select-String to find relevant errors
5. **Check MongoDB directly** (if needed):
   ```bash
   mongosh
   use volur
   db.symbols.countDocuments({ ParentExchange: "LSE" })
   db.symbols.find({ ParentExchange: "LSE" }).limit(5)
   ```

#### Debugging MongoDB Query Issues

When debugging MongoDB queries, look for these log patterns:

**Successful Query:**
```
[INF] Querying MongoDB for symbols with ParentExchange=LSE...
[INF] MongoDB query returned totalCount=6201 for LSE
[INF] MongoDB query returned 50 documents for page 1 of LSE
[INF] Successfully loaded 50 symbols from MongoDB for LSE, fetchedAt=2025-10-29T20:17:26.021Z
```

**Failed Query:**
```
[INF] Querying MongoDB for symbols with ParentExchange=LSE...
[INF] MongoDB query returned totalCount=6201 for LSE
[ERR] Failed to get symbols for LSE from MongoDB
MongoDB.Driver.Linq.ExpressionNotSupportedException: Expression not supported...
```

**No Data Found:**
```
[INF] Querying MongoDB for symbols with ParentExchange=LSE...
[INF] MongoDB query returned totalCount=0 for LSE
[WRN] No symbols found in MongoDB for LSE with filters. Checking raw count and alternative field values...
[WRN] Raw count (no filters, ParentExchange=LSE): 0
[WRN] Count by ExchangeCode field (ExchangeCode=LSE): 0
```

#### Tips for Effective Log Reading

1. **Filter by log level**: Focus on `[ERR]` and `[WRN]` entries first
2. **Look for patterns**: Same error repeating indicates a systematic issue
3. **Check timestamps**: Correlate errors with specific API calls
4. **Read full stack traces**: Error logs include full exception details for MongoDB errors
5. **Check before/after**: Look for successful operations before the error for context

### Frontend Debugging

#### Browser DevTools
- React DevTools extension
- Network tab for API calls
- Console for errors

#### VS Code
1. Install Debugger for Chrome/Edge
2. Set breakpoints
3. Press F5

## Common Tasks

### Add a new endpoint

1. **Create a Query/Command**
   ```csharp
   // In Application/UseCases/YourFeature/
   public record YourQuery(string Parameter);
   ```

2. **Create a Handler**
   ```csharp
   public class YourHandler
   {
       public async Task<Result<YourResponse>> HandleAsync(YourQuery query, CancellationToken ct)
       {
           // Implementation
       }
   }
   ```

3. **Register in DI**
   ```csharp
   // In Application/DependencyInjection.cs
   services.AddScoped<YourHandler>();
   ```

4. **Add Controller endpoint**
   ```csharp
   [HttpGet("your-route")]
   public async Task<IActionResult> YourEndpoint([FromQuery] YourQuery query)
   {
       var result = await _handler.HandleAsync(query);
       return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error);
   }
   ```

### Add a new React page

1. **Create the page component**
   ```tsx
   // In src/pages/YourPage.tsx
   export default function YourPage() {
       // Implementation
   }
   ```

2. **Add route**
   ```tsx
   // In src/App.tsx
   <Route path="your-route" element={<YourPage />} />
   ```

3. **Add API call**
   ```tsx
   // In src/api/client.ts
   async yourEndpoint(): Promise<YourResponse> {
       const response = await fetch(`${API_BASE_URL}/your-route`)
       return handleResponse<YourResponse>(response)
   }
   ```

4. **Use TanStack Query**
   ```tsx
   const { data, isLoading } = useQuery({
       queryKey: ['yourData'],
       queryFn: () => api.yourEndpoint(),
   })
   ```

## Troubleshooting

### MongoDB connection issues
```bash
# Check if MongoDB is running
mongosh

# If using Docker
docker ps | grep mongo
docker logs volur-mongo
```

### API not starting
```bash
# Check port 5000 is available
netstat -ano | findstr :5000  # Windows
lsof -i :5000                 # Mac/Linux

# Check appsettings
dotnet user-secrets list --project src/Volur.Api
```

### Frontend not connecting to API
- Check CORS settings in appsettings.json
- Verify proxy configuration in vite.config.ts
- Check browser console for errors

### Tests failing
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test

# Check MongoDB is running for integration tests
```

## Performance Tips

### Backend
- Use `AsNoTracking()` for read-only queries (if using EF Core)
- Implement pagination for large datasets
- Use `IMemoryCache` for frequently accessed data
- Profile with BenchmarkDotNet for critical paths

### Frontend
- Use React.memo() for expensive components
- Implement virtual scrolling for large lists
- Lazy load routes with React.lazy()
- Optimize bundle size with code splitting

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [React Documentation](https://react.dev)
- [MongoDB C# Driver](https://mongodb.github.io/mongo-csharp-driver/)
- [TanStack Query](https://tanstack.com/query)
- [Polly](https://github.com/App-vNext/Polly)

## Need Help?

- Check existing issues on GitHub
- Review the API documentation at /swagger
- Join our Discord/Slack channel
- Contact the team leads

