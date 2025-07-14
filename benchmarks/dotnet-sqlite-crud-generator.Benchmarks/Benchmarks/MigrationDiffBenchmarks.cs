#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for migration diffing operations measuring schema comparison performance.
/// Critical for maintaining database schema consistency across development and production environments.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class MigrationDiffBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private MigrationDiffService _migrationDiffService = null!;
    private Product _sampleProduct = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _migrationDiffService = new MigrationDiffService(_database);

        // Create sample entity with various property types
        _sampleProduct = new Product
        {
            Name = "Diff Test Product",
            Description = "Product for schema diff testing",
            Sku = "DIFF-001",
            CategoryId = 1,
            Price = 99.99m,
            Cost = 79.99m,
            StockQuantity = 100,
            ReorderLevel = 10,
            Unit = "piece",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add to database to create table
        var productRepo = new ProductRepository(_database);
        await productRepo.AddAsync(_sampleProduct);
        await _database.SaveChangesAsync();
    }

    [Benchmark(Description = "MigrationDiff: ComputeDiffAsync (up-to-date schema)")]
    public async Task ComputeDiffAsync_UpToDate()
        => await _migrationDiffService.ComputeDiffAsync(typeof(Product));

    [Benchmark(Description = "MigrationDiff: GetActualSchemaAsync")]
    public async Task GetActualSchemaAsync()
        => await _migrationDiffService.GetActualSchemaAsync("Products");

    [Benchmark(Description = "MigrationDiff: GetExpectedSchema")]
    public void GetExpectedSchema()
        => _migrationDiffService.GetExpectedSchema(typeof(Product));

    [Benchmark(Description = "MigrationDiff: GetTableInfoAsync")]
    public async Task GetTableInfoAsync()
        => await _migrationDiffService.GetActualSchemaAsync("Products");

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _database.DisposeAsync();
    }

    public void Dispose()
    {
        _database?.Dispose();
    }
}