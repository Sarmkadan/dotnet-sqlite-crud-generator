using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Benchmarks
{
    /// <summary>
    /// Provides extension methods for <see cref="RepositoryBenchmarks"/> to facilitate benchmarking operations.
    /// </summary>
    public static class RepositoryBenchmarksExtensions
    {
        /// <summary>
        /// Creates a batch of test entities and returns their IDs for benchmarking bulk operations.
        /// </summary>
        /// <param name="benchmarks">The repository benchmarks instance.</param>
        /// <param name="count">Number of entities to create.</param>
        /// <param name="entityType">Type of entity to create ("product" or "user").</param>
        /// <returns>Collection of created entity IDs.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entityType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="entityType"/> is not "product" or "user".</exception>
        public static async Task<IReadOnlyList<int>> CreateBatchAsync(this RepositoryBenchmarks benchmarks, int count, string entityType)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentNullException.ThrowIfNull(entityType);

            if (count <= 0)
            {
                return Array.Empty<int>();
            }

            var ids = new List<int>(count);

            switch (entityType)
            {
                case "product":
                case "Product":
                    for (int i = 0; i < count; i++)
                    {
                        var product = await benchmarks.AddProductAsync();
                        product.Name = $"Benchmark Product {i}";
                        product.Sku = $"BM-PROD-{i:D4}";
                        product.CategoryId = 1;
                        product.Price = 9.99m * (i + 1);
                        product.Cost = 7.99m * (i + 1);
                        product.StockQuantity = 100 + i * 10;
                        product.ReorderLevel = 10;
                        product.Unit = "piece";
                        product.Description = $"Test product for benchmarking operations";
                        product.IsActive = true;
                        product.CreatedAt = DateTime.UtcNow;
                        product.UpdatedAt = DateTime.UtcNow;
                        ids.Add(product.Id);
                    }
                    break;

                case "user":
                case "User":
                    for (int i = 0; i < count; i++)
                    {
                        var user = await benchmarks.AddUserAsync();
                        user.Username = $"benchmark_user_{i}";
                        user.Email = $"user{i}@benchmark.test";
                        user.PasswordHash = $"hashed_password_{i}";
                        user.FirstName = $"Benchmark";
                        user.LastName = $"User{i}";
                        user.IsActive = true;
                        user.EmailVerified = true;
                        user.CreatedAt = DateTime.UtcNow;
                        user.UpdatedAt = DateTime.UtcNow;
                        ids.Add(user.Id);
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported entity type: {entityType}", nameof(entityType));
            }

            await benchmarks.SaveChangesAsync();
            return ids.AsReadOnly();
        }

        /// <summary>
        /// Measures the time to execute a single operation against the repository.
        /// </summary>
        /// <param name="benchmarks">The repository benchmarks instance.</param>
        /// <param name="operation">The operation to measure.</param>
        /// <returns>TimeSpan representing the operation duration.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null"/>.</exception>
        public static async Task<TimeSpan> MeasureOperationAsync(this RepositoryBenchmarks benchmarks, Func<Task> operation)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentNullException.ThrowIfNull(operation);

            var start = DateTime.UtcNow;
            await operation();
            var end = DateTime.UtcNow;
            return end - start;
        }

        /// <summary>
        /// Measures the time to execute a single operation with return value against the repository.
        /// </summary>
        /// <typeparam name="T">Return type of the operation.</typeparam>
        /// <param name="benchmarks">The repository benchmarks instance.</param>
        /// <param name="operation">The operation to measure.</param>
        /// <returns>Tuple of (result, TimeSpan) representing the operation duration.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="operation"/> is <see langword="null"/>.</exception>
        public static async Task<(T Result, TimeSpan Duration)> MeasureOperationAsync<T>(this RepositoryBenchmarks benchmarks, Func<Task<T>> operation)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentNullException.ThrowIfNull(operation);

            var start = DateTime.UtcNow;
            var result = await operation();
            var end = DateTime.UtcNow;
            return (result, end - start);
        }

        /// <summary>
        /// Clears all test data from the repository and resets the database state.
        /// </summary>
        /// <param name="benchmarks">The repository benchmarks instance.</param>
        /// <returns>Task representing the async operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        public static async Task ResetDatabaseAsync(this RepositoryBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            await benchmarks.Cleanup();
            await benchmarks.SaveChangesAsync();
        }
    }
}