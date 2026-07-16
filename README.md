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