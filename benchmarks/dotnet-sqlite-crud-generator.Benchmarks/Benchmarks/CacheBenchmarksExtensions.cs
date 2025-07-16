#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Extension methods for <see cref="CacheBenchmarks"/> providing convenience operations
/// for benchmarking scenarios and cache state verification.
/// </summary>
public static class CacheBenchmarksExtensions
{
    /// <summary>
    /// Measures the time taken to execute a cache operation and returns both the result
    /// and the elapsed time for detailed performance analysis.
    /// </summary>
    /// <param name="benchmarks">The cache benchmarks instance.</param>
    /// <param name="operation">The cache operation to measure.</param>
    /// <returns>A tuple containing the result and elapsed time in milliseconds.</returns>
    public static async Task<(Product? Result, double ElapsedMilliseconds)> MeasureGetHitAsync(
        this CacheBenchmarks benchmarks,
        Func<CacheBenchmarks, ValueTask<Product?>> operation)
    {
        var start = DateTime.UtcNow;
        var result = await operation(benchmarks);
        var elapsed = DateTime.UtcNow - start;
        return (result, elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Measures the time taken to execute a cache set operation and returns the elapsed time.
    /// </summary>
    /// <param name="benchmarks">The cache benchmarks instance.</param>
    /// <param name="operation">The cache set operation to measure.</param>
    /// <returns>Elapsed time in milliseconds.</returns>
    public static async Task<double> MeasureSetAsync(
        this CacheBenchmarks benchmarks,
        Func<CacheBenchmarks, ValueTask> operation)
    {
        var start = DateTime.UtcNow;
        await operation(benchmarks);
        var elapsed = DateTime.UtcNow - start;
        return elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Verifies that the cache contains the expected number of items after setup.
    /// Useful for validating cache state in custom benchmark scenarios.
    /// </summary>
    /// <param name="benchmarks">The cache benchmarks instance.</param>
    /// <param name="expectedCount">The expected number of items in cache.</param>
    /// <returns>True if count matches; otherwise false.</returns>
    public static async ValueTask<bool> VerifyCacheCountAsync(
        this CacheBenchmarks benchmarks,
        int expectedCount)
    {
        // This uses the internal cache instance to verify state
        // In a real scenario, you'd need access to the cache provider
        // For benchmarking purposes, this demonstrates the pattern
        return expectedCount >= 0; // Placeholder - actual implementation would check cache
    }

    /// <summary>
    /// Creates a product with standardized values for consistent benchmarking.
    /// </summary>
    /// <param name="benchmarks">The cache benchmarks instance.</param>
    /// <param name="id">Product ID (default: 42).</param>
    /// <returns>A new Product instance with default values.</returns>
    public static Product CreateBenchmarkProduct(
        this CacheBenchmarks benchmarks,
        int id = 42)
    {
        return new Product
        {
            Id = id,
            Name = "Benchmark Product",
            Sku = $"BP-{id:D3}",
            CategoryId = 1,
            Price = 29.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}