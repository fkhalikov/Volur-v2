using Microsoft.Extensions.Logging;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Application.Mappers;
using Volur.Shared;

namespace Volur.Application.UseCases.GetStockDetails;

/// <summary>
/// Handler for GetStockDetailsQuery.
/// Implements read-through cache pattern for stock data.
/// </summary>
public sealed class GetStockDetailsHandler
{
    private readonly ISymbolRepository _symbolRepository;
    private readonly IStockDataRepository _stockDataRepository;
    private readonly IStockDataProvider _stockDataProvider;
    private readonly ILogger<GetStockDetailsHandler> _logger;

    public GetStockDetailsHandler(
        ISymbolRepository symbolRepository,
        IStockDataRepository stockDataRepository,
        IStockDataProvider stockDataProvider,
        ILogger<GetStockDetailsHandler> logger)
    {
        _symbolRepository = symbolRepository;
        _stockDataRepository = stockDataRepository;
        _stockDataProvider = stockDataProvider;
        _logger = logger;
    }

    public async Task<Result<StockDetailsResponse>> HandleAsync(GetStockDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var ticker = query.Ticker.ToUpperInvariant();
        _logger.LogInformation("Getting stock details for ticker: {Ticker}, ForceRefresh: {ForceRefresh}", ticker, query.ForceRefresh);

        // First, try to find the symbol to get basic info
        var symbolResult = await FindSymbolAsync(ticker, cancellationToken);
        if (symbolResult.IsFailure)
        {
            return Result.Failure<StockDetailsResponse>(symbolResult.Error!);
        }

        var symbol = symbolResult.Value;
        StockQuoteDto? quote = null;
        StockFundamentalsDto? fundamentals = null;
        DateTime? quoteFetchedAt = null;
        DateTime? fundamentalsFetchedAt = null;

        // Get quote data (cached or fresh)
        var quoteResult = await GetQuoteDataAsync(symbol, query.ForceRefresh, cancellationToken);
        if (quoteResult.IsSuccess)
        {
            quote = quoteResult.Value.quote;
            quoteFetchedAt = quoteResult.Value.fetchedAt;
        }
        else
        {
            _logger.LogWarning("Failed to get quote data for {Ticker}: {Error}", ticker, quoteResult.Error);
        }

        // Get fundamentals data (cached or fresh)
        var fundamentalsResult = await GetFundamentalsDataAsync(symbol, query.ForceRefresh, cancellationToken);
        if (fundamentalsResult.IsSuccess)
        {
            fundamentals = fundamentalsResult.Value.fundamentals;
            fundamentalsFetchedAt = fundamentalsResult.Value.fetchedAt;
        }
        else
        {
            _logger.LogWarning("Failed to get fundamentals data for {Ticker}: {Error}", ticker, fundamentalsResult.Error);
        }

        var response = new StockDetailsResponse(
            Symbol: symbol.ToDto(),
            Quote: quote,
            Fundamentals: fundamentals,
            QuoteFetchedAt: quoteFetchedAt,
            FundamentalsFetchedAt: fundamentalsFetchedAt,
            RequestedAt: DateTime.UtcNow
        );

        _logger.LogInformation("Successfully retrieved stock details for {Ticker}. Quote: {HasQuote}, Fundamentals: {HasFundamentals}", 
            ticker, quote != null, fundamentals != null);

        return Result.Success(response);
    }

    private async Task<Result<Domain.Entities.Symbol>> FindSymbolAsync(string ticker, CancellationToken cancellationToken)
    {
        // Try to find symbol by ticker across all exchanges
        // This is a simplified approach - in a real system you might want to specify exchange
        var symbol = await _symbolRepository.GetByTickerAsync(ticker, cancellationToken);
        
        if (symbol != null)
        {
            return Result.Success(symbol);
        }

        // If not found in cache, create a minimal symbol object
        // This allows the system to work even if symbol data isn't cached
        var minimalSymbol = new Domain.Entities.Symbol(
            Ticker: ticker,
            ExchangeCode: "UNKNOWN",
            ParentExchange: "UNKNOWN",
            Name: ticker,
            Type: "Common Stock",
            Isin: null,
            Currency: "USD",
            IsActive: true
        );

        _logger.LogWarning("Symbol {Ticker} not found in cache, using minimal symbol data", ticker);
        return Result.Success(minimalSymbol);
    }

    private async Task<Result<(StockQuoteDto quote, DateTime fetchedAt)>> GetQuoteDataAsync(
        Domain.Entities.Symbol symbol, 
        bool forceRefresh, 
        CancellationToken cancellationToken)
    {
        var ticker = symbol.Ticker;
        
        // Try cache first unless force refresh
        if (!forceRefresh)
        {
            var cached = await _stockDataRepository.GetQuoteAsync(ticker, cancellationToken);
            if (cached.HasValue)
            {
                _logger.LogDebug("Quote cache hit for {Ticker}, fetched at {FetchedAt}", ticker, cached.Value.fetchedAt);
                return Result.Success(cached.Value);
            }
        }

        // Fetch from provider using specific exchange
        _logger.LogDebug("Fetching quote from provider for {Ticker}.{Exchange}", ticker, symbol.ExchangeCode);
        var providerResult = await _stockDataProvider.GetQuoteAsync(ticker, symbol.ExchangeCode, cancellationToken);
        
        if (providerResult.IsFailure)
        {
            return Result.Failure<(StockQuoteDto, DateTime)>(providerResult.Error!);
        }

        var quote = providerResult.Value;
        var fetchedAt = DateTime.UtcNow;

        // Cache the result (fire-and-forget with error logging)
        try
        {
            await _stockDataRepository.UpsertQuoteAsync(quote, cancellationToken);
            _logger.LogDebug("Cached quote for {Ticker}", ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache quote for {Ticker}", ticker);
            // Continue - we have the data from provider
        }

        return Result.Success((quote, fetchedAt));
    }

    private async Task<Result<(StockFundamentalsDto fundamentals, DateTime fetchedAt)>> GetFundamentalsDataAsync(
        Domain.Entities.Symbol symbol, 
        bool forceRefresh, 
        CancellationToken cancellationToken)
    {
        var ticker = symbol.Ticker;
        
        // Try cache first unless force refresh
        if (!forceRefresh)
        {
            var cached = await _stockDataRepository.GetFundamentalsAsync(ticker, cancellationToken);
            if (cached.HasValue)
            {
                _logger.LogDebug("Fundamentals cache hit for {Ticker}, fetched at {FetchedAt}", ticker, cached.Value.fetchedAt);
                return Result.Success(cached.Value);
            }
        }

        // Fetch from provider using specific exchange
        _logger.LogDebug("Fetching fundamentals from provider for {Ticker}.{Exchange}", ticker, symbol.ExchangeCode);
        var providerResult = await _stockDataProvider.GetFundamentalsAsync(ticker, symbol.ExchangeCode, cancellationToken);
        
        if (providerResult.IsFailure)
        {
            return Result.Failure<(StockFundamentalsDto, DateTime)>(providerResult.Error!);
        }

        var fundamentals = providerResult.Value;
        var fetchedAt = DateTime.UtcNow;

        // Cache the result (fire-and-forget with error logging)
        try
        {
            await _stockDataRepository.UpsertFundamentalsAsync(fundamentals, cancellationToken);
            _logger.LogDebug("Cached fundamentals for {Ticker}", ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache fundamentals for {Ticker}", ticker);
            // Continue - we have the data from provider
        }

        return Result.Success((fundamentals, fetchedAt));
    }
}
