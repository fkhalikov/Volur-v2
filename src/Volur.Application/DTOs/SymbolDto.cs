namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for Symbol.
/// </summary>
public sealed record SymbolDto(
    string Ticker,
    string Name,
    string? Type,
    string? Currency,
    string? Isin,
    bool IsActive
);

