# ValidateCommand

A lightweight command object used to encapsulate a single validation rule for a model property. It supports asynchronous execution of the validation logic and carries metadata to describe the validation failure when it occurs.

## API

### `public async Task<int> ExecuteAsync()`
Runs the encapsulated validation logic and returns an exit code indicating success or failure.
- **Return value**: `Task<int>`
  - `0` if validation succeeds.
  - Any non-zero value if validation fails.
- **Exceptions**: Throws if the underlying validation logic throws (e.g., database access failures, I/O errors).

### `public string ModelName`
Gets the name of the model being validated.
- **Type**: `string`
- **Usage**: Used to construct error messages or log context.

### `public string? PropertyName`
Gets the name of the property being validated.
- **Type**: `string?`
- **Usage**: Optional; may be `null` if the validation applies to the entire model rather than a specific property.

### `public string Message`
Gets the user-facing message to display when validation fails.
- **Type**: `string`
- **Usage**: Should be localized or user-friendly.

### `public ValidationSeverity Severity`
Gets the severity level of the validation failure.
- **Type**: `ValidationSeverity`
- **Usage**: Used to categorize failures (e.g., warning vs. error).

### `public sealed class RequiredAttribute : Attribute`
A marker attribute indicating that a property is required.
- **Purpose**: Decorates properties to signal mandatory presence during model validation.
- **Usage**: Applied to properties; consumed by validation pipelines or generators.

## Usage
