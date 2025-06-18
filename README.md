# SQLite CRUD Generator

![CI](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-sqlite-crud-generator)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
[![Build](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/actions/workflows/build.yml)
[![Docker](https://img.shields.io/badge/Docker-ready-blue.svg)](https://www.docker.com/)

A comprehensive .NET 10 source generator and CRUD framework for SQLite databases that automates the creation of CRUD operations, migrations, and gRPC services from C# models. This project provides a production-ready architecture with repository pattern, unit of work, dependency injection, audit logging, and comprehensive code generation capabilities.

## Table of Contents

- [Features](#features)
- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [CLI Reference](#cli-reference)
- [Advanced Features](#advanced-features)
- [Testing](#testing)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Capabilities
- **Automatic Code Generation**: Generate repository interfaces, implementations, and migrations from entity models
- **Complete CRUD Operations**: Full Create, Read, Update, Delete operations with async/await support
- **SQLite Integration**: Native Microsoft.Data.Sqlite support with connection pooling and thread-safe operations
- **Repository Pattern**: Generic and strongly-typed repositories with LINQ query support
- **Unit of Work Pattern**: Transaction support with coordinated multi-repository access
- **Service Layer Architecture**: Well-structured service classes with business logic separation
- **Dependency Injection**: Complete Microsoft.Extensions.DependencyInjection setup with fluent configuration

### v2.0 New Features 🚀
- **Async Bulk Import/Export**: High-performance bulk operations with streaming and progress reporting
- **Streaming Support**: Process large datasets efficiently without memory overload
- **Progress Reporting**: Real-time progress tracking for long-running bulk operations
- **Enhanced CLI**: New commands and improved user experience
- **Migration Diffing**: Compare entity models against live DB schema and generate ALTER TABLE scripts
- **Query Builder Generation**: Generate fluent, type-safe query builder classes from entity types
- **Audit Trail Service**: Persist and query a full audit trail of entity mutations in SQLite

### Advanced Features
- **Source Generation**: Generate boilerplate code at compile-time
- **Audit Logging**: Track all entity changes with timestamps, operation types, and user context
- **Validation Framework**: Entity validation with comprehensive error handling
- **Entity Status Tracking**: Lifecycle management (Active, Inactive, Deleted)
- **Background Tasks**: Async background worker service for long-running operations
- **Event Bus**: Entity change event system for loosely-coupled communication
- **Data Export**: Export entities to JSON, CSV, and XML formats
- **Caching**: In-memory caching with configurable expiration policies
- **Error Handling**: Structured exception hierarchy and middleware integration
- **Rate Limiting**: Request rate limiting middleware for API protection
- **Logging Middleware**: Comprehensive request/response logging
- **gRPC Services**: Generate Protocol Buffer definitions from models

### Data Models
The framework includes five core domain models:
- **User**: User accounts with authentication, profile, and timestamp tracking
- **Product**: Product inventory with pricing, stock management, and category relations
- **Order**: Customer orders with status lifecycle and total calculations
- **Category**: Product categories with hierarchical support and display ordering
- **AuditLog**: Complete audit trail for compliance and debugging

## Project Overview

SQLite CRUD Generator is designed to accelerate .NET development by eliminating boilerplate code through intelligent code generation. The project demonstrates best practices in enterprise application architecture while remaining lightweight and suitable for everything from microservices to standalone applications.

### Design Philosophy

The project emphasizes:
1. **Separation of Concerns**: Clear boundaries between data, business, and presentation layers
2. **SOLID Principles**: Single responsibility, dependency injection, interface-based design
3. **Testability**: All components designed for unit testing with mockable dependencies
4. **Extensibility**: Pluggable services, middleware, and event handlers
5. **Developer Experience**: Fluent APIs, comprehensive documentation, and realistic examples

## Architecture

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Application Entry Point                      │
│                          (Program.cs / CLI)                          │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │                               │
         ┌──────────▼──────────┐          ┌────────▼─────────┐
         │   Middleware Stack  │          │  CLI Commands    │
         ├─────────────────────┤          ├──────────────────┤
         │ • Error Handling    │          │ • Generate       │
         │ • Logging           │          │ • Migrate        │
         │ • Validation        │          │ • Validate       │
         │ • Rate Limiting     │          │ • Stats          │
         └──────────┬──────────┘          └────────┬─────────┘
                    │                              │
                    └──────────────┬───────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │    Service Layer (Business  │
                    │       Logic)                │
                    ├─────────────────────────────┤
                    │ • UserService               │
                    │ • ProductService            │
                    │ • OrderService              │
                    │ • GenerationService         │
                    │ • DataExportService         │
                    │ • EventBus                  │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │   Unit of Work Pattern      │
                    │   (DbContextProvider)       │
                    ├─────────────────────────────┤
                    │ • Transaction Management    │
                    │ • Repository Coordination   │
                    │ • Scope Management          │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │   Data Access Layer         │
                    │   (Repository Pattern)      │
                    ├─────────────────────────────┤
                    │ • UserRepository            │
                    │ • ProductRepository         │
                    │ • OrderRepository           │
                    │ • CategoryRepository        │
                    │ • AuditLogRepository        │
                    │ • Generic IRepository<T>    │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │    Database Connection     │
                    │   (DatabaseConnection)     │
                    ├─────────────────────────────┤
                    │ • SQLite Connection Pool    │
                    │ • Schema Initialization     │
                    │ • Index Management          │
                    │ • Thread-Safe Operations    │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │       SQLite Database       │
                    │   (crudgenerator.db)        │
                    ├─────────────────────────────┤
                    │ • Users Table               │
                    │ • Products Table            │
                    │ • Orders Table              │
                    │ • Categories Table          │
                    │ • AuditLogs Table           │
                    └─────────────────────────────┘
```

### Layered Architecture

**Presentation / CLI Layer**: Command-line interface and entry points
- `Program.cs`: Application bootstrap
- `CLI/` : Command parsing and execution

**Service Layer**: Business logic and orchestration
- `Services/`: User, Product, Order business operations
- `Events/`: Event publishing for entity changes
- `Attributes/`: Code generation markers

**Data Access Layer**: Repository pattern implementation
- `Data/Repository.cs`: Generic base repository
- `Interfaces/IRepository.cs`: Contract for data access
- `Data/DbContextProvider.cs`: Unit of work implementation

**Infrastructure**: Cross-cutting concerns
- `Configuration/`: Dependency injection setup
- `Middleware/`: Request processing pipeline
- `Caching/`: Data caching layer
- `Integration/`: External API communication
- `Utilities/`: Helper functions and extensions

**Domain**: Core business entities
- `Models/`: Domain entities (User, Product, Order, Category, AuditLog)
- `Enums/`: Entity status and operation types
- `Events/`: Domain events

## Prerequisites

### System Requirements
- **OS**: Windows, macOS, or Linux
- **.NET SDK**: .NET 10.0 or later ([Download](https://dotnet.microsoft.com/download))
- **SQLite**: Included with .NET

### Development Environment
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Git**: For version control
- **Docker** (Optional): For containerized deployments
- **Docker Compose** (Optional): For multi-service orchestration

### Verify Installation

```bash
dotnet --version       # Should be 10.0.0 or later
dotnet --list-sdks     # Verify .NET 10 is installed
```

## Installation

### Method 0: NuGet Package

```bash
dotnet add package Zaiets.dotnet.sqlite.crud.generator
```

### Method 1: Clone from GitHub

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator

# Build the project
dotnet build

# Run the application
dotnet run --project src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
```

### Method 2: Docker

```bash
# Build Docker image
docker build -t dotnet-crud-generator .

# Run in container
docker run -it dotnet-crud-generator
```

### Method 3: Docker Compose

```bash
# Start all services
docker-compose up

# View logs
docker-compose logs -f app

# Stop services
docker-compose down
```

### Method 4: Using make (Unix/Linux/macOS)

```bash
# Build project
make build

# Run application
make run

# Run tests
make test

# Clean build artifacts
make clean
```

## Quick Start with Docker 🐳

Get up and running quickly with our Docker images:

```bash
# Pull the latest image
docker pull sarmkadan/dotnet-sqlite-crud-generator:latest

# Run the container
docker run -it sarmkadan/dotnet-sqlite-crud-generator

# Run with volume mount for persistence
docker run -v /local/data:/app/data sarmkadan/dotnet-sqlite-crud-generator
```

### Quick Start with Docker Compose

```yaml
version: '3.8'
services:
  crud-generator:
    image: sarmkadan/dotnet-sqlite-crud-generator:latest
    volumes:
      - ./data:/app/data
    ports:
      - "8080:8080"
```

### Environment Variables

- `DATABASE_PATH`: Path to SQLite database file
- `CONNECTION_TIMEOUT`: Database connection timeout in seconds
- `MAX_POOL_SIZE`: Maximum connection pool size

## Quick Start

### 1. Basic CRUD Operation

```csharp
// Initialize services
var services = new ServiceCollection();
var settings = new DatabaseSettings { FilePath = "app.db" };
services.AddApplicationServices(settings.ConnectionString);
var serviceProvider = services.BuildServiceProvider();

// Initialize database
await serviceProvider.InitializeDatabaseAsync();

// Create a user
var user = new User
{
    Username = "john.doe",
    Email = "john@example.com",
    PasswordHash = "hashed_password",
    FirstName = "John",
    LastName = "Doe"
};

using var scope = serviceProvider.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<UserService>();
var createdUser = await userService.CreateAsync(user);

Console.WriteLine($"User created: {createdUser.Username} (ID: {createdUser.Id})");
```

### 2. Querying Data

```csharp
// Get user by ID
var user = await userService.GetByIdAsync(1);

// Get all users
var allUsers = await userService.GetAllAsync();

// Find users with LINQ
var activeUsers = await userService.FindAsync(u => u.IsActive);

// Get with pagination
var (users, total) = await userService.GetPagedAsync(pageNumber: 1, pageSize: 10);
```

### 3. Updating Entities

```csharp
var user = await userService.GetByIdAsync(1);
user.Email = "newemail@example.com";
user.UpdatedAt = DateTime.UtcNow;

var updated = await userService.UpdateAsync(user);
Console.WriteLine($"User updated: {updated.Email}");
```

### 4. Deleting Entities

```csharp
// Soft delete
var success = await userService.DeleteAsync(userId);

// Hard delete (permanent)
await userService.DeletePermanentlyAsync(userId);
```

## Usage Examples

### Example 1: User Management

```csharp
public async Task ManageUsers(UserService userService)
{
    // Create a new user
    var user = new User
    {
        Username = "alice.smith",
        Email = "alice@example.com",
        PasswordHash = "secure_hash",
        FirstName = "Alice",
        LastName = "Smith"
    };
    
    user = await userService.CreateAsync(user);
    Console.WriteLine($"Created user: {user.Username}");
    
    // Search for users
    var users = await userService.FindAsync(u => u.Email.Contains("example"));
    Console.WriteLine($"Found {users.Count} users");
    
    // Update user
    user.LastName = "Johnson";
    user = await userService.UpdateAsync(user);
    
    // Delete user
    await userService.DeleteAsync(user.Id);
}
```

### Example 2: Product Inventory

```csharp
public async Task ManageInventory(ProductService productService)
{
    // Create a product
    var product = new Product
    {
        Name = "Laptop",
        Description = "High-performance laptop",
        Price = 999.99m,
        StockQuantity = 50,
        CategoryId = 1
    };
    
    product = await productService.CreateAsync(product);
    
    // Calculate total inventory value
    var allProducts = await productService.GetAllAsync();
    decimal totalValue = allProducts.Sum(p => p.Price * p.StockQuantity);
    Console.WriteLine($"Inventory value: ${totalValue:F2}");
    
    // Find low-stock items
    var lowStock = await productService.FindAsync(p => p.StockQuantity < 10);
    Console.WriteLine($"Items low on stock: {lowStock.Count}");
}
```

### Example 3: Order Processing

```csharp
public async Task ProcessOrder(OrderService orderService, UserService userService)
{
    var user = await userService.GetByIdAsync(1);
    
    var order = new Order
    {
        UserId = user.Id,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        TotalAmount = 299.99m
    };
    
    order = await orderService.CreateAsync(order);
    Console.WriteLine($"Order created: {order.Id}");
    
    // Update status
    order.Status = "Shipped";
    order = await orderService.UpdateAsync(order);
    
    // Get user's orders
    var userOrders = await orderService.FindAsync(o => o.UserId == user.Id);
    Console.WriteLine($"User has {userOrders.Count} orders");
}
```

### Example 4: Audit Trail

```csharp
public async Task ViewAuditLog(AuditLogRepository auditRepo)
{
    // Get all changes
    var logs = await auditRepo.GetAllAsync();
    
    // Find changes to a specific entity
    var userChanges = logs
        .Where(l => l.EntityName == "User" && l.EntityId == 1)
        .OrderByDescending(l => l.CreatedAt)
        .ToList();
    
    foreach (var log in userChanges)
    {
        Console.WriteLine($"{log.CreatedAt:yyyy-MM-dd HH:mm:ss} - {log.OperationType}: {log.ChangedProperties}");
    }
}
```

### Example 5: Bulk Operations

```csharp
public async Task BulkImport(ProductService productService, IUnitOfWork unitOfWork)
{
    var products = new List<Product>
    {
        new() { Name = "Product 1", Price = 10m, StockQuantity = 100 },
        new() { Name = "Product 2", Price = 20m, StockQuantity = 200 },
        new() { Name = "Product 3", Price = 30m, StockQuantity = 300 },
    };
    
    using (var transaction = unitOfWork.BeginTransaction())
    {
        try
        {
            foreach (var product in products)
            {
                await productService.CreateAsync(product);
            }
            
            await transaction.CommitAsync();
            Console.WriteLine($"Imported {products.Count} products");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### Example 6: Data Export

```csharp
public async Task ExportData(DataExportService exportService)
{
    var products = await exportService.GetAllProductsAsync();
    
    // Export to JSON
    var json = exportService.ExportToJson(products);
    File.WriteAllText("products.json", json);
    
    // Export to CSV
    var csv = exportService.ExportToCsv(products);
    File.WriteAllText("products.csv", csv);
    
    // Export to XML
    var xml = exportService.ExportToXml(products);
    File.WriteAllText("products.xml", xml);
}
```

### Example 7: Pagination

```csharp
public async Task PaginatedQuery(UserService userService)
{
    int pageSize = 20;
    int pageNumber = 1;
    
    while (true)
    {
        var (users, total) = await userService.GetPagedAsync(pageNumber, pageSize);
        
        foreach (var user in users)
        {
            Console.WriteLine($"{user.Id}: {user.Username}");
        }
        
        Console.WriteLine($"Page {pageNumber} of {Math.Ceiling((double)total / pageSize)}");
        
        if (pageNumber * pageSize >= total) break;
        pageNumber++;
    }
}
```

### Example 8: Caching

```csharp
public async Task UseCaching(ProductService productService)
{
    // First call: hits database
    var product1 = await productService.GetByIdAsync(1);
    Console.WriteLine("First call - hit database");
    
    // Second call: served from cache
    var product2 = await productService.GetByIdAsync(1);
    Console.WriteLine("Second call - served from cache");
    
    // Cache is automatically invalidated on update
    product1.Price = 99.99m;
    await productService.UpdateAsync(product1);
    
    // Next call: cache miss, hits database
    var product3 = await productService.GetByIdAsync(1);
    Console.WriteLine("After update - cache invalidated");
}
```

### Example 9: Error Handling

```csharp
public async Task HandleErrors(UserService userService)
{
    try
    {
        var user = await userService.GetByIdAsync(99999);
    }
    catch (RepositoryException ex)
    {
        Console.WriteLine($"Repository error: {ex.Message}");
    }
    catch (ValidationException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (GenerationException ex)
    {
        Console.WriteLine($"Generation error: {ex.Message}");
    }
}
```

### Example 10: Event Handling

```csharp
public void SubscribeToEvents(EventBus eventBus)
{
    eventBus.Subscribe<EntityChangedEvent>(async @event =>
    {
        Console.WriteLine($"Entity changed: {@event.EntityName}");
        Console.WriteLine($"Operation: {@event.OperationType}");
        Console.WriteLine($"Entity ID: {@event.EntityId}");
        
        await Task.CompletedTask;
    });
}
```

## API Reference

### UserService

```csharp
public class UserService : IService<User>
{
    Task<User> CreateAsync(User entity);
    Task<User> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<User> UpdateAsync(User entity);
    Task<bool> DeleteAsync(int id);
    Task<List<User>> FindAsync(Expression<Func<User, bool>> predicate);
    Task<(List<User>, int)> GetPagedAsync(int pageNumber, int pageSize);
    Task<User> AuthenticateAsync(string username, string password);
}
```

### ProductService

```csharp
public class ProductService : IService<Product>
{
    Task<Product> CreateAsync(Product entity);
    Task<Product> GetByIdAsync(int id);
    Task<List<Product>> GetAllAsync();
    Task<Product> UpdateAsync(Product entity);
    Task<bool> DeleteAsync(int id);
    Task<List<Product>> FindAsync(Expression<Func<Product, bool>> predicate);
    Task<(List<Product>, int)> GetPagedAsync(int pageNumber, int pageSize);
    Task<decimal> CalculateTotalValueAsync();
    Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}
```

### OrderService

```csharp
public class OrderService : IService<Order>
{
    Task<Order> CreateAsync(Order entity);
    Task<Order> GetByIdAsync(int id);
    Task<List<Order>> GetAllAsync();
    Task<Order> UpdateAsync(Order entity);
    Task<bool> DeleteAsync(int id);
    Task<List<Order>> FindAsync(Expression<Func<Order, bool>> predicate);
    Task<(List<Order>, int)> GetPagedAsync(int pageNumber, int pageSize);
    Task<List<Order>> GetUserOrdersAsync(int userId);
    Task<decimal> GetUserTotalSpentAsync(int userId);
}
```

### Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
```

## Configuration

### appsettings.json

```json
{
  "DatabaseSettings": {
    "FilePath": "crudgenerator.db",
    "ConnectionTimeout": 30,
    "MaxPoolSize": 10
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Dependency Injection Configuration

```csharp
services.AddApplicationServices(connectionString);

// Adds:
// - DbContextProvider (Unit of Work)
// - All repositories (UserRepository, ProductRepository, etc.)
// - All services (UserService, ProductService, etc.)
// - GenerationService
// - DataExportService
// - EventBus
// - Caching layer
// - Middleware components
```

## CLI Reference

### Generate Command

Generate code artifacts from models:

```bash
dotnet run -- generate --type migrations
dotnet run -- generate --type grpc
dotnet run -- generate --type repositories
```

### Migrate Command

Apply database migrations:

```bash
dotnet run -- migrate --target "202401010000_InitialCreate"
```

### Diff Command

Compare entity models against the live SQLite schema and produce an ALTER TABLE script for any differences:

```bash
# Show diff for all registered entities
dotnet run -- diff

# Show per-column details
dotnet run -- diff --verbose

# Write generated ALTER script to a file
dotnet run -- diff --output ./Migrations/pending.sql

# Use a custom connection string
dotnet run -- diff --connection "Data Source=/path/to/my.db"
```

The diff command reports three kinds of change:

| Symbol | Meaning |
|--------|---------|
| `+ ADDED` | Column exists in the model but is absent from the database; a safe `ALTER TABLE … ADD COLUMN` is generated |
| `- REMOVED` | Column exists in the database but is missing from the model; manual review comment is emitted |
| `~ CHANGED` | Column type differs between model and database; a table-recreation comment is emitted |

### Validate Command

Validate database schema:

```bash
dotnet run -- validate
```

### List Command

List entities and their properties:

```bash
dotnet run -- list --entity User
```

### Stats Command

Display database statistics:

```bash
dotnet run -- stats
```

## Advanced Features

### Migration Diffing

`MigrationDiffService` compares a C# entity type against its live SQLite table using `PRAGMA table_info` and returns a structured `MigrationDiff` record.

```csharp
var db = new DatabaseConnection("Data Source=mydb.db");
var svc = new MigrationDiffService(db);

var diff = await svc.ComputeDiffAsync(typeof(Product));

if (!diff.IsUpToDate)
{
    Console.WriteLine(diff.AlterScript);    // ALTER TABLE … ADD COLUMN …
    Console.WriteLine($"{diff.TableDiff.ColumnDiffs.Count} column(s) changed");
}
```

The service exposes two lower-level methods for custom workflows:

```csharp
// Current schema from the live database
var actual = await svc.GetActualSchemaAsync("Products");

// Expected schema derived from the entity type via reflection
var expected = svc.GetExpectedSchema(typeof(Product));
```

### Query Builder Generation

`QueryBuilderGenerationService` generates a fluent query builder .cs file for any entity type. The generated class can be dropped into any project and used immediately without additional dependencies.

```csharp
var svc = new QueryBuilderGenerationService("./Generated");
var filePath = await svc.GenerateQueryBuilderAsync(typeof(Product));
// Writes ./Generated/ProductQueryBuilder.cs
```

The generated builder supports:

```csharp
var (sql, parameters) = new ProductQueryBuilder()
    .Select("Id", "Name", "Price")
    .WhereName("LIKE", "%widget%")
    .WhereIsActive("=", true)
    .OrderByDescending("Price")
    .Limit(20)
    .Offset(40)
    .Build();

// sql  → SELECT Id, Name, Price FROM Products WHERE Name LIKE @p0 AND IsActive = @p1 ORDER BY Price DESC LIMIT 20 OFFSET 40
// parameters → { "@p0": "%widget%", "@p1": true }
```

You can also generate the source as a string without writing to disk using `BuildQueryBuilderSource(Type)`, which is useful for preview or testing.

### Audit Trail Service

`AuditTrailService` persists structured change records to the `AuditLogs` SQLite table and provides rich querying without loading data into memory.

#### Recording changes

```csharp
var audit = new AuditTrailService(db);

// Record a create
await audit.RecordAsync("Product", product.Id, OperationType.Create, currentUserId,
    newValues: JsonSerializer.Serialize(product));

// Convenience generic overload — serialises objects automatically
await audit.RecordAsync(product.Id, OperationType.Update, currentUserId,
    before: previousProduct, after: updatedProduct, reason: "Price adjustment");
```

#### Querying the trail

```csharp
// Full history for a single entity
var history = await audit.GetEntityTrailAsync("Product", productId);

// Everything a user has done
var userActivity = await audit.GetUserTrailAsync(userId, limit: 50);

// Latest 20 events across all entities
var recent = await audit.GetRecentAsync(20);

// Flexible filter
var results = await audit.QueryAsync(new AuditTrailFilter
{
    EntityType = "Order",
    OperationType = OperationType.Delete,
    From = DateTime.UtcNow.AddDays(-7),
    Limit = 100
});
```

#### Statistics and maintenance

```csharp
var summary = await audit.GetSummaryAsync();
Console.WriteLine($"Total events: {summary.TotalEntries}");
Console.WriteLine($"By entity: {string.Join(", ", summary.ByEntityType.Select(kv => $"{kv.Key}={kv.Value}"))}");

// Remove records older than 90 days
int purged = await audit.PurgeAsync(DateTime.UtcNow.AddDays(-90));
```

### Background Task Processing

```csharp
var queue = serviceProvider.GetRequiredService<BackgroundTaskQueue>();
await queue.QueueAsync(async token =>
{
    // Long-running operation
    await Task.Delay(5000, token);
    Console.WriteLine("Background task completed");
});
```

### Webhook Integration

```csharp
var handler = new WebhookHandler();
await handler.PublishAsync(new WebhookPayload
{
    Event = "user.created",
    Data = user,
    Timestamp = DateTime.UtcNow
});
```

### External API Integration

```csharp
var client = new ExternalApiClient(httpClientFactory);
var response = await client.PostAsync("/api/endpoint", data);
var result = JsonSerializer.Deserialize<ResponseModel>(response);
```

### Performance Monitoring

```csharp
var monitor = new PerformanceMonitor();
using (monitor.Measure("operation_name"))
{
    // Code to measure
}
Console.WriteLine($"Execution time: {monitor.GetMetrics()}");
```

## Troubleshooting

### Database Lock Issues

**Problem**: SQLite database is locked.

**Solution**:
1. Ensure only one process accesses the database
2. Increase connection timeout: `ConnectionTimeout: 60`
3. Use `PRAGMA journal_mode=WAL` for concurrent access

### Out of Memory

**Problem**: Large dataset operations cause memory issues.

**Solution**:
1. Use pagination: `GetPagedAsync(pageNumber, pageSize)`
2. Stream results instead of loading all at once
3. Increase heap size: `dotnet run --configuration Release`

### Slow Queries

**Problem**: Database queries are slow.

**Solution**:
1. Check database indexes
2. Use `FindAsync` with proper predicates to filter early
3. Enable caching for frequently accessed data
4. Analyze query plans with SQLite tools

### Configuration Issues

**Problem**: Settings validation fails.

**Solution**:
```csharp
var settings = new DatabaseSettings { FilePath = "app.db" };
if (!settings.Validate())
{
    Console.WriteLine(settings.ValidationErrors);
}
```

### Build Failures

**Problem**: Project fails to build.

**Solution**:
```bash
# Clean NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
dotnet build --no-restore
```

## Testing

The project follows best practices for testability:

```csharp
// Mock IRepository for unit tests
var mockRepo = new Mock<IRepository<User>>();
mockRepo.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1, Username = "test" });

// Test service with mocked dependency
var service = new UserService(mockRepo.Object);
var result = await service.GetByIdAsync(1);

Assert.NotNull(result);
Assert.Equal("test", result.Username);
```

## Performance

Microbenchmarks are located in `benchmarks/` and use [BenchmarkDotNet](https://benchmarkdotnet.org/).
Run them with:

```bash
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks \
           --configuration Release -- --filter '*'
```

Results below were measured on an AMD Ryzen 9 5900X, .NET 10, Release build.

### String Extensions

| Method | Description | Mean | Allocated |
|--------|-------------|-----:|----------:|
| `ToPascalCase` | snake → PascalCase | 84.2 ns | 144 B |
| `ToCamelCase` | snake → camelCase | 97.6 ns | 192 B |
| `ToSnakeCase` | PascalCase → snake_case (span, no regex) | 47.3 ns | 64 B |
| `ToKebabCase` | PascalCase → kebab-case | 51.8 ns | 64 B |
| `RemoveWhitespace` | strip all whitespace | 36.9 ns | 56 B |
| `ToSlug` | URL-safe slug | 308.4 ns | 296 B |
| `Repeat` ×8 | `string.Create` copy loop | 28.1 ns | 56 B |
| `Pluralize` | suffix-based pluralization | 23.7 ns | 40 B |

### Cache Operations

| Method | Description | Mean | Allocated |
|--------|-------------|-----:|----------:|
| `GetAsync` — hit | `ValueTask.FromResult`, zero alloc | 51.4 ns | 0 B |
| `GetAsync` — miss | key not present | 22.8 ns | 0 B |
| `SetAsync` — upsert | insert or replace entry | 163.2 ns | 120 B |
| `ExistsAsync` — hit | presence check, zero alloc | 29.6 ns | 0 B |
| `GetOrSetAsync` — hit | cache hit, factory skipped | 57.1 ns | 0 B |
| `GetOrSetAsync` — miss | factory invoked, entry stored | 214.5 ns | 168 B |

### Naming Convention Resolution

| Method | Description | Mean | Allocated |
|--------|-------------|-----:|----------:|
| `GetTableName` | pluralise + snake_case (cached) | 34.8 ns | 48 B |
| `GetColumnName` | attribute check + snake_case (cached) | 37.2 ns | 48 B |
| `GetConventionInfo` | full model property scan | 1.09 µs | 576 B |
| `IsValidPropertyName` — valid | span loop, no LINQ | 17.3 ns | 0 B |
| `IsValidPropertyName` — invalid | early-exit on first bad char | 13.6 ns | 0 B |
| `GetApiEndpoint` | versioned REST path | 79.4 ns | 112 B |
| `ToCSharpToSqlConvention` | round-trip name mapping | 49.1 ns | 64 B |

## Related Projects

### Ecosystem

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

The snippets below show two common patterns for using this library alongside other components in a larger application.

**Combining generated repositories with a custom domain service:**

```csharp
// Register the generator's services alongside your own
services.AddApplicationServices(connectionString);
services.AddScoped<IOrderFulfillmentService, OrderFulfillmentService>();

// Inject generated repositories directly into a domain service
public class OrderFulfillmentService(IRepository<Order> orders, IRepository<Product> products)
{
    public async Task<Order> FulfillAsync(int orderId)
    {
        var order   = await orders.GetByIdAsync(orderId);
        var product = await products.GetByIdAsync(order.ProductId);
        product.StockQuantity--;
        await products.UpdateAsync(product);
        order.Status = "Fulfilled";
        return await orders.UpdateAsync(order);
    }
}
```

**Exporting data and forwarding it via webhook:**

```csharp
var exportService  = scope.ServiceProvider.GetRequiredService<DataExportService>();
var webhookHandler = scope.ServiceProvider.GetRequiredService<WebhookHandler>();

var products = await exportService.GetAllProductsAsync();
var json     = exportService.ExportToJson(products);

await webhookHandler.PublishAsync(new WebhookPayload
{
    Event     = "products.exported",
    Data      = json,
    Timestamp = DateTime.UtcNow
});
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature`
3. **Commit** changes: `git commit -am 'Add feature'`
4. **Push** to branch: `git push origin feature/your-feature`
5. **Submit** a pull request

### Code Style

- Follow C# naming conventions (PascalCase for public members)
- Add XML comments to public methods
- Keep methods focused and under 30 lines
- Use async/await for I/O operations
- Include error handling for edge cases

### Commit Messages

- Use imperative mood: "Add feature" not "Added feature"
- Start with type: `feat:`, `fix:`, `docs:`, `refactor:`
- Keep first line under 72 characters
- Reference issues: `Closes #123`

## License

MIT License

Copyright (c) 2024-2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

## Support

- **Documentation**: See `/docs` directory
- **Examples**: See `/examples` directory  
- **Issues**: [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
