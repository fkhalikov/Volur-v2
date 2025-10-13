namespace Volur.Shared;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Common errors
    public static Error NotFound(string entityName, string identifier) =>
        new("NOT_FOUND", $"{entityName} with identifier '{identifier}' was not found.");

    public static Error Validation(string message) =>
        new("VALIDATION_ERROR", message);

    public static Error ProviderUnavailable(string message) =>
        new("PROVIDER_UNAVAILABLE", message);

    public static Error ProviderRateLimit(string message) =>
        new("PROVIDER_RATE_LIMIT", message);

    public static Error CacheWriteFailed(string message) =>
        new("CACHE_WRITE_FAILED", message);

    public static Error BadExchangeCode(string code) =>
        new("BAD_EXCHANGE_CODE", $"Exchange code '{code}' is not valid or not found.");

    public static Error InternalError(string message) =>
        new("INTERNAL_ERROR", message);
}

