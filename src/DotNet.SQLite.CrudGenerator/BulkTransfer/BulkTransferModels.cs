#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json.Serialization;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Immutable snapshot of bulk transfer progress emitted at configurable intervals.
/// Carries all metrics required to render a progress bar, ETA, and throughput indicator.
/// </summary>
/// <param name="ProcessedCount">Cumulative records processed (succeeded + failed) so far.</param>
/// <param name="TotalCount">Total records expected; <c>-1</c> when the source length is unknown.</param>
/// <param name="SucceededCount">Records successfully committed to the data store.</param>
/// <param name="FailedCount">Records that could not be processed due to errors or validation.</param>
/// <param name="BytesTransferred">Raw bytes read from or written to the I/O stream.</param>
/// <param name="StartedAt">UTC instant when the transfer operation began.</param>
/// <param name="CurrentBatch">Zero-based index of the batch currently being processed.</param>
public sealed record BulkTransferProgress(
    long ProcessedCount,
    long TotalCount,
    long SucceededCount,
    long FailedCount,
    long BytesTransferred,
    DateTime StartedAt,
    int CurrentBatch)
{
    /// <summary>
    /// Percentage complete in the range [0, 100].
    /// Returns <c>-1</c> when <see cref="TotalCount"/> is unknown.
    /// </summary>
    [JsonIgnore]
    public double PercentComplete =>
        TotalCount > 0
            ? Math.Round((double)ProcessedCount / TotalCount * 100, 2)
            : -1;

    /// <summary>
    /// Average records processed per second since the operation began.
    /// </summary>
    [JsonIgnore]
    public double Throughput
    {
        get
        {
            var elapsedSeconds = (DateTime.UtcNow - StartedAt).TotalSeconds;
            return elapsedSeconds > 0 ? Math.Round(ProcessedCount / elapsedSeconds, 1) : 0;
        }
    }

    /// <summary>
    /// Estimated time remaining based on current throughput.
    /// Returns <c>null</c> when total is unknown or throughput is zero.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (TotalCount <= 0 || Throughput <= 0)
                return null;

            var remaining = TotalCount - ProcessedCount;
            return TimeSpan.FromSeconds(remaining / Throughput);
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        TotalCount > 0
            ? $"[{PercentComplete:F1}%] {ProcessedCount:N0}/{TotalCount:N0} records — {Throughput:N0} rec/s"
            : $"{ProcessedCount:N0} records processed — {Throughput:N0} rec/s";
}

/// <summary>
/// Reason for operation completion status.
/// </summary>
public enum CancellationReason
{
    /// <summary>Operation completed normally without cancellation.</summary>
    None,

    /// <summary>Operation was cancelled by the caller via CancellationToken.</summary>
    Cancelled,

    /// <summary>Operation failed due to an error.</summary>
    Failed
}

/// <summary>
/// Describes a single record-level failure that occurred during a bulk import or export.
/// </summary>
public sealed record BulkTransferError
{
    /// <summary>Zero-based row number of the offending record in the source stream or batch.</summary>
    public required long RowNumber { get; init; }

    /// <summary>Human-readable description of the failure reason.</summary>
    public required string Message { get; init; }

    /// <summary>
    /// Raw serialized representation of the offending record as it appeared in the source,
    /// truncated to 512 characters for safety. <c>null</c> when unavailable.
    /// </summary>
    public string? RawRecord { get; init; }

    /// <summary>
    /// The exception that caused this failure. Excluded from serialization to avoid
    /// leaking internal stack frames through public APIs.
    /// </summary>
    [JsonIgnore]
    public Exception? InnerException { get; init; }
}

/// <summary>
/// Comprehensive outcome of a completed bulk import operation.
/// </summary>
public sealed class BulkImportResult
{
    /// <summary>Total records read from the source (includes both successes and failures).</summary>
    public long TotalRead { get; set; }

    /// <summary>Records that were successfully inserted or upserted into the data store.</summary>
    public long Succeeded { get; set; }

    /// <summary>Records rejected due to validation failures or database errors.</summary>
    public long Failed { get; set; }

    /// <summary>Number of discrete batch transactions committed to the database.</summary>
    public int BatchesCommitted { get; set; }

    /// <summary>Wall-clock duration from the first record read to the last batch committed.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>UTC instant the import operation started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Reason for the operation completion status.
    /// </summary>
    public CancellationReason CancellationReason { get; set; } = CancellationReason.None;

    /// <summary>
    /// Bulk transfer options used for this operation, if available.
    /// </summary>
    [JsonIgnore]
    internal BulkTransferOptions? _options;

    /// <summary>
    /// <c>true</c> when at least one record succeeded, even if some records failed.
    /// <c>false</c> only when every record in the source failed.
    /// </summary>
    public bool IsSuccess => CancellationReason == CancellationReason.Cancelled ? false : (Failed == 0 || Succeeded > 0);

    /// <summary>
    /// <c>true</c> when the operation was cancelled by the caller.
    /// </summary>
    [JsonIgnore]
    public bool IsCancelled => CancellationReason == CancellationReason.Cancelled;

    /// <summary>
    /// <c>true</c> when the operation was cancelled by the caller and checkpointing is enabled,
    /// indicating that a checkpoint file exists for resumption.
    /// </summary>
    [JsonIgnore]
    public bool HasCheckpointForCancellation => IsCancelled && _options?.EnableCheckpointing == true;

