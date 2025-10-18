using Volur.Application.DTOs;
using Volur.Application.DTOs.Provider;
using Volur.Domain.Entities;

namespace Volur.Application.Mappers;

/// <summary>
/// Maps Symbol entities to DTOs.
/// </summary>
public static class SymbolMapper
{
    public static Symbol ToDomain(this EodhdSymbolDto dto, string parentExchange) => new(
        Ticker: dto.Code,
        ExchangeCode: dto.Exchange,
        ParentExchange: parentExchange,
        Name: dto.Name,
        Type: dto.Type,
        Isin: dto.Isin,
        Currency: dto.Currency,
        IsActive: dto.IsDelisted is false || dto.IsDelisted is null
    );

    public static SymbolDto ToDto(this Symbol entity) => new(
        Ticker: entity.Ticker,
        FullSymbol: entity.FullSymbol,
        Name: entity.Name,
        Type: entity.Type,
        Currency: entity.Currency,
        Isin: entity.Isin,
        IsActive: entity.IsActive
    );
}

