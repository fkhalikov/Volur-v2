using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Application.UseCases.GetStockFundamentals;

/// <summary>
/// Handler for GetStockFundamentalsQuery.
/// </summary>
public sealed class GetStockFundamentalsHandler
{
    private readonly IStockDataProvider _stockDataProvider;
    private readonly ILogger<GetStockFundamentalsHandler> _logger;

    public GetStockFundamentalsHandler(
        IStockDataProvider stockDataProvider,
        ILogger<GetStockFundamentalsHandler> logger)
    {
        _stockDataProvider = stockDataProvider;
        _logger = logger;
    }

    public async Task<Result<StockFundamentalsResponse>> HandleAsync(GetStockFundamentalsQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting fundamentals for ticker: {Ticker}", query.Ticker);

        var result = await _stockDataProvider.GetFundamentalsAsync(query.Ticker, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get fundamentals for {Ticker}: {Error}", query.Ticker, result.Error);
            return Result.Failure<StockFundamentalsResponse>(result.Error!);
        }

        var response = new StockFundamentalsResponse(
            Fundamentals: result.Value,
            FetchedAt: DateTime.UtcNow
        );

        _logger.LogInformation("Successfully retrieved fundamentals for {Ticker}", query.Ticker);
        return Result.Success(response);
    }
}
