// existing content ...

## LoggingMiddleware

`LoggingMiddleware` is a middleware component that logs operation execution time, results, and any exceptions that occur during request processing. It provides detailed insights into application performance and operation flow by tracking:

- Request entry and exit
- Execution time measurements
- Success/failure outcomes
- Detailed request/response data (when enabled)
- Exception details and stack traces (when enabled)

The middleware can be configured to log detailed information including request payloads and stack traces for debugging purposes.

Below is a realistic example of using `LoggingMiddleware` in a request pipeline:

```csharp
// Configure services
services.AddSingleton<LoggingMiddleware>(new LoggingMiddleware(enableDetailedLogging: true));

// Use middleware in pipeline
app.Use(async (context, next) =>
{
    var middleware = context.RequestServices.GetRequiredService<LoggingMiddleware>();
    var result = await middleware.ExecuteAsync<MyRequest, MyResponse>(
        request,
        async req => 
        {
            // Your actual request handling logic here
            return new MiddlewareResult
            {
                Success = true,
                Message = "Request processed successfully",
                Data = responseData
            };
        }
    );
    
    if (!result.Success)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result.Message ?? "Request failed");
    }
});
```

## GenerateGrpcAttribute

`GenerateGrpcAttribute` is an attribute used to mark a class for gRPC service generation. It allows you to customize the generation process by specifying whether to generate async methods, CRUD operations, and streaming methods. 

Below is a realistic example of using `GenerateGrpcAttribute`:

```csharp
[GenerateGrpc(ServiceName = "MyService", GenerateAsync = true, GenerateCrud = true, GenerateStreaming = true, Namespace = "MyNamespace")]
public class MyService
{
    // Service implementation
}
```

## RateLimitingMiddleware

`RateLimitingMiddleware` is a middleware component that implements rate limiting using a sliding window algorithm to track request counts per client. It prevents abuse by limiting requests to a configured threshold per time window, providing protection against excessive API usage.

The middleware tracks request counts per client identity and automatically cleans up expired requests. It can be configured with custom request limits and time windows.

Below is a realistic example of using `RateLimitingMiddleware` in a request pipeline:

```csharp
// Configure middleware with 50 requests per 30-second window
services.AddSingleton<RateLimitingMiddleware>(new RateLimitingMiddleware(requestsPerWindow: 50, windowSeconds: 30));

// Use middleware in pipeline
app.Use(async (context, next) =>
{
    var middleware = context.RequestServices.GetRequiredService<RateLimitingMiddleware>();
    var result = await middleware.ExecuteAsync<MyRequest, MyResponse>(
        request,
        async req =>
        {
            // Your actual request handling logic here
            return new MiddlewareResult
            {
                Success = true,
                Message = "Request processed successfully",
                Data = responseData
            };
        }
    );

    if (!result.Success)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        await context.Response.WriteAsync(result.Message ?? "Rate limit exceeded");
    }
});

// Monitor rate limiting statistics
var stats = middleware.GetStatistics();
Console.WriteLine($"Total clients tracked: {stats.TotalClients}");
foreach (var client in stats.ClientLimits)
{
    Console.WriteLine($"Client {client.Key}: {client.Value.RequestCount} requests, " +
                     $"Allowed: {client.Value.IsAllowed}, " +
                     $"Resets at: {client.Value.ResetTime}");
}

// Reset all rate limits (e.g., during maintenance)
middleware.ResetLimits();
```

## ValidateCommand

`ValidateCommand` validates model definitions and database schema, checking naming conventions, required attributes, and constructor presence. It reports warnings or errors and can run in a strict mode where warnings are treated as errors.

Typical usage creates an instance of the command and invokes `ExecuteAsync`. After execution you can work with `ValidationResult` objects that expose details such as the model name, property name, message, and severity. The custom `RequiredAttribute` can be applied to model properties to indicate mandatory reference‑type fields.

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        // Create the command
        var validateCommand = new ValidateCommand();

        // Run validation (no command‑line arguments in this example)
        int exitCode = await validateCommand.ExecuteAsync(Array.Empty<string>());
        Console.WriteLine($"Validation finished with exit code {exitCode}");

        // Example of inspecting a validation result
        var result = new ValidationResult
        {
            ModelName   = "User",
            PropertyName = "email",
            Message      = "Property name should start with uppercase letter",
            Severity     = ValidationSeverity.Warning
        };

        // Demonstrate the custom RequiredAttribute
        var required = new RequiredAttribute();

        Console.WriteLine(
            $"Result: {result.ModelName}.{result.PropertyName} - {result.Message} ({result.Severity})");
    }
}
```

## AuditTrailFilter

`AuditTrailFilter` provides a flexible way to query audit trail entries with support for filtering by entity type, entity ID, user ID, operation type, and date ranges. It supports pagination through the `Limit` property and can be used with the `AuditTrailService` to retrieve filtered audit logs.

## ColumnInfo

`ColumnInfo` is an immutable record that represents a column in a database table, capturing its name, SQLite data type, nullability constraint, and whether it serves as a primary key. It is used throughout the migration system to compare model schemas with actual database schemas.

Below is a realistic example of creating and using `ColumnInfo` instances when working with the migration diff service:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Data;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection and migration service
        var database = new DatabaseConnection("inventory.db");
        var migrationService = new MigrationDiffService(database);

        // Create ColumnInfo instances representing expected columns
        var idColumn = new ColumnInfo(
            Name: "Id",
            SqliteType: "INTEGER",
            NotNull: true,
            IsPrimaryKey: true
        );

        var nameColumn = new ColumnInfo(
            Name: "Name",
            SqliteType: "TEXT",
            NotNull: true,
            IsPrimaryKey: false
        );

        var priceColumn = new ColumnInfo(
            Name: "Price",
            SqliteType: "REAL",
            NotNull: false,
            IsPrimaryKey: false
        );

        // Store expected schema in a dictionary
        var expectedSchema = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", idColumn },
            { "Name", nameColumn },
            { "Price", priceColumn }
        };

        // Get actual schema from database
        var actualSchema = await migrationService.GetActualSchemaAsync("Products");

        // Compare schemas or build migration scripts...
        Console.WriteLine("Expected columns:");
        foreach (var col in expectedSchema.Values)
        {
            Console.WriteLine($"  {col.Name} ({col.SqliteType}) {(col.NotNull ? "NOT NULL" : "NULL")} {(col.IsPrimaryKey ? "[PK]" : "")}");
        }
    }
}
```

