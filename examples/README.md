// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# SQLite CRUD Generator - Examples

Complete, production-ready examples demonstrating all features of the framework.

## Overview

This directory contains five comprehensive examples showcasing different aspects of SQLite CRUD Generator:

| Example | File | Focus | Complexity |
|---------|------|-------|-----------|
| 1 | `01-basic-crud-operations.cs` | CRUD operations | Beginner |
| 2 | `02-transactions-and-unitofwork.cs` | Transactions & UoW | Intermediate |
| 3 | `03-pagination-and-search.cs` | Pagination & queries | Intermediate |
| 4 | `04-data-export.cs` | Data export (JSON/CSV/XML) | Intermediate |
| 5 | `05-error-handling-and-events.cs` | Error handling & events | Advanced |

## Running Examples

### Prerequisites
- .NET 10 SDK installed
- SQLite support available

### Execute Individual Example

```bash
# Build the project
dotnet build

# Run example 1 (basic CRUD)
dotnet run --project src/DotNet.SQLite.CrudGenerator -- \
  --example 01

# Run example 2 (transactions)
dotnet run --project src/DotNet.SQLite.CrudGenerator -- \
  --example 02
```

Or directly from the CLI:

```bash
cd examples
dotnet csc 01-basic-crud-operations.cs
./01-basic-crud-operations.exe
```

### Run All Examples

```bash
dotnet run --project src/DotNet.SQLite.CrudGenerator --run-all-examples
```

## Example Details

### Example 1: Basic CRUD Operations

**File**: `01-basic-crud-operations.cs`

Demonstrates the fundamental operations on entities:
- **CREATE**: Adding new users to the database
- **READ**: Retrieving entities by ID and listing all
- **UPDATE**: Modifying existing entity properties
- **DELETE**: Removing entities (soft delete)
- **SEARCH**: Finding entities by criteria

**Key Concepts**:
- Service initialization with Dependency Injection
- Entity creation and persistence
- Entity retrieval and querying
- Entity modification
- Entity deletion

**Database File**: `example_basic.db`

**Use Case**: Perfect for beginners learning basic operations.

```csharp
// Create
var user = new User { /* properties */ };
var created = await userService.CreateAsync(user);

// Read
var user = await userService.GetByIdAsync(1);
var allUsers = await userService.GetAllAsync();

// Update
user.Email = "new@example.com";
var updated = await userService.UpdateAsync(user);

// Delete
await userService.DeleteAsync(1);

// Search
var results = await userService.FindAsync(u => u.IsActive);
```

### Example 2: Transactions & Unit of Work Pattern

**File**: `02-transactions-and-unitofwork.cs`

Demonstrates transaction handling and coordinated repository operations:
- **Successful Transaction**: Multiple operations committed together
- **Failed Transaction**: Rollback on error
- **Complex Operations**: Multi-entity transactions
- **Error Recovery**: Handling transaction failures

**Key Concepts**:
- IUnitOfWork pattern
- Explicit transaction management
- Commit and rollback operations
- Multi-repository coordination
- Error handling within transactions

**Database File**: `example_transactions.db`

**Use Case**: Essential for operations requiring data consistency across multiple entities.

```csharp
using (var transaction = unitOfWork.BeginTransaction())
{
    try
    {
        var user = await userService.CreateAsync(userData);
        var product = await productService.CreateAsync(productData);
        
        await transaction.CommitAsync();
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Example 3: Pagination & Advanced Search

**File**: `03-pagination-and-search.cs`

Demonstrates querying large datasets efficiently:
- **Pagination**: LoadingLarge result sets in pages
- **Search by Criteria**: Finding specific entities
- **Price Range Search**: Complex LINQ predicates
- **Inventory Analysis**: Business logic queries
- **Result Analysis**: Pagination metadata

**Key Concepts**:
- Paginated queries with GetPagedAsync
- LINQ predicate expressions
- Complex search conditions
- Result enumeration
- Page calculation

**Database File**: `example_pagination.db`

**Use Case**: Critical for applications with large datasets and list views.

```csharp
// Pagination
var (users, total) = await userService.GetPagedAsync(pageNumber: 1, pageSize: 10);

// Search with criteria
var activeUsers = await userService.FindAsync(u => u.IsActive);

// Price range
var affordable = await productService.GetByPriceRangeAsync(10m, 50m);

// Complex query
var recent = await userService.FindAsync(u => 
    u.IsActive && 
    u.CreatedAt > DateTime.UtcNow.AddDays(-30));
