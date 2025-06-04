// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.BulkTransfer;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Interfaces;

/// <summary>
/// Defines asynchronous streaming bulk import operations for entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type to import.</typeparam>
public interface IBulkImportService<T> where T : class
{
    /// <summary>
    /// Imports entities from a readable stream encoded in the specified format.
    /// </summary>
    /// <param name="source">Readable stream containing serialized entity data.</param>
    /// <param name="format">Serialization format of the source stream.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during import.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the outcome.</returns>
    Task<BulkImportResult> ImportFromStreamAsync(
        Stream source,
        ImportFormat format,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports entities from a file path encoded in the specified format.
    /// </summary>
    /// <param name="filePath">Absolute path to the source file.</param>
    /// <param name="format">Serialization format of the source file.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during import.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the outcome.</returns>
    Task<BulkImportResult> ImportFromFileAsync(
        string filePath,
        ImportFormat format,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a pre-materialized batch of entities, processing them in configured batch sizes.
    /// </summary>
    /// <param name="entities">Entities to insert.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during import.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the outcome.</returns>
    Task<BulkImportResult> ImportBatchAsync(
        IEnumerable<T> entities,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports entities from an async stream, buffering and committing in configured batch sizes
    /// without loading the entire dataset into memory.
    /// </summary>
    /// <param name="source">Lazily-evaluated async source of entities.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during import.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the outcome.</returns>
    Task<BulkImportResult> ImportStreamingAsync(
        IAsyncEnumerable<T> source,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines asynchronous streaming bulk export operations for entities of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type to export.</typeparam>
public interface IBulkExportService<T> where T : class
{
    /// <summary>
    /// Exports all persisted entities to a writable stream in the specified format.
    /// </summary>
    /// <param name="destination">Writable stream to receive serialized entity data.</param>
    /// <param name="format">Target serialization format.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during export.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkExportResult"/> describing the outcome.</returns>
    Task<BulkExportResult> ExportToStreamAsync(
        Stream destination,
        ExportFormat format,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports all persisted entities to a file in the specified format.
    /// The destination directory is created automatically if it does not exist.
    /// </summary>
    /// <param name="filePath">Absolute path to the destination file.</param>
    /// <param name="format">Target serialization format.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during export.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkExportResult"/> describing the outcome.</returns>
    Task<BulkExportResult> ExportToFileAsync(
        string filePath,
        ExportFormat format,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports entities matching the given predicate to a writable stream.
    /// </summary>
    /// <param name="predicate">Filter applied over all persisted entities before export.</param>
    /// <param name="destination">Writable stream to receive serialized entity data.</param>
    /// <param name="format">Target serialization format.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during export.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkExportResult"/> describing the outcome.</returns>
    Task<BulkExportResult> ExportFilteredAsync(
        Func<T, bool> predicate,
        Stream destination,
        ExportFormat format,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams persisted entities lazily as an <see cref="IAsyncEnumerable{T}"/> for
    /// pipeline-style downstream consumption without materialising the full result set.
    /// </summary>
    /// <param name="cancellationToken">Token to cooperatively cancel enumeration.</param>
    IAsyncEnumerable<T> StreamEntitiesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Combines bulk import and export capabilities for bidirectional data transfer.
/// Extends both <see cref="IBulkImportService{T}"/> and <see cref="IBulkExportService{T}"/>.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IBulkTransferService<T> : IBulkImportService<T>, IBulkExportService<T> where T : class
{
    /// <summary>
    /// Transfers entities from a source stream to a destination stream, applying an optional
    /// per-entity transformation between the import and export phases.
    /// </summary>
    /// <param name="source">Readable stream of source entities.</param>
    /// <param name="sourceFormat">Serialization format of the source stream.</param>
    /// <param name="destination">Writable stream to receive transformed entities.</param>
    /// <param name="destinationFormat">Target serialization format for the destination.</param>
    /// <param name="transform">Optional per-entity transformation; returning <c>null</c> skips the entity.</param>
    /// <param name="progress">Optional callback receiving progress snapshots during transfer.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkTransferResult"/> containing both import and export outcomes.</returns>
    Task<BulkTransferResult> TransferAsync(
        Stream source,
        ImportFormat sourceFormat,
        Stream destination,
        ExportFormat destinationFormat,
        Func<T, T?>? transform = null,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns cumulative statistics for all operations performed by this service instance.
    /// </summary>
    BulkTransferStatistics GetStatistics();
}
