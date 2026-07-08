#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Exception thrown when configuration is invalid or missing.
/// </summary>
public sealed class ConfigurationException : DotnetSqliteCrudGeneratorException
{
    public ConfigurationException(string message) : base(message) { }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Creates a configuration exception for missing required configuration.
    /// </summary>
    public static ConfigurationException MissingConfiguration(string configName)
    {
        return new ConfigurationException($"Required configuration '{configName}' is missing or invalid.");
    }

    /// <summary>
    /// Creates a configuration exception for invalid configuration value.
    /// </summary>
    public static ConfigurationException InvalidConfiguration(string configName, string value)
    {
        return new ConfigurationException($"Configuration '{configName}' has invalid value: '{value}'. Expected format or range not met.");
    }

    /// <summary>
    /// Creates a configuration exception for connection string issues.
    /// </summary>
    public static ConfigurationException InvalidConnectionString(string connectionStringName)
    {
        return new ConfigurationException($"Connection string '{connectionStringName}' is invalid or malformed.");
    }

    /// <summary>
    /// Creates a configuration exception for file path issues.
    /// </summary>
    public static ConfigurationException InvalidFilePath(string filePath)
    {
        return new ConfigurationException($"File path is invalid or inaccessible: '{filePath}'");
    }

    /// <summary>
    /// Creates a configuration exception for timeout configuration.
    /// </summary>
    public static ConfigurationException InvalidTimeout(string configName, int timeoutValue)
    {
        return new ConfigurationException($"Configuration '{configName}' has invalid timeout value: {timeoutValue}. Must be greater than 0.");
    }
}