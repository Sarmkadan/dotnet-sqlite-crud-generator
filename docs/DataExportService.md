# DataExportService

The `DataExportService` provides asynchronous methods to serialize collections of entity objects into common data interchange formats (JSON, CSV, XML) and to persist or stream the resulting output. It also offers a synchronous report generation member and read‑only properties that expose metadata about the most recent export operation.

## API

### DataExportService()
Initializes a new instance of the service. The instance is ready to work with entity data that has been supplied through the service’s internal state (e.g., via dependency injection or other initialization mechanisms not shown in the public API).

### ExportAsJsonAsync\<T\>()
**Purpose:** Serializes the managed collection of entities of type `T` to a JSON string.  
**Parameters:** None (the method operates on the data held by the service).  
**Return Value:** A `Task<string>` that completes with the JSON representation of the data.  
**Exceptions:**  
- `InvalidOperationException` if no entity data is available for type `T`.  
- `SerializationException` if the type `T` contains members that cannot be serialized to JSON.  
- `OperationCanceledException` if the underlying task is cancelled.

### ExportAsCsvAsync\<T\>()
**Purpose:** Serializes the managed collection of entities of type `T` to a CSV‑formatted string.  
**Parameters:** None.  
**Return Value:** A `Task<string>` that completes with the CSV representation of the data.  
**Exceptions:**  
- `InvalidOperationException` if no entity data is available for type `T`.  
- `SerializationException` if a property of `T` cannot be represented as a CSV column (e.g., complex nested objects).  
- `OperationCanceledException` if the operation is cancelled.

### ExportAsXmlAsync\<T\>()
**Purpose:** Serializes the managed collection of entities of type `T` to an XML string.  
**Parameters:** None.  
**Return Value:** A `Task<string>` that completes with the XML representation of the data.  
**Exceptions:**  
- `InvalidOperationException` if no entity data is available for type `T`.  
- `SerializationException` if the type `T` cannot be serialized to XML (e.g., lacks a parameterless constructor or contains unsupported types).  
- `OperationCanceledException` if the operation is cancelled.

### ExportToFileAsync\<T\>()
**Purpose:** Writes the serialized export of entities of type `T` to a file on disk and indicates whether the write succeeded.  
**Parameters:** None (the target file path is determined by the service’s internal configuration).  
**Return Value:** A `Task<bool>` that completes with `true` if the file was written successfully, otherwise `false`.  
**Exceptions:**  
- `IOException` if the file cannot be created, accessed, or written to.  
- `UnauthorizedAccessException` if the service lacks sufficient permissions for the target location.  
- `InvalidOperationException` if no entity data is available for type `T`.  
- `OperationCanceledException` if the operation is cancelled.

### ExportToStreamAsync\<T\>()
**Purpose:** Writes the serialized export of entities of type `T` to an output stream managed by the service.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the stream write operation finishes.  
**Exceptions:**  
- `ObjectDisposedException` if the internal stream has been closed before the write completes.  
- `InvalidOperationException` if no entity data is available for type `T`.  
- `IOException` if an error occurs while writing to the stream.  
- `OperationCanceledException` if the operation is cancelled.

### GenerateExportReport\<T\>()
**Purpose:** Produces a summary report (`ExportReport`) describing the most recent export operation for type `T`, including counts, timestamps, and format used.  
**Parameters:** None.  
**Return Value:** An `ExportReport` instance containing the report data.  
**Exceptions:**  
- `InvalidOperationException` if no export has been performed for type `T` yet.  

### EntityName
**Purpose:** Gets the name of the entity type that the service is currently configured to export.  
**Return Value:** A `string` representing the CLR name of `T`.  
**Exceptions:** None.

### ItemCount
**Purpose:** Gets the number of entity instances that were included in the last export operation.  
**Return Value:** An `int` indicating the count of items exported.  
**Exceptions:** None.

### ExportedAt
**Purpose:** Gets the date and time when the most recent export operation completed.  
**Return Value:** A `DateTime` value (local time).  
**Exceptions:** None.

### AvailableFormats
**Purpose:** Gets the list of data formats that the service can produce for the current entity type.  
**Return Value:** A `string[]` containing format identifiers such as `"json"`, `"csv"`, and `"xml"`.  
**Exceptions:** None.

### SampleItem
**Purpose:** Gets a sample entity instance from the last export operation, useful for preview or debugging.  
**Return Value:** An `object?` that is either an instance of `T` or `null` if no data is available.  
**Exceptions:** None.

### ToString()
**Purpose:** Returns a human‑readable string that summarizes the service’s current state (entity name, item count, and export timestamp).  
**Return Value:** A `string` representation of the service.  
**Exceptions:** None.

## Usage

```csharp
// Example 1: Exporting product data to JSON and saving it to a file.
var exportService = new DataExportService(); // Assume the service has been populated with Product entities elsewhere.
string json = await exportService.ExportAsJsonAsync<Product>();
bool saved = await exportService.ToFileAsync<Product>(); // Hypothetical wrapper; in reality ExportToFileAsync returns bool directly.
if (saved)
{
    Console.WriteLine("Products exported to JSON and written to file.");
}
```

```csharp
// Example 2: Generating a CSV report and inspecting metadata.
var exportService = new DataExportService(); // Assume the service holds Order entities.
string csv = await exportService.ExportAsCsvAsync<Order>();
ExportReport report = exportService.GenerateExportReport<Order>();
Console.WriteLine($"Exported {report.ItemCount} orders at {report.ExportedAt:u} in CSV format.");
```

## Notes

- The service mutates read‑only properties (`EntityName`, `ItemCount`, `ExportedAt`, `SampleItem`) as a side effect of each export operation. Consequently, concurrent calls to the export methods from multiple threads may result in race conditions where these properties reflect an interleaved state. External synchronization is required if the same instance is used concurrently.
- If the service has not been supplied with any data for a given type `T`, all export‑related members will throw an `InvalidOperationException`. Consumers should ensure data is available (e.g., by initializing the service with a data source) before invoking export methods.
- The `ExportToFileAsync<T>` method returns a `bool` rather than throwing on failure to allow callers to handle I/O issues gracefully; however, it still throws for precondition violations such as missing data or insufficient permissions.
- The `ExportToStreamAsync<T>` method does not expose the target stream; the stream is managed internally and is assumed to be reset before each call. Calling the method after the internal stream has been disposed will result in an `ObjectDisposedException`.
- The generic type parameter `T` must be a type that the service’s internal serializers can process (public parameterless constructor and serializable properties for XML; public properties for JSON and CSV). Using a type that does not meet these requirements will cause a `SerializationException`.
- The `AvailableFormats` property reflects the capabilities of the current build; custom formats are not supported without modifying the service.
