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
    /// When set, the connection string is derived from this path automatically.
    /// Supports absolute paths, relative paths, and paths with spaces or Unicode characters.
    /// </summary>
    public string? FilePath { get; set; } = "crudgenerator.db";

    private string? _customConnectionString;

    /// <summary>
    /// Gets or sets the connection string.
    /// When set explicitly, this value takes precedence over <see cref="FilePath"/>.
    /// Setting an empty string clears the custom connection string and falls back to <see cref="FilePath"/>.
    /// </summary>
    public string ConnectionString
    {
        get => _customConnectionString ?? $"Data Source=\"{FilePath}\";Version=3;";
        set => _customConnectionString = value;
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
    /// Returns <see langword="true"/> when a valid connection string or file path is configured.
    /// </summary>
    public bool Validate()
    {
        if (_customConnectionString is not null)
            return !string.IsNullOrWhiteSpace(_customConnectionString) && ConnectionTimeout > 0;

        return !string.IsNullOrWhiteSpace(FilePath) && ConnectionTimeout > 0;
    }
}
