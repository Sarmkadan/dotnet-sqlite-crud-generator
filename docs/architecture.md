// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Overview

SQLite CRUD Generator follows a layered architecture pattern with clear separation of concerns.

## Architectural Layers

### 1. Presentation Layer

**Purpose**: Handle CLI commands and user interaction

**Components**:
- `Program.cs`: Application entry point and bootstrap
- `CLI/CommandParser.cs`: Parse command-line arguments
- `CLI/GenerateCommand.cs`: Code generation
- `CLI/MigrateCommand.cs`: Database migrations
- `CLI/ValidateCommand.cs`: Schema validation
- `CLI/ListCommand.cs`: List entities
- `CLI/StatsCommand.cs`: Display statistics

**Responsibilities**:
- Parse user input
- Coordinate service calls
- Format and display output
- Handle CLI errors gracefully

### 2. Service Layer

**Purpose**: Implement business logic and orchestration

**Components**:
- `Services/UserService.cs`: User management
- `Services/ProductService.cs`: Product catalog
- `Services/OrderService.cs`: Order processing
- `Services/GenerationService.cs`: Code generation
- `Services/DataExportService.cs`: Data export functionality

**Key Interfaces**:
```csharp
public interface IService<T> where T : class
{
    Task<T> CreateAsync(T entity);
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

**Responsibilities**:
- Validate input data
- Implement business rules
- Orchestrate repository operations
- Publish domain events
- Handle caching logic

### 3. Data Access Layer

**Purpose**: Provide abstraction over database operations

**Key Components**:
- `Data/Repository.cs`: Generic repository base class
- `Interfaces/IRepository<T>`: Data access contract
- Concrete repositories: `UserRepository`, `ProductRepository`, etc.

**Repository Pattern**:
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

**Responsibilities**:
- Abstract database operations
- Build SQL queries
- Handle entity mapping
- Manage result sets
- Support LINQ queries

### 4. Infrastructure Layer

**Purpose**: Cross-cutting concerns and technical services

**Key Components**:

#### Database Connection
- `Data/DatabaseConnection.cs`: Connection management
- Thread-safe SQLite connection pooling
- Schema initialization and management

#### Unit of Work
- `Data/DbContextProvider.cs`: Implements IUnitOfWork
- Transaction management
- Coordinated repository access
- Scope management for DI

#### Configuration
- `Configuration/DependencyInjection.cs`: DI setup
- `Configuration/DatabaseSettings.cs`: Database configuration
- `Configuration/CacheConfiguration.cs`: Caching setup

#### Middleware
- `Middleware/ErrorHandlingMiddleware.cs`: Exception handling
- `Middleware/LoggingMiddleware.cs`: Request/response logging
- `Middleware/ValidationMiddleware.cs`: Input validation
- `Middleware/RateLimitingMiddleware.cs`: Rate limiting

#### Supporting Services
- `Caching/MemoryCacheProvider.cs`: In-memory caching
- `Events/EventBus.cs`: Event publishing
- `Integration/ExternalApiClient.cs`: External API calls
- `Integration/WebhookHandler.cs`: Webhook publishing

### 5. Domain Layer

**Purpose**: Core business entities and value objects

**Components**:
- `Models/User.cs`: User aggregate root
- `Models/Product.cs`: Product catalog
- `Models/Order.cs`: Order aggregate
- `Models/Category.cs`: Category value object
- `Models/AuditLog.cs`: Audit trail

**Attributes**:
- `Attributes/GenerateGrpcAttribute.cs`: Code generation marker

**Enums**:
- `Enums/EntityStatus.cs`: Lifecycle states
- `Enums/OperationType.cs`: Operation types

**Events**:
- `Events/EntityChangedEvent.cs`: Entity change event
- `Events/EventBus.cs`: Event system

## Data Flow

### Create Operation Flow

```
CLI Command
    ↓
CommandParser → CreateCommand
    ↓
Service.CreateAsync(entity)
    ↓
Validation checks
    ↓
Repository.AddAsync(entity)
    ↓
SQL INSERT query
    ↓
Database execution
    ↓
AuditLog creation
    ↓
EventBus.PublishAsync(EntityChangedEvent)
    ↓
Response returned
```

### Read Operation Flow

```
CLI Command / Service Call
    ↓
Service.GetByIdAsync(id)
    ↓
Check cache
    ├─ Hit: return cached value
    └─ Miss: continue
    ↓
Repository.GetByIdAsync(id)
    ↓
SQL SELECT query
    ↓
Database execution
    ↓
Map results to entity
    ↓
Store in cache
    ↓
Return entity
```

### Update Operation Flow

```
Service.UpdateAsync(entity)
    ↓
Validation checks
    ↓
Repository.UpdateAsync(entity)
    ↓
SQL UPDATE query
    ↓
Database execution
    ↓
Invalidate cache
    ↓
Create AuditLog
    ↓
EventBus.PublishAsync(EntityChangedEvent)
    ↓
Response returned
```

## Design Patterns

### 1. Repository Pattern

Abstracts data access, allowing swapping implementations:

```csharp
public interface IRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task<T> GetByIdAsync(int id);
    // ...
}

