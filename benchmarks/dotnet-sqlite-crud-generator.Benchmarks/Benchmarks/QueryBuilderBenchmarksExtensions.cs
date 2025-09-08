#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;

namespace DotNet.SQLite.CrudGenerator.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="QueryBuilderBenchmarks"/> to measure complex query builder generation scenarios.
    /// Contains benchmarks for combined operations, repeated generation, and cleanup cycles.
    /// </summary>
    public static class QueryBuilderBenchmarksExtensions
    {
        /// <summary>
        /// Benchmarks generation of query builders for all entity types in sequence.
        /// Useful for measuring overall query builder generation performance across the entire model.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark(Description = "QueryBuilder: GenerateAllEntityQueryBuilders")]
        public static async Task GenerateAllEntityQueryBuildersAsync(this QueryBuilderBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.GenerateQueryBuilderForUserAsync();
            await benchmarks.GenerateQueryBuilderForOrderAsync();
            await benchmarks.GenerateQueryBuilderForCategoryAsync();
        }

        /// <summary>
        /// Benchmarks repeated query builder generation for a specific entity type.
        /// Useful for measuring performance under repeated generation scenarios.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="iterations">Number of times to repeat the generation. Must be positive.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterations"/> is less than 1.</exception>
        [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderRepeatedly")]
        public static async Task GenerateQueryBuilderRepeatedlyAsync(this QueryBuilderBenchmarks benchmarks, int iterations = 10)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(iterations, 0);

            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.GenerateQueryBuilderAsync();
            }
        }

        /// <summary>
        /// Benchmarks combined query builder generation and source building operations.
        /// Useful for measuring end-to-end performance of complete query builder workflow.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        [Benchmark(Description = "QueryBuilder: GenerateAndBuildQueryBuilder")]
        public static async Task GenerateAndBuildQueryBuilderAsync(this QueryBuilderBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.GenerateQueryBuilderAsync();
            benchmarks.BuildQueryBuilderSource();
        }

        /// <summary>
        /// Benchmarks query builder generation with cleanup between iterations.
        /// Useful for measuring performance with proper resource management.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="iterations">Number of cleanup cycles to execute. Must be positive.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="benchmarks"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterations"/> is less than 1.</exception>
        [Benchmark(Description = "QueryBuilder: GenerateWithCleanupCycle")]
        public static async Task GenerateWithCleanupCycleAsync(this QueryBuilderBenchmarks benchmarks, int iterations = 5)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(iterations, 0);

            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.GenerateQueryBuilderAsync();
                await benchmarks.Cleanup();
                await benchmarks.Setup();
            }
        }
    }
}