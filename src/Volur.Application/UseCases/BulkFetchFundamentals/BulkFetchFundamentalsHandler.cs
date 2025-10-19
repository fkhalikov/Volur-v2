using Microsoft.Extensions.Logging;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Application.UseCases.BulkFetchFundamentals;

/// <summary>
/// Handler for bulk fetching fundamental data for symbols without cached data.
/// </summary>
public sealed class BulkFetchFundamentalsHandler
{
    private readonly ISymbolRepository _symbolRepository;
    private readonly IStockDataRepository _stockDataRepository;
    private readonly IStockDataProvider _stockDataProvider;
    private readonly ILogger<BulkFetchFundamentalsHandler> _logger;

    public BulkFetchFundamentalsHandler(
        ISymbolRepository symbolRepository,
        IStockDataRepository stockDataRepository,
        IStockDataProvider stockDataProvider,
        ILogger<BulkFetchFundamentalsHandler> logger)
    {
        _symbolRepository = symbolRepository;
        _stockDataRepository = stockDataRepository;
        _stockDataProvider = stockDataProvider;
        _logger = logger;
    }

    public async Task<Result<BulkFetchFundamentalsResponse>> HandleAsync(
        BulkFetchFundamentalsCommand command, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting bulk fetch of fundamental data for exchange {ExchangeCode} with batch size {BatchSize}", 
            command.ExchangeCode, command.BatchSize);

        try
        {
            // Get all symbols for the exchange (without pagination)
            var allSymbolsResult = await _symbolRepository.GetAllByExchangeAsync(command.ExchangeCode, cancellationToken);
            if (!allSymbolsResult.HasValue)
            {
                return Result.Failure<BulkFetchFundamentalsResponse>(Error.NotFound("Exchange", command.ExchangeCode));
            }

            var allSymbols = allSymbolsResult.Value.symbols;
            _logger.LogInformation("Found {TotalSymbols} symbols for exchange {ExchangeCode}", allSymbols.Count, command.ExchangeCode);

            // Filter symbols that don't have cached fundamental data
            var symbolsWithoutData = new List<Domain.Entities.Symbol>();
            
            foreach (var symbol in allSymbols)
            {
                var cachedFundamentals = await _stockDataRepository.GetFundamentalsAsync(symbol.Ticker, cancellationToken);
                if (cachedFundamentals == null)
                {
                    symbolsWithoutData.Add(symbol);
                }
            }

            _logger.LogInformation("Found {SymbolsWithoutData} symbols without cached fundamental data out of {TotalSymbols}", 
                symbolsWithoutData.Count, allSymbols.Count);

            if (!symbolsWithoutData.Any())
            {
                return Result.Success(new BulkFetchFundamentalsResponse(
                    ExchangeCode: command.ExchangeCode,
                    TotalSymbols: allSymbols.Count,
                    SymbolsWithoutData: 0,
                    ProcessedSymbols: 0,
                    SuccessfulFetches: 0,
                    FailedFetches: 0,
                    BatchesProcessed: 0,
                    StartedAt: DateTime.UtcNow,
                    CompletedAt: DateTime.UtcNow
                ));
            }

            // Process symbols in batches
            var startedAt = DateTime.UtcNow;
            var processedSymbols = 0;
            var successfulFetches = 0;
            var failedFetches = 0;
            var batchesProcessed = 0;

            var batches = symbolsWithoutData
                .Select((symbol, index) => new { symbol, index })
                .GroupBy(x => x.index / command.BatchSize)
                .Select(g => g.Select(x => x.symbol).ToList())
                .ToList();

            _logger.LogInformation("Processing {BatchCount} batches of up to {BatchSize} symbols each", 
                batches.Count, command.BatchSize);

            foreach (var batch in batches)
            {
                batchesProcessed++;
                _logger.LogInformation("Processing batch {BatchNumber}/{TotalBatches} with {BatchSymbols} symbols", 
                    batchesProcessed, batches.Count, batch.Count);

                // Process batch in parallel with limited concurrency
                var batchTasks = batch.Select(async symbol =>
                {
                    try
                    {
                        // Fetch fundamental data from provider
                        var fundamentalsResult = await _stockDataProvider.GetFundamentalsAsync(symbol.Ticker, symbol.ExchangeCode, cancellationToken);
                        
                        if (fundamentalsResult.IsSuccess)
                        {
                            // Cache the fundamental data
                            await _stockDataRepository.UpsertFundamentalsAsync(fundamentalsResult.Value, cancellationToken);
                            
                            _logger.LogDebug("Successfully fetched and cached fundamentals for {Ticker}", symbol.Ticker);
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to fetch fundamentals for {Ticker}: {Error}", symbol.Ticker, fundamentalsResult.Error?.Message);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing symbol {Ticker}", symbol.Ticker);
                        return false;
                    }
                });

                // Wait for batch to complete with limited concurrency (10 concurrent requests)
                var semaphore = new SemaphoreSlim(10, 10);
                var limitedTasks = batchTasks.Select(async task =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return await task;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var batchResults = await Task.WhenAll(limitedTasks);
                
                var batchSuccesses = batchResults.Count(r => r);
                var batchFailures = batchResults.Count(r => !r);
                
                processedSymbols += batch.Count;
                successfulFetches += batchSuccesses;
                failedFetches += batchFailures;

                _logger.LogInformation("Batch {BatchNumber} completed: {Successes} successful, {Failures} failed", 
                    batchesProcessed, batchSuccesses, batchFailures);

                // Small delay between batches to avoid overwhelming the API
                if (batchesProcessed < batches.Count)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startedAt;

            _logger.LogInformation("Bulk fetch completed for {ExchangeCode}: {ProcessedSymbols} processed, {SuccessfulFetches} successful, {FailedFetches} failed in {Duration}",
                command.ExchangeCode, processedSymbols, successfulFetches, failedFetches, duration);

            return Result.Success(new BulkFetchFundamentalsResponse(
                ExchangeCode: command.ExchangeCode,
                TotalSymbols: allSymbols.Count,
                SymbolsWithoutData: symbolsWithoutData.Count,
                ProcessedSymbols: processedSymbols,
                SuccessfulFetches: successfulFetches,
                FailedFetches: failedFetches,
                BatchesProcessed: batchesProcessed,
                StartedAt: startedAt,
                CompletedAt: completedAt
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk fetch fundamental data for exchange {ExchangeCode}", command.ExchangeCode);
            return Result.Failure<BulkFetchFundamentalsResponse>(Error.InternalError($"Failed to bulk fetch fundamental data: {ex.Message}"));
        }
    }
}

/// <summary>
/// Response for bulk fetch fundamental data operation.
/// </summary>
public sealed record BulkFetchFundamentalsResponse(
    string ExchangeCode,
    int TotalSymbols,
    int SymbolsWithoutData,
    int ProcessedSymbols,
    int SuccessfulFetches,
    int FailedFetches,
    int BatchesProcessed,
    DateTime StartedAt,
    DateTime CompletedAt
);
