# BulkTransferServiceExtensions

Static extension methods that enable bulk import and export capabilities for SQLite entities within an ASP.NET Core dependency‑injection container.

## API

### `public static IServiceCollection AddBulkTransfer<T>(this IServiceCollection services)`
Registers the core bulk‑transfer services required for entity type `T`.  
- **Parameters**  
  - `services`: The service collection to configure.  
- **Return value**  
  - The same `IServiceCollection` instance to allow method chaining.  
- **Exceptions**  
  - `ArgumentNullException` if `services` is `null`.  

### `public static IServiceCollection AddBulkTransfer<T>(this IServiceCollection services, Action<BulkTransferOptions> configure)`
Registers the bulk‑transfer services for entity type `T` and applies configuration options.  
- **Parameters**  
  - `services`: The service collection to configure.  
  - `configure`: A delegate that configures `BulkTransferOptions`.  
- **Return value**  
  - The same `IServiceCollection` instance.  
- **Exceptions**  
  - `ArgumentNullException` if `services` or `configure` is `null`.  

### `public static BulkImportResult ThrowIfFailed(this BulkImportResult result)`
Throws an exception when the bulk import operation reported failures; otherwise returns the result unchanged.  
- **Parameters**  
  - `result`: The import result to inspect.  
- **Return value**  
  - The original `result` if it indicates success.  
- **Exceptions**  
  - `InvalidOperationException` if `result.HasErrors` is `true` or `result.FailedCount` > 0.  
  - `ArgumentNullException` if `result` is `null`.  

### `public static string ToSummary(this BulkImportResult result)`
Produces a human‑readable summary of the bulk import outcome.  
- **Parameters**  
  - `result`: The import result to summarize.  
- **Return value**  
  - A string describing counts of inserted, updated, and failed records.  
- **Exceptions**  
  - `ArgumentNullException` if `result` is `null`.  

### `public static BulkExportResult ThrowIfFailed(this BulkExportResult result)`
Throws an exception when the bulk export operation reported failures; otherwise returns the result unchanged.  
- **Parameters**  
  - `result`: The export result to inspect.  
- **Return value**  
  - The original `result` if it indicates success.  
- **Exceptions**  
  - `InvalidOperationException` if `result.HasErrors` is `true` or `result.FailedCount` > 0.  
  - `ArgumentNullException` if `result` is `null`.  

### `public static string ToSummary(this BulkExportResult result)`
Produces a human‑readable summary of the bulk export outcome.  
- **Parameters**  
  - `result`: The export result to summarize.  
- **Return value**  
  - A string describing counts of exported and failed records.  
- **Exceptions**  
  - `ArgumentNullException` if `result` is `null`.  

### `public static async IAsyncEnumerable<TItem[]> BatchAsync<TItem>(this IAsyncEnumerable<TItem> source, int batchSize)`
Splits an asynchronous sequence into batches of the specified size.  
- **Parameters**  
  - `source`: The asynchronous sequence to batch.  
  - `batchSize`: The maximum number of items per batch; must be greater than zero.  
- **Return value**  
  - An `IAsyncEnumerable<TItem[]>` where each element is an array containing up to `batchSize` items from `source`.  
- **Exceptions**  
  - `ArgumentNullException` if `source` is `null`.  
  - `ArgumentOutOfRangeException` if `batchSize` is less than or equal to zero.  

### `public static Task<BulkImportResult> ImportIntoAsync<T>(this IBulkTransferService service, IAsyncEnumerable<T> items, CancellationToken cancellationToken = default)`
Performs a bulk import of the supplied items into the SQLite store for entity type `T`.  
- **Parameters**  
  - `service`: The bulk‑transfer service instance.  
  - `items`: The asynchronous sequence of entities to import.  
  - `cancellationToken`: Optional token to observe for cancellation requests.  
- **Return value**  
  - A `Task` that completes with a `BulkImportResult` detailing the operation outcome.  
- **Exceptions**  
  - `ArgumentNullException` if `service` or `items` is `null`.  
  - `OperationCanceledException` if the operation is cancelled via `cancellationToken`.  
  - Any exception thrown by the underlying data access layer (e.g., `SqliteException`) is propagated.  

## Usage

### Registering bulk‑transfer services
```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetSqliteCrudGenerator.BulkTransfer;

var services = new ServiceCollection();
services.AddBulkTransfer<Product>(); // registers defaults
// or with custom options
services.AddBulkTransfer<Order>(opts => opts.BatchSize = 500);
```

### Importing data and handling failures
```csharp
using System.Threading;
using System.Threading.Tasks;
using DotNetSqliteCrudGenerator.BulkTransfer;

public async Task ImportProductsAsync(IAsyncEnumerable<Product> products, CancellationToken ct)
{
    var bulkService = serviceProvider.GetRequiredService<IBulkTransferService>();
    var result = await bulkService.ImportIntoAsync<Product>(products, ct);
    result.ThrowIfFailed(); // throws if any errors occurred
    var summary = result.ToSummary(); // e.g. "Inserted: 1240, Failed: 0"
}
```

## Notes
- The `AddBulkTransfer` extensions are safe to call multiple times; subsequent calls will not duplicate registrations.  
- `ThrowIfFailed` methods do **not** modify the result object; they merely inspect it and may throw.  
- `ToSummary` methods allocate a new string each invocation; they are thread‑safe as they only read immutable properties of the result.  
- `BatchAsync` buffers items internally up to `batchSize`; consuming the returned `IAsyncEnumerable` does not cause the source sequence to be enumerated more than once.  
- `ImportIntoAsync` executes the bulk operation on a background thread; the method itself is asynchronous and respects the supplied `cancellationToken`. The service implementation should ensure that concurrent calls with different entity types do not interfere, but calling the same service instance concurrently for the same `T` may lead to undefined behavior unless the implementation documents otherwise.  
- All extension methods perform null‑checking on their arguments and throw `ArgumentNullException` where appropriate; they do not swallow exceptions from the underlying data access layer.