Below is a realistic example of using `AuditTrailFilter` with `AuditTrailService`:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Enums;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection and service
        var database = new DatabaseConnection("audit.db");
        var auditService = new AuditTrailService(database);

        // Create a filter for audit entries from the last 7 days
        var filter = new AuditTrailFilter
        {
            EntityType = "Product",
            OperationType = OperationType.Update,
            From = DateTime.UtcNow.AddDays(-7),
            To = DateTime.UtcNow,
            Limit = 200
        };

        // Query audit trail
        var auditEntries = await auditService.QueryAsync(filter);
        
        Console.WriteLine($"Found {auditEntries.Count} audit entries for Product updates in the last 7 days:");
        foreach (var entry in auditEntries)
        {
            Console.WriteLine($"  {entry.Timestamp:yyyy-MM-dd HH:mm:ss} - {entry.OperationType}: " +
                           $"User {entry.ChangedByUserId} changed Product {entry.EntityId}");
        }

        // Get summary statistics
        var summary = await auditService.GetSummaryAsync();
        Console.WriteLine($"\nTotal audit entries: {summary.TotalEntries}");
        Console.WriteLine($"Operations: {string.Join(", ", summary.ByOperation.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    }
}
```

// ... rest of README content ...

## UserService

`UserService` is a service class that provides comprehensive user management functionality including CRUD operations, authentication, password management, user status control, and activity monitoring. It implements validation, logging, and business logic for user-related operations while delegating data persistence to the underlying repository.

The service handles user creation with duplicate email detection, password authentication, account deactivation/reactivation, email verification, and provides summary statistics about user activity. All operations are logged and validated according to business rules.

Below is a realistic example of using `UserService` in an application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection and repository
        var database = new DatabaseConnection("users.db");
        var userRepository = new UserRepository(database);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<UserService>();
        
        // Create UserService instance
        var userService = new UserService(userRepository, logger);
        
        // Create a new user
        var newUser = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hashed_password_123",
            IsActive = true,
            EmailVerified = false
        };
        
        var createdUser = await userService.CreateAsync(newUser);
        Console.WriteLine($"Created user with ID: {createdUser.Id}");
        
        // Authenticate user
        var authenticatedUser = await userService.AuthenticateAsync(
            "john.doe@example.com", 
            "hashed_password_123"
        );
        
        if (authenticatedUser != null)
        {
            Console.WriteLine($"Authenticated user: {authenticatedUser.Email}");
            
            // Update user
            authenticatedUser.LastName = "Smith";
            await userService.UpdateAsync(authenticatedUser);
            
            // Verify email
            await userService.VerifyEmailAsync(createdUser.Id);
            
            // Get user activity summary
            var summary = await userService.GetActivitySummaryAsync();
            Console.WriteLine($"Total users: {summary.TotalUsers}, Active: {summary.ActiveUsers}, Verified: {summary.VerifiedEmails}");
            
            // Deactivate user
            await userService.DeactivateUserAsync(createdUser.Id);
            
            // Reset password
            await userService.ResetPasswordAsync(createdUser.Id, "new_hashed_password_456");
            
            // Check if user exists
            var exists = await userService.ExistsAsync(createdUser.Id);
            Console.WriteLine($"User exists: {exists}");
            
            // Get all users
            var allUsers = await userService.GetAllAsync();
            Console.WriteLine($"Total users in system: {allUsers.Count()}");
            
            // Delete user
            await userService.DeleteAsync(createdUser.Id);
            Console.WriteLine("User deleted successfully");
        }
    }
}
```

## DatabaseConnection

`DatabaseConnection` is a lightweight wrapper around a SQLite database connection that provides connection management, initialization, and basic unit-of-work pattern support. It handles connection pooling, database schema initialization with required tables, and provides both synchronous and asynchronous disposal patterns.

The connection automatically manages the underlying `SqliteConnection` instance and provides convenience methods for opening, closing, and initializing the database. It's designed to be used as a dependency in repositories and services throughout the application.

