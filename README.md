# DotNet SQLite CRUD Generator

## ConfigurationExceptionValidation

`ConfigurationExceptionValidation` is a static utility class that provides validation helpers for configuration values used throughout the SQLite CRUD Generator. It offers methods to validate configuration names, values, connection strings, file paths, and timeout values, ensuring consistent error handling when configuration validation fails.

The class provides three main patterns for each validation scenario:
- `Validate()` methods that return a list of validation problems
- `IsValid()` methods that return a boolean indicating validity  
- `EnsureValid()` methods that throw exceptions when validation fails


Here's a realistic example demonstrating how to use `ConfigurationExceptionValidation` in a configuration setup scenario:

```csharp
using DotNet.SQLite.CrudGenerator.Exceptions;
using System;
using System.IO;

public class ConfigurationExample
{
    private const string ConnectionStringName = "DefaultConnection";
    private const string DatabaseFilePath = @"/var/data/myapp.db";
    private const int CommandTimeout = 30000;

    public static void ConfigureDatabase()
    {
        // Validate connection string name
        if (ConfigurationExceptionValidation.IsValidConnectionString(ConnectionStringName))
        {
            Console.WriteLine($"Connection string '{ConnectionStringName}' is valid");
        }
        else
        {
            var problems = ConfigurationExceptionValidation.ValidateConnectionString(ConnectionStringName);
            Console.WriteLine($"Connection string validation failed: {string.Join(", ", problems)}");
        }

        // Validate file path
        if (ConfigurationExceptionValidation.IsValidFilePath(DatabaseFilePath))
        {
            Console.WriteLine($"Database path '{DatabaseFilePath}' is valid");
        }
        else
        {
            var problems = ConfigurationExceptionValidation.ValidateFilePath(DatabaseFilePath);
            foreach (var problem in problems)
            {
                Console.WriteLine($"File path validation error: {problem}");
            }
        }

        // Validate timeout using EnsureValid pattern for immediate failure
        try
        {
            ConfigurationExceptionValidation.EnsureValid("CommandTimeout", CommandTimeout);
            Console.WriteLine($"Timeout {CommandTimeout}ms is valid");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid timeout configuration: {ex.Message}");
        }

        // Validate configuration name and value pair
        var configName = "CacheSize";
        var configValue = "1024";
        
        var validationResults = ConfigurationExceptionValidation.Validate(configName, configValue);
        if (validationResults.Count == 0)
        {
            Console.WriteLine($"Configuration '{configName}' with value '{configValue}' is valid");
        }
        else
        {
            Console.WriteLine("Configuration validation errors:");
            foreach (var error in validationResults)
            {
                Console.WriteLine($"- {error}");
            }
        }
    }

    public static void Main()
    {
        ConfigureDatabase();
    }
}
```


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

## EventBusValidation

`EventBusValidation` is a static utility class that provides validation helpers for domain events and event bus related types. It offers methods to validate `DomainEvent`, `EventEnvelope`, and `EventBusStatistics` instances, ensuring consistent error handling when event validation fails.

The class provides three main patterns for each validation scenario:
- `Validate()` methods that return a list of validation problems
- `IsValid()` methods that return a boolean indicating validity  
- `EnsureValid()` methods that throw exceptions when validation fails

Here's a realistic example demonstrating how to use `EventBusValidation` in an event publishing scenario:

```csharp
using DotNet.SQLite.CrudGenerator.Events;
using System;

public class EventBusExample
{
    public static void Main()
    {
        // Create a valid domain event
        var validEvent = new DomainEvent(
            aggregateId: Guid.NewGuid(),
            eventName: "UserCreated",
            occurredAt: DateTime.UtcNow,
            data: new { UserId = 42, Username = "john_doe" }
        );

        // Validate using IsValid() pattern
        if (validEvent.IsValid())
        {
            Console.WriteLine("Domain event is valid");
        }
        else
        {
            var problems = validEvent.Validate();
            Console.WriteLine($"Validation failed: {string.Join(", ", problems)}");
        }

        // Validate using Validate() pattern
        var envelope = new EventEnvelope(
            eventId: Guid.NewGuid(),
            eventTypeName: "UserCreated",
            timestamp: DateTime.UtcNow,
            data: validEvent
        );

        var envelopeProblems = envelope.Validate();
        if (envelopeProblems.Count > 0)
        {
            Console.WriteLine("Event envelope validation errors:");
            foreach (var problem in envelopeProblems)
            {
                Console.WriteLine($"- {problem}");
            }
        }

        // Validate using EnsureValid() pattern for immediate failure
        try
        {
            var invalidEvent = new DomainEvent(
                aggregateId: Guid.Empty, // Invalid: empty GUID
                eventName: "", // Invalid: empty name
                occurredAt: DateTime.MinValue, // Invalid: default DateTime
                data: null
            );
            
            invalidEvent.EnsureValid(); // This will throw
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed as expected: {ex.Message}");
        }

        // Create valid statistics
        var stats = new EventBusStatistics
        {
            RegisteredEventTypes = 15,
            TotalSubscriptions = 42,
            TotalEventsPublished = 1000,
            Subscriptions = new Dictionary<string, int>
            {
                { "UserCreated", 10 },
                { "OrderPlaced", 15 },
                { "PaymentProcessed", 17 }
            }
        };

        // Validate statistics
        if (stats.IsValid())
        {
            Console.WriteLine("Event bus statistics are valid");
            Console.WriteLine($"Total events published: {stats.TotalEventsPublished}");
        }
    }
}
```

