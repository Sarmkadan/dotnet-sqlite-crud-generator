#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Monitors and tracks application performance metrics.
/// Records operation execution times, memory usage, and throughput.
/// Provides real-time and historical statistics.
/// </summary>
public sealed class PerformanceMonitor
{
    private readonly Dictionary<string, OperationMetrics> _metrics = new();
    private readonly object _lockObject = new();
    private readonly Stopwatch _uptime = Stopwatch.StartNew();

    public PerformanceScope StartOperation(string operationName)
    {
        return new PerformanceScope(operationName, this);
    }

    internal void RecordOperation(string operationName, long elapsedMilliseconds, bool success)
    {
        lock (_lockObject)
        {
            if (!_metrics.TryGetValue(operationName, out var metrics))
            {
                metrics = new OperationMetrics { OperationName = operationName };
                _metrics[operationName] = metrics;
            }

            metrics.ExecutionCount++;
            metrics.TotalTime += elapsedMilliseconds;
            metrics.LastExecutedAt = DateTime.UtcNow;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            if (elapsedMilliseconds < metrics.MinTime || metrics.MinTime == 0)
                metrics.MinTime = elapsedMilliseconds;

            if (elapsedMilliseconds > metrics.MaxTime)
                metrics.MaxTime = elapsedMilliseconds;
        }
    }

    public OperationMetrics? GetMetrics(string operationName)
    {
        lock (_lockObject)
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
    }

    public IEnumerable<OperationMetrics> GetAllMetrics()
    {
        lock (_lockObject)
        {
            return _metrics.Values.OrderByDescending(m => m.ExecutionCount).ToList();
        }
    }

    public PerformanceReport GetPerformanceReport()
    {
        lock (_lockObject)
        {
            var allMetrics = _metrics.Values.ToList();

            return new PerformanceReport
            {
                UptimeSeconds = _uptime.Elapsed.TotalSeconds,
                TotalOperations = allMetrics.Sum(m => m.ExecutionCount),
                TotalSuccessful = allMetrics.Sum(m => m.SuccessCount),
                TotalFailed = allMetrics.Sum(m => m.FailureCount),
                AverageResponseTime = allMetrics.Any()
                    ? allMetrics.Average(m => m.AverageTime)
                    : 0,
                SlowestOperation = allMetrics.OrderByDescending(m => m.AverageTime).FirstOrDefault(),
                FastestOperation = allMetrics.OrderBy(m => m.AverageTime).FirstOrDefault(),
                OperationMetrics = allMetrics.OrderByDescending(m => m.ExecutionCount).ToList()
            };
        }
    }

    public void Reset()
    {
        lock (_lockObject)
        {
            _metrics.Clear();
        }
    }

    public void ResetOperation(string operationName)
    {
        lock (_lockObject)
        {
            _metrics.Remove(operationName);
        }
    }

    public MemoryInfo GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();

        return new MemoryInfo
        {
            WorkingSetMB = process.WorkingSet64 / (1024 * 1024),
            PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
            ThreadCount = process.Threads.Count,
            GCTotalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024)
        };
    }
}

public sealed class PerformanceScope : IDisposable
{
    private readonly PerformanceMonitor _monitor;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    private bool _success = true;

    public PerformanceScope(string operationName, PerformanceMonitor monitor)
    {
        _operationName = operationName;
        _monitor = monitor;
        _stopwatch = Stopwatch.StartNew();
    }

    public void MarkFailure()
    {
        _success = false;
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _monitor.RecordOperation(_operationName, _stopwatch.ElapsedMilliseconds, _success);
    }
}

public sealed class OperationMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public long ExecutionCount { get; set; }
    public long SuccessCount { get; set; }
    public long FailureCount { get; set; }
    public long TotalTime { get; set; }
    public long MinTime { get; set; }
    public long MaxTime { get; set; }
    public DateTime LastExecutedAt { get; set; }

    public double AverageTime => ExecutionCount > 0 ? TotalTime / (double)ExecutionCount : 0;
    public double SuccessRate => ExecutionCount > 0 ? (SuccessCount / (double)ExecutionCount) * 100 : 0;
}

public sealed class PerformanceReport
{
    public double UptimeSeconds { get; set; }
    public long TotalOperations { get; set; }
    public long TotalSuccessful { get; set; }
    public long TotalFailed { get; set; }
    public double AverageResponseTime { get; set; }
    public OperationMetrics? SlowestOperation { get; set; }
    public OperationMetrics? FastestOperation { get; set; }
    public List<OperationMetrics> OperationMetrics { get; set; } = new();

    public override string ToString()
    {
        var report = "=== Performance Report ===\n";
        report += $"Uptime: {TimeSpan.FromSeconds(UptimeSeconds):hh\\:mm\\:ss}\n";
        report += $"Total Operations: {TotalOperations} ({TotalSuccessful} success, {TotalFailed} failed)\n";
        report += $"Average Response Time: {AverageResponseTime:F2}ms\n";

        if (SlowestOperation is not null)
            report += $"Slowest: {SlowestOperation.OperationName} ({SlowestOperation.AverageTime:F2}ms avg)\n";

        if (FastestOperation is not null)
            report += $"Fastest: {FastestOperation.OperationName} ({FastestOperation.AverageTime:F2}ms avg)\n";

        return report;
    }
}

public sealed class MemoryInfo
{
    public long WorkingSetMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public int ThreadCount { get; set; }
    public long GCTotalMemoryMB { get; set; }
}
