# BackgroundTaskQueue

`BackgroundTaskQueue` provides a persistent, retry-aware queue for background operations backed by SQLite storage. It allows enqueuing work items represented as asynchronous delegates, dequeuing them for execution, recording execution outcomes, and querying queue depth, execution history, and aggregate statistics. Each queue instance is identified by a unique `Id` and a human-readable `Name`, and every enqueued task carries its own retry policy and lifecycle timestamps.

## API

### EnqueueAsync

```csharp
public async Task EnqueueAsync(Func<CancellationToken, Task> action, int maxRetries = 0)
```

Enqueues a new background task for later execution. The `action` parameter is the asynchronous work to perform and receives a `CancellationToken` that signals graceful shutdown. `maxRetries` sets the maximum number of automatic retry attempts after a failure (defaults to zero). The method persists the task immediately to the backing store. Throws `ArgumentNullException` when `action` is `null`.

### DequeueAsync

```csharp
public async Task<BackgroundTask?> DequeueAsync()
```

Atomically retrieves and locks the next pending task from the queue, returning a `BackgroundTask` instance or `null` if no tasks are available. The returned object exposes the original `Action` delegate, `MaxRetries`, `RetryCount`, and lifecycle timestamps. The caller is responsible for executing the task and subsequently calling `RecordExecutionAsync`.

### RecordExecutionAsync

```csharp
public async Task RecordExecutionAsync(string taskId, bool success, string? error = null)
```

Records the outcome of a dequeued task’s execution. `taskId` identifies the task (obtained from the `TaskId` property of the dequeued `BackgroundTask`). `success` indicates whether execution completed without unhandled exceptions. `error` optionally captures an error message when `success` is `false`. Internally updates the task’s `CompletedAt` timestamp, increments `RetryCount` on failure, and either marks the task complete or re-queues it if retries remain. Throws `ArgumentException` when `taskId` is null or empty.

### GetQueueLength

```csharp
public int GetQueueLength()
```

Returns the current number of tasks in a pending state (not yet dequeued or awaiting retry). The count is read directly from the SQLite store and reflects the instantaneous queue depth.

### GetExecutionHistory

```csharp
public IEnumerable<TaskExecutionRecord> GetExecutionHistory()
```

Returns all execution records for this queue, ordered by execution time descending. Each `TaskExecutionRecord` includes `TaskId`, `ExecutedAt`, `Success`, and `Error` fields. The enumeration is materialized from the database at call time.

### GetStatistics

```csharp
public TaskStatistics GetStatistics()
```

Returns aggregate statistics for the queue, including `PendingTasks` (equivalent to `GetQueueLength`), total completed tasks, total failures, and average execution duration. The `TaskStatistics` object is computed from the current database state.

### BackgroundTask Properties

| Member | Type | Description |
|---|---|---|
| `Id` | `Guid` | Unique identifier of the queue that owns this task. |
| `Name` | `string` | Human-readable name of the owning queue. |
| `Action` | `Func<CancellationToken, Task>` | The asynchronous delegate to execute. |
| `EnqueuedAt` | `DateTime` | Timestamp when the task was originally enqueued. |
| `StartedAt` | `DateTime?` | Timestamp when the task was dequeued and execution began. |
| `CompletedAt` | `DateTime?` | Timestamp when execution finished (success or final failure). |
| `Error` | `string?` | Error message from the most recent failed attempt, if any. |
| `RetryCount` | `int` | Number of execution attempts already made. |
| `MaxRetries` | `int` | Maximum number of retries allowed before the task is considered permanently failed. |
| `TaskId` | `string` | Unique string identifier for this specific task instance. |

### TaskExecutionRecord Properties

| Member | Type | Description |
|---|---|---|
| `TaskId` | `string` | Identifier of the task this record belongs to. |
| `ExecutedAt` | `DateTime` | Timestamp when this execution attempt completed. |
| `Success` | `bool` | Whether this attempt succeeded. |
| `Error` | `string?` | Error details if the attempt failed. |

### TaskStatistics Properties

| Member | Type | Description |
|---|---|---|
| `PendingTasks` | `int` | Number of tasks currently awaiting execution. |

## Usage

### Example 1: Fire-and-forget with retries

```csharp
var queue = new BackgroundTaskQueue("email-queue");

await queue.EnqueueAsync(async (ct) =>
{
    await SendWelcomeEmailAsync(ct);
}, maxRetries: 3);

// Later, in a background processor loop:
var task = await queue.DequeueAsync();
if (task is not null)
{
    try
    {
        await task.Action(CancellationToken.None);
        await queue.RecordExecutionAsync(task.TaskId, success: true);
    }
    catch (Exception ex)
    {
        await queue.RecordExecutionAsync(task.TaskId, success: false, ex.Message);
    }
}
```

### Example 2: Monitoring queue health

```csharp
var queue = new BackgroundTaskQueue("report-generation");

// Enqueue several reports
for (int i = 0; i < 10; i++)
{
    await queue.EnqueueAsync(async (ct) =>
    {
        await GenerateMonthlyReportAsync(i, ct);
    }, maxRetries: 1);
}

// Check backlog before scaling workers
int pending = queue.GetQueueLength();
Console.WriteLine($"Pending tasks: {pending}");

// After processing, inspect outcomes
var stats = queue.GetStatistics();
Console.WriteLine($"Still pending: {stats.PendingTasks}");

foreach (var record in queue.GetExecutionHistory().Take(5))
{
    Console.WriteLine($"{record.TaskId}: {(record.Success ? "OK" : "FAIL")} at {record.ExecutedAt}");
}
```

## Notes

- **Thread safety**: `DequeueAsync` uses atomic row-level locking in SQLite to ensure that a task is claimed by exactly one consumer, even when multiple workers operate against the same queue concurrently. `EnqueueAsync` and `RecordExecutionAsync` are safe for concurrent calls from multiple threads or processes sharing the same database file.
- **Retry semantics**: When `RecordExecutionAsync` is called with `success: false` and the task’s `RetryCount` is less than `MaxRetries`, the task is automatically reset to pending state and its `RetryCount` is incremented. When `RetryCount` reaches `MaxRetries`, the task is marked permanently failed and will not be returned by future `DequeueAsync` calls.
- **Null dequeues**: `DequeueAsync` returns `null` when the queue is empty. Consumers must handle this case to avoid null-reference exceptions when accessing `BackgroundTask` members.
- **Cancellation token**: The `CancellationToken` passed to the action delegate is controlled by the caller at execution time, not by the queue itself. The queue does not impose its own timeout or cancellation policy.
- **Error recording**: The `error` parameter on `RecordExecutionAsync` is optional and stored as-is. It is surfaced in `TaskExecutionRecord.Error` and `BackgroundTask.Error` for the most recent failed attempt.
- **Statistics freshness**: `GetStatistics` and `GetQueueLength` query the database directly and reflect the state at the moment of the call. They do not cache results.
- **Lifecycle timestamps**: `StartedAt` is set internally by `DequeueAsync`. `CompletedAt` is set by `RecordExecutionAsync`. `EnqueuedAt` is set by `EnqueueAsync`. These timestamps use the system clock at the time the database write occurs.
