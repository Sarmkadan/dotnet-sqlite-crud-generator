#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Extension methods for registering bulk-transfer services into an
/// <see cref="IServiceCollection"/> and for working with <see cref="BulkImportResult"/>,
/// <see cref="BulkExportResult"/>, and async entity streams.
/// </summary>
public static class BulkTransferServiceExtensions
{
    // ── DI registrations ──────────────────────────────────────────────────────

    /// <summary>
    /// Registers all bulk-transfer services for entity type <typeparamref name="T"/>
    /// with <b>scoped</b> lifetime. Subsequent calls for the same <typeparamref name="T"/>
    /// are idempotent — existing registrations are preserved.
    /// </summary>
    /// <remarks>
    /// The following registrations are added (each via <c>TryAdd*</c>):
    /// <list type="bullet">
    /// <item><see cref="BulkImportExportEngine{T}"/> — the concrete, scoped implementation</item>
    /// <item><see cref="IBulkImportService{T}"/> resolved from the concrete registration above</item>
    /// <item><see cref="IBulkExportService{T}"/> resolved from the concrete registration above</item>
    /// <item><see cref="IBulkTransferService{T}"/> resolved from the concrete registration above</item>
    /// <item><see cref="BulkTransferPipeline{T}"/> — transient fluent builder</item>
    /// </list>
    /// An <see cref="IRepository{T,TKey}"/> with <c>TKey = int</c> must already be registered
    /// (e.g. via <c>AddApplicationServices</c>) before the first scope is created.
    /// </remarks>
    /// <typeparam name="T">The entity type to register transfer services for.</typeparam>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configure">
    /// Optional delegate that mutates a fresh <see cref="BulkTransferOptions"/> instance before
    /// it is captured by the engine factory. When <c>null</c>,
    /// <see cref="BulkTransferOptions.Default"/> is used unchanged.
    /// </param>
    /// <returns>The same service collection to allow call chaining.</returns>
    public static IServiceCollection AddBulkTransfer<T>(
        this IServiceCollection services,
        Action<BulkTransferOptions>? configure = null)
        where T : class
    {
        var options = BulkTransferOptions.Default;
        configure?.Invoke(options);

        services.TryAddScoped<DataExportService>();

        services.TryAddScoped<BulkImportExportEngine<T>>(provider =>
            new BulkImportExportEngine<T>(
                provider.GetRequiredService<IRepository<T, int>>(),
                provider.GetRequiredService<DataExportService>(),
                options));

        services.TryAddScoped<IBulkImportService<T>>(provider => provider.GetRequiredService<BulkImportExportEngine<T>>());
        services.TryAddScoped<IBulkExportService<T>>(provider => provider.GetRequiredService<BulkImportExportEngine<T>>());
        services.TryAddScoped<IBulkTransferService<T>>(provider => provider.GetRequiredService<BulkImportExportEngine<T>>());

        services.TryAddTransient<BulkTransferPipeline<T>>(provider =>
            new BulkTransferPipeline<T>(provider.GetRequiredService<IBulkTransferService<T>>()));

        return services;
    }

