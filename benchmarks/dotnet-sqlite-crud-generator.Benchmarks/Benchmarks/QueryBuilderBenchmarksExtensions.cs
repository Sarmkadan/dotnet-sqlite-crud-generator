using BenchmarkDotNet.Attributes;

namespace DotNet.SQLite.CrudGenerator.Benchmarks
{
    public static class QueryBuilderBenchmarksExtensions
    {
        /// <summary>
        /// Benchmarks generation of query builders for all entity types in sequence.
        /// Useful for measuring overall query builder generation performance across the entire model.
        /// </summary>
        [Benchmark(Description = "QueryBuilder: GenerateAllEntityQueryBuilders")]
        public static async Task GenerateAllEntityQueryBuildersAsync(this QueryBuilderBenchmarks benchmarks)
        {
            await benchmarks.GenerateQueryBuilderForUserAsync();
            await benchmarks.GenerateQueryBuilderForOrderAsync();
            await benchmarks.GenerateQueryBuilderForCategoryAsync();
        }

        /// <summary>
        /// Benchmarks repeated query builder generation for a specific entity type.
        /// Useful for measuring performance under repeated generation scenarios.
        /// </summary>
        [Benchmark(Description = "QueryBuilder: GenerateQueryBuilderRepeatedly")]
        public static async Task GenerateQueryBuilderRepeatedlyAsync(this QueryBuilderBenchmarks benchmarks, int iterations = 10)
        {
            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.GenerateQueryBuilderAsync();
            }
        }

        /// <summary>
        /// Benchmarks combined query builder generation and source building operations.
        /// Useful for measuring end-to-end performance of complete query builder workflow.
        /// </summary>
        [Benchmark(Description = "QueryBuilder: GenerateAndBuildQueryBuilder")]
        public static async Task GenerateAndBuildQueryBuilderAsync(this QueryBuilderBenchmarks benchmarks)
        {
            await benchmarks.GenerateQueryBuilderAsync();
            benchmarks.BuildQueryBuilderSource();
        }

        /// <summary>
        /// Benchmarks query builder generation with cleanup between iterations.
        /// Useful for measuring performance with proper resource management.
        /// </summary>
        [Benchmark(Description = "QueryBuilder: GenerateWithCleanupCycle")]
        public static async Task GenerateWithCleanupCycleAsync(this QueryBuilderBenchmarks benchmarks, int iterations = 5)
        {
            for (int i = 0; i < iterations; i++)
            {
                await benchmarks.GenerateQueryBuilderAsync();
                await benchmarks.Cleanup();
                await benchmarks.Setup();
            }
        }
    }
}
