using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Application.UseCases.GetHistoricalPrices;

/// <summary>
/// Handler for GetHistoricalPricesQuery.
/// </summary>
public sealed class GetHistoricalPricesHandler
{
    private readonly IStockDataProvider _stockDataProvider;
    private readonly ILogger<GetHistoricalPricesHandler> _logger;

    public GetHistoricalPricesHandler(
        IStockDataProvider stockDataProvider,
        ILogger<GetHistoricalPricesHandler> logger)
    {
        _stockDataProvider = stockDataProvider;
        _logger = logger;
    }

    public async Task<Result<HistoricalPriceResponse>> HandleAsync(GetHistoricalPricesQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting historical prices for {Ticker} from {StartDate} to {EndDate}", 
            query.Ticker, query.StartDate, query.EndDate);

        var result = await _stockDataProvider.GetHistoricalPricesAsync(
            query.Ticker, 
            query.StartDate, 
            query.EndDate, 
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get historical prices for {Ticker}: {Error}", query.Ticker, result.Error);
            return Result.Failure<HistoricalPriceResponse>(result.Error!);
        }

        var response = new HistoricalPriceResponse(
            Ticker: query.Ticker,
            Prices: result.Value,
            FetchedAt: DateTime.UtcNow
        );

        _logger.LogInformation("Successfully retrieved {Count} historical prices for {Ticker}", 
            result.Value.Count, query.Ticker);

        return Result.Success(response);
    }
}
