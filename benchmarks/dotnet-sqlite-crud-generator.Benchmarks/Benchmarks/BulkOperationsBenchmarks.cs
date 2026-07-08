#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.BulkTransfer;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for bulk import/export operations measuring throughput and memory efficiency.
/// These operations are critical for ETL pipelines and large dataset processing.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class BulkOperationsBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private BulkImportExportEngine<Product> _bulkEngine = null!;
    private List<Product> _sampleProducts = null!;
    private MemoryStream _importStream = null!;
    private MemoryStream _exportStream = null!;
    private const int BatchSize = 1000;
    private const int TotalItems = 10000;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _bulkEngine = new BulkImportExportEngine<Product>(new ProductRepository(_database), new DataExportService());

        // Generate sample data
        _sampleProducts = new List<Product>(TotalItems);
        for (int i = 0; i < TotalItems; i++)
        {
            _sampleProducts.Add(new Product
            {
                Name = $"Bulk Product {i}",
                Description = $"Bulk import test product {i}",
                Sku = $"BULK-{i:D6}",
                CategoryId = i % 10 + 1,
                Price = (decimal)(10 + (i % 1000)),
                Cost = (decimal)(8 + (i % 800)),
                StockQuantity = 100 + (i % 500),
                ReorderLevel = 50,
                Unit = "piece",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-i % 30),
                UpdatedAt = DateTime.UtcNow.AddDays(-i % 30)
            });
        }

        // Serialize sample data to JSON for import benchmarks
        _importStream = new MemoryStream();
        await System.Text.Json.JsonSerializer.SerializeAsync(_importStream, _sampleProducts, options: new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        _importStream.Position = 0;

        _exportStream = new MemoryStream();
    }

    [Benchmark(Description = "BulkImport: ImportBatchAsync (1000 items)")]
    public async Task<BulkImportResult> ImportBatchAsync()
        => await _bulkEngine.ImportBatchAsync(_sampleProducts.Take(BatchSize));

    [Benchmark(Description = "BulkImport: ImportStreamingAsync (1000 items)")]
    public async Task<BulkImportResult> ImportStreamingAsync()
    {
        var stream = _sampleProducts.Take(BatchSize).ToAsyncEnumerable();
        return await _bulkEngine.ImportStreamingAsync(stream);
    }

    [Benchmark(Description = "BulkExport: ExportToStreamAsync (all items)")]
    public async Task<BulkExportResult> ExportToStreamAsync()
    {
        _exportStream.SetLength(0);
        _exportStream.Position = 0;
        return await _bulkEngine.ExportToStreamAsync(_exportStream, ExportFormat.Json);
    }

    [Benchmark(Description = "BulkTransfer: TransferAsync (1000 items)")]
    public async Task<BulkTransferResult> TransferAsync()
    {
        _exportStream.SetLength(0);
        _exportStream.Position = 0;
        return await _bulkEngine.TransferAsync(
            _importStream,
            ImportFormat.Json,
            _exportStream,
            ExportFormat.Json
        );
    }

    [Benchmark(Description = "BulkImport: ImportFromStreamAsync (10000 items)")]
    public async Task<BulkImportResult> ImportFromStreamAsync()
    {
        _importStream.Position = 0;
        return await _bulkEngine.ImportFromStreamAsync(
            _importStream,
            ImportFormat.Json
        );
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _database.DisposeAsync();
        _importStream.Dispose();
        _exportStream.Dispose();
    }

    public void Dispose()
    {
        _database?.Dispose();
        _importStream?.Dispose();
        _exportStream?.Dispose();
    }
}