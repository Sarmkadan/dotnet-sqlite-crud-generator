using System;

namespace DotNet.SQLite.CrudGenerator.Attributes
{
    /// <summary>
    /// Provides extension methods for <see cref="GenerateGrpcAttribute"/> to simplify gRPC service generation configuration.
    /// </summary>
    public static class GenerateGrpcAttributeExtensions
    {
        /// <summary>
        /// Determines if the gRPC service should include CRUD operations based on the <see cref="GenerateGrpcAttribute.GenerateCrud"/> flag.
        /// </summary>
        /// <param name="attribute">The <see cref="GenerateGrpcAttribute"/> instance. Cannot be null.</param>
        /// <returns>True if CRUD operations should be generated; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> is null.</exception>
        public static bool ShouldGenerateCrudOperations(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateCrud;
        }

        /// <summary>
        /// Determines if the gRPC service should include asynchronous methods based on the <see cref="GenerateGrpcAttribute.GenerateAsync"/> flag.
        /// </summary>
        /// <param name="attribute">The <see cref="GenerateGrpcAttribute"/> instance. Cannot be null.</param>
        /// <returns>True if asynchronous methods should be generated; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> is null.</exception>
        public static bool ShouldGenerateAsyncMethods(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateAsync;
        }

        /// <summary>
        /// Gets the effective service name, falling back to the entity type name if not specified.
        /// </summary>
        /// <param name="attribute">The <see cref="GenerateGrpcAttribute"/> instance. Cannot be null.</param>
        /// <param name="entityTypeName">The name of the entity type. Cannot be null.</param>
        /// <returns>The service name to use for the gRPC service.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> or <paramref name="entityTypeName"/> is null.</exception>
        public static string GetEffectiveServiceName(this GenerateGrpcAttribute attribute, string entityTypeName)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            ArgumentNullException.ThrowIfNull(entityTypeName);

            return attribute.ServiceName ?? entityTypeName;
        }

        /// <summary>
        /// Determines if the gRPC service should include streaming operations based on the <see cref="GenerateGrpcAttribute.GenerateStreaming"/> flag.
        /// </summary>
        /// <param name="attribute">The <see cref="GenerateGrpcAttribute"/> instance. Cannot be null.</param>
        /// <returns>True if streaming operations should be generated; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> is null.</exception>
        public static bool ShouldGenerateStreamingOperations(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateStreaming;
        }
    }
}