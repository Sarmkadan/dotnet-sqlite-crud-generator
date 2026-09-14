# Bulk Transfer Pipeline

The `BulkTransferPipeline<T>` class provides a fluent interface for composing bulk data transfer operations that combine import, validation, transformation, and export stages into a single executable workflow.

## Overview

The pipeline wraps an `IBulkTransferService<T>` implementation and allows fluent configuration of:
- Transformation functions applied during import
- Filtering predicates applied during export
- Progress reporting
- Error handling
- Retry policies
- Bulk transfer options controlling batching, concurrency, and validation

## Stages

The pipeline executes the following logical stages:

1. **Import** - Read data from source (file or stream) using specified format
2. **Transformation** - Apply per-entity transform function (optional)
3. **Persistence** - Save transformed entities to data store via underlying service
4. **Filtering** - Apply server-side filter during export (optional)
5. **Export** - Write data to destination (file or stream) using specified format

## Configuration Options

### Fluent Configuration Methods

| Method | Description |
|--------|-------------|
| `WithOptions(BulkTransferOptions)` | Replace default options with custom instance |
| `WithTransform(Func<T, T?>)` | Register per-entity transformation (return null to exclude) |
| `WithFilter(Func<T, bool>)` | Register server-side filter for export operations |
| `WithProgress(IProgress<BulkTransferProgress>)` | Attach progress observer |
| `OnError(Action<BulkTransferError>)` | Register error handler for per-record failures |
| `WithRetry(int count, TimeSpan delay)` | Enable automatic retry of failed import operations |
| `Create(IBulkTransferService<T>)` | Static factory for creating unconfigured pipeline |

### BulkTransferOptions

The `BulkTransferOptions` class controls low-level behavior of the underlying import/export engine:

#### Batching & Concurrency
- `BatchSize` (default: 500) - Entities per database write batch
- `MaxConcurrency` (default: 4) - Concurrent batch write tasks
- `UseTransactions` (default: true) - Wrap batches in database transactions

#### Progress Reporting
- `EnableProgressReporting` (default: true) - Emit progress snapshots
- `ProgressReportingInterval` (default: 100) - Records between progress updates
- `BufferSize` (default: 65536) - Stream I/O buffer size in bytes

#### Checkpointing
- `EnableCheckpointing` (default: false) - Persist state for resumable imports
- `CheckpointFilePath` - Required when checkpointing enabled

#### Validation & Error Handling
- `ValidationMode` (default: Lenient) - How to handle validation failures:
  - `Strict` - Fail entire operation on first invalid record
  - `Lenient` - Skip invalid records, continue with valid ones
  - `None` - Disable validation for maximum throughput
- `MaxErrorThreshold` (default: 1000) - Max errors before aborting operation

#### Retry & Timeouts
- `BatchTimeout` (default: 30s) - Per-batch operation timeout
- `MaxRetryAttempts` (default: 5) - Retry attempts for SQLite lock contention
- `RetryBaseDelay` (default: 50ms) - Base delay for exponential backoff
- `RetryMaxDelay` (default: 5s) - Upper bound on retry delay

#### Predefined Option Profiles
- `BulkTransferOptions.Default` - Balanced settings for most workloads
- `BulkTransferOptions.HighThroughput` - Optimized for maximum ingestion speed
- `BulkTransferOptions.Safe` - Optimized for data safety over speed

## Supported Formats

### ImportFormat
- `Json` - JSON array or newline-delimited JSON (NDJSON)
- `Csv` - Comma-separated values with optional header row
- `Xml` - XML document with root element wrapping individual entity elements

### ExportFormat
- `Json` - JSON serialization
- `Csv` - CSV serialization
- `Xml` - XML serialization

## Usage Example

```csharp
// Register services (typically in Startup.cs or Program.cs)
services.AddDbContext<ApplicationDbContext>();
services.AddApplicationServices(); // Registers IRepository<T,int>
services.AddBulkTransfer<Product>(); // Registers bulk transfer services for Product

// In your service or controller:
public class ProductDataService
{
    private readonly IBulkTransferService<Product> _transferService;
    
    public ProductDataService(IBulkTransferService<Product> transferService)
    {
        _transferService = transferService;
    }
    
    public async Task TransferProductsAsync()
    {
        // Create pipeline with custom options
        var pipeline = BulkTransferPipeline<Product>.Create(_transferService)
            .WithOptions(new BulkTransferOptions
            {
                BatchSize = 1000,
                MaxConcurrency = 6,
                EnableCheckpointing = true,
                CheckpointFilePath = "./checkpoints/product-import.json",
                ValidationMode = ValidationMode.Lenient,
                ProgressReportingInterval = 500
            })
            .WithTransform(product =>
            {
                // Transform: trim whitespace, set default category if missing
                product.Name = product.Name?.Trim();
                if (string.IsNullOrEmpty(product.Category))
                    product.Category = "Uncategorized";
                return product;
            })
            .WithFilter(p => p.IsActive && p.Price > 0) // Only export active products with positive price
            .OnError(error => 
                Console.WriteLine($"Error processing row {error.RowNumber}: {error.Message}"))
            .WithRetry(3, TimeSpan.FromSeconds(5)); // Retry failed imports up to 3 times
        
        // Execute transfer from CSV to JSON
        await using var sourceStream = File.OpenRead("products-import.csv");
        await using var destStream = File.Create("products-export.json");
        
        var result = await pipeline.TransferAsync(
            sourceStream,
            ImportFormat.Csv,
            destStream,
            ExportFormat.Json);
        
        Console.WriteLine(result.ToString());
        // Output: Transfer: import [Import complete: 15,000 succeeded, 23 failed in 45.23s (331 rec/s)] | export [Export complete: 14,950 records (1.2 MB) in 12.45s (1,200 rec/s)]
        
        // Check if operation was successful
        if (result.IsSuccess)
        {
            // Both import and export succeeded
            var stats = pipeline.GetStatistics();
            Console.WriteLine($"Session stats: {stats.TotalRecordsImported} imported, {stats.TotalRecordsExported} exported");
        }
    }
}
```

## Important Notes

- Pipelines are **not thread-safe** and must not be shared across concurrent operations
- Configured stages (transform, filter, progress, error handler, retry) persist on the builder instance and apply to every subsequent execution call
- The pipeline is safely reusable within the same scope for multiple operations
- When using `ExportToFileAsync`, the configured filter is **not** applied - use `ExportToStreamAsync` when filtering is required
- Transformation functions returning `null` will silently exclude the entity from persistence
- Error handlers are invoked synchronously after each operation completes