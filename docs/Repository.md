# Repository

Generic abstract repository implementation for SQLite with CRUD operations, caching, and logging. Provides a base implementation for data access patterns with automatic caching, transaction support, and SQLite-specific optimizations.

## Type Parameters

- `T`: The entity type (must be a reference type)
- `TKey`: The type of the entity's primary key

## API

### Protected Constructor

#### `protected Repository(DatabaseConnection database, ILogger<Repository<T, TKey>>? logger = null)`

Initializes a new instance of the repository.

- **Parameters**:
  - `database` – The database connection to use for SQLite operations.
  - `logger` – Optional logger for diagnostic information.
- **Exceptions**:
  - Throws `ArgumentNullException` if `database` is null.

---

### `public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)`

Retrieves an entity by its ID from the database with caching support.

- **Parameters**:
  - `id` – The ID of the entity to retrieve.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<T?>` – The entity if found; otherwise, `null`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `id` is null.
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)`

Retrieves all entities from the database with automatic caching.

- **Parameters**:
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<IEnumerable<T>>` – A collection of all entities.
- **Exceptions**:
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)`

Finds entities matching the specified predicate.

- **Parameters**:
  - `predicate` – The function to test each element for a condition.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<IEnumerable<T>>` – A collection of entities that match the predicate.
- **Exceptions**:
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<int> CountAsync(Func<T, bool>? predicate = null, CancellationToken cancellationToken = default)`

Counts entities in the database, optionally filtered by a predicate.

- **Parameters**:
  - `predicate` – The function to test each element for a condition. If null, counts all entities.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<int>` – The number of entities matching the predicate.
- **Exceptions**:
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)`

Adds a new entity to the database and populates its ID property.

- **Parameters**:
  - `entity` – The entity to add.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<T>` – The added entity with its ID populated.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is null.
  - Throws `RepositoryException` when a database error occurs.
  - Throws `RepositoryException.DuplicateKey` for constraint violations.

---

### `public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)`

Adds a collection of entities to the database in a single transaction.

- **Parameters**:
  - `entities` – The collection of entities to add.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<IEnumerable<T>>` – The added entities with their IDs populated.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entities` is null.
  - Throws `RepositoryException` when a database error occurs.
  - Throws `RepositoryException.DuplicateKey` for constraint violations.

---

### `public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)`

Updates an existing entity in the database.

- **Parameters**:
  - `entity` – The entity to update.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<bool>` – True if the entity was updated; otherwise, false.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is null.
  - Throws `RepositoryException` when a database error occurs.
  - Throws `RepositoryException.EntityNotFound` if the entity doesn't exist.

---

### `public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)`

Deletes an entity with the specified ID from the database.

- **Parameters**:
  - `id` – The ID of the entity to delete.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<bool>` – True if the entity was deleted; otherwise, false.
- **Exceptions**:
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)`

Deletes an entity from the database by entity reference.

- **Parameters**:
  - `entity` – The entity to delete.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<bool>` – True if the entity was deleted; otherwise, false.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is null.
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)`

Deletes a collection of entities with the specified IDs from the database.

- **Parameters**:
  - `ids` – The collection of entity IDs to delete.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<int>` – The number of entities that were deleted.
- **Exceptions**:
  - Throws `ArgumentNullException` if `ids` is null.
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)`

Determines whether an entity with the specified ID exists in the database.

- **Parameters**:
  - `id` – The ID of the entity to check.
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<bool>` – True if the entity exists; otherwise, false.
- **Exceptions**:
  - Throws `RepositoryException` when a database error occurs.

---

### `public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)`

Gets the number of entities currently in the cache.

- **Parameters**:
  - `cancellationToken` – A token to cancel the operation.
- **Return value**:
  - `Task<int>` – The number of entities in the cache.

## Usage

### Example 1: Creating a Concrete Repository

```csharp
public class UserRepository : Repository<User, int>
{
    public UserRepository(DatabaseConnection database, ILogger<UserRepository>? logger = null)
        : base(database, logger)
    {
        // Optional: Override table name or primary key column
        // _tableName = "Users";
        // _primaryKeyColumn = "UserId";
    }
    
    // Optional: Override methods for custom behavior
    // public override async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    // {
    //     // Custom logic before adding
    //     return await base.AddAsync(entity, cancellationToken);
    // }
}
```

### Example 2: Using the Repository

```csharp
// Setup
var connection = new DatabaseConnection("Data Source=app.db");
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var userRepository = new UserRepository(connection, loggerFactory.CreateLogger<UserRepository>);

// Add a new user
var newUser = new User { Name = "John Doe", Email = "john@example.com" };
var addedUser = await userRepository.AddAsync(newUser);
Console.WriteLine($"Added user with ID: {addedUser.Id}");

// Retrieve user by ID
var user = await userRepository.GetByIdAsync(addedUser.Id);
if (user != null)
{
    Console.WriteLine($"Retrieved user: {user.Name}");
}

// Update user
user.Name = "Jane Doe";
await userRepository.UpdateAsync(user);

// Query users
var users = await userRepository.GetAllAsync();
Console.WriteLine($"Total users: {users.Count()}");

// Delete user
await userRepository.DeleteAsync(addedUser.Id);
```

### Example 3: With Custom Table Name

```csharp
public class ProductRepository : Repository<Product, Guid>
{
    public ProductRepository(DatabaseConnection database, ILogger<ProductRepository>? logger = null)
        : base(database, logger)
    {
        // Customize table name if it doesn't follow the default pattern
        _tableName = "Products"; // Instead of "Products" (default would be "Products" anyway)
        
        // Customize primary key column if it's not "Id"
        // _primaryKeyColumn = "ProductId";
    }
}

// Usage remains the same
var productRepo = new ProductRepository(connection);
var product = new Product { Name = "Laptop", Price = 999.99m };
var savedProduct = await productRepo.AddAsync(product);
```

## Implementation Details

### Caching Mechanism

The repository implements a two-level caching system:
1. **Static caches** (`_propertiesCache` and `_idPropertyCache`) store reflection metadata across all instances of the same generic type
2. **Instance cache** (`_cache`) stores entity instances to reduce database reads

Cache invalidation occurs automatically on insert, update, and delete operations.

### SQLite Specific Features

- Uses `Microsoft.Data.Sqlite` for SQLite operations
- Handles SQLite-specific error codes (e.g., constraint error code 19)
- Implements proper parameter handling for SQLite queries
- Uses `LastInsertRowId` to retrieve auto-generated IDs

### Thread Safety

The repository is thread-safe for concurrent read operations. Write operations are synchronized using locks on the cache.

### Logging

All operations log debug and information messages when a logger is provided. Errors are logged with full exception details.

## Notes

- The repository assumes entities have a property named "Id" as the primary key by default
- Entity properties must have both getters and setters to be included in CRUD operations
- DateTime properties are stored and retrieved in ISO 8601 format ("O")
- Nullable types are properly handled for database null values
- The repository is designed to be inherited; override methods to customize behavior