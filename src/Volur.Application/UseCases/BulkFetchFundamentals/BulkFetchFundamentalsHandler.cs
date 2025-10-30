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
    private readonly IStockDataRepositoryFactory _stockDataRepositoryFactory;
    private readonly ILogger<BulkFetchFundamentalsHandler> _logger;

    public BulkFetchFundamentalsHandler(
        ISymbolRepository symbolRepository,
        IStockDataRepository stockDataRepository,
        IStockDataProvider stockDataProvider,
        IStockDataRepositoryFactory stockDataRepositoryFactory,
        ILogger<BulkFetchFundamentalsHandler> logger)
    {
        _symbolRepository = symbolRepository;
        _stockDataRepository = stockDataRepository;
        _stockDataProvider = stockDataProvider;
        _stockDataRepositoryFactory = stockDataRepositoryFactory;
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

            // Filter symbols that don't have cached fundamental data and aren't marked as having no data available
            var symbolsWithoutData = new List<Domain.Entities.Symbol>();
            var skippedNoDataSymbols = 0;
            
            foreach (var symbol in allSymbols)
            {
                var cachedFundamentals = await _stockDataRepository.GetFundamentalsAsync(symbol.Ticker, cancellationToken);
                if (cachedFundamentals == null)
                {
                    // Check if this symbol is marked as having no data available
                    var isNoDataAvailable = await _stockDataRepository.IsNoDataAvailableAsync(symbol.Ticker, symbol.ExchangeCode, cancellationToken);
                    if (!isNoDataAvailable)
                    {
                        symbolsWithoutData.Add(symbol);
                    }
                    else
                    {
                        skippedNoDataSymbols++;
                        _logger.LogDebug("Skipping {Ticker}.{ExchangeCode} - marked as no data available", symbol.Ticker, symbol.ExchangeCode);
                    }
                }
            }

            _logger.LogInformation("Found {SymbolsWithoutData} symbols without cached fundamental data out of {TotalSymbols} (skipped {SkippedNoData} symbols marked as no-data-available)", 
                symbolsWithoutData.Count, allSymbols.Count, skippedNoDataSymbols);

            if (!symbolsWithoutData.Any())
            {
                return Result.Success(new BulkFetchFundamentalsResponse(
                    ExchangeCode: command.ExchangeCode,
                    TotalSymbols: allSymbols.Count,
                    SymbolsWithoutData: 0,
                    SkippedNoDataSymbols: skippedNoDataSymbols,
                    ProcessedSymbols: 0,
                    SuccessfulFetches: 0,
                    FailedFetches: 0,
                    RateLimitHits: 0,
                    DailyLimitHit: false,
                    TotalWaitTime: TimeSpan.Zero,
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
            var rateLimitHits = 0;
            var totalWaitTime = TimeSpan.Zero;

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
                    using var stockDataRepo = _stockDataRepositoryFactory.Create();
                    
                    try
                    {
                        // Fetch fundamental data from provider
                        var fundamentalsResult = await _stockDataProvider.GetFundamentalsAsync(symbol.Ticker, symbol.ExchangeCode, cancellationToken);
                        
                        if (fundamentalsResult.IsSuccess)
                        {
                            // Cache the fundamental data using the dedicated DbContext
                            await stockDataRepo.UpsertFundamentalsAsync(fundamentalsResult.Value, cancellationToken);
                            
                            // Remove from no-data-available list if it was there (data is now available)
                            await stockDataRepo.RemoveNoDataAvailableAsync(symbol.Ticker, symbol.ExchangeCode, cancellationToken);
                            
                            _logger.LogDebug("Successfully fetched and cached fundamentals for {Ticker}.{ExchangeCode}", symbol.Ticker, symbol.ExchangeCode);
                            return (success: true, isRateLimit: false, isDailyLimit: false);
                        }
                        else
                        {
                            // Check if this is a rate limit or daily limit error
                            var isRateLimit = fundamentalsResult.Error?.Code == "PROVIDER_RATE_LIMIT";
                            var isDailyLimit = fundamentalsResult.Error?.Code == "PROVIDER_DAILY_LIMIT";
                            
                            if (isDailyLimit)
                            {
                                _logger.LogError("Daily limit exceeded for {Ticker}.{ExchangeCode}: {Error} - STOPPING bulk fetch", 
                                    symbol.Ticker, symbol.ExchangeCode, fundamentalsResult.Error?.Message);
                                return (success: false, isRateLimit: false, isDailyLimit: true);
                            }
                            else if (isRateLimit)
                            {
                                _logger.LogWarning("Rate limit hit for {Ticker}.{ExchangeCode}: {Error}", 
                                    symbol.Ticker, symbol.ExchangeCode, fundamentalsResult.Error?.Message);
                                return (success: false, isRateLimit: true, isDailyLimit: false);
                            }
                            else
                            {
                                // Mark as having no data available to avoid future requests
                                await stockDataRepo.MarkAsNoDataAvailableAsync(symbol.Ticker, symbol.ExchangeCode, fundamentalsResult.Error?.Message, cancellationToken);
                                
                                _logger.LogWarning("Failed to fetch fundamentals for {Ticker}.{ExchangeCode}: {Error} - marked as no-data-available", 
                                    symbol.Ticker, symbol.ExchangeCode, fundamentalsResult.Error?.Message);
                                return (success: false, isRateLimit: false, isDailyLimit: false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Mark as having no data available due to exception
                        try
                        {
                            await stockDataRepo.MarkAsNoDataAvailableAsync(symbol.Ticker, symbol.ExchangeCode, ex.Message, cancellationToken);
                        }
                        catch
                        {
                            // Ignore errors when marking as no data available
                        }
                        
                        _logger.LogError(ex, "Error processing symbol {Ticker}.{ExchangeCode} - marked as no-data-available", symbol.Ticker, symbol.ExchangeCode);
                        return (success: false, isRateLimit: false, isDailyLimit: false);
                    }
                });

                // Wait for batch to complete with limited concurrency (5 concurrent requests to avoid overwhelming EODHD)
                var semaphore = new SemaphoreSlim(5, 5);
                var limitedTasks = batchTasks.Select(async (task, index) =>
                {
                    // Add a small delay to stagger requests and avoid overwhelming the API
                    // Delay increases with index to space out requests
                    if (index > 0)
                    {
                        await Task.Delay(index * 50, cancellationToken); // 50ms delay per request
                    }
                    
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
                
                var batchSuccesses = batchResults.Count(r => r.success);
                var batchFailures = batchResults.Count(r => !r.success);
                var batchRateLimits = batchResults.Count(r => r.isRateLimit);
                var batchDailyLimits = batchResults.Count(r => r.isDailyLimit);
                
                processedSymbols += batch.Count;
                successfulFetches += batchSuccesses;
                failedFetches += batchFailures;
                rateLimitHits += batchRateLimits;

                _logger.LogInformation("Batch {BatchNumber} completed: {Successes} successful, {Failures} failed, {RateLimits} rate limited, {DailyLimits} daily limited", 
                    batchesProcessed, batchSuccesses, batchFailures, batchRateLimits, batchDailyLimits);

                // Handle daily limit - STOP the entire bulk fetch operation
                if (batchDailyLimits > 0)
                {
                    _logger.LogError("Daily limit exceeded in batch {BatchNumber}. STOPPING bulk fetch operation immediately.", batchesProcessed);
                    
                    // Return early with current progress
                    var earlyCompletedAt = DateTime.UtcNow;
                    var earlyDuration = earlyCompletedAt - startedAt;

                    _logger.LogError("Bulk fetch STOPPED due to daily limit: {ProcessedSymbols} processed, {SuccessfulFetches} successful, {FailedFetches} failed, {RateLimitHits} rate limited in {Duration}",
                        processedSymbols, successfulFetches, failedFetches, rateLimitHits, earlyDuration);

                    return Result.Success(new BulkFetchFundamentalsResponse(
                        ExchangeCode: command.ExchangeCode,
                        TotalSymbols: allSymbols.Count,
                        SymbolsWithoutData: symbolsWithoutData.Count,
                        SkippedNoDataSymbols: skippedNoDataSymbols,
                        ProcessedSymbols: processedSymbols,
                        SuccessfulFetches: successfulFetches,
                        FailedFetches: failedFetches,
                        RateLimitHits: rateLimitHits,
                        DailyLimitHit: true,
                        TotalWaitTime: totalWaitTime,
                        BatchesProcessed: batchesProcessed,
                        StartedAt: startedAt,
                        CompletedAt: earlyCompletedAt
                    ));
                }

                // Handle rate limiting intelligently
                if (batchRateLimits > 0)
                {
                    _logger.LogWarning("Rate limit detected in batch {BatchNumber}. Pausing bulk fetch operation.", batchesProcessed);
                    
                    // Calculate wait time based on rate limit hits
                    var waitTime = TimeSpan.FromMinutes(Math.Min(5, batchRateLimits)); // Max 5 minutes
                    totalWaitTime = totalWaitTime.Add(waitTime);
                    
                    _logger.LogInformation("Pausing bulk fetch for {WaitTime} to respect rate limits", waitTime);
                    
                    try
                    {
                        await Task.Delay(waitTime, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Bulk fetch operation was cancelled during rate limit wait");
                        break;
                    }
                    
                    _logger.LogInformation("Resuming bulk fetch after rate limit wait");
                }
                else if (batchesProcessed < batches.Count)
                {
                    // Normal delay between batches to avoid overwhelming the API
                    await Task.Delay(1000, cancellationToken);
                }
            }

            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startedAt;

            _logger.LogInformation("Bulk fetch completed for {ExchangeCode}: {ProcessedSymbols} processed, {SuccessfulFetches} successful, {FailedFetches} failed, {RateLimitHits} rate limited in {Duration} (waited {WaitTime} for rate limits)",
                command.ExchangeCode, processedSymbols, successfulFetches, failedFetches, rateLimitHits, duration, totalWaitTime);

            return Result.Success(new BulkFetchFundamentalsResponse(
                ExchangeCode: command.ExchangeCode,
                TotalSymbols: allSymbols.Count,
                SymbolsWithoutData: symbolsWithoutData.Count,
                SkippedNoDataSymbols: skippedNoDataSymbols,
                ProcessedSymbols: processedSymbols,
                SuccessfulFetches: successfulFetches,
                FailedFetches: failedFetches,
                RateLimitHits: rateLimitHits,
                DailyLimitHit: false,
                TotalWaitTime: totalWaitTime,
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
    int SkippedNoDataSymbols,
    int ProcessedSymbols,
    int SuccessfulFetches,
    int FailedFetches,
    int RateLimitHits,
    bool DailyLimitHit,
    TimeSpan TotalWaitTime,
    int BatchesProcessed,
    DateTime StartedAt,
    DateTime CompletedAt
);
