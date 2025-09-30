// existing content ...

## AuditTrailBenchmarks

The `AuditTrailBenchmarks` class provides a set of benchmarking methods to evaluate the performance of audit trail operations. It allows you to measure the execution time of various operations, such as recording create, update, and delete operations, as well as retrieving entity and user trails.

Example usage:
```csharp
public class AuditTrailBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var benchmarks = new AuditTrailBenchmarks();
        await benchmarks.Setup();
        await benchmarks.RecordCreateOperationAsync();
        await benchmarks.RecordUpdateOperationAsync();
        await benchmarks.RecordDeleteOperationAsync();
        var entityTrail = await benchmarks.GetEntityTrailAsync();
        var userTrail = await benchmarks.GetUserTrailAsync();
        var recent = await benchmarks.GetRecentAsync();
        var query = await benchmarks.QueryAsync();
        var summary = await benchmarks.GetSummaryAsync();
        await benchmarks.BulkRecordAsync();
        await benchmarks.Cleanup();
        benchmarks.Dispose();
    }
}
```

