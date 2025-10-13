using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;
using YahooFinanceApi;

namespace Volur.Infrastructure.ExternalProviders;

/// <summary>
/// Yahoo Finance implementation of IStockDataProvider.
/// </summary>
public sealed class YahooFinanceProvider : IStockDataProvider
{
    private readonly ILogger<YahooFinanceProvider> _logger;

    public YahooFinanceProvider(ILogger<YahooFinanceProvider> logger)
    {
        _logger = logger;
    }

    public async Task<Result<StockQuoteDto>> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching quote for ticker: {Ticker}", ticker);

            var securities = await Yahoo.Symbols(ticker).Fields(
                Field.Symbol,
                Field.RegularMarketPrice,
                Field.RegularMarketPreviousClose,
                Field.RegularMarketOpen,
                Field.RegularMarketDayHigh,
                Field.RegularMarketDayLow,
                Field.RegularMarketVolume,
                Field.AverageDailyVolume3Month,
                Field.MarketCap,
                Field.RegularMarketChange,
                Field.RegularMarketChangePercent
            ).QueryAsync();

            if (!securities.Any())
            {
                _logger.LogWarning("No data found for ticker: {Ticker}", ticker);
                return Result.Failure<StockQuoteDto>(Error.NotFound("Stock", ticker));
            }

            var security = securities.First().Value;
            var quote = new StockQuoteDto(
                Ticker: security.Symbol,
                CurrentPrice: security.RegularMarketPrice,
                PreviousClose: security.RegularMarketPreviousClose,
                Change: security.RegularMarketChange,
                ChangePercent: security.RegularMarketChangePercent,
                Open: security.RegularMarketOpen,
                High: security.RegularMarketDayHigh,
                Low: security.RegularMarketDayLow,
                Volume: security.RegularMarketVolume,
                AverageVolume: security.AverageDailyVolume3Month,
                LastUpdated: DateTime.UtcNow
            );

