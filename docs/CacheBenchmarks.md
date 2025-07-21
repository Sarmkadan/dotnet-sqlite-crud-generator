# CacheBenchmarks

`CacheBenchmarks` is a benchmarking suite for evaluating the performance characteristics of a cache implementation used within the `dotnet-sqlite-crud-generator` project. It measures throughput and latency for common cache operations—hits, misses, sets, existence checks, and combined get-or-set patterns—under controlled conditions, enabling developers to quantify the overhead introduced by caching layers when generating SQLite CRUD operations.

## API

### Setup
```csharp
public async Task Setup()
```
Prepares the benchmarking environment before each iteration. This method initializes the cache instance and any required dependencies, ensuring a consistent starting state for subsequent benchmark operations. It does not return a value. Exceptions may be thrown if the underlying cache infrastructure fails to initialize.

### GetHit
```csharp
public async ValueTask<Product?> GetHit()
```
Retrieves a `Product` from the cache when the requested key is guaranteed to be present (a cache hit). Returns the cached `Product` instance, or `null` if the cache implementation returns null for a known-present key—an unexpected condition that would indicate a cache inconsistency. Throws if the cache access layer encounters an unrecoverable error.

### GetMiss
```csharp
public async ValueTask<Product?> GetMiss()
```
Retrieves a `Product` from the cache when the requested key is guaranteed to be absent (a cache miss). Returns `null` to signal the miss, consistent with standard cache semantics. Throws if the cache access layer encounters an unrecoverable error.

### Set
```csharp
public async ValueTask Set()
```
Inserts or updates a `Product` entry in the cache. This method measures the cost of writing an object into the caching layer. It does not return a value. Throws if the cache write operation fails due to capacity constraints, serialization errors, or infrastructure unavailability.

### ExistsHit
```csharp
public async ValueTask<bool> ExistsHit()
```
Checks whether a specific key exists in the cache when the key is known to be present. Returns `true` to confirm the hit. A return value of `false` under these conditions signals a cache integrity problem. Throws if the cache query operation fails.

### GetOrSetHit
```csharp
public async ValueTask<Product?> GetOrSetHit()
```
Performs a get-or-set operation where the requested key already exists in the cache. The method retrieves the cached `Product` without invoking the fallback factory function. Returns the cached `Product` instance. Throws if the cache access layer fails.

### GetOrSetMiss
```csharp
public async ValueTask<Product?> GetOrSetMiss()
```
Performs a get-or-set operation where the requested key is absent, forcing the cache to invoke the fallback factory, store the result, and return it. Returns the newly created and cached `Product` instance. Throws if the factory function fails, the cache write fails, or the cache access layer encounters an error.

### Cleanup
```csharp
public async Task Cleanup()
```
Releases resources and resets state after each benchmark iteration completes. This method disposes of cache instances, clears temporary data, and ensures no residual state leaks between benchmark runs. It does not return a value. Exceptions may be thrown if resource disposal encounters errors.

## Usage

### Example 1: Running Benchmarks with BenchmarkDotNet
```csharp
using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<CacheBenchmarks>();
        Console.WriteLine($"Mean GetHit latency: {summary.Reports.First(r => r.BenchmarkCase.Descriptor.WorkloadMethod.Name == nameof(CacheBenchmarks.GetHit)).ResultStatistics.Mean} ns");
    }
}
```

### Example 2: Manual Invocation for Diagnostic Testing
```csharp
var benchmarks = new CacheBenchmarks();
await benchmarks.Setup();

var stopwatch = Stopwatch.StartNew();
var hitResult = await benchmarks.GetHit();
stopwatch.Stop();
Console.WriteLine($"GetHit returned {hitResult?.Id} in {stopwatch.ElapsedMicroseconds} µs");

stopwatch.Restart();
var missResult = await benchmarks.GetMiss();
stopwatch.Stop();
Console.WriteLine($"GetMiss returned {(missResult == null ? "null" : "unexpected object")} in {stopwatch.ElapsedMicroseconds} µs");

await benchmarks.Cleanup();
```

## Notes

- **Iteration Isolation**: `Setup` and `Cleanup` are invoked before and after each benchmark iteration, respectively. The cache state established in `Setup` must be fully deterministic; any residual state from a prior iteration that survives `Cleanup` will skew subsequent measurements.
- **Null Semantics for Hits**: `GetHit` and `GetOrSetHit` are documented as returning `Product?` to accommodate implementations where a cache hit on a corrupted or evicted entry yields null. In a correctly functioning cache, these methods should never return null. Benchmarks that observe null returns for hit operations indicate a defect in the cache under test.
- **Thread Safety**: The public signatures are all asynchronous instance methods with no explicit synchronization primitives. BenchmarkDotNet executes each benchmark method sequentially by default. Concurrent invocation across multiple threads is not part of the intended design; doing so without external synchronization may produce race conditions in the underlying cache and invalidate timing measurements.
- **ValueTask Return Types**: Methods returning `ValueTask` or `ValueTask<T>` may complete synchronously when the underlying cache operation does not require true asynchrony. Benchmark harnesses should account for this by measuring elapsed wall-clock time rather than assuming asynchronous continuation overhead.
- **Exception Propagation**: All methods may throw exceptions originating from the cache infrastructure. Benchmark runners typically treat unhandled exceptions as benchmark failures. Production code wrapping these cache operations should implement retry or fallback logic appropriate to the specific cache implementation being measured.
