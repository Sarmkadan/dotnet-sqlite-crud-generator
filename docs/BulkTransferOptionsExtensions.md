# BulkTransferOptionsExtensions

The `BulkTransferOptionsExtensions` class provides a set of static helper methods for fluent configuration of `BulkTransferOptions` instances within the `dotnet-sqlite-crud-generator` project. These methods simplify the process of tailoring bulk data transfer operations by providing predefined presets and granular configuration controls for performance optimization, safety, and transactional behavior, ensuring consistency across data migration and bulk import/export workflows.

## API

### WithHighPerformance
Configures the `BulkTransferOptions` for maximum throughput, optimizing internal settings to handle large data volumes efficiently, potentially at the expense of strict safety guarantees. Returns a `BulkTransferOptions` instance.

### WithSafety
Configures the `BulkTransferOptions` for maximum data integrity, prioritizing safety checks and robust transaction management, which may result in lower throughput. Returns a `BulkTransferOptions` instance.

### WithBalancedDefaults
Applies a set of predefined, recommended default configurations that provide an optimal balance between performance and data safety for typical bulk operations. Returns a `BulkTransferOptions` instance.

### WithoutProgressReporting
Configures the options to disable progress reporting during the transfer, reducing overhead and improving performance for operations where real-time progress tracking is unnecessary. Returns a `BulkTransferOptions` instance.

### WithCheckpointing
Configures the options to enable checkpointing, allowing the transfer operation to resume from the last successful checkpoint in the event of an interruption. Returns a `BulkTransferOptions` instance.

### WithErrorThreshold
Configures the allowable error threshold for the transfer operation, determining the number of failures that can occur before the operation is terminated. Returns a `BulkTransferOptions` instance.

### WithoutTransactions
Configures the options to perform operations without using explicit transactions. This configuration can significantly increase throughput but sacrifices atomicity, meaning partial data could be committed if a failure occurs. Returns a `BulkTransferOptions` instance.

### WithBatchTimeout
Configures the timeout duration for processing batches during the bulk transfer operation. Returns a `BulkTransferOptions` instance.

### WithBufferSize
Configures the internal buffer size used for the transfer operation, allowing fine-tuning based on memory constraints and data volume. Returns a `BulkTransferOptions` instance.

### Clone
Creates a deep copy of the existing `BulkTransferOptions` instance, allowing for the creation of new configuration objects based on an existing configuration without affecting the original. Returns a `BulkTransferOptions` instance.

### IsHighThroughput
Determines whether the provided `BulkTransferOptions` instance is currently configured for high-throughput operations. Returns a `bool`.

### IsCheckpointingConfigured
Determines whether checkpointing is currently enabled in the provided `BulkTransferOptions` instance. Returns a `bool`.

## Usage

### Example 1: Configuring for High-Performance Import
```csharp
var options = new BulkTransferOptions()
    .WithBalancedDefaults()
    .WithHighPerformance()
    .WithoutProgressReporting();

// Use the options in a bulk transfer engine
var engine = new BulkImportExportEngine(options);
engine.Import(data);
```

### Example 2: Configuring for Safe, Transactional Export
```csharp
var options = new BulkTransferOptions()
    .WithSafety()
    .WithBatchTimeout(TimeSpan.FromSeconds(30));

// Check configuration before proceeding
if (!options.IsHighThroughput())
{
    var engine = new BulkImportExportEngine(options);
    engine.Export(destination);
}
```

## Notes

### Thread Safety
The `BulkTransferOptionsExtensions` methods are designed to be used in a fluent manner. It is recommended that `BulkTransferOptions` instances are not modified concurrently across multiple threads. Users should create and configure their options objects before passing them to the `BulkImportExportEngine`, or use `Clone` to create independent copies if shared configuration is required.

### Edge Cases
- When using `WithoutTransactions`, ensure the underlying data source or the integrity of the data being transferred can tolerate partial imports in the event of an unexpected process termination.
- `WithHighPerformance` may increase the risk of data inconsistencies if a system failure occurs during the bulk transfer. Ensure this configuration is used only when performance is prioritized over atomicity and strict safety guarantees.
