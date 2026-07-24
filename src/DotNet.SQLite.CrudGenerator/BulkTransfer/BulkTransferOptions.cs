#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Controls how validation failures are handled during a bulk import operation.
/// </summary>
public enum ValidationMode
{
    /// <summary>Reject the entire operation on the first record that fails validation.</summary>
    Strict,
    /// <summary>Skip invalid records and continue importing valid ones.</summary>
    Lenient,
    /// <summary>Disable per-record validation entirely for maximum ingestion throughput.</summary>
    None
}

/// <summary>
/// Serialization formats accepted by bulk import operations.
/// </summary>
public enum ImportFormat
{
    /// <summary>JSON array or newline-delimited JSON (NDJSON).</summary>
    Json,
    /// <summary>Comma-separated values with an optional header row.</summary>
    Csv,
    /// <summary>XML document with a root element wrapping individual entity elements.</summary>
    Xml
}

/// <summary>
/// Configuration options that govern the behavior of <see cref="BulkImportExportEngine{T}"/>.
/// Controls batching, concurrency, progress reporting, checkpointing, and error thresholds.
/// </summary>
public sealed class BulkTransferOptions
{
    /// <summary>
    /// Number of entities per database write batch. Larger values reduce round-trips but
    /// increase per-failure blast radius. Default: 500.
    /// </summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// Maximum number of concurrent batch write tasks. Increasing this value improves
    /// throughput on multi-core hosts but raises memory and connection pressure. Default: 4.
    /// </summary>
    public int MaxConcurrency { get; set; } = 4;

    /// <summary>
    /// Enables emission of <see cref="BulkTransferProgress"/> snapshots during the operation.
    /// Disable for tight-loop scenarios where observer overhead is measurable. Default: true.
    /// </summary>
    public bool EnableProgressReporting { get; set; } = true;

    /// <summary>
    /// Minimum number of records processed between consecutive progress notifications.
    /// Lower values produce finer-grained feedback at higher callback overhead. Default: 100.
    /// </summary>
    public int ProgressReportingInterval { get; set; } = 100;

    /// <summary>
    /// Stream read/write buffer size in bytes. Larger buffers reduce I/O syscalls on
    /// sequential workloads. Default: 65 536 (64 KB).
    /// </summary>
    public int BufferSize { get; set; } = 65_536;

    /// <summary>
    /// Persist checkpoint state to disk so that interrupted imports can be resumed from the
    /// last committed batch rather than restarted from scratch. Default: false.
    /// </summary>
    public bool EnableCheckpointing { get; set; } = false;

    /// <summary>
    /// Absolute file path used to read and write checkpoint state.
    /// Required when <see cref="EnableCheckpointing"/> is <c>true</c>.
    /// </summary>
    public string? CheckpointFilePath { get; set; }

    /// <summary>
    /// Governs how per-record validation failures are handled during import. Default: <see cref="ValidationMode.Lenient"/>.
    /// </summary>
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Lenient;

    /// <summary>
    /// Maximum cumulative validation or persistence errors before the operation is aborted.
    /// Default: 1 000.
    /// </summary>
    public int MaxErrorThreshold { get; set; } = 1_000;

    /// <summary>
    /// When <c>true</c>, each batch write is wrapped in a database transaction, providing
    /// atomicity at the batch level. Default: true.
    /// </summary>
    public bool UseTransactions { get; set; } = true;

    /// <summary>
    /// Per-batch operation timeout. Batches that exceed this duration are cancelled and
    /// recorded as failures, but the overall import continues. Default: 30 seconds.
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts for a batch commit that fails with a transient
    /// SQLite lock contention error (<c>SQLITE_BUSY</c> or <c>SQLITE_LOCKED</c>). A value of
    /// <c>0</c> disables retries entirely. Default: 5.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>
    /// Base delay used to compute exponential backoff between retry attempts. The actual
    /// delay for attempt <c>n</c> is <c>RetryBaseDelay * 2^(n-1)</c> plus random jitter,
    /// capped at <see cref="RetryMaxDelay"/>. Default: 50 milliseconds.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Upper bound on the backoff delay applied between retry attempts, regardless of how
    /// many attempts have already elapsed. Default: 5 seconds.
    /// </summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Returns a default options instance appropriate for most production workloads.
    /// </summary>
    public static BulkTransferOptions Default => new();

    /// <summary>
    /// Returns an options instance tuned for maximum ingestion throughput.
    /// Increases batch size, disables per-record validation, and raises concurrency.
    /// Not recommended when data quality cannot be guaranteed.
    /// </summary>
    public static BulkTransferOptions HighThroughput => new()
    {
        BatchSize = 2_000,
        MaxConcurrency = 8,
        ValidationMode = ValidationMode.None,
        ProgressReportingInterval = 500,
        EnableCheckpointing = false,
        BatchTimeout = TimeSpan.FromMinutes(2),
        MaxRetryAttempts = 8,
        RetryBaseDelay = TimeSpan.FromMilliseconds(25),
        RetryMaxDelay = TimeSpan.FromSeconds(10)
    };

    /// <summary>
    /// Returns an options instance optimised for safety over speed.
    /// Uses strict validation, small batches, and checkpointing enabled.
    /// Requires <see cref="CheckpointFilePath"/> to be set before use.
    /// </summary>
    public static BulkTransferOptions Safe => new()
    {
        BatchSize = 100,
        MaxConcurrency = 2,
        ValidationMode = ValidationMode.Strict,
        EnableCheckpointing = true,
        MaxErrorThreshold = 10,
        BatchTimeout = TimeSpan.FromSeconds(15),
        MaxRetryAttempts = 3,
        RetryBaseDelay = TimeSpan.FromMilliseconds(100),
        RetryMaxDelay = TimeSpan.FromSeconds(3)
    };
}
