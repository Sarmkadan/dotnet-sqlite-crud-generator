#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Data Transfer Object for serializing dependency injection configuration.
/// </summary>
internal sealed class DependencyInjectionConfigurationDto
{
    /// <summary>
    /// Gets or sets the database configuration.
    /// </summary>
    public DatabaseSettingsDto? Database { get; set; }

    /// <summary>
    /// Gets or sets the connection pool configuration.
    /// </summary>
    public ConnectionPoolConfigurationDto? ConnectionPool { get; set; }

    /// <summary>
    /// Gets or sets the cache configuration.
    /// </summary>
    public CacheConfigurationDto? Cache { get; set; }

    /// <summary>
    /// Gets or sets the event bus configuration.
    /// </summary>
    public EventBusConfigurationDto? EventBus { get; set; }

    /// <summary>
    /// Gets or sets the HTTP client configuration.
    /// </summary>
    public HttpClientConfigurationDto? HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the webhook configuration.
    /// </summary>
    public WebhookConfigurationDto? Webhook { get; set; }

    /// <summary>
    /// Gets or sets the background worker configuration.
    /// </summary>
    public BackgroundWorkerConfigurationDto? BackgroundWorker { get; set; }
}

/// <summary>
/// DTO for <see cref="DatabaseSettings"/>.
/// </summary>
internal sealed class DatabaseSettingsDto
{
    public string? FilePath { get; set; }
    public string? ConnectionString { get; set; }
    public bool EnableLogging { get; set; }
    public int ConnectionTimeout { get; set; }
    public bool AutoCreateDatabase { get; set; }
}

/// <summary>
/// DTO for <see cref="ConnectionPoolConfiguration"/>.
/// </summary>
internal sealed class ConnectionPoolConfigurationDto
{
    public int MinPoolSize { get; set; }
    public int MaxPoolSize { get; set; }
    public long IdleTimeoutMs { get; set; }
    public long AcquireTimeoutMs { get; set; }
    public long CleanupIntervalMs { get; set; }
    public bool EnableDiagnostics { get; set; }
}

/// <summary>
/// DTO for <see cref="CacheConfiguration"/>.
/// </summary>
internal sealed class CacheConfigurationDto
{
    public bool Enabled { get; set; }
    public long MaxSizeBytes { get; set; }
    public long DefaultTTLMs { get; set; }
    public int CleanupIntervalSeconds { get; set; }
}

/// <summary>
/// DTO for <see cref="EventBusConfiguration"/>.
/// </summary>
internal sealed class EventBusConfigurationDto
{
    public bool Enabled { get; set; }
    public int MaxEventHistory { get; set; }
    public bool PersistEvents { get; set; }
}

/// <summary>
/// DTO for <see cref="HttpClientConfiguration"/>.
/// </summary>
internal sealed class HttpClientConfigurationDto
{
    public int ConnectionLimit { get; set; }
    public long DefaultTimeoutMs { get; set; }
    public int MaxRetries { get; set; }
    public int RetryDelayMs { get; set; }
}

/// <summary>
/// DTO for <see cref="WebhookConfiguration"/>.
/// </summary>
internal sealed class WebhookConfigurationDto
{
    public bool Enabled { get; set; }
    public int MaxRetries { get; set; }
    public int RetryDelayMs { get; set; }
    public int MaxDeliveryHistorySize { get; set; }
}

/// <summary>
/// DTO for <see cref="BackgroundWorkerConfiguration"/>.
/// </summary>
internal sealed class BackgroundWorkerConfigurationDto
{
    public bool Enabled { get; set; }
    public int WorkerCount { get; set; }
    public int MaxQueueSize { get; set; }
    public int TaskTimeoutSeconds { get; set; }
    public int MaxRetries { get; set; }
}

/// <summary>
/// Provides System.Text.Json serialization extensions for dependency injection configuration.
/// Enables converting <see cref="DatabaseSettings"/> and <see cref="DotnetSqliteCrudGeneratorOptions"/> to/from JSON format.
/// </summary>
public static class DependencyInjectionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a <see cref="DatabaseSettings"/> object to a JSON string.
    /// </summary>
    /// <param name="value">The database settings to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the database settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DatabaseSettings value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DatabaseSettings"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized database settings, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DatabaseSettings? FromJsonToDatabaseSettings(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DatabaseSettings>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DatabaseSettings"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized database settings, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJsonToDatabaseSettings(string json, out DatabaseSettings? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<DatabaseSettings>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a <see cref="DotnetSqliteCrudGeneratorOptions"/> object to a JSON string.
    /// </summary>
    /// <param name="value">The options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DotnetSqliteCrudGeneratorOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DotnetSqliteCrudGeneratorOptions"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized options, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static DotnetSqliteCrudGeneratorOptions? FromJsonToOptions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DotnetSqliteCrudGeneratorOptions>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DotnetSqliteCrudGeneratorOptions"/> object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized options, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJsonToOptions(string json, out DotnetSqliteCrudGeneratorOptions? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<DotnetSqliteCrudGeneratorOptions>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}