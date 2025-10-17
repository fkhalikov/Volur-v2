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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filterBuilder = Builders<SymbolDocument>.Filter;
            var filter = filterBuilder.Eq(x => x.ExchangeCode, exchangeCode);

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

            // Get paged results
            var skip = (page - 1) * pageSize;
            var documents = await _context.Symbols
                .Find(filter)
                .Sort(Builders<SymbolDocument>.Sort.Ascending(x => x.Ticker))
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
            
            // De-duplicate by ticker (keep first occurrence)
            var uniqueSymbols = symbols
                .GroupBy(s => s.Ticker)
                .Select(g => g.First())
                .ToList();
            
            if (uniqueSymbols.Count < symbols.Count)
            {
                _logger.LogWarning("Removed {DuplicateCount} duplicate tickers for {ExchangeCode}", 
                    symbols.Count - uniqueSymbols.Count, exchangeCode);
            }
            
            var writes = uniqueSymbols.Select(s =>
            {
                var doc = s.ToDocument(fetchedAt, expiresAt);
                return new ReplaceOneModel<SymbolDocument>(
                    Builders<SymbolDocument>.Filter.And(
                        Builders<SymbolDocument>.Filter.Eq(x => x.ExchangeCode, exchangeCode),
                        Builders<SymbolDocument>.Filter.Eq(x => x.Ticker, s.Ticker)
                    ),
                    doc)
                {
                    IsUpsert = true
                };
            }).ToList();

            if (writes.Count > 0)
            {
                await _context.Symbols.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
                _logger.LogDebug("Upserted {Count} symbols for {ExchangeCode}", writes.Count, exchangeCode);
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
                x => x.ExchangeCode == exchangeCode,
                cancellationToken);

            _logger.LogDebug("Deleted {Count} symbols for {ExchangeCode}", result.DeletedCount, exchangeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete symbols for {ExchangeCode} from MongoDB", exchangeCode);
            throw;
        }
    }
}

