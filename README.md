## Performance

Microbenchmarks are located in `benchmarks/` and use [BenchmarkDotNet](https://benchmarkdotnet.org/).
Run them with:

```bash
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks \
--configuration Release -- --filter '*'
```

To run specific benchmark categories:

```bash
# Run all benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release

# Run only repository benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Repository*'

# Run only service benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Service*'

# Run only bulk operations benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Bulk*'

# Run only audit trail benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Audit*'

# Run only migration diff benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*MigrationDiff*'

# Run only query builder benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*QueryBuilder*'
```

Results below were measured on an AMD Ryzen 9 5900X, .NET 10, Release build.