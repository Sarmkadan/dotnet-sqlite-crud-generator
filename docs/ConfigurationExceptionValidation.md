# ConfigurationExceptionValidation

Static utility class providing validation functionality for SQLite connection configuration settings. Ensures that database connection parameters meet required criteria before use, preventing runtime errors through early validation of connection strings, file paths, and timeout values.

## API

### Validate()

Returns a read-only list of validation error messages for the current configuration state. When no errors exist, returns an empty collection.

**Returns:** `IReadOnlyList<string>` - Collection of error messages describing validation failures.

### IsValid { get; }

Indicates whether the current configuration passes all validation checks.

**Returns:** `bool` - True if configuration is valid, false otherwise.

### EnsureValid()

Throws an exception if the current configuration fails validation. Provides a fail-fast mechanism for configuration validation.

**Exceptions:** Throws `ConfigurationException` when validation fails.

### ValidateConnectionString()

Validates the connection string format and required parameters. Checks for proper SQLite connection string syntax and mandatory fields.

**Returns:** `IReadOnlyList<string>` - Collection of error messages related to connection string validation.

### IsValidConnectionString { get; }

Indicates whether the connection string meets SQLite requirements.

**Returns:** `bool` - True if connection string is valid, false otherwise.

### EnsureValidConnectionString()

Throws an exception if the connection string fails validation requirements.

**Exceptions:** Throws `ConfigurationException` when connection string is invalid.

### ValidateFilePath()

Validates the database file path for accessibility and proper format. Ensures the path points to a valid location for SQLite database operations.

**Returns:** `IReadOnlyList<string>` - Collection of error messages related to file path validation.

### IsValidFilePath { get; }

Indicates whether the file path is suitable for SQLite database operations.

**Returns:** `bool` - True if file path is valid, false otherwise.

### EnsureValidFilePath()

Throws an exception if the file path fails validation.

**Exceptions:** Throws `ConfigurationException` when file path is invalid.

### ValidateTimeout()

Validates the timeout value for reasonableness and proper range. Ensures timeout values are within acceptable bounds for database operations.

**Returns:** `IReadOnlyList<string>` - Collection of error messages related to timeout validation.

### IsValidTimeout { get; }

Indicates whether the timeout value is within valid parameters.

**Returns:** `bool` - True if timeout is valid, false otherwise.

### EnsureValidTimeout()

Throws an exception if the timeout value fails validation.

**Exceptions:** Throws `ConfigurationException` when timeout is invalid.

## Usage

```csharp
using dotnet_sqlite_crud_generator;

// Validate entire configuration before use
var errors = ConfigurationExceptionValidation.Validate();
if (!ConfigurationExceptionValidation.IsValid)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

// Use fail-fast approach for critical operations
try
{
    ConfigurationExceptionValidation.EnsureValid();
    // Proceed with database operations
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Invalid configuration: {ex.Message}");
}
```

```csharp
using dotnet_sqlite_crud_generator;

// Validate specific configuration aspects
if (!ConfigurationExceptionValidation.IsValidConnectionString)
{
    var connectionErrors = ConfigurationExceptionValidation.ValidateConnectionString();
    throw new InvalidOperationException($"Invalid connection string: {string.Join(", ", connectionErrors)}");
}

if (!ConfigurationExceptionValidation.IsValidFilePath)
{
    var pathErrors = ConfigurationExceptionValidation.ValidateFilePath();
    // Handle file path issues
}

// Ensure timeout is properly configured
ConfigurationExceptionValidation.EnsureValidTimeout();
```

## Notes

The class provides both inspection methods (`Validate`, `IsValid` properties) and assertion methods (`EnsureValid` variants), allowing flexible integration into different validation workflows. All methods are static, suggesting the class operates on global or ambient configuration state rather than instance-specific settings.

Thread-safety considerations: Since all members are static and operate on shared configuration state, concurrent access may occur in multi-threaded applications. External synchronization may be required when modifying underlying configuration while validation is in progress.

Edge cases: Empty or null configuration values will produce validation errors. Timeout values outside the valid range (typically positive values) will fail validation. File paths may be validated for existence, permissions, or format depending on implementation details not visible in the public API.
