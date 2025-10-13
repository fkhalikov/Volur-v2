using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Application.UseCases.GetStockQuote;

/// <summary>
/// Handler for GetStockQuoteQuery.
/// </summary>
public sealed class GetStockQuoteHandler
{
    private readonly IStockDataProvider _stockDataProvider;
    private readonly ILogger<GetStockQuoteHandler> _logger;

    public GetStockQuoteHandler(
        IStockDataProvider stockDataProvider,
        ILogger<GetStockQuoteHandler> logger)
    {
        _stockDataProvider = stockDataProvider;
        _logger = logger;
    }

    public async Task<Result<StockQuoteResponse>> HandleAsync(GetStockQuoteQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock quote for ticker: {Ticker}", query.Ticker);

        var result = await _stockDataProvider.GetQuoteAsync(query.Ticker, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get quote for {Ticker}: {Error}", query.Ticker, result.Error);
            return Result.Failure<StockQuoteResponse>(result.Error!);
        }

        var response = new StockQuoteResponse(
            Quote: result.Value,
            FetchedAt: DateTime.UtcNow
        );

        _logger.LogInformation("Successfully retrieved quote for {Ticker}", query.Ticker);
        return Result.Success(response);
    }
}
