# BulkTransferOptions

`BulkTransferOptions` encapsulates configuration parameters for bulk data transfer operations in the `dotnet-sqlite-crud-generator` library. It controls batching, concurrency, progress reporting, checkpointing, validation, error handling, transaction usage, and timeouts. Instances are typically created and populated before being passed to a bulk transfer method; after configuration, the object is intended to be read-only during the transfer.

## API

### `public int BatchSize`

Gets or sets the number of rows per batch.  
**Purpose:** Determines how many rows are grouped together for a single insert/update operation.  
**Parameters:** None (property).  
**Return value:** The current batch size.  
**Throws:** `ArgumentOutOfRangeException` if set to a value less than 1.

### `public int MaxConcurrency`

Gets or sets the maximum number of concurrent batch operations.  
**Purpose:** Limits parallelism when multiple batches are processed simultaneously.  
**Parameters:** None (property).  
**Return value:** The current concurrency limit.  
**Throws:** `ArgumentOutOfRangeException` if set to a value less than 1.

### `public bool EnableProgressReporting`

Gets or sets a flag indicating whether progress reporting is enabled.  
**Purpose:** When `true`, the transfer operation periodically raises progress events or updates a progress callback.  
**Parameters:** None (property).  
**Return value:** `true` if progress reporting is enabled; otherwise `false`.  
**Throws:** None.

### `public int ProgressReportingInterval`

Gets or sets the interval (in rows) between progress reports.  
**Purpose:** Defines how many rows are processed before a new progress notification is emitted. Only meaningful when `EnableProgressReporting` is `true`.  
**Parameters:** None (property).  
**Return value:** The current interval in rows.  
**Throws:** `ArgumentOutOfRangeException` if set to a value less than 1.

### `public int BufferSize`

Gets or sets the size (in bytes) of the internal buffer used for reading source data.  
**Purpose:** Controls memory allocation for data streaming; larger values may improve throughput at the cost of memory.  
**Parameters:** None (property).  
**Return value:** The current buffer size in bytes.  
**Throws:** `ArgumentOutOfRangeException` if set to a value less than 1.

### `public bool EnableCheckpointing`

Gets or sets a flag indicating whether checkpointing is enabled.  
**Purpose:** When `true`, the transfer saves its progress to a file so that it can resume after an interruption.  
**Parameters:** None (property).  
**Return value:** `true` if checkpointing is enabled; otherwise `false`.  
**Throws:** None.

### `public string? CheckpointFilePath`

Gets or sets the file path for the checkpoint file.  
**Purpose:** Specifies where checkpoint state is persisted. If `null` and `EnableCheckpointing` is `true`, a default path may be used.  
**Parameters:** None (property).  
**Return value:** The current checkpoint file path, or `null`.  
**Throws:** `ArgumentException` if set to an empty or whitespace-only string (but not `null`).

### `public ValidationMode ValidationMode`

Gets or sets the validation mode applied to each row before transfer.  
**Purpose:** Controls whether and how data is validated (e.g., `None`, `Strict`, `Loose`). The exact behavior depends on the `ValidationMode` enum definition.  
**Parameters:** None (property).  
**Return value:** The current validation mode.  
**Throws:** `ArgumentException` if set to an undefined enum value.

### `public int MaxErrorThreshold`

Gets or sets the maximum number of row-level errors allowed before the transfer is aborted.  
**Purpose:** Provides a tolerance for non-fatal errors (e.g., validation failures).  
**Parameters:** None (property).  
**Return value:** The current error threshold.  
**Throws:** `ArgumentOutOfRangeException` if set to a negative value.

### `public bool UseTransactions`

Gets or sets a flag indicating whether each batch is wrapped in a database transaction.  
**Purpose:** When `true`, each batch is committed atomically; when `false`, rows are inserted without explicit transaction boundaries.  
**Parameters:** None (property).  
**Return value:** `true` if transactions are used; otherwise `false`.  
**Throws:** None.

### `public TimeSpan BatchTimeout`

Gets or sets the maximum time allowed for a single batch to complete.  
**Purpose:** Prevents a batch from hanging indefinitely.  
**Parameters:** None (property).  
**Return value:** The current timeout duration.  
**Throws:** `ArgumentOutOfRangeException` if set to a value less than or equal to `TimeSpan.Zero`.

## Usage

### Example 1: Basic configuration for a high‑throughput transfer

```csharp
var options = new BulkTransferOptions
{
    BatchSize = 5000,
    MaxConcurrency = 4,
    BufferSize = 65536,
    UseTransactions = true,
    BatchTimeout = TimeSpan.FromMinutes(5),
    ValidationMode = ValidationMode.None,
    MaxErrorThreshold = 10
};

// Pass options to a bulk transfer method
await BulkTransferAsync(sourceData, destinationTable, options);
```

### Example 2: Configuration with progress reporting and checkpointing

```csharp
var options = new BulkTransferOptions
{
    BatchSize = 1000,
    MaxConcurrency = 2,
    EnableProgressReporting = true,
    ProgressReportingInterval = 500,
    EnableCheckpointing = true,
    CheckpointFilePath = @".\checkpoint.dat",
    UseTransactions = true,
    BatchTimeout = TimeSpan.FromSeconds(120),
    ValidationMode = ValidationMode.Strict,
    MaxErrorThreshold = 5
};

// Subscribe to progress events (assuming an event or callback)
options.ProgressChanged += (sender, args) =>
{
    Console.WriteLine($"Processed {args.RowsProcessed} rows");
};

await BulkTransferAsync(sourceData, destinationTable, options);
```

## Notes

- **Thread safety:** `BulkTransferOptions` is not thread‑safe for concurrent writes. All property assignments should be completed before the object is passed to a transfer method. After that, the object should not be modified while a transfer is in progress.
- **Edge cases:**  
  - Setting `BatchSize`, `MaxConcurrency`, `ProgressReportingInterval`, or `BufferSize` to zero or a negative value throws `ArgumentOutOfRangeException`.  
  - Setting `MaxErrorThreshold` to zero means no errors are tolerated; any row error aborts the transfer.  
  - Setting `BatchTimeout` to `TimeSpan.Zero` or a negative duration throws `ArgumentOutOfRangeException`.  
  - If `EnableCheckpointing` is `true` and `CheckpointFilePath` is `null`, the library may use a default path (e.g., a temporary file); this behavior is implementation‑specific.  
  - `ValidationMode` must be a defined value of the `ValidationMode` enum; passing an undefined integer cast throws `ArgumentException`.  
- **Performance considerations:** Large `BatchSize` values reduce round trips but increase memory usage per batch. `MaxConcurrency` should be tuned to the target database’s connection pool and I/O capacity. `BufferSize` affects streaming efficiency; values that are multiples of the disk sector size (e.g., 4096, 65536) are recommended.
