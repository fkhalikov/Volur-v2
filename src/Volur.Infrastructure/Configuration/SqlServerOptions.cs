namespace Volur.Infrastructure.Configuration;

/// <summary>
/// Configuration for SQL Server connection.
/// </summary>
public sealed class SqlServerOptions
{
    public const string SectionName = "SqlServer";

    public string ConnectionString { get; set; } = "Server=.\\SQLEXPRESS;Database=Volur;Trusted_Connection=True;TrustServerCertificate=True;";
}
