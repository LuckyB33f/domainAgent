namespace DomainAgent.Configuration;

/// <summary>
/// Configuration options for MySQL database connection.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// MySQL connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