            _logger.LogInformation("Successfully fetched quote for {Ticker}: ${Price}", ticker, quote.CurrentPrice);
            return Result.Success(quote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quote for ticker: {Ticker}", ticker);
            return Result.Failure<StockQuoteDto>(Error.ProviderUnavailable($"Failed to fetch quote for {ticker}: {ex.Message}"));
        }
    }

    public async Task<Result<IReadOnlyList<HistoricalPriceDto>>> GetHistoricalPricesAsync(
        string ticker, 
        DateTime startDate, 
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching historical prices for {Ticker} from {StartDate} to {EndDate}", 
                ticker, startDate, endDate);

            var history = await Yahoo.GetHistoricalAsync(ticker, startDate, endDate);

            var historicalPrices = history.Select(c => new HistoricalPriceDto(
                Date: c.DateTime,
                Open: (double)c.Open,
                High: (double)c.High,
                Low: (double)c.Low,
                Close: (double)c.Close,
                Volume: c.Volume
            )).ToList();

            _logger.LogInformation("Successfully fetched {Count} historical prices for {Ticker}", 
                historicalPrices.Count, ticker);

            return Result.Success<IReadOnlyList<HistoricalPriceDto>>(historicalPrices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch historical prices for ticker: {Ticker}", ticker);
            return Result.Failure<IReadOnlyList<HistoricalPriceDto>>(
                Error.ProviderUnavailable($"Failed to fetch historical prices for {ticker}: {ex.Message}"));
        }
    }

    public async Task<Result<StockFundamentalsDto>> GetFundamentalsAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching fundamentals for ticker: {Ticker}", ticker);

            var securities = await Yahoo.Symbols(ticker).Fields(
                Field.Symbol,
                Field.LongName,
                Field.MarketCap,
                Field.TrailingPE,
                Field.ForwardPE,
                Field.BookValue,
                Field.PriceToBook,
                Field.FiftyTwoWeekLow,
                Field.FiftyTwoWeekHigh
            ).QueryAsync();

            if (!securities.Any())
            {
                _logger.LogWarning("No fundamental data found for ticker: {Ticker}", ticker);
                return Result.Failure<StockFundamentalsDto>(Error.NotFound("Stock", ticker));
            }

            var security = securities.First().Value;
            var fundamentals = new StockFundamentalsDto(
                Ticker: security.Symbol,
                CompanyName: security.LongName,
                Sector: null,
                Industry: null,
                Description: null,
                Website: null,
                LogoUrl: null,
                MarketCap: security.MarketCap,
                EnterpriseValue: null,
                TrailingPE: security.TrailingPE,
                ForwardPE: security.ForwardPE,
                PEG: null,
                PriceToSales: null,
                PriceToBook: security.PriceToBook,
                EnterpriseToRevenue: null,
                EnterpriseToEbitda: null,
                ProfitMargins: null,
                GrossMargins: null,
                OperatingMargins: null,
                ReturnOnAssets: null,
                ReturnOnEquity: null,
                Revenue: null,
                RevenuePerShare: null,
                QuarterlyRevenueGrowth: null,
                QuarterlyEarningsGrowth: null,
                TotalCash: null,
                TotalCashPerShare: null,
                TotalDebt: null,
                DebtToEquity: null,
                CurrentRatio: null,
                BookValue: security.BookValue,
                PriceToBookValue: security.PriceToBook,
                DividendRate: null,
                DividendYield: null,
                PayoutRatio: null,
                Beta: null,
                FiftyTwoWeekLow: security.FiftyTwoWeekLow,
                FiftyTwoWeekHigh: security.FiftyTwoWeekHigh,
                LastUpdated: DateTime.UtcNow
            );

            _logger.LogInformation("Successfully fetched fundamentals for {Ticker}", ticker);
            return Result.Success(fundamentals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch fundamentals for ticker: {Ticker}", ticker);
            return Result.Failure<StockFundamentalsDto>(
                Error.ProviderUnavailable($"Failed to fetch fundamentals for {ticker}: {ex.Message}"));
        }
    }

    public async Task<Result<IReadOnlyList<StockQuoteDto>>> GetQuotesAsync(IReadOnlyList<string> tickers, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching quotes for {Count} tickers", tickers.Count);

            var securities = await Yahoo.Symbols(tickers.ToArray()).Fields(
                Field.Symbol,
                Field.RegularMarketPrice,
                Field.RegularMarketPreviousClose,
                Field.RegularMarketOpen,
                Field.RegularMarketDayHigh,
                Field.RegularMarketDayLow,
                Field.RegularMarketVolume,
                Field.AverageDailyVolume3Month,
                Field.RegularMarketChange,
                Field.RegularMarketChangePercent
            ).QueryAsync();

            var quotes = securities.Select(kvp => new StockQuoteDto(
                Ticker: kvp.Value.Symbol,
                CurrentPrice: kvp.Value.RegularMarketPrice,
                PreviousClose: kvp.Value.RegularMarketPreviousClose,
                Change: kvp.Value.RegularMarketChange,
                ChangePercent: kvp.Value.RegularMarketChangePercent,
                Open: kvp.Value.RegularMarketOpen,
                High: kvp.Value.RegularMarketDayHigh,
                Low: kvp.Value.RegularMarketDayLow,
                Volume: kvp.Value.RegularMarketVolume,
                AverageVolume: kvp.Value.AverageDailyVolume3Month,
                LastUpdated: DateTime.UtcNow
            )).ToList();

            _logger.LogInformation("Successfully fetched quotes for {Count} tickers", quotes.Count);
            return Result.Success<IReadOnlyList<StockQuoteDto>>(quotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quotes for multiple tickers");
            return Result.Failure<IReadOnlyList<StockQuoteDto>>(
                Error.ProviderUnavailable($"Failed to fetch quotes: {ex.Message}"));
        }
    }
}
