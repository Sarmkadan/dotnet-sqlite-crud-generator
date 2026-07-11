# GenerateGrpcAttribute

The `GenerateGrpcAttribute` is a configuration attribute used within the `dotnet-sqlite-crud-generator` project to control the code generation behavior for gRPC services. By applying this attribute to a target class or interface, developers can specify whether CRUD operations, asynchronous methods, or streaming capabilities should be generated, along with defining the resulting service name and namespace for the produced artifacts.

## API

### GenerateAsync
Gets or sets a value indicating whether the generator should produce asynchronous method implementations.
*   **Purpose**: Enables the generation of `async`/`await` compatible gRPC methods, allowing non-blocking I/O operations during database interactions.
*   **Parameters**: None (Property setter accepts a `bool`).
*   **Return Value**: Returns a `bool` representing the current configuration state.
*   **Throws**: This property does not throw exceptions.

### GenerateCrud
Gets or sets a value indicating whether standard Create, Read, Update, and Delete (CRUD) methods should be generated for the target entity.
*   **Purpose**: Toggles the inclusion of conventional data manipulation methods in the generated gRPC service definition.
*   **Parameters**: None (Property setter accepts a `bool`).
*   **Return Value**: Returns a `bool` representing the current configuration state.
*   **Throws**: This property does not throw exceptions.

### ServiceName
Gets or sets the explicit name assigned to the generated gRPC service.
*   **Purpose**: Overrides the default naming convention to define a specific identifier for the service in the resulting `.proto` definition and C# class.
*   **Parameters**: None (Property setter accepts a `string?`).
*   **Return Value**: Returns a `string` if defined, or `null` if the default naming strategy should be used.
*   **Throws**: This property does not throw exceptions.

### GenerateStreaming
Gets or sets a value indicating whether server-side or client-side streaming methods should be included in the generation output.
*   **Purpose**: Enables the creation of streaming RPCs for handling large datasets or real-time data feeds rather than unary calls.
*   **Parameters**: None (Property setter accepts a `bool`).
*   **Return Value**: Returns a `bool` representing the current configuration state.
*   **Throws**: This property does not throw exceptions.

### Namespace
Gets or sets the target namespace for the generated C# classes.
*   **Purpose**: Ensures the generated code resides within a specific logical namespace, preventing naming conflicts and organizing the codebase.
*   **Parameters**: None (Property setter accepts a `string?`).
*   **Return Value**: Returns a `string` if defined, or `null` if the source namespace should be inherited.
*   **Throws**: This property does not throw exceptions.

## Usage

The following example demonstrates configuring the attribute to generate a fully asynchronous CRUD service with a custom name and namespace, while disabling streaming.

```csharp
using DotNetSqliteCrudGenerator;

[GenerateGrpc(
    GenerateAsync = true,
    GenerateCrud = true,
    GenerateStreaming = false,
    ServiceName = "UserManagementService",
    Namespace = "MyApp.Grpc.Services"
)]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
}
```

The next example illustrates a scenario focused solely on high-throughput data streaming, where standard CRUD operations are explicitly disabled.

```csharp
using DotNetSqliteCrudGenerator;

[GenerateGrpc(
    GenerateAsync = true,
    GenerateCrud = false,
    GenerateStreaming = true,
    ServiceName = "LogStreamService",
    Namespace = null // Inherits namespace from the declaring assembly
)]
public class SystemLog
{
    public long Timestamp { get; set; }
    public string Message { get; set; }
}
```

## Notes

*   **Configuration Interdependence**: While the properties are independent booleans, logical conflicts may arise during the generation phase if `GenerateCrud` is set to `false` while other flags imply data manipulation. The generator implementation determines how these combinations are resolved.
*   **Null Handling**: Both `ServiceName` and `Namespace` accept `null` values. When `null`, the generator falls back to default inference logic based on the decorated class name and its existing namespace. Passing an empty string (`""`) may result in invalid generated code depending on the generator's validation rules.
*   **Thread Safety**: As this attribute serves as a passive data container for metadata read during compile-time or build-time generation, it is inherently thread-safe. No internal state mutation occurs during the generation process.
*   **Edge Cases**: Setting `GenerateAsync` to `false` while `GenerateStreaming` is `true` may produce blocking streaming calls, which is generally discouraged in gRPC patterns but is technically permissible by this attribute's schema.
