using Volur.Application.DTOs;
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
            ParentExchange = entity.ParentExchange,
            FullSymbol = entity.FullSymbol,
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
            ParentExchange: doc.ParentExchange,
            Name: doc.Name,
            Type: doc.Type,
            Isin: doc.Isin,
            Currency: doc.Currency,
            IsActive: doc.IsActive
        );
    }

    public static StockQuoteDocument ToDocument(this StockQuoteDto dto, DateTime fetchedAt)
    {
        return new StockQuoteDocument
        {
            Ticker = dto.Ticker,
            CurrentPrice = dto.CurrentPrice,
            PreviousClose = dto.PreviousClose,
            Change = dto.Change,
            ChangePercent = dto.ChangePercent,
            Open = dto.Open,
            High = dto.High,
            Low = dto.Low,
            Volume = dto.Volume,
            AverageVolume = dto.AverageVolume,
            LastUpdated = dto.LastUpdated,
            FetchedAt = fetchedAt
        };
    }

    public static StockQuoteDto ToDto(this StockQuoteDocument doc)
    {
        return new StockQuoteDto(
            Ticker: doc.Ticker,
            CurrentPrice: doc.CurrentPrice,
            PreviousClose: doc.PreviousClose,
            Change: doc.Change,
            ChangePercent: doc.ChangePercent,
            Open: doc.Open,
            High: doc.High,
            Low: doc.Low,
            Volume: doc.Volume,
            AverageVolume: doc.AverageVolume,
            LastUpdated: doc.LastUpdated
        );
    }

    public static StockFundamentalsDocument ToDocument(this StockFundamentalsDto dto, DateTime fetchedAt)
    {
        return new StockFundamentalsDocument
        {
            Ticker = dto.Ticker,
            CompanyName = dto.CompanyName,
            Sector = dto.Sector,
            Industry = dto.Industry,
            Description = dto.Description,
            Website = dto.Website,
            LogoUrl = dto.LogoUrl,
            CurrencyCode = dto.CurrencyCode,
            CurrencySymbol = dto.CurrencySymbol,
            CurrencyName = dto.CurrencyName,
            // Note: Highlights are stored as individual fields, not as a separate nested object
            MarketCap = dto.MarketCap,
            EnterpriseValue = dto.EnterpriseValue,
            TrailingPE = dto.TrailingPE,
            ForwardPE = dto.ForwardPE,
            PEG = dto.PEG,
            PriceToSales = dto.PriceToSales,
            PriceToBook = dto.PriceToBook,
            EnterpriseToRevenue = dto.EnterpriseToRevenue,
            EnterpriseToEbitda = dto.EnterpriseToEbitda,
            ProfitMargins = dto.ProfitMargins,
            GrossMargins = dto.GrossMargins,
            OperatingMargins = dto.OperatingMargins,
            ReturnOnAssets = dto.ReturnOnAssets,
            ReturnOnEquity = dto.ReturnOnEquity,
            Revenue = dto.Revenue,
            RevenuePerShare = dto.RevenuePerShare,
            QuarterlyRevenueGrowth = dto.QuarterlyRevenueGrowth,
            QuarterlyEarningsGrowth = dto.QuarterlyEarningsGrowth,
            TotalCash = dto.TotalCash,
            TotalCashPerShare = dto.TotalCashPerShare,
            TotalDebt = dto.TotalDebt,
            DebtToEquity = dto.DebtToEquity,
            CurrentRatio = dto.CurrentRatio,
            BookValue = dto.BookValue,
            PriceToBookValue = dto.PriceToBookValue,
            DividendRate = dto.DividendRate,
            DividendYield = dto.DividendYield,
            PayoutRatio = dto.PayoutRatio,
            Beta = dto.Beta,
            FiftyTwoWeekLow = dto.FiftyTwoWeekLow,
            FiftyTwoWeekHigh = dto.FiftyTwoWeekHigh,
            LastUpdated = dto.LastUpdated,
            FetchedAt = fetchedAt
        };
    }

    public static StockFundamentalsDto ToDto(this StockFundamentalsDocument doc)
    {
        return new StockFundamentalsDto(
            Ticker: doc.Ticker,
            CompanyName: doc.CompanyName,
            Sector: doc.Sector,
            Industry: doc.Industry,
            Description: doc.Description,
            Website: doc.Website,
            LogoUrl: doc.LogoUrl,
            CurrencyCode: doc.CurrencyCode,
            CurrencySymbol: doc.CurrencySymbol,
            CurrencyName: doc.CurrencyName,
            Highlights: null, // Not stored separately in document - individual fields are used instead
            Valuation: null, // Not stored separately in document - individual fields are used instead
            Technicals: null, // Not stored separately in document - individual fields are used instead
            SplitsDividends: null, // Not stored separately in document - individual fields are used instead
            Earnings: null, // Not stored separately in document - individual fields are used instead
            Financials: null, // Not stored separately in document - individual fields are used instead
            MarketCap: doc.MarketCap,
            EnterpriseValue: doc.EnterpriseValue,
            TrailingPE: doc.TrailingPE,
            ForwardPE: doc.ForwardPE,
            PEG: doc.PEG,
            PriceToSales: doc.PriceToSales,
            PriceToBook: doc.PriceToBook,
            EnterpriseToRevenue: doc.EnterpriseToRevenue,
            EnterpriseToEbitda: doc.EnterpriseToEbitda,
            ProfitMargins: doc.ProfitMargins,
            GrossMargins: doc.GrossMargins,
            OperatingMargins: doc.OperatingMargins,
            ReturnOnAssets: doc.ReturnOnAssets,
            ReturnOnEquity: doc.ReturnOnEquity,
            Revenue: doc.Revenue,
            RevenuePerShare: doc.RevenuePerShare,
            QuarterlyRevenueGrowth: doc.QuarterlyRevenueGrowth,
            QuarterlyEarningsGrowth: doc.QuarterlyEarningsGrowth,
            TotalCash: doc.TotalCash,
            TotalCashPerShare: doc.TotalCashPerShare,
            TotalDebt: doc.TotalDebt,
            DebtToEquity: doc.DebtToEquity,
            CurrentRatio: doc.CurrentRatio,
            BookValue: doc.BookValue,
            PriceToBookValue: doc.PriceToBookValue,
            DividendRate: doc.DividendRate,
            DividendYield: doc.DividendYield,
            PayoutRatio: doc.PayoutRatio,
            Beta: doc.Beta,
            FiftyTwoWeekLow: doc.FiftyTwoWeekLow,
            FiftyTwoWeekHigh: doc.FiftyTwoWeekHigh,
            LastUpdated: doc.LastUpdated
        );
    }
}

