// existing content ...

## BulkTransferOptions

`BulkTransferOptions` controls batching, concurrency, progress reporting, checkpointing, validation behavior, and error handling for bulk import/export operations performed by `BulkImportExportEngine<T>`. It allows tuning performance characteristics versus safety guarantees depending on workload requirements.

Below is a realistic example configuring and using bulk transfer with custom options:

```csharp
// Configure bulk transfer options for a high-throughput, memory-efficient import
var options = new BulkTransferOptions
{
    BatchSize = 1500,
    MaxConcurrency = 6,
    BufferSize = 131_072, // 128 KB
    EnableProgressReporting = true,
    ProgressReportingInterval = 250,
    ValidationMode = ValidationMode.Lenient,
    MaxErrorThreshold = 500,
    UseTransactions = true,
    BatchTimeout = TimeSpan.FromSeconds(45),
    EnableCheckpointing = true,
    CheckpointFilePath = @"./import-checkpoint.json"
};

// Register services with custom options
var services = new ServiceCollection();
services.AddBulkTransfer<Product>(options);

var provider = services.BuildServiceProvider();
var engine = provider.GetRequiredService<BulkImportExportEngine<Product>>();

// Perform a bulk import with progress callback
var result = await engine.ImportFromFileAsync(
    "products.ndjson",
    ImportFormat.Json,
    progress => Console.WriteLine($"Batch {progress.BatchNumber}: {progress.Processed} items"),
    options
);

if (result.HasErrors)
{
    Console.Error.WriteLine(result.ToSummary());
    return;
}

Console.WriteLine($"Imported {result.TotalProcessed} items in {result.Duration.TotalSeconds:F2}s");
```

## BulkTransferServiceExtensions

The `BulkTransferServiceExtensions` class provides a set of extension methods for registering
bulk-transfer services and working with bulk import and export results. It simplifies the
process of adding bulk transfer capabilities to an application, especially when used in
conjunction with dependency injection.

Here's a realistic example of using some of the methods provided by `BulkTransferServiceExtensions`:

```csharp
// Register bulk transfer services for Product entity
var services = new ServiceCollection();
services.AddBulkTransfer<Product>(options =>
{
    options.BatchSize = 1000;
    options.EnableProgressReporting = true;
});

// Create a service provider and resolve the bulk transfer service
var serviceProvider = services.BuildServiceProvider();
var bulkTransferService = serviceProvider.GetRequiredService<IBulkTransferService<Product>>();

// Perform a bulk import
var importResult = await bulkTransferService.ImportFromFileAsync(
    "products.json",
    progress => Console.WriteLine($"Imported {progress.Processed} of {progress.Total} items")
);

// Throw if the import failed
importResult.ThrowIfFailed();

// Summarize the import result
Console.WriteLine(importResult.ToSummary());

// Partition an async sequence into batches
var products = new[] { new Product(), new Product(), new Product() }.ToAsyncEnumerable();
await foreach (var batch in products.BatchAsync(2))
{
    Console.WriteLine($"Batch: {string.Join(", ", batch.Select(p => p.GetType().Name))}");
}

// Stream entities into a database
var streamingResult = await products.ImportIntoAsync(
    bulkTransferService,
    progress: new Progress<BulkTransferProgress>(p => Console.WriteLine($"Progress: {p.Processed}"))
);
Console.WriteLine(streamingResult.ToSummary());
```

## BulkTransferPipeline

The `BulkTransferPipeline<T>` is a fluent pipeline builder that composes import, validation, transformation, and export stages into a single, cohesive transfer operation. It wraps an underlying `IBulkTransferService<T>` to provide advanced configuration options, such as per-entity transformations, filtering, progress tracking, and automatic retries, streamlining complex data migration and ETL tasks.

Below is a realistic example of configuring and executing a pipeline:

```csharp
// Configure a bulk transfer pipeline with transformation, filtering, and retries
var pipeline = BulkTransferPipeline<Product>.Create(bulkTransferService)
    .WithOptions(new BulkTransferOptions { BatchSize = 500 })
    .WithTransform(p => {
        p.Name = p.Name.ToUpper();
        return p;
    })
    .WithFilter(p => p.Price > 0)
    .WithProgress(new Progress<BulkTransferProgress>(p => Console.WriteLine($"Progress: {p.Processed}")))
    .OnError(e => Console.Error.WriteLine($"Error on {e.Entity}: {e.Message}"))
    .WithRetry(3, TimeSpan.FromSeconds(5));

// Execute the pipeline
var result = await pipeline.ImportFromFileAsync("products.json", ImportFormat.Json);
Console.WriteLine($"Imported {result.TotalProcessed} items.");
```

// ... rest of README content ...
