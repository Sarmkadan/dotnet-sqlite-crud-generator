// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.BackgroundWorkers;

/// <summary>
/// Service for managing background task execution.
/// Supports multiple worker threads, retry logic, and error handling.
/// Provides graceful shutdown with task completion tracking.
/// </summary>
public class BackgroundWorkerService
{
    private readonly BackgroundTaskQueue _taskQueue;
    private readonly int _workerCount;
    private CancellationTokenSource? _cancellationTokenSource;
    private List<Task>? _workerTasks;
    private bool _isRunning = false;

    public BackgroundWorkerService(BackgroundTaskQueue taskQueue, int workerCount = 1)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        _workerCount = Math.Max(1, workerCount);
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
        Console.WriteLine($"Background worker service started with {_workerCount} workers");
        await Task.CompletedTask;
    }

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (!_isRunning || _cancellationTokenSource == null || _workerTasks == null)
            return;

        _cancellationTokenSource.Cancel();
        var actualTimeout = timeout ?? TimeSpan.FromSeconds(30);

        try
        {
            var allTasks = Task.WhenAll(_workerTasks);
            if (await Task.WhenAny(allTasks, Task.Delay(actualTimeout)) != allTasks)
            {
                Console.WriteLine("Background worker tasks did not complete within timeout");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Background worker tasks did not complete within timeout");
        }

        _isRunning = false;
        _cancellationTokenSource.Dispose();
        Console.WriteLine("Background worker service stopped");
    }

    public bool IsRunning => _isRunning;

    private async Task ProcessTasksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var task = await _taskQueue.DequeueAsync(cancellationToken);
                if (task == null)
                    continue;

                await ExecuteTaskAsync(task, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in background worker: {ex.Message}");
            }
        }
    }

    private async Task ExecuteTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[BG Task] Starting: {task.Name} (ID: {task.Id})");

            await task.Action(cancellationToken);

            task.CompletedAt = DateTime.UtcNow;
            await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: true);

            Console.WriteLine($"[BG Task] Completed: {task.Name}");
        }
        catch (OperationCanceledException)
        {
            task.Error = "Task was canceled";
            await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: false, error: task.Error);
        }
        catch (Exception ex)
        {
            task.Error = ex.Message;
            task.RetryCount++;

            Console.Error.WriteLine($"[BG Task] Error in {task.Name}: {ex.Message}");

            if (task.RetryCount < task.MaxRetries)
            {
                // Re-queue the task for retry
                await _taskQueue.EnqueueAsync(task, TaskPriority.Low);
                Console.WriteLine($"[BG Task] Requeued {task.Name} (Attempt {task.RetryCount}/{task.MaxRetries})");
            }
            else
            {
                await _taskQueue.RecordExecutionAsync(task.Id.ToString(), success: false, error: task.Error);
                Console.Error.WriteLine($"[BG Task] Failed after {task.MaxRetries} retries: {task.Name}");
            }
        }
    }
}

/// <summary>
/// Helper class for scheduling periodic background tasks.
/// </summary>
public class ScheduledTaskRunner
{
    private readonly BackgroundTaskQueue _taskQueue;
    private CancellationTokenSource? _cancellationTokenSource;

    public ScheduledTaskRunner(BackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
    }

    public async Task ScheduleAsync(
        string taskName,
        Func<CancellationToken, Task> action,
        TimeSpan interval,
        TimeSpan? initialDelay = null)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        var actualDelay = initialDelay ?? interval;

        _ = Task.Run(async () =>
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
                }
            }
        });
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }
}
