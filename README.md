## QueryBuilderBenchmarks

The `QueryBuilderBenchmarks` class provides a set of benchmarks for measuring the performance of the query builder. It allows you to generate query builders for different scenarios, such as generating a query builder for a user, order, or category.

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Benchmarks;

public class Demo
{
    public async Task RunAsync()
    {
        var queryBuilderBenchmarks = new QueryBuilderBenchmarks();
        await queryBuilderBenchmarks.Setup();
        await queryBuilderBenchmarks.GenerateQueryBuilderAsync();
        queryBuilderBenchmarks.BuildQueryBuilderSource();
        await queryBuilderBenchmarks.GenerateQueryBuilderForUserAsync();
        await queryBuilderBenchmarks.GenerateQueryBuilderForOrderAsync();
        await queryBuilderBenchmarks.GenerateQueryBuilderForCategoryAsync();
        await queryBuilderBenchmarks.Cleanup();
        queryBuilderBenchmarks.Dispose();
    }
}
```
```