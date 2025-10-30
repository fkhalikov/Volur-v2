using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Domain.Entities;
using Volur.Infrastructure.Persistence;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of stock data repository.
/// </summary>
public sealed class StockDataRepository : IStockDataRepository
{
    private readonly VolurDbContext _context;
    private readonly ILogger<StockDataRepository> _logger;
    private readonly ISymbolRepository _symbolRepository;

    public StockDataRepository(
        VolurDbContext context, 
        ILogger<StockDataRepository> logger,
        ISymbolRepository symbolRepository)
    {
        _context = context;
        _logger = logger;
        _symbolRepository = symbolRepository;
    }

    public async Task<(StockQuoteDto quote, DateTime fetchedAt)?> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.StockQuotes
                .FirstOrDefaultAsync(q => q.Ticker == ticker.ToUpperInvariant(), cancellationToken);

            if (entity == null)
            {
                _logger.LogDebug("No cached quote found for ticker: {Ticker}", ticker);
                return null;
            }

            var dto = new StockQuoteDto(
                Ticker: entity.Ticker,
                CurrentPrice: entity.CurrentPrice,
                PreviousClose: entity.PreviousClose,
                Change: entity.Change,
                ChangePercent: entity.ChangePercent,
                Open: entity.Open,
                High: entity.High,
                Low: entity.Low,
                Volume: entity.Volume,
                AverageVolume: entity.AverageVolume,
                LastUpdated: entity.LastUpdated
            );

            _logger.LogDebug("Retrieved cached quote for {Ticker}, fetched at {FetchedAt}", ticker, entity.UpdatedAt);
            
