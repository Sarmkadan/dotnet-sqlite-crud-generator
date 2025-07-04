#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Fluent pipeline builder that composes import, validation, transformation, and export
/// stages into a single executable transfer operation backed by <see cref="IBulkTransferService{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Pipelines are not thread-safe and must not be shared across concurrent operations.
/// The configured stages (transform, filter, progress, error handler, retry) persist on the
/// builder instance and apply to every subsequent execution call, making the pipeline safely
/// reusable within the same scope.
/// </para>
/// <para>
/// Obtain an instance via constructor injection of <see cref="BulkTransferPipeline{T}"/>
/// or the static <see cref="Create"/> factory after registering bulk-transfer services
/// with <c>AddBulkTransfer&lt;T&gt;</c>.
/// </para>
/// </remarks>
/// <typeparam name="T">The entity type flowing through the pipeline. Must be a reference type.</typeparam>
public sealed class BulkTransferPipeline<T> where T : class
{
    private readonly IBulkTransferService<T> _service;

    private BulkTransferOptions _options = BulkTransferOptions.Default;
    private Func<T, T?>? _transform;
    private Func<T, bool>? _filter;
    private IProgress<BulkTransferProgress>? _progress;
    private Action<BulkTransferError>? _errorHandler;
    private int _retryCount;
    private TimeSpan _retryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Constructs a new pipeline wrapping the given bulk-transfer service.
    /// </summary>
    /// <param name="service">Underlying service that handles I/O and persistence.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <c>null</c>.</exception>
    public BulkTransferPipeline(IBulkTransferService<T> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // ── Fluent configuration ──────────────────────────────────────────────────

    /// <summary>
    /// Replaces the default <see cref="BulkTransferOptions"/> with the supplied instance.
    /// </summary>
    /// <param name="options">Options applied to all subsequent execution calls.</param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    public BulkTransferPipeline<T> WithOptions(BulkTransferOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Registers a per-entity transformation applied before each entity is persisted during
    /// the import or transfer phase. Returning <c>null</c> silently drops the entity.
    /// </summary>
    /// <param name="transform">Transformation function; return <c>null</c> to exclude an entity.</param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    public BulkTransferPipeline<T> WithTransform(Func<T, T?> transform)
    {
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        return this;
    }

    /// <summary>
    /// Registers a server-side predicate applied before export. Only entities satisfying
    /// the predicate are written to the destination stream or file.
    /// </summary>
    /// <param name="filter">Predicate evaluated against each persisted entity at export time.</param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    public BulkTransferPipeline<T> WithFilter(Func<T, bool> filter)
    {
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        return this;
    }

    /// <summary>
    /// Attaches a progress observer that receives <see cref="BulkTransferProgress"/> snapshots
    /// at the interval configured in <see cref="BulkTransferOptions.ProgressReportingInterval"/>.
    /// </summary>
    /// <param name="progress">Observer receiving real-time progress notifications.</param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    public BulkTransferPipeline<T> WithProgress(IProgress<BulkTransferProgress> progress)
    {
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        return this;
    }

    /// <summary>
    /// Registers a delegate invoked for every <see cref="BulkTransferError"/> produced
    /// during any stage. Called synchronously after each operation completes.
    /// </summary>
    /// <param name="errorHandler">Delegate receiving individual per-record error details.</param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    public BulkTransferPipeline<T> OnError(Action<BulkTransferError> errorHandler)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        return this;
    }

    /// <summary>
    /// Enables automatic retry of failed import operations. Each attempt is preceded by
    /// a fixed delay. Retry state does not persist across independent execution calls.
    /// </summary>
    /// <param name="count">Maximum retry attempts per execution call. Must be zero or greater.</param>
    /// <param name="delay">
    /// Fixed pause between consecutive attempts.
    /// Defaults to 2 seconds when <see cref="TimeSpan.Zero"/> is supplied.
    /// </param>
    /// <returns>The same pipeline instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
    public BulkTransferPipeline<T> WithRetry(int count, TimeSpan delay = default)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Retry count must be zero or greater.");

