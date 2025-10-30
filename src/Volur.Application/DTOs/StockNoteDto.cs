namespace Volur.Application.DTOs;

/// <summary>
/// Stock note data transfer object.
/// </summary>
public sealed record StockNoteDto(
    int Id,
    string Ticker,
    string ExchangeCode,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
