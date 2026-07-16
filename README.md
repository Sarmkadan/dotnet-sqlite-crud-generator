// existing content ...

## AuditLog

`AuditLog` represents an audit log entry for tracking entity changes. It captures details such as the entity type, ID, operation type, changed-by user ID, old and new values, change reason, IP address, and timestamp. The class includes validation methods and factory methods for creating audit log entries for create, update, and delete operations.

Below is a realistic example of using `AuditLog`:

```csharp
// Create an audit log entry for a new entity
var auditLog = AuditLog.CreateForCreate(
    entityType: "Product",
    entityId: 1,
    userId: 42,
    newValues: "{\"Name\": \"Premium Wireless Headphones\", \"Description\": \"Noise-cancelling wireless headphones with 30-hour battery life\"}",
    reason: "Initial product creation",
    ipAddress: "192.168.1.100"
);

// Validate the audit log entry
bool isValid = auditLog.Validate();
Console.WriteLine($"Audit log entry is valid: {isValid}");

// Print the audit log entry details
Console.WriteLine($"Entity type: {auditLog.EntityType}");
Console.WriteLine($"Entity ID: {auditLog.EntityId}");
Console.WriteLine($"Operation type: {auditLog.OperationType}");
Console.WriteLine($"Changed-by user ID: {auditLog.ChangedByUserId}");
Console.WriteLine($"Old values: {auditLog.OldValues}");
Console.WriteLine($"New values: {auditLog.NewValues}");
Console.WriteLine($"Change reason: {auditLog.ChangeReason}");
Console.WriteLine($"IP address: {auditLog.IpAddress}");
Console.WriteLine($"Timestamp: {auditLog.Timestamp}");
```

## BackgroundWorkerService

`BackgroundWorkerService` is a service for managing background task execution in .NET applications. It provides a robust infrastructure for running long-running tasks asynchronously with support for multiple worker threads, automatic retry logic, error handling, and graceful shutdown. The service coordinates task execution through a `BackgroundTaskQueue` and tracks task completion status.

Below is a realistic example of using `BackgroundWorkerService`:

```csharp
// Create a task queue
var taskQueue = new BackgroundTaskQueue();

// Create and configure the background worker service with 3 worker threads
var workerService = new BackgroundWorkerService(taskQueue, workerCount: 3);

// Start the background worker service
await workerService.StartAsync();

// Schedule a background task
var task = new BackgroundTask
{
    Name = "Database Cleanup",
    Action = async (cancellationToken) =>
    {
        // Perform database cleanup operations
        await Task.Delay(1000, cancellationToken);
        Console.WriteLine("Database cleanup completed");
    },
    MaxRetries = 3
};

await taskQueue.EnqueueAsync(task);

// Schedule a recurring task using ScheduledTaskRunner
var scheduledRunner = new ScheduledTaskRunner(taskQueue);
await scheduledRunner.ScheduleAsync(
    taskName: "Cache Refresh",
    action: async (cancellationToken) =>
    {
        Console.WriteLine("Refreshing cache...");
        await Task.Delay(5000, cancellationToken);
    },
    interval: TimeSpan.FromMinutes(5),
    initialDelay: TimeSpan.FromSeconds(10)
);

// Run for some time...
await Task.Delay(TimeSpan.FromSeconds(30));

// Gracefully stop the worker service
await workerService.StopAsync(TimeSpan.FromSeconds(10));

// Stop the scheduled runner
scheduledRunner.Stop();
```

## BackgroundTaskQueue

`BackgroundTaskQueue` is a thread-safe queue implementation for managing and executing background tasks asynchronously. It provides priority-based task scheduling, execution tracking, and comprehensive statistics about task performance. The queue supports multiple worker threads and maintains a history of task executions with automatic cleanup of old records.

Below is a realistic example of using `BackgroundTaskQueue` directly:

```csharp
// Create a task queue
var taskQueue = new BackgroundTaskQueue();

// Enqueue a high-priority task
var highPriorityTask = new BackgroundTask
{
    Name = "Generate Report",
    Action = async (cancellationToken) =>
    {
        Console.WriteLine("Generating report...");
        await Task.Delay(2000, cancellationToken);
        Console.WriteLine("Report generation completed");
    },
    MaxRetries = 2
};

await taskQueue.EnqueueAsync(highPriorityTask, TaskPriority.High);

// Enqueue a normal-priority task
var normalTask = new BackgroundTask
{
    Name = "Send Email Notifications",
    Action = async (cancellationToken) =>
    {
        Console.WriteLine("Sending notifications...");
        await Task.Delay(1000, cancellationToken);
        Console.WriteLine("Notifications sent");
    }
};

await taskQueue.EnqueueAsync(normalTask);

// Process tasks from the queue
var cancellationTokenSource = new CancellationTokenSource();
var processingTask = Task.Run(async () =>
{
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
        var task = await taskQueue.DequeueAsync(cancellationTokenSource.Token);
        if (task != null)
        {
            try
            {
                await task.Action(cancellationTokenSource.Token);
                await taskQueue.RecordExecutionAsync(task.Id.ToString(), success: true);
                task.CompletedAt = DateTime.UtcNow;
                Console.WriteLine($"Task '{task.Name}' completed successfully");
            }
            catch (Exception ex)
            {
                await taskQueue.RecordExecutionAsync(task.Id.ToString(), success: false, error: ex.Message);
                task.Error = ex.Message;
                Console.WriteLine($"Task '{task.Name}' failed: {ex.Message}");
            }
        }
    }
});

// Wait for some tasks to complete
await Task.Delay(TimeSpan.FromSeconds(5));

// Get queue statistics
var stats = taskQueue.GetStatistics();
Console.WriteLine($"Queue length: {taskQueue.GetQueueLength()}");
Console.WriteLine($"Pending tasks: {stats.PendingTasks}");
Console.WriteLine($"Total executed: {stats.TotalExecuted}");
Console.WriteLine($"Successful tasks: {stats.SuccessfulTasks}");
Console.WriteLine($"Failed tasks: {stats.FailedTasks}");

// Get execution history
var history = taskQueue.GetExecutionHistory();
foreach (var record in history)
{
    Console.WriteLine($"Task {record.TaskId} executed at {record.ExecutedAt}: {(record.Success ? "Success" : "Failed")}");
}

// Cleanup
cancellationTokenSource.Cancel();
await processingTask;
```

// ... rest of README content ...