        _retryCount = count;
        _retryDelay = delay == TimeSpan.Zero ? TimeSpan.FromSeconds(2) : delay;
        return this;
    }

    // ── Execution ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Imports entities from a file, applying configured transformation and routing all
    /// per-record errors to the registered <see cref="OnError"/> handler.
    /// </summary>
    /// <param name="filePath">Absolute path to the source file.</param>
    /// <param name="format">Serialization format of the source file.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the full outcome.</returns>
    public async Task<BulkImportResult> ImportFromFileAsync(
        string filePath,
        ImportFormat format,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteWithRetryAsync(
            () => _service.ImportFromFileAsync(filePath, format, _progress, cancellationToken),
            cancellationToken);

        NotifyErrors(result.Errors);
        return result;
    }

    /// <summary>
    /// Imports entities from a readable stream, applying configured transformation and routing
    /// all per-record errors to the registered <see cref="OnError"/> handler.
    /// </summary>
    /// <param name="source">Readable stream containing serialized entity data.</param>
    /// <param name="format">Serialization format of the source stream.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkImportResult"/> describing the full outcome.</returns>
    public async Task<BulkImportResult> ImportFromStreamAsync(
        Stream source,
        ImportFormat format,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteWithRetryAsync(
            () => _service.ImportFromStreamAsync(source, format, _progress, cancellationToken),
            cancellationToken);

        NotifyErrors(result.Errors);
        return result;
    }

    /// <summary>
    /// Exports entities to a file. The configured server-side filter is not applied here;
    /// use <see cref="ExportToStreamAsync"/> when filtering is required.
    /// </summary>
    /// <param name="filePath">Absolute path to the destination file.</param>
    /// <param name="format">Target serialization format.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkExportResult"/> describing the full outcome.</returns>
    public Task<BulkExportResult> ExportToFileAsync(
        string filePath,
        ExportFormat format,
        CancellationToken cancellationToken = default) =>
        _service.ExportToFileAsync(filePath, format, _progress, cancellationToken);

    /// <summary>
    /// Exports entities to a writable stream, routing through
    /// <see cref="IBulkExportService{T}.ExportFilteredAsync"/> when a filter has been registered
    /// via <see cref="WithFilter"/>, or <see cref="IBulkExportService{T}.ExportToStreamAsync"/>
    /// otherwise.
    /// </summary>
    /// <param name="destination">Writable stream to receive serialized entity data.</param>
    /// <param name="format">Target serialization format.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>A <see cref="BulkExportResult"/> describing the full outcome.</returns>
    public Task<BulkExportResult> ExportToStreamAsync(
        Stream destination,
        ExportFormat format,
        CancellationToken cancellationToken = default) =>
        _filter is not null
            ? _service.ExportFilteredAsync(_filter, destination, format, _progress, cancellationToken)
            : _service.ExportToStreamAsync(destination, format, _progress, cancellationToken);

    /// <summary>
    /// Pipes data from a source stream to a destination stream, applying the configured
    /// per-entity transformation between the import and export phases.
    /// </summary>
    /// <param name="source">Readable stream of source entities in <paramref name="sourceFormat"/>.</param>
    /// <param name="sourceFormat">Serialization format of the source stream.</param>
    /// <param name="destination">Writable stream to receive entities in <paramref name="destinationFormat"/>.</param>
    /// <param name="destinationFormat">Target serialization format for the destination stream.</param>
    /// <param name="cancellationToken">Token to cooperatively cancel the operation.</param>
    /// <returns>
    /// A <see cref="BulkTransferResult"/> containing outcomes for both the import and export phases.
    /// </returns>
    public async Task<BulkTransferResult> TransferAsync(
        Stream source,
        ImportFormat sourceFormat,
        Stream destination,
        ExportFormat destinationFormat,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.TransferAsync(
            source, sourceFormat,
            destination, destinationFormat,
            _transform, _progress,
            cancellationToken);

        NotifyErrors(result.Import.Errors);
        return result;
    }

    /// <summary>
    /// Returns cumulative statistics for all operations executed through the underlying
    /// service since it was instantiated.
    /// </summary>
    public BulkTransferStatistics GetStatistics() => _service.GetStatistics();

    /// <summary>
    /// Creates a new, unconfigured pipeline wrapping the supplied service. Semantically
    /// equivalent to calling the constructor directly but reads more naturally in fluent
    /// initialization chains.
    /// </summary>
    /// <param name="service">Underlying bulk-transfer service.</param>
    /// <returns>A new <see cref="BulkTransferPipeline{T}"/> with default configuration.</returns>
    public static BulkTransferPipeline<T> Create(IBulkTransferService<T> service) => new(service);

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<BulkImportResult> ExecuteWithRetryAsync(
        Func<Task<BulkImportResult>> operation,
        CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < _retryCount && !cancellationToken.IsCancellationRequested)
            {
                attempt++;
                Console.Error.WriteLine(
                    $"[BulkTransferPipeline] Attempt {attempt}/{_retryCount} failed: {ex.Message}. " +
                    $"Retrying in {_retryDelay.TotalSeconds:F1}s...");
                await Task.Delay(_retryDelay, cancellationToken);
            }
        }
    }

    private void NotifyErrors(List<BulkTransferError> errors)
    {
        if (_errorHandler is null || errors.Count == 0) return;
        foreach (var error in errors)
            _errorHandler(error);
    }
}
