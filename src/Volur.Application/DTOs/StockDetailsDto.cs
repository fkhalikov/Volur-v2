namespace Volur.Application.DTOs;

/// <summary>
/// Combined response for stock details including symbol, quote, and fundamentals.
/// </summary>
public sealed record StockDetailsResponse(
    SymbolDto Symbol,
    StockQuoteDto? Quote,
    StockFundamentalsDto? Fundamentals,
    DateTime? QuoteFetchedAt,
    DateTime? FundamentalsFetchedAt,
    DateTime RequestedAt
);

/// <summary>
/// Metadata about when different data types were last fetched.
/// </summary>
public sealed record StockDataMetadata(
    DateTime? QuoteFetchedAt,
    DateTime? FundamentalsFetchedAt,
    bool HasQuote,
    bool HasFundamentals
);
