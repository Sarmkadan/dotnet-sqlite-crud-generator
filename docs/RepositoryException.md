# RepositoryException

`RepositoryException` is a custom exception type designed to represent data-access failures within the repository layer. It extends the standard `Exception` class and enriches error context by optionally carrying the affected entity type and identifier. Factory methods provide strongly-typed, consistent instances for common failure modes such as missing entities, duplicate keys, and constraint violations.

## API

### Constructors

- **`RepositoryException(string message)`**  
  Initializes a new instance with the specified error message. The `EntityType` and `EntityId` properties remain `null`.  
  *Parameters*: `message` – the message that describes the error.  
  *Return value*: a new `RepositoryException` instance.  
  *Throws*: nothing (constructor).

### Properties

- **`EntityType`** (`string?`)  
  Gets the name of the entity type associated with the failure, or `null` if no entity type was specified.

- **`EntityId`** (`int?`)  
  Gets the identifier of the entity instance associated with the failure, or `null` if no identifier was specified.

### Static Factory Members

- **`EntityNotFound`** (`static RepositoryException`)  
  Returns a pre-configured exception indicating that a requested entity could not be found. The returned instance carries a default message; `EntityType` and `EntityId` are `null` and should be set by the caller when context is available.

- **`DuplicateKey`** (`static RepositoryException`)  
  Returns a pre-configured exception indicating that an operation failed because of a duplicate key conflict. The returned instance carries a default message; `EntityType` and `EntityId` are `null` and should be set by the caller when context is available.

- **`ConstraintViolation`** (`static RepositoryException`)  
  Returns a pre-configured exception indicating that a database constraint was violated. The returned instance carries a default message; `EntityType` and `EntityId` are `null` and should be set by the caller when context is available.

## Usage

### Example 1: Throwing for a missing entity

```csharp
public async Task<Product> GetProductByIdAsync(int id, CancellationToken ct)
{
    var product = await _db.Products.FindAsync(new object[] { id }, ct);
    if (product is null)
    {
        var ex = RepositoryException.EntityNotFound;
        ex.EntityType = nameof(Product);
        ex.EntityId = id;
        throw ex;
    }
    return product;
}
```

### Example 2: Handling a duplicate key during insert

```csharp
public async Task InsertCustomerAsync(Customer customer, CancellationToken ct)
{
    try
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException dbEx) when (dbEx.InnerException is SqliteException sqlEx
                                         && sqlEx.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
    {
        var repoEx = RepositoryException.DuplicateKey;
        repoEx.EntityType = nameof(Customer);
        throw repoEx;
    }
}
```

## Notes

- The static factory members (`EntityNotFound`, `DuplicateKey`, `ConstraintViolation`) return **the same pre-allocated instance** on every access. Modifying `EntityType` or `EntityId` on these instances is not thread-safe and will affect all consumers that reference them concurrently. To avoid shared-state issues, clone or re-instantiate before mutating properties in multi-threaded scenarios.
- `EntityType` and `EntityId` are writable after construction. Callers are responsible for populating them when context is available; they are not automatically inferred.
- The exception message set by the static factories is generic. For richer diagnostics, prefer the constructor overload and supply a detailed message that incorporates the entity type and identifier directly.
- This type does not carry the original database exception as an inner exception. When wrapping provider-specific errors, capture the original exception via the `InnerException` property of the base `Exception` class if needed for upstream logging or diagnostics.