Below is a realistic example of using `DatabaseConnection` in an application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a database connection
        var database = new DatabaseConnection("inventory.db");
        
        // Optionally inject a logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DatabaseConnection>();
        var databaseWithLogging = new DatabaseConnection("inventory.db", logger);
        
        // Open the connection
        await database.OpenAsync();
        Console.WriteLine("Database connection opened");
        
        // Initialize the database with required tables
        await database.InitializeDatabaseAsync();
        Console.WriteLine("Database initialized with tables");
        
        // Use the connection with repositories
        var productRepository = new ProductRepository(database);
        var categoryRepository = new CategoryRepository(database);
        
        // Save changes (returns 0 as changes are committed immediately)
        var changes = await database.SaveChangesAsync();
        Console.WriteLine("Changes saved");
        
        // Close the connection when done
        database.Close();
        
        // Dispose when finished (automatically closes connection)
        database.Dispose();
        
        // Or use async disposal
        await database.DisposeAsync();
    }
}
```

## ProductService

`ProductService` is a service class that provides comprehensive product management functionality with inventory tracking, stock management, and pricing operations. It implements business logic for product lifecycle management while delegating data persistence to the underlying repository.

The service handles product creation with validation and duplicate SKU detection, inventory restocking and sales operations, category-based product queries, low stock monitoring, and inventory valuation. All operations are validated according to business rules and maintain audit trails through the repository layer.

Below is a realistic example of using `ProductService` in an application:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection and repositories
        var database = new DatabaseConnection("inventory.db");
        var productRepository = new ProductRepository(database);
        var categoryRepository = new CategoryRepository(database);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ProductService>();

        // Create ProductService instance
        var productService = new ProductService(productRepository, categoryRepository);

        // Create a new product
        var newProduct = new Product
        {
            Name = "Premium Wireless Headphones",
            Description = "Noise-cancelling wireless headphones with 30-hour battery",
            Sku = "AUD-PWH-001",
            Price = 199.99m,
            Cost = 120.50m,
            StockQuantity = 50,
            CategoryId = 1,
            MinimumStockLevel = 10
        };

        var createdProduct = await productService.CreateAsync(newProduct);
        Console.WriteLine($"Created product with ID: {createdProduct.Id}, SKU: {createdProduct.Sku}");

        // Get a product by ID
        var fetchedProduct = await productService.GetAsync(createdProduct.Id);
        Console.WriteLine($"Fetched product: {fetchedProduct?.Name} - ${fetchedProduct?.Price}");

        // Get all products
        var allProducts = await productService.GetAllAsync();
        Console.WriteLine($"Total products in system: {allProducts.Count()}");

        // Update a product
        fetchedProduct!.Price = 189.99m;
        await productService.UpdateAsync(fetchedProduct);
        Console.WriteLine("Product price updated");

        // Restock inventory
        var restockedProduct = await productService.RestockProductAsync(createdProduct.Id, 25);
        Console.WriteLine($"Restocked. New stock level: {restockedProduct.StockQuantity}");

        // Sell inventory
        var soldProduct = await productService.SellProductAsync(createdProduct.Id, 5);
        Console.WriteLine($"Sold 5 units. Remaining stock: {soldProduct.StockQuantity}");

        // Check if product exists
        var exists = await productService.ExistsAsync(createdProduct.Id);
        Console.WriteLine($"Product exists: {exists}");

        // Get products by category
        var categoryProducts = await productService.GetByCategoryAsync(1);
        Console.WriteLine($"Products in category 1: {categoryProducts.Count()}");

        // Get low stock products
        var lowStockProducts = await productService.GetLowStockProductsAsync();
        Console.WriteLine($"Low stock products: {lowStockProducts.Count()}");

        // Calculate inventory value
        var inventoryValue = await productService.GetInventoryValueAsync();
        Console.WriteLine($"Total inventory value: ${inventoryValue}");

        // Get inventory statistics
        var stats = await productService.GetInventoryStatsAsync();
        Console.WriteLine($"Inventory Stats:");
        Console.WriteLine($" Total Products: {stats.TotalProducts}");
        Console.WriteLine($" Total Units: {stats.TotalUnitsInStock}");
        Console.WriteLine($" Total Value: ${stats.TotalInventoryValue}");
        Console.WriteLine($" Low Stock Items: {stats.LowStockCount}");
        Console.WriteLine($" Average Stock: {stats.AverageStockLevel}");

        // Validate a product
        var isValid = productService.Validate(createdProduct);
        Console.WriteLine($"Product validation: {isValid}");

        // Delete a product
        await productService.DeleteAsync(createdProduct.Id);
        Console.WriteLine("Product deleted successfully");
    }
}
```

## DbContextProvider

`DbContextProvider` is a unit of work implementation that manages multiple repositories and database transactions in a coordinated manner. It serves as the central access point for all repositories (Users, Products, Orders, Categories, AuditLogs) and provides transaction management capabilities including begin, commit, and rollback operations. The provider ensures that changes across multiple repositories are committed atomically and provides both synchronous and asynchronous disposal patterns.

The `DbContextProvider` implements `IUnitOfWork` and coordinates database operations across all registered repositories, making it ideal for scenarios requiring transactional integrity across multiple entity types.

Below is a realistic example of using `DbContextProvider` in an application:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Enums;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection
        var database = new DatabaseConnection("ecommerce.db");
        await database.OpenAsync();
        await database.InitializeDatabaseAsync();

        // Create DbContextProvider instance
        var dbContext = new DbContextProvider(database);

        try
        {
            // Begin a transaction
            await dbContext.BeginTransactionAsync();
            Console.WriteLine("Transaction started");

            // Access repositories through the provider
            var userRepository = dbContext.Users;
            var productRepository = dbContext.Products;
            var orderRepository = dbContext.Orders;
            var categoryRepository = dbContext.Categories;

            // Create a new user
            var newUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Username = "johndoe",
                PasswordHash = "hashed_password_123",
                IsActive = true,
                EmailVerified = true
            };
            var createdUser = await userRepository.AddAsync(newUser);
            Console.WriteLine($"Created user with ID: {createdUser.Id}");

            // Create a new category
            var newCategory = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                IsActive = true,
                ParentCategoryId = null
            };
            var createdCategory = await categoryRepository.AddAsync(newCategory);
            Console.WriteLine($"Created category with ID: {createdCategory.Id}");

            // Create a new product
            var newProduct = new Product
            {
                Name = "Wireless Headphones",
                Description = "Noise-cancelling wireless headphones",
                Sku = "AUD-WH-001",
                Price = 149.99m,
                Cost = 95.50m,
                StockQuantity = 50,
                CategoryId = createdCategory.Id,
                MinimumStockLevel = 10,
                IsActive = true
            };
            var createdProduct = await productRepository.AddAsync(newProduct);
            Console.WriteLine($"Created product with ID: {createdProduct.Id}, SKU: {createdProduct.Sku}");

            // Create a new order
            var newOrder = new Order
            {
                UserId = createdUser.Id,
                OrderDate = DateTime.UtcNow,
                Status = EntityStatus.Pending,
                ShippingAddress = "123 Main St, City, Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = createdProduct.Id, Quantity = 2, UnitPrice = 149.99m }
                },
                Subtotal = 299.98m,
                TaxAmount = 23.99m,
                DiscountAmount = 0.00m,
                ShippingCost = 10.00m
            };
            var createdOrder = await orderRepository.AddAsync(newOrder);
            Console.WriteLine($"Created order with ID: {createdOrder.Id}, Status: {createdOrder.Status}");

            // Save changes across all repositories
            int changes = await dbContext.SaveChangesAsync();
            Console.WriteLine($"Saved {changes} changes to database");

            // Commit the transaction
            await dbContext.CommitTransactionAsync();
            Console.WriteLine("Transaction committed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            
            // Rollback the transaction on error
            await dbContext.RollbackTransactionAsync();
            Console.WriteLine("Transaction rolled back due to error");
            
            throw;
        }
        finally
        {
            // Dispose the provider (automatically disposes the database connection)
            dbContext.Dispose();
            
            // Or use async disposal
            // await dbContext.DisposeAsync();
        }
    }
}
```

## DatabaseSettings

`DatabaseSettings` is a configuration class that defines all database-related settings for the SQLite CRUD Generator library. It controls database file location, connection behavior, logging preferences, and automatic database creation. The settings can be configured either through a file path or a custom connection string, with sensible defaults provided for quick setup.

Below is a realistic example of configuring and using `DatabaseSettings` in an application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Configuration;

class Program
{
    static void Main(string[] args)
    {
        // Create database settings with default values
        var settings = new DatabaseSettings
        {
            FilePath = "myapp.db",
            EnableLogging = true,
            ConnectionTimeout = 60,
            AutoCreateDatabase = true
        };

        Console.WriteLine("Database Configuration:");
        Console.WriteLine($" File Path: {settings.FilePath}");
        Console.WriteLine($" Connection Timeout: {settings.ConnectionTimeout} seconds");
        Console.WriteLine($" Enable Logging: {settings.EnableLogging}");
        Console.WriteLine($" Auto Create Database: {settings.AutoCreateDatabase}");
        Console.WriteLine($" Validate: {settings.Validate()}");

        // Use custom connection string instead of file path
        settings.ConnectionString = "Data Source=custom.db;Version=3;Pooling=True;";
        Console.WriteLine($"\nCustom Connection String: {settings.ConnectionString}");
    }
}
```

