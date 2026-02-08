#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Caching;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for MemoryCacheProvider covering hit, miss, set, and
/// GetOrSet patterns — the dominant cache access paths at runtime.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class CacheBenchmarks
{
    private MemoryCacheProvider _cache = null!;
    private Product _product = null!;

    private const string HitKey = "product:hit";
    private const string MissKey = "product:miss";
    private const string WriteKey = "product:write";

    [GlobalSetup]
    public async Task Setup()
    {
        _cache = new MemoryCacheProvider(maxSizeBytes: 50_000_000);
        _product = new Product
        {
            Id = 1,
            Name = "Benchmark Widget",
            Sku = "BW-001",
            CategoryId = 1,
            Price = 19.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _cache.SetAsync(HitKey, _product, TimeSpan.FromMinutes(5));
    }

    [Benchmark(Description = "GetAsync — cache hit")]
    public async ValueTask<Product?> GetHit() =>
        await _cache.GetAsync<Product>(HitKey);

    [Benchmark(Description = "GetAsync — cache miss")]
    public async ValueTask<Product?> GetMiss() =>
        await _cache.GetAsync<Product>(MissKey);

    [Benchmark(Description = "SetAsync — upsert entry")]
    public async ValueTask Set() =>
        await _cache.SetAsync(WriteKey, _product, TimeSpan.FromMinutes(5));

    [Benchmark(Description = "ExistsAsync — entry present")]
    public async ValueTask<bool> ExistsHit() =>
        await _cache.ExistsAsync(HitKey);

    [Benchmark(Description = "GetOrSetAsync — hit (no factory)")]
    public async ValueTask<Product?> GetOrSetHit() =>
        await _cache.GetOrSetAsync<Product>(HitKey, () => Task.FromResult(_product));

    [Benchmark(Description = "GetOrSetAsync — miss (factory invoked)")]
    public async ValueTask<Product?> GetOrSetMiss() =>
        await _cache.GetOrSetAsync<Product>(MissKey + Guid.NewGuid(), () => Task.FromResult(_product));

    [GlobalCleanup]
    public async Task Cleanup() => await _cache.ClearAsync();
}
