# DotNet SQLite CRUD Generator

This project is a .NET tool that generates a complete CRUD (Create, Read, Update, Delete) API for SQLite databases, including:

- Entity models
- Repository interfaces and implementations
- Service layer
- gRPC services
- Database migrations
- Unit tests

## Features

- Automatic code generation from database schema
- Support for multiple output formats (C# classes, SQL migrations, gRPC services)
- Customizable templates and naming conventions
- Built-in unit tests for generated code
- SQLite-specific optimizations

## Usage

See [USAGE.md](USAGE.md) for detailed instructions.

## Test Documentation


### MigrationDiffServiceTests

`MigrationDiffServiceTests` is a test class that contains unit tests for `MigrationDiffService`. It provides various test methods to verify the correctness of database schema generation, diff computation, and actual schema retrieval against an in-memory SQLite database.

Here's an example of using `MigrationDiffServiceTests` in a test class:

```csharp
using DotNet.SQLite.CrudGenerator.Tests;

public class MigrationDiffServiceTestsExample : IDisposable
{
    private readonly MigrationDiffServiceTests _test;

    public MigrationDiffServiceTestsExample()
    {
        _test = new MigrationDiffServiceTests();
    }

    public void Dispose()
    {
        _test.Dispose();
    }

    [Fact]
    public void TestGetExpectedSchema_ReturnsCorrectColumnTypes()
    {
        // Arrange
        // Act
        var schema = _test.GetExpectedSchema(typeof(_test.SimpleEntity));

        // Assert
        // assertions...
    }
}
```

## UserServiceTests

`UserServiceTests` is a test class that verifies the behavior of **UserService** by using a mocked `IRepository<User, int>`. It contains async test methods that cover the most common service operations: retrieving a user by id (both existing and non‑existent), fetching all users, creating a new user, updating an existing user, and deleting a user.

```csharp
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Interfaces;
using NSubstitute;

public class UserServiceTestsExample
{
    public static async Task Main()
    {
        var tests = new UserServiceTests();

        await tests.GetAsync_WithValidId_ReturnsUserFromRepository();
        await tests.GetAsync_WithNonExistentId_ReturnsNull();
        await tests.GetAllAsync_ReturnsAllUsersFromRepository();
        await tests.CreateAsync_AddsUserThroughRepository();
        await tests.UpdateAsync_UpdatesUserThroughRepository();
        await tests.DeleteAsync_DeletesUserThroughRepository();
    }
}
```

## GenerationServiceTests

`GenerationServiceTests` is a test class that contains unit tests for the `GenerationService`.
It demonstrates how to instantiate the test class, run a test method, and clean up resources. The example below shows a simple usage pattern that compiles and runs the test methods.

```csharp
using DotNet.SQLite.CrudGenerator.Tests;
using System.Threading.Tasks;

public class GenerationServiceTestsExample
{
    public static async Task Main()
    {
        var tests = new GenerationServiceTests();
        await tests.GenerateRepositoryInterfaceAsync_GeneratesCorrectInterfaceFile();
        await tests.GenerateMigrationAsync_GeneratesCorrectSqlMigrationFile();
        await tests.GenerateGrpcServiceAsync_GeneratesCorrectProtoFile();
        tests.Dispose();
    }
}
```

## CachingIntegrationTests

`CachingIntegrationTests` is a test class that contains integration tests for the `MemoryCacheProvider` caching implementation. It verifies that the cache provider correctly stores, retrieves, expires, and manages items according to the configured eviction policy and TTL settings.

Here's an example of using `CachingIntegrationTests` to demonstrate the cache provider's functionality:

```csharp
using DotNet.SQLite.CrudGenerator.Caching;
using DotNet.SQLite.CrudGenerator.Configuration;
using System;
using System.Threading.Tasks;

public class CacheProviderExample : IDisposable
{
    private MemoryCacheProvider _cacheProvider;
    private CacheConfiguration _cacheConfiguration;

    public CacheProviderExample()
    {
        _cacheConfiguration = new CacheConfiguration
        {
            Enabled = true,
            MaxSizeBytes = 1024 * 10, // 10 KB for testing
            DefaultTTL = TimeSpan.FromMinutes(1)
        };
        _cacheProvider = new MemoryCacheProvider(_cacheConfiguration.MaxSizeBytes);
    }

    public void Dispose()
    {
        _cacheProvider.ClearAsync().GetAwaiter().GetResult();
    }

    public async Task ExampleUsage()
    {
        // Set a value in cache
        await _cacheProvider.SetAsync("user:123", "John Doe");
        
        // Check if value exists
        var exists = await _cacheProvider.ExistsAsync("user:123");
        Console.WriteLine($"User exists: {exists}"); // Output: User exists: True
        
        // Get value from cache
        var userName = await _cacheProvider.GetAsync<string>("user:123");
        Console.WriteLine($"User name: {userName}"); // Output: User name: John Doe
        
        // Get or set with factory pattern
        var productName = await _cacheProvider.GetOrSetAsync(
            "product:456",
            async () => await FetchProductFromDatabaseAsync(456)
        );
        
        // Remove item from cache
        await _cacheProvider.RemoveAsync("user:123");
        
        // Clear entire cache
        await _cacheProvider.ClearAsync();
        
        // Get cache statistics
        var stats = _cacheProvider.GetStatistics();
        Console.WriteLine($"Total items: {stats.TotalItems}");
        Console.WriteLine($"Total size: {stats.TotalSizeBytes} bytes");
    }

    private async Task<string> FetchProductFromDatabaseAsync(int id)
    {
        // Simulate database fetch
        await Task.Delay(100);
        return $"Product {id}";
    }
}
```

## StringExtensionsTests

`StringExtensionsTests` is a test class that contains unit tests for the `StringExtensions` utility class, verifying various string manipulation and formatting operations such as PascalCase conversion, camelCase conversion, pluralization, truncation, slug generation, and snake_case conversion.

Here's an example demonstrating how to use the `StringExtensions` methods based on the actual test cases:


```csharp
using DotNet.SQLite.CrudGenerator.Utilities;

public class StringExtensionsExample
{
    public static void Main()
    {
        // PascalCase conversion
        var pascalResult = "user_profile_id".ToPascalCase();
        Console.WriteLine(pascalResult); // Output: UserProfileId
        
        // camelCase conversion
        var camelResult = "first_name".ToCamelCase();
        Console.WriteLine(camelResult); // Output: firstName
        
        // Pluralization
        var pluralResult = "category".Pluralize();
        Console.WriteLine(pluralResult); // Output: categories
        
        // Truncation with ellipsis
        var truncatedResult = "This is a long product description".Truncate(20, addEllipsis: true);
        Console.WriteLine(truncatedResult); // Output: This is a long produ...
        
        // Slug generation
        var slugResult = "My Product Name 2024".ToSlug();
        Console.WriteLine(slugResult); // Output: my-product-name-2024
        
        // snake_case conversion
        var snakeResult = "StockQuantity".ToSnakeCase();
        Console.WriteLine(snakeResult); // Output: stock_quantity
        
        // Null/empty handling
        var nullResult = ((string?)null).ToPascalCase();
        Console.WriteLine(nullResult); // Output: (null)
        
        var emptyResult = "".ToSnakeCase();
        Console.WriteLine(emptyResult); // Output: (empty string)
    }
}
```