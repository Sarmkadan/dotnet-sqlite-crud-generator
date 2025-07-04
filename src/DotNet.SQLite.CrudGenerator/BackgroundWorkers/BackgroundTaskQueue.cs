// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System.Collections.Concurrent;

namespace DotNet.SQLite.CrudGenerator.BackgroundWorkers;

/// <summary>
/// Thread-safe queue for background tasks.
/// Supports async task execution with priority and scheduling.
/// Tracks task execution history and provides statistics.
/// </summary>
public sealed class BackgroundTaskQueue
{
    private readonly PriorityQueue<BackgroundTask, int> _taskQueue = new();
    private readonly ConcurrentDictionary<string, TaskExecutionRecord> _executionHistory = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _maxHistorySize = 500;

    public async Task EnqueueAsync(BackgroundTask task, TaskPriority priority = TaskPriority.Normal)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        task.Id = Guid.NewGuid();
        task.EnqueuedAt = DateTime.UtcNow;

        await _semaphore.WaitAsync();
        try
        {
            _taskQueue.Enqueue(task, (int)priority);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<BackgroundTask?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_taskQueue.TryDequeue(out var task, out _))
                {
                    task.StartedAt = DateTime.UtcNow;
                    return task;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            // Wait briefly before checking again
            await Task.Delay(100, cancellationToken);
        }
    }

    public async Task RecordExecutionAsync(string taskId, bool success, string? error = null)
    {
        var record = new TaskExecutionRecord
        {
            TaskId = taskId,
            ExecutedAt = DateTime.UtcNow,
            Success = success,
            Error = error
        };

        _executionHistory[taskId] = record;

        // Cleanup old records
        if (_executionHistory.Count > _maxHistorySize)
        {
            var oldestKey = _executionHistory
                .OrderBy(kvp => kvp.Value.ExecutedAt)
                .First()
                .Key;

            _executionHistory.TryRemove(oldestKey, out _);
        }

        await Task.CompletedTask;
    }

    public int GetQueueLength()
    {
        return _taskQueue.Count;
    }

    public IEnumerable<TaskExecutionRecord> GetExecutionHistory()
    {
        return _executionHistory.Values.OrderByDescending(r => r.ExecutedAt);
    }

    public TaskStatistics GetStatistics()
    {
        var records = _executionHistory.Values.ToList();
        var successCount = records.Count(r => r.Success);
        var failureCount = records.Count(r => !r.Success);

        return new TaskStatistics
        {
            PendingTasks = _taskQueue.Count,
            TotalExecuted = records.Count,
            SuccessfulTasks = successCount,
            FailedTasks = failureCount,
            AverageExecutionTime = records.Any()
                ? records.Select(r => (r.ExecutedAt - DateTime.UtcNow).TotalMilliseconds).Average()
                : 0
        };
    }
}

public sealed class BackgroundTask
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Func<CancellationToken, Task> Action { get; set; } = null!;
    public DateTime EnqueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
}

public enum TaskPriority
{
    Low = 10,
    Normal = 5,
    High = 1,
    Critical = 0
}

public sealed class TaskExecutionRecord
{
    public string TaskId { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public sealed class TaskStatistics
{
    public int PendingTasks { get; set; }
    public int TotalExecuted { get; set; }
    public int SuccessfulTasks { get; set; }
    public int FailedTasks { get; set; }
    public double AverageExecutionTime { get; set; }
}
