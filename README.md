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

## NamingConventionBenchmarks

The `NamingConventionBenchmarks` class provides a set of benchmarks for testing naming conventions. It allows you to test the conversion of C# property names to SQL column names, as well as the validation of property names against a set of rules.

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Benchmarks;

public class Demo
{
    public void Run()
    {
        var namingConventionBenchmarks = new NamingConventionBenchmarks();
        Console.WriteLine(namingConventionBenchmarks.GetTableName);
        Console.WriteLine(namingConventionBenchmarks.GetColumnNamePlain("MyProperty"));
        Console.WriteLine(namingConventionBenchmarks.GetColumnNameDecimal("MyDecimalProperty"));
        var conventionInfo = namingConventionBenchmarks.GetConventionInfo();
        Console.WriteLine(conventionInfo.Name);
        Console.WriteLine(namingConventionBenchmarks.IsValidPropertyNameValid("ValidProperty"));
        Console.WriteLine(namingConventionBenchmarks.IsValidPropertyNameInvalid("Invalid Property"));
        Console.WriteLine(namingConventionBenchmarks.GetApiEndpoint("MyApiEndpoint"));
        Console.WriteLine(namingConventionBenchmarks.CSharpToSql("MyProperty"));
    }
}
```