            return (dto, entity.UpdatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached quote for ticker: {Ticker}", ticker);
            return null;
        }
    }

    public async Task UpsertQuoteAsync(StockQuoteDto quote, CancellationToken cancellationToken = default)
    {
        try
        {
            var ticker = quote.Ticker.ToUpperInvariant();
            var entity = await _context.StockQuotes
                .FirstOrDefaultAsync(q => q.Ticker == ticker, cancellationToken);

            if (entity != null)
            {
                // Update existing
                entity.CurrentPrice = quote.CurrentPrice;
                entity.PreviousClose = quote.PreviousClose;
                entity.Change = quote.Change;
                entity.ChangePercent = quote.ChangePercent;
                entity.Open = quote.Open;
                entity.High = quote.High;
                entity.Low = quote.Low;
                entity.Volume = quote.Volume;
                entity.AverageVolume = quote.AverageVolume;
                entity.LastUpdated = quote.LastUpdated;
            }
            else
            {
                // Insert new
                entity = new StockQuoteEntity
                {
                    Ticker = ticker,
                    CurrentPrice = quote.CurrentPrice,
                    PreviousClose = quote.PreviousClose,
                    Change = quote.Change,
                    ChangePercent = quote.ChangePercent,
                    Open = quote.Open,
                    High = quote.High,
                    Low = quote.Low,
                    Volume = quote.Volume,
                    AverageVolume = quote.AverageVolume,
                    LastUpdated = quote.LastUpdated
                };
                _context.StockQuotes.Add(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update denormalized fields in SymbolEntity for efficient sorting (best-effort, don't fail if this fails)
            try
            {
                await _symbolRepository.UpdateDenormalizedFieldsAsync(
                    quote.Ticker,
                    currentPrice: quote.CurrentPrice,
                    changePercent: quote.ChangePercent,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update denormalized fields for {Ticker} after quote update", quote.Ticker);
            }

            _logger.LogDebug("Cached quote for {Ticker}", quote.Ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache quote for ticker: {Ticker}", quote.Ticker);
            throw;
        }
    }

    public async Task<(StockFundamentalsDto fundamentals, DateTime fetchedAt)?> GetFundamentalsAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.StockFundamentals
                .FirstOrDefaultAsync(f => f.Ticker == ticker.ToUpperInvariant(), cancellationToken);

            if (entity == null)
            {
                _logger.LogDebug("No cached fundamentals found for ticker: {Ticker}", ticker);
                return null;
            }

            var dto = new StockFundamentalsDto(
                Ticker: entity.Ticker,
                CompanyName: entity.CompanyName,
                Sector: entity.Sector,
                Industry: entity.Industry,
                Description: entity.Description,
                Website: entity.Website,
                LogoUrl: entity.LogoUrl,
                CurrencyCode: entity.CurrencyCode,
                CurrencySymbol: entity.CurrencySymbol,
                CurrencyName: entity.CurrencyName,
                Highlights: null, // Not stored separately - individual fields are used
                Valuation: null, // Not stored separately - individual fields are used
                Technicals: null, // Not stored separately - individual fields are used
                SplitsDividends: null, // Not stored separately - individual fields are used
                Earnings: null, // Not stored separately - individual fields are used
                Financials: null, // Not stored separately - individual fields are used
                MarketCap: entity.MarketCap,
                EnterpriseValue: entity.EnterpriseValue,
                TrailingPE: entity.TrailingPE,
                ForwardPE: entity.ForwardPE,
                PEG: entity.PEG,
                PriceToSales: entity.PriceToSales,
                PriceToBook: entity.PriceToBook,
                EnterpriseToRevenue: entity.EnterpriseToRevenue,
                EnterpriseToEbitda: entity.EnterpriseToEbitda,
                ProfitMargins: entity.ProfitMargins,
                GrossMargins: entity.GrossMargins,
                OperatingMargins: entity.OperatingMargins,
                ReturnOnAssets: entity.ReturnOnAssets,
                ReturnOnEquity: entity.ReturnOnEquity,
                Revenue: entity.Revenue,
                RevenuePerShare: entity.RevenuePerShare,
                QuarterlyRevenueGrowth: entity.QuarterlyRevenueGrowth,
                QuarterlyEarningsGrowth: entity.QuarterlyEarningsGrowth,
                TotalCash: entity.TotalCash,
                TotalCashPerShare: entity.TotalCashPerShare,
                TotalDebt: entity.TotalDebt,
                DebtToEquity: entity.DebtToEquity,
                CurrentRatio: entity.CurrentRatio,
                BookValue: entity.BookValue,
                PriceToBookValue: entity.PriceToBookValue,
                DividendRate: entity.DividendRate,
                DividendYield: entity.DividendYield,
                PayoutRatio: entity.PayoutRatio,
                Beta: entity.Beta,
                FiftyTwoWeekLow: entity.FiftyTwoWeekLow,
                FiftyTwoWeekHigh: entity.FiftyTwoWeekHigh,
                LastUpdated: entity.LastUpdated
            );

            _logger.LogDebug("Retrieved cached fundamentals for {Ticker}, fetched at {FetchedAt}", ticker, entity.UpdatedAt);
            
            return (dto, entity.UpdatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached fundamentals for ticker: {Ticker}", ticker);
            return null;
        }
    }

    public async Task UpsertFundamentalsAsync(StockFundamentalsDto fundamentals, CancellationToken cancellationToken = default)
    {
        try
        {
            var ticker = fundamentals.Ticker.ToUpperInvariant();
            var entity = await _context.StockFundamentals
                .FirstOrDefaultAsync(f => f.Ticker == ticker, cancellationToken);

            if (entity != null)
            {
                // Update existing
                entity.CompanyName = fundamentals.CompanyName;
                entity.Sector = fundamentals.Sector;
                entity.Industry = fundamentals.Industry;
                entity.Description = fundamentals.Description;
                entity.Website = fundamentals.Website;
                entity.LogoUrl = fundamentals.LogoUrl;
                entity.CurrencyCode = fundamentals.CurrencyCode;
                entity.CurrencySymbol = fundamentals.CurrencySymbol;
                entity.CurrencyName = fundamentals.CurrencyName;
                entity.MarketCap = fundamentals.MarketCap;
                entity.EnterpriseValue = fundamentals.EnterpriseValue;
                entity.TrailingPE = fundamentals.TrailingPE;
                entity.ForwardPE = fundamentals.ForwardPE;
                entity.PEG = fundamentals.PEG;
                entity.PriceToSales = fundamentals.PriceToSales;
                entity.PriceToBook = fundamentals.PriceToBook;
                entity.EnterpriseToRevenue = fundamentals.EnterpriseToRevenue;
                entity.EnterpriseToEbitda = fundamentals.EnterpriseToEbitda;
                entity.ProfitMargins = fundamentals.ProfitMargins;
                entity.GrossMargins = fundamentals.GrossMargins;
                entity.OperatingMargins = fundamentals.OperatingMargins;
                entity.ReturnOnAssets = fundamentals.ReturnOnAssets;
                entity.ReturnOnEquity = fundamentals.ReturnOnEquity;
                entity.Revenue = fundamentals.Revenue;
                entity.RevenuePerShare = fundamentals.RevenuePerShare;
                entity.QuarterlyRevenueGrowth = fundamentals.QuarterlyRevenueGrowth;
                entity.QuarterlyEarningsGrowth = fundamentals.QuarterlyEarningsGrowth;
                entity.TotalCash = fundamentals.TotalCash;
                entity.TotalCashPerShare = fundamentals.TotalCashPerShare;
                entity.TotalDebt = fundamentals.TotalDebt;
                entity.DebtToEquity = fundamentals.DebtToEquity;
                entity.CurrentRatio = fundamentals.CurrentRatio;
                entity.BookValue = fundamentals.BookValue;
                entity.PriceToBookValue = fundamentals.PriceToBookValue;
                entity.DividendRate = fundamentals.DividendRate;
                entity.DividendYield = fundamentals.DividendYield;
                entity.PayoutRatio = fundamentals.PayoutRatio;
                entity.Beta = fundamentals.Beta;
                entity.FiftyTwoWeekLow = fundamentals.FiftyTwoWeekLow;
                entity.FiftyTwoWeekHigh = fundamentals.FiftyTwoWeekHigh;
                entity.LastUpdated = fundamentals.LastUpdated;
            }
            else
            {
                // Insert new
                entity = new StockFundamentalsEntity
                {
                    Ticker = ticker,
                    CompanyName = fundamentals.CompanyName,
                    Sector = fundamentals.Sector,
                    Industry = fundamentals.Industry,
                    Description = fundamentals.Description,
                    Website = fundamentals.Website,
                    LogoUrl = fundamentals.LogoUrl,
                    CurrencyCode = fundamentals.CurrencyCode,
                    CurrencySymbol = fundamentals.CurrencySymbol,
                    CurrencyName = fundamentals.CurrencyName,
                    MarketCap = fundamentals.MarketCap,
                    EnterpriseValue = fundamentals.EnterpriseValue,
                    TrailingPE = fundamentals.TrailingPE,
                    ForwardPE = fundamentals.ForwardPE,
                    PEG = fundamentals.PEG,
                    PriceToSales = fundamentals.PriceToSales,
                    PriceToBook = fundamentals.PriceToBook,
                    EnterpriseToRevenue = fundamentals.EnterpriseToRevenue,
                    EnterpriseToEbitda = fundamentals.EnterpriseToEbitda,
                    ProfitMargins = fundamentals.ProfitMargins,
                    GrossMargins = fundamentals.GrossMargins,
                    OperatingMargins = fundamentals.OperatingMargins,
                    ReturnOnAssets = fundamentals.ReturnOnAssets,
                    ReturnOnEquity = fundamentals.ReturnOnEquity,
                    Revenue = fundamentals.Revenue,
                    RevenuePerShare = fundamentals.RevenuePerShare,
                    QuarterlyRevenueGrowth = fundamentals.QuarterlyRevenueGrowth,
                    QuarterlyEarningsGrowth = fundamentals.QuarterlyEarningsGrowth,
                    TotalCash = fundamentals.TotalCash,
                    TotalCashPerShare = fundamentals.TotalCashPerShare,
                    TotalDebt = fundamentals.TotalDebt,
                    DebtToEquity = fundamentals.DebtToEquity,
                    CurrentRatio = fundamentals.CurrentRatio,
                    BookValue = fundamentals.BookValue,
                    PriceToBookValue = fundamentals.PriceToBookValue,
                    DividendRate = fundamentals.DividendRate,
                    DividendYield = fundamentals.DividendYield,
                    PayoutRatio = fundamentals.PayoutRatio,
                    Beta = fundamentals.Beta,
                    FiftyTwoWeekLow = fundamentals.FiftyTwoWeekLow,
                    FiftyTwoWeekHigh = fundamentals.FiftyTwoWeekHigh,
                    LastUpdated = fundamentals.LastUpdated
                };
                _context.StockFundamentals.Add(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update denormalized fields in SymbolEntity for efficient sorting (best-effort, don't fail if this fails)
            try
            {
                await _symbolRepository.UpdateDenormalizedFieldsAsync(
                    fundamentals.Ticker,
                    trailingPE: fundamentals.TrailingPE,
                    marketCap: fundamentals.MarketCap,
                    dividendYield: fundamentals.DividendYield,
                    sector: fundamentals.Sector,
                    industry: fundamentals.Industry,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update denormalized fields for {Ticker} after fundamentals update", fundamentals.Ticker);
            }

            _logger.LogDebug("Cached fundamentals for {Ticker}", fundamentals.Ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache fundamentals for ticker: {Ticker}", fundamentals.Ticker);
            throw;
        }
    }

    public async Task<bool> IsNoDataAvailableAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.NoDataAvailable
                .FirstOrDefaultAsync(n => 
                    n.Ticker == ticker.ToUpperInvariant() && 
                    n.ExchangeCode == exchangeCode.ToUpperInvariant(), 
                    cancellationToken);
            
            var isMarked = entity != null;
            _logger.LogDebug("NoDataAvailable check for {Ticker}.{ExchangeCode}: {IsMarked}", ticker, exchangeCode, isMarked);
            
            return isMarked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check NoDataAvailable for {Ticker}.{ExchangeCode}", ticker, exchangeCode);
            return false; // Default to allowing the request on error
        }
    }

    public async Task MarkAsNoDataAvailableAsync(string ticker, string exchangeCode, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var tickerUpper = ticker.ToUpperInvariant();
            var exchangeCodeUpper = exchangeCode.ToUpperInvariant();

            var entity = await _context.NoDataAvailable
                .FirstOrDefaultAsync(n => 
                    n.Ticker == tickerUpper && 
                    n.ExchangeCode == exchangeCodeUpper, 
                    cancellationToken);

            if (entity != null)
            {
                // Update existing
                entity.FailureCount++;
                entity.LastAttemptedAt = DateTime.UtcNow;
                entity.LastErrorMessage = errorMessage;
            }
            else
            {
                // Create new
                entity = new NoDataAvailableEntity
                {
                    Ticker = tickerUpper,
                    ExchangeCode = exchangeCodeUpper,
                    FailureCount = 1,
                    FirstFailedAt = DateTime.UtcNow,
                    LastAttemptedAt = DateTime.UtcNow,
                    LastErrorMessage = errorMessage
                };
                _context.NoDataAvailable.Add(entity);
            }

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Marked {Ticker}.{ExchangeCode} as NoDataAvailable, failure count: {FailureCount}", 
                ticker, exchangeCode, entity.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark {Ticker}.{ExchangeCode} as NoDataAvailable", ticker, exchangeCode);
            // Don't throw - this is not critical for the main operation
        }
    }

    public async Task RemoveNoDataAvailableAsync(string ticker, string exchangeCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.NoDataAvailable
                .FirstOrDefaultAsync(n => 
                    n.Ticker == ticker.ToUpperInvariant() && 
                    n.ExchangeCode == exchangeCode.ToUpperInvariant(), 
                    cancellationToken);

            if (entity != null)
            {
                entity.SoftDelete();
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Removed {Ticker}.{ExchangeCode} from NoDataAvailable list", ticker, exchangeCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove {Ticker}.{ExchangeCode} from NoDataAvailable list", ticker, exchangeCode);
            // Don't throw - this is not critical for the main operation
        }
    }

    public async Task<IReadOnlyList<(string ticker, string exchangeCode)>> GetNoDataAvailableForExchangeAsync(string exchangeCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.NoDataAvailable
                .Where(n => n.ExchangeCode == exchangeCode.ToUpperInvariant())
                .Select(n => new { n.Ticker, n.ExchangeCode })
                .ToListAsync(cancellationToken);

            var result = entities.Select(e => (e.Ticker, e.ExchangeCode)).ToList();
            
            _logger.LogDebug("Found {Count} NoDataAvailable entries for exchange {ExchangeCode}", result.Count, exchangeCode);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get NoDataAvailable entries for exchange {ExchangeCode}", exchangeCode);
            return Array.Empty<(string, string)>();
        }
    }
}
