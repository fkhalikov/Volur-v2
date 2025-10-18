using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Volur.Infrastructure.Persistence.Models;

/// <summary>
/// MongoDB document for Symbol.
/// </summary>
public sealed class SymbolDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Ticker { get; set; } = string.Empty;
    public string ExchangeCode { get; set; } = string.Empty;
    public string ParentExchange { get; set; } = string.Empty;
    public string FullSymbol { get; set; } = string.Empty; // {Ticker}.{ExchangeCode}
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Isin { get; set; }
    public string? Currency { get; set; }
    public bool IsActive { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

