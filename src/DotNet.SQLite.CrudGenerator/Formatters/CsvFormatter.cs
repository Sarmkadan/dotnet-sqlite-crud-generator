#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace DotNet.SQLite.CrudGenerator.Formatters;

/// <summary>
/// Formats data to CSV (Comma-Separated Values).
/// Handles escaping, quoting, and custom delimiters.
/// Supports both single objects (as single row) and collections.
/// </summary>
public sealed class CsvFormatter : IFormatter
{
    private readonly string _delimiter;
    private readonly bool _includeHeaders;
    private readonly string _quote = "\"";
    private readonly string _newLine = Environment.NewLine;

    public CsvFormatter(string delimiter = ",", bool includeHeaders = true)
    {
        _delimiter = delimiter;
        _includeHeaders = includeHeaders;
    }

    public string Format<T>(T data) where T : class
    {
        try
        {
            var items = new[] { data };
            return Format<T>(items);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format data as CSV: {ex.Message}", ex);
        }
    }

    public string Format<T>(IEnumerable<T> items) where T : class
    {
        try
        {
            if (items is null || !items.Any())
                return string.Empty;

            var itemList = items.ToList();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sb = new StringBuilder();

            // Add header row
            if (_includeHeaders)
            {
                var headers = properties.Select(p => EscapeValue(p.Name));
                sb.AppendLine(string.Join(_delimiter, headers));
            }

            // Add data rows
            foreach (var item in itemList)
            {
                var values = properties.Select(p => EscapeValue(p.GetValue(item)?.ToString() ?? string.Empty));
                sb.AppendLine(string.Join(_delimiter, values));
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to format collection as CSV: {ex.Message}", ex);
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

    public T? Parse<T>(string csv) where T : class
    {
        try
        {
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
                return null;

            var headers = ParseLine(lines[0]);
            var values = ParseLine(lines[1]);

            return MapToObject<T>(headers, values);
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse CSV: {ex.Message}", ex);
        }
    }

    public IEnumerable<T>? ParseCollection<T>(string csv) where T : class
    {
        try
        {
            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return null;

            var headers = ParseLine(lines[0]);
            var items = new List<T>();

            for (int i = _includeHeaders ? 1 : 0; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                var item = MapToObject<T>(headers, values);
                if (item is not null)
                    items.Add(item);
            }

            return items;
        }
        catch (Exception ex)
        {
            throw new FormattingException($"Failed to parse CSV collection: {ex.Message}", ex);
        }
    }

    private string EscapeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(_delimiter) || value.Contains("\"") || value.Contains("\n"))
        {
            return _quote + value.Replace("\"", "\"\"") + _quote;
        }

        return value;
    }

    private List<string> ParseLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch.ToString() == _delimiter && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        values.Add(current.ToString());
        return values;
    }

    private T? MapToObject<T>(List<string> headers, List<string> values) where T : class
    {
        var obj = Activator.CreateInstance<T>();
        if (obj is null)
            return null;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < headers.Count && i < values.Count; i++)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(headers[i], StringComparison.OrdinalIgnoreCase));
            if (property is not null && property.CanWrite)
            {
                try
                {
                    var value = Convert.ChangeType(values[i], property.PropertyType);
                    property.SetValue(obj, value);
                }
                catch
                {
                    // Skip conversion errors
                }
            }
        }

        return obj;
    }
}
