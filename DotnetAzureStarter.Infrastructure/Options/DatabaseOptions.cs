namespace DotnetAzureStarter.Infrastructure.Options;

/// <summary>
/// Strongly typed configuration for the database connection.
/// Bound from the "Database" section in appsettings.json via the Options Pattern.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>Full ADO.NET connection string for the SQL Server database.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>EF Core command timeout in seconds. Defaults to 30.</summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
