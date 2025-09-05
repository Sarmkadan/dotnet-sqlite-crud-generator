# GenerationServiceExtensions

The `GenerationServiceExtensions` class provides a suite of static extension methods designed to automate the scaffolding of essential components within a SQLite-based CRUD application. These utilities streamline development workflows by programmatically generating source code for repository interfaces, gRPC service definitions, and database migrations based on specified entity types, ensuring structural consistency across the generated codebase.

## API

### GenerateRepositoryInterfaceAsync&lt;T&gt;

Generates the source code for a repository interface corresponding to the entity type `T`.

- **Returns:** A `Task&lt;string&gt;` containing the generated C# source code for the interface.
- **Exceptions:** Throws an `ArgumentException` if the type `T` does not satisfy the requirements for repository generation.

### GenerateGrpcServiceAsync&lt;T&gt;

Generates the source code for a gRPC service definition tailored for the entity type `T`.

- **Returns:** A `Task&lt;string&gt;` containing the generated C# source code for the gRPC service.
- **Exceptions:** Throws an `InvalidOperationException` if the type `T` lacks the necessary metadata for gRPC service generation.

### GenerateMigrationAsync&lt;T&gt;

Generates the SQL migration script necessary to align the database schema with the entity type `T`.

- **Returns:** A `Task&lt;string&gt;` containing the SQL migration script.
- **Exceptions:** Throws a `NotSupportedException` if the entity mapping for `T` cannot be resolved to a valid SQLite schema.

### GenerateRepositoryInterfacesAsync

Generates repository interfaces for a predefined collection of entity types, returning a dictionary that maps each `Type` to its corresponding generated source code.

- **Returns:** A `Task&lt;Dictionary&lt;Type, string&gt;&gt;` containing the mapping of types to their generated repository interface source code.

## Usage

### Generating a single repository interface

```csharp
using MyProject.Models;
using MyProject.Generators;

public async Task GenerateCodeAsync()
{
    // Generate the repository interface for the User entity
    string sourceCode = await GenerationServiceExtensions.GenerateRepositoryInterfaceAsync<User>();
    
    // Save to disk
    await File.WriteAllTextAsync("IUserRepository.cs", sourceCode);
}
```

### Generating repository interfaces for multiple models

```csharp
using MyProject.Models;
using MyProject.Generators;

public async Task GenerateAllInterfacesAsync()
{
    // Generate repository interfaces for all registered model types
    var generatedCodeMap = await GenerationServiceExtensions.GenerateRepositoryInterfacesAsync();
    
    foreach (var entry in generatedCodeMap)
    {
        string fileName = $"I{entry.Key.Name}Repository.cs";
        await File.WriteAllTextAsync(fileName, entry.Value);
    }
}
```

## Notes

- **Edge Cases:** If the provided type `T` does not have proper configuration (e.g., missing primary key attributes, incorrect class modifiers), the generation methods may throw exceptions. Ensure all target entities are correctly annotated before invoking these methods.
- **Thread Safety:** The methods in `GenerationServiceExtensions` are `static` and perform stateless code generation. They are thread-safe regarding internal state; however, if these methods are invoked to write to the same physical file concurrently from multiple threads, external synchronization mechanisms must be implemented to prevent file access conflicts.
- **Performance:** These methods involve reflection and template processing. For large projects with many entities, invoke these generation tasks asynchronously to prevent blocking the main execution thread.
