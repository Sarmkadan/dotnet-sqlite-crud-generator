#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Formatters;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Production-grade engine for async bulk import and export with full streaming support,
/// configurable batching, real-time progress reporting, and optional durable checkpointing.
/// </summary>
/// <remarks>
/// <para>
/// The engine implements both <see cref="IBulkImportService{T}"/> and <see cref="IBulkExportService{T}"/>
/// through the combined <see cref="IBulkTransferService{T}"/> interface, making it suitable for
/// direct injection or construction via <see cref="BulkTransferPipeline{T}"/>.
/// </para>
/// <para>
/// All public methods honour the supplied <see cref="CancellationToken"/>.
/// Per-batch timeouts are enforced independently via a linked token source so that a slow
/// batch does not prevent the operation from being cancelled cleanly.
/// </para>
/// </remarks>
/// <typeparam name="T">The entity type managed by this engine. Must be a reference type.</typeparam>
public sealed class BulkImportExportEngine<T> : IBulkTransferService<T> where T : class
{
  private readonly IRepository<T, int> _repository;
  private readonly DataExportService _exportService;
  private readonly BulkTransferOptions _options;
  private readonly BulkTransferStatistics _statistics;
  private readonly Random _retryJitter = new();

  /// <summary>
  /// Initialises a new engine backed by the supplied repository and export service.
  /// </summary>
  /// <param name="repository">Repository providing data access for type <typeparamref name="T"/>.</param>
  /// <param name="exportService">Service used to serialise entities during export.</param>
  /// <param name="options">
  /// Behavioral configuration. Defaults to <see cref="BulkTransferOptions.Default"/> when <c>null</c>.
  /// </param>
  public BulkImportExportEngine(
    IRepository<T, int> repository,
    DataExportService exportService,
    BulkTransferOptions? options = null)
  {
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    _options = options ?? BulkTransferOptions.Default;
    _statistics = new BulkTransferStatistics();
  }

  // ── Import ───────────────────────────────────────────────────────────────

  /// <inheritdoc/>
  public async Task<BulkImportResult> ImportFromStreamAsync(
    Stream source,
    ImportFormat format,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (source is null) throw new ArgumentNullException(nameof(source));

    using var reader = new StreamReader(
      source,
      Encoding.UTF8,
      detectEncodingFromByteOrderMarks: true,
      bufferSize: _options.BufferSize,
      leaveOpen: true);

    var rawContent = await reader.ReadToEndAsync(cancellationToken);
    var entities = DeserializeEntities(rawContent, format);
    return await ImportBatchAsync(entities, progress, cancellationToken);
  }

  /// <inheritdoc/>
  public async Task<BulkImportResult> ImportFromFileAsync(
    string filePath,
    ImportFormat format,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(filePath))
      throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

    if (!File.Exists(filePath))
      throw new FileNotFoundException($"Import source file not found: {filePath}", filePath);

    await using var stream = new FileStream(
      filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
      _options.BufferSize, useAsync: true);