## EntityChangedEventValidation

`EntityChangedEventValidation` is a static utility class that provides validation helpers for entity change events. It offers methods to validate `EntityChangedEvent<T>`, `EntityCreatedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityDeletedEvent<T>`, `BulkEntityChangedEvent<T>`, `ProductRestockedEvent`, and `ProductSoldEvent` instances, ensuring consistent error handling when event validation fails.

The class provides three main patterns for each validation scenario:
- `Validate()` methods that return a list of validation problems
- `IsValid()` methods that return a boolean indicating validity  
- `EnsureValid()` methods that throw exceptions when validation fails

Here's a realistic example demonstrating how to use `EntityChangedEventValidation` in an event handling scenario:

```csharp
using DotNet.SQLite.CrudGenerator.Events;
using System;
using System.Collections.Generic;

public class EntityEventExample
{
    public static void Main()
    {
        // Create a valid entity created event
        var product = new Product { Id = 1, Name = "Widget", Price = 19.99m };
        var createdEvent = new EntityCreatedEvent<Product>
        {
            Entity = product,
            EntityType = "Product",
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventName = "ProductCreated"
        };

        // Validate using IsValid() pattern
        if (createdEvent.IsValid())
        {
            Console.WriteLine("EntityCreatedEvent is valid");
        }
        else
        {
            var problems = createdEvent.Validate();
            Console.WriteLine($"Validation failed: {string.Join(", ", problems)}");
        }

        // Validate using Validate() pattern with detailed feedback
        var updatedEvent = new EntityUpdatedEvent<Product>
        {
            Entity = product,
            OldEntity = new Product { Id = 1, Name = "Widget", Price = 15.99m },
            EntityType = "Product",
            AggregateId = Guid.Empty, // Invalid: empty GUID
            OccurredAt = DateTime.UtcNow.AddDays(-366), // Invalid: too old
            EventName = "ProductUpdated",
            Changes = new Dictionary<string, object>
            {
                { "Price", 19.99m },
                { "Name", "Widget" }
            }
        };

        var problems = updatedEvent.Validate();
        if (problems.Count > 0)
        {
            Console.WriteLine("EntityUpdatedEvent validation errors:");
            foreach (var problem in problems)
            {
                Console.WriteLine($"- {problem}");
            }
        }

        // Validate using EnsureValid() pattern for immediate failure
        try
        {
            var invalidEvent = new EntityCreatedEvent<Product>
            {
                Entity = null, // Invalid: null entity
                EntityType = "", // Invalid: empty type
                AggregateId = Guid.Empty, // Invalid: empty GUID
                OccurredAt = DateTime.MinValue, // Invalid: default DateTime
                EventName = new string('x', 201) // Invalid: too long
            };

            invalidEvent.EnsureValid(); // This will throw
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Validation failed as expected:{Environment.NewLine}{ex.Message}");
        }

        // Create a bulk entity changed event
        var bulkEvent = new BulkEntityChangedEvent<Product>
        {
            Count = 3,
            Operation = "UpdateInventory",
            Entities = new List<Product?>
            {
                new Product { Id = 1, Name = "Widget" },
                new Product { Id = 2, Name = "Gadget" },
                new Product { Id = 3, Name = "Gizmo" }
            },
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventName = "BulkProductUpdate"
        };

        // Validate bulk event
        if (bulkEvent.IsValid())
        {
            Console.WriteLine("BulkEntityChangedEvent is valid");
            Console.WriteLine($"Processing {bulkEvent.Count} entities");
        }

        // Validate domain-specific events
        var restockedEvent = new ProductRestockedEvent
        {
            ProductId = 42,
            QuantityAdded = 100,
            NewQuantity = 150,
            AggregateId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            EventName = "InventoryRestocked"
        };

        if (restockedEvent.IsValid())
        {
            Console.WriteLine($"Product {restockedEvent.ProductId} restocked: +{restockedEvent.QuantityAdded}");
        }
    }
}

// Example entity class
public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
}
```


## OrderValidation

`OrderValidation` is a static utility class that provides comprehensive validation helpers for the `Order` entity. It offers methods to validate order state, final total calculations, and business rules for shipping and cancellation operations, ensuring consistent error handling throughout the order lifecycle.

The class provides three main patterns for each validation scenario:
- `Validate*()` methods that return a list of validation problems
- `IsValid*()` methods that return a boolean indicating validity
- `Ensure*Valid()` methods that throw exceptions when validation fails

Here's a realistic example demonstrating how to use `OrderValidation` in an order processing scenario:

