#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="PerformanceMonitor"/> and related types.
/// </summary>
public static class PerformanceMonitorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Converts the <see cref="PerformanceMonitor"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The performance monitor instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the performance monitor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PerformanceMonitor value, bool indented = false) => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PerformanceMonitor"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="PerformanceMonitor"/> instance populated from the JSON data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PerformanceMonitor? FromJson(string json) => JsonSerializer.Deserialize<PerformanceMonitor>(json, _jsonOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PerformanceMonitor"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="PerformanceMonitor"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToPerformanceMonitor(string json, out PerformanceMonitor? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<PerformanceMonitor>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes an <see cref="OperationMetrics"/> instance to JSON.
    /// </summary>
    /// <param name="value">The operation metrics to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the operation metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this OperationMetrics value, bool indented = false) => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to an <see cref="OperationMetrics"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="OperationMetrics"/> instance populated from the JSON data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static OperationMetrics? FromJsonToOperationMetrics(string json) => JsonSerializer.Deserialize<OperationMetrics>(json, _jsonOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="OperationMetrics"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="OperationMetrics"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToOperationMetrics(string json, out OperationMetrics? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<OperationMetrics>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a <see cref="PerformanceReport"/> instance to JSON.
    /// </summary>
    /// <param name="value">The performance report to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the performance report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PerformanceReport value, bool indented = false) => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PerformanceReport"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="PerformanceReport"/> instance populated from the JSON data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PerformanceReport? FromJsonToPerformanceReport(string json) => JsonSerializer.Deserialize<PerformanceReport>(json, _jsonOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PerformanceReport"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="PerformanceReport"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToPerformanceReport(string json, out PerformanceReport? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<PerformanceReport>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Serializes a <see cref="MemoryInfo"/> instance to JSON.
    /// </summary>
    /// <param name="value">The memory info to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the memory info.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this MemoryInfo value, bool indented = false) => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="MemoryInfo"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="MemoryInfo"/> instance populated from the JSON data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static MemoryInfo? FromJsonToMemoryInfo(string json) => JsonSerializer.Deserialize<MemoryInfo>(json, _jsonOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="MemoryInfo"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="MemoryInfo"/> instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJsonToMemoryInfo(string json, out MemoryInfo? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<MemoryInfo>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}