# QueryBuilderBenchmarks

The `QueryBuilderBenchmarks` class provides a structured suite of performance benchmarking methods for evaluating the query generation efficiency within the `dotnet-sqlite-crud-generator` project. This class enables accurate measurement of execution time and resource allocation when generating database query builders for different entity types, assisting in performance regression testing and optimization efforts.

## API

### Setup
`public async Task Setup()`
Prepares the environment required for benchmarking, including initializing necessary database connections or mock configurations.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous initialization operation.
*   **Throws:** Throws an exception if the environment initialization fails.

### GenerateQueryBuilderAsync
`public async Task GenerateQueryBuilderAsync()`
Executes a benchmark for the general query builder generation logic.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous benchmark operation.
*   **Throws:** Throws an exception if the generation process fails during the benchmark.

### BuildQueryBuilderSource
`public void BuildQueryBuilderSource()`
Synchronously triggers the generation of source code for the query builder component, used as a baseline for cold-start performance measurement.
*   **Parameters:** None.
*   **Return Value:** `void`.
*   **Throws:** Throws an exception if source generation fails.

### GenerateQueryBuilderForUserAsync
`public async Task GenerateQueryBuilderForUserAsync()`
Executes a benchmark for query builder generation specific to the `User` entity.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous benchmark operation.
*   **Throws:** Throws an exception if the generation process fails.

### GenerateQueryBuilderForOrderAsync
`public async Task GenerateQueryBuilderForOrderAsync()`
Executes a benchmark for query builder generation specific to the `Order` entity.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous benchmark operation.
*   **Throws:** Throws an exception if the generation process fails.

### GenerateQueryBuilderForCategoryAsync
`public async Task GenerateQueryBuilderForCategoryAsync()`
Executes a benchmark for query builder generation specific to the `Category` entity.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous benchmark operation.
*   **Throws:** Throws an exception if the generation process fails.

### Cleanup
`public async Task Cleanup()`
Performs post-benchmark environment teardown, removing temporary files, data, or resetting database states.
*   **Parameters:** None.
*   **Return Value:** A `Task` representing the asynchronous cleanup operation.
*   **Throws:** Throws an exception if cleanup fails.

### Dispose
`public void Dispose()`
Releases all unmanaged and managed resources held by the `QueryBuilderBenchmarks` instance.
*   **Parameters:** None.
*   **Return Value:** `void`.

## Usage

### Example 1: Basic Benchmark Execution
This example demonstrates a standard manual execution flow of a specific benchmark method.

```csharp
using var benchmarks = new QueryBuilderBenchmarks();

await benchmarks.Setup();
try
{
    await benchmarks.GenerateQueryBuilderForUserAsync();
}
finally
{
    await benchmarks.Cleanup();
}
```

### Example 2: Synchronous Source Build
This example demonstrates how to trigger the synchronous source code generation process.

```csharp
using var benchmarks = new QueryBuilderBenchmarks();

// Prepare environment before synchronous operation
await benchmarks.Setup();
try
{
    benchmarks.BuildQueryBuilderSource();
}
finally
{
    await benchmarks.Cleanup();
}
```

## Notes

*   **Thread Safety:** The `QueryBuilderBenchmarks` class is not thread-safe. Concurrent execution of its methods may result in race conditions, corrupted test data, or inaccurate benchmark measurements.
*   **Resource Management:** This class implements `IDisposable`. Users must ensure `Dispose()` is called—typically via a `using` statement or `finally` block—to prevent resource leaks.
*   **Error Handling:** Failures during setup or execution may leave temporary artifacts or database states that require manual intervention if `Cleanup()` fails to execute correctly.
*   **Performance Stability:** For accurate results, benchmarks should be run in a clean environment, as external factors such as I/O load or background processes can significantly impact measurements.
