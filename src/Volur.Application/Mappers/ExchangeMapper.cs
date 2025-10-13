using Volur.Application.DTOs;
using Volur.Application.DTOs.Provider;
using Volur.Domain.Entities;

namespace Volur.Application.Mappers;

/// <summary>
/// Maps Exchange entities to DTOs.
/// </summary>
public static class ExchangeMapper
{
    public static Exchange ToDomain(this EodhdExchangeDto dto) => new(
        Code: dto.Code,
        Name: dto.Name,
        OperatingMic: dto.OperatingMic,
        Country: dto.Country,
        Currency: dto.Currency
    );

    public static ExchangeDto ToDto(this Exchange entity) => new(
        Code: entity.Code,
        Name: entity.Name,
        Country: entity.Country,
        Currency: entity.Currency,
        OperatingMic: entity.OperatingMic
    );
}

