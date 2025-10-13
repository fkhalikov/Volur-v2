using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volur.Application.Configuration;
using Volur.Application.Interfaces;
using Volur.Application.Mappers;
using Volur.Shared;

namespace Volur.Application.UseCases.RefreshSymbols;

/// <summary>
/// Handler for RefreshSymbolsCommand.
/// Forces a fresh fetch from provider and updates cache.
/// </summary>
public sealed class RefreshSymbolsHandler
{
    private readonly ISymbolRepository _symbolRepository;
    private readonly IExchangeRepository _exchangeRepository;
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<RefreshSymbolsHandler> _logger;
    private readonly CacheTtlOptions _cacheTtl;

    public RefreshSymbolsHandler(
        ISymbolRepository symbolRepository,
        IExchangeRepository exchangeRepository,
        IEodhdClient eodhdClient,
        ILogger<RefreshSymbolsHandler> logger,
        IOptions<CacheTtlOptions> cacheTtl)
    {
        _symbolRepository = symbolRepository;
        _exchangeRepository = exchangeRepository;
        _eodhdClient = eodhdClient;
        _logger = logger;
        _cacheTtl = cacheTtl.Value;
    }

    public async Task<Result> HandleAsync(RefreshSymbolsCommand command, CancellationToken cancellationToken = default)
    {
        // Validate exchange exists
        var exchange = await _exchangeRepository.GetByCodeAsync(command.ExchangeCode, cancellationToken);
        if (exchange == null)
        {
            return Result.Failure(Error.BadExchangeCode(command.ExchangeCode));
        }

        _logger.LogInformation("Refreshing symbols for exchange {ExchangeCode}", command.ExchangeCode);

        // Fetch from provider
        var providerResult = await _eodhdClient.GetSymbolsAsync(command.ExchangeCode, cancellationToken);
        if (providerResult.IsFailure)
        {
            _logger.LogWarning("Failed to refresh symbols for {ExchangeCode}: {Error}", 
                command.ExchangeCode, providerResult.Error);
            return Result.Failure(providerResult.Error!);
        }

        var providerSymbols = providerResult.Value;
        var domainSymbols = providerSymbols.Select(s => s.ToDomain()).ToList();
        var fetchedAtUtc = DateTime.UtcNow;
        var ttl = TimeSpan.FromHours(_cacheTtl.SymbolsHours);

        // Delete old and insert new
        await _symbolRepository.DeleteByExchangeAsync(command.ExchangeCode, cancellationToken);
        await _symbolRepository.UpsertManyAsync(command.ExchangeCode, domainSymbols, fetchedAtUtc, ttl, cancellationToken);

        _logger.LogInformation("Successfully refreshed {Count} symbols for {ExchangeCode}", 
            domainSymbols.Count, command.ExchangeCode);

        return Result.Success();
    }
}

