using System;
using System.Linq;

namespace DotNet.SQLite.CrudGenerator.Attributes
{
    public static class GenerateGrpcAttributeExtensions
    {
        /// <summary>
        /// Determines if the gRPC service should include CRUD operations based on the GenerateCrud flag.
        /// </summary>
        /// <param name="attribute">The GenerateGrpcAttribute instance</param>
        /// <returns>True if CRUD operations should be generated; otherwise false</returns>
        public static bool ShouldGenerateCrudOperations(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateCrud;
        }

        /// <summary>
        /// Determines if the gRPC service should include asynchronous methods based on the GenerateAsync flag.
        /// </summary>
        /// <param name="attribute">The GenerateGrpcAttribute instance</param>
        /// <returns>True if asynchronous methods should be generated; otherwise false</returns>
        public static bool ShouldGenerateAsyncMethods(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateAsync;
        }

        /// <summary>
        /// Gets the effective service name, falling back to the entity type name if not specified.
        /// </summary>
        /// <param name="attribute">The GenerateGrpcAttribute instance</param>
        /// <param name="entityTypeName">The name of the entity type</param>
        /// <returns>The service name to use for the gRPC service</returns>
        public static string GetEffectiveServiceName(this GenerateGrpcAttribute attribute, string entityTypeName)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            ArgumentNullException.ThrowIfNull(entityTypeName);

            return attribute.ServiceName ?? entityTypeName;
        }

        /// <summary>
        /// Determines if the gRPC service should include streaming operations based on the GenerateStreaming flag.
        /// </summary>
        /// <param name="attribute">The GenerateGrpcAttribute instance</param>
        /// <returns>True if streaming operations should be generated; otherwise false</returns>
        public static bool ShouldGenerateStreamingOperations(this GenerateGrpcAttribute attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            return attribute.GenerateStreaming;
        }
    }
}