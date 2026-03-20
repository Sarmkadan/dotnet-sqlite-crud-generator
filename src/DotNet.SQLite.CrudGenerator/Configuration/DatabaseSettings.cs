#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Database configuration settings.
/// </summary>
public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>
    /// Gets or sets the SQLite database file path.
    /// </summary>
    public string? FilePath { get; set; } = "crudgenerator.db";

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string ConnectionString
    {
        get => $"Data Source={FilePath};Version=3;";
        set { }
    }

    /// <summary>
    /// Gets or sets whether to enable detailed logging.
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether to create the database automatically.
    /// </summary>
    public bool AutoCreateDatabase { get; set; } = true;

    /// <summary>
    /// Validates the database settings.
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(FilePath) && ConnectionTimeout > 0;
    }
}
