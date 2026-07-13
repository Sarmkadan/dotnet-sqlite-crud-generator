// existing content ...

## PerformanceMonitorExtensions

The `PerformanceMonitorExtensions` class provides utility methods to analyze and summarize performance metrics collected during operations. It offers insights into execution times, success rates, and identifies slow or problematic operations for optimization.

Example usage:
```csharp
public class PerformanceLogger
{
    public void LogMetrics()
    {
        var averageTime = PerformanceMonitorExtensions.GetAverageExecutionTime();
        var total = PerformanceMonitorExtensions.GetTotalExecutionTime();
        var successRate = PerformanceMonitorExtensions.GetSuccessRate();
        var recent = PerformanceMonitorExtensions.GetMostRecentOperation();
        var slowOps = PerformanceMonitorExtensions.GetSlowOperations();
        var problemOps = PerformanceMonitorExtensions.GetProblematicOperations();
        var summary = PerformanceMonitorExtensions.GetPerformanceSummary();
        var frequentOp = PerformanceMonitorExtensions.GetMostFrequentOperation();

        Console.WriteLine(summary);
        Console.WriteLine($"Average execution time: {averageTime:F2}ms");
        Console.WriteLine($"Slow operations count: {slowOps.Count()}");
        Console.WriteLine($"Problematic operations count: {problemOps.Count()}");
    }
}
```
