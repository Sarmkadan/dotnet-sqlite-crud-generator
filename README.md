// existing content ...

## AuditHelper

`AuditHelper` is a utility class for tracking and logging audit information. Records entity changes, user actions, and system events. Provides methods to log entity changes, property changes, and other operations. Below is a realistic example of using `AuditHelper` in a console application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Utilities;

class Program
{
static void Main(string[] args)
{
// Log entity change
AuditHelper.LogEntityChange("Product", "12345", "CREATE");

// Log property change
AuditHelper.LogPropertyChange("Product", "12345", "Price", 10.99m, 9.99m);

// Get audit trail for an entity
var auditTrail = AuditHelper.GetEntityAuditTrail("Product", "12345");
foreach (var entry in auditTrail)
{
Console.WriteLine($"Timestamp: {entry.Timestamp}, Operation: {entry.Operation}");
}

// Get operation log
var operationLog = AuditHelper.GetOperationLog("CREATE");
foreach (var entry in operationLog)
{
Console.WriteLine($"Timestamp: {entry.Timestamp}, Entity: {entry.EntityType}");
}

// Get user audit trail
var userAuditTrail = AuditHelper.GetUserAuditTrail("user123");
foreach (var entry in userAuditTrail)
{
Console.WriteLine($"Timestamp: {entry.Timestamp}, Operation: {entry.Operation}");
}

// Get audit entries in a date range
var auditEntries = AuditHelper.GetAuditEntriesInRange(DateTime.Now.AddDays(-7), DateTime.Now);
foreach (var entry in auditEntries)
{
Console.WriteLine($"Timestamp: {entry.Timestamp}, Entity: {entry.EntityType}");
}

// Get audit statistics
var statistics = AuditHelper.GetStatistics();
Console.WriteLine($"Total Entries: {statistics.TotalEntries}");
Console.WriteLine($"Operations: {string.Join(", ", statistics.Operations.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

// Clear audit log
AuditHelper.ClearAuditLog();

// Export audit log to CSV
var csv = AuditHelper.ExportToCsv();
Console.WriteLine(csv);
}
}

public class AuditLogEntry
{
public Guid Id { get; set; }
public DateTime Timestamp { get; set; }
public string EntityType { get; set; } = string.Empty;
public string EntityId { get; set; } = string.Empty;
public string Operation { get; set; } = string.Empty;
public string? UserId { get; set; }
public string? IpAddress { get; set; }
public string? Details { get; set; }
}

public class AuditStatistics
{
public int TotalEntries { get; set; }
public int EntryCount { get; set; }
public Dictionary<string, int> Operations { get; set; } = new();
}
```

// ... rest of README content ...

## NamingConventionHelper

`NamingConventionHelper` is a utility class for applying consistent naming conventions across different contexts (C#, SQL, gRPC, and API endpoints). It provides methods to convert between naming conventions, validate property names, and extract naming information for display purposes. Below is a realistic example of using `NamingConventionHelper` in a console application:

```csharp
using System;
using System.Reflection;
using DotNet.SQLite.CrudGenerator.Utilities;

class Program
{
static void Main(string[] args)
{
// Example entity type
var productType = typeof(Product);

// Get SQLite column type for a property type
string sqlType = NamingConventionHelper.GetSqlType(typeof(int));
Console.WriteLine($"SQL type for int: {sqlType}"); // OUTPUT: INTEGER

string nullableSqlType = NamingConventionHelper.GetSqlType(typeof(int?));
Console.WriteLine($"SQL type for int?: {nullableSqlType}"); // OUTPUT: INTEGER

// Convert C# property names to SQL column names
string sqlColumn = NamingConventionHelper.ToCSharpToSqlConvention("UserId");
Console.WriteLine($"C# 'UserId' to SQL: {sqlColumn}"); // OUTPUT: user_id

string sqlColumn2 = NamingConventionHelper.ToCSharpToSqlConvention("FirstName");
Console.WriteLine($"C# 'FirstName' to SQL: {sqlColumn2}"); // OUTPUT: first_name

// Convert SQL column names to C# property names
string csharpProperty = NamingConventionHelper.ToSqlToCSharpConvention("user_id");
Console.WriteLine($"SQL 'user_id' to C#: {csharpProperty}"); // OUTPUT: UserId

string csharpProperty2 = NamingConventionHelper.ToSqlToCSharpConvention("first_name");
Console.WriteLine($"SQL 'first_name' to C#: {csharpProperty2}"); // OUTPUT: FirstName

// Get database table name for a type
string tableName = NamingConventionHelper.GetTableName(typeof(Product));
Console.WriteLine($"Table name for Product: {tableName}"); // OUTPUT: products

string singularTableName = NamingConventionHelper.GetTableName(typeof(Product), false);
Console.WriteLine($"Singular table name for Product: {singularTableName}"); // OUTPUT: product

// Get database column name for a property
var idProperty = typeof(Product).GetProperty("Id")!;
string columnName = NamingConventionHelper.GetColumnName(idProperty);
Console.WriteLine($"Column name for Product.Id: {columnName}"); // OUTPUT: id

// Get gRPC service name
string grpcService = NamingConventionHelper.GetGrpcServiceName("ProductService");
Console.WriteLine($"gRPC service name: {grpcService}"); // OUTPUT: ProductService

string grpcService2 = NamingConventionHelper.GetGrpcServiceName("Product");
Console.WriteLine($"gRPC service name: {grpcService2}"); // OUTPUT: ProductService

// Get gRPC message name
string grpcMessage = NamingConventionHelper.GetGrpcMessageName("ProductMessage");
Console.WriteLine($"gRPC message name: {grpcMessage}"); // OUTPUT: ProductMessage

string grpcMessage2 = NamingConventionHelper.GetGrpcMessageName("Product");
Console.WriteLine($"gRPC message name: {grpcMessage2}"); // OUTPUT: ProductMessage

// Get API endpoint
string apiEndpoint = NamingConventionHelper.GetApiEndpoint(typeof(Product));
Console.WriteLine($"API endpoint: {apiEndpoint}"); // OUTPUT: /api/v1/products

string apiEndpoint2 = NamingConventionHelper.GetApiEndpoint(typeof(Product), "v2");
Console.WriteLine($"API endpoint (v2): {apiEndpoint2}"); // OUTPUT: /api/v2/products

// Validate property name
bool isValid = NamingConventionHelper.IsValidPropertyName("UserId");
Console.WriteLine($"Is 'UserId' valid: {isValid}"); // OUTPUT: True

bool isInvalid = NamingConventionHelper.IsValidPropertyName("123Invalid");
Console.WriteLine($"Is '123Invalid' valid: {isInvalid}"); // OUTPUT: False

// Get naming convention info for display
var conventionInfo = NamingConventionHelper.GetConventionInfo(typeof(Product));
Console.WriteLine($"Entity: {conventionInfo.EntityName}");
Console.WriteLine($"Table: {conventionInfo.TableName}");
Console.WriteLine($"API Endpoint: {conventionInfo.ApiEndpoint}");
Console.WriteLine($"gRPC Service: {conventionInfo.GrpcServiceName}");
Console.WriteLine("Properties:");
foreach (var prop in conventionInfo.Properties)
{
Console.WriteLine($" - {prop.PropertyName} -> {prop.ColumnName} ({prop.Type})");
}
}
}

// Example entity class
public class Product
{
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public decimal Price { get; set; }
public DateTime CreatedDate { get; set; }
}
```

## CsvFormatter

`CsvFormatter` is a utility class for formatting data to CSV (Comma-Separated Values). It handles escaping, quoting, and custom delimiters, and supports both single objects and collections. Below is a realistic example of using `CsvFormatter` in a console application:

```csharp
using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Formatters;

class Program
{
static void Main(string[] args)
{
var formatter = new CsvFormatter();
var product = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CreatedDate = DateTime.Now };
var csv = formatter.Format(product);
Console.WriteLine(csv);

var products = new List<Product>
{
new Product { Id = 1, Name = "Test Product 1", Price = 10.99m, CreatedDate = DateTime.Now },
new Product { Id = 2, Name = "Test Product 2", Price = 9.99m, CreatedDate = DateTime.Now },
};
csv = formatter.Format(products);
Console.WriteLine(csv);

var parsedProduct = formatter.Parse<Product>(csv);
Console.WriteLine($"Parsed Product: {parsedProduct.Name}");

var parsedProducts = formatter.ParseCollection<Product>(csv);
foreach (var p in parsedProducts)
{
Console.WriteLine($"Parsed Product: {p.Name}");
}
}
}

