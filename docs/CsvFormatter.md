# CsvFormatter

A utility class for converting between C# objects and CSV (Comma-Separated Values) strings. Supports both synchronous and asynchronous formatting of collections or single items, as well as parsing CSV strings back into typed collections or individual objects.

## API

### `CsvFormatter`

The default constructor initializes a new instance of the `CsvFormatter` class. No configuration is required for basic usage.

### `string Format<T>(IEnumerable<T> items)`

Formats an enumerable collection of objects of type `T` into a CSV string.

- **Parameters**
  - `items`: The collection of objects to serialize.
- **Return value**
  - A string containing the CSV representation of the collection.
- **Exceptions**
  - Throws `ArgumentNullException` if `items` is `null`.
  - Throws `InvalidOperationException` if any object in the collection cannot be serialized to CSV.

### `string Format<T>(T item)`

Formats a single object of type `T` into a CSV string.

- **Parameters**
  - `item`: The object to serialize.
- **Return value**
  - A string containing the CSV representation of the object.
- **Exceptions**
  - Throws `ArgumentNullException` if `item` is `null`.
  - Throws `InvalidOperationException` if the object cannot be serialized to CSV.

### `async Task<string> FormatAsync<T>(IEnumerable<T> items)`

Asynchronously formats an enumerable collection of objects of type `T` into a CSV string.

- **Parameters**
  - `items`: The collection of objects to serialize.
- **Return value**
  - A `Task<string>` representing the asynchronous operation, yielding the CSV string.
- **Exceptions**
  - Throws `ArgumentNullException` if `items` is `null`.
  - Throws `InvalidOperationException` if any object in the collection cannot be serialized to CSV.

### `async Task<string> FormatAsync<T>(T item)`

Asynchronously formats a single object of type `T` into a CSV string.

- **Parameters**
  - `item`: The object to serialize.
- **Return value**
  - A `Task<string>` representing the asynchronous operation, yielding the CSV string.
- **Exceptions**
  - Throws `ArgumentNullException` if `item` is `null`.
  - Throws `InvalidOperationException` if the object cannot be serialized to CSV.

### `T? Parse<T>(string csv)`

Parses a CSV string into a single object of type `T`.

- **Parameters**
  - `csv`: The CSV string to deserialize.
- **Return value**
  - An object of type `T` if parsing succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `csv` is `null`.

### `IEnumerable<T>? ParseCollection<T>(string csv)`

Parses a CSV string into a collection of objects of type `T`.

- **Parameters**
  - `csv`: The CSV string to deserialize.
- **Return value**
  - An enumerable collection of objects of type `T` if parsing succeeds; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `csv` is `null`.

## Usage

### Example 1: Synchronous formatting and parsing
