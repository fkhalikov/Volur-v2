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

            // Check if ticker is in full symbol format (e.g., "0A00.LSE")
            if (ticker.Contains('.'))
            {
                var parts = ticker.Split('.');
                if (parts.Length == 2)
                {
                    var tickerOnly = parts[0];
                    var exchange = parts[1];
                    var normalizedExchange = NormalizeExchangeForEodhd(exchange);
                    _logger.LogDebug("Parsed full symbol {FullSymbol} -> ticker: {Ticker}, exchange: {Exchange} -> normalized: {NormalizedExchange}", ticker, tickerOnly, exchange, normalizedExchange);
                    
                    var result = await _eodhdClient.GetStockQuoteAsync(tickerOnly, normalizedExchange, cancellationToken);
                    if (result.IsSuccess)
                    {
                        var quote = MapToStockQuoteDto(result.Value);
                        _logger.LogInformation("Successfully fetched quote for {FullSymbol}: ${Price}", ticker, quote.CurrentPrice);
                        return Result.Success(quote);
                    }
                    
                    _logger.LogError("Failed to get quote for {FullSymbol}: {Error}", ticker, result.Error?.Message ?? "Unknown error");
                    return Result.Failure<StockQuoteDto>(Error.ProviderUnavailable($"Failed to fetch quote for {ticker}: {result.Error?.Message ?? "Unknown error"}"));
                }
                else
                {
                    _logger.LogError("Invalid ticker format: {Ticker}. Expected format: TICKER.EXCHANGE", ticker);
                    return Result.Failure<StockQuoteDto>(Error.Validation($"Invalid ticker format: {ticker}. Expected format: TICKER.EXCHANGE"));
                }
            }
            else
            {
                _logger.LogError("No exchange specified for ticker: {Ticker}. Use format TICKER.EXCHANGE", ticker);
                return Result.Failure<StockQuoteDto>(Error.Validation($"No exchange specified for ticker: {ticker}. Use format TICKER.EXCHANGE"));
            }
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
            var normalizedExchange = NormalizeExchangeForEodhd(exchange);
            _logger.LogInformation("Fetching quote for ticker: {Ticker} on exchange: {Exchange} -> normalized: {NormalizedExchange}", ticker, exchange, normalizedExchange);

            var result = await _eodhdClient.GetStockQuoteAsync(ticker, normalizedExchange, cancellationToken);
            if (result.IsSuccess)
            {
                var quote = MapToStockQuoteDto(result.Value);
                _logger.LogInformation("Successfully fetched quote for {Ticker}.{Exchange}: ${Price}", ticker, exchange, quote.CurrentPrice);
                return Result.Success(quote);
            }

            _logger.LogError("Failed to get quote for {Ticker}.{Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            return Result.Failure<StockQuoteDto>(Error.ProviderUnavailable($"Failed to fetch quote for {ticker}.{exchange}: {result.Error?.Message ?? "Unknown error"}"));
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

            // Check if ticker is in full symbol format (e.g., "0A00.LSE")
            if (ticker.Contains('.'))
            {
                var parts = ticker.Split('.');
                if (parts.Length == 2)
                {
                    var tickerOnly = parts[0];
                    var exchange = parts[1];
                    var normalizedExchange = NormalizeExchangeForEodhd(exchange);
                    _logger.LogDebug("Parsed full symbol {FullSymbol} -> ticker: {Ticker}, exchange: {Exchange} -> normalized: {NormalizedExchange}", ticker, tickerOnly, exchange, normalizedExchange);
                    
                    var result = await _eodhdClient.GetHistoricalPricesAsync(tickerOnly, normalizedExchange, startDate, endDate, cancellationToken);
                    if (result.IsSuccess)
                    {
                        var historicalPrices = result.Value.Select(MapToHistoricalPriceDto).ToList();
                        _logger.LogInformation("Successfully fetched {Count} historical prices for {FullSymbol}", 
                            historicalPrices.Count, ticker);
                        return Result.Success<IReadOnlyList<HistoricalPriceDto>>(historicalPrices);
                    }
                    
                    _logger.LogError("Failed to get historical prices for {FullSymbol}: {Error}", ticker, result.Error?.Message ?? "Unknown error");
                    return Result.Failure<IReadOnlyList<HistoricalPriceDto>>(Error.ProviderUnavailable($"Failed to fetch historical prices for {ticker}: {result.Error?.Message ?? "Unknown error"}"));
                }
                else
                {
                    _logger.LogError("Invalid ticker format: {Ticker}. Expected format: TICKER.EXCHANGE", ticker);
                    return Result.Failure<IReadOnlyList<HistoricalPriceDto>>(Error.Validation($"Invalid ticker format: {ticker}. Expected format: TICKER.EXCHANGE"));
                }
            }
            else
            {
                _logger.LogError("No exchange specified for ticker: {Ticker}. Use format TICKER.EXCHANGE", ticker);
                return Result.Failure<IReadOnlyList<HistoricalPriceDto>>(Error.Validation($"No exchange specified for ticker: {ticker}. Use format TICKER.EXCHANGE"));
            }
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

            // Check if ticker is in full symbol format (e.g., "0A00.LSE")
            if (ticker.Contains('.'))
            {
                var parts = ticker.Split('.');
                if (parts.Length == 2)
                {
                    var tickerOnly = parts[0];
                    var exchange = parts[1];
                    var normalizedExchange = NormalizeExchangeForEodhd(exchange);
                    _logger.LogDebug("Parsed full symbol {FullSymbol} -> ticker: {Ticker}, exchange: {Exchange} -> normalized: {NormalizedExchange}", ticker, tickerOnly, exchange, normalizedExchange);
                    
                    var result = await _eodhdClient.GetFundamentalsAsync(tickerOnly, normalizedExchange, cancellationToken);
                    if (result.IsSuccess)
                    {
                        var fundamentals = MapToStockFundamentalsDto(result.Value);
                        _logger.LogInformation("Successfully fetched fundamentals for {FullSymbol}", ticker);
                        return Result.Success(fundamentals);
                    }
                    
                    _logger.LogError("Failed to get fundamentals for {FullSymbol}: {Error}", ticker, result.Error?.Message ?? "Unknown error");
                    return Result.Failure<StockFundamentalsDto>(Error.ProviderUnavailable($"Failed to fetch fundamentals for {ticker}: {result.Error?.Message ?? "Unknown error"}"));
                }
                else
                {
                    _logger.LogError("Invalid ticker format: {Ticker}. Expected format: TICKER.EXCHANGE", ticker);
                    return Result.Failure<StockFundamentalsDto>(Error.Validation($"Invalid ticker format: {ticker}. Expected format: TICKER.EXCHANGE"));
                }
            }
            else
            {
                _logger.LogError("No exchange specified for ticker: {Ticker}. Use format TICKER.EXCHANGE", ticker);
                return Result.Failure<StockFundamentalsDto>(Error.Validation($"No exchange specified for ticker: {ticker}. Use format TICKER.EXCHANGE"));
            }
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
            var normalizedExchange = NormalizeExchangeForEodhd(exchange);
            _logger.LogInformation("Fetching fundamentals for ticker: {Ticker} on exchange: {Exchange} -> normalized: {NormalizedExchange}", ticker, exchange, normalizedExchange);

            var result = await _eodhdClient.GetFundamentalsAsync(ticker, normalizedExchange, cancellationToken);
            if (result.IsSuccess)
            {
                var fundamentals = MapToStockFundamentalsDto(result.Value);
                _logger.LogInformation("Successfully fetched fundamentals for {Ticker}.{Exchange}", ticker, exchange);
                return Result.Success(fundamentals);
            }

            _logger.LogError("Failed to get fundamentals for {Ticker}.{Exchange}: {Error}", ticker, exchange, result.Error?.Message ?? "Unknown error");
            return Result.Failure<StockFundamentalsDto>(Error.ProviderUnavailable($"Failed to fetch fundamentals for {ticker}.{exchange}: {result.Error?.Message ?? "Unknown error"}"));
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

            // Validate all tickers are in FullSymbol format before processing
            foreach (var ticker in tickers)
            {
                if (!ticker.Contains('.'))
                {
                    _logger.LogError("Invalid ticker format: {Ticker}. Expected format: TICKER.EXCHANGE", ticker);
                    return Result.Failure<IReadOnlyList<StockQuoteDto>>(Error.Validation($"Invalid ticker format: {ticker}. Expected format: TICKER.EXCHANGE"));
                }
            }

            var quotes = new List<StockQuoteDto>();
            var tasks = tickers.Select(async ticker =>
            {
                var result = await GetQuoteAsync(ticker, cancellationToken);
                if (result.IsSuccess)
                {
                    return result.Value;
                }
                _logger.LogError("Failed to fetch quote for {Ticker}: {Error}", ticker, result.Error?.Message ?? "Unknown error");
                return null;
            });

            var results = await Task.WhenAll(tasks);
            var successfulQuotes = results.Where(q => q != null).Cast<StockQuoteDto>().ToList();

            if (successfulQuotes.Count == 0)
            {
                _logger.LogError("Failed to fetch quotes for any tickers");
                return Result.Failure<IReadOnlyList<StockQuoteDto>>(Error.ProviderUnavailable("Failed to fetch quotes for any tickers"));
            }

            _logger.LogInformation("Successfully fetched quotes for {Count} of {Total} tickers", successfulQuotes.Count, tickers.Count);
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
        return new StockQuoteDto(
            Ticker: eodhdQuote.Code,
            CurrentPrice: eodhdQuote.Close,
            PreviousClose: eodhdQuote.PreviousClose,
            Change: eodhdQuote.Change,
            ChangePercent: eodhdQuote.ChangeP,
            Open: eodhdQuote.Open,
            High: eodhdQuote.High,
            Low: eodhdQuote.Low,
            Volume: eodhdQuote.Volume,
            AverageVolume: null, // EODHD doesn't provide average volume in real-time
            LastUpdated: DateTimeOffset.FromUnixTimeSeconds(eodhdQuote.Timestamp).UtcDateTime
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
            Ticker: eodhdFundamentals.General?.Code ?? string.Empty,
            CompanyName: eodhdFundamentals.General?.Name,
            Sector: eodhdFundamentals.General?.GicSector ?? eodhdFundamentals.General?.Sector,
            Industry: eodhdFundamentals.General?.GicIndustry ?? eodhdFundamentals.General?.Industry,
            Description: eodhdFundamentals.General?.Description,
            Website: eodhdFundamentals.General?.WebUrl,
            LogoUrl: eodhdFundamentals.General?.LogoUrl,
            CurrencyCode: eodhdFundamentals.General?.CurrencyCode,
            CurrencySymbol: eodhdFundamentals.General?.CurrencySymbol,
            CurrencyName: eodhdFundamentals.General?.CurrencyName,
            Highlights: eodhdFundamentals.Highlights == null ? null : new StockHighlightsDto(
                MarketCapitalization: eodhdFundamentals.Highlights.MarketCapitalization,
                MarketCapitalizationMln: eodhdFundamentals.Highlights.MarketCapitalizationMln,
                Ebitda: eodhdFundamentals.Highlights.Ebitda,
                PeRatio: eodhdFundamentals.Highlights.PeRatio,
                PegRatio: eodhdFundamentals.Highlights.PegRatio,
                WallStreetTargetPrice: eodhdFundamentals.Highlights.WallStreetTargetPrice,
                BookValue: eodhdFundamentals.Highlights.BookValue,
                DividendShare: eodhdFundamentals.Highlights.DividendShare,
                DividendYield: eodhdFundamentals.Highlights.DividendYield,
                EarningsShare: eodhdFundamentals.Highlights.EarningsShare,
                EpsEstimateCurrentYear: eodhdFundamentals.Highlights.EpsEstimateCurrentYear,
                EpsEstimateNextYear: eodhdFundamentals.Highlights.EpsEstimateNextYear,
                EpsEstimateNextQuarter: eodhdFundamentals.Highlights.EpsEstimateNextQuarter,
                EpsEstimateCurrentQuarter: eodhdFundamentals.Highlights.EpsEstimateCurrentQuarter,
                MostRecentQuarter: eodhdFundamentals.Highlights.MostRecentQuarter,
                ProfitMargin: eodhdFundamentals.Highlights.ProfitMargin,
                OperatingMarginTtm: eodhdFundamentals.Highlights.OperatingMarginTtm,
                ReturnOnAssetsTtm: eodhdFundamentals.Highlights.ReturnOnAssetsTtm,
                ReturnOnEquityTtm: eodhdFundamentals.Highlights.ReturnOnEquityTtm,
                RevenueTtm: eodhdFundamentals.Highlights.RevenueTtm,
                RevenuePerShareTtm: eodhdFundamentals.Highlights.RevenuePerShareTtm,
                QuarterlyRevenueGrowthYoy: eodhdFundamentals.Highlights.QuarterlyRevenueGrowthYoy,
                GrossProfitTtm: eodhdFundamentals.Highlights.GrossProfitTtm,
                DilutedEpsTtm: eodhdFundamentals.Highlights.DilutedEpsTtm,
                QuarterlyEarningsGrowthYoy: eodhdFundamentals.Highlights.QuarterlyEarningsGrowthYoy
            ),
            Valuation: eodhdFundamentals.Valuation == null ? null : new StockValuationDto(
                TrailingPe: eodhdFundamentals.Valuation.TrailingPe,
                ForwardPe: eodhdFundamentals.Valuation.ForwardPe,
                PriceSalesTtm: eodhdFundamentals.Valuation.PriceSalesTtm,
                PriceBookMrq: eodhdFundamentals.Valuation.PriceBookMrq,
                EnterpriseValue: eodhdFundamentals.Valuation.EnterpriseValue,
                EnterpriseValueRevenue: eodhdFundamentals.Valuation.EnterpriseValueRevenue,
                EnterpriseValueEbitda: eodhdFundamentals.Valuation.EnterpriseValueEbitda
            ),
            Technicals: eodhdFundamentals.Technicals == null ? null : new StockTechnicalsDto(
                Beta: eodhdFundamentals.Technicals.Beta,
                FiftyTwoWeekHigh: eodhdFundamentals.Technicals.FiftyTwoWeekHigh,
                FiftyTwoWeekLow: eodhdFundamentals.Technicals.FiftyTwoWeekLow,
                FiftyDayMa: eodhdFundamentals.Technicals.FiftyDayMa,
                TwoHundredDayMa: eodhdFundamentals.Technicals.TwoHundredDayMa
            ),
            SplitsDividends: eodhdFundamentals.SplitsDividends == null ? null : new StockSplitsDividendsDto(
                PayoutRatio: eodhdFundamentals.SplitsDividends.PayoutRatio,
                DividendDate: eodhdFundamentals.SplitsDividends.DividendDate,
                ExDividendDate: eodhdFundamentals.SplitsDividends.ExDividendDate,
                DividendPerShare: eodhdFundamentals.SplitsDividends.ForwardAnnualDividendRate,
                DividendYield: eodhdFundamentals.SplitsDividends.ForwardAnnualDividendYield,
                NumberDividendsByYear: null
            ),
            Earnings: null,
            Financials: null,
            MarketCap: eodhdFundamentals.Highlights?.MarketCapitalization,
            EnterpriseValue: eodhdFundamentals.Valuation?.EnterpriseValue,
            TrailingPE: eodhdFundamentals.Valuation?.TrailingPe,
            ForwardPE: eodhdFundamentals.Valuation?.ForwardPe,
            PEG: eodhdFundamentals.Highlights?.PegRatio,
            PriceToSales: eodhdFundamentals.Valuation?.PriceSalesTtm,
            PriceToBook: eodhdFundamentals.Valuation?.PriceBookMrq,
            EnterpriseToRevenue: eodhdFundamentals.Valuation?.EnterpriseValueRevenue,
            EnterpriseToEbitda: eodhdFundamentals.Valuation?.EnterpriseValueEbitda,
            ProfitMargins: eodhdFundamentals.Highlights?.ProfitMargin,
            GrossMargins: null,
            OperatingMargins: eodhdFundamentals.Highlights?.OperatingMarginTtm,
            ReturnOnAssets: eodhdFundamentals.Highlights?.ReturnOnAssetsTtm,
            ReturnOnEquity: eodhdFundamentals.Highlights?.ReturnOnEquityTtm,
            Revenue: eodhdFundamentals.Highlights?.RevenueTtm,
            RevenuePerShare: eodhdFundamentals.Highlights?.RevenuePerShareTtm,
            QuarterlyRevenueGrowth: eodhdFundamentals.Highlights?.QuarterlyRevenueGrowthYoy,
            QuarterlyEarningsGrowth: eodhdFundamentals.Highlights?.QuarterlyEarningsGrowthYoy,
            TotalCash: null,
            TotalCashPerShare: null,
            TotalDebt: null,
            DebtToEquity: null,
            CurrentRatio: null,
            BookValue: eodhdFundamentals.Highlights?.BookValue,
            PriceToBookValue: eodhdFundamentals.Valuation?.PriceBookMrq,
            DividendRate: eodhdFundamentals.Highlights?.DividendShare,
            DividendYield: eodhdFundamentals.Highlights?.DividendYield,
            PayoutRatio: eodhdFundamentals.SplitsDividends?.PayoutRatio,
            Beta: eodhdFundamentals.Technicals?.Beta,
            FiftyTwoWeekLow: eodhdFundamentals.Technicals?.FiftyTwoWeekLow,
            FiftyTwoWeekHigh: eodhdFundamentals.Technicals?.FiftyTwoWeekHigh,
            LastUpdated: DateTime.TryParse(eodhdFundamentals.General?.UpdatedAt, out var updatedAt) ? updatedAt : DateTime.UtcNow
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
            "0A00" => ("Akzo Nobel N.V.", "Materials", "Specialty Chemicals", 83_400_000_000.0),
            "0A05" => ("Medacta Group S.A.", "Healthcare", "Medical Devices", 90_100_000_000.0),
            "0A0C" => ("Stadler Rail AG", "Industrials", "Rail Equipment", 37_900_000_000.0),
            "0A0D" => ("Alcon Inc.", "Healthcare", "Medical Equipment", 46_700_000_000.0),
            "0A0F" => ("Citycon Oyj", "Real Estate", "REITs", 1_200_000_000.0),
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
            CurrencyCode: "USD",
            CurrencySymbol: "$",
            CurrencyName: "US Dollar",
            Highlights: new StockHighlightsDto(
                MarketCapitalization: (long?)marketCap,
                MarketCapitalizationMln: marketCap / 1_000_000,
                Ebitda: (long?)(marketCap * 0.15),
                PeRatio: 15.0 + random.NextDouble() * 20,
                PegRatio: 1.0 + random.NextDouble() * 2,
                WallStreetTargetPrice: null,
                BookValue: 10.0 + random.NextDouble() * 50,
                DividendShare: 1.0 + random.NextDouble() * 5,
                DividendYield: 0.01 + random.NextDouble() * 0.05,
                EarningsShare: 5.0 + random.NextDouble() * 15,
                EpsEstimateCurrentYear: 5.5 + random.NextDouble() * 15,
                EpsEstimateNextYear: 6.0 + random.NextDouble() * 16,
                EpsEstimateNextQuarter: 1.3 + random.NextDouble() * 4,
                EpsEstimateCurrentQuarter: 1.2 + random.NextDouble() * 4,
                MostRecentQuarter: "2024-09-30",
                ProfitMargin: 0.05 + random.NextDouble() * 0.25,
                OperatingMarginTtm: 0.08 + random.NextDouble() * 0.20,
                ReturnOnAssetsTtm: 0.03 + random.NextDouble() * 0.15,
                ReturnOnEquityTtm: 0.08 + random.NextDouble() * 0.25,
                RevenueTtm: (long?)(marketCap * 2),
                RevenuePerShareTtm: 50.0 + random.NextDouble() * 200,
                QuarterlyRevenueGrowthYoy: -0.05 + random.NextDouble() * 0.20,
                GrossProfitTtm: (long?)(marketCap * 0.8),
                DilutedEpsTtm: 5.0 + random.NextDouble() * 15,
                QuarterlyEarningsGrowthYoy: -0.10 + random.NextDouble() * 0.30
            ),
            Valuation: new StockValuationDto(
                TrailingPe: 15.0 + random.NextDouble() * 20,
                ForwardPe: 12.0 + random.NextDouble() * 18,
                PriceSalesTtm: 1.0 + random.NextDouble() * 5,
                PriceBookMrq: 1.0 + random.NextDouble() * 4,
                EnterpriseValue: (long?)(marketCap * 1.1),
                EnterpriseValueRevenue: 2.0 + random.NextDouble() * 8,
                EnterpriseValueEbitda: 8.0 + random.NextDouble() * 15
            ),
            Technicals: new StockTechnicalsDto(
                Beta: 0.5 + random.NextDouble() * 2,
                FiftyTwoWeekHigh: 100.0 + random.NextDouble() * 200,
                FiftyTwoWeekLow: 50.0 + random.NextDouble() * 100,
                FiftyDayMa: 80.0 + random.NextDouble() * 120,
                TwoHundredDayMa: 70.0 + random.NextDouble() * 130
            ),
            SplitsDividends: new StockSplitsDividendsDto(
                PayoutRatio: 0.20 + random.NextDouble() * 0.60,
                DividendDate: null,
                ExDividendDate: null,
                DividendPerShare: 1.0 + random.NextDouble() * 5,
                DividendYield: 0.01 + random.NextDouble() * 0.05,
                NumberDividendsByYear: 4
            ),
            Earnings: null, // Complex structure - will implement later
            Financials: null, // Complex structure - will implement later
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

    /// <summary>
    /// Normalizes exchange codes to EODHD's expected format.
    /// For US stocks, EODHD expects "US" rather than specific exchanges like NYSE, NASDAQ.
    /// </summary>
    private static string NormalizeExchangeForEodhd(string exchange)
    {
        return exchange.ToUpperInvariant() switch
        {
            // US Exchanges - all map to "US"
            "NYSE" or "NASDAQ" or "AMEX" or "BATS" or "ARCA" or "OTC" or "OTCBB" or "PINK" => "US",
            
            // Keep other exchanges as-is (LSE, TSE, etc.)
            _ => exchange
        };
    }
}
