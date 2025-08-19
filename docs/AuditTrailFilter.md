# AuditTrailFilter

The `AuditTrailFilter` class serves as a composite data transfer object and service facade within the `dotnet-sqlite-crud-generator` project, designed to encapsulate query parameters for auditing operations while exposing direct methods to interact with the underlying `AuditTrailService`. It aggregates filtering criteria such as entity type, operation type, date ranges, and user identifiers to refine audit log retrieval, while simultaneously providing convenience methods for recording events, querying historical data, and managing log retention. The class combines stateful filter properties with stateless service actions, allowing consumers to configure a filter context and immediately execute relevant audit operations without manually instantiating or injecting the service dependency.

## API

### Properties

*   **`public string? EntityType`**
    Specifies the fully qualified name or identifier of the entity type to filter audit logs. When null, the filter applies to all entity types.

*   **`public int? EntityId`**
    Represents the specific primary key of the entity instance to filter. Used in conjunction with `EntityType` to isolate changes to a single record.

*   **`public int? ChangedByUserId`**
    Filters audit entries based on the ID of the user who performed the action. If null, changes by all users are included.

*   **`public OperationType? OperationType`**
    Restricts results to a specific operation category (e.g., Create, Update, Delete). The `OperationType` is an enumeration defined elsewhere in the project.

*   **`public DateTime? From`**
    Defines the start of the time window for the query. Only entries created on or after this timestamp are included.

*   **`public DateTime? To`**
    Defines the end of the time window for the query. Only entries created on or before this timestamp are included.

*   **`public int Limit`**
    Sets the maximum number of records to return in list-based queries. This property is typically used by `QueryAsync` and related methods to paginate results.

*   **`public int TotalEntries`**
    Represents the total count of audit entries matching the current filter criteria. This property is usually populated after executing a count operation or a query that calculates the total set size.

*   **`public Dictionary<string, int> ByOperation`**
    Contains an aggregation of audit entries grouped by operation type. The key is the operation name (string), and the value is the count (int).

*   **`public Dictionary<string, int> ByEntityType`**
    Contains an aggregation of audit entries grouped by entity type. The key is the entity name (string), and the value is the count (int).

*   **`public DateTime? OldestEntry`**
    Stores the timestamp of the earliest audit log entry matching the current filter criteria.

*   **`public DateTime? NewestEntry`**
    Stores the timestamp of the most recent audit log entry matching the current filter criteria.

*   **`public AuditTrailService AuditTrailService`**
    Exposes the underlying service instance responsible for executing database operations. This property allows direct access to the service if specialized methods not wrapped by this filter class are required.

### Methods

*   **`public async Task RecordAsync`**
    Records a new audit log entry using the context defined in the filter properties (such as `EntityType`, `EntityId`, and `ChangedByUserId`).
    *   **Parameters:** None (relies on instance property state).
    *   **Return Value:** A `Task` representing the asynchronous operation.
    *   **Exceptions:** Throws if the database connection is unavailable or if required fields for the log entry are missing in the instance state.

*   **`public Task RecordAsync<T>`**
    Generic overload for recording an audit log entry, inferring the `EntityType` from the generic type parameter `T`.
    *   **Parameters:** None explicit; uses the generic type `T` to resolve entity metadata.
    *   **Return Value:** A `Task` representing the asynchronous operation.
    *   **Exceptions:** Throws if type `T` is not tracked by the audit configuration or if database write fails.

*   **`public async Task<IReadOnlyList<AuditLog>> QueryAsync`**
    Executes a query against the audit store using all populated filter properties (`From`, `To`, `OperationType`, etc.).
    *   **Parameters:** None (uses instance property state).
    *   **Return Value:** A read-only list of `AuditLog` objects matching the criteria, capped by the `Limit` property.
    *   **Exceptions:** Throws on database read errors or if the underlying SQLite connection is locked.

*   **`public Task<IReadOnlyList<AuditLog>> GetEntityTrailAsync`**
    Retrieves the complete history of changes for a specific entity instance defined by `EntityType` and `EntityId`.
    *   **Parameters:** None (relies on `EntityType` and `EntityId` properties).
    *   **Return Value:** A chronologically ordered read-only list of `AuditLog` entries for the specified entity.
    *   **Exceptions:** Throws if `EntityType` or `EntityId` is null.

