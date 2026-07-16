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

// ... rest of README content ...
