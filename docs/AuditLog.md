# AuditLog
The `AuditLog` type is designed to store and manage audit logs for database operations, providing a record of changes made to entities within the system. It captures essential information about each operation, including the entity type, operation type, and user responsible for the change, allowing for comprehensive tracking and auditing of database activities.

## API
* `public int Id`: A unique identifier for the audit log entry.
* `public required string EntityType`: The type of entity that was modified.
* `public int EntityId`: The identifier of the entity that was modified.
* `public OperationType OperationType`: The type of operation performed (e.g., create, update, delete).
* `public int ChangedByUserId`: The identifier of the user who performed the operation.
* `public string? OldValues`: A string representation of the entity's values before the operation.
* `public string? NewValues`: A string representation of the entity's values after the operation.
* `public string? ChangeReason`: An optional reason for the change.
* `public string? IpAddress`: The IP address of the user who performed the operation.
* `public DateTime Timestamp`: The date and time the operation was performed.
* `public bool Validate`: A flag indicating whether the audit log entry should be validated.
* `public static AuditLog CreateForCreate`: Creates a new `AuditLog` instance for a create operation.
* `public static AuditLog CreateForUpdate`: Creates a new `AuditLog` instance for an update operation.
* `public static AuditLog CreateForDelete`: Creates a new `AuditLog` instance for a delete operation.

## Usage
The following examples demonstrate how to use the `AuditLog` type:
```csharp
// Example 1: Creating an audit log for a new user
var newUser = new User { Name = "John Doe", Email = "john.doe@example.com" };
var auditLog = AuditLog.CreateForCreate;
auditLog.EntityType = "User";
auditLog.EntityId = newUser.Id;
auditLog.ChangedByUserId = 1;
auditLog.NewValues = JsonConvert.SerializeObject(newUser);
// Save the audit log to the database

// Example 2: Updating an existing product
var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 19.99m };
var auditLog = AuditLog.CreateForUpdate;
auditLog.EntityType = "Product";
auditLog.EntityId = updatedProduct.Id;
auditLog.ChangedByUserId = 1;
auditLog.OldValues = JsonConvert.SerializeObject(new Product { Id = 1, Name = "Original Product", Price = 9.99m });
auditLog.NewValues = JsonConvert.SerializeObject(updatedProduct);
// Save the audit log to the database
```

## Notes
When using the `AuditLog` type, consider the following:
* The `Validate` flag should be set to `true` to ensure the audit log entry is validated before saving.
* The `OldValues` and `NewValues` properties should be populated with string representations of the entity's values before and after the operation, respectively.
* The `IpAddress` property may be null if the IP address of the user is not available.
* The `AuditLog` type is not thread-safe, and concurrent access to its instances should be synchronized accordingly.
* The `CreateForCreate`, `CreateForUpdate`, and `CreateForDelete` methods create new `AuditLog` instances with default values for the respective operation types. These instances should be further populated with relevant data before saving to the database.