*   **`public Task<IReadOnlyList<AuditLog>> GetUserTrailAsync`**
    Retrieves all audit logs associated with a specific user defined by `ChangedByUserId`.
    *   **Parameters:** None (relies on `ChangedByUserId` property).
    *   **Return Value:** A read-only list of `AuditLog` entries performed by the specified user.
    *   **Exceptions:** Throws if `ChangedByUserId` is null.

*   **`public Task<IReadOnlyList<AuditLog>> GetRecentAsync`**
    Fetches the most recent audit entries regardless of specific entity or user, constrained only by the `Limit` property.
    *   **Parameters:** None.
    *   **Return Value:** A read-only list of the latest `AuditLog` entries.
    *   **Exceptions:** Throws on database access failure.

*   **`public async Task<int> PurgeAsync`**
    Permanently deletes audit log entries that match the current filter criteria (often used with `To` date to archive old logs).
    *   **Parameters:** None (uses instance property state).
    *   **Return Value:** The number of records deleted from the database.
    *   **Exceptions:** Throws if the filter criteria are too broad without safeguards (implementation dependent) or on database write failure.

## Usage

### Example 1: Querying and Aggregating Audit Data
This example demonstrates configuring the filter to retrieve update operations for a specific entity type within a date range, then accessing aggregation properties.

```csharp
var filter = new AuditTrailFilter
{
    EntityType = typeof(Product).FullName,
    OperationType = OperationType.Update,
    From = DateTime.UtcNow.AddDays(-7),
    To = DateTime.UtcNow,
    Limit = 100
};

// Execute the query
var logs = await filter.QueryAsync();

// Access aggregation data if populated by the service during query
if (filter.ByOperation != null)
{
    Console.WriteLine($"Updates found: {filter.ByOperation.GetValueOrDefault("Update", 0)}");
}

// Iterate results
foreach (var log in logs)
{
    Console.WriteLine($"User {log.ChangedByUserId} modified entity at {log.Timestamp}");
}
```

### Example 2: Recording an Event and Retrieving Entity History
This example shows how to record a new change and immediately retrieve the full trail for that specific entity instance.

```csharp
var filter = new AuditTrailFilter
{
    EntityType = typeof(Order).FullName,
    EntityId = 452,
    ChangedByUserId = 101
};

// Record a new entry
await filter.RecordAsync();

// Retrieve the full history for this specific order
var history = await filter.GetEntityTrailAsync();

Console.WriteLine($"Total entries for Order #452: {history.Count}");
if (filter.OldestEntry.HasValue)
{
    Console.WriteLine($"Tracking started: {filter.OldestEntry.Value}");
}
```

## Notes

*   **Stateful Filtering:** The class maintains state in its properties. Calling multiple methods (e.g., `QueryAsync` followed by `PurgeAsync`) on the same instance will use the same filter criteria unless properties are explicitly modified between calls. Ensure properties are reset or a new instance is created for distinct operations.
*   **Nullability and Validation:** Methods like `GetEntityTrailAsync` and `GetUserTrailAsync` logically depend on `EntityType`/`EntityId` and `ChangedByUserId` respectively. Passing null values for these required context properties will likely result in runtime exceptions or empty result sets, depending on the underlying `AuditTrailService` implementation.
*   **Thread Safety:** The class exposes mutable public properties and shares an `AuditTrailService` instance. It is not thread-safe for concurrent modification of filter properties. If multiple threads need to perform audit operations with different criteria, separate instances of `AuditTrailFilter` should be created. However, the underlying `AuditTrailService` methods are asynchronous and generally safe for concurrent invocation provided they manage their own database connections correctly.
*   **Purge Caution:** The `PurgeAsync` method performs destructive operations. When using this method, ensure the `From` and `To` properties are strictly defined to prevent accidental deletion of the entire audit log if the service defaults to "all records" when filters are null.
*   **Aggregation Population:** Properties such as `ByOperation`, `ByEntityType`, `TotalEntries`, `OldestEntry`, and `NewestEntry` are typically populated as side effects of executing query methods like `QueryAsync`. They may return null or default values if no query method has been invoked yet.
