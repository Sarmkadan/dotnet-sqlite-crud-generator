# GenerateGrpcAttributeExtensions

The `GenerateGrpcAttributeExtensions` class provides a set of static extension methods designed to simplify the retrieval and interpretation of configuration settings defined within the `GenerateGrpcAttribute`. These methods encapsulate common logic used by the code generation pipeline to determine the scope and style of the gRPC services being produced based on attribute parameters.

## API

### ShouldGenerateCrudOperations
Determines whether standard CRUD (Create, Read, Update, Delete) operations should be generated for the target service.

*   **Parameters:** `GenerateGrpcAttribute` instance.
*   **Returns:** `bool` - `true` if CRUD operations are enabled, otherwise `false`.

### ShouldGenerateAsyncMethods
Determines whether the generated gRPC methods should include asynchronous (`async`) implementations.

*   **Parameters:** `GenerateGrpcAttribute` instance.
*   **Returns:** `bool` - `true` if asynchronous methods should be generated, otherwise `false`.

### GetEffectiveServiceName
Resolves the actual service name to be used for the gRPC service definition, handling any naming conventions or overrides applied by the attribute.

*   **Parameters:** `GenerateGrpcAttribute` instance.
*   **Returns:** `string` - The determined effective service name.

### ShouldGenerateStreamingOperations
Checks if the generator should include support for gRPC streaming operations for the target service.

*   **Parameters:** `GenerateGrpcAttribute` instance.
*   **Returns:** `bool` - `true` if streaming operations are enabled, otherwise `false`.

## Usage

```csharp
// Example 1: Checking generation options during code generation
var attribute = serviceType.GetCustomAttribute<GenerateGrpcAttribute>();

if (attribute.ShouldGenerateCrudOperations())
{
    GenerateCrudImplementation();
}

if (attribute.ShouldGenerateAsyncMethods())
{
    GenerateAsyncDefinitions();
}
```

```csharp
// Example 2: Configuring service metadata
var attribute = serviceType.GetCustomAttribute<GenerateGrpcAttribute>();
string serviceName = attribute.GetEffectiveServiceName();

Console.WriteLine($"Configuring gRPC service: {serviceName}");

if (attribute.ShouldGenerateStreamingOperations())
{
    ConfigureStreamingEndpoints();
}
```

## Notes

*   **Thread Safety:** The methods in this class are thread-safe and stateless, as they perform read-only operations on the provided `GenerateGrpcAttribute` instance.
*   **Input Validation:** While these methods are designed to work with `GenerateGrpcAttribute`, providing a `null` attribute instance will result in a `NullReferenceException`. Ensure the attribute instance is validated prior to calling these extensions.