    return await ImportFromStreamAsync(stream, format, progress, cancellationToken);
  }

  /// <inheritdoc/>
  public async Task<BulkImportResult> ImportBatchAsync(
    IEnumerable<T> entities,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (entities is null) throw new ArgumentNullException(nameof(entities));

    var result = new BulkImportResult { StartedAt = DateTime.UtcNow };
            result._options = _options;
    var sessionId = Guid.NewGuid();
    var allEntities = entities is List<T> list ? list : entities.ToList();
    result.TotalRead = allEntities.Count;

    var batches = Partition(allEntities, _options.BatchSize).ToList();

    for (var batchIndex = 0; batchIndex < batches.Count; batchIndex++)
    {
      cancellationToken.ThrowIfCancellationRequested();

      if (result.Errors.Count >= _options.MaxErrorThreshold)
      {
        Console.Error.WriteLine(
          $"[BulkTransfer] Aborting after {_options.MaxErrorThreshold} errors.");
        break;
      }

      await ExecuteSingleBatchAsync(
        batches[batchIndex], batchIndex, result, result.TotalRead,
        sessionId, progress, cancellationToken);
    }

    result.Duration = DateTime.UtcNow - result.StartedAt;
    _statistics.TotalImports++;
    _statistics.TotalRecordsImported += result.Succeeded;
    _statistics.TotalErrors += result.Failed;

    return result;
  }

  /// <inheritdoc/>
  public async Task<BulkImportResult> ImportStreamingAsync(
    IAsyncEnumerable<T> source,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (source is null) throw new ArgumentNullException(nameof(source));

    var result = new BulkImportResult { StartedAt = DateTime.UtcNow };
    var sessionId = Guid.NewGuid();
    var buffer = new List<T>(_options.BatchSize);
    var batchIndex = 0;

    async Task FlushAsync()
    {
      if (buffer.Count == 0) return;
      var batch = buffer.ToList();
      buffer.Clear();
      await ExecuteSingleBatchAsync(batch, batchIndex, result, -1, sessionId, progress, cancellationToken);
      batchIndex++;
    }

    await foreach (var entity in source.WithCancellation(cancellationToken))
    {
      buffer.Add(entity);
      result.TotalRead++;

      if (buffer.Count >= _options.BatchSize)
        await FlushAsync();
    }

    await FlushAsync();

    result.Duration = DateTime.UtcNow - result.StartedAt;
    _statistics.TotalImports++;
    _statistics.TotalRecordsImported += result.Succeeded;
    _statistics.TotalErrors += result.Failed;

    return result;
  }

  // ── Export ───────────────────────────────────────────────────────────────

  /// <inheritdoc/>
  public async Task<BulkExportResult> ExportToStreamAsync(
    Stream destination,
    ExportFormat format,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (destination is null) throw new ArgumentNullException(nameof(destination));

    var result = new BulkExportResult { StartedAt = DateTime.UtcNow, Format = format };

    try
    {
      var entities = (await _repository.GetAllAsync(cancellationToken)).ToList();
      result.TotalExported = entities.Count;

      var content = format switch
      {
        ExportFormat.Json => await _exportService.ExportAsJsonAsync(entities),
        ExportFormat.Csv => await _exportService.ExportAsCsvAsync(entities),
        ExportFormat.Xml => await _exportService.ExportAsXmlAsync(entities),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
      };

      var bytes = Encoding.UTF8.GetBytes(content);
      await destination.WriteAsync(bytes.AsMemory(), cancellationToken);
      await destination.FlushAsync(cancellationToken);

      result.BytesWritten = bytes.Length;
      result.IsSuccess = true;

      if (_options.EnableProgressReporting && progress is not null)
      {
        var snap = BuildProgress(
          result.TotalExported, result.TotalExported,
          result.TotalExported, 0, result.BytesWritten, result.StartedAt, 1);
        progress.Report(snap);
        _statistics.LastProgress = snap;
      }
    }
    catch (Exception ex)
    {
      result.IsSuccess = false;
      Console.Error.WriteLine($"[BulkTransfer] Export failed: {ex.Message}");
      throw;
    }

    result.Duration = DateTime.UtcNow - result.StartedAt;
    _statistics.TotalExports++;
    _statistics.TotalRecordsExported += result.TotalExported;
    _statistics.TotalBytesTransferred += result.BytesWritten;

    return result;
  }

  /// <inheritdoc/>
  public async Task<BulkExportResult> ExportToFileAsync(
    string filePath,
    ExportFormat format,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(filePath))
      throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

    var directory = Path.GetDirectoryName(filePath);
    if (!string.IsNullOrEmpty(directory))
      Directory.CreateDirectory(directory);

    await using var stream = new FileStream(
      filePath, FileMode.Create, FileAccess.Write, FileShare.None,
      _options.BufferSize, useAsync: true);

    var result = await ExportToStreamAsync(stream, format, progress, cancellationToken);
    result.DestinationPath = filePath;
    return result;
  }

  /// <inheritdoc/>
  public async Task<BulkExportResult> ExportFilteredAsync(
    Func<T, bool> predicate,
    Stream destination,
    ExportFormat format,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    if (predicate is null) throw new ArgumentNullException(nameof(predicate));
    if (destination is null) throw new ArgumentNullException(nameof(destination));

    var result = new BulkExportResult { StartedAt = DateTime.UtcNow, Format = format };

    try
    {
      var filtered = (await _repository.FindAsync(predicate, cancellationToken)).ToList();
      result.TotalExported = filtered.Count;

      var content = format switch
      {
        ExportFormat.Json => await _exportService.ExportAsJsonAsync(filtered),
        ExportFormat.Csv => await _exportService.ExportAsCsvAsync(filtered),
        ExportFormat.Xml => await _exportService.ExportAsXmlAsync(filtered),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
      };

      var bytes = Encoding.UTF8.GetBytes(content);
      await destination.WriteAsync(bytes.AsMemory(), cancellationToken);
      await destination.FlushAsync(cancellationToken);

      result.BytesWritten = bytes.Length;
      result.IsSuccess = true;

      if (_options.EnableProgressReporting && progress is not null)
      {
        var snap = BuildProgress(
          result.TotalExported, result.TotalExported,
          result.TotalExported, 0, result.BytesWritten, result.StartedAt, 1);
        progress.Report(snap);
        _statistics.LastProgress = snap;
      }
    }
    catch (Exception ex)
    {
      result.IsSuccess = false;
      Console.Error.WriteLine($"[BulkTransfer] Filtered export failed: {ex.Message}");
      throw;
    }

    result.Duration = DateTime.UtcNow - result.StartedAt;
    _statistics.TotalExports++;
    _statistics.TotalRecordsExported += result.TotalExported;
    _statistics.TotalBytesTransferred += result.BytesWritten;

    return result;
  }

  /// <inheritdoc/>
  public async IAsyncEnumerable<T> StreamEntitiesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    var all = await _repository.GetAllAsync(cancellationToken);

    foreach (var entity in all)
    {
      cancellationToken.ThrowIfCancellationRequested();
      yield return entity;
    }
  }

  // ── Transfer ─────────────────────────────────────────────────────────────

  /// <inheritdoc/>
  public async Task<BulkTransferResult> TransferAsync(
    Stream source,
    ImportFormat sourceFormat,
    Stream destination,
    ExportFormat destinationFormat,
    Func<T, T?>? transform = null,
    IProgress<BulkTransferProgress>? progress = null,
    CancellationToken cancellationToken = default)
  {
    var importResult = await ImportFromStreamAsync(source, sourceFormat, progress, cancellationToken);

    var allEntities = (IEnumerable<T>)await _repository.GetAllAsync(cancellationToken);

    if (transform is not null)
      allEntities = allEntities.Select(transform).OfType<T>().ToList();

    var exportResult = new BulkExportResult { StartedAt = DateTime.UtcNow, Format = destinationFormat };

    try
    {
      var content = destinationFormat switch
      {
        ExportFormat.Json => await _exportService.ExportAsJsonAsync(allEntities),
        ExportFormat.Csv => await _exportService.ExportAsCsvAsync(allEntities),
        ExportFormat.Xml => await _exportService.ExportAsXmlAsync(allEntities),
        _ => throw new ArgumentOutOfRangeException(nameof(destinationFormat))
      };

      var bytes = Encoding.UTF8.GetBytes(content);
      await destination.WriteAsync(bytes.AsMemory(), cancellationToken);
      await destination.FlushAsync(cancellationToken);

      exportResult.TotalExported = importResult.Succeeded;
      exportResult.BytesWritten = bytes.Length;
      exportResult.IsSuccess = true;
    }
    catch (Exception ex)
    {
      exportResult.IsSuccess = false;
      Console.Error.WriteLine($"[BulkTransfer] Transfer export phase failed: {ex.Message}");
    }

    exportResult.Duration = DateTime.UtcNow - exportResult.StartedAt;
    return new BulkTransferResult(importResult, exportResult);
  }

  /// <inheritdoc/>
  public BulkTransferStatistics GetStatistics() => _statistics;

  // ── Private helpers ───────────────────────────────────────────────────────

  private async Task ExecuteSingleBatchAsync(
    List<T> batch,
    int batchIndex,
    BulkImportResult result,
    long totalCount,
    Guid sessionId,
    IProgress<BulkTransferProgress>? progress,
    CancellationToken cancellationToken)
  {
    var batchStartRow = (long)batchIndex * _options.BatchSize;
    var attempt = 0;
    var retriedThisBatch = false;

    while (true)
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(_options.BatchTimeout);

      try
      {
        await _repository.AddRangeAsync(batch, cts.Token);
        result.Succeeded += batch.Count;
        result.BatchesCommitted++;
        if (retriedThisBatch)
          _statistics.RetriedBatches++;
        break;
      }
      catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
      {
        result.Failed += batch.Count;
        for (var i = 0; i < batch.Count; i++)
        {
          result.Errors.Add(new BulkTransferError
          {
            RowNumber = batchStartRow + i,
            Message = $"Batch {batchIndex + 1} timed out after {_options.BatchTimeout.TotalSeconds}s."
          });
        }
        Console.Error.WriteLine($"[BulkTransfer] Batch {batchIndex + 1} timed out.");
        break;
      }
      catch (Exception ex) when (IsTransientLockError(ex) && attempt < _options.MaxRetryAttempts)
      {
        attempt++;
        retriedThisBatch = true;
        _statistics.TotalRetryAttempts++;

        var delay = ComputeRetryDelay(attempt);
        Console.Error.WriteLine(
          $"[BulkTransfer] Batch {batchIndex + 1} hit a transient lock error " +
          $"({ex.Message}); retrying attempt {attempt}/{_options.MaxRetryAttempts} after {delay.TotalMilliseconds:F0}ms.");

        try
        {
          await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
          throw;
        }
      }
      catch (Exception ex)
      {
        var isExhaustedRetry = IsTransientLockError(ex) && attempt >= _options.MaxRetryAttempts;
        if (isExhaustedRetry)
          _statistics.RetriesExhausted++;

        result.Failed += batch.Count;
        var message = isExhaustedRetry
          ? $"Batch {batchIndex + 1} still locked after {attempt} retr{(attempt == 1 ? "y" : "ies")}: {ex.Message}"
          : ex.Message;

        for (var i = 0; i < batch.Count; i++)
        {
          result.Errors.Add(new BulkTransferError
          {
            RowNumber = batchStartRow + i,
            Message = message,
            InnerException = ex
          });
        }
        Console.Error.WriteLine($"[BulkTransfer] Batch {batchIndex + 1} failed: {message}");
        break;
      }
    }

    if (!_options.EnableProgressReporting || progress is null)
      return;

    var processed = result.Succeeded + result.Failed;
    var threshold = Math.Max(1, _options.ProgressReportingInterval / Math.Max(1, _options.BatchSize));

    if ((batchIndex + 1) % threshold == 0 || processed >= totalCount)
    {
      var snap = BuildProgress(processed, totalCount, result.Succeeded, result.Failed, 0, result.StartedAt, batchIndex + 1);
      progress.Report(snap);
      _statistics.LastProgress = snap;
    }

    if (_options.EnableCheckpointing && !string.IsNullOrEmpty(_options.CheckpointFilePath))
      await SaveCheckpointAsync(sessionId, result.Succeeded, batchIndex);
  }

  private IEnumerable<T> DeserializeEntities(string content, ImportFormat format)
  {
    try
    {
      return format switch
      {
        ImportFormat.Json => JsonSerializer.Deserialize<List<T>>(
          content,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
          ?? Enumerable.Empty<T>(),

        ImportFormat.Csv => new CsvFormatter().ParseCollection<T>(content)
          ?? Enumerable.Empty<T>(),

        ImportFormat.Xml => new XmlFormatter().ParseCollection<T>(content)
          ?? Enumerable.Empty<T>(),

        _ => throw new ArgumentOutOfRangeException(nameof(format), $"Unsupported import format: {format}")
      };
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException(
        $"Failed to deserialize {format} content: {ex.Message}", ex);
    }
  }

  private async Task SaveCheckpointAsync(Guid sessionId, long committed, int batchIndex)
  {
    try
    {
      var checkpoint = new TransferCheckpoint
      {
        SessionId = sessionId,
        CommittedCount = committed,
        LastCommittedBatch = batchIndex
      };

      var json = JsonSerializer.Serialize(checkpoint);
      await File.WriteAllTextAsync(_options.CheckpointFilePath!, json);
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine($"[BulkTransfer] Checkpoint save failed: {ex.Message}");
    }
  }

  /// <summary>
  /// Determines whether the supplied exception represents a transient SQLite lock
  /// contention condition (<c>SQLITE_BUSY</c> = 5 or <c>SQLITE_LOCKED</c> = 6) that is
  /// safe to retry, as opposed to a permanent failure such as a constraint violation
  /// (<c>SQLITE_CONSTRAINT</c> = 19) or database corruption (<c>SQLITE_CORRUPT</c> = 11).
  /// Unwraps <see cref="RepositoryException"/> wrappers to inspect the underlying
  /// <see cref="SqliteException"/> when present.
  /// </summary>
  /// <param name="exception">The exception raised by the failed batch write.</param>
  /// <returns><c>true</c> when the failure is a transient lock error eligible for retry.</returns>
  private static bool IsTransientLockError(Exception exception)
  {
    var sqliteException = exception as SqliteException
      ?? exception.InnerException as SqliteException;

    if (sqliteException is null)
      return false;

    // Mask off SQLite's extended result code bits (upper byte) to compare against the
    // primary result code, since drivers may surface either form.
    var primaryErrorCode = sqliteException.SqliteErrorCode & 0xFF;
    return primaryErrorCode is 5 or 6; // SQLITE_BUSY, SQLITE_LOCKED
  }

  /// <summary>
  /// Computes the exponential backoff delay for the given retry attempt, combining
  /// <see cref="BulkTransferOptions.RetryBaseDelay"/> doubled per attempt with random
  /// full jitter, capped at <see cref="BulkTransferOptions.RetryMaxDelay"/>.
  /// </summary>
  /// <param name="attempt">One-based retry attempt number.</param>
  /// <returns>The delay to wait before the next retry.</returns>
  private TimeSpan ComputeRetryDelay(int attempt)
  {
    var exponential = _options.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
    var capped = Math.Min(exponential, _options.RetryMaxDelay.TotalMilliseconds);
    var jittered = capped * _retryJitter.NextDouble();
    return TimeSpan.FromMilliseconds(Math.Max(1, jittered));
  }

  private static IEnumerable<List<T>> Partition(List<T> source, int size)
  {
    for (var i = 0; i < source.Count; i += size)
      yield return source.GetRange(i, Math.Min(size, source.Count - i));
  }

  private static BulkTransferProgress BuildProgress(
    long processed, long total, long succeeded, long failed,
    long bytes, DateTime startedAt, int batchIndex) =>
    new(processed, total, succeeded, failed, bytes, startedAt, batchIndex);
}