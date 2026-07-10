// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.BackgroundWorkers;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for BackgroundTaskQueue.
/// </summary>
public static class BackgroundTaskQueueJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the BackgroundTaskQueue instance to a JSON string.
    /// </summary>
    /// <param name="value">The BackgroundTaskQueue instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the BackgroundTaskQueue.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this BackgroundTaskQueue value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a BackgroundTaskQueue instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A BackgroundTaskQueue instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static BackgroundTaskQueue? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackgroundTaskQueue>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a BackgroundTaskQueue instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized BackgroundTaskQueue instance, or null on failure.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out BackgroundTaskQueue? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<BackgroundTaskQueue>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}