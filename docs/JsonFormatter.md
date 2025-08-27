# JsonFormatter

The `JsonFormatter` class provides a centralized utility for serializing .NET objects to JSON strings and deserializing JSON data back into strongly typed objects or collections within the `dotnet-sqlite-crud-generator` project. It supports both synchronous and asynchronous operations, handles raw `JsonDocument` parsing, and includes custom serialization logic for specific types like `DateTime` via overridden read/write methods, while signaling data integrity issues through the dedicated `FormattingException`.

## API

### Constructors

*   **`public JsonFormatter()`**
    Initializes a new instance of the `JsonFormatter` class with default configuration settings.

*   **`public FormattingException(string message)`**
    Initializes a new instance of the `FormattingException` class with a specified error message. This exception is thrown when serialization or deserialization fails.
    *   **Parameters**: `message` – A string describing the error.

*   **`public FormattingException(string message, Exception innerException)`**
    Initializes a new instance of the `FormattingException` class with a specified error message and a reference to the inner exception that caused this exception.
    *   **Parameters**: `message` – A string describing the error; `innerException` – The exception that is the cause of the current exception.

### Serialization Methods

*   **`public string Format<T>(T value)`**
    Synchronously serializes an object of type `T` into a JSON string.
    *   **Parameters**: `value` – The object to serialize.
    *   **Returns**: A `string` containing the JSON representation of the object.
    *   **Throws**: `FormattingException` if the object cannot be serialized.

*   **`public string Format<T>()`**
    Synchronously serializes a default or context-specific instance of type `T` into a JSON string. (Note: Signature implies parameterless usage, likely targeting a specific internal state or default value resolution).
    *   **Returns**: A `string` containing the JSON representation.
    *   **Throws**: `FormattingException` if serialization fails.

*   **`public async Task<string> FormatAsync<T>(T value)`**
    Asynchronously serializes an object of type `T` into a JSON string.
    *   **Parameters**: `value` – The object to serialize.
    *   **Returns**: A `Task<string>` that yields the JSON representation.
    *   **Throws**: `FormattingException` if the object cannot be serialized.

*   **`public async Task<string> FormatAsync<T>()`**
    Asynchronously serializes a default or context-specific instance of type `T` into a JSON string.
    *   **Returns**: A `Task<string>` that yields the JSON representation.
    *   **Throws**: `FormattingException` if serialization fails.

### Deserialization Methods

*   **`public T? Parse<T>(string json)`**
    Deserializes a JSON string into an object of type `T`.
    *   **Parameters**: `json` – The JSON string to parse.
    *   **Returns**: An instance of `T`, or `null` if the JSON represents a null value or parsing yields no result.
    *   **Throws**: `FormattingException` if the JSON is malformed or cannot be mapped to type `T`.

*   **`public IEnumerable<T>? ParseCollection<T>(string json)`**
    Deserializes a JSON string into a collection of objects of type `T`.
    *   **Parameters**: `json` – The JSON string representing an array or collection.
    *   **Returns**: An `IEnumerable<T>`, or `null` if the JSON is null or empty.
    *   **Throws**: `FormattingException` if the JSON structure does not match a collection or contains invalid elements.

*   **`public JsonDocument ParseAsDocument(string json)`**
    Parses a JSON string into a `JsonDocument` for low-level inspection without immediate strong-typed deserialization.
    *   **Parameters**: `json` – The JSON string to parse.
    *   **Returns**: A `JsonDocument` instance.
    *   **Throws**: `FormattingException` if the input is not valid JSON.

### Utility Methods

*   **`public string? GetJsonPath<T>(string propertyName)`**
    Retrieves the JSON path expression for a specific property of type `T`.
    *   **Parameters**: `propertyName` – The name of the property.
    *   **Returns**: A `string` representing the JSON path, or `null` if the property is not found.

### Overrides

*   **`public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)`**
    Custom logic for reading `DateTime` values from a UTF-8 JSON reader.
    *   **Parameters**: `reader` – The reader to read from; `typeToConvert` – The type being converted; `options` – Serialization options.
    *   **Returns**: A `DateTime` value parsed according to custom rules.
    *   **Throws**: `FormattingException` if the date format is invalid.

*   **`public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)`**
    Custom logic for writing `DateTime` values to a UTF-8 JSON writer.
    *   **Parameters**: `writer` – The writer to write to; `value` – The `DateTime` value to serialize; `options` – Serialization options.
    *   **Throws**: `FormattingException` if the value cannot be written.

## Usage

### Basic Serialization and Deserialization
The following example demonstrates converting a POCO to JSON and back, handling potential formatting errors.

```csharp
var formatter = new JsonFormatter();
var user = new User { Id = 1, Name = "Alice", CreatedAt = DateTime.UtcNow };

try 
{
    // Serialize object to JSON string
    string json = formatter.Format(user);
    
    // Deserialize JSON string back to object
    User? restoredUser = formatter.Parse<User>(json);
    
    if (restoredUser != null)
    {
        Console.WriteLine($"Restored: {restoredUser.Name}");
    }
}
catch (FormattingException ex)
{
    Console.Error.WriteLine($"Failed to process JSON: {ex.Message}");
}
```

### Asynchronous Collection Processing
This example shows how to asynchronously format a list of entities and parse them back as a collection.

```csharp
public async Task ProcessDataAsync(JsonFormatter formatter, List<Order> orders)
{
    // Asynchronously format the entire list
    string jsonPayload = await formatter.FormatAsync(orders);
    
    // Simulate storage or transmission delay
    await Task.Delay(100);
    
    // Parse the JSON back into a strongly-typed collection
    IEnumerable<Order>? retrievedOrders = formatter.ParseCollection<Order>(jsonPayload);
    
    if (retrievedOrders != null)
    {
        foreach (var order in retrievedOrders)
        {
            // Process individual order
            Console.WriteLine($"Order ID: {order.Id}");
        }
    }
}
```

## Notes

*   **Exception Handling**: All parsing and formatting operations wrap underlying serialization errors in `FormattingException`. Callers should explicitly catch this exception rather than generic `Exception` types to handle data format issues gracefully.
*   **Null Safety**: The `Parse<T>` and `ParseCollection<T>` methods return nullable types (`T?` and `IEnumerable<T>?`). A `null` return value indicates the input JSON was null, empty, or explicitly represented a null value, distinct from a thrown exception which indicates malformed data.
*   **DateTime Handling**: The class overrides standard `Read` and `Write` behaviors for `DateTime`. This implies that date strings in JSON produced or consumed by this formatter may follow a specific format different from the default ISO 8601 standard used by `System.Text.Json`. Ensure consistency when exchanging data with external systems.
*   **Thread Safety**: The `JsonFormatter` instance methods rely on stateless serialization logic typical of `System.Text.Json`, suggesting that a single instance is generally safe for concurrent read operations (formatting/parsing). However, if the internal configuration allows mutation, external synchronization may be required for write operations to the formatter's configuration state.
*   **Resource Management**: When using `ParseAsDocument`, the returned `JsonDocument` implements `IDisposable`. Consumers must ensure the document is disposed of properly to release underlying memory buffers, preferably via a `using` statement.
