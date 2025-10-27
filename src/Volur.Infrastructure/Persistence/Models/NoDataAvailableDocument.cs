using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Volur.Infrastructure.Persistence.Models;

/// <summary>
/// MongoDB document for tracking stocks that don't have fundamental data available.
/// This helps avoid wasting API calls on stocks that consistently return no data.
/// </summary>
public sealed class NoDataAvailableDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Stock ticker symbol
    /// </summary>
    [BsonElement("ticker")]
    public required string Ticker { get; set; }

    /// <summary>
    /// Exchange code for the stock
    /// </summary>
    [BsonElement("exchangeCode")]
    public required string ExchangeCode { get; set; }

    /// <summary>
    /// Number of consecutive times this stock has failed to return data
    /// </summary>
    [BsonElement("failureCount")]
    public int FailureCount { get; set; }

    /// <summary>
    /// When this stock was first marked as having no data available
    /// </summary>
    [BsonElement("firstFailedAt")]
    public DateTime FirstFailedAt { get; set; }

    /// <summary>
    /// When this stock was last attempted to be fetched
    /// </summary>
    [BsonElement("lastAttemptedAt")]
    public DateTime LastAttemptedAt { get; set; }

    /// <summary>
    /// When this document should be automatically deleted (TTL)
    /// </summary>
    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Optional error message from the last failed attempt
    /// </summary>
    [BsonElement("lastErrorMessage")]
    public string? LastErrorMessage { get; set; }
}
