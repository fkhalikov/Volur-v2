using Volur.Application.Common;

namespace Volur.Application.DTOs;

/// <summary>
/// Response for GET /exchanges.
/// </summary>
public sealed record ExchangesResponse(
    int Count,
    IReadOnlyList<ExchangeDto> Items,
    DateTime FetchedAt,
    CacheMetadata Cache
);

