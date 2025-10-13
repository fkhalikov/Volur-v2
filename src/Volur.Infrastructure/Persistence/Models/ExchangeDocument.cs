using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Volur.Infrastructure.Persistence.Models;

/// <summary>
/// MongoDB document for Exchange.
/// </summary>
public sealed class ExchangeDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? OperatingMic { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

