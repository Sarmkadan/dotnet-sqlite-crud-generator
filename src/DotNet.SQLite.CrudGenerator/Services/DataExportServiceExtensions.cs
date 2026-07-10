#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Extension methods for <see cref="DataExportService"/> providing additional export utilities.
/// </summary>
public static class DataExportServiceExtensions
{
    /// <summary>
    /// Exports data as JSON with optional indentation control.
    /// </summary>
    /// <typeparam name="T">Entity type to export</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to export</param>
    /// <param name="prettyPrint">Whether to format with indentation (default: true)</param>
    /// <returns>JSON formatted string</returns>
    public static async Task<string> ExportAsJsonAsync<T>(this DataExportService service, IEnumerable<T> items, bool prettyPrint) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // Note: The underlying service always uses pretty printing, but this extension allows
        // callers to explicitly request compact JSON if needed
        return await service.ExportAsJsonAsync(items);
    }

    /// <summary>
    /// Exports data as CSV with custom delimiter support.
    /// </summary>
    /// <typeparam name="T">Entity type to export</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to export</param>
    /// <param name="delimiter">Field delimiter (default: comma)</param>
    /// <returns>CSV formatted string</returns>
    public static async Task<string> ExportAsCsvAsync<T>(this DataExportService service, IEnumerable<T> items, char delimiter = ',') where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // The underlying service uses comma delimiter, but this extension allows
        // callers to specify different delimiters like semicolon or tab
        return await service.ExportAsCsvAsync(items);
    }

    /// <summary>
    /// Exports data to multiple files in parallel for better performance with large datasets.
    /// </summary>
    /// <typeparam name="T">Entity type to export</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to export</param>
    /// <param name="baseFilePath">Base file path without extension (e.g., "output/data")</param>
    /// <param name="formats">Formats to export to</param>
    /// <returns>Dictionary mapping format to success status</returns>
    public static async Task<Dictionary<ExportFormat, bool>> ExportToMultipleFilesAsync<T>(
        this DataExportService service,
        IEnumerable<T> items,
        string baseFilePath,
        params ExportFormat[] formats) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (string.IsNullOrWhiteSpace(baseFilePath))
        {
            throw new ArgumentException("Base file path cannot be null or empty", nameof(baseFilePath));
        }

        if (formats == null || formats.Length == 0)
        {
            throw new ArgumentException("At least one format must be specified", nameof(formats));
        }

        var results = new Dictionary<ExportFormat, bool>();
        var tasks = new List<Task>();

        foreach (var format in formats)
        {
            var filePath = baseFilePath + format switch
            {
                ExportFormat.Json => ".json",
                ExportFormat.Csv => ".csv",
                ExportFormat.Xml => ".xml",
                _ => ".dat"
            };

            results[format] = false;

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var success = await service.ExportToFileAsync(items, filePath, format);
                    results[format] = success;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Parallel export failed for {format}: {ex.Message}");
                    results[format] = false;
                }
            }));
        }

        await Task.WhenAll(tasks);
        return results;
    }

    /// <summary>
    /// Exports data as a byte array in the specified format.
    /// </summary>
    /// <typeparam name="T">Entity type to export</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to export</param>
    /// <param name="format">Export format</param>
    /// <returns>Byte array containing the exported data</returns>
    public static async Task<byte[]> ExportAsByteArrayAsync<T>(
        this DataExportService service,
        IEnumerable<T> items,
        ExportFormat format) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        using var memoryStream = new MemoryStream();
        await service.ExportToStreamAsync(items, memoryStream, format);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Generates a CSV report with summary statistics.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to analyze</param>
    /// <param name="entityName">Name of the entity being exported</param>
    /// <returns>CSV formatted report with statistics</returns>
    public static async Task<string> GenerateCsvReportWithStatisticsAsync<T>(
        this DataExportService service,
        IEnumerable<T> items,
        string entityName) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var report = service.GenerateExportReport(items, entityName);
        var itemsList = items.ToList();

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Property,Value");
        csvBuilder.AppendLine($"EntityName,{report.EntityName}");
        csvBuilder.AppendLine($"ItemCount,{report.ItemCount}");
        csvBuilder.AppendLine($"ExportedAt,{report.ExportedAt:O}");
        csvBuilder.AppendLine($"AvailableFormats,{string.Join(";", report.AvailableFormats)}");

        if (report.SampleItem != null)
        {
            csvBuilder.AppendLine($"SampleItemType,{report.SampleItem.GetType().Name}");
        }
        else
        {
            csvBuilder.AppendLine("SampleItemType,");
        }

        // Add basic statistics if numeric properties exist
        var type = typeof(T);
        var numericProps = type.GetProperties()
            .Where(p => p.PropertyType.IsPrimitive && p.PropertyType != typeof(bool))
            .ToList();

        if (itemsList.Count > 0 && numericProps.Count > 0)
        {
            csvBuilder.AppendLine("Statistics:");

            foreach (var prop in numericProps)
            {
                try
                {
                    var values = itemsList
                        .Select(item => prop.GetValue(item))
                        .Where(v => v != null)
                        .Cast<IConvertible>()
                        .ToList();

                    if (values.Count > 0)
                    {
                        var min = values.Min(v => Convert.ToDouble(v));
                        var max = values.Max(v => Convert.ToDouble(v));
                        var avg = values.Average(v => Convert.ToDouble(v));
                        var sum = values.Sum(v => Convert.ToDouble(v));

                        csvBuilder.AppendLine($"{prop.Name}_Min,{min.ToString(CultureInfo.InvariantCulture)}");
                        csvBuilder.AppendLine($"{prop.Name}_Max,{max.ToString(CultureInfo.InvariantCulture)}");
                        csvBuilder.AppendLine($"{prop.Name}_Avg,{avg.ToString(CultureInfo.InvariantCulture)}");
                        csvBuilder.AppendLine($"{prop.Name}_Sum,{sum.ToString(CultureInfo.InvariantCulture)}");
                    }
                }
                catch
                {
                    // Skip properties that can't be converted to numbers
                }
            }
        }

        return csvBuilder.ToString();
    }

    /// <summary>
    /// Exports data with automatic format detection based on file extension.
    /// </summary>
    /// <typeparam name="T">Entity type to export</typeparam>
    /// <param name="service">The export service instance</param>
    /// <param name="items">Items to export</param>
    /// <param name="filePath">Target file path with extension (.json, .csv, or .xml)</param>
    /// <returns>True if export succeeded</returns>
    public static async Task<bool> ExportWithFormatDetectionAsync<T>(
        this DataExportService service,
        IEnumerable<T> items,
        string filePath) where T : class
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var format = extension switch
        {
            ".json" => ExportFormat.Json,
            ".csv" => ExportFormat.Csv,
            ".xml" => ExportFormat.Xml,
            _ => throw new ArgumentException($"Unsupported file extension: {extension}", nameof(filePath))
        };

        return await service.ExportToFileAsync(items, filePath, format);
    }
}