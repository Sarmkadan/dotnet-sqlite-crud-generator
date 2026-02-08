#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Formatters;

/// <summary>
/// Formats data to JSON with customizable serialization options.
/// Supports both pretty-printing and compact output.
/// Handles circular references and custom type conversions.
/// </summary>
public sealed class JsonFormatter : IFormatter
{
    private readonly JsonSerializerOptions _options;

    public JsonFormatter(bool pretty = true, bool ignoreNull = false)
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = pretty,
            DefaultIgnoreCondition = ignoreNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeConverter()
            }
        };
    }

    public string Format<T>(T data) where T : class
    {
        try
        {
            if (data is null)
                return "null";

            return JsonSerializer.Serialize(data, _options);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format data as JSON: {ex.Message}", ex);
        }
    }

    public string Format<T>(IEnumerable<T> items) where T : class
    {
        try
        {
            if (items is null)
                return "[]";

            return JsonSerializer.Serialize(items.ToList(), _options);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format collection as JSON: {ex.Message}", ex);
        }
    }

    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        return await Task.FromResult(Format(data));
    }

    public async Task<string> FormatAsync<T>(IEnumerable<T> items) where T : class
    {
        return await Task.FromResult(Format(items));
    }

    public T? Parse<T>(string json) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<T>(json, _options);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse JSON: {ex.Message}", ex);
        }
    }

    public IEnumerable<T>? ParseCollection<T>(string json) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<List<T>>(json, _options);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse JSON collection: {ex.Message}", ex);
        }
    }

    public JsonDocument ParseAsDocument(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse JSON document: {ex.Message}", ex);
        }
    }

    public string? GetJsonPath<T>(T data, string path) where T : class
    {
        try
        {
            var json = Format(data);
            var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            foreach (var segment in path.Split('.'))
            {
                if (element.TryGetProperty(segment, out var property))
                    element = property;
                else
                    return null;
            }

            return element.GetRawText();
        }
        catch
        {
            return null;
        }
    }
}

public interface IFormatter
{
    string Format<T>(T data) where T : class;
    string Format<T>(IEnumerable<T> items) where T : class;
    Task<string> FormatAsync<T>(T data) where T : class;
    Task<string> FormatAsync<T>(IEnumerable<T> items) where T : class;
}

public sealed class FormattingException : Exception
{
    public FormattingException(string message) : base(message) { }
    public FormattingException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? DateTime.UtcNow.ToString("O"));
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}
