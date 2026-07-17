#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Provides System.Text.Json serialization/deserialization extensions for
/// <see cref="BulkTransferProgress"/> to enable compact JSON storage and transport.
/// </summary>
public static class BulkTransferProgressJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
	};

	/// <summary>
	/// Serializes the <see cref="BulkTransferProgress"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The progress instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the progress.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this BulkTransferProgress value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string into a <see cref="BulkTransferProgress"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="BulkTransferProgress"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static BulkTransferProgress? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<BulkTransferProgress>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string into a <see cref="BulkTransferProgress"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance on success; <see langword="null"/> on failure.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
	public static bool TryFromJson(string json, out BulkTransferProgress? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<BulkTransferProgress>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}