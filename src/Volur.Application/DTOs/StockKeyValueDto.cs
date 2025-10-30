namespace Volur.Application.DTOs;

/// <summary>
/// Stock key-value pair data transfer object.
/// </summary>
public sealed record StockKeyValueDto(
    int Id,
    string Ticker,
    string ExchangeCode,
    string Key,
    string Value,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
