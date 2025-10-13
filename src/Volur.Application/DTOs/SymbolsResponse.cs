using Volur.Application.Common;

namespace Volur.Application.DTOs;

/// <summary>
/// Response for GET /exchanges/{code}/symbols.
/// </summary>
public sealed record SymbolsResponse(
    ExchangeDto Exchange,
    PaginationMetadata Pagination,
    IReadOnlyList<SymbolDto> Items,
    DateTime FetchedAt,
    CacheMetadata Cache
);

public sealed record PaginationMetadata(
    int Page,
    int PageSize,
    int Total,
    bool HasNext
);

