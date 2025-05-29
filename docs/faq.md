// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Frequently Asked Questions

## General Questions

### Q: What is SQLite CRUD Generator?

A: SQLite CRUD Generator is a .NET 10 framework that automates CRUD operations, repository pattern implementation, and data layer code generation for SQLite databases. It provides a production-ready architecture with dependency injection, audit logging, and comprehensive error handling.

### Q: What .NET versions are supported?

A: Only .NET 10.0 and later. The project uses latest C# language features (records, nullable reference types, implicit usings).

### Q: Can I use this with other databases?

A: Currently optimized for SQLite. The repository pattern makes it possible to extend to other databases (SQL Server, PostgreSQL) by implementing IRepository<T>.

### Q: Is this suitable for production?

A: Yes. The framework includes transaction support, audit logging, error handling, caching, and comprehensive testing considerations.

### Q: What's the license?

A: MIT License. You can use it freely in commercial and personal projects with attribution.

## Installation & Setup

### Q: I'm getting "dotnet: command not found"

A: .NET SDK is not installed or not in PATH. Download from https://dotnet.microsoft.com/download and verify with `dotnet --version`.

### Q: Build fails with "NU1200: Package version not found"

A: Clear NuGet cache and restore:
```bash
dotnet nuget locals all --clear
dotnet restore
dotnet build --no-restore
```

### Q: How do I change the database file location?

A: In DatabaseSettings:
```csharp
var settings = new DatabaseSettings { FilePath = "C:/data/myapp.db" };
services.AddApplicationServices(settings.ConnectionString);
```

### Q: Can I use in-memory database for testing?

A: Modify DatabaseSettings:
```csharp
var settings = new DatabaseSettings { FilePath = ":memory:" };
```

### Q: How do I configure multiple database instances?

A: Create separate DI containers:
```csharp
var services1 = new ServiceCollection();
services1.AddApplicationServices("Data Source=db1.db");

var services2 = new ServiceCollection();
services2.AddApplicationServices("Data Source=db2.db");
```

## Usage Questions

### Q: How do I query with multiple conditions?

A: Use LINQ predicates:
```csharp
var results = await userService.FindAsync(u => 
    u.IsActive && 
    u.Email.Contains("@example.com") &&
    u.CreatedAt > DateTime.UtcNow.AddDays(-30));
```

### Q: What's the difference between soft delete and hard delete?

A: Soft delete marks IsDeleted flag without removing data. Hard delete permanently removes the record.
```csharp
await userService.DeleteAsync(id);           // Soft delete
await userService.DeletePermanentlyAsync(id); // Hard delete
```

### Q: How do I handle transactions across multiple repositories?

A: Use IUnitOfWork:
```csharp
using (var transaction = unitOfWork.BeginTransaction())
{
    var user = await userService.CreateAsync(userData);
    var product = await productService.CreateAsync(productData);
    await transaction.CommitAsync(); // Both succeed or both fail
}
```

### Q: Can I exclude columns from queries?

A: The repository loads all columns. To optimize, use FindAsync with projection:
```csharp
var users = await userService.FindAsync(u => u.IsActive)
    .Select(u => new { u.Id, u.Username }); // Project in-memory
```

### Q: How do I implement custom business logic?

A: Extend service classes:
```csharp
public class CustomUserService : UserService
{
    public async Task<User> CreateWithDefaultsAsync(User user)
    {
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;
        return await CreateAsync(user);
    }
}
```

### Q: How do I subscribe to entity changes?

A: Subscribe to EventBus:
```csharp
eventBus.Subscribe<EntityChangedEvent>(async @event =>
{
    Console.WriteLine($"Entity changed: {@event.EntityName}");
    // Handle event
});
```

## Performance Questions

### Q: How can I improve query performance?

A: 
1. Use pagination for large result sets
2. Enable caching for read-heavy operations
3. Add indexes on frequently queried columns
4. Use FindAsync with specific predicates

### Q: What's the caching overhead?

A: Default TTL is 60 minutes. Configure in appsettings.json:
```json
"CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 120
}
```

### Q: How many concurrent connections are supported?

A: Default pool size is 10, configurable:
```csharp
var settings = new DatabaseSettings { MaxPoolSize = 50 };
```

### Q: What's the maximum database size?

A: SQLite supports databases up to 281TB theoretically, but practical limit depends on hardware (typically 1TB+).

### Q: How do I monitor performance?

A: Enable logging and use PerformanceMonitor:
```csharp
var monitor = new PerformanceMonitor();
using (monitor.Measure("operation"))
{
    await userService.GetAllAsync();
}
Console.WriteLine(monitor.GetMetrics());
```

## Data Questions

### Q: How do I import data in bulk?

A: Use transaction for better performance:
```csharp
using (var transaction = unitOfWork.BeginTransaction())
{
    foreach (var item in largeList)
        await service.CreateAsync(item);
    await transaction.CommitAsync();
}
```

