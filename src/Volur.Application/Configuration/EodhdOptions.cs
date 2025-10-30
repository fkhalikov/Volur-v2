namespace Volur.Application.Configuration;

/// <summary>
/// Configuration for EODHD provider.
/// </summary>
public sealed class EodhdOptions
{
    public const string SectionName = "Eodhd";

    public string ApiToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://eodhd.com/";
    public int TimeoutSeconds { get; set; } = 15; // Increased from 10 to 15 to handle slower EODHD responses
}

