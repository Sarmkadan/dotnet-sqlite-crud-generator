// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

Complete API documentation for SQLite CRUD Generator.

## IRepository<T>

Base interface for all repository operations.

```csharp
public interface IRepository<T> where T : class
{
    /// <summary>Adds entity to database</summary>
    Task<T> AddAsync(T entity);
    
    /// <summary>Gets entity by primary key</summary>
    Task<T> GetByIdAsync(int id);
    
    /// <summary>Gets all entities</summary>
    Task<List<T>> GetAllAsync();
    
    /// <summary>Updates existing entity</summary>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>Soft deletes entity</summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>Finds entities matching predicate</summary>
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
```

## IService<T>

Interface for service layer operations.

```csharp
public interface IService<T> where T : class
{
    Task<T> CreateAsync(T entity);
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<(List<T>, int)> GetPagedAsync(int pageNumber, int pageSize);
}
```

## IUnitOfWork

Manages transactions and repository coordination.

```csharp
public interface IUnitOfWork
{
    /// <summary>Begins a new database transaction</summary>
    ITransaction BeginTransaction();
    
    /// <summary>Saves all changes to database</summary>
    Task<bool> SaveAsync();
    
    /// <summary>Disposes transaction and releases resources</summary>
    void Dispose();
}
```

## UserService

User management service.

### Methods

#### CreateAsync
```csharp
Task<User> CreateAsync(User entity)
```

Creates a new user.

**Parameters**:
- `entity` (User): User to create

**Returns**: Created user with ID

**Throws**: ValidationException if invalid

**Example**:
```csharp
var user = new User
{
    Username = "john.doe",
    Email = "john@example.com",
    FirstName = "John",
    LastName = "Doe",
    PasswordHash = "hashed_password"
};

var created = await userService.CreateAsync(user);
// created.Id will be set
```

#### GetByIdAsync
```csharp
Task<User> GetByIdAsync(int id)
```

Gets user by ID.

**Parameters**:
- `id` (int): User ID

**Returns**: User or null if not found

**Example**:
```csharp
var user = await userService.GetByIdAsync(1);
if (user != null)
    Console.WriteLine($"Found: {user.Username}");
```

#### GetAllAsync
```csharp
Task<List<User>> GetAllAsync()
```

Gets all users.

**Returns**: List of all users

**Example**:
```csharp
var allUsers = await userService.GetAllAsync();
Console.WriteLine($"Total users: {allUsers.Count}");
```

#### UpdateAsync
```csharp
Task<User> UpdateAsync(User entity)
```

Updates existing user.

**Parameters**:
- `entity` (User): User with updated values

**Returns**: Updated user

**Example**:
```csharp
user.Email = "newemail@example.com";
user = await userService.UpdateAsync(user);
```

#### DeleteAsync
```csharp
Task<bool> DeleteAsync(int id)
```

Soft deletes user (marks as deleted).

**Parameters**:
- `id` (int): User ID

**Returns**: True if deleted, false if not found

**Example**:
```csharp
var deleted = await userService.DeleteAsync(1);
if (deleted)
    Console.WriteLine("User deleted");
```

#### FindAsync
```csharp
Task<List<User>> FindAsync(Expression<Func<User, bool>> predicate)
```

Finds users matching criteria.

**Parameters**:
- `predicate` (Expression): LINQ predicate

**Returns**: Matching users

**Example**:
```csharp
var activeUsers = await userService.FindAsync(u => u.IsActive);
var withEmail = await userService.FindAsync(u => u.Email.Contains("@example.com"));
```

#### GetPagedAsync
```csharp
Task<(List<User>, int)> GetPagedAsync(int pageNumber, int pageSize)
```

Gets paginated users.

**Parameters**:
- `pageNumber` (int): Page number (1-based)
- `pageSize` (int): Records per page

**Returns**: Tuple of (users, totalCount)

**Example**:
```csharp
var (users, total) = await userService.GetPagedAsync(1, 10);
Console.WriteLine($"Page 1 of {Math.Ceiling((double)total / 10)}");
```

#### AuthenticateAsync
```csharp
Task<User> AuthenticateAsync(string username, string password)
```

Authenticates user credentials.

**Parameters**:
- `username` (string): Username
- `password` (string): Password (will be hashed)

**Returns**: Authenticated user or null

**Example**:
```csharp
var user = await userService.AuthenticateAsync("john.doe", "password");
if (user != null)
    Console.WriteLine("Authentication successful");
```

## ProductService

Product management service.

### Methods

#### CreateAsync
```csharp
Task<Product> CreateAsync(Product entity)
```

Creates new product.

**Example**:
```csharp
var product = new Product
{
    Name = "Laptop",
    Description = "Gaming laptop",
    Price = 1299.99m,
    StockQuantity = 50
};

var created = await productService.CreateAsync(product);
```

#### GetByIdAsync
```csharp
Task<Product> GetByIdAsync(int id)
```

Gets product by ID.

#### GetAllAsync
```csharp
Task<List<Product>> GetAllAsync()
```

Gets all products.

#### UpdateAsync
```csharp
Task<Product> UpdateAsync(Product entity)
```

Updates product.

#### DeleteAsync
```csharp
Task<bool> DeleteAsync(int id)
```

