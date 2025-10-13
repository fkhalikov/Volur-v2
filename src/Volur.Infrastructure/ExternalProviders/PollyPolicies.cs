using Polly;
using Polly.Extensions.Http;

namespace Volur.Infrastructure.ExternalProviders;

/// <summary>
/// Polly resilience policies for HTTP clients.
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Retry policy with exponential backoff.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Logging handled by the client
                });
    }

    /// <summary>
    /// Circuit breaker policy.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Timeout policy.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int seconds = 10)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(seconds));
    }
}

