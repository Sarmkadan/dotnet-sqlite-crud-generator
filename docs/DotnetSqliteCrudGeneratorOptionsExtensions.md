# DotnetSqliteCrudGeneratorOptionsExtensions

`DotnetSqliteCrudGeneratorOptionsExtensions` provides a set of fluent extension methods for configuring `DotnetSqliteCrudGeneratorOptions` instances. These methods simplify the setup process for the generator, enabling a declarative and readable configuration style when defining database connection details, performance settings, and operational modes.

## API

### WithConnectionString
Configures the generator to use the specified SQLite connection string.
- **Parameters:** `string connectionString` - The SQLite connection string.
- **Returns:** The updated `DotnetSqliteCrudGeneratorOptions` instance.

### WithInMemoryDatabase
Configures the generator to utilize an in-memory SQLite database instance instead of a file-based one.
- **Returns:** The updated `DotnetSqliteCrudGeneratorOptions` instance.

### WithDevelopmentPoolSettings
Configures connection pooling settings specifically optimized for development environments.
- **Parameters:** `int poolSize` (maximum pool size), `bool enablePooling` (flag to enable or disable pooling).
- **Returns:** The updated `DotnetSqliteCrudGeneratorOptions` instance.

### WithCacheDisabled
Configures the generator to bypass all caching mechanisms, ensuring direct data access.
- **Returns:** The updated `DotnetSqliteCrudGeneratorOptions` instance.

### WithProcessorBasedWorkerCount
Sets the number of concurrent workers utilized by the generator based on the underlying processor capacity.
- **Parameters:** `int workerCount` - The number of worker threads to allocate.
- **Returns:** The updated `DotnetSqliteCrudGeneratorOptions` instance.

### ValidateWithDetails
Performs structural and logical validation on the provided options instance.
- **Parameters:** `DotnetSqliteCrudGeneratorOptions options` - The options instance to validate.
- **Returns:** `void`.
- **Throws:** `ValidationException` if the provided options configuration is invalid or incomplete.

### Clone
Creates a deep copy of the specified `DotnetSqliteCrudGeneratorOptions` instance.
- **Parameters:** `DotnetSqliteCrudGeneratorOptions options` - The instance to clone.
- **Returns:** A new `DotnetSqliteCrudGeneratorOptions` instance that is an independent copy of the original.

## Usage

### Basic Configuration
```csharp
var options = new DotnetSqliteCrudGeneratorOptions()
    .WithConnectionString("Data Source=my_database.db")
    .WithProcessorBasedWorkerCount(4);

DotnetSqliteCrudGeneratorOptionsExtensions.ValidateWithDetails(options);
```

### Advanced Configuration
```csharp
var options = new DotnetSqliteCrudGeneratorOptions()
    .WithInMemoryDatabase()
    .WithDevelopmentPoolSettings(10, true)
    .WithCacheDisabled();

var clonedOptions = DotnetSqliteCrudGeneratorOptionsExtensions.Clone(options);
```

## Notes

- **Fluent API:** The extension methods that return `DotnetSqliteCrudGeneratorOptions` allow for method chaining, supporting a fluent configuration style.
- **Thread Safety:** These extension methods are intended for use during the initial configuration phase of the application lifecycle. The resulting `DotnetSqliteCrudGeneratorOptions` object is not inherently thread-safe if mutated after being passed to the generator service.
- **Validation:** Always call `ValidateWithDetails` after completing the fluent configuration chain to ensure the resulting options are valid before passing them to the generator.
- **Cloning:** The `Clone` method performs a deep copy, which is useful when creating multiple generator instances with slightly different variations of a base configuration.
