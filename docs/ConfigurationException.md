# ConfigurationException

Represents errors that occur during the reading, validation, or application of configuration settings for the SQLite CRUD generator. This exception type provides a unified mechanism for signaling configuration-related failures, including missing required settings, malformed values, and invalid connection parameters. It exposes several pre-built static instances for common error scenarios, enabling consistent error reporting without repeated message construction.

## API

### Constructors

#### `ConfigurationException(string message)`
Initializes a new instance of the `ConfigurationException` class with a specified error message.

- **Parameters:**
  - `message` (`string`): The message that describes the error.
- **Return value:** A new `ConfigurationException` instance.
- **Throws:** Nothing beyond base constructor behavior (e.g., `ArgumentNullException` if `message` is `null`, inherited from `Exception`).

#### `ConfigurationException(string message, Exception innerException)`
Initializes a new instance of the `ConfigurationException` class with a specified error message and a reference to the inner exception that is the cause of this exception.

- **Parameters:**
  - `message` (`string`): The message that describes the error.
  - `innerException` (`Exception`): The exception that is the cause of the current exception.
- **Return value:** A new `ConfigurationException` instance.
- **Throws:** Nothing beyond base constructor behavior.

### Static Properties

#### `MissingConfiguration`
Gets a pre-built `ConfigurationException` instance indicating that the required configuration section or file is entirely absent.

- **Return value:** A `ConfigurationException` with a standard message describing the missing configuration condition.
- **Throws:** Never throws.

#### `InvalidConfiguration`
Gets a pre-built `ConfigurationException` instance indicating that the configuration is present but structurally invalid or contains unrecognized elements.

- **Return value:** A `ConfigurationException` with a standard message describing the invalid configuration condition.
- **Throws:** Never throws.

#### `InvalidConnectionString`
Gets a pre-built `ConfigurationException` instance indicating that the configured connection string is missing, empty, or malformed.

- **Return value:** A `ConfigurationException` with a standard message describing the invalid connection string condition.
- **Throws:** Never throws.

#### `InvalidFilePath`
Gets a pre-built `ConfigurationException` instance indicating that a configured file path (e.g., template path, output directory) is invalid, inaccessible, or does not exist.

- **Return value:** A `ConfigurationException` with a standard message describing the invalid file path condition.
- **Throws:** Never throws.

#### `InvalidTimeout`
Gets a pre-built `ConfigurationException` instance indicating that a configured timeout value is outside the acceptable range or not a valid duration.

- **Return value:** A `ConfigurationException` with a standard message describing the invalid timeout condition.
- **Throws:** Never throws.

## Usage

### Example 1: Throwing a Pre-Built Exception for Missing Configuration

```csharp
public void LoadConfiguration(string configFilePath)
{
    if (!File.Exists(configFilePath))
    {
        throw ConfigurationException.MissingConfiguration;
    }

    // Proceed with parsing the configuration file
    var config = JsonSerializer.Deserialize<GeneratorConfig>(
        File.ReadAllText(configFilePath));
}
```

### Example 2: Wrapping a Lower-Level Exception with Context

```csharp
public void ValidateConnectionString(string connectionString)
{
    try
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
    }
    catch (ArgumentException ex)
    {
        throw new ConfigurationException(
            "The provided connection string could not be parsed. See inner exception for details.",
            ex);
    }
}
```

## Notes

- The static properties return new instances on each access, not singletons. This prevents shared mutable state (e.g., the `Data` dictionary) from leaking between unrelated error-handling paths.
- All static properties are safe to access from any thread without synchronization; they perform no shared-state mutation.
- When using the static properties, the caller should not modify the returned exception’s `Data` dictionary or other mutable members if the exception might be observed by multiple consumers, as each access produces a distinct object but the caller may inadvertently share the reference.
- The constructors delegate entirely to `Exception`; any argument validation behavior (such as `ArgumentNullException` for a `null` message) is inherited and not overridden.
- This type does not introduce serialization-specific concerns beyond those of `System.Exception`. Consumers relying on serialization across app domains should ensure the standard exception serialization pattern is followed.
