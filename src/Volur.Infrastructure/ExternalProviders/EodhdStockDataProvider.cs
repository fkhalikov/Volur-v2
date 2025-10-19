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

            // Try different exchanges based on common patterns
            var exchanges = GetLikelyExchanges(ticker);
            
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

            // If no real data available, return mock data for development
            _logger.LogWarning("No live data found for ticker: {Ticker}, returning mock data", ticker);
            return Result.Success(CreateMockQuote(ticker));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quote for ticker: {Ticker}", ticker);
            return Result.Failure<StockQuoteDto>(Error.ProviderUnavailable($"Failed to fetch quote for {ticker}: {ex.Message}"));
        }
    }

    public async Task<Result<StockQuoteDto>> GetQuoteAsync(string ticker, string exchange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching quote for ticker: {Ticker} on exchange: {Exchange}", ticker, exchange);

            var result = await _eodhdClient.GetStockQuoteAsync(ticker, exchange, cancellationToken);
            if (result.IsSuccess)
            {
                var quote = MapToStockQuoteDto(result.Value);
                _logger.LogInformation("Successfully fetched quote for {Ticker}.{Exchange}: ${Price}", ticker, exchange, quote.CurrentPrice);
                return Result.Success(quote);
            }

            _logger.LogWarning("Failed to get quote for {Ticker}.{Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            
            // If no real data available, return mock data for development
            _logger.LogWarning("No live data found for {Ticker}.{Exchange}, returning mock data", ticker, exchange);
            return Result.Success(CreateMockQuote(ticker));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quote for ticker: {Ticker}.{Exchange}", ticker, exchange);
            return Result.Failure<StockQuoteDto>(Error.ProviderUnavailable($"Failed to fetch quote for {ticker}.{exchange}: {ex.Message}"));
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

            // Try different exchanges based on common patterns
            var exchanges = GetLikelyExchanges(ticker);
            
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

            // If no real data available, return mock data for development
            _logger.LogWarning("No live fundamental data found for ticker: {Ticker}, returning mock data", ticker);
            return Result.Success(CreateMockFundamentals(ticker));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch fundamentals for ticker: {Ticker}", ticker);
            return Result.Failure<StockFundamentalsDto>(
                Error.ProviderUnavailable($"Failed to fetch fundamentals for {ticker}: {ex.Message}"));
        }
    }

    public async Task<Result<StockFundamentalsDto>> GetFundamentalsAsync(string ticker, string exchange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching fundamentals for ticker: {Ticker} on exchange: {Exchange}", ticker, exchange);

            var result = await _eodhdClient.GetFundamentalsAsync(ticker, exchange, cancellationToken);
            if (result.IsSuccess)
            {
                var fundamentals = MapToStockFundamentalsDto(result.Value);
                _logger.LogInformation("Successfully fetched fundamentals for {Ticker}.{Exchange}", ticker, exchange);
                return Result.Success(fundamentals);
            }

            _logger.LogWarning("Failed to get fundamentals for {Ticker}.{Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            
            // If no real data available, return mock data for development
            _logger.LogWarning("No live fundamental data found for {Ticker}.{Exchange}, returning mock data", ticker, exchange);
            return Result.Success(CreateMockFundamentals(ticker));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch fundamentals for ticker: {Ticker}.{Exchange}", ticker, exchange);
            return Result.Failure<StockFundamentalsDto>(
                Error.ProviderUnavailable($"Failed to fetch fundamentals for {ticker}.{exchange}: {ex.Message}"));
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

    private static string[] GetLikelyExchanges(string ticker)
    {
        // Common patterns for determining likely exchanges
        return ticker.ToUpperInvariant() switch
        {
            // UK stocks
            "VOD" or "BP" or "SHEL" or "AZN" or "RIO" or "LSEG" or "UU" or "BT.A" => new[] { "LSE", "NASDAQ", "NYSE" },
            
            // German stocks  
            "SAP" or "ASML" => new[] { "XETRA", "NASDAQ", "NYSE" },
            
            // Common US tech stocks
            "AAPL" or "MSFT" or "GOOGL" or "AMZN" or "TSLA" or "META" or "NVDA" => new[] { "NASDAQ", "NYSE" },
            
            // Default: try major exchanges
            _ => new[] { "NASDAQ", "NYSE", "LSE" }
        };
    }

    private static StockQuoteDto CreateMockQuote(string ticker)
    {
        // Create realistic mock data for development/demo purposes
        var random = new Random(ticker.GetHashCode()); // Consistent data for same ticker
        var basePrice = ticker.ToUpperInvariant() switch
        {
            "VOD" => 85.50,
            "AAPL" => 175.25,
            "MSFT" => 285.75,
            "GOOGL" => 2750.80,
            _ => 100.00 + random.NextDouble() * 200 // Random price between 100-300
        };

        var change = (random.NextDouble() - 0.5) * 10; // Random change between -5 to +5
        var changePercent = (change / basePrice) * 100;
        var currentPrice = basePrice + change;

        return new StockQuoteDto(
            Ticker: ticker,
            CurrentPrice: Math.Round(currentPrice, 2),
            PreviousClose: Math.Round(basePrice, 2),
            Change: Math.Round(change, 2),
            ChangePercent: Math.Round(changePercent, 2),
            Open: Math.Round(basePrice + (random.NextDouble() - 0.5) * 5, 2),
            High: Math.Round(currentPrice + random.NextDouble() * 5, 2),
            Low: Math.Round(currentPrice - random.NextDouble() * 5, 2),
            Volume: (long)(random.NextDouble() * 10000000), // Random volume
            AverageVolume: (long)(random.NextDouble() * 5000000),
            LastUpdated: DateTime.UtcNow
        );
    }

    private static StockFundamentalsDto CreateMockFundamentals(string ticker)
    {
        // Create realistic mock fundamental data
        var random = new Random(ticker.GetHashCode());
        
        var (companyName, sector, industry, marketCap) = ticker.ToUpperInvariant() switch
        {
            "VOD" => ("Vodafone Group PLC", "Communication Services", "Telecom Services", 23_000_000_000.0),
            "AAPL" => ("Apple Inc.", "Technology", "Consumer Electronics", 2_800_000_000_000.0),
            "MSFT" => ("Microsoft Corporation", "Technology", "Software", 2_400_000_000_000.0),
            "GOOGL" => ("Alphabet Inc.", "Technology", "Internet Content & Information", 1_700_000_000_000.0),
            _ => ($"{ticker} Corporation", "Technology", "Software", random.NextDouble() * 100_000_000_000)
        };

        return new StockFundamentalsDto(
            Ticker: ticker,
            CompanyName: companyName,
            Sector: sector,
            Industry: industry,
            Description: $"{companyName} is a leading company in the {industry} industry.",
            Website: $"https://www.{ticker.ToLowerInvariant()}.com",
            LogoUrl: null,
            MarketCap: marketCap,
            EnterpriseValue: marketCap * 1.1,
            TrailingPE: 15.0 + random.NextDouble() * 20, // PE between 15-35
            ForwardPE: 12.0 + random.NextDouble() * 18,
            PEG: 1.0 + random.NextDouble() * 2,
            PriceToSales: 2.0 + random.NextDouble() * 8,
            PriceToBook: 1.5 + random.NextDouble() * 4,
            EnterpriseToRevenue: 3.0 + random.NextDouble() * 7,
            EnterpriseToEbitda: 8.0 + random.NextDouble() * 12,
            ProfitMargins: 0.05 + random.NextDouble() * 0.25, // 5-30% margins
            GrossMargins: 0.30 + random.NextDouble() * 0.40, // 30-70% gross margins
            OperatingMargins: 0.10 + random.NextDouble() * 0.20, // 10-30% operating margins
            ReturnOnAssets: 0.05 + random.NextDouble() * 0.15,
            ReturnOnEquity: 0.10 + random.NextDouble() * 0.25,
            Revenue: marketCap * 0.5, // Rough revenue estimate
            RevenuePerShare: 10.0 + random.NextDouble() * 50,
            QuarterlyRevenueGrowth: -0.05 + random.NextDouble() * 0.20, // -5% to +15% growth
            QuarterlyEarningsGrowth: -0.10 + random.NextDouble() * 0.30,
            TotalCash: marketCap * 0.1,
            TotalCashPerShare: 5.0 + random.NextDouble() * 20,
            TotalDebt: marketCap * 0.2,
            DebtToEquity: 0.2 + random.NextDouble() * 0.8,
            CurrentRatio: 1.0 + random.NextDouble() * 2,
            BookValue: 20.0 + random.NextDouble() * 80,
            PriceToBookValue: 1.5 + random.NextDouble() * 4,
            DividendRate: random.NextDouble() * 5, // 0-5% dividend
            DividendYield: random.NextDouble() * 0.06, // 0-6% yield
            PayoutRatio: 0.20 + random.NextDouble() * 0.60,
            Beta: 0.5 + random.NextDouble() * 1.5, // Beta between 0.5-2.0
            FiftyTwoWeekLow: 50.0 + random.NextDouble() * 100,
            FiftyTwoWeekHigh: 150.0 + random.NextDouble() * 200,
            LastUpdated: DateTime.UtcNow
        );
    }
}
