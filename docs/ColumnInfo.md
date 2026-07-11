# ColumnInfo

`ColumnInfo` is an immutable record that represents the schema metadata of a single database column. It is the fundamental unit used by `MigrationDiffService` to compare expected schema definitions against the actual structure of a SQLite database, enabling automatic generation of migration scripts.

## API

### ColumnInfo

A sealed record describing a column’s name, type, nullability, default value, and primary-key status. Instances are compared by value to detect schema changes.

**Members (compiler-generated for records):**
- Equality members (`Equals`, `GetHashCode`, `==`, `!=`) perform structural comparison.
- `ToString()` returns a formatted representation of all properties.

---

### ColumnDiff

A sealed record that captures the difference between two `ColumnInfo` instances. It indicates whether a column was added, removed, or modified, and carries the old and new values when a modification is detected.

---

### TableDiff

A sealed record representing the aggregated differences for a single table. It contains the table name and a collection of `ColumnDiff` entries for each column that has changed.

---

### MigrationDiff

A sealed record that holds the complete set of schema differences across all tables. It is the top-level result produced by `ComputeDiffAsync` and contains a collection of `TableDiff` records.

---

### MigrationDiffService

A service class responsible for computing schema differences between an expected model and the live database.

---

### MigrationDiffService.ComputeDiffAsync

```csharp
public async Task<MigrationDiff> ComputeDiffAsync()
```

Computes the full `MigrationDiff` by comparing the expected schema (obtained from `GetExpectedSchema`) against the actual schema (retrieved asynchronously via `GetActualSchemaAsync`).

**Returns:** A `MigrationDiff` containing all detected table and column differences.

**Exceptions:** May throw if the underlying database connection fails or if the actual schema cannot be retrieved (e.g., connection timeout, invalid connection string, or the database file is locked).

---

### MigrationDiffService.GetActualSchemaAsync

```csharp
public async Task<Dictionary<string, ColumnInfo>> GetActualSchemaAsync()
```

Asynchronously queries the connected SQLite database for its current schema and returns it as a dictionary keyed by fully qualified column identifiers.

**Returns:** A dictionary where each key uniquely identifies a column (typically `TableName.ColumnName`) and each value is a `ColumnInfo` representing the column’s live definition.

**Exceptions:** Throws if the database connection is not open, the connection string is invalid, or a query execution error occurs (e.g., attempting to read from a malformed or corrupt database).

---

### MigrationDiffService.GetExpectedSchema

```csharp
public Dictionary<string, ColumnInfo> GetExpectedSchema
```

A property (or parameterless method returning a dictionary) that provides the authoritative expected schema, typically derived from entity models, attributes, or configuration. It serves as the baseline for comparison.

**Returns:** A dictionary mapping column identifiers to their expected `ColumnInfo` definitions.

**Exceptions:** Implementation-defined; may throw if the expected schema source is misconfigured or inaccessible.

## Usage

### Example 1: Basic schema comparison and migration generation

```csharp
var service = new MigrationDiffService(connectionString);
MigrationDiff diff = await service.ComputeDiffAsync();

foreach (var tableDiff in diff.TableDiffs)
{
    Console.WriteLine($"Table: {tableDiff.TableName}");
    foreach (var columnDiff in tableDiff.ColumnDiffs)
    {
        Console.WriteLine($"  Column change: {columnDiff.ChangeType}");
        if (columnDiff.OldColumn is not null)
            Console.WriteLine($"    Old: {columnDiff.OldColumn}");
        if (columnDiff.NewColumn is not null)
            Console.WriteLine($"    New: {columnDiff.NewColumn}");
    }
}
```

### Example 2: Inspecting actual schema before comparison

```csharp
var service = new MigrationDiffService(connectionString);

// Retrieve the live schema for diagnostics
Dictionary<string, ColumnInfo> actualSchema = await service.GetActualSchemaAsync();
foreach (var kvp in actualSchema)
{
    Console.WriteLine($"{kvp.Key}: Type={kvp.Value.DataType}, Nullable={kvp.Value.IsNullable}");
}

// Retrieve the expected schema
Dictionary<string, ColumnInfo> expectedSchema = service.GetExpectedSchema;

// Compute and process differences
MigrationDiff diff = await service.ComputeDiffAsync();
// ... handle diff
```

## Notes

- All records (`ColumnInfo`, `ColumnDiff`, `TableDiff`, `MigrationDiff`) are immutable and perform value-based equality checks. They are safe to share across threads without synchronization.
- `GetActualSchemaAsync` accesses the database asynchronously; callers should ensure the connection is properly opened and that concurrent writes do not interfere with schema reads.
- `ComputeDiffAsync` combines results from `GetExpectedSchema` and `GetActualSchemaAsync`. If either source fails, the exception propagates immediately.
- The dictionary keys used by both schema methods are expected to follow a consistent convention (e.g., `TableName.ColumnName`). Mismatched key formats will cause columns to be reported as added or removed rather than modified.
- `MigrationDiffService` itself is not guaranteed to be thread-safe unless its implementation explicitly synchronizes access to shared state such as the database connection.
