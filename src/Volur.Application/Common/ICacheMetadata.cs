namespace Volur.Application.Common;

/// <summary>
/// Metadata about cache state for a response.
/// </summary>
public sealed record CacheMetadata(
    string Source,        // "sql" | "memory" | "provider"
    int TtlSeconds
);