## DotnetSqliteCrudGeneratorOptions

`DotnetSqliteCrudGeneratorOptions` is the root configuration class for the DotNet SQLite CRUD Generator application. It consolidates all configuration options into a single class that provides centralized access to application settings through the IOptions pattern. This class serves as the main entry point for configuring the entire CRUD generator system.

The options class includes configuration for database settings, connection pooling, caching, event bus, HTTP client, webhooks, and background workers. All properties are optional with sensible defaults to ensure the application runs without requiring extensive configuration in development environments.

Below is a realistic example of configuring and using `DotnetSqliteCrudGeneratorOptions` in a .NET application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
  static void Main(string[] args)
  {
    // Create options with custom configuration
    var options = new DotnetSqliteCrudGeneratorOptions
    {
      Database = new DatabaseSettings
      {
        FilePath = "production.db",
        ConnectionTimeout = 60,
        AutoCreateDatabase = true,
        EnableLogging = true
      },
      ConnectionPool = new ConnectionPoolConfiguration
      {
        MinPoolSize = 2,
        MaxPoolSize = 20,
        IdleTimeout = TimeSpan.FromMinutes(10),
        AcquireTimeout = TimeSpan.FromSeconds(45),
        CleanupInterval = TimeSpan.FromMinutes(2),
        EnableDiagnostics = true
      },
      Cache = new CacheConfiguration
      {
        Enabled = true,
        MaxSizeBytes = 50_000_000, // 50 MB
        DefaultTTL = TimeSpan.FromHours(1),
        CleanupIntervalSeconds = 600
      },
      EventBus = new EventBusConfiguration
      {
        Enabled = true,
        MaxEventHistory = 5000,
        PersistEvents = true
      },
      HttpClient = new HttpClientConfiguration
      {
        ConnectionLimit = 20,
        DefaultTimeout = TimeSpan.FromSeconds(60),
        MaxRetries = 5,
        RetryDelayMs = 2000
      },
      Webhook = new WebhookConfiguration
      {
        Enabled = true,
        MaxRetries = 5,
        RetryDelayMs = 10000,
        MaxDeliveryHistorySize = 5000
      },
      BackgroundWorker = new BackgroundWorkerConfiguration
      {
        Enabled = true,
        WorkerCount = 4,
        MaxQueueSize = 2000,
        TaskTimeoutSeconds = 600,
        MaxRetries = 5
      }
    };

    // Validate configuration
    options.Validate();
    Console.WriteLine("Configuration validated successfully!");

    // Use default configuration if needed
    var defaultOptions = DotnetSqliteCrudGeneratorOptions.CreateDefault();
    Console.WriteLine($"\nDefault database file: {defaultOptions.Database.FilePath}");
    Console.WriteLine($"Default connection pool size: {defaultOptions.ConnectionPool.MinPoolSize}-{defaultOptions.ConnectionPool.MaxPoolSize}");
    Console.WriteLine($"Default cache enabled: {defaultOptions.Cache.Enabled}");

    // Example of using in DI container
    var services = new ServiceCollection();
    services.AddSingleton(options);
    services.AddSingleton(_ => DotnetSqliteCrudGeneratorOptions.CreateDefault());
  }
}
```

## DependencyInjection

`DependencyInjection` is a static configuration class that provides extension methods for registering all application services and repositories with the Microsoft.Extensions.DependencyInjection container. It supports multiple configuration approaches including direct connection strings, `DatabaseSettings`, `DotnetSqliteCrudGeneratorOptions`, `IConfiguration`, and `IOptions<T>` patterns, making it suitable for various application types from console apps to ASP.NET Core web applications.

The class registers:
- Database connection and connection pooling
- Unit of work pattern implementation (`DbContextProvider`)
- All repository implementations (User, Product, Order, Category, AuditLog)
- All service classes (UserService, ProductService, OrderService, GenerationService, etc.)
- Logging infrastructure

Below is a realistic example of using `DependencyInjection` in a console application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure services using connection string
        var services = new ServiceCollection();
        services.AddApplicationServices("Data Source=app.db;Version=3;");
        
        // Or configure services using DatabaseSettings
        var settings = new DatabaseSettings
        {
            FilePath = "production.db",
            ConnectionTimeout = 60,
            AutoCreateDatabase = true,
            EnableLogging = true
        };
        services.AddApplicationServices(settings);
        
        // Or configure services using IConfiguration (from appsettings.json)
        // services.AddApplicationServices(builder.Configuration);
        
        // Or configure services using IOptions pattern
        // services.AddApplicationServices(options => 
        // {
        //     options.Database.FilePath = "options.db";
        //     options.Database.ConnectionTimeout = 30;
        // });
        
        // Build service provider
        var serviceProvider = services.BuildServiceProvider();
        
        // Initialize database with required tables
        await serviceProvider.InitializeDatabaseAsync();
        
        // Resolve services from DI container
        var userService = serviceProvider.GetRequiredService<UserService>();
        var productService = serviceProvider.GetRequiredService<ProductService>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        
        logger.LogInformation("Services registered and database initialized successfully!");
        
        // Use services...
    }
}
```

## CacheConfiguration

`CacheConfiguration` is a configuration class that defines all caching-related settings for the DotNet SQLite CRUD Generator library. It controls whether caching is enabled, cache size limits, time-to-live for cached items, and cleanup behavior to maintain optimal cache performance.

The configuration supports multiple cache providers including in-memory caching and can be used to optimize application performance by reducing database round-trips for frequently accessed data.

