namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for Symbol with optional fundamental data.
/// </summary>
public sealed record SymbolDto(
    string Ticker,
    string FullSymbol,
    string Name,
    string? Type,
    string? Currency,
    string? Isin,
    bool IsActive,
    
    // Fundamental data (optional - may be null if not available)
    double? MarketCap = null,
    double? TrailingPE = null,
    double? DividendYield = null,
    double? CurrentPrice = null,
    double? ChangePercent = null,
    string? Sector = null,
    string? Industry = null,
    DateTime? FundamentalsFetchedAt = null
);

