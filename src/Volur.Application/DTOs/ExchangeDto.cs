namespace Volur.Application.DTOs;

/// <summary>
/// Data transfer object for Exchange.
/// </summary>
public sealed record ExchangeDto(
    string Code,
    string Name,
    string Country,
    string Currency,
    string? OperatingMic = null
);