```

### Example 4: Data Export

**File**: `04-data-export.cs`

Demonstrates data serialization to multiple formats:
- **JSON Export**: Full entity serialization
- **CSV Export**: Tabular format for spreadsheets
- **XML Export**: Hierarchical data format
- **Filtered Export**: Exporting subsets of data
- **File Management**: Writing and verifying exports

**Key Concepts**:
- JsonFormatter usage
- CsvFormatter usage
- XmlFormatter usage
- File I/O operations
- Format-specific formatting

**Database File**: `example_export.db`

**Export Files**:
- `users_export.json` - All users in JSON format
- `products_export.csv` - All products in CSV format
- `categories_export.xml` - All categories in XML format
- `active_users_export.json` - Filtered active users
- `affordable_products.csv` - Products under $50

**Use Case**: Essential for data interchange, reporting, and integrations.

```csharp
var formatter = new JsonFormatter();
var json = await formatter.FormatAsync(entities);
File.WriteAllText("export.json", json);

var csvFormatter = new CsvFormatter();
var csv = await csvFormatter.FormatAsync(entities);
File.WriteAllText("export.csv", csv);
```

### Example 5: Error Handling & Event-Driven Architecture

**File**: `05-error-handling-and-events.cs`

Demonstrates robust error handling and event publishing:
- **Exception Types**: Handling different exception types
- **Validation Errors**: Input validation failures
- **Repository Errors**: Database operation failures
- **Event Publishing**: Publishing entity change events
- **Event Handling**: Subscribing and responding to events
- **Error Recovery**: Graceful failure handling

**Key Concepts**:
- Custom exception hierarchy
- ValidationException handling
- RepositoryException handling
- EventBus subscription
- Event-driven communication
- Error propagation

**Database File**: `example_errors.db`

**Use Case**: Building resilient applications with loosely-coupled components.

```csharp
// Handle specific exceptions
try
{
    await userService.CreateAsync(user);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}

// Subscribe to events
eventBus.Subscribe<EntityChangedEvent>(async @event =>
{
    Console.WriteLine($"Entity changed: {@event.EntityName}");
});
```

## Database Files

Each example creates its own SQLite database:
- `example_basic.db` (Example 1)
- `example_transactions.db` (Example 2)
- `example_pagination.db` (Example 3)
- `example_export.db` (Example 4)
- `example_errors.db` (Example 5)

**Cleanup**: Remove example databases:
```bash
rm -f example_*.db
```

## Common Patterns Demonstrated

### Dependency Injection Setup
```csharp
var services = new ServiceCollection();
services.AddApplicationServices(connectionString);
var serviceProvider = services.BuildServiceProvider();
```

### Scoped Service Access
```csharp
using (var scope = serviceProvider.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<UserService>();
    // Use service
}
```

### Entity Creation Pattern
```csharp
var entity = new Entity
{
    Property1 = "value",
    Property2 = 123
};
var created = await service.CreateAsync(entity);
```

### Error Handling Pattern
```csharp
try
{
    // Operation
}
catch (SpecificException ex)
{
    // Handle
}
finally
{
    // Cleanup
}
```

## Performance Considerations

- **Pagination**: Use `GetPagedAsync` for large datasets (>1000 records)
- **Caching**: Frequently read data is cached automatically
- **Transactions**: Use for atomic operations across multiple entities
- **Batch Operations**: Use transactions for bulk inserts/updates

## Extending Examples

To create your own example:

1. Copy one of the example files
2. Rename following pattern: `XX-descriptive-name.cs`
3. Update the class name
4. Modify to demonstrate your use case
5. Add console output for clarity

## Troubleshooting Examples

### Database File Already Exists
The examples automatically reinitialize existing databases. To start fresh:
```bash
rm example_*.db
```

### Missing Dependencies
Ensure NuGet packages are restored:
```bash
dotnet restore
```

### Running in IDE
- **Visual Studio**: Right-click Project → Set as Startup Project → Run
- **VS Code**: Use Terminal → Run Example
- **JetBrains Rider**: Run configuration in dropdown menu

## Next Steps

After exploring these examples:

1. **Read the Documentation**: `/docs` directory
2. **Review the API Reference**: `docs/api-reference.md`
3. **Understand Architecture**: `docs/architecture.md`
4. **Deploy Application**: `docs/deployment.md`
5. **Build Your Application**: Create your models and services
6. **Integrate Framework**: Add to your existing projects

## Support

- **Questions**: Check `/docs/faq.md`
- **Issues**: [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions)