public class Product
{
public int Id { get; set; }
public string Name { get; set; }
public decimal Price { get; set; }
public DateTime CreatedDate { get; set; }
}
```

## XmlFormatter

`XmlFormatter` is a utility class for formatting data to XML with customizable serialization options. It supports both single objects and collections with proper namespace handling, and provides methods for parsing XML back to objects. The formatter supports pretty-printing, omitting XML declarations, and extracting values via XPath.

Below is a realistic example of using `XmlFormatter` in a console application:

```csharp
using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Formatters;

class Program
{
    static void Main(string[] args)
    {
        // Create formatter with pretty printing enabled and XML declaration omitted
        var formatter = new XmlFormatter(pretty: true, omitDeclaration: true);

        // Format a single object to XML
        var product = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CreatedDate = DateTime.Now };
        var xml = formatter.Format(product);
        Console.WriteLine("Single Product XML:");
        Console.WriteLine(xml);

        // Format a collection to XML
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 10.99m, CreatedDate = DateTime.Now },
            new Product { Id = 2, Name = "Test Product 2", Price = 9.99m, CreatedDate = DateTime.Now },
        };
        var xmlCollection = formatter.Format(products);
        Console.WriteLine("\nProduct Collection XML:");
        Console.WriteLine(xmlCollection);

        // Parse XML back to object
        var parsedProduct = formatter.Parse<Product>(xml);
        Console.WriteLine($"\nParsed Product: {parsedProduct?.Name}");

        // Parse XML collection back to objects
        var parsedProducts = formatter.ParseCollection<Product>(xmlCollection);
        Console.WriteLine("\nParsed Products:");
        foreach (var p in parsedProducts ?? new List<Product>())
        {
            Console.WriteLine($" - Product {p.Id}: {p.Name} (${p.Price})");
        }

        // Get XML value via XPath
        var priceValue = formatter.GetXmlValue(product, "//Price");
        Console.WriteLine($"\nPrice from XPath: {priceValue}");

        // Add XML attribute to an object
        formatter.AddXmlAttribute(product, "Description", "Premium quality product");
        Console.WriteLine("\nProduct with added attribute:");
        Console.WriteLine(formatter.Format(product));

        // Async formatting
        var asyncXml = await formatter.FormatAsync(product);
        Console.WriteLine("\nAsync formatted XML:");
        Console.WriteLine(asyncXml);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

