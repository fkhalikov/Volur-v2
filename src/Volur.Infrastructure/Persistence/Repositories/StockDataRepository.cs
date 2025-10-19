using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Infrastructure.Persistence.Mappers;
using Volur.Infrastructure.Persistence.Models;

namespace Volur.Infrastructure.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of stock data repository.
/// </summary>
public sealed class StockDataRepository : IStockDataRepository
{
    private readonly MongoDbContext _context;
    private readonly ILogger<StockDataRepository> _logger;

    public StockDataRepository(MongoDbContext context, ILogger<StockDataRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(StockQuoteDto quote, DateTime fetchedAt)?> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<StockQuoteDocument>.Filter.Eq(x => x.Ticker, ticker.ToUpperInvariant());
            var document = await _context.StockQuotes.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (document == null)
            {
                _logger.LogDebug("No cached quote found for ticker: {Ticker}", ticker);
                return null;
            }

            var dto = document.ToDto();
            _logger.LogDebug("Retrieved cached quote for {Ticker}, fetched at {FetchedAt}", ticker, document.FetchedAt);
            
            return (dto, document.FetchedAt);
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
            var fetchedAt = DateTime.UtcNow;
            var document = quote.ToDocument(fetchedAt);
            
            var filter = Builders<StockQuoteDocument>.Filter.Eq(x => x.Ticker, quote.Ticker.ToUpperInvariant());
            var update = Builders<StockQuoteDocument>.Update
                .Set(x => x.Ticker, document.Ticker)
                .Set(x => x.CurrentPrice, document.CurrentPrice)
                .Set(x => x.PreviousClose, document.PreviousClose)
                .Set(x => x.Change, document.Change)
                .Set(x => x.ChangePercent, document.ChangePercent)
                .Set(x => x.Open, document.Open)
                .Set(x => x.High, document.High)
                .Set(x => x.Low, document.Low)
                .Set(x => x.Volume, document.Volume)
                .Set(x => x.AverageVolume, document.AverageVolume)
                .Set(x => x.LastUpdated, document.LastUpdated)
                .Set(x => x.FetchedAt, document.FetchedAt);

            await _context.StockQuotes.UpdateOneAsync(
                filter, 
                update, 
                new UpdateOptions { IsUpsert = true }, 
                cancellationToken);

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
            var filter = Builders<StockFundamentalsDocument>.Filter.Eq(x => x.Ticker, ticker.ToUpperInvariant());
            var document = await _context.StockFundamentals.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (document == null)
            {
                _logger.LogDebug("No cached fundamentals found for ticker: {Ticker}", ticker);
                return null;
            }

            var dto = document.ToDto();
            _logger.LogDebug("Retrieved cached fundamentals for {Ticker}, fetched at {FetchedAt}", ticker, document.FetchedAt);
            
            return (dto, document.FetchedAt);
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
            var fetchedAt = DateTime.UtcNow;
            var document = fundamentals.ToDocument(fetchedAt);
            
            var filter = Builders<StockFundamentalsDocument>.Filter.Eq(x => x.Ticker, fundamentals.Ticker.ToUpperInvariant());
            var update = Builders<StockFundamentalsDocument>.Update
                .Set(x => x.Ticker, document.Ticker)
                .Set(x => x.CompanyName, document.CompanyName)
                .Set(x => x.Sector, document.Sector)
                .Set(x => x.Industry, document.Industry)
                .Set(x => x.Description, document.Description)
                .Set(x => x.Website, document.Website)
                .Set(x => x.LogoUrl, document.LogoUrl)
                .Set(x => x.MarketCap, document.MarketCap)
                .Set(x => x.EnterpriseValue, document.EnterpriseValue)
                .Set(x => x.TrailingPE, document.TrailingPE)
                .Set(x => x.ForwardPE, document.ForwardPE)
                .Set(x => x.PEG, document.PEG)
                .Set(x => x.PriceToSales, document.PriceToSales)
                .Set(x => x.PriceToBook, document.PriceToBook)
                .Set(x => x.EnterpriseToRevenue, document.EnterpriseToRevenue)
                .Set(x => x.EnterpriseToEbitda, document.EnterpriseToEbitda)
                .Set(x => x.ProfitMargins, document.ProfitMargins)
                .Set(x => x.GrossMargins, document.GrossMargins)
                .Set(x => x.OperatingMargins, document.OperatingMargins)
                .Set(x => x.ReturnOnAssets, document.ReturnOnAssets)
                .Set(x => x.ReturnOnEquity, document.ReturnOnEquity)
                .Set(x => x.Revenue, document.Revenue)
                .Set(x => x.RevenuePerShare, document.RevenuePerShare)
                .Set(x => x.QuarterlyRevenueGrowth, document.QuarterlyRevenueGrowth)
                .Set(x => x.QuarterlyEarningsGrowth, document.QuarterlyEarningsGrowth)
                .Set(x => x.TotalCash, document.TotalCash)
                .Set(x => x.TotalCashPerShare, document.TotalCashPerShare)
                .Set(x => x.TotalDebt, document.TotalDebt)
                .Set(x => x.DebtToEquity, document.DebtToEquity)
                .Set(x => x.CurrentRatio, document.CurrentRatio)
                .Set(x => x.BookValue, document.BookValue)
                .Set(x => x.PriceToBookValue, document.PriceToBookValue)
                .Set(x => x.DividendRate, document.DividendRate)
                .Set(x => x.DividendYield, document.DividendYield)
                .Set(x => x.PayoutRatio, document.PayoutRatio)
                .Set(x => x.Beta, document.Beta)
                .Set(x => x.FiftyTwoWeekLow, document.FiftyTwoWeekLow)
                .Set(x => x.FiftyTwoWeekHigh, document.FiftyTwoWeekHigh)
                .Set(x => x.LastUpdated, document.LastUpdated)
                .Set(x => x.FetchedAt, document.FetchedAt);

            await _context.StockFundamentals.UpdateOneAsync(
                filter, 
                update, 
                new UpdateOptions { IsUpsert = true }, 
                cancellationToken);

            _logger.LogDebug("Cached fundamentals for {Ticker}", fundamentals.Ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache fundamentals for ticker: {Ticker}", fundamentals.Ticker);
            throw;
        }
    }
}
