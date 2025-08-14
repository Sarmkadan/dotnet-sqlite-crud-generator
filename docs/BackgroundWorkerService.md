# BackgroundWorkerService
The `BackgroundWorkerService` class is designed to manage and execute tasks in the background, providing a simple and efficient way to run scheduled tasks. It allows for the creation and management of tasks that can be executed at specific times or intervals, making it a useful tool for a variety of applications, including data processing, maintenance tasks, and more.

## API
* `public BackgroundWorkerService`: The constructor for the `BackgroundWorkerService` class, used to create a new instance of the service.
* `public async Task StartAsync`: Starts the background worker service, allowing it to begin executing scheduled tasks. This method is asynchronous and returns a `Task` that represents the operation.
* `public async Task StopAsync`: Stops the background worker service, preventing it from executing any further tasks. This method is asynchronous and returns a `Task` that represents the operation.
* `public ScheduledTaskRunner ScheduledTaskRunner`: A property that provides access to the scheduled task runner, which is used to manage and execute scheduled tasks.
* `public async Task ScheduleAsync`: Schedules a task to be executed at a specific time or interval. This method is asynchronous and returns a `Task` that represents the operation.
* `public void Stop`: Stops the background worker service immediately, preventing it from executing any further tasks.

## Usage
The following examples demonstrate how to use the `BackgroundWorkerService` class:
```csharp
// Example 1: Starting and stopping the service
var service = new BackgroundWorkerService();
await service.StartAsync();
// Perform some work...
await service.StopAsync();
```

```csharp
// Example 2: Scheduling a task
var service = new BackgroundWorkerService();
await service.StartAsync();
var taskRunner = service.ScheduledTaskRunner;
await taskRunner.ScheduleAsync(() => {
    // Perform some work...
}, TimeSpan.FromHours(1)); // Execute the task every hour
```

## Notes
The `BackgroundWorkerService` class is designed to be used in a multithreaded environment, and its methods are thread-safe. However, it is important to note that the `Stop` method will immediately stop the service, potentially interrupting any currently executing tasks. Additionally, the `ScheduleAsync` method will throw an exception if the service is not started before attempting to schedule a task. It is also worth noting that the `ScheduledTaskRunner` property provides access to the underlying task runner, which can be used to manage and execute tasks directly.
