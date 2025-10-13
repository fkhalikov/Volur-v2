using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence.Mappers;
using Volur.Infrastructure.Persistence.Models;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of IExchangeRepository.
/// </summary>
public sealed class ExchangeRepository : IExchangeRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<ExchangeRepository> _logger;

    public ExchangeRepository(MongoDbContext context, ILogger<ExchangeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Exchange> Exchanges, DateTime? FetchedAt)?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _context.Exchanges
                .Find(_ => true)
                .ToListAsync(cancellationToken);

            if (documents.Count == 0)
                return null;

            var exchanges = documents.Select(d => d.ToDomain()).ToList();
            var fetchedAt = documents.FirstOrDefault()?.FetchedAt;

            return (exchanges, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchanges from MongoDB");
            return null;
        }
    }

    public async Task<Exchange?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _context.Exchanges
                .Find(x => x.Code == code)
                .FirstOrDefaultAsync(cancellationToken);

            return document?.ToDomain();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange {Code} from MongoDB", code);
            return null;
        }
    }

    public async Task UpsertManyAsync(IReadOnlyList<Exchange> exchanges, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var expiresAt = fetchedAt.Add(ttl);
            var writes = exchanges.Select(e =>
            {
                var doc = e.ToDocument(fetchedAt, expiresAt);
                return new ReplaceOneModel<ExchangeDocument>(
                    Builders<ExchangeDocument>.Filter.Eq(x => x.Code, doc.Code),
                    doc)
                {
                    IsUpsert = true
                };
            }).ToList();

            if (writes.Count > 0)
            {
                await _context.Exchanges.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false }, cancellationToken);
                _logger.LogDebug("Upserted {Count} exchanges", writes.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert exchanges in MongoDB");
            throw;
        }
    }
}

