using Microsoft.AspNetCore.Mvc;
using Volur.Application.UseCases.GetExchanges;
using Volur.Application.UseCases.GetSymbols;
using Volur.Application.UseCases.RefreshSymbols;
using Volur.Application.Validators;
using Volur.Api.Models;

namespace Volur.Api.Controllers;

[ApiController]
[Route("api/exchanges")]
public class ExchangesController : ControllerBase
{
    private readonly GetExchangesHandler _getExchangesHandler;
    private readonly GetSymbolsHandler _getSymbolsHandler;
    private readonly RefreshSymbolsHandler _refreshSymbolsHandler;
    private readonly ILogger<ExchangesController> _logger;

    public ExchangesController(
        GetExchangesHandler getExchangesHandler,
        GetSymbolsHandler getSymbolsHandler,
        RefreshSymbolsHandler refreshSymbolsHandler,
        ILogger<ExchangesController> logger)
    {
        _getExchangesHandler = getExchangesHandler;
        _getSymbolsHandler = getSymbolsHandler;
        _refreshSymbolsHandler = refreshSymbolsHandler;
        _logger = logger;
    }

    /// <summary>
    /// Get all exchanges.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Application.DTOs.ExchangesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExchanges([FromQuery] bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var query = new GetExchangesQuery(forceRefresh);
        var result = await _getExchangesHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get symbols for a specific exchange.
    /// </summary>
    [HttpGet("{code}/symbols")]
    [ProducesResponseType(typeof(Application.DTOs.SymbolsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSymbols(
        string code,
        [FromQuery] string? q = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? type = null,
        [FromQuery] bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSymbolsQuery(code, page, pageSize, q, type, forceRefresh);

        // Validate
        var validator = new GetSymbolsQueryValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse(
                "VALIDATION_ERROR",
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                HttpContext.TraceIdentifier
            ));
        }

        var result = await _getSymbolsHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Force refresh the exchange list from the provider.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshExchanges(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Force refresh requested for exchanges");
        var query = new GetExchangesQuery(ForceRefresh: true);
        var result = await _getExchangesHandler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        _logger.LogInformation("Successfully refreshed {Count} exchanges", result.Value.Count);
        return NoContent();
    }

    /// <summary>
    /// Force refresh symbols for a specific exchange.
    /// </summary>
    [HttpPost("{code}/symbols/refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshSymbols(string code, CancellationToken cancellationToken = default)
    {
        var command = new RefreshSymbolsCommand(code);
        var result = await _refreshSymbolsHandler.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleError(result.Error!);
        }

        return NoContent();
    }

    private IActionResult HandleError(Shared.Error error)
    {
        var errorResponse = new ErrorResponse(error.Code, error.Message, HttpContext.TraceIdentifier);

        return error.Code switch
        {
            "BAD_EXCHANGE_CODE" => NotFound(errorResponse),
            "NOT_FOUND" => NotFound(errorResponse),
            "VALIDATION_ERROR" => BadRequest(errorResponse),
            "PROVIDER_RATE_LIMIT" => StatusCode(StatusCodes.Status429TooManyRequests, errorResponse),
            _ => StatusCode(StatusCodes.Status500InternalServerError, errorResponse)
        };
    }
}

