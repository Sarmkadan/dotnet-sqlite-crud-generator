#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for DateTime values.
/// Enables consistent JSON serialization/deserialization of DateTime objects with camelCase naming.
/// </summary>
public static class DateTimeExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonDateTimeConverter() }
    };

    /// <summary>
    /// Serializes a DateTime to a JSON string.
    /// </summary>
    /// <param name="value">The DateTime value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the DateTime.</returns>
    public static string ToJson(this DateTime value, bool indented = false)
    {
        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTime? value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized DateTime? value, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to DateTime.</exception>
    public static DateTime? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTime? value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized DateTime? value if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string? json, out DateTime? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DateTime>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Custom JSON converter for DateTime values to ensure consistent ISO 8601 formatting.
    /// </summary>
    private sealed class JsonDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToIso8601());
        }
    }
}