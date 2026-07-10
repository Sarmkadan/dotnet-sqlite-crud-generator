# GenerationException

`GenerationException` is a custom exception type used by the dotnet-sqlite-crud-generator tool to signal errors encountered during code generation. It extends the standard `Exception` class to include additional context about the generation process, such as the type of code being generated, the source entity involved, and the line number where the error occurred.

## API

### `GenerationException(string message)`
Constructs a new `GenerationException` with a specified error message.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
- **Remarks**: This constructor initializes the base `Exception` class with the provided message.

### `GenerationException()`
Constructs a new `GenerationException` with default values for all properties.

- **Remarks**: All properties (`GenerationType`, `SourceEntity`, `LineNumber`) will be `null`.

### `string? GenerationType`
Gets or sets the type of code generation that was in progress when the error occurred (e.g., "Entity", "Repository", "Service").

- **Type**: `string?`
- **Remarks**: This property is optional and may be `null` if not set.

### `string? SourceEntity`
Gets or sets the name of the entity involved in the generation process when the error occurred.

- **Type**: `string?`
- **Remarks**: This property is optional and may be `null` if not set.

### `int? LineNumber`
Gets or sets the line number in the source file where the error occurred.

- **Type**: `int?`
- **Remarks**: This property is optional and may be `null` if the error is not tied to a specific line.

### `static GenerationException MissingConfiguration`
A static factory method that creates a `GenerationException` indicating that required configuration is missing.

- **Returns**: A `GenerationException` with a predefined message: "Missing required configuration for code generation."
- **Remarks**: Use this when the tool cannot proceed due to missing settings.

### `static GenerationException InvalidModel`
A static factory method that creates a `GenerationException` indicating that the input model is invalid.

- **Returns**: A `GenerationException` with a predefined message: "The provided model is invalid for code generation."
- **Remarks**: Use this when the input data does not meet the expected schema.

### `static GenerationException CodeGenerationFailed`
A static factory method that creates a `GenerationException` indicating that code generation failed.

- **Returns**: A `GenerationException` with a predefined message: "Code generation failed due to an unexpected error."
- **Remarks**: Use this as a catch-all for general generation failures.

## Usage

```csharp
// Example 1: Throwing a MissingConfiguration exception
try
{
    var config = LoadConfiguration();
    if (config == null)
    {
        throw GenerationException.MissingConfiguration;
    }
}
catch (GenerationException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// Example 2: Throwing a custom GenerationException with context
try
{
    var entity = ParseEntity(sourceCode);
    GenerateCode(entity);
}
catch (Exception ex)
{
    throw new GenerationException("Failed to generate repository for entity")
    {
        GenerationType = "Repository",
        SourceEntity = "Customer",
        LineNumber = 42
    };
}
```

## Notes

- **Thread Safety**: This type is safe for concurrent use. All properties are get/set and no internal state is shared.
- **Edge Cases**:
  - If `GenerationType`, `SourceEntity`, or `LineNumber` are set to `null`, they will not contribute to the exception's message or stack trace unless explicitly included in a custom message.
  - The static factory methods (`MissingConfiguration`, `InvalidModel`, `CodeGenerationFailed`) are immutable and safe to call from any context.
  - When rethrowing an exception with additional context, ensure the original exception is preserved in the `InnerException` if relevant.
