using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of ISymbolRepository.
/// </summary>
public sealed class SymbolRepository : ISymbolRepository
{
    private readonly VolurDbContext _context;
    private readonly ILogger<SymbolRepository> _logger;

    public SymbolRepository(VolurDbContext context, ILogger<SymbolRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Symbol> Symbols, int TotalCount, DateTime? FetchedAt)?> GetByExchangeAsync(
        string exchangeCode,
        int page,
        int pageSize,
        string? searchQuery = null,
        string? typeFilter = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exchangeCodeUpper = exchangeCode.ToUpperInvariant();
            
            var query = _context.Symbols
                .Where(s => s.ParentExchange == exchangeCodeUpper);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchUpper = searchQuery.ToUpperInvariant();
                query = query.Where(s => 
                    s.Ticker.Contains(searchUpper) || 
                    s.Name.Contains(searchQuery));
            }

            // Apply type filter
            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                query = query.Where(s => s.Type == typeFilter);
            }

            _logger.LogInformation("Querying SQL Server for symbols with ParentExchange={ExchangeCode}, TypeFilter={TypeFilter}, SortBy={SortBy}, SearchQuery={SearchQuery}", 
                exchangeCodeUpper, typeFilter ?? "none", sortBy ?? "none", searchQuery ?? "none");

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);
            _logger.LogInformation("SQL Server query returned totalCount={TotalCount} for {ExchangeCode}", totalCount, exchangeCode);

            if (totalCount == 0)
            {
                _logger.LogWarning("No symbols found in SQL Server for {ExchangeCode} with filters", exchangeCode);
                return null;
            }

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDirection);

            // Apply pagination
            var skip = (page - 1) * pageSize;
            var entities = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("SQL Server query returned {Count} documents for page {Page} of {ExchangeCode} (totalCount={TotalCount})", 
                entities.Count, page, exchangeCode, totalCount);

            var symbols = entities.Select(e => new Symbol(
                Ticker: e.Ticker,
                ExchangeCode: e.ExchangeCode,
                ParentExchange: e.ParentExchange,
                Name: e.Name,
                Type: e.Type,
                Isin: e.Isin,
                Currency: e.Currency,
                IsActive: e.IsActive
            )).ToList();

            var fetchedAt = entities.FirstOrDefault()?.UpdatedAt;

            _logger.LogInformation("Successfully loaded {Count} symbols from SQL Server for {ExchangeCode}, fetchedAt={FetchedAt}", 
                symbols.Count, exchangeCode, fetchedAt);

            return (symbols, totalCount, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols for {ExchangeCode} from SQL Server", exchangeCode);
            return null;
        }
    }

    public async Task UpsertManyAsync(string exchangeCode, IReadOnlyList<Symbol> symbols, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var symbol in symbols)
            {
                var fullSymbol = symbol.FullSymbol;
                var existing = await _context.Symbols
                    .FirstOrDefaultAsync(s => s.FullSymbol == fullSymbol, cancellationToken);

                if (existing != null)
                {
                    // Update existing
                    existing.Ticker = symbol.Ticker;
                    existing.ExchangeCode = symbol.ExchangeCode;
                    existing.ParentExchange = symbol.ParentExchange;
                    existing.Name = symbol.Name;
                    existing.Type = symbol.Type;
                    existing.Isin = symbol.Isin;
                    existing.Currency = symbol.Currency;
                    existing.IsActive = symbol.IsActive;
                    // UpdatedAt will be set by interceptor
                }
                else
                {
                    // Insert new
                    var entity = new SymbolEntity
                    {
                        Ticker = symbol.Ticker,
                        ExchangeCode = symbol.ExchangeCode,
                        ParentExchange = symbol.ParentExchange,
                        FullSymbol = symbol.FullSymbol,
                        Name = symbol.Name,
                        Type = symbol.Type,
                        Isin = symbol.Isin,
                        Currency = symbol.Currency,
                        IsActive = symbol.IsActive
                    };
                    _context.Symbols.Add(entity);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully upserted {Count} symbols for {ExchangeCode}", symbols.Count, exchangeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert symbols for {ExchangeCode} in SQL Server", exchangeCode);
            throw;
        }
    }

    public async Task DeleteByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Symbols
                .Where(s => s.ParentExchange == exchangeCode)
                .ToListAsync(cancellationToken);

            foreach (var entity in entities)
            {
                entity.SoftDelete();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Soft deleted {Count} symbols for {ExchangeCode}", entities.Count, exchangeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete symbols for {ExchangeCode} from SQL Server", exchangeCode);
            throw;
        }
    }

    public async Task<Symbol?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Symbols
                .FirstOrDefaultAsync(s => s.Ticker == ticker.ToUpperInvariant(), cancellationToken);

            if (entity == null)
            {
                _logger.LogDebug("Symbol not found for ticker: {Ticker}", ticker);
                return null;
            }

            var symbol = new Symbol(
                Ticker: entity.Ticker,
                ExchangeCode: entity.ExchangeCode,
                ParentExchange: entity.ParentExchange,
                Name: entity.Name,
                Type: entity.Type,
                Isin: entity.Isin,
                Currency: entity.Currency,
                IsActive: entity.IsActive
            );

            _logger.LogDebug("Found symbol for ticker: {Ticker} on exchange {Exchange}", ticker, symbol.ExchangeCode);

            return symbol;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbol by ticker: {Ticker}", ticker);
            return null;
        }
    }

    public async Task<(IReadOnlyList<Symbol> symbols, int totalCount, DateTime? fetchedAt)?> GetAllByExchangeAsync(
        string exchangeCode, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Symbols
                .Where(s => s.ParentExchange == exchangeCode);

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                _logger.LogDebug("No symbols found for exchange: {ExchangeCode}", exchangeCode);
                return null;
            }

            var entities = await query
                .OrderBy(s => s.Ticker)
                .ToListAsync(cancellationToken);

            var symbols = entities.Select(e => new Symbol(
                Ticker: e.Ticker,
                ExchangeCode: e.ExchangeCode,
                ParentExchange: e.ParentExchange,
                Name: e.Name,
                Type: e.Type,
                Isin: e.Isin,
                Currency: e.Currency,
                IsActive: e.IsActive
            )).ToList();

            var fetchedAt = entities.FirstOrDefault()?.UpdatedAt;

            _logger.LogDebug("Retrieved {Count} symbols for exchange: {ExchangeCode}", symbols.Count, exchangeCode);

            return (symbols, totalCount, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all symbols for exchange: {ExchangeCode}", exchangeCode);
            return null;
        }
    }

    public async Task UpdateDenormalizedFieldsAsync(
        string ticker,
        double? trailingPE = null,
        double? marketCap = null,
        double? currentPrice = null,
        double? changePercent = null,
        double? dividendYield = null,
        string? sector = null,
        string? industry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Symbols
                .FirstOrDefaultAsync(s => s.Ticker == ticker.ToUpperInvariant(), cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Symbol not found for ticker: {Ticker}", ticker);
                return;
            }

            // Update only if value is provided
            if (trailingPE.HasValue)
                entity.TrailingPE = trailingPE.Value;
            if (marketCap.HasValue)
                entity.MarketCap = marketCap.Value;
            if (currentPrice.HasValue)
                entity.CurrentPrice = currentPrice.Value;
            if (changePercent.HasValue)
                entity.ChangePercent = changePercent.Value;
            if (dividendYield.HasValue)
                entity.DividendYield = dividendYield.Value;
            if (sector != null)
                entity.Sector = sector;
            if (industry != null)
                entity.Industry = industry;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Updated denormalized fields for {Ticker}", ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update denormalized fields for {Ticker}", ticker);
            // Don't throw - this is a best-effort optimization
        }
    }

    private IQueryable<SymbolEntity> ApplySorting(IQueryable<SymbolEntity> query, string? sortBy, string? sortDirection)
    {
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderBy(s => s.Ticker);
        }

        return sortBy.ToLowerInvariant() switch
        {
            "ticker" or "symbol" => isDescending 
                ? query.OrderByDescending(s => s.Ticker)
                : query.OrderBy(s => s.Ticker),
            "name" => isDescending 
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name),
            "type" => isDescending 
                ? query.OrderByDescending(s => s.Type)
                : query.OrderBy(s => s.Type),
            "currency" => isDescending 
                ? query.OrderByDescending(s => s.Currency)
                : query.OrderBy(s => s.Currency),
            "isactive" => isDescending 
                ? query.OrderByDescending(s => s.IsActive)
                : query.OrderBy(s => s.IsActive),
            "price" => isDescending 
                ? query.OrderByDescending(s => s.CurrentPrice).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.CurrentPrice ?? double.MaxValue).ThenBy(s => s.Ticker),
            "marketcap" => isDescending 
                ? query.OrderByDescending(s => s.MarketCap).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.MarketCap ?? double.MaxValue).ThenBy(s => s.Ticker),
            "pe" => isDescending 
                ? query.OrderByDescending(s => s.TrailingPE).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.TrailingPE ?? double.MaxValue).ThenBy(s => s.Ticker),
            "dividend" => isDescending 
                ? query.OrderByDescending(s => s.DividendYield).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.DividendYield ?? double.MaxValue).ThenBy(s => s.Ticker),
            "change" => isDescending 
                ? query.OrderByDescending(s => s.ChangePercent).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.ChangePercent ?? double.MaxValue).ThenBy(s => s.Ticker),
            "sector" => isDescending 
                ? query.OrderByDescending(s => s.Sector).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.Sector ?? "").ThenBy(s => s.Ticker),
            "industry" => isDescending 
                ? query.OrderByDescending(s => s.Industry).ThenBy(s => s.Ticker)
                : query.OrderBy(s => s.Industry ?? "").ThenBy(s => s.Ticker),
            _ => query.OrderBy(s => s.Ticker)
        };
    }
}
