# ConfigurationExceptionExtensions

Extension methods for `ConfigurationException` that simplify adding contextual information and checking for missing configuration.

## API

### `WithContext(ConfigurationException, string key, string value)`
Adds a configuration key-value pair to the exception's context data.

- **Parameters**
  - `exception`: The `ConfigurationException` instance to extend.
  - `key`: The configuration key to add.
  - `value`: The configuration value to associate with the key.
- **Return Value**
  Returns the same `ConfigurationException` instance for method chaining.
- **Throws**
  Throws `ArgumentNullException` if `exception` or `key` is `null`.

### `WithContext(ConfigurationException, string key, object value)`
Adds a configuration key-value pair with an object value to the exception's context data.

- **Parameters**
  - `exception`: The `ConfigurationException` instance to extend.
  - `key`: The configuration key to add.
  - `value`: The configuration value to associate with the key.
- **Return Value**
  Returns the same `ConfigurationException` instance for method chaining.
- **Throws**
  Throws `ArgumentNullException` if `exception` or `key` is `null`.

### `WithMessage(ConfigurationException, string message)`
Replaces or augments the exception's message with additional context.

- **Parameters**
  - `exception`: The `ConfigurationException` instance to extend.
  - `message`: The additional message to append to the exception's existing message.
- **Return Value**
  Returns the same `ConfigurationException` instance for method chaining.
- **Throws**
  Throws `ArgumentNullException` if `exception` or `message` is `null`.

### `IsMissingConfiguration(Exception)`
Checks whether an exception or its inner exception is a `ConfigurationException` indicating missing configuration.

- **Parameters**
  - `exception`: The exception to inspect.
- **Return Value**
  Returns `true` if the exception or any inner exception is a `ConfigurationException` with a message indicating missing configuration; otherwise, `false`.
- **Throws**
  Throws `ArgumentNullException` if `exception` is `null`.

## Usage
