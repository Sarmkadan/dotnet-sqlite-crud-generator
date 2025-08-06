#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Provides JSON serialization and deserialization extensions for Repository types.
/// </summary>
public static class RepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the repository instance to a JSON string.
    /// </summary>
    /// <typeparam name="T">The entity type stored in the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="value">The repository instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<T, TKey>(this Repository<T, TKey> value, bool indented = false)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Repository instance.
    /// </summary>
    /// <typeparam name="T">The entity type stored in the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized Repository instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static Repository<T, TKey>? FromJson<T, TKey>(string json)
        where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Repository<T, TKey>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Repository instance.
    /// </summary>
    /// <typeparam name="T">The entity type stored in the repository.</typeparam>
    /// <typeparam name="TKey">The key type of the repository.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized Repository instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson<T, TKey>(string json, out Repository<T, TKey>? value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<Repository<T, TKey>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}