// existing content ...

## QueryBuilderGenerationServiceTests

`QueryBuilderGenerationServiceTests` is a test class that contains unit tests for `QueryBuilderGenerationService`. It provides various test methods to verify the correctness of the query builder generation service, including tests for the `BuildQueryBuilderSource` method and the `GenerateQueryBuilderAsync` method. Below is a realistic example of using `QueryBuilderGenerationServiceTests` in a console application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Tests;

class Program
{
    static void Main(string[] args)
    {
        var test = new QueryBuilderGenerationServiceTests();
        test.BuildQueryBuilderSource_ContainsClassName();
        test.BuildQueryBuilderSource_ContainsTableName();
        test.Dispose();
    }
}
```

// rest of README content
```