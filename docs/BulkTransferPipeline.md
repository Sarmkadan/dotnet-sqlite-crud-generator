# BulkTransferPipeline

A fluent, configurable pipeline for bulk data transfer operations between SQLite databases and external sources. Supports import, export, and direct transfer scenarios with built-in progress reporting, error handling, retry logic, and data transformation capabilities.

## API

### `public BulkTransferPipeline()`

Initializes a new instance of the pipeline with default configuration. Use `Create<T>()` for a typed pipeline instance.

### `public static BulkTransferPipeline<T> Create<T>()`

Creates a new typed pipeline instance for entity type `T`.

**Returns:** A configured `BulkTransferPipeline<T>` ready for fluent configuration.

### `public BulkTransferPipeline<T> WithOptions(Action<BulkTransferOptions> configure)`

Configures pipeline behavior through the provided options delegate.

**Parameters:**
- `configure`: Delegate to modify `BulkTransferOptions` (batch size, timeout, transaction behavior, etc.)

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `configure` is null.

### `public BulkTransferPipeline<T> WithTransform(Func<T, T> transform)`

Registers a transformation function applied to each entity during transfer.

**Parameters:**
- `transform`: Function that takes an entity of type `T` and returns a transformed entity of type `T`.

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `transform` is null.

### `public BulkTransferPipeline<T> WithFilter(Func<T, bool> predicate)`

Registers a filter predicate to include only matching entities in the transfer.

**Parameters:**
- `predicate`: Function that returns `true` for entities to include, `false` to exclude.

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `predicate` is null.

### `public BulkTransferPipeline<T> WithProgress(IProgress<BulkTransferProgress> progress)`

Registers a progress reporter for transfer operations.

**Parameters:**
- `progress`: `IProgress<BulkTransferProgress>` implementation to receive periodic updates.

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `progress` is null.

### `public BulkTransferPipeline<T> OnError(Action<BulkTransferErrorContext> handler)`

Registers an error handler invoked when a batch or entity fails during transfer.

**Parameters:**
- `handler`: Delegate receiving `BulkTransferErrorContext` with error details, entity data, and retry decision.

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `handler` is null.

### `public BulkTransferPipeline<T> WithRetry(BulkRetryPolicy policy)`

Configures retry behavior for transient failures.

**Parameters:**
- `policy`: `BulkRetryPolicy` defining max attempts, backoff strategy, and retryable exception types.

**Returns:** The same pipeline instance for method chaining.

**Throws:** `ArgumentNullException` if `policy` is null.

### `public async Task<BulkImportResult> ImportFromFileAsync(string filePath, CancellationToken cancellationToken = default)`

Imports entities from a file (CSV, JSON, or SQLite) into the target database.

**Parameters:**
- `filePath`: Path to the source file.
- `cancellationToken`: Token to cancel the operation.

**Returns:** `BulkImportResult` containing rows processed, errors, and duration.

**Throws:** `FileNotFoundException` if file does not exist. `InvalidDataException` if file format is unrecognized or corrupt. `OperationCanceledException` if cancelled.

### `public async Task<BulkImportResult> ImportFromStreamAsync(Stream stream, BulkImportFormat format, CancellationToken cancellationToken = default)`

Imports entities from a stream with explicit format specification.

**Parameters:**
- `stream`: Readable stream containing source data.
- `format`: `BulkImportFormat` enum (Csv, Json, Sqlite).
- `cancellationToken`: Token to cancel the operation.

**Returns:** `BulkImportResult` containing rows processed, errors, and duration.

**Throws:** `ArgumentNullException` if `stream` is null. `InvalidDataException` if stream data is malformed. `OperationCanceledException` if cancelled.

### `public Task<BulkExportResult> ExportToFileAsync(string filePath, BulkExportFormat format, CancellationToken cancellationToken = default)`

Exports entities from the database to a file.

**Parameters:**
- `filePath`: Destination file path.
- `format`: `BulkExportFormat` enum (Csv, Json, Sqlite).
- `cancellationToken`: Token to cancel the operation.

**Returns:** `BulkExportResult` containing rows written, file size, and duration.

**Throws:** `ArgumentNullException` if `filePath` is null or empty. `UnauthorizedAccessException` if path is not writable. `OperationCanceledException` if cancelled.

### `public Task<BulkExportResult> ExportToStreamAsync(Stream stream, BulkExportFormat format, CancellationToken cancellationToken = default)`

