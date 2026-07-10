# AuditTrailBenchmarks

`AuditTrailBenchmarks` is a benchmarking harness for measuring the performance characteristics of audit trail operations in the `dotnet-sqlite-crud-generator` framework. It provides standardized, isolated test scenarios for create, update, delete, query, bulk, and summary operations against an audit trail store backed by SQLite. Each benchmark method is self-contained, relying on a common setup phase to prepare the database state, and a corresponding cleanup phase to reset it. The class implements `IDisposable` to ensure deterministic release of underlying resources.

## API

### Setup
```csharp
public async Task Setup()
```
Prepares the benchmarking environment. Initializes the SQLite database connection, creates required schema objects, and seeds any prerequisite data so that subsequent benchmark methods operate against a known, consistent state. Must be invoked once before any individual benchmark run.

- **Parameters**: None.
- **Returns**: A `Task` representing the asynchronous initialization.
- **Throws**: May throw if the database file cannot be created or accessed, or if schema migration fails.

### RecordCreateOperationAsync
```csharp
public async Task RecordCreateOperationAsync()
```
Benchmarks the latency and throughput of recording a single create operation in the audit trail. Inserts a new audit record representing an entity creation event.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the benchmark iteration finishes.
- **Throws**: May throw on database insertion failures or constraint violations.

### RecordUpdateOperationAsync
```csharp
public async Task RecordUpdateOperationAsync()
```
Benchmarks recording an update operation. Writes an audit entry that captures changes to an existing entity, including before/after state if applicable.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the benchmark iteration finishes.
- **Throws**: May throw if the target entity reference is invalid or the database write fails.

### RecordDeleteOperationAsync
```csharp
public async Task RecordDeleteOperationAsync()
```
Benchmarks recording a delete operation. Inserts an audit record marking entity removal.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the benchmark iteration finishes.
- **Throws**: May throw on database insertion failure or missing referenced entity.

### GetEntityTrailAsync
```csharp
public async Task GetEntityTrailAsync()
```
Benchmarks retrieval of the full audit trail for a specific entity. Executes a query that fetches all historical audit records associated with a given entity identifier.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the query and materialization finish.
- **Throws**: May throw if the entity identifier is not found or the query execution fails.

### GetUserTrailAsync
```csharp
public async Task GetUserTrailAsync()
```
Benchmarks retrieval of all audit records attributed to a specific user. Measures query performance when filtering the trail by user identity.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the query and materialization finish.
- **Throws**: May throw on query execution failure.

### GetRecentAsync
```csharp
public async Task GetRecentAsync()
```
Benchmarks fetching the most recent audit entries across all entities. Typically involves a time-ordered, limited selection query.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the query and materialization finish.
- **Throws**: May throw on query execution failure.

### QueryAsync
```csharp
public async Task QueryAsync()
```
Benchmarks a general-purpose audit trail query with filtering, ordering, and projection. Exercises the query pipeline under a representative multi-criteria scenario.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the query and materialization finish.
- **Throws**: May throw on query parsing or execution failure.

### GetSummaryAsync
```csharp
public async Task GetSummaryAsync()
```
Benchmarks generation of an audit summary, such as counts grouped by operation type or time window. Measures aggregation query performance.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the aggregation finishes.
- **Throws**: May throw on query execution failure.

### BulkRecordAsync
```csharp
public async Task BulkRecordAsync()
```
Benchmarks bulk insertion of multiple audit records in a single operation. Measures throughput when writing large batches of trail entries.

- **Parameters**: None.
- **Returns**: A `Task` that completes when the bulk insert finishes.
- **Throws**: May throw on batch insertion failure, transaction rollback, or constraint violations.

### Cleanup
```csharp
public async Task Cleanup()
```
Resets the benchmarking environment to a clean state. Deletes data generated during benchmark runs, closes open connections, and removes temporary artifacts. Should be invoked after a benchmark session completes.

- **Parameters**: None.
- **Returns**: A `Task` representing the asynchronous teardown.
- **Throws**: May throw if resource release fails or file deletion is blocked.

### Dispose
```csharp
public void Dispose()
```
Synchronously releases all managed and unmanaged resources held by the instance. Ensures database connections and file handles are properly closed. Safe to call multiple times.

- **Parameters**: None.
- **Returns**: Void.
- **Throws**: Does not throw under normal conditions; implementations should suppress exceptions in disposal paths.

## Usage

### Example 1: Single-method benchmark session
```csharp
var benchmarks = new AuditTrailBenchmarks();
try
{
    await benchmarks.Setup();

    // Run a specific benchmark
    await benchmarks.RecordCreateOperationAsync();
}
finally
{
    await benchmarks.Cleanup();
    benchmarks.Dispose();
}
```

### Example 2: Running multiple benchmarks in sequence
```csharp
var benchmarks = new AuditTrailBenchmarks();
await benchmarks.Setup();

try
{
    await benchmarks.RecordCreateOperationAsync();
    await benchmarks.RecordUpdateOperationAsync();
    await benchmarks.BulkRecordAsync();
    await benchmarks.GetEntityTrailAsync();
    await benchmarks.GetSummaryAsync();
}
finally
{
    await benchmarks.Cleanup();
    benchmarks.Dispose();
}
```

## Notes

- **Setup and Cleanup pairing**: `Setup` must be called before any benchmark method. `Cleanup` should be called exactly once after all desired benchmarks complete. Skipping `Cleanup` may leave residual data in the database file, affecting subsequent runs.
- **Dispose safety**: `Dispose` can be invoked even if `Setup` was never called or if `Cleanup` threw an exception. Implementations are expected to guard against double disposal and null references internally.
- **Thread safety**: The public members are designed for sequential execution within a single benchmarking context. They are not thread-safe; concurrent invocation of benchmark methods from multiple threads without external synchronization will lead to race conditions on the shared database state.
- **Exception propagation**: Benchmark methods surface underlying database exceptions directly. Callers should wrap individual benchmark invocations in try-catch blocks if partial failure tolerance is required during a multi-benchmark session.
- **Resource lifetime**: The underlying SQLite connection and any file streams remain open until `Dispose` is called. Deferring disposal can lock the database file and prevent concurrent access by other processes.
