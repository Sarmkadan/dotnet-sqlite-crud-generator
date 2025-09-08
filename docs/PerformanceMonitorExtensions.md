# PerformanceMonitorExtensions

The `PerformanceMonitorExtensions` class provides a suite of diagnostic utility methods designed to analyze and report on database operation metrics within the `dotnet-sqlite-crud-generator` framework. These extensions facilitate the retrieval of performance statistics—such as execution averages, success rates, and operation frequency—and enable the identification of slow or problematic operations, allowing developers to effectively monitor system health and optimize database interaction efficiency.

## API

### GetAverageExecutionTime
Calculates the arithmetic mean duration of all tracked database operations.
*   **Return:** A `double` representing the average execution time in milliseconds.

### GetTotalExecutionTime
Calculates the cumulative time spent across all tracked database operations.
*   **Return:** A `long` representing the total execution time in milliseconds.

### GetSuccessRate
Computes the ratio of successful database operations to the total number of operations performed.
*   **Return:** A `double` representing the success rate (0.0 to 1.0).

### GetMostRecentOperation
Retrieves the most recently recorded operation metrics.
*   **Return:** An `OperationMetrics?` instance containing details of the latest operation, or `null` if no operations have been recorded.

### GetSlowOperations
Retrieves a collection of operations whose execution duration exceeded predefined performance thresholds.
*   **Return:** An `IEnumerable<OperationMetrics>` containing the identified slow operations.

### GetProblematicOperations
Retrieves a collection of operations that have been flagged as problematic, such as those that resulted in failures or resource exhaustion.
*   **Return:** An `IEnumerable<OperationMetrics>` containing the identified problematic operations.

### GetPerformanceSummary
Generates a human-readable string summarizing the current performance metrics, including total execution time, success rate, and operation counts.
*   **Return:** A `string` containing the formatted summary report.

### GetMostFrequentOperation
Identifies the operation that has been executed the highest number of times.
*   **Return:** An `OperationMetrics?` instance containing the metrics for the most frequent operation, or `null` if no operations have been recorded.

## Usage

**Example 1: Displaying a performance summary to the console**

```csharp
var monitor = performanceService.GetMonitor();
string summary = PerformanceMonitorExtensions.GetPerformanceSummary(monitor);

Console.WriteLine("Performance Report:");
Console.WriteLine(summary);
```

**Example 2: Logging slow operations for investigation**

```csharp
var monitor = performanceService.GetMonitor();
var slowOps = PerformanceMonitorExtensions.GetSlowOperations(monitor);

foreach (var op in slowOps)
{
    logger.LogWarning("Slow operation detected: {OperationName} took {Duration}ms", 
        op.Name, op.ExecutionTimeMs);
}
```

## Notes

*   **Edge Cases:** Methods that return `OperationMetrics?` (`GetMostRecentOperation`, `GetMostFrequentOperation`) will return `null` if the underlying monitor has not yet recorded any operations. Methods returning `IEnumerable<OperationMetrics>` will return an empty enumerable rather than `null` when no matching operations are found.
*   **Thread Safety:** While these extension methods are static and stateless themselves, they operate on the instance provided as the first parameter. It is assumed that the underlying `PerformanceMonitor` instance is thread-safe and allows concurrent reads during the collection of metrics. In high-concurrency environments, callers should ensure that the state of the monitor is not being mutated during the execution of these reporting methods to guarantee accuracy.
