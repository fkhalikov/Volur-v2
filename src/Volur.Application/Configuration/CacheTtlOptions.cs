namespace Volur.Application.Configuration;

/// <summary>
/// Configuration for cache TTL settings.
/// </summary>
public sealed class CacheTtlOptions
{
    public const string SectionName = "CacheTtl";

    public int ExchangesHours { get; set; } = 24;
    public int SymbolsHours { get; set; } = 24;
}

