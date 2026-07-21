#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Service for exporting entity data to various formats.
// Supports JSON, CSV, and XML exports with streaming capability.
// =============================================================================

using DotNet.SQLite.CrudGenerator.Formatters;

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
        return await _jsonFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsCsvAsync<T>(IEnumerable<T> items) where T : class
    {
        return await _csvFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsXmlAsync<T>(IEnumerable<T> items) where T : class
    {
        return await _xmlFormatter.FormatAsync(items);
    }

    public async Task<string> ExportAsJsonLinesAsync<T>(IEnumerable<T> items) where T : class
    {
        return await _jsonFormatter.FormatJsonLinesAsync(items);
    }

    public async Task ExportAsJsonLinesToFileAsync<T>(IEnumerable<T> items, string filePath) where T : class
    {
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

    public async Task<bool> ExportToFileAsync<T>(IEnumerable<T> items, string filePath, ExportFormat format) where T : class
    {
        try
        {
            // Validate file path before attempting export
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.Error.WriteLine("Export failed: File path cannot be null or empty");
                return false;
            }

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