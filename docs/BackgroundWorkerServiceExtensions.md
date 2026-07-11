# BackgroundWorkerServiceExtensions

The `BackgroundWorkerServiceExtensions` class provides a set of static utility methods designed to streamline the management, orchestration, and monitoring of background tasks within the application. These extensions facilitate the lifecycle management of scheduled task runners, provide direct access to the underlying task queuing infrastructure, and offer robust observability through real-time processing statistics. By abstracting complex threading and coordination logic, this class ensures consistent and reliable execution of asynchronous background operations across the system.

## API

### StartScheduledTaskAsync
Initiates a new scheduled task runner that executes the specified action at a defined interval.
*   **Parameters:**
    *   `string taskName`: A unique identifier for the scheduled task.
    *   `Func<Task> action`: The asynchronous task to be executed.
    *   `TimeSpan interval`: The frequency at which the task should run.
*   **Returns:** `Task<ScheduledTaskRunner>`: A handle to the started scheduled task runner.
*   **Throws:** `ArgumentNullException` if `action` is null; `InvalidOperationException` if a task with the same `taskName` is already running.

### StopScheduledTasksAsync
Signals all currently running scheduled task runners to cease execution and initiates a graceful shutdown.
*   **Parameters:** None.
*   **Returns:** `Task`: A task representing the completion of the shutdown process.

### GetTaskQueue
Retrieves the singleton instance of the `BackgroundTaskQueue` responsible for managing enqueued tasks.
*   **Parameters:** None.
*   **Returns:** `BackgroundTaskQueue`: The current task queue instance.

### GetWorkerCount
Returns the number of active worker threads currently configured to process tasks from the queue.
*   **Parameters:** None.
*   **Returns:** `int`: The active worker count.

### EnqueueTaskAsync
Adds a new asynchronous task to the background queue for execution.
*   **Parameters:**
    *   `Func<Task> task`: The asynchronous task to perform.
    *   `string description`: A descriptive string for tracking and logging purposes.
*   **Returns:** `Task`: A task representing the successful addition of the work item to the queue.
*   **Throws:** `ArgumentNullException` if `task` is null.

### GetQueueLength
Returns the current number of tasks residing in the queue awaiting execution.
*   **Parameters:** None.
*   **Returns:** `int`: The count of pending tasks.

### GetTaskStatistics
Retrieves summary metrics regarding background task performance, including completed, failed, and in-progress tasks.
*   **Parameters:** None.
*   **Returns:** `TaskStatistics`: An object containing the current performance metrics.

## Usage

### Example 1: Enqueuing a One-Off Task
```csharp
// Enqueue a task to process a database cleanup
await BackgroundWorkerServiceExtensions.EnqueueTaskAsync(async () =>
{
    await _dbService.CleanupOldRecordsAsync();
}, "Database Cleanup Task");
```

### Example 2: Managing a Recurring Scheduled Task
```csharp
// Start a recurring synchronization task
var runner = await BackgroundWorkerServiceExtensions.StartScheduledTaskAsync(
    "DataSyncTask", 
    async () => await _syncService.PerformSyncAsync(), 
    TimeSpan.FromHours(1)
);

// Stop all scheduled tasks during application shutdown
await BackgroundWorkerServiceExtensions.StopScheduledTasksAsync();
```

## Notes

*   **Thread Safety:** The underlying queue and management structures are thread-safe, allowing safe calls to `EnqueueTaskAsync` and status retrieval methods from multiple concurrent threads.
*   **Lifecycle Management:** It is critical to call `StopScheduledTasksAsync` during application shutdown to ensure all scheduled runners are terminated gracefully and to prevent orphaned background processes.
*   **Exception Handling:** Exceptions occurring within enqueued tasks will be captured by the background worker service and reflected in the `TaskStatistics` output; therefore, tasks should internally handle expected exceptions to avoid premature task failure reporting.
*   **Performance:** `GetQueueLength` and `GetWorkerCount` are performant, thread-safe snapshot operations and can be polled for monitoring purposes without significantly impacting system performance.
