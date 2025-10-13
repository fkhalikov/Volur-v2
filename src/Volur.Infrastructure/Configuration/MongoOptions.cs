namespace Volur.Infrastructure.Configuration;

/// <summary>
/// Configuration for MongoDB connection.
/// </summary>
public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "volur";
}

