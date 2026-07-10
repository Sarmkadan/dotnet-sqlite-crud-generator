# BulkImportExportEngine
The `BulkImportExportEngine` class is designed to facilitate bulk import and export operations for SQLite databases. It provides a range of methods for importing data from various sources, such as streams and files, and exporting data to streams or files. Additionally, it supports streaming entities and transferring data between databases.

## API
* `public BulkImportExportEngine`: The constructor for the `BulkImportExportEngine` class.
* `public async Task<BulkImportResult> ImportFromStreamAsync`: Imports data from a stream. The return value is a `BulkImportResult` object, which contains information about the import operation. This method throws if there is an error reading from the stream or importing the data.
* `public async Task<BulkImportResult> ImportFromFileAsync`: Imports data from a file. The return value is a `BulkImportResult` object, which contains information about the import operation. This method throws if there is an error reading from the file or importing the data.
* `public async Task<BulkImportResult> ImportBatchAsync`: Imports a batch of data. The return value is a `BulkImportResult` object, which contains information about the import operation. This method throws if there is an error importing the data.
* `public async Task<BulkImportResult> ImportStreamingAsync`: Imports data using a streaming approach. The return value is a `BulkImportResult` object, which contains information about the import operation. This method throws if there is an error importing the data.
* `public async Task<BulkExportResult> ExportToStreamAsync`: Exports data to a stream. The return value is a `BulkExportResult` object, which contains information about the export operation. This method throws if there is an error writing to the stream or exporting the data.
* `public async Task<BulkExportResult> ExportToFileAsync`: Exports data to a file. The return value is a `BulkExportResult` object, which contains information about the export operation. This method throws if there is an error writing to the file or exporting the data.
* `public async Task<BulkExportResult> ExportFilteredAsync`: Exports filtered data. The return value is a `BulkExportResult` object, which contains information about the export operation. This method throws if there is an error exporting the data.
* `public async IAsyncEnumerable<T> StreamEntitiesAsync`: Streams entities from the database. The return value is an `IAsyncEnumerable<T>`, which allows for asynchronous iteration over the entities.
* `public async Task<BulkTransferResult> TransferAsync`: Transfers data between databases. The return value is a `BulkTransferResult` object, which contains information about the transfer operation. This method throws if there is an error transferring the data.
* `public BulkTransferStatistics GetStatistics`: Gets statistics about the transfer operation. The return value is a `BulkTransferStatistics` object, which contains information about the transfer operation.

## Usage
The following example demonstrates how to use the `BulkImportExportEngine` class to import data from a file and export data to a stream:
```csharp
var engine = new BulkImportExportEngine();
var importResult = await engine.ImportFromFileAsync("data.csv");
var exportResult = await engine.ExportToStreamAsync(new MemoryStream());
```
The following example demonstrates how to use the `BulkImportExportEngine` class to stream entities and transfer data between databases:
```csharp
var engine = new BulkImportExportEngine();
await foreach (var entity in engine.StreamEntitiesAsync())
{
    Console.WriteLine(entity);
}
var transferResult = await engine.TransferAsync();
```

## Notes
The `BulkImportExportEngine` class is designed to be used in a multithreaded environment, but it is not thread-safe by default. To ensure thread-safety, the class should be used with a lock or other synchronization mechanism. Additionally, the class may throw exceptions if there are errors reading or writing to streams or files, or if there are errors importing or exporting data. The `GetStatistics` method may return incomplete or inaccurate statistics if the transfer operation is not complete. The `StreamEntitiesAsync` method may throw exceptions if there are errors streaming the entities. The `TransferAsync` method may throw exceptions if there are errors transferring the data.
