# GenerationService

The `GenerationService` class provides asynchronous methods for generating C# source code artifacts commonly used in a SQLite-based CRUD application. It is designed to produce repository interfaces, database migration scripts, and gRPC service stubs as strings, which can then be written to files or processed further. The service is instantiated without configuration parameters; it relies on internal or ambient project context (e.g., configuration files, conventions) to determine the generation targets.

## API

### `public GenerationService()`

Initializes a new instance of the `GenerationService` class. No parameters are required. The constructor does not perform any I/O or heavy initialization.

### `public async Task<string> GenerateRepositoryInterfaceAsync()`

Generates the C# source code for a repository interface.

- **Parameters**: None.
- **Returns**: A `Task<string>` that resolves to the complete source code of the repository interface.
- **Throws**:  
  - `InvalidOperationException` if the required project metadata (e.g., table name, namespace) cannot be resolved from the current environment.  
  - `IOException` if an underlying file read fails during metadata resolution.

### `public async Task<string> GenerateMigrationAsync()`

Generates a SQLite migration script (typically a `.sql` file or embedded migration class) that creates or alters the database schema for the target entity.

- **Parameters**: None.
- **Returns**: A `Task<string>` that resolves to the migration script as a string.
- **Throws**:  
  - `InvalidOperationException` if the entity schema information is missing or ambiguous.  
  - `NotSupportedException` if the migration type (e.g., initial vs. incremental) cannot be determined.

### `public async Task<string> GenerateGrpcServiceAsync()`

Generates the C# source code for a gRPC service implementation that wraps the CRUD operations for the target entity.

- **Parameters**: None.
- **Returns**: A `Task<string>` that resolves to the complete gRPC service class source code.
- **Throws**:  
  - `InvalidOperationException` if the protobuf service definition or entity mapping is not configured.  
  - `FileNotFoundException` if a required `.proto` file cannot be located.

## Usage

### Example 1: Generating a repository interface and writing it to a file

```csharp
using System.IO;
using System.Threading.Tasks;

public class CodeGenerator
{
    public async Task GenerateAsync()
    {
        var service = new GenerationService();

        string interfaceCode = await service.GenerateRepositoryInterfaceAsync();
        await File.WriteAllTextAsync("IProductRepository.cs", interfaceCode);

        string migrationScript = await service.GenerateMigrationAsync();
        await File.WriteAllTextAsync("Migration_001.sql", migrationScript);
    }
}
```

### Example 2: Generating a gRPC service and embedding it in a build pipeline

```csharp
using System;
using System.Threading.Tasks;

public class BuildTask
{
    public async Task<int> ExecuteAsync()
    {
        try
        {
            var service = new GenerationService();
            string grpcCode = await service.GenerateGrpcServiceAsync();
            Console.WriteLine(grpcCode);
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Generation failed: {ex.Message}");
            return 1;
        }
    }
}
```

## Notes

- **Edge cases**:  
  - If the project contains multiple entity definitions, the service uses a default or first-found entity. This may lead to unexpected output if the intended entity is not the first one discovered.  
  - The `GenerateMigrationAsync` method may produce an empty string if no schema changes are detected (e.g., when the database is already up-to-date).  
  - When the required configuration files (e.g., `appsettings.json`, `.csproj` metadata) are missing, all three methods throw `InvalidOperationException`.

- **Thread safety**:  
  Instances of `GenerationService` are not thread-safe. Concurrent calls to any of the generation methods on the same instance may produce inconsistent results or throw exceptions. Each thread or asynchronous operation should use its own instance, or callers must synchronize access externally.
