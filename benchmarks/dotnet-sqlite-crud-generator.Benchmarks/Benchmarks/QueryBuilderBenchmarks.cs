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
/// Benchmarks for query builder generation operations measuring performance of SQL query generation.
/// Critical for code generation scenarios and dynamic query building.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class QueryBuilderBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private QueryBuilderGenerationService _queryBuilderService = null!;
    private const int Iterations = 100;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _queryBuilderService = new QueryBuilderGenerationService("./Generated");

        // Create sample entity
        var productRepo = new ProductRepository(_database);
        await productRepo.AddAsync(new Product
        {
            Name = "Query Builder Test",
            Sku = "QB-001",
            CategoryId = 1,
            Price = 49.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _database.SaveChangesAsync();
    }

    [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderAsync")]
    public async Task GenerateQueryBuilderAsync()
        => await _queryBuilderService.GenerateQueryBuilderAsync(typeof(Product));

    [Benchmark(Description = "QueryBuilder: BuildQueryBuilderSource")]
    public void BuildQueryBuilderSource()
    {
        for (int i = 0; i < Iterations; i++)
        {
            _queryBuilderService.BuildQueryBuilderSource(typeof(Product));
        }
    }

    [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderAsync (User)")]
    public async Task GenerateQueryBuilderForUserAsync()
        => await _queryBuilderService.GenerateQueryBuilderAsync(typeof(User));

    [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderAsync (Order)")]
    public async Task GenerateQueryBuilderForOrderAsync()
        => await _queryBuilderService.GenerateQueryBuilderAsync(typeof(Order));

    [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderAsync (Category)")]
    public async Task GenerateQueryBuilderForCategoryAsync()
        => await _queryBuilderService.GenerateQueryBuilderAsync(typeof(Category));

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