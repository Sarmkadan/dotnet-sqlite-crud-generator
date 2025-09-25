## Performance

Microbenchmarks are located in `benchmarks/` and use [BenchmarkDotNet](https://benchmarkdotnet.org/).  
Run them with:

```bash
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks \
--configuration Release -- --filter '*'
```

...

## Validation


All configuration options are validated using DataAnnotations. Invalid configurations will throw a `ValidationException` during application startup.

```csharp
try
{
    var options = new DotnetSqliteCrudGeneratorOptions();
    options.Validate(); // Will throw if invalid
}
catch (ValidationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## ExternalApiClientExtensions

The `ExternalApiClientExtensions` class provides HTTP client extensions with retry logic and pagination support for external API integrations. It includes methods for single-item retrieval, collection fetching, and health checks with detailed response metadata.

```csharp
var result = await ExternalApiClientExtensions.GetWithRetryAsync<MyDataModel>(
    "https://api.example.com/data/123", 
    cancellationToken: CancellationToken.None);

if (result.Success)
{
    Console.WriteLine($"Fetched {result.ResponseTimeMs}ms: {result.Item}");
}
else
{
    Console.WriteLine($"Error: {result.Error} (Timestamp: {result.Timestamp})");
}
```

## BulkImportExportEngineExtensions

The `BulkImportExportEngineExtensions` class provides convenient extension methods for bulk importing and exporting entities to and from JSON, as well as transferring data between databases. It supports asynchronous operations, filtered exports, and statistics gathering.

```csharp
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.BulkTransfer;
using Microsoft.EntityFrameworkCore;

public class Demo
{
    public async Task RunAsync()
    {
        var sourceContext = new SourceDbContext();
        var destinationContext = new DestinationDbContext();

        // Import entities from JSON
        string jsonInput = File.ReadAllText("entities.json");
        BulkImportResult importResult = await BulkImportExportEngineExtensions.ImportFromJsonAsync<MyEntity>(jsonInput, sourceContext);

        // Export all entities to JSON
        (BulkExportResult exportResult, string jsonOutput) = await BulkImportExportEngineExtensions.ExportToJsonAsync<MyEntity>(sourceContext);

        // Export filtered entities to JSON
        Expression<Func<MyEntity, bool>> filter = e => e.IsActive;
        (BulkExportResult filteredResult, string filteredJson) = await BulkImportExportEngineExtensions.ExportFilteredToJsonAsync<MyEntity>(sourceContext, filter);

        // Transfer entities from source to destination
        BulkTransferResult transferResult = await BulkImportExportEngineExtensions.TransferToAsync<MyEntity>(sourceContext, destinationContext);

        // Get transfer statistics
        BulkTransferStatistics stats = BulkImportExportEngineExtensions.GetStats<MyEntity>(sourceContext);
    }
}
```
