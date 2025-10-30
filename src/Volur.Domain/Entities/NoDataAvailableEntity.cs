namespace Volur.Domain.Entities;

/// <summary>
/// SQL Server entity for tracking stocks that don't have fundamental data available.
/// </summary>
public sealed class NoDataAvailableEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the stock ticker symbol.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange code for the stock.
    /// </summary>
    public string ExchangeCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of consecutive times this stock has failed to return data.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets when this stock was first marked as having no data available.
    /// </summary>
    public DateTime FirstFailedAt { get; set; }

    /// <summary>
    /// Gets or sets when this stock was last attempted to be fetched.
    /// </summary>
    public DateTime LastAttemptedAt { get; set; }

    /// <summary>
    /// Gets or sets the optional error message from the last failed attempt.
    /// </summary>
    public string? LastErrorMessage { get; set; }
}