Deletes product.

#### FindAsync
```csharp
Task<List<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
```

Finds products by criteria.

**Example**:
```csharp
var expensive = await productService.FindAsync(p => p.Price > 1000);
var lowStock = await productService.FindAsync(p => p.StockQuantity < 10);
```

#### GetPagedAsync
```csharp
Task<(List<Product>, int)> GetPagedAsync(int pageNumber, int pageSize)
```

Gets paginated products.

#### CalculateTotalValueAsync
```csharp
Task<decimal> CalculateTotalValueAsync()
```

Calculates total inventory value.

**Returns**: Sum of (price × quantity) for all products

**Example**:
```csharp
var totalValue = await productService.CalculateTotalValueAsync();
Console.WriteLine($"Inventory value: ${totalValue:F2}");
```

#### GetByPriceRangeAsync
```csharp
Task<List<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
```

Gets products within price range.

**Example**:
```csharp
var affordable = await productService.GetByPriceRangeAsync(10m, 100m);
```

## OrderService

Order management service.

### Methods

#### CreateAsync
```csharp
Task<Order> CreateAsync(Order entity)
```

Creates new order.

#### GetByIdAsync
```csharp
Task<Order> GetByIdAsync(int id)
```

Gets order by ID.

#### GetAllAsync
```csharp
Task<List<Order>> GetAllAsync()
```

Gets all orders.

#### UpdateAsync
```csharp
Task<Order> UpdateAsync(Order entity)
```

Updates order.

#### DeleteAsync
```csharp
Task<bool> DeleteAsync(int id)
```

Deletes order.

#### FindAsync
```csharp
Task<List<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
```

Finds orders by criteria.

#### GetPagedAsync
```csharp
Task<(List<Order>, int)> GetPagedAsync(int pageNumber, int pageSize)
```

Gets paginated orders.

#### GetUserOrdersAsync
```csharp
Task<List<Order>> GetUserOrdersAsync(int userId)
```

Gets all orders for a user.

**Example**:
```csharp
var userOrders = await orderService.GetUserOrdersAsync(1);
```

#### GetUserTotalSpentAsync
```csharp
Task<decimal> GetUserTotalSpentAsync(int userId)
```

Calculates total amount spent by user.

**Example**:
```csharp
var spent = await orderService.GetUserTotalSpentAsync(1);
Console.WriteLine($"User spent: ${spent:F2}");
```

## Models

### User

```csharp
public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

### Product

```csharp
public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? Sku { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Order

```csharp
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Category

```csharp
public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### AuditLog

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public required string EntityName { get; set; }
    public int EntityId { get; set; }
    public required string OperationType { get; set; }
    public string? ChangedProperties { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Exceptions

### RepositoryException

Thrown for data access errors.

```csharp
try
{
    var user = await userService.GetByIdAsync(invalid);
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### ValidationException

Thrown for invalid data.

```csharp
try
{
    var user = new User { /* invalid */ };
    await userService.CreateAsync(user);
}
catch (ValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

### GenerationException

Thrown for code generation errors.

```csharp
try
{
    await generationService.GenerateMigrationsAsync();
}
catch (GenerationException ex)
{
    Console.WriteLine($"Generation error: {ex.Message}");
}
```

## Enums

### EntityStatus

```csharp
public enum EntityStatus
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}
```

### OperationType

```csharp
public enum OperationType
{
    Create,
    Update,
    Delete,
    Restore
}
```

## Configuration

### DatabaseSettings

```csharp
public class DatabaseSettings
{
    public string FilePath { get; set; }
    public int ConnectionTimeout { get; set; } = 30;
    public int MaxPoolSize { get; set; } = 10;
    
    public string ConnectionString =>
        $"Data Source={FilePath};Pooling=true;Max Pool Size={MaxPoolSize};";
    
    public bool Validate() => /* validation logic */;
}
```

### CacheConfiguration

```csharp
public class CacheConfiguration
{
    public bool Enabled { get; set; } = true;
    public int DefaultExpirationMinutes { get; set; } = 60;
    public int SlidingExpirationMinutes { get; set; } = 30;
}
```

## Utilities

### StringExtensions

```csharp
public static class StringExtensions
{
    public static string ToSnakeCase(this string str);
    public static string ToCamelCase(this string str);
    public static string ToPascalCase(this string str);
    public static bool IsValidEmail(this string str);
}
```

### DateTimeExtensions

```csharp
public static class DateTimeExtensions
{
    public static bool IsToday(this DateTime dateTime);
    public static bool IsThisMonth(this DateTime dateTime);
    public static bool IsThisYear(this DateTime dateTime);
    public static TimeSpan DaysSince(this DateTime dateTime);
}
```

### ReflectionHelper

```csharp
public static class ReflectionHelper
{
    public static List<PropertyInfo> GetPublicProperties(Type type);
    public static object? GetPropertyValue(object obj, string propertyName);
    public static void SetPropertyValue(object obj, string propertyName, object? value);
}
```

## Attributes

### GenerateGrpcAttribute

Marks a model class for gRPC service generation.

```csharp
[GenerateGrpc]
public class User
{
    // Properties
}
```

This generates `.proto` definition for the User model.