Below is a realistic example of configuring and using `CacheConfiguration` in an application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Configuration;

class Program
{
    static void Main(string[] args)
    {
        // Create cache configuration with custom settings
        var cacheConfig = new CacheConfiguration
        {
            Enabled = true,
            MaxSizeBytes = 100_000_000, // 100 MB
            DefaultTTL = TimeSpan.FromHours(2),
            CleanupIntervalSeconds = 300 // 5 minutes
        };

        Console.WriteLine("Cache Configuration:");
        Console.WriteLine($" Enabled: {cacheConfig.Enabled}");
        Console.WriteLine($" Max Size: {cacheConfig.MaxSizeBytes / 1024 / 1024} MB");
        Console.WriteLine($" Default TTL: {cacheConfig.DefaultTTL.TotalHours} hours");
        Console.WriteLine($" Cleanup Interval: {cacheConfig.CleanupIntervalSeconds / 60} minutes");

        // Create a memory cache provider from the configuration
        var provider = CacheConfiguration.CreateProvider(cacheConfig);
        Console.WriteLine($"\nMemory cache provider created: {provider != null}");
    }
}
```

## MemoryCacheProvider

`MemoryCacheProvider` is an in-memory caching implementation that provides fast, thread-safe access to cached data with configurable size limits and automatic expiration. It supports both synchronous and asynchronous operations, comprehensive cache statistics, and provides methods for common caching patterns like GetOrSet. The cache automatically tracks metadata such as creation time, last access time, size, and access counts for each entry.

Below is a realistic example of using `MemoryCacheProvider` in an application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Caching;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MemoryCacheProvider>();

        // Create cache provider with configuration
        var cacheProvider = new MemoryCacheProvider(
            maxSizeBytes: 50_000_000, // 50 MB
            cleanupIntervalSeconds: 600, // 10 minutes
            logger: logger
        );

        // Cache a simple value
        await cacheProvider.SetAsync("app_version", "1.0.0");
        var version = await cacheProvider.GetAsync<string>("app_version");
        Console.WriteLine($"App version from cache: {version}");

        // Get or set with automatic expiration
        var expensiveData = await cacheProvider.GetOrSetAsync(
            "expensive_query_results",
            async () => await FetchExpensiveDataAsync(),
            TimeSpan.FromHours(1)
        );

        // Check if cache entry exists
        var exists = await cacheProvider.ExistsAsync("app_version");
        Console.WriteLine($"Cache entry exists: {exists}");

        // Remove a cache entry
        var removed = await cacheProvider.RemoveAsync("app_version");
        Console.WriteLine($"Entry removed: {removed}");

        // Get cache statistics
        var stats = cacheProvider.GetStatistics();
        Console.WriteLine($"\nCache Statistics:");
        Console.WriteLine($" Total Items: {stats.TotalItems}");
        Console.WriteLine($" Total Size: {stats.TotalSizeBytes / 1024 / 1024} MB");
        Console.WriteLine($" Max Size: {stats.MaxSizeBytes / 1024 / 1024} MB");
        Console.WriteLine($" Access Count: {stats.AccessCount}");
        Console.WriteLine($" Total Hits: {stats.Hits}");
        Console.WriteLine($" Total Misses: {stats.Misses}");

        // Clear the entire cache
        await cacheProvider.ClearAsync();
        Console.WriteLine("Cache cleared");

        // Manual cleanup of expired entries
        cacheProvider.CleanupExpired();
        Console.WriteLine("Expired entries cleaned up");
    }

    static async Task<string> FetchExpensiveDataAsync()
    {
        // Simulate expensive operation
        await Task.Delay(100);
        return "Expensive data result";
    }
}
```

## IConnectionPool

`IConnectionPool` is a lightweight interface that provides a thread-safe pool of SQLite connections with configurable concurrency limits, idle connection cleanup, and comprehensive connection management. It efficiently manages connection lifecycle by reusing idle connections and automatically opening new ones when needed, up to the configured maximum pool size.

The pool maintains statistics about connection usage, timeouts, and pool metrics, and provides a clean disposal mechanism through `IAsyncDisposable`. Connections are acquired via `AcquireAsync()` and should always be used within a `using` statement or `await using` declaration to ensure proper return to the pool.

Below is a realistic example of using `IConnectionPool` in an application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ConnectionPool>();

        // Configure connection pool
        var config = new ConnectionPoolConfiguration
        {
            MaxPoolSize = 10,
            MinPoolSize = 2,
            IdleTimeout = TimeSpan.FromMinutes(5),
            AcquireTimeout = TimeSpan.FromSeconds(30),
            CleanupInterval = TimeSpan.FromMinutes(1),
            EnableDiagnostics = true
        };

        // Create connection pool
        var pool = new ConnectionPool("Data Source=app.db", config, logger);

        // Acquire a connection from the pool
        await using (var pooledConnection = await pool.AcquireAsync())
        {
            var connection = pooledConnection.Connection;
            
            // Use the connection for database operations
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Products";
            var count = await command.ExecuteScalarAsync();
            Console.WriteLine($"Total products: {count}");
        }
        // Connection automatically returned to pool when disposed

        // Acquire another connection
        await using (var pooledConnection2 = await pool.AcquireAsync())
        {
            var connection = pooledConnection2.Connection;
            
            // Perform database operations
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Name, Price FROM Products WHERE Price > @price";
            command.Parameters.AddWithValue("@price", 100.00m);
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"{reader["Name"]} - ${reader["Price"]}");
            }
        }

        // Get pool statistics
        var stats = pool.GetStatistics();
        Console.WriteLine($"\nPool Statistics:");
        Console.WriteLine($" Total Connections: {stats.TotalConnections}");
        Console.WriteLine($" Available Connections: {stats.AvailableConnections}");
        Console.WriteLine($" Active Connections: {stats.ActiveConnections}");
        Console.WriteLine($" Max Pool Size: {stats.MaxPoolSize}");
        Console.WriteLine($" Min Pool Size: {stats.MinPoolSize}");
        Console.WriteLine($" Total Acquire Count: {stats.TotalAcquireCount}");
        Console.WriteLine($" Total Timeout Count: {stats.TotalTimeoutCount}");

        // Dispose the pool when done
        await pool.DisposeAsync();
    }
}
```

## FileSystemExtensions

`FileSystemExtensions` is a utility class that provides extension methods for safe file system operations. It offers a comprehensive set of methods for directory and file management, including creation, deletion, path manipulation, and size calculations, all with built-in error handling and null/whitespace validation.

The class is designed to prevent common file system issues by providing safe alternatives to standard .NET file system operations, returning boolean indicators for success/failure rather than throwing exceptions for expected conditions like missing files or directories.

Below is a realistic example of using `FileSystemExtensions` in an application:



```csharp
using System;
using System.IO;
using DotNet.SQLite.CrudGenerator.Utilities;

