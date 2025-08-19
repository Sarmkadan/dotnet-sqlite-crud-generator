# QueryBuilderGenerationService

Generates C# source code for a type-specific query builder class that wraps raw SQLite operations into a fluent, compile-time-safe API. The service reads metadata from an existing entity class, resolves its table and column mappings, and emits a complete query builder file—ready to be written to disk or returned as a string for further processing.

## API

### `public string OutputPath`

Gets the file system path where the generated query builder source file will be written by default. This value is determined during construction and reflects the target directory and naming conventions applied to the entity type.

- **Type**: `string`
- **Access**: read-only instance property
- **Exceptions**: none

---

### `public QueryBuilderGenerationService`

Constructs a new instance of the service configured for a specific entity type and output location. All required metadata resolution and path computation occurs at construction time.

- **Parameters**: (constructor arguments depend on the underlying implementation; typically an entity type and a base output directory)
- **Exceptions**: may throw `ArgumentException` if the supplied type is not a valid entity, or `InvalidOperationException` if metadata cannot be resolved

---

### `public async Task<string> GenerateQueryBuilderAsync`

Asynchronously produces the full C# source code for the query builder class, including all standard CRUD operations and any custom mappings derived from the entity. The returned string contains the complete file content.

- **Returns**: `Task<string>` — the generated source code as a string
- **Exceptions**: 
  - `InvalidOperationException` if entity metadata is missing or malformed
  - `IOException`-derived exceptions if file-system access is required during generation and fails

---

### `public string BuildQueryBuilderSource`

Synchronously builds and returns the query builder source code. This method performs the same generation logic as `GenerateQueryBuilderAsync` but runs entirely on the calling thread.

- **Returns**: `string` — the complete generated source code
- **Exceptions**:
  - `InvalidOperationException` if entity metadata is missing or malformed
  - `IOException`-derived exceptions if file-system access is required during generation and fails

## Usage

### Example 1: Generate and write to the configured output path

```csharp
var service = new QueryBuilderGenerationService(typeof(Customer), @"C:\Generated\Queries");
string source = await service.GenerateQueryBuilderAsync();
await File.WriteAllTextAsync(service.OutputPath, source);
```

### Example 2: Obtain source in-memory for further transformation

```csharp
var service = new QueryBuilderGenerationService(typeof(Order), @"C:\Generated\Queries");
string rawSource = service.BuildQueryBuilderSource();
string formattedSource = CodeFormatter.Format(rawSource);
Console.WriteLine(formattedSource);
```

## Notes

- `OutputPath` is fixed at construction and does not change even if the underlying file system state changes later. Consumers should treat it as read-only configuration.
- `GenerateQueryBuilderAsync` and `BuildQueryBuilderSource` produce identical output for the same instance; the asynchronous overload exists for callers that need to avoid blocking while generation performs I/O-bound metadata resolution.
- Both generation methods may throw `InvalidOperationException` if the entity type lacks the expected attributes or contains unsupported property types. Validate entity definitions before calling generation to avoid runtime failures.
- The service does not perform any file writes itself—it only produces source strings. Callers are responsible for writing to `OutputPath` or any other destination.
- Thread safety: instance members are not guarded by synchronization. A single instance should be used from one thread at a time. Concurrent calls to the generation methods on the same instance may lead to race conditions if internal state is lazily initialized.
