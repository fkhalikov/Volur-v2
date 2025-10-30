using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IExchangeRepository.
/// </summary>
public sealed class ExchangeRepository : IExchangeRepository
{
    private readonly VolurDbContext _context;
    private readonly ILogger<ExchangeRepository> _logger;

    public ExchangeRepository(VolurDbContext context, ILogger<ExchangeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Exchange> Exchanges, DateTime? FetchedAt)?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.Exchanges
                .OrderBy(e => e.Code)
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
                return null;

            var exchanges = entities.Select(e => new Exchange(
                Code: e.Code,
                Name: e.Name,
                OperatingMic: e.OperatingMic,
                Country: e.Country,
                Currency: e.Currency
            )).ToList();

            var fetchedAt = entities.FirstOrDefault()?.UpdatedAt;

            return (exchanges, fetchedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchanges from SQL Server");
            return null;
        }
    }

    public async Task<Exchange?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);

            if (entity == null)
                return null;

            return new Exchange(
                Code: entity.Code,
                Name: entity.Name,
                OperatingMic: entity.OperatingMic,
                Country: entity.Country,
                Currency: entity.Currency
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get exchange {Code} from SQL Server", code);
            return null;
        }
    }

    public async Task UpsertManyAsync(IReadOnlyList<Exchange> exchanges, DateTime fetchedAt, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var exchange in exchanges)
            {
                var existing = await _context.Exchanges.FindAsync(new object[] { exchange.Code }, cancellationToken);

                if (existing != null)
                {
                    // Update existing
                    existing.Name = exchange.Name;
                    existing.OperatingMic = exchange.OperatingMic;
                    existing.Country = exchange.Country;
                    existing.Currency = exchange.Currency;
                    // UpdatedAt will be set by interceptor
                }
                else
                {
                    // Insert new
                    var entity = new ExchangeEntity
                    {
                        Code = exchange.Code,
                        Name = exchange.Name,
                        OperatingMic = exchange.OperatingMic,
                        Country = exchange.Country,
                        Currency = exchange.Currency
                    };
                    _context.Exchanges.Add(entity);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Upserted {Count} exchanges", exchanges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert exchanges in SQL Server");
            throw;
        }
    }
}
