// existing content ...

## RepositoryException

The `RepositoryException` class represents an exception that occurs during repository operations. It provides additional information about the type of repository error, the entity type, and the entity ID.

Example usage:
```csharp
try
{
    // Code that may throw a RepositoryException
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Repository error: {ex.Message}");
    Console.WriteLine($"Entity type: {ex.EntityType}");
    Console.WriteLine($"Entity ID: {ex.EntityId}");

    if (ex is RepositoryException.EntityNotFoundException)
    {
        // Handle entity not found
    }
    else if (ex is RepositoryException.DuplicateKeyException)
    {
        // Handle duplicate key
    }
}

// Creating a RepositoryException
var repositoryException = RepositoryException.EntityNotFound("Product", 123);
Console.WriteLine(repositoryException.Message); // Output: Entity of type 'Product' with ID 123 was not found.

// Creating a RepositoryException for duplicate key violations
var duplicateKeyException = RepositoryException.DuplicateKey("Product", "Name", "Test Product");
Console.WriteLine(duplicateKeyException.Message); // Output: An entity of type 'Product' with Name = 'Test Product' already exists.

// Creating a RepositoryException for constraint violations
var constraintViolationException = RepositoryException.ConstraintViolation("Product", "UniqueConstraint");
Console.WriteLine(constraintViolationException.Message); // Output: Constraint violation in entity 'Product': UniqueConstraint
```
