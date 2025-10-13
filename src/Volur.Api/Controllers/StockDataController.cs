using Microsoft.AspNetCore.Mvc;
using Volur.Api.Models;
using Volur.Application.UseCases.GetStockQuote;
using Volur.Application.UseCases.GetStockFundamentals;
using Volur.Application.UseCases.GetHistoricalPrices;

namespace Volur.Api.Controllers;

[ApiController]
[Route("api/stocks")]
public class StockDataController : ControllerBase
{
    private readonly GetStockQuoteHandler _getQuoteHandler;
    private readonly GetStockFundamentalsHandler _getFundamentalsHandler;
    private readonly GetHistoricalPricesHandler _getHistoricalPricesHandler;
    private readonly ILogger<StockDataController> _logger;

    public StockDataController(
        GetStockQuoteHandler getQuoteHandler,
        GetStockFundamentalsHandler getFundamentalsHandler,
        GetHistoricalPricesHandler getHistoricalPricesHandler,
        ILogger<StockDataController> logger)
    {
        _getQuoteHandler = getQuoteHandler;
        _getFundamentalsHandler = getFundamentalsHandler;
        _getHistoricalPricesHandler = getHistoricalPricesHandler;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time stock quote for a ticker.
    /// </summary>
    [HttpGet("{ticker}/quote")]
    [ProducesResponseType(typeof(Application.DTOs.StockQuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetQuote(string ticker, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Ticker is required.",
                HttpContext.TraceIdentifier
            ));
        }

        var query = new GetStockQuoteQuery(ticker.ToUpperInvariant());
        var result = await _getQuoteHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get fundamental data for a ticker.
    /// </summary>
    [HttpGet("{ticker}/fundamentals")]
    [ProducesResponseType(typeof(Application.DTOs.StockFundamentalsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFundamentals(string ticker, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Ticker is required.",
                HttpContext.TraceIdentifier
            ));
        }

        var query = new GetStockFundamentalsQuery(ticker.ToUpperInvariant());
        var result = await _getFundamentalsHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get historical price data for a ticker.
    /// </summary>
    [HttpGet("{ticker}/history")]
    [ProducesResponseType(typeof(Application.DTOs.HistoricalPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHistoricalPrices(
        string ticker,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Ticker is required.",
                HttpContext.TraceIdentifier
            ));
        }

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (start >= end)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Start date must be before end date.",
                HttpContext.TraceIdentifier
            ));
        }

        if ((end - start).TotalDays > 365)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                "Date range cannot exceed 365 days.",
                HttpContext.TraceIdentifier
            ));
        }

        var query = new GetHistoricalPricesQuery(ticker.ToUpperInvariant(), start, end);
        var result = await _getHistoricalPricesHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(result.Value);
    }

    private IActionResult HandleError(Shared.Error error)
    {
        var errorResponse = new ErrorResponse(error.Code, error.Message, HttpContext.TraceIdentifier);

        return error.Code switch
        {
            "NOT_FOUND" => NotFound(errorResponse),
            "VALIDATION_ERROR" => BadRequest(errorResponse),
            "PROVIDER_RATE_LIMIT" => StatusCode(StatusCodes.Status429TooManyRequests, errorResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
        };
    }
}
