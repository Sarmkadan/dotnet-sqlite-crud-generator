# SoftDeleteOptions

`SoftDeleteOptions` provides the configuration necessary to enable and manage soft-delete functionality within generated CRUD operations. By defining a specific database column and mapping integer values for active and deleted states, it allows database interactions to filter out logically deleted records and perform updates to mark records as deleted rather than performing a physical removal.

## API

*   **`Enabled`** (`bool`)
    Gets or sets a value indicating whether soft-delete functionality is active for the entity. When false, standard physical deletion behavior is used.

*   **`ColumnName`** (`string`)
    Gets or sets the name of the database column used to track the deletion status of the row.

*   **`DeletedValue`** (`int`)
    Gets or sets the integer value that represents a deleted entity in the database.

*   **`ActiveValue`** (`int`)
    Gets or sets the integer value that represents an active entity in the database.

*   **`GetWhereClause`** (`string`)
    Returns the SQL fragment required for a `WHERE` clause to filter out records marked as deleted (e.g., `[ColumnName] = [ActiveValue]`).

*   **`GetSetClause`** (`string`)
    Returns the SQL fragment required for an `UPDATE` statement to mark a record as deleted (e.g., `[ColumnName] = [DeletedValue]`).

*   **`Validate()`** (`void`)
    Performs validation on the configuration. If `Enabled` is set to true, it verifies that `ColumnName` is provided and that `ActiveValue` and `DeletedValue` are defined. Throws an `InvalidOperationException` if the configuration is invalid.

## Usage

### Configuring Options

```csharp
var options = new SoftDeleteOptions
{
    Enabled = true,
    ColumnName = "IsDeleted",
    ActiveValue = 0,
    DeletedValue = 1
};
```

### Validating Configuration

```csharp
var options = new SoftDeleteOptions { Enabled = true };

try
{
    options.Validate();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## Notes

*   **Thread-Safety**: This class is intended to be used as a configuration object and is not inherently thread-safe for concurrent read/write access. It should be configured before being passed to service or generator instances.
*   **Edge Cases**: Ensure that `ActiveValue` and `DeletedValue` are distinct; otherwise, filtering and update operations will fail to behave as intended. If `Enabled` is set to `true`, the `Validate` method must be invoked to ensure database compatibility. Invalid or SQL-reserved keywords used in `ColumnName` may result in generated SQL syntax errors.
