#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for <see cref="PerformanceMonitor"/> providing additional functionality
/// for performance monitoring and analysis.
/// </summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>
    /// Gets the average execution time across all operations in milliseconds.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>The average execution time in milliseconds, or 0 if no operations recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static double GetAverageExecutionTime(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        return monitor.GetAllMetrics().AverageOrDefault(m => m.AverageTime);
    }

    /// <summary>
    /// Gets the total execution time across all operations in milliseconds.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>The total execution time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static long GetTotalExecutionTime(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        return monitor.GetAllMetrics().Sum(m => m.TotalTime);
    }

    /// <summary>
    /// Gets the success rate percentage across all operations.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>The success rate as a percentage (0-100), or 0 if no operations recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static double GetSuccessRate(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var allMetrics = monitor.GetAllMetrics().ToList();
        var totalOperations = allMetrics.Sum(m => m.ExecutionCount);
        var totalSuccess = allMetrics.Sum(m => m.SuccessCount);

        return totalOperations > 0
            ? (totalSuccess / (double)totalOperations) * 100
            : 0;
    }

    /// <summary>
    /// Gets the most recently executed operation metrics.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>The metrics for the most recently executed operation, or null if no operations recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static OperationMetrics? GetMostRecentOperation(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        return monitor.GetAllMetrics()
            .MaxBy(m => m.LastExecutedAt);
    }

    /// <summary>
    /// Gets operations that exceed a specified average execution time threshold.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="thresholdMilliseconds">The threshold in milliseconds.</param>
    /// <returns>Collection of operations exceeding the threshold, ordered by execution count descending.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="thresholdMilliseconds"/> is negative.</exception>
    public static IEnumerable<OperationMetrics> GetSlowOperations(this PerformanceMonitor monitor, long thresholdMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentOutOfRangeException.ThrowIfNegative(thresholdMilliseconds);

        return monitor.GetAllMetrics()
            .Where(m => m.AverageTime > thresholdMilliseconds)
            .OrderByDescending(m => m.ExecutionCount);
    }

    /// <summary>
    /// Gets operations with failure rate above a specified threshold percentage.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="failureRateThreshold">The failure rate threshold as a percentage (0-100).</param>
    /// <returns>Collection of operations with high failure rates, ordered by failure count descending.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="failureRateThreshold"/> is not in range [0, 100].</exception>
    public static IEnumerable<OperationMetrics> GetProblematicOperations(this PerformanceMonitor monitor, double failureRateThreshold = 10.0)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentOutOfRangeException.ThrowIfLessThan(failureRateThreshold, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(failureRateThreshold, 100);

        return monitor.GetAllMetrics()
            .Where(m => m.SuccessRate < 100 - failureRateThreshold)
            .OrderByDescending(m => m.FailureCount);
    }

    /// <summary>
    /// Creates a formatted performance summary string for display or logging.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="includeDetailedMetrics">Whether to include detailed operation metrics.</param>
    /// <returns>A formatted performance summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static string GetPerformanceSummary(this PerformanceMonitor monitor, bool includeDetailedMetrics = true)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var report = monitor.GetPerformanceReport();
        var summary = new System.Text.StringBuilder();
        
        summary.AppendLine("=== Performance Summary ===");
        summary.AppendLine($"Uptime: {TimeSpan.FromSeconds(report.UptimeSeconds):hh\\:mm\\:ss}");
        summary.AppendLine($"Total Operations: {report.TotalOperations} ({report.TotalSuccessful} success, {report.TotalFailed} failed)");
        summary.AppendLine($"Success Rate: {report.TotalOperations:F2}%");
        summary.AppendLine($"Average Response Time: {report.AverageResponseTime:F2}ms");
        
        if (report.SlowestOperation is not null)
        {
            summary.AppendLine($"Slowest Operation: {report.SlowestOperation.OperationName} ({report.SlowestOperation.AverageTime:F2}ms avg)");
        }
        
        if (report.FastestOperation is not null)
        {
            summary.AppendLine($"Fastest Operation: {report.FastestOperation.OperationName} ({report.FastestOperation.AverageTime:F2}ms avg)");
        }
        
        if (includeDetailedMetrics && report.OperationMetrics.Any())
        {
            summary.AppendLine("\n=== Detailed Metrics ===");
            foreach (var metric in report.OperationMetrics.Take(10))
            {
                summary.AppendLine($"{metric.OperationName}:");
                summary.AppendLine($"  Executions: {metric.ExecutionCount}, Success: {metric.SuccessCount}, Failures: {metric.FailureCount}");
                summary.AppendLine($"  Avg: {metric.AverageTime:F2}ms, Min: {metric.MinTime}ms, Max: {metric.MaxTime}ms");
                summary.AppendLine($"  Success Rate: {metric.SuccessRate:F2}%, Last: {metric.LastExecutedAt:yyyy-MM-dd HH:mm:ss}");
            }
            
            if (report.OperationMetrics.Count > 10)
            {
                summary.AppendLine($"\n... and {report.OperationMetrics.Count - 10} more operations");
            }
        }
        
        return summary.ToString();
    }

    /// <summary>
    /// Gets the operation with the highest execution count.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>The most frequently executed operation metrics, or null if no operations recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static OperationMetrics? GetMostFrequentOperation(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        return monitor.GetAllMetrics()
            .MaxBy(m => m.ExecutionCount);
    }

    /// <summary>
    /// Computes the average of a sequence of nullable <see cref="double"/> values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">A sequence of values to calculate the average of.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The average of the sequence, or 0 if the sequence is empty or contains only null values.</returns>
    private static double AverageOrDefault<T>(this IEnumerable<T> source, Func<T, double> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        var values = source.Select(selector).Where(v => v >= 0).ToList();
        return values.Any() ? values.Average() : 0;
    }
}
