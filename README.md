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
