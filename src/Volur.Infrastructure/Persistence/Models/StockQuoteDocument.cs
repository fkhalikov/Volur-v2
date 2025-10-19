using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Volur.Infrastructure.Persistence.Models;

/// <summary>
/// MongoDB document for caching stock quote data.
/// </summary>
public sealed class StockQuoteDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ticker")]
    public required string Ticker { get; set; }

    [BsonElement("currentPrice")]
    public double? CurrentPrice { get; set; }

    [BsonElement("previousClose")]
    public double? PreviousClose { get; set; }

    [BsonElement("change")]
    public double? Change { get; set; }

    [BsonElement("changePercent")]
    public double? ChangePercent { get; set; }

    [BsonElement("open")]
    public double? Open { get; set; }

    [BsonElement("high")]
    public double? High { get; set; }

    [BsonElement("low")]
    public double? Low { get; set; }

    [BsonElement("volume")]
    public double? Volume { get; set; }

    [BsonElement("averageVolume")]
    public double? AverageVolume { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [BsonElement("fetchedAt")]
    public DateTime FetchedAt { get; set; }
}
