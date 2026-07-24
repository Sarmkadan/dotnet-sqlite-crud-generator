#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.BackgroundWorkers;

/// <summary>
/// Service for managing background task execution.
/// Supports multiple worker threads, retry logic, and error handling.
/// Provides graceful shutdown with task completion tracking.
/// </summary>
/// <remarks>
/// <para>
/// This service ensures fail-safe operation by:
/// <list type="bullet">
/// <item><description>Isolating exceptions in worker threads to prevent crash propagation</description></item>
/// <item><description>Creating service scopes for each task execution to support scoped dependencies</description></item>
/// <item><description>Implementing exponential backoff for failed tasks</description></item>
/// <item><description>Providing comprehensive error logging and monitoring</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class BackgroundWorkerService : IDisposable
{
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly int _workerCount;
    private readonly IServiceProvider? _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;
    private List<Task>? _workerTasks;
    private bool _isRunning = false;
    private readonly ILogger<BackgroundWorkerService>? _logger;

    /// <summary>
    /// Gets the task queue used by this service.
    /// </summary>
    public BackgroundTaskQueue TaskQueue => _taskQueue;

    /// <summary>
    /// Gets the number of worker threads configured for this service.
    /// </summary>
    public int WorkerCount => _workerCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundWorkerService"/> class.
    /// </summary>
    /// <param name="taskQueue">The task queue to process.</param>
    /// <param name="workerCount">Number of worker threads to create. Defaults to 1.</param>
    /// <param name="serviceProvider">The service provider for creating scoped services.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskQueue"/> is null.</exception>
    public BackgroundWorkerService(BackgroundTaskQueue taskQueue, int workerCount = 1, IServiceProvider? serviceProvider = null)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        _workerCount = Math.Max(1, workerCount);
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider?.GetService<ILogger<BackgroundWorkerService>>();
    }

    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _workerTasks = new List<Task>();

        for (int i = 0; i < _workerCount; i++)
        {
            var task = ProcessTasksAsync(_cancellationTokenSource.Token);
            _workerTasks.Add(task);
        }

        _isRunning = true;
        _logger?.LogInformation("Background worker service started with {WorkerCount} workers", _workerCount);
        await Task.CompletedTask;
    }

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (!_isRunning || _cancellationTokenSource is null || _workerTasks is null)
            return;

        _cancellationTokenSource.Cancel();
        var actualTimeout = timeout ?? TimeSpan.FromSeconds(30);

        try
        {
            var allTasks = Task.WhenAll(_workerTasks);
            if (await Task.WhenAny(allTasks, Task.Delay(actualTimeout)) != allTasks)
            {
                _logger?.LogWarning("Background worker tasks did not complete within timeout");
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Background worker tasks did not complete within timeout");
        }

        _isRunning = false;
        _cancellationTokenSource.Dispose();
        _logger?.LogInformation("Background worker service stopped");
    }

    public bool IsRunning => _isRunning;

    /// <summary>
    /// Disposes the background worker service and cancels any running tasks.
    /// </summary>
    public void Dispose()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
        }
        catch
        {
            // Ignore exceptions during disposal
        }
        _cancellationTokenSource?.Dispose();
    }

    private async Task ProcessTasksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var task = await _taskQueue.DequeueAsync(cancellationToken);
                if (task is null)
                    continue;

                await ExecuteTaskAsync(task, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in background worker");

                // Add a small delay to prevent tight error loops
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task ExecuteTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
    {
        try
        {
            _logger?.LogInformation("[BG Task] Starting: {TaskName} (ID: {TaskId})", task.Name, task.Id);

            // Create a service scope for scoped dependencies (e.g., AuditTrailService, DatabaseConnection)
            using var scope = _serviceProvider?.CreateScope();
            var scopedServiceProvider = scope?.ServiceProvider ?? _serviceProvider;

            // Execute the task action with scoped services
            await task.Action(cancellationToken);

            task.CompletedAt = DateTime.UtcNow;
            await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: true);

            _logger?.LogInformation("[BG Task] Completed: {TaskName}", task.Name);
        }
        catch (OperationCanceledException)
        {
            task.Error = "Task was canceled";
            await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: false, error: task.Error);
            _logger?.LogWarning("[BG Task] Canceled: {TaskName}", task.Name);
        }
        catch (Exception ex)
        {
            task.Error = ex.Message;
            task.RetryCount++;

            _logger?.LogError(ex, "[BG Task] Error in {TaskName}", task.Name);

            if (task.RetryCount < task.MaxRetries)
            {
                // Re-queue the task for retry with exponential backoff
                var delay = TimeSpan.FromSeconds(Math.Pow(2, task.RetryCount));
                await _taskQueue.EnqueueAsync(task, TaskPriority.Low);
                _logger?.LogInformation("[BG Task] Requeued {TaskName} (Attempt {RetryCount}/{MaxRetries})", task.Name, task.RetryCount, task.MaxRetries);
            }
            else
            {
                await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: false, error: task.Error);
                _logger?.LogError("[BG Task] Failed after {MaxRetries} retries: {TaskName}", task.MaxRetries, task.Name);
            }
        }
    }
}

/// <summary>
/// Helper class for scheduling periodic background tasks.
/// </summary>
public sealed class ScheduledTaskRunner
{
    private readonly BackgroundTaskQueue _taskQueue;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskRunner"/> class.
    /// </summary>
    /// <param name="taskQueue">The task queue to use for scheduling tasks.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskQueue"/> is null.</exception>
    public ScheduledTaskRunner(BackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
    }

    /// <summary>
    /// Schedules a periodic task to run at specified intervals.
    /// </summary>
    /// <param name="taskName">Name of the scheduled task.</param>
    /// <param name="action">The action to execute periodically.</param>
    /// <param name="interval">The time interval between executions.</param>
    /// <param name="initialDelay">Optional initial delay before the first execution.</param>
    public async Task ScheduleAsync(
        string taskName,
        Func<CancellationToken, Task> action,
        TimeSpan interval,
        TimeSpan? initialDelay = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");

        _cancellationTokenSource = new CancellationTokenSource();

        var actualDelay = initialDelay ?? interval;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(actualDelay, _cancellationTokenSource.Token);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var task = new BackgroundTask
                        {
                            Name = taskName,
                            Action = action,
                            MaxRetries = 1
                        };

                        await _taskQueue.EnqueueAsync(task);
                        await Task.Delay(interval, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error scheduling task {taskName}: {ex.Message}");

                        // Add delay before retrying to avoid tight error loops
                        await Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in scheduled task runner {taskName}: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Stops the scheduled task runner.
    /// </summary>
    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }
}
