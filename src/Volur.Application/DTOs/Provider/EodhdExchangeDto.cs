using System.Text.Json.Serialization;

namespace Volur.Application.DTOs.Provider;

/// <summary>
/// EODHD provider DTO for exchange data.
/// Maps to the JSON returned by https://eodhd.com/api/exchanges-list/
/// </summary>
public sealed record EodhdExchangeDto(
    [property: JsonPropertyName("Code")] string Code,
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("OperatingMIC")] string? OperatingMic,
    [property: JsonPropertyName("Country")] string Country,
    [property: JsonPropertyName("Currency")] string Currency
);

