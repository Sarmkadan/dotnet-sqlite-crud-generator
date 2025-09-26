// existing content ...

## DotnetSqliteCrudGeneratorOptionsExtensions

The `DotnetSqliteCrudGeneratorOptionsExtensions` class provides a set of extension methods for configuring `DotnetSqliteCrudGeneratorOptions`. It allows you to customize the behavior of the CRUD generator, such as setting the connection string, enabling/disabling caching, and configuring worker settings.

Here's an example usage:
```csharp
using DotNet.SQLite.CrudGenerator.Configuration;

public class Demo
{
    public void Run()
    {
        var options = new DotnetSqliteCrudGeneratorOptions();
        options = options.WithConnectionString("Data Source=example.db")
                         .WithDevelopmentPoolSettings()
                         .WithCacheDisabled()
                         .WithProcessorBasedWorkerCount();

        // Validate options
        DotnetSqliteCrudGeneratorOptionsExtensions.ValidateWithDetails(options);

        // Clone options
        var clonedOptions = DotnetSqliteCrudGeneratorOptionsExtensions.Clone(options);
    }
}
```
// existing content ...
