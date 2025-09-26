// existing content ...

## BulkOperationsBenchmarks

The `BulkOperationsBenchmarks` class provides a set of benchmarking methods to evaluate the performance of bulk operations on a SQLite database. It allows you to measure the execution time of import and export operations, as well as transfer operations between the database and a stream.

Example usage:
```csharp
public class BulkOperationsBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var bulkOperationsBenchmarks = new BulkOperationsBenchmarks();
        await bulkOperationsBenchmarks.Setup();
        var importResult = await bulkOperationsBenchmarks.ImportBatchAsync();
        var exportResult = await bulkOperationsBenchmarks.ExportToStreamAsync();
        var transferResult = await bulkOperationsBenchmarks.TransferAsync();
        await bulkOperationsBenchmarks.Cleanup();
    }
}
```
