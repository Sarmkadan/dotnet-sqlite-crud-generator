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
