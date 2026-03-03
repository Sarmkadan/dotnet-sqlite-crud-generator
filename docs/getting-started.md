// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with SQLite CRUD Generator

This guide walks you through setting up and using SQLite CRUD Generator in your .NET 10 project.

## Prerequisites Check

Before starting, verify your environment:

```bash
# Check .NET version
dotnet --version
# Output should be 10.0.0 or later

# List installed SDKs
dotnet --list-sdks
# Verify .NET 10.0 appears in the list

# Check for available project templates
dotnet new --list | grep console
```

## Installation Steps

### Step 1: Clone the Repository

```bash
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator
```

### Step 2: Verify Project Structure

```
dotnet-sqlite-crud-generator/
├── src/
│   └── DotNet.SQLite.CrudGenerator/
│       ├── Models/
│       ├── Services/
│       ├── Data/
│       ├── Configuration/
│       ├── Interfaces/
│       ├── CLI/
│       ├── Program.cs
│       └── DotNet.SQLite.CrudGenerator.csproj
├── examples/
├── docs/
├── .gitignore
├── LICENSE
├── README.md
└── dotnet-sqlite-crud-generator.sln
```

### Step 3: Restore NuGet Packages

```bash
dotnet restore
```

Output should show:
- Microsoft.Data.Sqlite
- Microsoft.Extensions.DependencyInjection
- Grpc.Tools
- Grpc.AspNetCore

### Step 4: Build the Project

```bash
dotnet build
```

Expected output:
```
Build started...
Restoring packages...
Compiling project...
Build succeeded!
```

### Step 5: Run the Application

```bash
dotnet run --project src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
```

You should see:
1. Banner with project information
2. Database initialization message
3. CRUD operation demonstrations
4. Success message with completed features

## First CRUD Operation

### Create Your First Entity

```csharp
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

// Initialize services
var services = new ServiceCollection();
var settings = new DatabaseSettings { FilePath = "myapp.db" };
services.AddApplicationServices(settings.ConnectionString);

var serviceProvider = services.BuildServiceProvider();
await serviceProvider.InitializeDatabaseAsync();

// Create user
using var scope = serviceProvider.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<UserService>();

var user = new User
{
    Username = "john_doe",
    Email = "john@example.com",
    FirstName = "John",
    LastName = "Doe",
    PasswordHash = "hashed_password_here",
    IsActive = true
};

var createdUser = await userService.CreateAsync(user);
Console.WriteLine($"✓ User created with ID: {createdUser.Id}");
```

### Read Entities

```csharp
// Get by ID
var user = await userService.GetByIdAsync(1);

// Get all
var allUsers = await userService.GetAllAsync();

// Find with query
var activeUsers = await userService.FindAsync(u => u.IsActive && u.Email.Contains("@example.com"));

// Paginated results
var (users, total) = await userService.GetPagedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Found {total} users");
```

### Update Entities

```csharp
var user = await userService.GetByIdAsync(1);
user.Email = "newemail@example.com";
user.UpdatedAt = DateTime.UtcNow;

var updated = await userService.UpdateAsync(user);
Console.WriteLine($"✓ User updated: {updated.Email}");
```

### Delete Entities

```csharp
// Soft delete (marks as deleted)
var success = await userService.DeleteAsync(1);

// Permanent delete
await userService.DeletePermanentlyAsync(1);
```

## Working with Transactions

For operations involving multiple entities:

```csharp
var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

using (var transaction = unitOfWork.BeginTransaction())
{
    try
    {
        var user = new User { Username = "user1", Email = "user1@example.com", ... };
        await userService.CreateAsync(user);
        
        var product = new Product { Name = "Product 1", Price = 10m, ... };
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
        await productService.CreateAsync(product);
        
        await transaction.CommitAsync();
        Console.WriteLine("✓ Transaction committed");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"✗ Transaction rolled back: {ex.Message}");
    }
}
```

## Configuration

### Database Settings

Create `appsettings.json`:

```json
{
  "DatabaseSettings": {
    "FilePath": "myapp.db",
    "ConnectionTimeout": 30,
    "MaxPoolSize": 10
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60
  }
}
```

### Load Configuration

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var settings = new DatabaseSettings();
configuration.GetSection("DatabaseSettings").Bind(settings);

services.AddApplicationServices(settings.ConnectionString);
```

## Common Tasks

### Find Records by Criteria

```csharp
// Products under $50
var affordable = await productService.FindAsync(p => p.Price < 50);

// Active categories
var active = await categoryService.FindAsync(c => c.IsActive);

// Orders from the last 30 days
var recent = await orderService.FindAsync(o => 
    o.OrderDate >= DateTime.UtcNow.AddDays(-30));
```

### Export Data

```csharp
var exportService = scope.ServiceProvider.GetRequiredService<DataExportService>();

// Get all products
var products = await productService.GetAllAsync();

// Export to JSON
var jsonFormatter = new JsonFormatter();
string json = await jsonFormatter.FormatAsync(products);
File.WriteAllText("products.json", json);

// Export to CSV
var csvFormatter = new CsvFormatter();
string csv = await csvFormatter.FormatAsync(products);
File.WriteAllText("products.csv", csv);
```

### Enable Caching

```csharp
var settings = new CacheConfiguration { Enabled = true };
services.Configure<CacheConfiguration>(options =>
{
    options.Enabled = true;
    options.DefaultExpirationMinutes = 60;
});

// Cache is automatically applied to Get operations
var product = await productService.GetByIdAsync(1); // Cached
var samePage = await productService.GetByIdAsync(1); // Served from cache
```

### Track Entity Changes

```csharp
var auditLog = scope.ServiceProvider.GetRequiredService<AuditLogRepository>();

// Get all changes to a specific user
var userChanges = await auditLog.FindAsync(log => 
    log.EntityName == "User" && 
    log.EntityId == 1);

foreach (var change in userChanges)
{
    Console.WriteLine($"{change.CreatedAt} - {change.OperationType}");
    Console.WriteLine($"Changes: {change.ChangedProperties}");
}
```

## Troubleshooting

### Database File Not Found

```
Error: Could not open database file
```

Solution:
- Ensure database path exists
- Check file permissions
- Create directory if needed: `Directory.CreateDirectory(Path.GetDirectoryName(filePath))`

### Connection Timeout

```
Error: SqliteConnection timeout expired
```

Solution:
```csharp
var settings = new DatabaseSettings 
{ 
    FilePath = "myapp.db",
    ConnectionTimeout = 60  // Increase timeout
};
```

### Table Not Found

```
Error: no such table: Users
```

Solution:
```csharp
// Ensure database is initialized
await serviceProvider.InitializeDatabaseAsync();
```

### NuGet Package Errors

```
Error: Package restore failed
```

Solution:
```bash
# Clear cache and restore
dotnet nuget locals all --clear
dotnet restore
```

## Next Steps

1. **Read** [Architecture Documentation](./architecture.md)
2. **Review** [API Reference](./api-reference.md)
3. **Explore** Examples in `/examples` directory
4. **Check** [Configuration Guide](./configuration.md)
5. **Learn** [Deployment Options](./deployment.md)

## Need Help?

- Check [FAQ](./faq.md) for common questions
- Review example files in `/examples`
- Open an issue on [GitHub](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- Read inline code documentation in source files