// Can swap implementations (SQL, NoSQL, File-based, etc.)
public class Repository<T> : IRepository<T> where T : class
{
    private readonly DatabaseConnection _connection;
    
    public async Task<T> AddAsync(T entity)
    {
        // Implementation
    }
}
```

### 2. Unit of Work Pattern

Coordinates multiple repositories in a transaction:

```csharp
public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Product> Products { get; }
    ITransaction BeginTransaction();
    Task<bool> SaveAsync();
}
```

### 3. Service Layer Pattern

Encapsulates business logic:

```csharp
public class UserService : IService<User>
{
    private readonly IRepository<User> _repository;
    private readonly IRepository<AuditLog> _auditLog;
    private readonly EventBus _eventBus;
    
    public async Task<User> CreateAsync(User entity)
    {
        // Business logic
        if (!ValidateUser(entity))
            throw new ValidationException("Invalid user");
        
        var created = await _repository.AddAsync(entity);
        await _auditLog.AddAsync(/* audit entry */);
        await _eventBus.PublishAsync(new EntityChangedEvent(created));
        
        return created;
    }
}
```

### 4. Dependency Injection

Components depend on abstractions, not concrete types:

```csharp
services.AddScoped<IRepository<User>, UserRepository>();
services.AddScoped<IService<User>, UserService>();

// Service receives dependencies via constructor
public class UserService
{
    public UserService(IRepository<User> repository, 
                      EventBus eventBus,
                      ILogger logger)
    {
        // Dependencies injected
    }
}
```

### 5. Event-Driven Architecture

Services communicate through events:

```csharp
// Publish event
await _eventBus.PublishAsync(new EntityChangedEvent
{
    EntityName = "User",
    OperationType = "Create",
    EntityId = user.Id
});

// Subscribe to events
_eventBus.Subscribe<EntityChangedEvent>(async @event =>
{
    await LogChangeAsync(@event);
    await SendNotificationAsync(@event);
});
```

## Dependency Injection Container

### Configured Services

```csharp
public static void AddApplicationServices(
    this IServiceCollection services, 
    string connectionString)
{
    // Database
    services.AddSingleton(new DatabaseConnection(connectionString));
    
    // Repositories
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddScoped<UserRepository>();
    services.AddScoped<ProductRepository>();
    // ...
    
    // Services
    services.AddScoped<UserService>();
    services.AddScoped<ProductService>();
    // ...
    
    // Infrastructure
    services.AddSingleton<EventBus>();
    services.AddSingleton<MemoryCacheProvider>();
    services.AddScoped<IUnitOfWork, DbContextProvider>();
    
    // Middleware
    services.AddSingleton<ErrorHandlingMiddleware>();
    services.AddSingleton<LoggingMiddleware>();
    // ...
}
```

## Entity Lifecycle

### User Entity Lifecycle

```
New Instance
    ↓
Validation (email format, password strength)
    ↓
Service.CreateAsync()
    ↓
Persisted to Database (CreatedAt timestamp)
    ↓
Cached in Memory
    ↓
Active State (IsActive = true)
    ↓
Operations: Read, Update, Delete
    ↓
Soft Delete (IsDeleted = true, UpdatedAt set)
    ↓
Permanently Deleted (removed from database)
    ↓
History in AuditLog
```

## Transaction Handling

### Implicit Transactions

```csharp
using (var scope = serviceProvider.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    
    // Each service call is auto-committed
    var user = await userService.CreateAsync(userEntity);
    var product = await productService.CreateAsync(productEntity);
    // Both committed independently
}
```

### Explicit Transactions

```csharp
var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

using (var transaction = unitOfWork.BeginTransaction())
{
    try
    {
        var user = await userService.CreateAsync(userEntity);
        var product = await productService.CreateAsync(productEntity);
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## Error Handling

### Exception Hierarchy

```
Exception
├── RepositoryException
│   └── Data access errors
├── ValidationException
│   └── Business rule violations
├── GenerationException
│   └── Code generation errors
└── System exceptions
    └── Database/IO errors
```

### Error Handling Flow

```
Operation
    ↓
Try block
    ↓
Catch specific exceptions
    ├─ RepositoryException: Log and return error
    ├─ ValidationException: Return validation errors
    └─ GenerationException: Log and rethrow
    ↓
Finally: Cleanup resources
    ↓
Return result or throw
```

## Performance Considerations

### Caching Strategy

```
Request
    ↓
Check cache
    ├─ Hit (TTL not expired)
    │  └─ Return cached value
    └─ Miss
        ↓
        Query database
        ↓
        Store in cache (60 min default TTL)
        ↓
        Return value

On Update:
    ↓
    Invalidate cache for entity
    ↓
    Update database
    ↓
    Cache rebuilt on next read
```

### Query Optimization

1. **Use FindAsync with predicates** - Filter at database level
2. **Pagination** - Load data in chunks
3. **Caching** - Reduce repeated queries
4. **Connection pooling** - Reuse connections
5. **Indexes** - On frequently queried columns

### Scaling Considerations

- **Vertical Scaling**: Increase cache, database connection pool
- **Read Replicas**: Multiple read-only database copies
- **Sharding**: Partition data by entity ID
- **Microservices**: Separate services per bounded context
