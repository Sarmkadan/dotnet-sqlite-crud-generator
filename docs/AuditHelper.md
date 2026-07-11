# AuditHelper

Utility class providing auditing capabilities for entity changes, property modifications, and user operations in SQLite-backed applications. It records detailed logs of CRUD operations, property changes, and user activities with timestamps, entity context, and optional user/session metadata. The class supports querying audit trails by time ranges, entities, operations, or users, and provides summary statistics and export functionality.

## API

### `public static void LogEntityChange(object entity, string operation, string? userId = null, string? ipAddress = null, string? details = null)`

Records a high-level change for an entire entity. Captures the entity type, its identifier (via `ToString()`), the operation performed, and optional user/session metadata.

- **Parameters**
  - `entity`: The entity instance whose change is being logged.
  - `operation`: The operation type (e.g., "Create", "Update", "Delete").
  - `userId`: Optional identifier of the user performing the operation.
  - `ipAddress`: Optional IP address associated with the operation.
  - `details`: Optional additional context or payload.

- **Throws**
  - `ArgumentNullException`: If `entity` or `operation` is `null`.

---

### `public static void LogPropertyChange(object entity, string propertyName, object? oldValue, object? newValue, string? userId = null, string? ipAddress = null, string? details = null)`

Records a change to a specific property of an entity. Captures the entity type, its identifier, the property name, old and new values, and optional user/session metadata.

- **Parameters**
  - `entity`: The entity instance containing the changed property.
  - `propertyName`: Name of the property that changed.
  - `oldValue`: Previous value of the property.
  - `newValue`: New value of the property.
  - `userId`: Optional identifier of the user performing the change.
  - `ipAddress`: Optional IP address associated with the operation.
  - `details`: Optional additional context or payload.

- **Throws**
  - `ArgumentNullException`: If `entity`, `propertyName`, or `operation` is `null`.

---

### `public static IEnumerable<AuditLogEntry> GetEntityAuditTrail(string entityType, string entityId)`

Retrieves all audit entries related to a specific entity identified by its type and ID.

- **Parameters**
  - `entityType`: Fully qualified or simple type name of the entity.
  - `entityId`: String representation of the entity’s unique identifier.

- **Returns**
  - An enumerable of `AuditLogEntry` records, ordered by timestamp ascending.

- **Throws**
  - `ArgumentException`: If `entityType` or `entityId` is `null` or whitespace.

---

### `public static IEnumerable<AuditLogEntry> GetOperationLog(string operation)`

Retrieves all audit entries for a specific operation type (e.g., "Update", "Delete").

- **Parameters**
  - `operation`: The operation type to filter by.

- **Returns**
  - An enumerable of `AuditLogEntry` records, ordered by timestamp ascending.

- **Throws**
  - `ArgumentException`: If `operation` is `null` or whitespace.

---
### `public static IEnumerable<AuditLogEntry> GetUserAuditTrail(string userId)`

Retrieves all audit entries associated with a specific user.

- **Parameters**
  - `userId`: Identifier of the user whose actions are being audited.

- **Returns**
  - An enumerable of `AuditLogEntry` records, ordered by timestamp ascending.

- **Throws**
  - `ArgumentException`: If `userId` is `null` or whitespace.

---
### `public static IEnumerable<AuditLogEntry> GetAuditEntriesInRange(DateTime start, DateTime end)`

Retrieves all audit entries within a specified time range.

- **Parameters**
  - `start`: Inclusive start of the time range.
  - `end`: Inclusive end of the time range.

- **Returns**
  - An enumerable of `AuditLogEntry` records, ordered by timestamp ascending.

- **Throws**
  - `ArgumentOutOfRangeException`: If `start` is after `end`.

---
### `public static AuditStatistics GetStatistics()`

Computes summary statistics over all recorded audit entries, including total count and distribution of operations.

- **Returns**
  - An `AuditStatistics` object containing:
    - `TotalEntries`: Total number of audit entries.
    - `EntryCount`: Alias for `TotalEntries`.
    - `Operations`: Dictionary mapping operation names to their occurrence counts.

---
### `public static void ClearAuditLog()`

Removes all audit entries from the underlying storage. Use with caution; this action is irreversible.

---
### `public static string ExportToCsv()`

Exports all audit entries to a CSV-formatted string with headers: `Id,Timestamp,EntityType,EntityId,Operation,UserId,IpAddress,Details`.

- **Returns**
  - A CSV string containing all audit entries.

---
### `public Guid Id`

Unique identifier for the audit entry. Immutable after creation.

---
### `public DateTime Timestamp`

Timestamp when the audit entry was created. Immutable after creation.

---
### `public string EntityType`

Fully qualified or simple type name of the entity being audited.

---
### `public string EntityId`

String representation of the entity’s unique identifier.

---
### `public string Operation`

Type of operation performed (e.g., "Create", "Update", "Delete").

---
### `public string? UserId`

Optional identifier of the user who performed the operation.

---
### `public string? IpAddress`

Optional IP address associated with the operation.

---
### `public string? Details`

Optional additional context or payload describing the change.

---
### `public int TotalEntries`

Total number of audit entries recorded. Alias for `EntryCount`.

---
### `public int EntryCount`

Alias for `TotalEntries`.

---
### `public Dictionary<string, int> Operations`

Dictionary mapping operation names to their occurrence counts across all audit entries.

## Usage
