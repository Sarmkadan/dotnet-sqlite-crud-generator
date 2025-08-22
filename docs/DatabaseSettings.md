# DatabaseSettings

The `DatabaseSettings` class serves as a configuration container for defining the behavior and connection parameters of the SQLite database within the `dotnet-sqlite-crud-generator` project. It encapsulates essential properties such as the database file location, connection timeout thresholds, and flags controlling automatic database creation, logging verbosity, and schema validation, allowing developers to customize the data access layer without modifying core logic.

## API

### `FilePath`
```csharp
public string? FilePath { get; set; }
```
Specifies the relative or absolute path to the SQLite database file. If set to `null`, the provider may default to an in-memory database or throw an exception depending on the `AutoCreateDatabase` setting. This property accepts a string representing the file system path or `null`. It does not return a value (property getter returns the current string) and does not throw exceptions during standard get/set operations, though invalid path characters may cause failures during the actual connection phase.

### `EnableLogging`
```csharp
public bool EnableLogging { get; set; }
```
Determines whether the database context should emit detailed logs for SQL commands, connection events, and errors. When set to `true`, diagnostic information is written to the configured logging provider; when `false`, logging is suppressed to improve performance. This is a boolean flag with no parameters. It does not throw exceptions.

### `ConnectionTimeout`
```csharp
public int ConnectionTimeout { get; set; }
```
Defines the maximum time in seconds to wait for a connection to the database before terminating the attempt and generating an error. The value must be a positive integer. Setting this to zero or a negative value may result in an exception when the connection string is constructed or when the connection is opened. No parameters are required for access, and the property returns the current timeout integer.

### `AutoCreateDatabase`
```csharp
public bool AutoCreateDatabase { get; set; }
```
Indicates whether the system should automatically create the database file and initialize the schema if the file specified in `FilePath` does not exist. If `true`, the generator attempts to create the file on startup; if `false`, an exception is thrown if the file is missing. This boolean property has no parameters and does not throw during assignment, though runtime errors may occur during initialization if the file is missing and this flag is disabled.

### `Validate`
```csharp
public bool Validate { get; set; }
```
Controls whether strict validation checks are performed on the database schema and configuration settings during initialization. When enabled, the system verifies table structures and constraint integrity before allowing CRUD operations. This is a boolean flag. It does not accept parameters or return values beyond the boolean state and does not throw exceptions during property access, though enabling it may cause initialization to fail if validation rules are not met.

## Usage

### Example 1: Basic Configuration for Development
The following example demonstrates configuring the settings for a local development environment where the database should be created automatically if missing, and detailed logging is required for debugging.

```csharp
var settings = new DatabaseSettings
{
    FilePath = "./data/app.db",
    EnableLogging = true,
    ConnectionTimeout = 30,
    AutoCreateDatabase = true,
    Validate = true
};

// Pass settings to the generator or context initializer
var generator = new CrudGenerator(settings);
```

### Example 2: Production Configuration with Strict Validation
This example illustrates a production setup where the database file must already exist, logging is disabled for performance, and strict validation is enforced to ensure schema integrity.

```csharp
var settings = new DatabaseSettings
{
    FilePath = "/var/lib/sqlite/production.db",
    EnableLogging = false,
    ConnectionTimeout = 15,
    AutoCreateDatabase = false,
    Validate = true
};

if (!File.Exists(settings.FilePath))
{
    throw new FileNotFoundException("Production database file is missing.", settings.FilePath);
}

var generator = new CrudGenerator(settings);
```

## Notes

*   **Null Handling**: The `FilePath` property is nullable. If `FilePath` is `null` and `AutoCreateDatabase` is `true`, the behavior depends on the underlying SQLite provider implementation, which may default to a transient in-memory database. If `AutoCreateDatabase` is `false` and `FilePath` is `null`, initialization will likely fail.
*   **Timeout Constraints**: While the `ConnectionTimeout` property accepts any integer, values less than or equal to zero are invalid for SQLite connection strings and will result in a runtime exception when the connection is attempted.
*   **Thread Safety**: The `DatabaseSettings` class is a simple Data Transfer Object (DTO) with mutable properties. It is not thread-safe for concurrent modification. Instances should be fully configured before being shared across threads or passed to singleton services. Once injected into the database context, the settings should be treated as immutable.
*   **Validation Timing**: Setting `Validate` to `true` defers schema checks until the initialization phase. If the database schema is malformed, the application will fail to start rather than failing during the first CRUD operation.
