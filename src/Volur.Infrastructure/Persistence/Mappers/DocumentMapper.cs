using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence.Models;

namespace Volur.Infrastructure.Persistence.Mappers;

/// <summary>
/// Maps domain entities to MongoDB documents.
/// </summary>
public static class DocumentMapper
{
    public static ExchangeDocument ToDocument(this Exchange entity, DateTime fetchedAt, DateTime expiresAt)
    {
        return new ExchangeDocument
        {
            Code = entity.Code,
            Name = entity.Name,
            OperatingMic = entity.OperatingMic,
            Country = entity.Country,
            Currency = entity.Currency,
            FetchedAt = fetchedAt,
            ExpiresAt = expiresAt
        };
    }

    public static Exchange ToDomain(this ExchangeDocument doc)
    {
        return new Exchange(
            Code: doc.Code,
            Name: doc.Name,
            OperatingMic: doc.OperatingMic,
            Country: doc.Country,
            Currency: doc.Currency
        );
    }

    public static SymbolDocument ToDocument(this Symbol entity, DateTime fetchedAt, DateTime expiresAt)
    {
        return new SymbolDocument
        {
            Ticker = entity.Ticker,
            ExchangeCode = entity.ExchangeCode,
            Name = entity.Name,
            Type = entity.Type,
            Isin = entity.Isin,
            Currency = entity.Currency,
            IsActive = entity.IsActive,
            FetchedAt = fetchedAt,
            ExpiresAt = expiresAt
        };
    }

    public static Symbol ToDomain(this SymbolDocument doc)
    {
        return new Symbol(
            Ticker: doc.Ticker,
            ExchangeCode: doc.ExchangeCode,
            Name: doc.Name,
            Type: doc.Type,
            Isin: doc.Isin,
            Currency: doc.Currency,
            IsActive: doc.IsActive
        );
    }
}