    /// <summary>
    /// Overload that accepts a pre-built <see cref="BulkTransferOptions"/> instance, useful
    /// when options are constructed from external configuration (e.g. appsettings.json).
    /// </summary>
    /// <typeparam name="T">The entity type to register transfer services for.</typeparam>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="options">Pre-built options instance to capture in the engine factory.</param>
    /// <returns>The same service collection to allow call chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static IServiceCollection AddBulkTransfer<T>(
        this IServiceCollection services,
        BulkTransferOptions options)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(options);

        return services.AddBulkTransfer<T>(o =>
        {
            o.BatchSize = options.BatchSize;
            o.MaxConcurrency = options.MaxConcurrency;
            o.EnableProgressReporting = options.EnableProgressReporting;
            o.ProgressReportingInterval = options.ProgressReportingInterval;
            o.BufferSize = options.BufferSize;
            o.EnableCheckpointing = options.EnableCheckpointing;
            o.CheckpointFilePath = options.CheckpointFilePath;
            o.ValidationMode = options.ValidationMode;
            o.MaxErrorThreshold = options.MaxErrorThreshold;
            o.UseTransactions = options.UseTransactions;
            o.BatchTimeout = options.BatchTimeout;
        });
    }

    // ── BulkImportResult helpers ──────────────────────────────────────────────

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> when the import result indicates
    /// that no records were committed successfully (i.e. <see cref="BulkImportResult.IsSuccess"/>
    /// is <c>false</c>).
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The same result to allow fluent call chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="BulkImportResult.IsSuccess"/> is <c>false</c>.
    /// </exception>
    public static BulkImportResult ThrowIfFailed(this BulkImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccess)
        {
            var firstError = result.Errors.FirstOrDefault()?.Message ?? "unknown error";
            throw new InvalidOperationException(
                $"Bulk import failed: {result.Failed:N0} record(s) could not be processed. " +
                $"First error: {firstError}");
        }

        return result;
    }

    /// <summary>
    /// Formats the import result as a concise single-line diagnostic summary.
    /// </summary>
    /// <param name="result">The result to summarise.</param>
    /// <returns>A human-readable summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    public static string ToSummary(this BulkImportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return $"Import: {result.Succeeded:N0} ok / {result.Failed:N0} failed / " +
               $"{result.TotalRead:N0} read in {result.Duration.TotalSeconds:F2}s " +
               $"({result.Throughput:N0} rec/s)";
    }

    // ── BulkExportResult helpers ──────────────────────────────────────────────

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> when the export operation did not
    /// complete successfully (i.e. <see cref="BulkExportResult.IsSuccess"/> is <c>false</c>).
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>The same result to allow fluent call chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="BulkExportResult.IsSuccess"/> is <c>false</c>.
    /// </exception>
    public static BulkExportResult ThrowIfFailed(this BulkExportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Bulk export failed after writing {result.TotalExported:N0} record(s) " +
                $"({result.BytesWritten:N0} bytes).");
        }

        return result;
    }

    /// <summary>
    /// Formats the export result as a concise single-line diagnostic summary.
    /// </summary>
    /// <param name="result">The result to summarise.</param>
    /// <returns>A human-readable summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    public static string ToSummary(this BulkExportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return $"Export: {result.TotalExported:N0} records / {result.BytesWritten:N0} bytes " +
               $"in {result.Duration.TotalSeconds:F2}s ({result.Throughput:N0} rec/s)";
    }

    // ── IAsyncEnumerable helpers ──────────────────────────────────────────────

    /// <summary>
    /// Partitions an <see cref="IAsyncEnumerable{T}"/> into fixed-size arrays suitable for
    /// passing to <see cref="IBulkImportService{T}.ImportBatchAsync"/>. The final array may
    /// be smaller than <paramref name="batchSize"/> when the source count is not a multiple of it.
    /// </summary>
    /// <typeparam name="TItem">The element type of the async sequence.</typeparam>
    /// <param name="source">The async sequence to partition.</param>
    /// <param name="batchSize">Maximum number of elements per array. Must be at least 1.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel enumeration.</param>
    /// <returns>
    /// An async sequence of non-empty arrays, each containing at most
    /// <paramref name="batchSize"/> elements in source order.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="batchSize"/> is less than 1.</exception>
    public static async IAsyncEnumerable<TItem[]> BatchAsync<TItem>(
        this IAsyncEnumerable<TItem> source,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (batchSize < 1)
            throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Batch size must be at least 1.");

        var buffer = new List<TItem>(batchSize);

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            buffer.Add(item);
            if (buffer.Count < batchSize) continue;

            yield return buffer.ToArray();
            buffer.Clear();
        }

        if (buffer.Count > 0)
            yield return buffer.ToArray();
    }

    /// <summary>
    /// Streams all elements of <paramref name="source"/> into the given
    /// <paramref name="service"/> via <see cref="IBulkImportService{T}.ImportStreamingAsync"/>,
    /// preserving backpressure and avoiding full materialisation of the dataset into memory.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">Lazy async source of entities to persist.</param>
    /// <param name="service">Import service used for persistence.</param>
    /// <param name="progress">Optional progress observer.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the full outcome.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> or <paramref name="service"/> is <c>null</c>.
    /// </exception>
    public static Task<BulkImportResult> ImportIntoAsync<T>(
        this IAsyncEnumerable<T> source,
        IBulkImportService<T> service,
        IProgress<BulkTransferProgress>? progress = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(service);

        return service.ImportStreamingAsync(source, progress, cancellationToken);
    }
}