class Program
{
    static void Main(string[] args)
    {
        string basePath = "/tmp/myapp";
        string tempDir = Path.Combine(basePath, "temp");
        string dataFile = Path.Combine(tempDir, "data.txt");
        string logDir = Path.Combine(basePath, "logs");
        
        // Safely create directory if it doesn't exist
        bool dirCreated = tempDir.CreateDirectoryIfNotExists();
        Console.WriteLine($"Directory created: {dirCreated}");
        
        // Create a file and write some data
        File.WriteAllText(dataFile, "Sample data content");
        Console.WriteLine($"File size: {dataFile.GetFileSize()} bytes");
        Console.WriteLine($"File size formatted: {dataFile.GetFileSizeFormatted()}");
        
        // Check if file has specific extension
        bool hasTxtExtension = dataFile.HasExtension("txt", "csv", "json");
        Console.WriteLine($"Has .txt extension: {hasTxtExtension}");
        
        // Get extension without dot
        string extension = dataFile.GetExtensionWithoutDot();
        Console.WriteLine($"Extension without dot: {extension}");
        
        // Check if path is absolute
        bool isAbsolute = tempDir.IsAbsolutePath();
        Console.WriteLine($"Is absolute path: {isAbsolute}");
        
        // Combine multiple path segments
        string combinedPath = basePath.CombinePaths("src", "utils", "FileSystemExtensions.cs");
        Console.WriteLine($"Combined path: {combinedPath}");
        
        // Safely delete file if it exists
        bool fileDeleted = dataFile.DeleteFileIfExists();
        Console.WriteLine($"File deleted: {fileDeleted}");
        
        // Safely delete directory if it exists
        bool dirDeleted = tempDir.DeleteDirectoryIfExists();
        Console.WriteLine($"Directory deleted: {dirDeleted}");
        
        // Check if directory is empty
        bool isEmpty = logDir.IsEmpty();
        Console.WriteLine($"Directory is empty: {isEmpty}");
        
        // Recursively get files with specific extensions
        // First create some test structure
        Directory.CreateDirectory(logDir);
        File.WriteAllText(Path.Combine(logDir, "app.log"), "Log entry 1");
        File.WriteAllText(Path.Combine(logDir, "error.log"), "Error entry");
        File.WriteAllText(Path.Combine(logDir, "config.txt"), "Config data");
        
        var logFiles = logDir.GetFilesRecursively("log");
        Console.WriteLine($"Found {logFiles.Count()} .log files:");
        foreach (var file in logFiles)
        {
            Console.WriteLine($"  - {Path.GetFileName(file)}");
        }
        
        // Copy a directory recursively
        string sourceDir = Path.Combine(basePath, "source");
        string destDir = Path.Combine(basePath, "destination");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "file1.txt"), "Content 1");
        File.WriteAllText(Path.Combine(sourceDir, "file2.cs"), "Content 2");
        
        sourceDir.CopyDirectory(destDir, overwrite: true);
        Console.WriteLine($"Directory copied: {Directory.Exists(destDir)}");
        
        // Cleanup
        destDir.DeleteDirectoryIfExists();
        logDir.DeleteDirectoryIfExists();
    }
}
```

## ConnectionPoolConfiguration

`ConnectionPoolConfiguration` is a configuration class that defines all parameters for the SQLite connection pool, including pool size limits, timeouts, and cleanup behavior. It provides validation to ensure all settings are within acceptable ranges and can be bound directly from configuration files using the `SectionName` constant.

The configuration controls:
- Minimum and maximum pool sizes
- How long idle connections are retained before cleanup
- How long connection acquisition calls will wait for an available connection
- How frequently the pool performs cleanup sweeps
- Whether detailed diagnostic logging is enabled

Below is a realistic example of configuring and using `ConnectionPoolConfiguration` in an application:

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Configuration;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ConnectionPool>();

        // Configure connection pool settings
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 2,
            MaxPoolSize = 15,
            IdleTimeout = TimeSpan.FromMinutes(10),
            AcquireTimeout = TimeSpan.FromSeconds(45),
            CleanupInterval = TimeSpan.FromMinutes(2),
            EnableDiagnostics = true
        };

        // Validate configuration before use
        config.Validate();

        Console.WriteLine("Connection pool configuration:");
        Console.WriteLine($" Min Pool Size: {config.MinPoolSize}");
        Console.WriteLine($" Max Pool Size: {config.MaxPoolSize}");
        Console.WriteLine($" Idle Timeout: {config.IdleTimeout.TotalMinutes} minutes");
        Console.WriteLine($" Acquire Timeout: {config.AcquireTimeout.TotalSeconds} seconds");
        Console.WriteLine($" Cleanup Interval: {config.CleanupInterval.TotalMinutes} minutes");
        Console.WriteLine($" Diagnostics Enabled: {config.EnableDiagnostics}");
    }
}
```

## Repository

`Repository<T, TKey>` is a generic base class that provides a complete implementation of the repository pattern for SQLite databases. It handles all CRUD operations (Create, Read, Update, Delete) with built-in caching, connection management, and comprehensive logging through `ILogger`.

The repository automatically manages entity persistence to the SQLite database table named after the entity type (e.g., `Product` → `Products` table), handles primary key operations, and maintains an in-memory cache for improved performance on read operations.

