#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotNet.SQLite.CrudGenerator.Configuration;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ConnectionPoolConfiguration"/>.
/// </summary>
public static class ConnectionPoolExtensionsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
	};

	/// <summary>
	/// Serializes the <see cref="ConnectionPoolConfiguration"/> to a JSON string.
	/// </summary>
	/// <param name="value">The configuration to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the configuration.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ConnectionPoolConfiguration value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ConnectionPoolConfiguration"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="ConnectionPoolConfiguration"/> instance, or null if the JSON is empty.</returns>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static ConnectionPoolConfiguration? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
			return null;

		return JsonSerializer.Deserialize<ConnectionPoolConfiguration>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ConnectionPoolConfiguration"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance if successful; otherwise, null.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out ConnectionPoolConfiguration? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
			return false;

		try
		{
			value = JsonSerializer.Deserialize<ConnectionPoolConfiguration>(json, _jsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}