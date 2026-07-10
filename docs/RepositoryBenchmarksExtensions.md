# RepositoryBenchmarksExtensions

`RepositoryBenchmarksExtensions` provides utility methods for benchmarking repository operations in SQLite-based CRUD applications. It simplifies measuring execution time and batch operations for testing or performance analysis scenarios.

## API

### `CreateBatchAsync`

Creates a batch of entities in the repository and returns the IDs of the inserted items.

**Parameters:**
- `repository`: The repository instance implementing batch insertion.
- `items`: The collection of entities to insert.

**Return value:**
- `Task<IReadOnlyList<int>>`: A list of IDs for the inserted entities, in the same order as the input items.

**Exceptions:**
- Throws `ArgumentNullException` if `repository` or `items` is `null`.
- Throws `InvalidOperationException` if the batch insertion fails.

---

### `MeasureOperationAsync`

Measures the duration of an asynchronous repository operation.

**Parameters:**
- `operation`: The asynchronous operation to measure.

**Return value:**
- `Task<TimeSpan>`: The elapsed time of the operation.

**Exceptions:**
- Throws `ArgumentNullException` if `operation` is `null`.

---

### `MeasureOperationAsync<T>`

Measures the duration of an asynchronous repository operation and returns the result along with the duration.

**Parameters:**
- `operation`: The asynchronous operation to measure.

**Return value:**
- `Task<(T Result, TimeSpan Duration)>`: A tuple containing the operation result and its elapsed time.

**Exceptions:**
- Throws `ArgumentNullException` if `operation` is `null`.

---
### `ResetDatabaseAsync`

Resets the SQLite database to a clean state by dropping and recreating all tables.

**Parameters:**
- `connectionString`: The connection string for the SQLite database.

**Return value:**
- `Task`: A task representing the asynchronous operation.

**Exceptions:**
- Throws `ArgumentNullException` if `connectionString` is `null`.
- Throws `InvalidOperationException` if the database reset fails.

## Usage

### Example 1: Benchmarking Insertion
