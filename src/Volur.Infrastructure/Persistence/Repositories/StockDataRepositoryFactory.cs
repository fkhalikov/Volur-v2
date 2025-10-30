using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volur.Application.Interfaces;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// Factory for creating parallel-safe StockDataRepository instances.
/// </summary>
public sealed class StockDataRepositoryFactory : IStockDataRepositoryFactory
{
    private readonly IDbContextFactory<VolurDbContext> _dbContextFactory;
    private readonly ILoggerFactory _loggerFactory;

    public StockDataRepositoryFactory(
        IDbContextFactory<VolurDbContext> dbContextFactory,
        ILoggerFactory loggerFactory)
    {
        _dbContextFactory = dbContextFactory;
        _loggerFactory = loggerFactory;
    }

    public IStockDataRepository Create()
    {
        var context = _dbContextFactory.CreateDbContext();
        var logger = _loggerFactory.CreateLogger<StockDataRepository>();
        var symbolRepoLogger = _loggerFactory.CreateLogger<SymbolRepository>();
        var symbolRepo = new SymbolRepository(context, symbolRepoLogger);
        return new StockDataRepository(context, logger, symbolRepo);
    }
}

