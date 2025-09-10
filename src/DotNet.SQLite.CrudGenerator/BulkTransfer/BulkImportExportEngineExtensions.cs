#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.BulkTransfer;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Extension methods for <see cref="BulkImportExportEngine{T}"/> providing convenient
/// bulk transfer operations for common scenarios like JSON serialization, file operations,
/// and streaming transfers.
/// </summary>
public static class BulkImportExportEngineExtensions
{
    /// <summary>
    /// Imports entities from a JSON string asynchronously.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="engine">The bulk import/export engine.</param>
    /// <param name="json">The JSON string containing the entities to import.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the bulk import operation with the result.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="engine"/> or <paramref name="json"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<BulkImportResult> ImportFromJsonAsync<T>(
        this BulkImportExportEngine<T> engine,
        string json,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (json is null)
            throw new ArgumentNullException(nameof(json));

        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        return await engine.ImportFromStreamAsync(stream, ImportFormat.Json, progress, cancellationToken);
    }

    /// <summary>
    /// Exports entities to a JSON string asynchronously.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="engine">The bulk import/export engine.</param>
    /// <param name="format">The export format (JSON, CSV, or XML). Defaults to <see cref="ExportFormat.Json"/>.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the bulk export operation with the result containing the JSON string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> is not a supported value.</exception>
    public static async Task<(BulkExportResult Result, string Json)> ExportToJsonAsync<T>(
        this BulkImportExportEngine<T> engine,
        ExportFormat format = ExportFormat.Json,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));

        if (format is not (ExportFormat.Json or ExportFormat.Csv or ExportFormat.Xml))
            throw new ArgumentOutOfRangeException(nameof(format));

        await using var stream = new MemoryStream();
        var result = await engine.ExportToStreamAsync(stream, format, progress, cancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        var json = await reader.ReadToEndAsync(cancellationToken);

        return (result, json);
    }

    /// <summary>
    /// Exports filtered entities to a JSON string asynchronously.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="engine">The bulk import/export engine.</param>
    /// <param name="predicate">Filter predicate to select which entities to export.</param>
    /// <param name="format">The export format (JSON, CSV, or XML). Defaults to <see cref="ExportFormat.Json"/>.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the bulk export operation with the result containing the JSON string.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="engine"/> or <paramref name="predicate"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> is not a supported value.</exception>
    public static async Task<(BulkExportResult Result, string Json)> ExportFilteredToJsonAsync<T>(
        this BulkImportExportEngine<T> engine,
        Func<T, bool> predicate,
        ExportFormat format = ExportFormat.Json,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (format is not (ExportFormat.Json or ExportFormat.Csv or ExportFormat.Xml))
            throw new ArgumentOutOfRangeException(nameof(format));

        await using var stream = new MemoryStream();
        var result = await engine.ExportFilteredAsync(predicate, stream, format, progress, cancellationToken);

        stream.Position = 0;
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
        var json = await reader.ReadToEndAsync(cancellationToken);

        return (result, json);
    }

    /// <summary>
    /// Transfers entities from one engine to another with optional transformation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="sourceEngine">The source engine to export from.</param>
    /// <param name="destinationEngine">The destination engine to import to.</param>
    /// <param name="transform">Optional transformation function to apply to each entity during transfer.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the bulk transfer operation with combined results.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sourceEngine"/> or <paramref name="destinationEngine"/> is <see langword="null"/>.
    /// </exception>
    public static async Task<BulkTransferResult> TransferToAsync<T>(
        this BulkImportExportEngine<T> sourceEngine,
        BulkImportExportEngine<T> destinationEngine,
        Func<T, T?>? transform = null,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (sourceEngine is null)
            throw new ArgumentNullException(nameof(sourceEngine));
        if (destinationEngine is null)
            throw new ArgumentNullException(nameof(destinationEngine));

        await using var stream = new MemoryStream();

        var exportResult = await sourceEngine.ExportToStreamAsync(stream, ExportFormat.Json, progress, cancellationToken);
        stream.Position = 0;

        var importResult = await destinationEngine.ImportFromStreamAsync(stream, ImportFormat.Json, progress, cancellationToken);

        return new BulkTransferResult(importResult, exportResult);
    }

    /// <summary>
    /// Gets the current statistics from the engine.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="engine">The bulk import/export engine.</param>
    /// <returns>The current statistics object.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    public static BulkTransferStatistics GetStats<T>(this BulkImportExportEngine<T> engine) where T : class
    {
        if (engine is null)
            throw new ArgumentNullException(nameof(engine));

        return engine.GetStatistics();
    }
}