## ConfigurationTests

`ConfigurationTests` is a test class that validates the configuration settings for the SQLite CRUD Generator library. It tests various configuration classes including `DatabaseSettings`, `ConnectionPoolConfiguration`, `CacheConfiguration`, and `ApplicationConfiguration` to ensure they handle edge cases properly and throw appropriate exceptions for invalid configurations.

Below is a realistic example of using the configuration classes in a console application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Configuration;

class Program
{
    static void Main(string[] args)
    {
        // Database settings with file path containing spaces
        var dbSettings = new DatabaseSettings
        {
            FilePath = "path with spaces/test database.db"
        };
        Console.WriteLine($"Connection string: {dbSettings.ConnectionString}");
        // OUTPUT: Connection string: Data Source="path with spaces/test database.db";

        // Database settings with file path containing Unicode characters
        var unicodeDbSettings = new DatabaseSettings
        {
            FilePath = "path/with/unicode/データベース.db"
        };
        Console.WriteLine($"Unicode connection string: {unicodeDbSettings.ConnectionString}");
        // OUTPUT: Unicode connection string: Data Source="path/with/unicode/データベース.db";

        // Connection pool configuration with valid settings
        var poolConfig = new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 10,
            IdleTimeout = TimeSpan.FromMinutes(5),
            AcquireTimeout = TimeSpan.FromSeconds(30),
            CleanupInterval = TimeSpan.FromMinutes(1),
            EnableDiagnostics = true
        };
        poolConfig.Validate();
        Console.WriteLine("Connection pool configuration is valid");

