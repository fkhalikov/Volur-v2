using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence.Mappers;
using Volur.Infrastructure.Persistence.Models;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of ISymbolRepository.
/// </summary>
public sealed class SymbolRepository : ISymbolRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<SymbolRepository> _logger;

    public SymbolRepository(MongoDbContext context, ILogger<SymbolRepository> logger)
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
            var filterBuilder = Builders<SymbolDocument>.Filter;
            var filter = filterBuilder.Eq(x => x.ParentExchange, exchangeCode);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Eq(x => x.Ticker, searchQuery.ToUpperInvariant()),
                    filterBuilder.Regex(x => x.Ticker, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                    filterBuilder.Text(searchQuery)
                );
                filter = filterBuilder.And(filter, searchFilter);
            }

            // Apply type filter
            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Type, typeFilter));
            }

            // Get total count
            var totalCount = await _context.Symbols.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            if (totalCount == 0)
                return null;

            // Build sort definition
            var sortDefinition = BuildSortDefinition(sortBy, sortDirection);

            // Get paged results
            var skip = (page - 1) * pageSize;
            var documents = await _context.Symbols
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            if (documents.Count == 0)
                return (Array.Empty<Symbol>(), (int)totalCount, DateTime.UtcNow);

            var symbols = documents.Select(d => d.ToDomain()).ToList();
            var fetchedAt = documents.FirstOrDefault()?.FetchedAt;

            return (symbols, (int)totalCount, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols for {ExchangeCode} from MongoDB", exchangeCode);
            return null;
        }
    }

    public async Task UpsertManyAsync(string exchangeCode, IReadOnlyList<Symbol> symbols, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var expiresAt = fetchedAt.Add(ttl);
            
            var writes = symbols.Select(s =>
            {
                var doc = s.ToDocument(fetchedAt, expiresAt);
                var filter = Builders<SymbolDocument>.Filter.And(
                    Builders<SymbolDocument>.Filter.Eq(x => x.Ticker, s.Ticker),
                    Builders<SymbolDocument>.Filter.Eq(x => x.ExchangeCode, s.ExchangeCode)
                );
                
                var update = Builders<SymbolDocument>.Update
                    .Set(x => x.Ticker, doc.Ticker)
                    .Set(x => x.ExchangeCode, doc.ExchangeCode)
                    .Set(x => x.ParentExchange, doc.ParentExchange)
                    .Set(x => x.FullSymbol, doc.FullSymbol)
                    .Set(x => x.Name, doc.Name)
                    .Set(x => x.Type, doc.Type)
                    .Set(x => x.Isin, doc.Isin)
                    .Set(x => x.Currency, doc.Currency)
                    .Set(x => x.IsActive, doc.IsActive)
                    .Set(x => x.FetchedAt, doc.FetchedAt)
                    .Set(x => x.ExpiresAt, doc.ExpiresAt);
                
                return new UpdateOneModel<SymbolDocument>(filter, update)
                {
                    IsUpsert = true
                };
            }).ToList();

            if (writes.Count > 0)
            {
                // Process in batches to avoid MongoDB timeouts on large datasets
                const int batchSize = 1000;
                var totalProcessed = 0;
                
                for (int i = 0; i < writes.Count; i += batchSize)
                {
                    var batch = writes.Skip(i).Take(batchSize).ToList();
                    await _context.Symbols.BulkWriteAsync(batch, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
                    totalProcessed += batch.Count;
                    _logger.LogDebug("Upserted batch {Batch}/{Total} ({Count} symbols) for {ExchangeCode}", 
                        (i / batchSize) + 1, (writes.Count + batchSize - 1) / batchSize, batch.Count, exchangeCode);
                }
                
                _logger.LogInformation("Successfully upserted {Count} symbols for {ExchangeCode}", totalProcessed, exchangeCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert symbols for {ExchangeCode} in MongoDB", exchangeCode);
            throw;
        }
    }

    public async Task DeleteByExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _context.Symbols.DeleteManyAsync(
                x => x.ParentExchange == exchangeCode,
                cancellationToken);

            _logger.LogDebug("Deleted {Count} symbols for {ExchangeCode}", result.DeletedCount, exchangeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete symbols for {ExchangeCode} from MongoDB", exchangeCode);
            throw;
        }
    }

    public async Task<Symbol?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<SymbolDocument>.Filter.Eq(x => x.Ticker, ticker.ToUpperInvariant());
            var document = await _context.Symbols.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (document == null)
            {
                _logger.LogDebug("Symbol not found for ticker: {Ticker}", ticker);
                return null;
            }

            var symbol = document.ToDomain();
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
            var filter = Builders<SymbolDocument>.Filter.Eq(x => x.ParentExchange, exchangeCode);
            
            // Get total count
            var totalCount = await _context.Symbols.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            
            if (totalCount == 0)
            {
                _logger.LogDebug("No symbols found for exchange: {ExchangeCode}", exchangeCode);
                return null;
            }

            // Get all documents without pagination
            var documents = await _context.Symbols
                .Find(filter)
                .SortBy(x => x.Ticker)
                .ToListAsync(cancellationToken);

            if (!documents.Any())
            {
                _logger.LogDebug("No symbols found for exchange: {ExchangeCode}", exchangeCode);
                return null;
            }

            var symbols = documents.Select(d => d.ToDomain()).ToList();
            var fetchedAt = documents.FirstOrDefault()?.FetchedAt;

            _logger.LogDebug("Retrieved {Count} symbols for exchange: {ExchangeCode}", symbols.Count, exchangeCode);
            
            return (symbols, (int)totalCount, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all symbols for exchange: {ExchangeCode}", exchangeCode);
            return null;
        }
    }

    private SortDefinition<SymbolDocument> BuildSortDefinition(string? sortBy, string? sortDirection)
    {
        var sortBuilder = Builders<SymbolDocument>.Sort;
        var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        // Default sorting by ticker if no sort field specified
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return sortBuilder.Ascending(x => x.Ticker);
        }

        // Map frontend sort fields to MongoDB document fields
        // Note: Some fields like price, marketcap, pe, etc. are enriched data and will be sorted client-side
        return sortBy.ToLowerInvariant() switch
        {
            "ticker" or "symbol" => isDescending ? sortBuilder.Descending(x => x.Ticker) : sortBuilder.Ascending(x => x.Ticker),
            "name" => isDescending ? sortBuilder.Descending(x => x.Name) : sortBuilder.Ascending(x => x.Name),
            "type" => isDescending ? sortBuilder.Descending(x => x.Type) : sortBuilder.Ascending(x => x.Type),
            "currency" => isDescending ? sortBuilder.Descending(x => x.Currency) : sortBuilder.Ascending(x => x.Currency),
            "isactive" => isDescending ? sortBuilder.Descending(x => x.IsActive) : sortBuilder.Ascending(x => x.IsActive),
            // Sector and Industry are enriched fields, not stored in database - handle client-side
            // For enriched fields (price, marketcap, pe, dividend, change), we'll sort by ticker and handle client-side
            "price" or "marketcap" or "pe" or "dividend" or "change" => sortBuilder.Ascending(x => x.Ticker),
            _ => sortBuilder.Ascending(x => x.Ticker) // Default fallback
        };
    }
}

