# BulkTransferProgress

`BulkTransferProgress` is a record type used to track and report the progress of bulk data transfer operations, such as exporting data from a SQLite database to an external file. It provides detailed metrics, status messages, and error information to monitor the progress and outcome of the transfer process.

## API

### `public sealed record BulkTransferProgress`

The root type representing the progress of a bulk transfer operation.

### `public override string ToString()`

Overrides the default `ToString()` method to provide a human-readable summary of the transfer progress, including key metrics such as succeeded, failed, and duration.

**Returns**
A string containing a formatted summary of the transfer progress.

---

### `public required long RowNumber`

Gets the current row number being processed during the transfer operation.

**Remarks**
This value is incremented sequentially as rows are read from the source.

---

### `public required string Message`

Gets a human-readable status message describing the current state of the transfer operation.

**Remarks**
This message may change throughout the operation (e.g., "Reading batch", "Committing batch", "Export completed").

---

### `public string? RawRecord`

Gets the raw record data associated with the current progress update, if applicable.

**Remarks**
This field is optional and may be `null` for non-record-specific updates.

---

### `public Exception? InnerException`

Gets the inner exception that caused the transfer operation to fail, if applicable.

**Remarks**
This field is populated only when the transfer operation encounters an unrecoverable error.

---

### `public long TotalRead`

Gets the total number of rows read from the source during the transfer operation.

**Remarks**
This value is cumulative and increases as rows are processed.

---

### `public long Succeeded`

Gets the number of rows successfully processed and written to the destination.

**Remarks**
This value is cumulative and increases as rows are successfully exported.

---

### `public long Failed`

Gets the number of rows that failed to be processed or written.

**Remarks**
This value is cumulative and increases when errors occur during processing.

---

### `public int BatchesCommitted`

Gets the number of batches committed to the destination during the transfer operation.

**Remarks**
This value indicates how many discrete write operations were completed.

---

### `public TimeSpan Duration`

Gets the total time elapsed since the transfer operation started.

**Remarks**
This value is calculated from `StartedAt` to the current time or completion time.

---

### `public DateTime StartedAt`

Gets the timestamp when the transfer operation began.

**Remarks**
This value is set once at the start of the operation.

---

### `public List<BulkTransferError> Errors`

Gets the collection of errors encountered during the transfer operation.

**Remarks**
This list is populated as errors occur and remains empty if no errors are encountered.

---

### `public long TotalExported`

Gets the total number of records exported to the destination.

**Remarks**
This value is cumulative and increases as records are successfully written.

---

### `public long BytesWritten`

Gets the total number of bytes written to the destination file.

**Remarks**
This value is cumulative and increases as data is written.

---

### `public ExportFormat Format`

Gets the format used for exporting the data (e.g., CSV, JSON).

**Remarks**
This value is set at the start of the operation and does not change.

---

### `public string? DestinationPath`

Gets the file path where the exported data was written, if applicable.

**Remarks**
This value may be `null` if the destination is not a file (e.g., stream).

---

## Usage

### Example 1: Monitoring a bulk export operation
