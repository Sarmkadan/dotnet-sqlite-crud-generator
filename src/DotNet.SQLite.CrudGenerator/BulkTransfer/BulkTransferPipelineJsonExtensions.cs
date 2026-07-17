#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="BulkTransferPipeline{T}"/>.
/// </summary>
public static class BulkTransferPipelineJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the pipeline configuration to a JSON string.
    /// </summary>
    /// <typeparam name="T">The entity type flowing through the pipeline.</typeparam>
    /// <param name="value">The pipeline instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the pipeline configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson<T>(this BulkTransferPipeline<T> value, bool indented = false) where T : class
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
    /// Deserializes a JSON string to a <see cref="BulkTransferPipeline{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The entity type flowing through the pipeline.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized pipeline instance, or <c>null</c> if the JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static BulkTransferPipeline<T>? FromJson<T>(string json) where T : class
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<BulkTransferPipeline<T>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BulkTransferPipeline{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The entity type flowing through the pipeline.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized pipeline instance if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson<T>(string json, out BulkTransferPipeline<T>? value) where T : class
    {
        value = null;

        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = FromJson<T>(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