    /// <summary>
    /// <c>true</c> when the operation completed successfully without cancellation or errors.
    /// </summary>
    [JsonIgnore]
    public bool IsCompletedSuccessfully => IsSuccess && !IsCancelled;

    /// <summary>
    /// Ordered list of per-record failures. Capped by <see cref="BulkTransferOptions.MaxErrorThreshold"/>.
    /// </summary>
    public List<BulkTransferError> Errors { get; set; } = new();

    /// <summary>Average records successfully inserted per second.</summary>
    public double Throughput =>
        Duration.TotalSeconds > 0
            ? Math.Round(TotalRead / Duration.TotalSeconds, 1)
            : 0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Import complete: {Succeeded:N0} succeeded, {Failed:N0} failed " +
        (IsCancelled ? "(CANCELLED) " : "") +
        $"in {Duration.TotalSeconds:F2}s ({Throughput:N0} rec/s)";
}

/// <summary>
/// Comprehensive outcome of a completed bulk export operation.
/// </summary>
public sealed class BulkExportResult
{
    /// <summary>Total entity records written to the destination.</summary>
    public long TotalExported { get; set; }

    /// <summary>Total bytes written to the destination stream or file.</summary>
    public long BytesWritten { get; set; }

    /// <summary>The serialization format used for the export.</summary>
    public ExportFormat Format { get; set; }

    /// <summary>Absolute file path of the export destination; <c>null</c> for stream-only exports.</summary>
    public string? DestinationPath { get; set; }

    /// <summary>Wall-clock duration of the export operation.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>UTC instant the export started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary><c>true</c> when the export completed without an unhandled exception.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Average records serialized and written per second.</summary>
    public double Throughput =>
        Duration.TotalSeconds > 0
            ? Math.Round(TotalExported / Duration.TotalSeconds, 1)
            : 0;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Export complete: {TotalExported:N0} records ({BytesWritten:N0} bytes) " +
        $"in {Duration.TotalSeconds:F2}s ({Throughput:N0} rec/s)";
}

/// <summary>
/// Combined result when data is piped from one stream directly into another via
/// <see cref="IBulkTransferService{T}.TransferAsync"/>.
/// </summary>
/// <param name="Import">Outcome of the import (read) phase.</param>
/// <param name="Export">Outcome of the export (write) phase.</param>
public sealed record BulkTransferResult(BulkImportResult Import, BulkExportResult Export)
{
    /// <summary><c>true</c> only when both the import and export phases completed successfully.</summary>
    public bool IsSuccess => Import.IsSuccess && Export.IsSuccess;

    /// <inheritdoc/>
    public override string ToString() =>
        $"Transfer: import [{Import}] | export [{Export}]";
}

/// <summary>
/// Cumulative session-level statistics for a single <see cref="BulkImportExportEngine{T}"/> instance.
/// Aggregated across all operations performed since the engine was constructed.
/// </summary>
public sealed class BulkTransferStatistics
{
    /// <summary>Total import operations executed in this session.</summary>
    public long TotalImports { get; set; }

    /// <summary>Total export operations executed in this session.</summary>
    public long TotalExports { get; set; }

    /// <summary>Cumulative records successfully imported across all import operations.</summary>
    public long TotalRecordsImported { get; set; }

    /// <summary>Cumulative records successfully exported across all export operations.</summary>
    public long TotalRecordsExported { get; set; }

    /// <summary>Cumulative record-level failures across all operations.</summary>
    public long TotalErrors { get; set; }

    /// <summary>Total bytes transferred in both directions across all operations.</summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>UTC instant this statistics object was created (i.e. the engine session start).</summary>
    public DateTime SessionStartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Most recent progress snapshot received from any active operation.</summary>
    public BulkTransferProgress? LastProgress { get; set; }

    /// <summary>
    /// Number of batches that required at least one retry attempt due to a transient
    /// SQLite lock contention error (<c>SQLITE_BUSY</c> or <c>SQLITE_LOCKED</c>).
    /// </summary>
    public long RetriedBatches { get; set; }

    /// <summary>
    /// Cumulative number of individual retry attempts performed across all batches,
    /// regardless of whether the batch ultimately succeeded.
    /// </summary>
    public long TotalRetryAttempts { get; set; }

    /// <summary>
    /// Number of batches whose retry budget was exhausted without a successful commit,
    /// resulting in the batch's records being recorded as <see cref="BulkTransferError"/> entries.
    /// </summary>
    public long RetriesExhausted { get; set; }
}

/// <summary>
/// Durable checkpoint state persisted to disk so that an interrupted import operation
/// can be resumed from the last committed batch rather than replayed from the beginning.
/// </summary>
public sealed class TransferCheckpoint
{
    /// <summary>Unique identifier linking this checkpoint to the originating transfer session.</summary>
    public required Guid SessionId { get; init; }

    /// <summary>Number of records already committed before this checkpoint was saved.</summary>
    public required long CommittedCount { get; init; }

    /// <summary>Zero-based index of the last batch that was committed successfully.</summary>
    public required int LastCommittedBatch { get; init; }

    /// <summary>UTC instant this checkpoint was persisted to disk.</summary>
    public DateTime SavedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional caller-supplied label for identifying the source operation in monitoring tools.
    /// </summary>
    public string? OperationTag { get; init; }
}