# DataExportServiceExtensions

The `DataExportServiceExtensions` static class provides a collection of extension methods for the `IDataExportService<T>` interface within the `dotnet-sqlite-crud-generator` framework. These utilities streamline common data export operations, enabling developers to perform formatted data serialization, multi-format exports, and statistical report generation with minimal boilerplate code.

## API

### ExportAsJsonAsync&lt;T&gt;
Serializes the specified data set and exports it as a JSON file.
*   **Parameters:** `this IDataExportService<T> service`, `string filePath`.
*   **Returns:** `Task<string>` representing the absolute path of the generated JSON file upon successful completion.
*   **Throws:** `IOException` if the file system is inaccessible or the file cannot be written.

### ExportAsCsvAsync&lt;T&gt;
Serializes the specified data set and exports it as a CSV file.
*   **Parameters:** `this IDataExportService<T> service`, `string filePath`.
*   **Returns:** `Task<string>` representing the absolute path of the generated CSV file upon successful completion.
*   **Throws:** `IOException` if the file system is inaccessible or the file cannot be written.

### ExportToMultipleFilesAsync&lt;T&gt;
Exports the data set simultaneously into multiple supported formats based on the provided configuration.
*   **Parameters:** `this IDataExportService<T> service`, `string baseFilePath`.
*   **Returns:** `Task<Dictionary<ExportFormat, bool>>` containing the status of each export operation keyed by the `ExportFormat` enumeration.
*   **Throws:** `ArgumentException` if the `baseFilePath` is invalid or empty.

### ExportAsByteArrayAsync&lt;T&gt;
Exports the data set into a byte array, typically used for returning binary data in web API responses.
*   **Parameters:** `this IDataExportService<T> service`.
*   **Returns:** `Task<byte[]>` containing the serialized data in a default format.
*   **Throws:** `InvalidOperationException` if the data source is empty or cannot be serialized.

### GenerateCsvReportWithStatisticsAsync&lt;T&gt;
Generates a comprehensive CSV report that includes aggregated statistical data for the entity set.
*   **Parameters:** `this IDataExportService<T> service`, `string filePath`.
*   **Returns:** `Task<string>` representing the absolute path of the generated report file.
*   **Throws:** `IOException` if writing to the specified path fails.

### ExportWithFormatDetectionAsync&lt;T&gt;
Analyzes the file extension of the target path and automatically detects and applies the appropriate serialization format for the export.
*   **Parameters:** `this IDataExportService<T> service`, `string filePath`.
*   **Returns:** `Task<bool>` indicating `true` if the export was successful, or `false` if the format is unsupported or the operation failed.
*   **Throws:** `NotSupportedException` if the file extension does not map to a supported format.

## Usage

### Basic JSON Export
```csharp
public async Task SaveDataToJson(IDataExportService<User> userService)
{
    string path = "exports/users.json";
    string resultPath = await userService.ExportAsJsonAsync(path);
    Console.WriteLine($"Data exported successfully to: {resultPath}");
}
```

### Generating a CSV Statistical Report
```csharp
public async Task CreateUserStatsReport(IDataExportService<User> userService)
{
    string reportPath = "reports/user_statistics.csv";
    string path = await userService.GenerateCsvReportWithStatisticsAsync(reportPath);
    Console.WriteLine($"Statistical report generated at: {path}");
}
```

## Notes

*   **Thread Safety:** The methods in this class are thread-safe, provided that the underlying `IDataExportService<T>` implementation is thread-safe and the I/O operations do not conflict with concurrent access to the same file path.
*   **Asynchronous I/O:** All methods are asynchronous and should be awaited to ensure proper I/O completion. Avoid blocking on these tasks using `.Result` or `.Wait()` to prevent potential deadlocks in synchronization contexts.
*   **Error Handling:** These methods generally propagate `IOException` exceptions. Ensure appropriate try-catch blocks are implemented in the calling code to handle file system permissions or disk space issues gracefully.
*   **Data Consistency:** Ensure the data source queried by the `IDataExportService<T>` is in a consistent state before invoking export methods to avoid partial or corrupted output files.
