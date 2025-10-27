namespace Volur.Application.UseCases.GetSymbols;

/// <summary>
/// Query to get symbols for an exchange.
/// </summary>
public sealed record GetSymbolsQuery(
    string ExchangeCode,
    int Page = 1,
    int PageSize = 50,
    string? SearchQuery = null,
    string? TypeFilter = null,
    bool ForceRefresh = false,
    string? SortBy = null,
    string? SortDirection = null
);

