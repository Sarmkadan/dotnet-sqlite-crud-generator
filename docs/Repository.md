# Repository

Generic asynchronous repository for performing CRUD operations against a SQLite database using Entity Framework Core. Designed to abstract common data access patterns while maintaining flexibility for entity-specific behavior through virtual methods. Supports both single-entity and batch operations with transactional consistency.

## API

### `public virtual async Task<T?> GetByIdAsync(int id)`

Retrieves an entity by its primary key. Returns `null` if the entity does not exist.

- **Parameters**:
  - `id` – The primary key value of the entity to retrieve.
- **Return value**:
  - `Task<T?>` – The entity instance if found; otherwise, `null`.
- **Exceptions**:
  - Throws `ArgumentException` if `id` is negative or zero.
  - Throws `DbUpdateException` if a database-level error occurs.

---

### `public virtual async Task<IEnumerable<T>> GetAllAsync()`

Retrieves all entities of type `T` from the database.

- **Return value**:
  - `Task<IEnumerable<T>>` – An enumerable sequence of all entities.
- **Exceptions**:
  - Throws `InvalidOperationException` if the underlying `DbContext` is disposed.
  - Throws `DbUpdateException` if a database-level error occurs.

---

### `public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)`

Retrieves entities matching the specified predicate.

- **Parameters**:
  - `predicate` – A lambda expression defining the filter condition.
- **Return value**:
  - `Task<IEnumerable<T>>` – An enumerable sequence of matching entities.
- **Exceptions**:
  - Throws `ArgumentNullException` if `predicate` is `null`.
  - Throws `DbUpdateException` if a database-level error occurs.

---

### `public virtual async Task<int> CountAsync()`

Returns the total number of entities of type `T` in the database.

- **Return value**:
  - `Task<int>` – The count of entities.
- **Exceptions**:
  - Throws `InvalidOperationException` if the underlying `DbContext` is disposed.
  - Throws `DbUpdateException` if a database-level error occurs.

---

### `public virtual async Task<T> AddAsync(T entity)`

Adds a new entity to the database.

- **Parameters**:
  - `entity` – The entity to add.
- **Return value**:
  - `Task<T>` – The added entity, including any database-generated values (e.g., auto-increment IDs).
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is `null`.
  - Throws `DbUpdateException` if a constraint violation or other database error occurs.

---

### `public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)`

Adds multiple entities to the database in a single operation.

- **Parameters**:
  - `entities` – An enumerable sequence of entities to add.
- **Return value**:
  - `Task<IEnumerable<T>>` – The added entities, including any database-generated values.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entities` is `null`.
  - Throws `ArgumentException` if `entities` contains a `null` element.
  - Throws `DbUpdateException` if a constraint violation or other database error occurs.

---
### `public virtual async Task<bool> UpdateAsync(T entity)`

Updates an existing entity in the database.

- **Parameters**:
  - `entity` – The entity to update, identified by its primary key.
- **Return value**:
  - `Task<bool>` – `true` if the entity was found and updated; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is `null`.
  - Throws `DbUpdateException` if a database-level error occurs.

---
### `public virtual async Task<bool> DeleteAsync(T entity)`

Deletes the specified entity from the database.

- **Parameters**:
  - `entity` – The entity to delete.
- **Return value**:
  - `Task<bool>` – `true` if the entity was found and deleted; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `entity` is `null`.
  - Throws `DbUpdateException` if a database-level error occurs.

---
### `public virtual async Task<bool> DeleteAsync(int id)`

Deletes the entity with the specified primary key.

- **Parameters**:
  - `id` – The primary key of the entity to delete.
- **Return value**:
  - `Task<bool>` – `true` if the entity was found and deleted; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentException` if `id` is negative or zero.
  - Throws `DbUpdateException` if a database-level error occurs.

---
### `public virtual async Task<int> DeleteRangeAsync(IEnumerable<int> ids)`

Deletes multiple entities by their primary keys in a single operation.

- **Parameters**:
  - `ids` – An enumerable sequence of primary key values.
- **Return value**:
  - `Task<int>` – The number of entities successfully deleted.
- **Exceptions**:
  - Throws `ArgumentNullException` if `ids` is `null`.
  - Throws `ArgumentException` if any `id` is negative or zero.
  - Throws `DbUpdateException` if a database-level error occurs.

---
### `public virtual async Task<bool> ExistsAsync(int id)`

Checks whether an entity with the specified primary key exists.

- **Parameters**:
  - `id` – The primary key to check.
- **Return value**:
  - `Task<bool>` – `true` if the entity exists; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentException` if `id` is negative or zero.
  - Throws `DbUpdateException` if a database-level error occurs.

---
### `public virtual async Task<int> SaveChangesAsync()`

Persists all pending changes to the database.

- **Return value**:
  - `Task<int>` – The number of state entries written to the database.
- **Exceptions**:
  - Throws `DbUpdateException` if any constraints are violated or other database errors occur.
  - Throws `DbUpdateConcurrencyException` if a concurrency conflict is detected.

---

## Usage

### Example 1: Basic CRUD Operations
