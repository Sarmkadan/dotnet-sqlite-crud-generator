# MigrationDiffBenchmarks
The `MigrationDiffBenchmarks` type is designed to facilitate the comparison and analysis of database schema migrations. It provides a set of methods to compute differences between expected and actual database schemas, allowing for the identification of discrepancies and potential issues. This type is particularly useful in scenarios where database schema changes need to be thoroughly tested and validated.

## API
* `public async Task Setup`: Initializes the benchmarking process. This method should be called before any other methods to ensure proper setup.
* `public async Task ComputeDiffAsync_UpToDate`: Computes the difference between the expected and actual database schemas, assuming the expected schema is up-to-date. This method returns a task that represents the asynchronous operation.
* `public async Task GetActualSchemaAsync`: Retrieves the actual schema of the database. This method returns a task that represents the asynchronous operation.
* `public void GetExpectedSchema`: Retrieves the expected schema of the database. This method does not return a value.
* `public async Task GetTableInfoAsync`: Retrieves information about a specific table in the database. This method returns a task that represents the asynchronous operation.
* `public async Task Cleanup`: Cleans up resources used during the benchmarking process. This method returns a task that represents the asynchronous operation.
* `public void Dispose`: Releases all resources used by the `MigrationDiffBenchmarks` instance. This method does not return a value and should be called when the instance is no longer needed.

## Usage
The following examples demonstrate how to use the `MigrationDiffBenchmarks` type:
```csharp
// Example 1: Basic usage
var benchmarks = new MigrationDiffBenchmarks();
await benchmarks.Setup();
await benchmarks.ComputeDiffAsync_UpToDate();
benchmarks.GetExpectedSchema();
await benchmarks.Cleanup();
benchmarks.Dispose();

// Example 2: Using GetTableInfoAsync
var benchmarks = new MigrationDiffBenchmarks();
await benchmarks.Setup();
var tableInfo = await benchmarks.GetTableInfoAsync("MyTable");
Console.WriteLine($"Table name: {tableInfo.Name}, Column count: {tableInfo.Columns.Count}");
await benchmarks.Cleanup();
benchmarks.Dispose();
```

## Notes
When using the `MigrationDiffBenchmarks` type, consider the following:
* The `Setup` method must be called before any other methods to ensure proper initialization.
* The `ComputeDiffAsync_UpToDate` method assumes the expected schema is up-to-date. If this is not the case, the method may not produce accurate results.
* The `GetExpectedSchema` method does not return a value, so it should be called for its side effects only.
* The `Dispose` method should be called when the `MigrationDiffBenchmarks` instance is no longer needed to release resources.
* The `MigrationDiffBenchmarks` type is not thread-safe. It is recommended to create a new instance for each thread or to synchronize access to a shared instance.