Exports entities from the database to a stream.

**Parameters:**
- `stream`: Writable destination stream.
- `format`: `BulkExportFormat` enum (Csv, Json, Sqlite).
- `cancellationToken`: Token to cancel the operation.

**Returns:** `BulkExportResult` containing rows written, bytes written, and duration.

**Throws:** `ArgumentNullException` if `stream` is null. `InvalidOperationException` if stream is not writable. `OperationCanceledException` if cancelled.

### `public async Task<BulkTransferResult> TransferAsync(string sourceConnectionString, string destinationConnectionString, CancellationToken cancellationToken = default)`

Transfers entities directly between two SQLite databases.

**Parameters:**
- `sourceConnectionString`: Connection string for the source database.
- `destinationConnectionString`: Connection string for the destination database.
- `cancellationToken`: Token to cancel the operation.

**Returns:** `BulkTransferResult` containing rows transferred, errors, and duration.

**Throws:** `ArgumentException` if connection strings are invalid. `SqliteException` on database errors. `OperationCanceledException` if cancelled.

### `public BulkTransferStatistics GetStatistics()`

Retrieves cumulative statistics for the current pipeline instance.

**Returns:** `BulkTransferStatistics` with totals for processed, succeeded, failed, and retried entities across all operations.

## Usage

### Import CSV with transformation and progress reporting

```csharp
var result = await BulkTransferPipeline.Create<Customer>()
    .WithOptions(o => o.BatchSize = 5000)
    .WithTransform(c => c with { Email = c.Email.ToLowerInvariant() })
    .WithFilter(c => c.IsActive)
    .WithProgress(new Progress<BulkTransferProgress>(p => 
        Console.WriteLine($"Processed: {p.Processed}, Errors: {p.Errors}")))
    .OnError(ctx => 
    {
        if (ctx.Exception is SqliteException sqlEx && sqlEx.SqliteErrorCode == 19)
            ctx.Retry = false; // Don't retry constraint violations
    })
    .WithRetry(new BulkRetryPolicy 
    { 
        MaxAttempts = 3, 
        Backoff = TimeSpan.FromSeconds(2) 
    })
    .ImportFromFileAsync("customers.csv");

Console.WriteLine($"Imported: {result.SuccessCount}, Failed: {result.ErrorCount}");
```

### Direct database-to-database transfer with statistics

```csharp
var pipeline = BulkTransferPipeline.Create<Order>()
    .WithOptions(o => 
    {
        o.BatchSize = 10000;
        o.UseTransaction = true;
        o.ForeignKeys = false;
    })
    .WithFilter(o => o.CreatedDate >= DateTime.UtcNow.AddDays(-30));

var result = await pipeline.TransferAsync(
    "Data Source=source.db",
    "Data Source=dest.db");

var stats = pipeline.GetStatistics();
Console.WriteLine($"Total processed: {stats.TotalProcessed}");
Console.WriteLine($"Success rate: {stats.SuccessRate:P2}");
Console.WriteLine($"Retries: {stats.TotalRetries}");
```

## Notes

- **Thread safety:** `BulkTransferPipeline<T>` instances are not thread-safe. Create separate instances for concurrent operations. The static `Create<T>()` method is thread-safe.
- **Resource disposal:** Streams passed to `ImportFromStreamAsync` and `ExportToStreamAsync` are not disposed by the pipeline; caller retains ownership.
- **Transaction behavior:** By default, each batch runs in its own transaction. Set `UseTransaction = false` in options for autocommit mode (faster but no rollback on batch failure).
- **Filter and transform order:** Filters are applied before transforms. Entities excluded by `WithFilter` never reach `WithTransform` or the error handler.
- **Progress frequency:** Progress reports are emitted per batch. For small batch sizes, consider throttling UI updates to avoid overhead.
- **Retry policy scope:** `WithRetry` applies to transient SQLite errors (busy, locked, I/O). Constraint violations and schema errors are never retried regardless of policy.
- **Statistics lifetime:** `GetStatistics()` returns cumulative data since pipeline creation. Statistics are not reset between operations on the same instance.
- **Cancellation:** All async methods honor `CancellationToken`. Cancellation between batches is immediate; in-progress batches complete before stopping.
- **Format detection:** `ImportFromFileAsync` infers format from file extension (.csv, .json, .sqlite/.db). Explicit format via `ImportFromStreamAsync` is required for streams.
