#nullable enable

namespace DotNet.SQLite.CrudGenerator.BackgroundWorkers;

/// <summary>
/// Extension methods for <see cref="BackgroundWorkerService"/> providing additional functionality
/// for managing background workers and scheduled tasks.
/// </summary>
public static class BackgroundWorkerServiceExtensions
{
    /// <summary>
    /// Creates and starts a scheduled task runner that executes the specified action periodically.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <param name="taskName">The name of the scheduled task.</param>
    /// <param name="action">The action to execute periodically.</param>
    /// <param name="interval">The time interval between executions.</param>
    /// <param name="initialDelay">Optional initial delay before the first execution. Defaults to the interval.</param>
    /// <returns>A <see cref="ScheduledTaskRunner"/> instance that can be used to manage the scheduled task.</returns>
    public static async Task<ScheduledTaskRunner> StartScheduledTaskAsync(
        this BackgroundWorkerService service,
        string taskName,
        Func<CancellationToken, Task> action,
        TimeSpan interval,
        TimeSpan? initialDelay = null)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
        }

        var runner = new ScheduledTaskRunner(service.GetTaskQueue());
        await runner.ScheduleAsync(taskName, action, interval, initialDelay);

        return runner;
    }

    /// <summary>
    /// Stops all scheduled tasks managed by the service.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <param name="timeout">Optional timeout for graceful shutdown. Defaults to 5 seconds.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task StopScheduledTasksAsync(
        this BackgroundWorkerService service,
        TimeSpan? timeout = null)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // Note: ScheduledTaskRunner instances are managed by the caller
        // This method provides a convenient way to stop all scheduled tasks
        // when stopping the background worker service
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
        await service.StopAsync(timeoutValue);
    }

    /// <summary>
    /// Gets the current task queue being used by the background worker service.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <returns>The <see cref="BackgroundTaskQueue"/> instance.</returns>
    public static BackgroundTaskQueue GetTaskQueue(this BackgroundWorkerService service)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // Use reflection to access the private field
        var field = typeof(BackgroundWorkerService).GetField("_taskQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (BackgroundTaskQueue)field?.GetValue(service)!;
    }

    /// <summary>
    /// Gets the current worker count configured for this service.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <returns>The number of worker threads.</returns>
    public static int GetWorkerCount(this BackgroundWorkerService service)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // Use reflection to access the private field
        var field = typeof(BackgroundWorkerService).GetField("_workerCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)field?.GetValue(service)!;
    }

    /// <summary>
    /// Enqueues a task to be processed by the background worker service.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <param name="task">The task to enqueue.</param>
    /// <param name="priority">Optional priority for the task. Defaults to Normal.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task EnqueueTaskAsync(
        this BackgroundWorkerService service,
        BackgroundTask task,
        TaskPriority priority = TaskPriority.Normal)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        var queue = service.GetTaskQueue();
        await queue.EnqueueAsync(task, priority);
    }

    /// <summary>
    /// Gets the current queue length (number of pending tasks).
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <returns>The number of pending tasks in the queue.</returns>
    public static int GetQueueLength(this BackgroundWorkerService service)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var queue = service.GetTaskQueue();
        return queue.GetQueueLength();
    }

    /// <summary>
    /// Gets the task execution statistics.
    /// </summary>
    /// <param name="service">The background worker service instance.</param>
    /// <returns>A <see cref="TaskStatistics"/> object containing execution statistics.</returns>
    public static TaskStatistics GetTaskStatistics(this BackgroundWorkerService service)
    {
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var queue = service.GetTaskQueue();
        return queue.GetStatistics();
    }
}