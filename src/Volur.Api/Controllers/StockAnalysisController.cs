using Microsoft.AspNetCore.Mvc;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;

namespace Volur.Api.Controllers;

/// <summary>
/// Controller for managing stock analysis data (notes and key-value pairs).
/// </summary>
[ApiController]
[Route("api/stock-analysis")]
public sealed class StockAnalysisController : ControllerBase
{
    private readonly IStockAnalysisRepository _repository;
    private readonly ILogger<StockAnalysisController> _logger;

    public StockAnalysisController(
        IStockAnalysisRepository repository,
        ILogger<StockAnalysisController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get all notes for a stock symbol.
    /// </summary>
    [HttpGet("{ticker}/{exchangeCode}/notes")]
    public async Task<ActionResult<IEnumerable<StockNoteDto>>> GetNotes(string ticker, string exchangeCode, CancellationToken cancellationToken)
    {
        var notes = await _repository.GetNotesAsync(ticker, exchangeCode, cancellationToken);
        var dtos = notes.Select(n => new StockNoteDto(
            n.Id,
            n.Ticker,
            n.ExchangeCode,
            n.Content,
            n.CreatedAt,
            n.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Create a new note for a stock symbol.
    /// </summary>
    [HttpPost("{ticker}/{exchangeCode}/notes")]
    public async Task<ActionResult<StockNoteDto>> CreateNote(string ticker, string exchangeCode, [FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required");
        }

        var note = new Domain.Entities.StockNote
        {
            Ticker = ticker,
            ExchangeCode = exchangeCode,
            Content = request.Content
        };

        var created = await _repository.CreateNoteAsync(note, cancellationToken);
        var dto = new StockNoteDto(
            created.Id,
            created.Ticker,
            created.ExchangeCode,
            created.Content,
            created.CreatedAt,
            created.UpdatedAt
        );
        return CreatedAtAction(nameof(GetNote), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Get a specific note by ID.
    /// </summary>
    [HttpGet("notes/{id}")]
    public async Task<ActionResult<StockNoteDto>> GetNote(int id, CancellationToken cancellationToken)
    {
        var note = await _repository.GetNoteByIdAsync(id, cancellationToken);
        if (note == null)
        {
            return NotFound();
        }

        var dto = new StockNoteDto(
            note.Id,
            note.Ticker,
            note.ExchangeCode,
            note.Content,
            note.CreatedAt,
            note.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Update an existing note.
    /// </summary>
    [HttpPut("notes/{id}")]
    public async Task<ActionResult<StockNoteDto>> UpdateNote(int id, [FromBody] UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetNoteByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required");
        }

        existing.Content = request.Content;
        var updated = await _repository.UpdateNoteAsync(existing, cancellationToken);
        var dto = new StockNoteDto(
            updated.Id,
            updated.Ticker,
            updated.ExchangeCode,
            updated.Content,
            updated.CreatedAt,
            updated.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Delete a note.
    /// </summary>
    [HttpDelete("notes/{id}")]
    public async Task<IActionResult> DeleteNote(int id, CancellationToken cancellationToken)
    {
        await _repository.DeleteNoteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get all key-value pairs for a stock symbol.
    /// </summary>
    [HttpGet("{ticker}/{exchangeCode}/key-values")]
    public async Task<ActionResult<IEnumerable<StockKeyValueDto>>> GetKeyValues(string ticker, string exchangeCode, CancellationToken cancellationToken)
    {
        var keyValues = await _repository.GetKeyValuesAsync(ticker, exchangeCode, cancellationToken);
        var dtos = keyValues.Select(kv => new StockKeyValueDto(
            kv.Id,
            kv.Ticker,
            kv.ExchangeCode,
            kv.Key,
            kv.Value,
            kv.CreatedAt,
            kv.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Create a new key-value pair for a stock symbol.
    /// </summary>
    [HttpPost("{ticker}/{exchangeCode}/key-values")]
    public async Task<ActionResult<StockKeyValueDto>> CreateKeyValue(string ticker, string exchangeCode, [FromBody] CreateKeyValueRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest("Key and Value are required");
        }

        var keyValue = new Domain.Entities.StockKeyValue
        {
            Ticker = ticker,
            ExchangeCode = exchangeCode,
            Key = request.Key,
            Value = request.Value
        };

        var created = await _repository.CreateKeyValueAsync(keyValue, cancellationToken);
        var dto = new StockKeyValueDto(
            created.Id,
            created.Ticker,
            created.ExchangeCode,
            created.Key,
            created.Value,
            created.CreatedAt,
            created.UpdatedAt
        );
        return CreatedAtAction(nameof(GetKeyValue), new { id = created.Id }, dto);
    }

    /// <summary>
    /// Get a specific key-value pair by ID.
    /// </summary>
    [HttpGet("key-values/{id}")]
    public async Task<ActionResult<StockKeyValueDto>> GetKeyValue(int id, CancellationToken cancellationToken)
    {
        var keyValue = await _repository.GetKeyValueByIdAsync(id, cancellationToken);
        if (keyValue == null)
        {
            return NotFound();
        }

        var dto = new StockKeyValueDto(
            keyValue.Id,
            keyValue.Ticker,
            keyValue.ExchangeCode,
            keyValue.Key,
            keyValue.Value,
            keyValue.CreatedAt,
            keyValue.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Update an existing key-value pair.
    /// </summary>
    [HttpPut("key-values/{id}")]
    public async Task<ActionResult<StockKeyValueDto>> UpdateKeyValue(int id, [FromBody] UpdateKeyValueRequest request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetKeyValueByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Key) || string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest("Key and Value are required");
        }

        existing.Key = request.Key;
        existing.Value = request.Value;
        var updated = await _repository.UpdateKeyValueAsync(existing, cancellationToken);
        var dto = new StockKeyValueDto(
            updated.Id,
            updated.Ticker,
            updated.ExchangeCode,
            updated.Key,
            updated.Value,
            updated.CreatedAt,
            updated.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Delete a key-value pair.
    /// </summary>
    [HttpDelete("key-values/{id}")]
    public async Task<IActionResult> DeleteKeyValue(int id, CancellationToken cancellationToken)
    {
        await _repository.DeleteKeyValueAsync(id, cancellationToken);
        return NoContent();
    }

    // Request DTOs
    public sealed record CreateNoteRequest(string Content);
    public sealed record UpdateNoteRequest(string Content);
    public sealed record CreateKeyValueRequest(string Key, string Value);
    public sealed record UpdateKeyValueRequest(string Key, string Value);
}
