# BulkOperationsBenchmarks

A benchmarking utility for measuring the performance of bulk database operations (import, export, and transfer) against SQLite using the `dotnet-sqlite-crud-generator` library.

## API

### `public async Task Setup()`

Initializes the benchmark environment, including creating the test database schema, seeding data if required, and preparing any necessary resources. This method must be called before any other operations.

- **Parameters**: None
- **Return value**: `Task` (completes when setup is finished)
- **Exceptions**: Throws if database connection cannot be established, schema creation fails, or required resources are unavailable.

---

### `public async Task<BulkImportResult> ImportBatchAsync()`

Performs a bulk import operation using batch processing (non-streaming). Measures the time and resource usage for inserting a predefined dataset into the target table.

- **Parameters**: None
- **Return value**: `BulkImportResult` containing metrics such as elapsed time, row count, and success status.
- **Exceptions**: Throws if the import operation fails or if the dataset is invalid.

---

### `public async Task<BulkImportResult> ImportStreamingAsync()`

Performs a bulk import operation using streaming (chunked) processing. Measures performance when data is read in chunks and inserted incrementally.

- **Parameters**: None
- **Return value**: `BulkImportResult` with performance metrics and success status.
- **Exceptions**: Throws if streaming fails, data corruption is detected, or chunk processing errors occur.

---
### `public async Task<BulkExportResult> ExportToStreamAsync()`

Exports data from the database to an output stream (e.g., `MemoryStream`) in bulk. Measures the time and resources required to serialize a large dataset.

- **Parameters**: None
- **Return value**: `BulkExportResult` containing export duration, row count, and stream size.
- **Exceptions**: Throws if the query fails, stream cannot be written to, or data serialization errors occur.

---
### `public async Task<BulkTransferResult> TransferAsync()`

Transfers data between two SQLite databases (or tables) in bulk. Measures the time and resources used for cross-database data migration.

- **Parameters**: None
- **Return value**: `BulkTransferResult` with transfer duration, row count, and success status.
- **Exceptions**: Throws if connection to either database fails, transfer logic fails, or data integrity is compromised.

---
### `public async Task<BulkImportResult> ImportFromStreamAsync()`

Imports data from an input stream (e.g., `MemoryStream`) into the database in bulk. Measures performance when data is read from a stream and inserted.

- **Parameters**: None
- **Return value**: `BulkImportResult` with import metrics and success status.
- **Exceptions**: Throws if the stream is invalid, data parsing fails, or insertion errors occur.

---
### `public async Task Cleanup()`

Cleans up the benchmark environment by dropping test tables, closing connections, and releasing resources. Should be called after benchmarking to restore system state.

- **Parameters**: None
- **Return value**: `Task` (completes when cleanup is finished)
- **Exceptions**: Throws if cleanup operations fail (e.g., connection already closed).

---
### `public void Dispose()`

Releases all managed and unmanaged resources used by the benchmark instance. Calls `Cleanup()` if not already invoked.

- **Parameters**: None
- **Return value**: None
- **Exceptions**: None

## Usage

### Example 1: Benchmarking Bulk Import (Streaming vs Batch)
