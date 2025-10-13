using System.Text.Json.Serialization;

namespace Volur.Application.DTOs.Provider;

/// <summary>
/// EODHD provider DTO for symbol data.
/// Maps to the JSON returned by https://eodhd.com/api/exchange-symbol-list/{EXCHANGE_CODE}
/// </summary>
public sealed record EodhdSymbolDto(
    [property: JsonPropertyName("Code")] string Code,
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Country")] string? Country,
    [property: JsonPropertyName("Exchange")] string Exchange,
    [property: JsonPropertyName("Currency")] string? Currency,
    [property: JsonPropertyName("Type")] string? Type,
    [property: JsonPropertyName("Isin")] string? Isin,
    [property: JsonPropertyName("IsDelisted")] bool? IsDelisted = null
);

