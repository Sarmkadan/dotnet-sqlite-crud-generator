// existing content ...

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

// ... rest of README content ...
