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
