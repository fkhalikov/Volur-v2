using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Volur.Infrastructure.Configuration;
using Volur.Infrastructure.Persistence.Models;

namespace Volur.Infrastructure.Persistence;

/// <summary>
/// MongoDB context for accessing collections.
/// </summary>
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbContext> _logger;

    public MongoDbContext(IOptions<MongoOptions> options, ILogger<MongoDbContext> logger)
    {
        _logger = logger;
        var mongoOptions = options.Value;
        
        var client = new MongoClient(mongoOptions.ConnectionString);
        _database = client.GetDatabase(mongoOptions.Database);
    }

    public IMongoCollection<ExchangeDocument> Exchanges => 
        _database.GetCollection<ExchangeDocument>("exchanges");

    public IMongoCollection<SymbolDocument> Symbols => 
        _database.GetCollection<SymbolDocument>("symbols");

    public IMongoCollection<StockQuoteDocument> StockQuotes => 
        _database.GetCollection<StockQuoteDocument>("stockQuotes");

    public IMongoCollection<StockFundamentalsDocument> StockFundamentals => 
        _database.GetCollection<StockFundamentalsDocument>("stockFundamentals");

    /// <summary>
    /// Ensures all required indexes are created.
    /// </summary>
    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Exchanges collection indexes
            var exchangeIndexes = new List<CreateIndexModel<ExchangeDocument>>
            {
                // TTL index on expiresAt
                new CreateIndexModel<ExchangeDocument>(
                    Builders<ExchangeDocument>.IndexKeys.Ascending(x => x.ExpiresAt),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "idx_expiresAt_ttl" }
                )
            };
            await Exchanges.Indexes.CreateManyAsync(exchangeIndexes, cancellationToken);

            // Symbols collection indexes
            var symbolIndexes = new List<CreateIndexModel<SymbolDocument>>
            {
                // Unique index on FullSymbol ({Ticker}.{ExchangeCode})
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.FullSymbol),
                    new CreateIndexOptions { Name = "idx_fullsymbol_unique" }
                ),
                // Index on exchangeCode for filtering
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.ExchangeCode),
                    new CreateIndexOptions { Name = "idx_exchangecode" }
                ),
                // Index on parentExchange for filtering
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.ParentExchange),
                    new CreateIndexOptions { Name = "idx_parentexchange" }
                ),
                // TTL index on expiresAt
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.ExpiresAt),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "idx_expiresAt_ttl" }
                ),
                // Text index on name for search
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Text(x => x.Name),
                    new CreateIndexOptions { Name = "idx_name_text" }
                ),
                // Index on ticker for search
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.Ticker),
                    new CreateIndexOptions { Name = "idx_ticker" }
                ),
                // Index on type for filtering
                new CreateIndexModel<SymbolDocument>(
                    Builders<SymbolDocument>.IndexKeys.Ascending(x => x.Type),
                    new CreateIndexOptions { Name = "idx_type" }
                )
            };
            await Symbols.Indexes.CreateManyAsync(symbolIndexes, cancellationToken);

            // Stock quotes collection indexes
            var quoteIndexes = new List<CreateIndexModel<StockQuoteDocument>>
            {
                // Unique index on ticker
                new CreateIndexModel<StockQuoteDocument>(
                    Builders<StockQuoteDocument>.IndexKeys.Ascending(x => x.Ticker),
                    new CreateIndexOptions { Unique = true, Name = "idx_ticker_unique" }
                ),
                // Index on fetchedAt for sorting
                new CreateIndexModel<StockQuoteDocument>(
                    Builders<StockQuoteDocument>.IndexKeys.Descending(x => x.FetchedAt),
                    new CreateIndexOptions { Name = "idx_fetchedat" }
                )
            };
            await StockQuotes.Indexes.CreateManyAsync(quoteIndexes, cancellationToken);

            // Stock fundamentals collection indexes
            var fundamentalsIndexes = new List<CreateIndexModel<StockFundamentalsDocument>>
            {
                // Unique index on ticker
                new CreateIndexModel<StockFundamentalsDocument>(
                    Builders<StockFundamentalsDocument>.IndexKeys.Ascending(x => x.Ticker),
                    new CreateIndexOptions { Unique = true, Name = "idx_ticker_unique" }
                ),
                // Index on fetchedAt for sorting
                new CreateIndexModel<StockFundamentalsDocument>(
                    Builders<StockFundamentalsDocument>.IndexKeys.Descending(x => x.FetchedAt),
                    new CreateIndexOptions { Name = "idx_fetchedat" }
                )
            };
            await StockFundamentals.Indexes.CreateManyAsync(fundamentalsIndexes, cancellationToken);

            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MongoDB indexes");
            throw;
        }
    }
}