Below is a realistic example of using a concrete repository implementation (e.g., `ProductRepository`) in an application:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection
        var database = new DatabaseConnection("products.db");
        
        // Create repository instance with logging
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ProductRepository>();
        var productRepository = new ProductRepository(database, logger);

        // Create and add a new product
        var newProduct = new Product
        {
            Name = "Wireless Headphones",
            Description = "High-quality wireless headphones with noise cancellation",
            Sku = "AUD-WH-001",
            Price = 149.99m,
            Cost = 95.50m,
            StockQuantity = 100,
            CategoryId = 1
        };

        var createdProduct = await productRepository.AddAsync(newProduct);
        Console.WriteLine($"Created product with ID: {createdProduct.Id}");

        // Get a product by ID (from cache if available)
        var fetchedProduct = await productRepository.GetByIdAsync(createdProduct.Id);
        Console.WriteLine($"Fetched product: {fetchedProduct?.Name} - ${fetchedProduct?.Price}");

        // Get all products (cached after first call)
        var allProducts = await productRepository.GetAllAsync();
        Console.WriteLine($"Total products: {allProducts.Count()}");

        // Find products using predicate
        var expensiveProducts = await productRepository.FindAsync(p => p.Price > 100);
        Console.WriteLine($"Expensive products (>$100): {expensiveProducts.Count()}");

        // Count products
        var totalProducts = await productRepository.CountAsync();
        Console.WriteLine($"Total products in database: {totalProducts}");

        // Update a product
        fetchedProduct!.Price = 139.99m;
        var updateSuccess = await productRepository.UpdateAsync(fetchedProduct);
        Console.WriteLine($"Update successful: {updateSuccess}");

        // Check if product exists
        var exists = await productService.ExistsAsync(createdProduct.Id);
        Console.WriteLine($"Product exists: {exists}");

        // Add multiple products at once
        var productsToAdd = new[]
        {
            new Product { Name = "Bluetooth Speaker", Sku = "AUD-BS-002", Price = 59.99m, StockQuantity = 75 },
            new Product { Name = "Smart Watch", Sku = "AUD-SW-003", Price = 199.99m, StockQuantity = 50 }
        };
        var addedProducts = await productRepository.AddRangeAsync(productsToAdd);
        Console.WriteLine($"Added {addedProducts.Count()} products in bulk");

        // Delete a product by ID
        var deleteSuccess = await productRepository.DeleteAsync(createdProduct.Id);
        Console.WriteLine($"Delete successful: {deleteSuccess}");

        // Delete multiple products
        var idsToDelete = new[] { 2, 3 };
        var deletedCount = await productRepository.DeleteRangeAsync(idsToDelete);
        Console.WriteLine($"Deleted {deletedCount} products");
    }
}
```

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connection and repositories
        var database = new DatabaseConnection("inventory.db");
        var productRepository = new ProductRepository(database);
        var categoryRepository = new CategoryRepository(database);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ProductService>();

        // Create ProductService instance
        var productService = new ProductService(productRepository, categoryRepository);

        // Create a new product
        var newProduct = new Product
        {
            Name = "Premium Wireless Headphones",
            Description = "Noise-cancelling wireless headphones with 30-hour battery",
            Sku = "AUD-PWH-001",
            Price = 199.99m,
            Cost = 120.50m,
            StockQuantity = 50,
            CategoryId = 1,
            MinimumStockLevel = 10
        };

        var createdProduct = await productService.CreateAsync(newProduct);
        Console.WriteLine($"Created product with ID: {createdProduct.Id}, SKU: {createdProduct.Sku}");

        // Get a product by ID
        var fetchedProduct = await productService.GetAsync(createdProduct.Id);
        Console.WriteLine($"Fetched product: {fetchedProduct?.Name} - ${fetchedProduct?.Price}");

        // Get all products
        var allProducts = await productService.GetAllAsync();
        Console.WriteLine($"Total products in system: {allProducts.Count()}");

        // Update a product
        fetchedProduct!.Price = 189.99m;
        await productService.UpdateAsync(fetchedProduct);
        Console.WriteLine("Product price updated");

        // Restock inventory
        var restockedProduct = await productService.RestockProductAsync(createdProduct.Id, 25);
        Console.WriteLine($"Restocked. New stock level: {restockedProduct.StockQuantity}");

        // Sell inventory
        var soldProduct = await productService.SellProductAsync(createdProduct.Id, 5);
        Console.WriteLine($"Sold 5 units. Remaining stock: {soldProduct.StockQuantity}");

        // Check if product exists
        var exists = await productService.ExistsAsync(createdProduct.Id);
        Console.WriteLine($"Product exists: {exists}");

        // Get products by category
        var categoryProducts = await productService.GetByCategoryAsync(1);
        Console.WriteLine($"Products in category 1: {categoryProducts.Count()}");

        // Get low stock products
        var lowStockProducts = await productService.GetLowStockProductsAsync();
        Console.WriteLine($"Low stock products: {lowStockProducts.Count()}");

        // Calculate inventory value
        var inventoryValue = await productService.GetInventoryValueAsync();
        Console.WriteLine($"Total inventory value: ${inventoryValue}");

        // Get inventory statistics
        var stats = await productService.GetInventoryStatsAsync();
        Console.WriteLine($"Inventory Stats:");
        Console.WriteLine($"  Total Products: {stats.TotalProducts}");
        Console.WriteLine($"  Total Units: {stats.TotalUnitsInStock}");
        Console.WriteLine($"  Total Value: ${stats.TotalInventoryValue}");
        Console.WriteLine($"  Low Stock Items: {stats.LowStockCount}");
        Console.WriteLine($"  Average Stock: {stats.AverageStockLevel}");

        // Validate a product
        var isValid = productService.Validate(createdProduct);
        Console.WriteLine($"Product validation: {isValid}");

        // Delete a product
        await productService.DeleteAsync(createdProduct.Id);
        Console.WriteLine("Product deleted successfully");
    }
}
```

## OrderService

`OrderService` is a service class that provides comprehensive order management functionality including CRUD operations, order lifecycle management, user-specific order queries, and order status transitions. It implements business logic for order processing while delegating data persistence to the underlying repository.

The service handles order creation with validation and user verification, order status updates (pending → shipped → delivered), user-specific order queries, and order metrics calculation. All operations are validated according to business rules and maintain audit trails through the repository layer.