```csharp
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Enums;
using System;

public class OrderProcessingExample
{
    public static void Main()
    {
        // Create a valid order
        var order = new Order
        {
            UserId = 42,
            OrderNumber = "ORD-2024-00123",
            Status = EntityStatus.Pending,
            TotalAmount = 199.99m,
            TaxAmount = 19.99m,
            DiscountAmount = 5.00m,
            ItemCount = 3,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            ShippingAddress = "123 Main St, Anytown, USA 12345",
            BillingAddress = "123 Main St, Anytown, USA 12345",
            Notes = "Express delivery"
        };

        // Validate complete order state
        if (order.IsValid())
        {
            Console.WriteLine("Order is valid and ready for processing");
        }
        else
        {
            var problems = order.ValidateAll();
            Console.WriteLine("Order validation errors:");
            foreach (var problem in problems)
            {
                Console.WriteLine($"- {problem}");
            }
        }

        // Validate final total calculation
        if (order.IsFinalTotalValid())
        {
            Console.WriteLine($"Final total is correct: {order.CalculateFinalTotal():C}");
        }
        else
        {
            var errors = order.ValidateFinalTotal();
            Console.WriteLine("Final total validation errors:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Check if order can be shipped using validation pattern
        if (order.CanShip())
        {
            Console.WriteLine("Order can be shipped");
        }
        else
        {
            var errors = order.ValidateCanShip();
            Console.WriteLine("Order cannot be shipped:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Check if order can be cancelled using validation pattern
        if (order.CanCancel())
        {
            Console.WriteLine("Order can be cancelled");
        }
        else
        {
            var errors = order.ValidateCanCancel();
            Console.WriteLine("Order cannot be cancelled:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        // Use EnsureValid pattern for immediate failure
        try
        {
            var invalidOrder = new Order
            {
                UserId = 0, // Invalid: zero user ID
                OrderNumber = "", // Invalid: empty order number
                Status = EntityStatus.Pending,
                TotalAmount = -100, // Invalid: negative amount
                TaxAmount = 0,
                DiscountAmount = 0,
                ItemCount = 0, // Invalid: zero items
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-2), // Invalid: updated before created
                ShippingAddress = "", // Invalid: empty shipping address
                BillingAddress = "", // Invalid: empty billing address
                Notes = new string('x', 600) // Invalid: too long
            };

            invalidOrder.EnsureValid(); // This will throw
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Order validation failed as expected:{Environment.NewLine}{ex.Message}");
        }

        // Use EnsureCanShip pattern for immediate shipping validation
        try
        {
            var nonPendingOrder = new Order
            {
                UserId = 1,
                OrderNumber = "ORD-123",
                Status = EntityStatus.Shipped, // Invalid: already shipped
                TotalAmount = 100,
                ItemCount = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ShippingAddress = "123 Main St"
            };

            nonPendingOrder.EnsureCanShip(); // This will throw
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Shipping validation failed as expected:{Environment.NewLine}{ex.Message}");
        }
    }
}
```## CachingIntegrationTests

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

## AuditTrailServiceTests

`AuditTrailServiceTests` is a test class that contains unit tests for the `AuditTrailService` class. It verifies that audit trail entries are correctly persisted, queried, and managed in the database. The test class demonstrates various scenarios including recording operations, querying by filters, entity-specific trails, and summary statistics.



Here's an example of using `AuditTrailServiceTests` to demonstrate audit trail functionality:


```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Tests;
using DotNet.SQLite.CrudGenerator.Enums;

public class AuditTrailExample : IDisposable
{
    private readonly AuditTrailServiceTests _test;

    public AuditTrailExample()
    {
        _test = new AuditTrailServiceTests();
    }

    public void Dispose()
    {
        _test.Dispose();
    }

    public async Task ExampleUsage()
    {
        // Record a create operation
        await _test._sut.RecordAsync("Product", 1, OperationType.Create, 42, 
            newValues: "{\"Name\":\"Widget\"}");

        // Record an update operation with before/after objects
        var before = new { Id = 1, Name = "Old Product" };
        var after = new { Id = 1, Name = "New Product" };
        await _test._sut.RecordAsync(1, OperationType.Update, 42, before, after);

        // Query all operations for a specific entity
        var productTrail = await _test._sut.GetEntityTrailAsync("Product", 1);
        Console.WriteLine($"Found {productTrail.Count} operations for Product 1");

        // Query with filter
        var createOperations = await _test._sut.QueryAsync(new AuditTrailFilter 
        {
            EntityType = "Product",
            OperationType = OperationType.Create
        });
        Console.WriteLine($"Found {createOperations.Count} create operations");

        // Get recent operations with limit
        var recent = await _test._sut.GetRecentAsync(limit: 10);
        Console.WriteLine($"Found {recent.Count} recent operations");

        // Get summary statistics
        var summary = await _test._sut.GetSummaryAsync();
        Console.WriteLine($"Total entries: {summary.TotalEntries}");
        Console.WriteLine($"Operations by type: {string.Join(", ", summary.ByOperation.Keys)}");

        // Purge old entries
        var deletedCount = await _test._sut.PurgeAsync(DateTime.UtcNow.AddDays(-30));
        Console.WriteLine($"Deleted {deletedCount} old entries");
    }
}
```

