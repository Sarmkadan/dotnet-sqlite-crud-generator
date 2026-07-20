using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="GenerationService"/> to simplify code generation operations.
    /// </summary>
    public static class GenerationServiceExtensions
    {
        /// <summary>
        /// Generates the repository interface code for the specified entity type using a generic type parameter.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The generation service instance.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
        /// <returns>A task representing the asynchronous operation with the generated code file path.</returns>
        public static Task<string> GenerateRepositoryInterfaceAsync<T>(
            this GenerationService service,
            CancellationToken cancellationToken = default)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(service);
            return service.GenerateRepositoryInterfaceAsync(typeof(T), cancellationToken);
        }

        /// <summary>
        /// Generates the gRPC service code for the specified entity type using a generic type parameter.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The generation service instance.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
        /// <returns>A task representing the asynchronous operation with the generated code file path.</returns>
        public static Task<string> GenerateGrpcServiceAsync<T>(
            this GenerationService service,
            CancellationToken cancellationToken = default)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(service);
            return service.GenerateGrpcServiceAsync(typeof(T), cancellationToken);
        }

        /// <summary>
        /// Generates the migration code for the specified entity type using a generic type parameter.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The generation service instance.</param>
        /// <param name="migrationName">The name of the migration.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="migrationName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <returns>A task representing the asynchronous operation with the generated code file path.</returns>
        public static Task<string> GenerateMigrationAsync<T>(
            this GenerationService service,
            string migrationName,
            CancellationToken cancellationToken = default)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentException.ThrowIfNullOrWhiteSpace(migrationName);
            return service.GenerateMigrationAsync(typeof(T), migrationName, cancellationToken);
        }

        /// <summary>
        /// Generates repository interface code for a collection of entity types.
        /// </summary>
        /// <param name="service">The generation service instance.</param>
        /// <param name="entityTypes">The collection of entity types to generate interfaces for.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/> or <paramref name="entityTypes"/> is <see langword="null"/>.</exception>
        /// <returns>A dictionary mapping entity types to their generated code file paths.</returns>
        public static async Task<Dictionary<Type, string>> GenerateRepositoryInterfacesAsync(
            this GenerationService service,
            IEnumerable<Type> entityTypes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(entityTypes);

            var results = new Dictionary<Type, string>();
            foreach (var entityType in entityTypes)
            {
                var code = await service.GenerateRepositoryInterfaceAsync(entityType, cancellationToken)
                    .ConfigureAwait(false);
                results[entityType] = code;
            }

            return results;
        }
    }
}
