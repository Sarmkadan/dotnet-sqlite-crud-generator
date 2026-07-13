// existing content ...

## CacheBenchmarks

The `CacheBenchmarks` class provides a set of benchmarking methods to evaluate the performance of a cache. It allows you to measure the execution time of cache hits, misses, and set operations.

Example usage:
```csharp
public class CacheBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var cacheBenchmarks = new CacheBenchmarks();
        await cacheBenchmarks.Setup();
        var product = await cacheBenchmarks.GetHit();
        if (product == null)
        {
            product = await cacheBenchmarks.GetMiss();
            await cacheBenchmarks.Set(product);
        }
        var exists = await cacheBenchmarks.ExistsHit();
        var productOrSet = await cacheBenchmarks.GetOrSetHit();
        var productOrSetMiss = await cacheBenchmarks.GetOrSetMiss();
        await cacheBenchmarks.Cleanup();
    }
}
```
