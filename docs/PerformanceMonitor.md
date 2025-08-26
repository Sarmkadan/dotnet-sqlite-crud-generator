# PerformanceMonitor

`PerformanceMonitor` provides a lightweight, in-process instrumentation layer for tracking operation latency, success/failure rates, and memory usage across named code paths. It is designed for use in the `dotnet-sqlite-crud-generator` toolchain to measure CRUD generation steps, but it is general-purpose enough to wrap any short-lived or long-running operation. The type aggregates per-operation metrics and can produce summary reports suitable for diagnostics and tuning.

## API

### StartOperation

```csharp
public PerformanceScope StartOperation(string operationName)
```

Begins timing a named operation and returns a `PerformanceScope` that must be disposed to stop the clock. The scope records elapsed wall-clock time and updates success/failure counts based on whether `MarkFailure` is called before disposal.

- **Parameters**: `operationName` — a non-null, non-empty string identifying the operation.
- **Returns**: A `PerformanceScope` instance linked to this monitor.
- **Throws**: `ArgumentNullException` when `operationName` is null; `ArgumentException` when it is empty.

### GetMetrics

```csharp
public OperationMetrics? GetMetrics(string operationName)
```

Retrieves the accumulated metrics for a single operation, or `null` if no data has been recorded for that name.

- **Parameters**: `operationName` — the operation to look up.
- **Returns**: An `OperationMetrics` object containing execution counts, timing statistics, and last-executed timestamp, or `null`.

### GetAllMetrics

```csharp
public IEnumerable<OperationMetrics> GetAllMetrics()
```

Returns a snapshot of metrics for every operation that has been recorded at least once. The collection is a copy; modifying it does not affect the monitor.

- **Returns**: A lazy enumeration of `OperationMetrics` instances. Order is unspecified.

### GetPerformanceReport

```csharp
public PerformanceReport GetPerformanceReport()
```

Produces a structured report that includes all per-operation metrics, total uptime, aggregate operation count, and current memory information. This is the primary method for exporting a complete diagnostic view.

- **Returns**: A `PerformanceReport` object.

### Reset

```csharp
public void Reset()
```

Clears all accumulated metrics for every operation and resets the internal uptime baseline. After this call the monitor behaves as if newly constructed.

### ResetOperation

```csharp
public void ResetOperation(string operationName)
```

Clears metrics for a single named operation without affecting others.

- **Parameters**: `operationName` — the operation to reset. If the operation has never been recorded, the call is a no-op.

### GetMemoryInfo

```csharp
public MemoryInfo GetMemoryInfo()
```

Captures current process memory statistics (working set, private bytes, managed heap) at the moment of invocation.

- **Returns**: A `MemoryInfo` value type with memory figures in bytes.

### PerformanceScope (nested type)

```csharp
public sealed class PerformanceScope : IDisposable
```

A disposable struct or class returned by `StartOperation`. Disposing it stops the timer and commits the elapsed ticks to the parent `PerformanceMonitor`. Calling `MarkFailure` on the scope before disposal causes the operation to be counted as a failure rather than a success. Each scope must be disposed exactly once; double-disposal is safe and ignored.

### MarkFailure

```csharp
public void MarkFailure()
```

Can be called on an active `PerformanceScope` (before disposal) to flag the current operation as failed. Has no effect if called after disposal or on a scope not associated with this monitor.

### Dispose

```csharp
public void Dispose()
```

Releases any internal timers or resources held by the monitor. After disposal, calling other members may produce `ObjectDisposedException`.

### OperationName

```csharp
public string OperationName { get; }
```

The name of the operation this metrics instance represents. Read-only.

### ExecutionCount

```csharp
public long ExecutionCount { get; }
```

Total number of times the operation has been executed (sum of successes and failures).

### SuccessCount

```csharp
public long SuccessCount { get; }
```

Number of executions that completed without `MarkFailure` being called.

### FailureCount

```csharp
public long FailureCount { get; }
```

Number of executions where `MarkFailure` was invoked before scope disposal.

### TotalTime

```csharp
public long TotalTime { get; }
```

Cumulative elapsed time in ticks (or a platform-defined high-resolution unit) across all executions.

### MinTime

```csharp
public long MinTime { get; }
```

Minimum single-execution time recorded. Zero if no executions have occurred.

### MaxTime

```csharp
public long MaxTime { get; }
```

Maximum single-execution time recorded. Zero if no executions have occurred.

### LastExecutedAt

```csharp
public DateTime LastExecutedAt { get; }
```

UTC timestamp of the most recent execution. `DateTime.MinValue` if the operation has never run.

### UptimeSeconds

```csharp
public double UptimeSeconds { get; }
```

Elapsed wall-clock seconds since the monitor was created or last reset.

### TotalOperations

```csharp
public long TotalOperations { get; }
```

Sum of `ExecutionCount` across all tracked operations.

## Usage

### Example 1: Timing a single CRUD generation step

```csharp
using var monitor = new PerformanceMonitor();

for (int i = 0; i < 100; i++)
{
    using var scope = monitor.StartOperation("GenerateInsert");
    try
    {
        GenerateInsertStatement(table);
    }
    catch
    {
        scope.MarkFailure();
        throw;
    }
}

OperationMetrics? metrics = monitor.GetMetrics("GenerateInsert");
Console.WriteLine($"Insert generation: {metrics?.SuccessCount} successes, " +
                  $"avg {(metrics?.TotalTime / Math.Max(1, metrics?.ExecutionCount)) ?? 0} ticks");
```

### Example 2: Generating a full performance report

```csharp
var monitor = new PerformanceMonitor();

// Simulate multiple operations
using (var scope = monitor.StartOperation("BuildModel"))
{
    BuildEntityModel();
}

using (var scope = monitor.StartOperation("EmitSQL"))
{
    EmitSqlFiles();
    scope.MarkFailure(); // simulate a recoverable error
}

PerformanceReport report = monitor.GetPerformanceReport();
Console.WriteLine($"Uptime: {report.UptimeSeconds:F2}s, Total ops: {report.TotalOperations}");
foreach (var op in report.Metrics)
{
    Console.WriteLine($"{op.OperationName}: {op.ExecutionCount} runs, " +
                      $"{op.FailureCount} failures, min={op.MinTime}, max={op.MaxTime}");
}

MemoryInfo mem = monitor.GetMemoryInfo();
Console.WriteLine($"Working set: {mem.WorkingSetBytes / 1024} KB");
```

## Notes

- **Thread safety**: `StartOperation`, `MarkFailure`, `Reset`, and `ResetOperation` are safe to call concurrently. Metrics aggregation uses lock-free or fine-grained locking internally. `GetMetrics`, `GetAllMetrics`, and `GetPerformanceReport` return point-in-time snapshots that may not reflect in-flight operations.
- **Disposal**: Always dispose `PerformanceScope` instances in the reverse order of creation. Failing to dispose a scope leaks the timing entry and may permanently skew `ExecutionCount` for that operation.
- **Time units**: `TotalTime`, `MinTime`, and `MaxTime` are expressed in `Stopwatch` ticks. Convert to milliseconds or seconds using `TimeSpan.FromTicks(...)` as needed.
- **Memory information**: `GetMemoryInfo` queries the current process and may force a garbage collection depending on implementation; avoid calling it inside hot paths.
- **Reset semantics**: `Reset` also resets `UptimeSeconds` to zero. `ResetOperation` does not affect the uptime baseline.
- **Empty metrics**: `GetMetrics` returns `null` for an operation name that has never been passed to `StartOperation`. `GetAllMetrics` excludes such names entirely.