### Q: How do I export data?

A: Use DataExportService:
```csharp
var formatter = new JsonFormatter();
var json = await formatter.FormatAsync(data);
File.WriteAllText("export.json", json);
```

### Q: How do I backup the database?

A: Simple file copy:
```bash
cp /path/to/app.db /backup/app_$(date +%s).db
```

Or use built-in backup utility in docs/deployment.md.

### Q: Can I migrate from another database?

A: Yes, but you'll need to:
1. Export data from old database
2. Transform to match models
3. Use bulk import with transactions
4. Validate data integrity

### Q: How do I clean up old data?

A: Implement data retention:
```csharp
var oldLogs = await auditLogRepo.FindAsync(l => 
    l.CreatedAt < DateTime.UtcNow.AddDays(-365));

foreach (var log in oldLogs)
    await auditLogRepo.DeleteAsync(log.Id);
```

## Architecture Questions

### Q: Can I use this without Dependency Injection?

A: Not recommended, but possible:
```csharp
var connection = new DatabaseConnection(connectionString);
var repo = new UserRepository(connection);
var service = new UserService(repo);
```

### Q: How do I add custom middleware?

A: Implement and register:
```csharp
public class CustomMiddleware
{
    public Task InvokeAsync(/* dependencies */) { }
}

services.AddSingleton<CustomMiddleware>();
```

### Q: Can I override repository behavior?

A: Yes, create custom repository:
```csharp
public class CustomUserRepository : UserRepository
{
    public override async Task<User> GetByIdAsync(int id)
    {
        // Custom logic
        return await base.GetByIdAsync(id);
    }
}
```

### Q: How do I implement a service facade?

A: Create wrapper service:
```csharp
public class UserFacade
{
    private readonly UserService _userService;
    private readonly OrderService _orderService;
    
    public async Task<UserWithOrdersDto> GetUserWithOrdersAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        var orders = await _orderService.GetUserOrdersAsync(id);
        return new UserWithOrdersDto { User = user, Orders = orders };
    }
}
```

## Error Handling Questions

### Q: How do I handle validation errors?

A: Catch ValidationException:
```csharp
try
{
    await userService.CreateAsync(invalidUser);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

### Q: What happens if database is unavailable?

A: RepositoryException is thrown with details:
```csharp
try
{
    await userService.GetAllAsync();
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}
```

### Q: How do I log errors?

A: Use built-in logging middleware or ILogger:
```csharp
var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserService>>();
logger.LogError($"Operation failed: {ex.Message}");
```

### Q: Can I create custom exceptions?

A: Yes, extend base exceptions:
```csharp
public class UserAlreadyExistsException : ValidationException
{
    public UserAlreadyExistsException(string username) 
        : base($"User {username} already exists") { }
}
```

## Deployment Questions

### Q: How do I deploy to production?

A: See docs/deployment.md for detailed guides covering:
- Standalone deployment
- Docker containerization
- Kubernetes orchestration
- Cloud platform deployment

### Q: What are the system requirements?

A: Minimum:
- .NET 10 Runtime
- 512 MB RAM
- 10 GB disk
- Windows/Linux/macOS

Recommended for production:
- 4+ cores, 8+ GB RAM, SSD storage

### Q: How do I enable SSL/TLS?

A: Configure in deployment environment or reverse proxy (nginx, IIS).

### Q: What about load balancing?

A: Use sticky sessions if using local SQLite. For scale, migrate to distributed database with replication.

### Q: How do I monitor the application?

A: Implement health checks and logging as shown in deployment guide.

## Testing Questions

### Q: How do I unit test services?

A: Mock IRepository:
```csharp
var mockRepo = new Mock<IRepository<User>>();
mockRepo.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1 });

var service = new UserService(mockRepo.Object);
var result = await service.GetByIdAsync(1);

Assert.NotNull(result);
```

### Q: Can I use the in-memory database for tests?

A: Yes, with `:memory:` connection string:
```csharp
var settings = new DatabaseSettings { FilePath = ":memory:" };
await serviceProvider.InitializeDatabaseAsync();
```

### Q: How do I test transactions?

A: Use test transaction:
```csharp
using (var transaction = unitOfWork.BeginTransaction())
{
    // Test code
    await transaction.RollbackAsync(); // Always rollback
}
```

## Contributing Questions

### Q: How can I contribute?

A: See CONTRIBUTING section in README.md:
1. Fork the repository
2. Create feature branch
3. Make changes with tests
4. Submit pull request

### Q: What's the code style?

A: Follow C# conventions:
- PascalCase for public members
- camelCase for private members
- XML comments for public APIs
- 4-space indentation

### Q: Can I suggest features?

A: Open an issue on GitHub with detailed description and use case.

---

## Still Have Questions?

- Check documentation in `/docs`
- Review examples in `/examples`
- Search [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- Open a new issue with your question
