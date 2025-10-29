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
            var filter = filterBuilder.Eq(x => x.ParentExchange, exchangeCode.ToUpperInvariant());

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

            var exchangeCodeUpper = exchangeCode.ToUpperInvariant();
            _logger.LogInformation("Querying MongoDB for symbols with ParentExchange={ExchangeCode} (normalized to {ExchangeCodeUpper}), TypeFilter={TypeFilter}, SortBy={SortBy}, SearchQuery={SearchQuery}", 
                exchangeCode, exchangeCodeUpper, typeFilter ?? "none", sortBy ?? "none", searchQuery ?? "none");

            // Get total count
            var totalCount = await _context.Symbols.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            _logger.LogInformation("MongoDB query returned totalCount={TotalCount} for {ExchangeCode}", totalCount, exchangeCode);
            
            if (totalCount == 0)
            {
                _logger.LogWarning("No symbols found in MongoDB for {ExchangeCode} with filters. Checking raw count and alternative field values...", exchangeCode);
                
                // Debug: Check if any symbols exist for this exchange at all
                var rawFilter = filterBuilder.Eq(x => x.ParentExchange, exchangeCodeUpper);
                var rawCount = await _context.Symbols.CountDocumentsAsync(rawFilter, cancellationToken: cancellationToken);
                _logger.LogWarning("Raw count (no filters, ParentExchange={ExchangeCodeUpper}): {RawCount}", exchangeCodeUpper, rawCount);
                
                // Debug: Check ExchangeCode field as well (case-insensitive search)
                var exchangeCodeFilter = filterBuilder.Eq(x => x.ExchangeCode, exchangeCodeUpper);
                var exchangeCodeCount = await _context.Symbols.CountDocumentsAsync(exchangeCodeFilter, cancellationToken: cancellationToken);
                _logger.LogWarning("Count by ExchangeCode field (ExchangeCode={ExchangeCodeUpper}): {ExchangeCodeCount}", exchangeCodeUpper, exchangeCodeCount);
                
                // Debug: Sample a few documents to see what ParentExchange values exist
                var sampleDocs = await _context.Symbols
                    .Find(Builders<SymbolDocument>.Filter.Empty)
                    .Limit(10)
                    .ToListAsync(cancellationToken);
                
                if (sampleDocs.Any())
                {
                    var sampleExchanges = sampleDocs
                        .Select(d => new { d.ParentExchange, d.ExchangeCode, d.Ticker })
                        .GroupBy(x => x.ParentExchange)
                        .Select(g => $"{g.Key} ({g.Count()} samples)")
                        .ToList();
                    
                    _logger.LogWarning("Sample ParentExchange values from first 10 documents in collection: {SampleExchanges}", 
                        string.Join(", ", sampleExchanges));
                    
                    // Also log individual samples for debugging
                    foreach (var doc in sampleDocs.Take(5))
                    {
                        _logger.LogWarning("Sample document - ParentExchange: {ParentExchange}, ExchangeCode: {ExchangeCode}, Ticker: {Ticker}", 
                            doc.ParentExchange, doc.ExchangeCode, doc.Ticker);
                    }
                }
                else
                {
                    _logger.LogWarning("No documents found in symbols collection at all");
                }
                
                return null;
            }

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

            _logger.LogInformation("MongoDB query returned {Count} documents for page {Page} of {ExchangeCode} (totalCount={TotalCount})", 
                documents.Count, page, exchangeCode, totalCount);

            if (documents.Count == 0)
            {
                _logger.LogWarning("MongoDB query returned 0 documents despite totalCount={TotalCount} for {ExchangeCode}", 
                    totalCount, exchangeCode);
                return (Array.Empty<Symbol>(), (int)totalCount, DateTime.UtcNow);
            }

            var symbols = documents.Select(d => d.ToDomain()).ToList();
            var fetchedAt = documents.FirstOrDefault()?.FetchedAt;

            _logger.LogInformation("Successfully loaded {Count} symbols from MongoDB for {ExchangeCode}, fetchedAt={FetchedAt}", 
                symbols.Count, exchangeCode, fetchedAt);

            // Log sample symbols for debugging
            if (symbols.Any())
            {
                var sampleSymbols = symbols.Take(3).Select(s => $"{s.Ticker} ({s.ExchangeCode})").ToList();
                _logger.LogDebug("Sample symbols loaded: {SampleSymbols}", string.Join(", ", sampleSymbols));
            }

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
            var filter = Builders<SymbolDocument>.Filter.Eq(x => x.Ticker, ticker.ToUpperInvariant());
            var updateBuilder = Builders<SymbolDocument>.Update;
            var updates = new List<UpdateDefinition<SymbolDocument>>();

            // Only update fields that are explicitly provided (not null means we want to update)
            // We can't distinguish between "not provided" and "explicitly set to null" in this signature,
            // so we'll only update if HasValue is true for nullable doubles
            if (trailingPE.HasValue)
                updates.Add(updateBuilder.Set(x => x.TrailingPE, trailingPE.Value));
            else if (trailingPE == null) // Treat as unset if explicitly null
                updates.Add(updateBuilder.Unset(x => x.TrailingPE));

            if (marketCap.HasValue)
                updates.Add(updateBuilder.Set(x => x.MarketCap, marketCap.Value));
            else if (marketCap == null)
                updates.Add(updateBuilder.Unset(x => x.MarketCap));

            if (currentPrice.HasValue)
                updates.Add(updateBuilder.Set(x => x.CurrentPrice, currentPrice.Value));
            else if (currentPrice == null)
                updates.Add(updateBuilder.Unset(x => x.CurrentPrice));

            if (changePercent.HasValue)
                updates.Add(updateBuilder.Set(x => x.ChangePercent, changePercent.Value));
            else if (changePercent == null)
                updates.Add(updateBuilder.Unset(x => x.ChangePercent));

            if (dividendYield.HasValue)
                updates.Add(updateBuilder.Set(x => x.DividendYield, dividendYield.Value));
            else if (dividendYield == null)
                updates.Add(updateBuilder.Unset(x => x.DividendYield));

            if (sector != null)
                updates.Add(updateBuilder.Set(x => x.Sector, sector));
            else if (sector == null)
                updates.Add(updateBuilder.Unset(x => x.Sector));

            if (industry != null)
                updates.Add(updateBuilder.Set(x => x.Industry, industry));
            else if (industry == null)
                updates.Add(updateBuilder.Unset(x => x.Industry));

            if (updates.Count > 0)
            {
                var combinedUpdate = updateBuilder.Combine(updates);
                await _context.Symbols.UpdateOneAsync(filter, combinedUpdate, cancellationToken: cancellationToken);
                _logger.LogDebug("Updated denormalized fields for {Ticker}", ticker);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update denormalized fields for {Ticker}", ticker);
            // Don't throw - this is a best-effort optimization
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
        // Denormalized fields (price, marketcap, pe, dividend, change, sector, industry) can now be sorted at database level
        return sortBy.ToLowerInvariant() switch
        {
            "ticker" or "symbol" => isDescending ? sortBuilder.Descending(x => x.Ticker) : sortBuilder.Ascending(x => x.Ticker),
            "name" => isDescending ? sortBuilder.Descending(x => x.Name) : sortBuilder.Ascending(x => x.Name),
            "type" => isDescending ? sortBuilder.Descending(x => x.Type) : sortBuilder.Ascending(x => x.Type),
            "currency" => isDescending ? sortBuilder.Descending(x => x.Currency) : sortBuilder.Ascending(x => x.Currency),
            "isactive" => isDescending ? sortBuilder.Descending(x => x.IsActive) : sortBuilder.Ascending(x => x.IsActive),
            // Denormalized fields - sort at database level for performance
            // MongoDB handles nulls naturally: ascending sorts nulls first, descending sorts nulls last
            // Use Combine for multi-field sorting with ticker as secondary sort
            "price" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.CurrentPrice), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.CurrentPrice), sortBuilder.Ascending(x => x.Ticker)),
            "marketcap" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.MarketCap), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.MarketCap), sortBuilder.Ascending(x => x.Ticker)),
            "pe" => isDescending 
                // For descending: nulls will sort last naturally (MongoDB behavior)
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.TrailingPE), sortBuilder.Ascending(x => x.Ticker))
                // For ascending: nulls will sort first (MongoDB treats null as smallest in ascending)
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.TrailingPE), sortBuilder.Ascending(x => x.Ticker)),
            "dividend" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.DividendYield), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.DividendYield), sortBuilder.Ascending(x => x.Ticker)),
            "change" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.ChangePercent), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.ChangePercent), sortBuilder.Ascending(x => x.Ticker)),
            "sector" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.Sector), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.Sector), sortBuilder.Ascending(x => x.Ticker)),
            "industry" => isDescending 
                ? sortBuilder.Combine(sortBuilder.Descending(x => x.Industry), sortBuilder.Ascending(x => x.Ticker))
                : sortBuilder.Combine(sortBuilder.Ascending(x => x.Industry), sortBuilder.Ascending(x => x.Ticker)),
            _ => sortBuilder.Ascending(x => x.Ticker) // Default fallback
        };
    }
}

