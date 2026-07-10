#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="AuditLog"/>.
/// </summary>
public static class AuditLogJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the audit log entry to a JSON string.
    /// </summary>
    /// <param name="value">The audit log entry to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the audit log entry.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this AuditLog value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
    }

    /// <summary>
    /// Deserializes an audit log entry from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized audit log entry, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static AuditLog? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuditLog>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an audit log entry from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized audit log entry if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out AuditLog? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<AuditLog>(json, _jsonOptions)!;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the JSON serializer options with indentation enabled.
    /// </summary>
    /// <returns>Configured JsonSerializerOptions with indentation.</returns>
    private static JsonSerializerOptions GetIndentedOptions()
    {
        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true
        };
        return options;
    }
}