        // Cache configuration with valid settings
        var cacheConfig = new CacheConfiguration
        {
            Enabled = true,
            MaxSizeBytes = 10_000_000,
            DefaultTTL = TimeSpan.FromMinutes(30),
            CleanupIntervalSeconds = 300
        };
        Console.WriteLine($"Cache enabled: {cacheConfig.Enabled}, Max size: {cacheConfig.MaxSizeBytes} bytes");

        // Application configuration with all settings
        var appConfig = new ApplicationConfiguration
        {
            Database = new DatabaseSettings { ConnectionString = "Data Source=app.db" },
            Cache = new CacheConfiguration { MaxSizeBytes = 5_000_000 },
            BackgroundWorker = new BackgroundWorkerConfiguration { WorkerCount = 4 }
        };
        appConfig.Validate();
        Console.WriteLine("Application configuration is valid");
    }
}
```

## RepositoryIntegrationTests

`RepositoryIntegrationTests` is a test class that provides integration tests for repository operations against an in-memory SQLite database. It tests CRUD operations for products and users, ensuring database interactions work correctly with proper seeding and cleanup. The class demonstrates how to set up an in-memory database, initialize repositories, and test basic repository operations.

Below is a realistic example of using the repository pattern with the test infrastructure in a console application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Tests;

class Program
{
    static async Task Main(string[] args)
    {
        // Create in-memory database connection
        var databaseConnection = new DatabaseConnection("Data Source=:memory:");
        
        // Initialize repositories
        var productRepository = new ConcreteProductRepository(databaseConnection);
        var userRepository = new ConcreteUserRepository(databaseConnection);
        
        // Initialize database and seed required data
        await databaseConnection.InitializeDatabaseAsync(true);
        SeedCategories(databaseConnection);
        
        // Add a new product
        var newProduct = new Product
        {
            Name = "Premium Coffee",
            Sku = "PC-001",
            CategoryId = 1,
            Price = 12.99m,
            Cost = 6.50m,
            StockQuantity = 100,
            ReorderLevel = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var addedProduct = await productRepository.AddAsync(newProduct);
        Console.WriteLine($"Added product with ID: {addedProduct.Id}");
        
        // Retrieve the product
        var retrievedProduct = await productRepository.GetByIdAsync(addedProduct.Id);
        Console.WriteLine($"Retrieved product: {retrievedProduct?.Name} (${retrievedProduct?.Price})");
        
        // Update the product
        retrievedProduct!.Price = 14.99m;
        retrievedProduct.UpdatedAt = DateTime.UtcNow;
        var updateResult = await productRepository.UpdateAsync(retrievedProduct);
        Console.WriteLine($"Update successful: {updateResult}");
        
        // Get all products
        var allProducts = await productRepository.GetAllAsync();
        Console.WriteLine($"Total products: {allProducts.Count}");
        
        // Delete the product
        var deleteResult = await productRepository.DeleteAsync(addedProduct.Id);
        Console.WriteLine($"Delete successful: {deleteResult}");
        
        // Cleanup
        await databaseConnection.DisposeAsync();
    }
    
    private static void SeedCategories(DatabaseConnection connection)
    {
        using var command = connection.Connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Categories (Id, Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES 
        (1, 'Category 1', 0, 1, datetime('now'), datetime('now')),
        (2, 'Category 2', 0, 1, datetime('now'), datetime('now')),
        (3, 'Category 3', 0, 1, datetime('now'), datetime('now'));";
        command.ExecuteNonQuery();
    }
}
```

## JsonFormatter

`JsonFormatter` is a utility class for formatting data to JSON with customizable serialization options. It supports both pretty-printing and compact output, handles circular references, and provides custom type conversions. The formatter includes synchronous and asynchronous methods for formatting single objects or collections, parsing JSON back to objects, and extracting values via JSON paths.

Below is a realistic example of using `JsonFormatter` in a console application:

