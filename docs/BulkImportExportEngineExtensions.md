# BulkImportExportEngineExtensions

The `BulkImportExportEngineExtensions` class provides a set of static extension methods designed to simplify bulk data operations within the `dotnet-sqlite-crud-generator` framework. These methods facilitate the efficient import, export, and transfer of entity data in JSON format, enabling streamlined high-volume data handling between SQLite databases and external JSON sources or between different database contexts.

## API

### ImportFromJsonAsync&lt;T&gt;
Imports a collection of entities of type `T` from a specified JSON source into the database.

*   **Parameters**: `string jsonPath` (The path to the JSON file), `DbContext context` (The target database context).
*   **Return**: A `Task` representing the asynchronous operation, containing a `BulkImportResult` indicating the outcome.
*   **Exceptions**: Throws `InvalidOperationException` if deserialization fails, or `DbUpdateException` if database constraints are violated.

### ExportToJsonAsync&lt;T&gt;
Exports the entire dataset of type `T` from the database to a JSON string.

*   **Parameters**: `DbContext context` (The source database context).
*   **Return**: A `Task` representing the asynchronous operation, containing a tuple with a `BulkExportResult` and the resulting `string` containing the JSON data.
*   **Exceptions**: Throws `SerializationException` if the data cannot be converted to JSON.

### ExportFilteredToJsonAsync&lt;T&gt;
Exports a subset of the dataset of type `T` based on a provided filter, serializing the results to a JSON string.

*   **Parameters**: `DbContext context` (The source database context), `Expression&lt;Func&lt;T, bool&gt;&gt; filter` (The filtering criteria).
*   **Return**: A `Task` representing the asynchronous operation, containing a tuple with a `BulkExportResult` and the resulting `string` containing the filtered JSON data.
*   **Exceptions**: Throws `ArgumentNullException` if the filter is null, or `SerializationException` if the data cannot be converted to JSON.

### TransferToAsync&lt;T&gt;
Transfers data of type `T` directly from a source database context to a target destination context.

*   **Parameters**: `DbContext source` (The source database context), `DbContext destination` (The target database context).
*   **Return**: A `Task` representing the asynchronous operation, containing a `BulkTransferResult` detailing the transfer status.
*   **Exceptions**: Throws `InvalidOperationException` if connectivity to either context fails.

### GetStats&lt;T&gt;
Retrieves performance statistics for bulk operations performed on entity type `T`.

*   **Parameters**: `DbContext context` (The database context to query).
*   **Return**: A `BulkTransferStatistics` object containing metrics such as execution time and record counts.

## Usage

### Exporting and Importing Data
```csharp
using MyProject.Models;
using MyProject.Data;

// Export all Orders to a file
var (exportResult, json) = await BulkImportExportEngineExtensions.ExportToJsonAsync<Order>(dbContext);
if (exportResult.Success)
{
    await File.WriteAllTextAsync("orders.json", json);
}

// Import Orders from a file
var importResult = await BulkImportExportEngineExtensions.ImportFromJsonAsync<Order>("orders.json", dbContext);
```

### Exporting Filtered Data and Checking Statistics
```csharp
using MyProject.Models;

// Export only high-value orders
var (exportResult, json) = await BulkImportExportEngineExtensions.ExportFilteredToJsonAsync<Order>(
    dbContext, 
    o => o.TotalAmount > 1000
);

// Retrieve statistics for the operation
var stats = BulkImportExportEngineExtensions.GetStats<Order>(dbContext);
Console.WriteLine($"Processed {stats.TotalRecordsProcessed} records.");
```

## Notes

*   **Thread Safety**: These extension methods utilize the underlying `DbContext` instances. Standard Entity Framework Core thread-safety rules apply; the `DbContext` is not thread-safe and should not be accessed concurrently by multiple threads during these operations.
*   **Memory Usage**: For `ExportToJsonAsync` and `ExportFilteredToJsonAsync`, very large datasets may consume significant amounts of memory when serializing to a single JSON string. Consider using stream-based approaches for exceptionally large datasets.
*   **SQLite Locking**: Bulk import and transfer operations may acquire table-level locks within the SQLite database. Ensure that these operations are performed during periods of low concurrent activity to avoid `database is locked` exceptions.
