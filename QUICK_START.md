// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Quick Start Guide

Get SQLite CRUD Generator running in 5 minutes.

## Installation (2 minutes)

```bash
# Clone repository
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator

# Restore and build
dotnet restore
dotnet build
```

## Run Example (1 minute)

```bash
# Run the application
dotnet run --project src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
```

You should see CRUD operations demonstrated with sample data.

## Basic Usage (2 minutes)

### Initialize Services

```csharp
var services = new ServiceCollection();
var settings = new DatabaseSettings { FilePath = "app.db" };
services.AddApplicationServices(settings.ConnectionString);
var serviceProvider = services.BuildServiceProvider();
await serviceProvider.InitializeDatabaseAsync();
```

### Create

```csharp
using var scope = serviceProvider.CreateScope();
var userService = scope.ServiceProvider.GetRequiredService<UserService>();

var user = new User
{
    Username = "john.doe",
    Email = "john@example.com",
    FirstName = "John",
    LastName = "Doe",
    PasswordHash = "hashed_password"
};

var created = await userService.CreateAsync(user);
Console.WriteLine($"User created: {created.Id}");
```

### Read

```csharp
// Get by ID
var user = await userService.GetByIdAsync(1);

// Get all
var allUsers = await userService.GetAllAsync();

// Find with query
var results = await userService.FindAsync(u => u.Email.Contains("@example.com"));
```

### Update

```csharp
user.Email = "newemail@example.com";
var updated = await userService.UpdateAsync(user);
```

### Delete

```csharp
await userService.DeleteAsync(user.Id);
```

## Run Examples

Check the `/examples` directory for 7 complete examples:

```bash
# Example 1: Basic CRUD
dotnet run --project src/DotNet.SQLite.CrudGenerator -- --example 01

# Example 2: Transactions
dotnet run --project src/DotNet.SQLite.CrudGenerator -- --example 02

# View all examples
ls examples/
```

## Common Tasks

### Pagination

```csharp
var (users, total) = await userService.GetPagedAsync(pageNumber: 1, pageSize: 10);
Console.WriteLine($"Page 1 of {Math.Ceiling((double)total / 10)}");
```

### Search

```csharp
var activeUsers = await userService.FindAsync(u => u.IsActive);
var expensive = await productService.FindAsync(p => p.Price > 100);
```

### Transactions

```csharp
using (var transaction = unitOfWork.BeginTransaction())
{
    await userService.CreateAsync(user);
    await productService.CreateAsync(product);
    await transaction.CommitAsync();
}
```

### Export

```csharp
var formatter = new JsonFormatter();
var json = await formatter.FormatAsync(entities);
File.WriteAllText("export.json", json);
```

## Configuration

Edit `appsettings.json`:

```json
{
  "DatabaseSettings": {
    "FilePath": "myapp.db",
    "MaxPoolSize": 10
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60
  }
}
```

## Docker

```bash
# Build
docker build -t crud-app .

# Run
docker run -it crud-app

# Or use Docker Compose
docker-compose up
```

## Makefile Commands

```bash
make build          # Build project
make run            # Run application
make test           # Run tests
make docker-build   # Build Docker image
make publish        # Create Release build
make clean          # Clean artifacts
```

## Project Structure

```
src/
├── Models/          # User, Product, Order, Category, AuditLog
├── Services/        # UserService, ProductService, OrderService
├── Data/            # Repository implementations
├── Interfaces/      # IRepository, IService, IUnitOfWork
└── Configuration/   # DI setup, settings

docs/               # Full documentation
examples/           # 7 complete examples
```

## Next Steps

1. **Explore Examples**: Check `/examples` for real-world patterns
2. **Read Documentation**: `/docs` directory has detailed guides
3. **Try Features**: Pagination, caching, transactions, events
4. **Build Your App**: Extend with your own models and services

## Troubleshooting

### Database Error
```
"Error: Could not open database file"
```

Solution: Ensure database path is valid and writable.

### Build Failed
```bash
dotnet nuget locals all --clear
dotnet restore
dotnet build
```

### Need Help?
- Check `/docs/faq.md`
- Read `/docs/getting-started.md`
- See `CONTRIBUTING.md` for development setup

## Key Features

✅ Automatic CRUD operations  
✅ Repository pattern  
✅ Unit of Work transactions  
✅ Dependency injection  
✅ Audit logging  
✅ Data export (JSON/CSV/XML)  
✅ Pagination support  
✅ Event-driven architecture  
✅ In-memory caching  
✅ Error handling  
✅ Docker ready  

## Resources

- [README](README.md) - Full documentation
- [Getting Started](docs/getting-started.md) - Detailed setup
- [API Reference](docs/api-reference.md) - All methods
- [Architecture](docs/architecture.md) - Design patterns
- [Examples](examples/README.md) - Sample code
- [FAQ](docs/faq.md) - Common questions

## License

MIT License - See [LICENSE](LICENSE) file

---

**Ready to start?** Run `dotnet build` and `dotnet run` now! 🚀