```csharp
using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Formatters;

class Program
{
    static void Main(string[] args)
    {
        // Create formatter with pretty printing enabled and null values ignored
        var formatter = new JsonFormatter(pretty: true, ignoreNull: true);

        // Format a single object to JSON
        var product = new Product
        {
            Id = 1,
            Name = "Premium Coffee",
            Price = 12.99m,
            CreatedDate = DateTime.UtcNow,
            Description = "Organic Arabica beans"
        };
        
        var json = formatter.Format(product);
        Console.WriteLine("Single Product JSON:");
        Console.WriteLine(json);

        // Format a collection to JSON
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Premium Coffee", Price = 12.99m, CreatedDate = DateTime.UtcNow },
            new Product { Id = 2, Name = "Green Tea", Price = 8.50m, CreatedDate = DateTime.UtcNow },
            new Product { Id = 3, Name = "Black Tea", Price = 7.25m, CreatedDate = DateTime.UtcNow }
        };
        
        var jsonCollection = formatter.Format(products);
        Console.WriteLine("\nProduct Collection JSON:");
        Console.WriteLine(jsonCollection);

        // Parse JSON back to object
        var parsedProduct = formatter.Parse<Product>(json);
        Console.WriteLine($"\nParsed Product: {parsedProduct?.Name} (${parsedProduct?.Price})");

        // Parse JSON collection back to objects
        var parsedProducts = formatter.ParseCollection<Product>(jsonCollection);
        Console.WriteLine("\nParsed Products:");
        foreach (var p in parsedProducts ?? new List<Product>()) 
        {
            Console.WriteLine($" - Product {p.Id}: {p.Name} (${p.Price})");
        }

        // Get JSON value via path
        var priceValue = formatter.GetJsonPath(product, "price");
        Console.WriteLine($"\nPrice from path: {priceValue}");

        // Parse as JsonDocument for advanced JSON manipulation
        var jsonDoc = formatter.ParseAsDocument(json);
        Console.WriteLine("\nJSON Document Root Element:");
        Console.WriteLine(jsonDoc.RootElement.GetRawText());

        // Async formatting
        var asyncJson = await formatter.FormatAsync(product);
        Console.WriteLine("\nAsync formatted JSON:");
        Console.WriteLine(asyncJson);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Description { get; set; }
}
```

```csharp
using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Formatters;

class Program
{
static void Main(string[] args)
{
// Create formatter with pretty printing enabled and XML declaration omitted
var formatter = new XmlFormatter(pretty: true, omitDeclaration: true);

// Format a single object to XML
var product = new Product { Id = 1, Name = "Test Product", Price = 10.99m, CreatedDate = DateTime.Now };
var xml = formatter.Format(product);
Console.WriteLine("Single Product XML:");
Console.WriteLine(xml);

// Format a collection to XML
var products = new List<Product>
{
new Product { Id = 1, Name = "Test Product 1", Price = 10.99m, CreatedDate = DateTime.Now },
new Product { Id = 2, Name = "Test Product 2", Price = 9.99m, CreatedDate = DateTime.Now },
};
var xmlCollection = formatter.Format(products);
Console.WriteLine("\nProduct Collection XML:");
Console.WriteLine(xmlCollection);

// Parse XML back to object
var parsedProduct = formatter.Parse<Product>(xml);
Console.WriteLine($"\nParsed Product: {parsedProduct?.Name}");

// Parse XML collection back to objects
var parsedProducts = formatter.ParseCollection<Product>(xmlCollection);
Console.WriteLine("\nParsed Products:");
foreach (var p in parsedProducts ?? new List<Product>())
{
Console.WriteLine($" - Product {p.Id}: {p.Name} (${p.Price})");
}

// Get XML value via XPath
var priceValue = formatter.GetXmlValue(product, "//Price");
Console.WriteLine($"\nPrice from XPath: {priceValue}");

// Add XML attribute to an object
formatter.AddXmlAttribute(product, "Description", "Premium quality product");
Console.WriteLine("\nProduct with added attribute:");
Console.WriteLine(formatter.Format(product));

// Async formatting
var asyncXml = await formatter.FormatAsync(product);
Console.WriteLine("\nAsync formatted XML:");
Console.WriteLine(asyncXml);
}
}

public class Product
{
public int Id { get; set; }
public string Name { get; set; } = string.Empty;
public decimal Price { get; set; }
public DateTime CreatedDate { get; set; }
}
```