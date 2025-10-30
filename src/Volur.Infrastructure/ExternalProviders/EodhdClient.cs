using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volur.Application.Configuration;
using Volur.Application.DTOs.Provider;
using Volur.Application.DTOs;
using Volur.Application.Interfaces;
using Volur.Shared;

namespace Volur.Infrastructure.ExternalProviders;

/// <summary>
/// HTTP client for EODHD market data API.
/// </summary>
public sealed class EodhdClient : IEodhdClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EodhdClient> _logger;
    private readonly EodhdOptions _options;
    private readonly RateLimiter _rateLimiter;

    public EodhdClient(HttpClient httpClient, IOptions<EodhdOptions> options, ILogger<EodhdClient> logger, RateLimiter rateLimiter)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _rateLimiter = rateLimiter;
    }

    public async Task<Result<IReadOnlyList<EodhdExchangeDto>>> GetExchangesAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = "api/exchanges-list/";
        var url = BuildUrl(endpoint);

        return await ExecuteRequestAsync<EodhdExchangeDto>(url, endpoint, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<EodhdSymbolDto>>> GetSymbolsAsync(string exchangeCode, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/exchange-symbol-list/{Uri.EscapeDataString(exchangeCode)}";
        var url = BuildUrl(endpoint);

        return await ExecuteRequestAsync<EodhdSymbolDto>(url, endpoint, cancellationToken);
    }

    public async Task<Result<EodhdStockQuoteDto>> GetStockQuoteAsync(string ticker, string exchange, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/real-time/{Uri.EscapeDataString(ticker)}.{Uri.EscapeDataString(exchange)}";
        var url = BuildUrl(endpoint);

        return await ExecuteSingleRequestAsync<EodhdStockQuoteDto>(url, endpoint, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<EodhdHistoricalPriceDto>>> GetHistoricalPricesAsync(
        string ticker, 
        string exchange, 
        DateTime from, 
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var fromStr = from.ToString("yyyy-MM-dd");
        var toStr = to.ToString("yyyy-MM-dd");
        var endpoint = $"api/eod/{Uri.EscapeDataString(ticker)}.{Uri.EscapeDataString(exchange)}?from={fromStr}&to={toStr}";
        var url = BuildUrl(endpoint);

        return await ExecuteRequestAsync<EodhdHistoricalPriceDto>(url, endpoint, cancellationToken);
    }

    public async Task<Result<EodhdFundamentalDto>> GetFundamentalsAsync(string ticker, string exchange, CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/fundamentals/{Uri.EscapeDataString(ticker)}.{Uri.EscapeDataString(exchange)}";
        var url = BuildUrl(endpoint);

        return await ExecuteSingleRequestAsync<EodhdFundamentalDto>(url, endpoint, cancellationToken);
    }

    private string BuildUrl(string endpoint)
    {
        return $"{endpoint}?api_token={_options.ApiToken}&fmt=json";
    }

    private async Task<Result<IReadOnlyList<T>>> ExecuteRequestAsync<T>(string url, string endpoint, CancellationToken cancellationToken) where T : class
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Wait for rate limit permit before making request
            // AcquireAsync will wait until tokens are available (up to 1 minute for replenishment)
            var waitStartTime = DateTime.UtcNow;
            using var lease = await _rateLimiter.AcquireAsync(permitCount: 1, cancellationToken);
            
            // Only check IsAcquired for cancellation - AcquireAsync waits indefinitely for tokens
            if (!lease.IsAcquired)
            {
                // This only happens if cancellation was requested
                _logger.LogInformation("EODHD rate limit permit acquisition cancelled for {Endpoint}", endpoint);
                throw new OperationCanceledException("Request was cancelled while waiting for rate limit permit", cancellationToken);
            }

            var waitTime = DateTime.UtcNow - waitStartTime;
            if (waitTime.TotalMilliseconds > 100)
            {
                _logger.LogInformation("EODHD rate limit wait completed: waited {WaitMs}ms for permit for {Endpoint}", 
                    waitTime.TotalMilliseconds, endpoint);
            }

            _logger.LogInformation("Requesting EODHD: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var elapsed = DateTime.UtcNow - startTime;

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60;
                
                // Check if this is a daily limit vs rate limit
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var isDailyLimit = IsDailyLimitResponse(responseContent);
                
                if (isDailyLimit)
                {
                    _logger.LogWarning("EODHD daily limit exceeded. Quota reset tomorrow.");
                    return Result.Failure<IReadOnlyList<T>>(Error.ProviderDailyLimit("Daily API quota exceeded. Try again tomorrow."));
                }
                else
                {
                    _logger.LogWarning("EODHD rate limit hit. Retry after {Seconds}s", retryAfter);
                    return Result.Failure<IReadOnlyList<T>>(Error.ProviderRateLimit($"Rate limit exceeded. Retry after {retryAfter} seconds."));
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("EODHD request failed: {StatusCode} {Endpoint} ({ElapsedMs}ms)", 
                    response.StatusCode, endpoint, elapsed.TotalMilliseconds);
                
                return Result.Failure<IReadOnlyList<T>>(
                    Error.ProviderUnavailable($"Provider returned {response.StatusCode}: {response.ReasonPhrase}"));
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<List<T>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null)
            {
                _logger.LogError("Failed to deserialize EODHD response for {Endpoint}", endpoint);
                return Result.Failure<IReadOnlyList<T>>(Error.ProviderUnavailable("Failed to parse provider response."));
            }

            _logger.LogInformation("EODHD request successful: {Endpoint} - {Count} items ({ElapsedMs}ms)", 
                endpoint, data.Count, elapsed.TotalMilliseconds);

            return Result.Success<IReadOnlyList<T>>(data);
        }
        catch (TaskCanceledException)
        {
            // HTTP client timeout
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogWarning("EODHD request timeout: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<IReadOnlyList<T>>(Error.ProviderUnavailable("Request timeout."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Request was cancelled while waiting for rate limit or during request
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation("EODHD request cancelled: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            throw; // Re-throw cancellation to propagate properly
        }
        catch (OperationCanceledException)
        {
            // Other cancellation scenarios (timeout, etc.)
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogWarning("EODHD request cancelled/timeout: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<IReadOnlyList<T>>(Error.ProviderUnavailable("Request cancelled or timed out."));
        }
        catch (HttpRequestException ex)
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "EODHD request failed: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<IReadOnlyList<T>>(Error.ProviderUnavailable($"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Unexpected error calling EODHD: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<IReadOnlyList<T>>(Error.InternalError($"Unexpected error: {ex.Message}"));
        }
    }

    private async Task<Result<T>> ExecuteSingleRequestAsync<T>(string url, string endpoint, CancellationToken cancellationToken) where T : class
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Wait for rate limit permit before making request
            // AcquireAsync will wait until tokens are available (up to 1 minute for replenishment)
            var waitStartTime = DateTime.UtcNow;
            using var lease = await _rateLimiter.AcquireAsync(permitCount: 1, cancellationToken);
            
            // Only check IsAcquired for cancellation - AcquireAsync waits indefinitely for tokens
            if (!lease.IsAcquired)
            {
                // This only happens if cancellation was requested
                _logger.LogInformation("EODHD rate limit permit acquisition cancelled for {Endpoint}", endpoint);
                throw new OperationCanceledException("Request was cancelled while waiting for rate limit permit", cancellationToken);
            }

            var waitTime = DateTime.UtcNow - waitStartTime;
            if (waitTime.TotalMilliseconds > 100)
            {
                _logger.LogInformation("EODHD rate limit wait completed: waited {WaitMs}ms for permit for {Endpoint}", 
                    waitTime.TotalMilliseconds, endpoint);
            }

            _logger.LogInformation("Requesting EODHD: {Endpoint}", endpoint);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var elapsed = DateTime.UtcNow - startTime;

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60;
                
                // Check if this is a daily limit vs rate limit
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var isDailyLimit = IsDailyLimitResponse(responseContent);
                
                if (isDailyLimit)
                {
                    _logger.LogWarning("EODHD daily limit exceeded. Quota reset tomorrow.");
                    return Result.Failure<T>(Error.ProviderDailyLimit("Daily API quota exceeded. Try again tomorrow."));
                }
                else
                {
                    _logger.LogWarning("EODHD rate limit hit. Retry after {Seconds}s", retryAfter);
                    return Result.Failure<T>(Error.ProviderRateLimit($"Rate limit exceeded. Retry after {retryAfter} seconds."));
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("EODHD request failed: {StatusCode} {Endpoint} ({ElapsedMs}ms)", 
                    response.StatusCode, endpoint, elapsed.TotalMilliseconds);
                
                return Result.Failure<T>(
                    Error.ProviderUnavailable($"Provider returned {response.StatusCode}: {response.ReasonPhrase}"));
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data == null)
            {
                _logger.LogError("Failed to deserialize EODHD response for {Endpoint}", endpoint);
                return Result.Failure<T>(Error.ProviderUnavailable("Failed to parse provider response."));
            }

            _logger.LogInformation("EODHD request successful: {Endpoint} ({ElapsedMs}ms)", 
                endpoint, elapsed.TotalMilliseconds);

            return Result.Success(data);
        }
        catch (TaskCanceledException)
        {
            // HTTP client timeout
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogWarning("EODHD request timeout: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<T>(Error.ProviderUnavailable("Request timeout."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Request was cancelled while waiting for rate limit or during request
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation("EODHD request cancelled: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            throw; // Re-throw cancellation to propagate properly
        }
        catch (OperationCanceledException)
        {
            // Other cancellation scenarios (timeout, etc.)
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogWarning("EODHD request cancelled/timeout: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<T>(Error.ProviderUnavailable("Request cancelled or timed out."));
        }
        catch (HttpRequestException ex)
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "EODHD request failed: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<T>(Error.ProviderUnavailable($"HTTP request failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Unexpected error calling EODHD: {Endpoint} ({ElapsedMs}ms)", endpoint, elapsed.TotalMilliseconds);
            return Result.Failure<T>(Error.InternalError($"Unexpected error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Determines if the response indicates a daily limit vs rate limit.
    /// EODHD typically returns different messages for daily quota vs rate limits.
    /// </summary>
    private static bool IsDailyLimitResponse(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
            return false;

        var content = responseContent.ToLowerInvariant();
        
        // Common daily limit indicators from EODHD
        return content.Contains("daily") || 
               content.Contains("quota") || 
               content.Contains("limit exceeded") ||
               content.Contains("maximum requests") ||
               content.Contains("per day") ||
               content.Contains("daily limit");
    }

    public void Dispose()
    {
        // Rate limiter is injected as a singleton and managed by DI container
        // Do not dispose it here as it's shared across all instances
    }
}

