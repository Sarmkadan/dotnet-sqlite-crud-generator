#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Service for exporting entity data to various formats.
// Supports JSON, CSV, and XML exports with streaming capability.
// =============================================================================

using DotNet.SQLite.CrudGenerator.Formatters;
using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for exporting entity data to various formats.
/// Supports JSON, CSV, and XML exports with streaming capability.
/// </summary>
public sealed class DataExportService
{
    private readonly JsonFormatter _jsonFormatter;
    private readonly CsvFormatter _csvFormatter;
    private readonly XmlFormatter _xmlFormatter;

    public DataExportService()
    {
        _jsonFormatter = new JsonFormatter(pretty: true);
        _csvFormatter = new CsvFormatter();
        _xmlFormatter = new XmlFormatter();
    }

    public async Task<string> ExportAsJsonAsync<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        return await _jsonFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsCsvAsync<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        return await _csvFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsXmlAsync<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        return await _xmlFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsJsonLinesAsync<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        return await _jsonFormatter.FormatJsonLinesAsync(items);
    }

    public async Task ExportAsJsonLinesToFileAsync<T>(IEnumerable<T> items, string filePath) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            var jsonLines = await _jsonFormatter.FormatJsonLinesAsync(items);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(filePath, jsonLines);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"JSON Lines export failed: {ex.Message}");
            throw;
        }
    }

    public async Task ExportAsJsonLinesToStreamAsync<T>(IEnumerable<T> items, Stream stream) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        try
        {
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                var jsonLines = await _jsonFormatter.FormatJsonLinesAsync(items);
                await writer.WriteAsync(jsonLines);
                await writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"JSON Lines stream export failed: {ex.Message}");
            throw;
        }
    }

    public async Task<string> ExportAsMarkdownAsync<T>(IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        return GenerateMarkdownTable(items);
    }

    public async Task ExportAsMarkdownToFileAsync<T>(IEnumerable<T> items, string filePath) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            var markdown = GenerateMarkdownTable(items);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(filePath, markdown);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Markdown export failed: {ex.Message}");
            throw;
        }
    }

    private static string GenerateMarkdownTable<T>(IEnumerable<T> items) where T : class
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        if (!properties.Any())
            return string.Empty;

        var header = "| " + string.Join(" | ", properties.Select(p => p.Name)) + " |";
        var separator = "| " + string.Join(" | ", properties.Select(p => "---")) + " |";

        var rows = new List<string>();
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null)
                    return string.Empty;
                var stringValue = value.ToString()!;
                return stringValue.Replace("|", "\\|");
            });
            rows.Add("| " + string.Join(" | ", values) + " |");
        }

        return string.Join(Environment.NewLine, header, separator, string.Join(Environment.NewLine, rows));
    }

    public async Task<bool> ExportToFileAsync<T>(IEnumerable<T> items, string filePath, ExportFormat format) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            if (!Path.IsPathRooted(filePath))
            {
                Console.Error.WriteLine("Export failed: File path must be rooted");
                return false;
            }

            var content = format switch
            {
                ExportFormat.Json => await ExportAsJsonAsync(items),
                ExportFormat.Csv => await ExportAsCsvAsync(items),
                ExportFormat.Xml => await ExportAsXmlAsync(items),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Export failed: {ex.Message}");
            return false;
        }
    }

    public async Task ExportToStreamAsync<T>(IEnumerable<T> items, Stream stream, ExportFormat format) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        try
        {
            var content = format switch
            {
                ExportFormat.Json => await ExportAsJsonAsync(items),
                ExportFormat.Csv => await ExportAsCsvAsync(items),
                ExportFormat.Xml => await ExportAsXmlAsync(items),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Stream export failed: {ex.Message}");
            throw;
        }
    }

    public ExportReport GenerateExportReport<T>(IEnumerable<T> items, string entityName) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemList = items.ToList();

        return new ExportReport
        {
            EntityName = entityName,
            ItemCount = itemList.Count,
            ExportedAt = DateTime.UtcNow,
            AvailableFormats = new[] { "json", "csv", "xml" },
            SampleItem = itemList.FirstOrDefault()
        };
    }
}

public enum ExportFormat
{
    Json,
    Csv,
    Xml
}

public sealed class ExportReport
{
    public string EntityName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public string[] AvailableFormats { get; set; } = [];
    public object? SampleItem { get; set; }

    public override string ToString()
    {
        return $"Exported {ItemCount} {EntityName} items in {string.Join(", ", AvailableFormats)} format(s) at {ExportedAt:O}";
    }
}