Below is a realistic example of using `OrderService` in an application:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Enums;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup database connections and repositories
        var database = new DatabaseConnection("orders.db");
        var orderRepository = new OrderRepository(database);
        var userRepository = new UserRepository(database);
        var auditLogRepository = new AuditLogRepository(database);

        // Create OrderService instance
        var orderService = new OrderService(orderRepository, userRepository, auditLogRepository);

        // Create a new user first (orders require a valid user)
        var newUser = new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PasswordHash = "hashed_password_789",
            IsActive = true,
            EmailVerified = true
        };
        
        var userRepositoryForUser = new UserRepository(database);
        var createdUser = await userRepositoryForUser.AddAsync(newUser);
        Console.WriteLine($"Created user with ID: {createdUser.Id}");

        // Create a new order
        var newOrder = new Order
        {
            UserId = createdUser.Id,
            OrderDate = DateTime.UtcNow,
            Status = EntityStatus.Pending,
            ShippingAddress = "123 Main St, City, Country",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 99.99m },
                new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 49.99m }
            },
            Subtotal = 249.97m,
            TaxAmount = 19.99m,
            DiscountAmount = 5.00m,
            ShippingCost = 10.00m
        };

        var createdOrder = await orderService.CreateAsync(newOrder);
        Console.WriteLine($"Created order with ID: {createdOrder.Id}, Status: {createdOrder.Status}");

        // Get an order by ID
        var fetchedOrder = await orderService.GetAsync(createdOrder.Id);
        Console.WriteLine($"Fetched order: Order #{fetchedOrder?.Id}, Total: ${fetchedOrder?.CalculateFinalTotal()}");

        // Get all orders
        var allOrders = await orderService.GetAllAsync();
        Console.WriteLine($"Total orders in system: {allOrders.Count()}");

        // Get orders for a specific user
        var userOrders = await orderService.GetUserOrdersAsync(createdUser.Id);
        Console.WriteLine($"User has {userOrders.Count()} orders");

        // Get pending orders
        var pendingOrders = await orderService.GetPendingOrdersAsync();
        Console.WriteLine($"Total pending orders: {pendingOrders.Count()}");

        // Ship an order
        var shipSuccess = await orderService.ShipOrderAsync(createdOrder.Id, "TRK123456789");
        Console.WriteLine($"Order shipped successfully: {shipSuccess}");

        // Mark an order as delivered
        var deliverSuccess = await orderService.MarkDeliveredAsync(createdOrder.Id);
        Console.WriteLine($"Order marked as delivered: {deliverSuccess}");

        // Check if order exists
        var exists = await orderService.ExistsAsync(createdOrder.Id);
        Console.WriteLine($"Order exists: {exists}");

        // Get order metrics
        var metrics = await orderService.GetMetricsAsync();
        Console.WriteLine($"Order Metrics:");
        Console.WriteLine($" Total Orders: {metrics.TotalOrders}");
        Console.WriteLine($" Pending Orders: {metrics.PendingOrders}");
        Console.WriteLine($" Delivered Orders: {metrics.DeliveredOrders}");
        Console.WriteLine($" Total Revenue: ${metrics.TotalRevenue}");
        Console.WriteLine($" Average Order Value: ${metrics.AverageOrderValue:F2}");
        Console.WriteLine($" Average Tax Amount: ${metrics.AverageTaxAmount:F2}");
        Console.WriteLine($" Total Discounts: ${metrics.TotalDiscounts}");

        // Validate an order
        var isValid = orderService.Validate(createdOrder);
        Console.WriteLine($"Order validation: {isValid}");

        // Update an order
        fetchedOrder!.Status = EntityStatus.Pending;
        await orderService.UpdateAsync(fetchedOrder);
        Console.WriteLine("Order status updated");

        // Delete an order
        await orderService.DeleteAsync(createdOrder.Id);
        Console.WriteLine("Order deleted successfully");
    }
}
```

## DataExportService

`DataExportService` is a service class that provides functionality for exporting entity data to various formats including JSON, CSV, and XML. It supports both string-based exports and direct file/stream output, making it suitable for web APIs, file-based exports, and data migration scenarios.

The service handles formatting through specialized formatters and provides reporting capabilities to track export metadata such as entity name, item count, and export timestamp.

Below is a realistic example of using `DataExportService` in an application:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Models;

class Program
{
    static async Task Main(string[] args)
    {
        // Create DataExportService instance
        var exportService = new DataExportService();

        // Sample data - list of products
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Wireless Headphones",
                Sku = "AUD-WH-001",
                Price = 99.99m,
                StockQuantity = 100
            },
            new Product
            {
                Id = 2,
                Name = "Bluetooth Speaker",
                Sku = "AUD-BS-002",
                Price = 59.99m,
                StockQuantity = 75
            },
            new Product
            {
                Id = 3,
                Name = "Smart Watch",
                Sku = "AUD-SW-003",
                Price = 149.99m,
                StockQuantity = 50
            }
        };

        // Export as JSON string
        var jsonExport = await exportService.ExportAsJsonAsync(products);
        Console.WriteLine("JSON Export:");
        Console.WriteLine(jsonExport);
        Console.WriteLine();

        // Export as CSV string
        var csvExport = await exportService.ExportAsCsvAsync(products);
        Console.WriteLine("CSV Export:");
        Console.WriteLine(csvExport);
        Console.WriteLine();

        // Export as XML string
        var xmlExport = await exportService.ExportAsXmlAsync(products);
        Console.WriteLine("XML Export:");
        Console.WriteLine(xmlExport);
        Console.WriteLine();

        // Generate export report
        var report = exportService.GenerateExportReport(products, "Products");
        Console.WriteLine($"Export Report: {report}");
        Console.WriteLine($"  Entity: {report.EntityName}");
        Console.WriteLine($"  Items: {report.ItemCount}");
        Console.WriteLine($"  Formats: {string.Join(", ", report.AvailableFormats)}");
        Console.WriteLine($"  Sample: {report.SampleItem}");

        // Export to file (JSON format)
        var filePath = "./exports/products_export.json";
        var exportSuccess = await exportService.ExportToFileAsync(
            products,
            filePath,
            ExportFormat.Json
        );
        Console.WriteLine($"File export to {filePath}: {(exportSuccess ? "Success" : "Failed")}");

        // Export to stream (CSV format)
        using (var memoryStream = new MemoryStream())
        {
            await exportService.ExportToStreamAsync(products, memoryStream, ExportFormat.Csv);
            memoryStream.Position = 0;
            using (var reader = new StreamReader(memoryStream))
            {
                var streamContent = await reader.ReadToEndAsync();
                Console.WriteLine("Stream export (first 100 chars):");
                Console.WriteLine(streamContent.Substring(0, Math.Min(100, streamContent.Length)));
            }
        }
    }
}
```
