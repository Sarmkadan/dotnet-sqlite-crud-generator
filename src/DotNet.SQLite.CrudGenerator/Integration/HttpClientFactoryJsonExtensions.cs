#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Provides System.Text.Json serialization extensions for HttpClientFactory.
/// </summary>
public static class HttpClientFactoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the HttpClientFactory configuration to a JSON string.
    /// </summary>
    /// <param name="value">The HttpClientFactory instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the HttpClientFactory configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this HttpClientFactory value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var config = new HttpClientFactoryConfiguration
        {
            ConnectionLimit = value.GetConnectionLimit()
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an HttpClientFactory configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An HttpClientFactory instance initialized with the deserialized configuration, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static HttpClientFactory? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var config = JsonSerializer.Deserialize<HttpClientFactoryConfiguration>(json, _jsonSerializerOptions);
        return config is null ? null : new HttpClientFactory(config.ConnectionLimit);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an HttpClientFactory configuration.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized HttpClientFactory instance if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out HttpClientFactory? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            var config = JsonSerializer.Deserialize<HttpClientFactoryConfiguration>(json, _jsonSerializerOptions);
            value = config is null ? null : new HttpClientFactory(config.ConnectionLimit);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the configured connection limit from HttpClientFactory.
    /// </summary>
    /// <param name="factory">The HttpClientFactory instance.</param>
    /// <returns>The configured connection limit.</returns>
    private static int GetConnectionLimit(this HttpClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        // Use reflection to get the private _defaultHandler field
        var field = typeof(HttpClientFactory).GetField("_defaultHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field?.GetValue(factory) is HttpClientHandler handler)
        {
            return handler.MaxConnectionsPerServer;
        }

        // Fallback to default value
        return 10;
    }

    /// <summary>
    /// Represents the serializable configuration of an HttpClientFactory.
    /// </summary>
    private sealed class HttpClientFactoryConfiguration
    {
        public int ConnectionLimit { get; set; } = 10;
    }
}