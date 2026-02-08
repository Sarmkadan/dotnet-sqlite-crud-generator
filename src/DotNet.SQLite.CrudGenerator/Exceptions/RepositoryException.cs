#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Exception thrown when a repository operation fails.
/// </summary>
public sealed class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message) { }

    public RepositoryException(string message, Exception innerException)
        : base(message, innerException) { }

    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    /// <summary>
    /// Creates a repository exception with entity context.
    /// </summary>
    public static RepositoryException EntityNotFound(string entityType, int entityId)
    {
        return new RepositoryException($"Entity of type '{entityType}' with ID {entityId} was not found.")
        {
            EntityType = entityType,
            EntityId = entityId
        };
    }

    /// <summary>
    /// Creates a repository exception for duplicate key violations.
    /// </summary>
    public static RepositoryException DuplicateKey(string entityType, string property, object value)
    {
        return new RepositoryException($"An entity of type '{entityType}' with {property} = '{value}' already exists.")
        {
            EntityType = entityType
        };
    }

    /// <summary>
    /// Creates a repository exception for constraint violations.
    /// </summary>
    public static RepositoryException ConstraintViolation(string entityType, string constraint)
    {
        return new RepositoryException($"Constraint violation in entity '{entityType}': {constraint}")
        {
            EntityType = entityType
        };
    }
}
