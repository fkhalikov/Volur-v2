using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Infrastructure.ExternalProviders;

/// <summary>
/// EODHD implementation of IStockDataProvider.
/// </summary>
public sealed class EodhdStockDataProvider : IStockDataProvider
{
    private readonly IEodhdClient _eodhdClient;
    private readonly ILogger<EodhdStockDataProvider> _logger;

    public EodhdStockDataProvider(IEodhdClient eodhdClient, ILogger<EodhdStockDataProvider> logger)
    {
        _eodhdClient = eodhdClient;
        _logger = logger;
    }

    public async Task<Result<StockQuoteDto>> GetQuoteAsync(string ticker, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching quote for ticker: {Ticker}", ticker);

            // For now, assume US stocks on NASDAQ or NYSE
            // In a real implementation, you'd need to determine the exchange from the ticker
            var exchanges = new[] { "NASDAQ", "NYSE" };
            
            foreach (var exchange in exchanges)
            {
                var result = await _eodhdClient.GetStockQuoteAsync(ticker, exchange, cancellationToken);
                if (result.IsSuccess)
                {
                    var quote = MapToStockQuoteDto(result.Value);
                    _logger.LogInformation("Successfully fetched quote for {Ticker}: ${Price}", ticker, quote.CurrentPrice);
                    return Result.Success(quote);
                }
                
                _logger.LogDebug("Failed to get quote for {Ticker} on {Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            }

            _logger.LogWarning("No data found for ticker: {Ticker}", ticker);
            return Result.Failure<StockQuoteDto>(Error.NotFound("Stock", ticker));
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

            // For now, assume US stocks on NASDAQ or NYSE
            var exchanges = new[] { "NASDAQ", "NYSE" };
            
            foreach (var exchange in exchanges)
            {
                var result = await _eodhdClient.GetHistoricalPricesAsync(ticker, exchange, startDate, endDate, cancellationToken);
                if (result.IsSuccess)
                {
                    var historicalPrices = result.Value.Select(MapToHistoricalPriceDto).ToList();
                    _logger.LogInformation("Successfully fetched {Count} historical prices for {Ticker}", 
                        historicalPrices.Count, ticker);
                    return Result.Success<IReadOnlyList<HistoricalPriceDto>>(historicalPrices);
                }
                
                _logger.LogDebug("Failed to get historical prices for {Ticker} on {Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            }

            _logger.LogWarning("No historical data found for ticker: {Ticker}", ticker);
            return Result.Failure<IReadOnlyList<HistoricalPriceDto>>(Error.NotFound("Historical data", ticker));
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

            // For now, assume US stocks on NASDAQ or NYSE
            var exchanges = new[] { "NASDAQ", "NYSE" };
            
            foreach (var exchange in exchanges)
            {
                var result = await _eodhdClient.GetFundamentalsAsync(ticker, exchange, cancellationToken);
                if (result.IsSuccess)
                {
                    var fundamentals = MapToStockFundamentalsDto(result.Value);
                    _logger.LogInformation("Successfully fetched fundamentals for {Ticker}", ticker);
                    return Result.Success(fundamentals);
                }
                
                _logger.LogDebug("Failed to get fundamentals for {Ticker} on {Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            }

            _logger.LogWarning("No fundamental data found for ticker: {Ticker}", ticker);
            return Result.Failure<StockFundamentalsDto>(Error.NotFound("Stock fundamentals", ticker));
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

            var quotes = new List<StockQuoteDto>();
            var tasks = tickers.Select(async ticker =>
            {
                var result = await GetQuoteAsync(ticker, cancellationToken);
                if (result.IsSuccess)
                {
                    return result.Value;
                }
                return null;
            });

            var results = await Task.WhenAll(tasks);
            var successfulQuotes = results.Where(q => q != null).Cast<StockQuoteDto>().ToList();

            _logger.LogInformation("Successfully fetched quotes for {Count} tickers", successfulQuotes.Count);
            return Result.Success<IReadOnlyList<StockQuoteDto>>(successfulQuotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quotes for multiple tickers");
            return Result.Failure<IReadOnlyList<StockQuoteDto>>(
                Error.ProviderUnavailable($"Failed to fetch quotes: {ex.Message}"));
        }
    }

    private static StockQuoteDto MapToStockQuoteDto(Volur.Application.DTOs.Provider.EodhdStockQuoteDto eodhdQuote)
    {
        var change = eodhdQuote.Close.HasValue && eodhdQuote.PreviousClose.HasValue 
            ? eodhdQuote.Close.Value - eodhdQuote.PreviousClose.Value 
            : (double?)null;
        
        var changePercent = change.HasValue && eodhdQuote.PreviousClose.HasValue && eodhdQuote.PreviousClose.Value != 0
            ? (change.Value / eodhdQuote.PreviousClose.Value) * 100
            : (double?)null;

        return new StockQuoteDto(
            Ticker: eodhdQuote.Code,
            CurrentPrice: eodhdQuote.Close,
            PreviousClose: eodhdQuote.PreviousClose,
            Change: change,
            ChangePercent: changePercent,
            Open: eodhdQuote.Open,
            High: eodhdQuote.High,
            Low: eodhdQuote.Low,
            Volume: eodhdQuote.Volume,
            AverageVolume: null, // EODHD doesn't provide average volume in real-time
            LastUpdated: eodhdQuote.Timestamp ?? DateTime.UtcNow
        );
    }

    private static HistoricalPriceDto MapToHistoricalPriceDto(Volur.Application.DTOs.Provider.EodhdHistoricalPriceDto eodhdPrice)
    {
        return new HistoricalPriceDto(
            Date: eodhdPrice.Date,
            Open: eodhdPrice.Open,
            High: eodhdPrice.High,
            Low: eodhdPrice.Low,
            Close: eodhdPrice.Close,
            Volume: eodhdPrice.Volume
        );
    }

    private static StockFundamentalsDto MapToStockFundamentalsDto(Volur.Application.DTOs.Provider.EodhdFundamentalDto eodhdFundamentals)
    {
        return new StockFundamentalsDto(
            Ticker: eodhdFundamentals.Code,
            CompanyName: eodhdFundamentals.Name ?? eodhdFundamentals.General?.Name,
            Sector: eodhdFundamentals.Sector ?? eodhdFundamentals.General?.Sector,
            Industry: eodhdFundamentals.Industry ?? eodhdFundamentals.General?.Industry,
            Description: eodhdFundamentals.Description ?? eodhdFundamentals.General?.Description,
            Website: eodhdFundamentals.Website ?? eodhdFundamentals.General?.WebUrl,
            LogoUrl: eodhdFundamentals.LogoUrl ?? eodhdFundamentals.General?.LogoUrl,
            MarketCap: eodhdFundamentals.MarketCapitalization ?? eodhdFundamentals.Highlights?.MarketCapitalization,
            EnterpriseValue: null, // EODHD doesn't provide enterprise value directly
            TrailingPE: eodhdFundamentals.Highlights?.PeRatio,
            ForwardPE: null, // EODHD doesn't provide forward PE directly
            PEG: eodhdFundamentals.Highlights?.PegRatio,
            PriceToSales: eodhdFundamentals.Valuation?.PriceSalesTtm,
            PriceToBook: eodhdFundamentals.Valuation?.PriceBookMrq,
            EnterpriseToRevenue: eodhdFundamentals.Valuation?.EnterpriseValueRevenue,
            EnterpriseToEbitda: eodhdFundamentals.Valuation?.EnterpriseValueEbitda,
            ProfitMargins: eodhdFundamentals.Highlights?.ProfitMargin,
            GrossMargins: null, // EODHD doesn't provide gross margins directly
            OperatingMargins: eodhdFundamentals.Highlights?.OperatingMarginTtm,
            ReturnOnAssets: eodhdFundamentals.Highlights?.ReturnOnAssetsTtm,
            ReturnOnEquity: eodhdFundamentals.Highlights?.ReturnOnEquityTtm,
            Revenue: eodhdFundamentals.Highlights?.RevenueTtm,
            RevenuePerShare: eodhdFundamentals.Highlights?.RevenuePerShareTtm,
            QuarterlyRevenueGrowth: eodhdFundamentals.Highlights?.QuarterlyRevenueGrowthYoy,
            QuarterlyEarningsGrowth: eodhdFundamentals.Highlights?.QuarterlyEarningsGrowthYoy,
            TotalCash: null, // EODHD doesn't provide cash data directly
            TotalCashPerShare: null,
            TotalDebt: null, // EODHD doesn't provide debt data directly
            DebtToEquity: null,
            CurrentRatio: null, // EODHD doesn't provide current ratio directly
            BookValue: eodhdFundamentals.Highlights?.BookValue,
            PriceToBookValue: eodhdFundamentals.Valuation?.PriceBookMrq,
            DividendRate: eodhdFundamentals.Highlights?.DividendShare,
            DividendYield: eodhdFundamentals.Highlights?.DividendYield,
            PayoutRatio: null, // EODHD doesn't provide payout ratio directly
            Beta: eodhdFundamentals.Technicals?.Beta,
            FiftyTwoWeekLow: eodhdFundamentals.Technicals?.FiftyTwoWeekLow,
            FiftyTwoWeekHigh: eodhdFundamentals.Technicals?.FiftyTwoWeekHigh,
            LastUpdated: DateTime.UtcNow
        );
    }
}
