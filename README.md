// existing content ...

## GenerationException

The `GenerationException` class represents an exception that occurs during the generation process. It provides additional information about the type of generation error, the source entity, and the line number where the error occurred.

Example usage:
```csharp
try
{
    // Code that may throw a GenerationException
}
catch (GenerationException ex)
{
    Console.WriteLine($"Generation error: {ex.Message}");
    Console.WriteLine($"Generation type: {ex.GenerationType}");
    Console.WriteLine($"Source entity: {ex.SourceEntity}");
    Console.WriteLine($"Line number: {ex.LineNumber}");
}
```

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

## ValidationException

The `ValidationException` class represents an exception thrown when entity validation fails during CRUD operations. It collects multiple validation errors and provides methods to add errors and create exceptions from existing error lists.

Example usage:
```csharp
try
{
    var product = new Product
    {
        Name = "Test Product",
        Price = -100
    };

    if (product.Price < 0)
    {
        var validationException = new ValidationException("Product validation failed");
        validationException.AddError(nameof(Product.Price), "Price must be a positive value");
        throw validationException;
    }
}
catch (ValidationException ex) when (ex.Errors.Any())
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"- {error.Property}: {error.Message}");
    }
}

// Creating from multiple errors
var errors = new List<ValidationError>
{
    new() { Property = nameof(Product.Name), Message = "Name is required" },
    new() { Property = nameof(Product.Price), Message = "Price must be positive" }
};
var exception = ValidationException.FromErrors(errors);
throw exception;
```

## MigrationDiffBenchmarks

The `MigrationDiffBenchmarks` class provides a set of benchmarking methods to evaluate the performance of migration diff operations. It allows you to measure the execution time of various operations, such as computing the diff between two schema versions, getting the actual schema, and getting table info.

Example usage:
```csharp
public class MigrationDiffBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var benchmarks = new MigrationDiffBenchmarks();
        await benchmarks.Setup();
        await benchmarks.ComputeDiffAsync_UpToDate();
        benchmarks.GetExpectedSchema();
        await benchmarks.GetTableInfoAsync();
        await benchmarks.Cleanup();
        benchmarks.Dispose();
    }
}
```

