# NamingConventionHelper

Utility class providing naming convention transformations and metadata extraction for SQL, C#, gRPC, and API endpoints in the `dotnet-sqlite-crud-generator` project. Supports bidirectional conversion between database and code naming styles, validation of property names, and retrieval of table, column, and service naming information.

## API

### `public static string GetSqlType(string csharpType)`

Converts a C# type name to its corresponding SQLite type name.

- **Parameters**
  - `csharpType`: The C# type name (e.g., `System.Int32`, `string`, `DateTime`).
- **Returns**
  - The SQLite type name (e.g., `INTEGER`, `TEXT`, `BLOB`).
- **Throws**
  - `ArgumentNullException` if `csharpType` is `null`.
  - `ArgumentException` if the type is not supported.

---

### `public static string ToCSharpToSqlConvention(string input)`

Transforms a C# identifier (e.g., PascalCase) to a SQL column name using the project's naming convention (typically snake_case).

- **Parameters**
  - `input`: The C# identifier (e.g., `UserId`, `CreatedAt`).
- **Returns**
  - The SQL column name (e.g., `user_id`, `created_at`).
- **Throws**
  - `ArgumentNullException` if `input` is `null`.

---

### `public static string ToSqlToCSharpConvention(string input)`

Transforms a SQL column name (e.g., snake_case) to a C# property name using the project's naming convention (typically PascalCase).

- **Parameters**
  - `input`: The SQL column name (e.g., `user_id`, `created_at`).
- **Returns**
  - The C# property name (e.g., `UserId`, `CreatedAt`).
- **Throws**
  - `ArgumentNullException` if `input` is `null`.

---
### `public static string GetTableName(Type entityType)`

Extracts the table name for a given entity type based on the `TableAttribute` or falls back to the type name transformed by convention.

- **Parameters**
  - `entityType`: The entity type (e.g., `User`, `OrderItem`).
- **Returns**
  - The resolved table name (e.g., `users`, `order_items`).
- **Throws**
  - `ArgumentNullException` if `entityType` is `null`.

---
### `public static string GetColumnName(PropertyInfo property)`

Extracts the column name for a property based on the `ColumnAttribute` or falls back to the property name transformed by convention.

- **Parameters**
  - `property`: The property info (e.g., `public int Id { get; set; }`).
- **Returns**
  - The resolved column name (e.g., `id`, `user_id`).
- **Throws**
  - `ArgumentNullException` if `property` is `null`.

---
### `public static string GetGrpcServiceName(Type serviceType)`

Derives the gRPC service name from the service type name using the project's convention.

- **Parameters**
  - `serviceType`: The gRPC service type (e.g., `UserService`).
- **Returns**
  - The gRPC service name (e.g., `UserService`).
- **Throws**
  - `ArgumentNullException` if `serviceType` is `null`.

---
### `public static string GetGrpcMessageName(Type messageType)`

Derives the gRPC message name from the message type name using the project's convention.

- **Parameters**
  - `messageType`: The gRPC message type (e.g., `UserRequest`).
- **Returns**
  - The gRPC message name (e.g., `UserRequest`).
- **Throws**
  - `ArgumentNullException` if `messageType` is `null`.

---
### `public static string GetApiEndpoint(string basePath, string resourceName, string httpMethod)`

Constructs a RESTful API endpoint path from a base path, resource name, and HTTP method.

- **Parameters**
  - `basePath`: The base API path (e.g., `/api/v1`).
  - `resourceName`: The resource name (e.g., `users`).
  - `httpMethod`: The HTTP method (e.g., `GET`, `POST`).
- **Returns**
  - The full endpoint path (e.g., `/api/v1/users`).
- **Throws**
  - `ArgumentNullException` if any parameter is `null`.

---
### `public static bool IsValidPropertyName(string name)`

Validates whether a string is a valid C# property name according to the project's rules.

- **Parameters**
  - `name`: The property name to validate.
- **Returns**
  - `true` if valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `name` is `null`.

---
### `public static NamingConventionInfo GetConventionInfo(Type entityType)`

Retrieves naming convention metadata for an entity type, including table name, column names, and property mappings.

- **Parameters**
  - `entityType`: The entity type.
- **Returns**
  - A `NamingConventionInfo` object containing resolved names and property mappings.
- **Throws**
  - `ArgumentNullException` if `entityType` is `null`.

---
### `public string? Name`

Gets or sets the name used for display or logging purposes.

- **Type**
  - `string?`
- **Remarks**
  - Can be `null`.

---
### `public ColumnAttribute()`

Default constructor for `ColumnAttribute`.

---
### `public ColumnAttribute(string name)`

Constructs a `ColumnAttribute` with a specified column name.

- **Parameters**
  - `name`: The column name override.

---
### `public string EntityName`

Gets or sets the entity name associated with the attribute.

- **Type**
  - `string`

---
### `public string TableName`

Gets or sets the resolved table name for the entity.

- **Type**
  - `string`

---
### `public string ApiEndpoint`

Gets or sets the resolved API endpoint path.

- **Type**
  - `string`

---
### `public string GrpcServiceName`

Gets or sets the resolved gRPC service name.

- **Type**
  - `string`

---
### `public List<PropertyConventionInfo> Properties`

Gets or sets the list of property convention mappings for the entity.

- **Type**
  - `List<PropertyConventionInfo>`

---
### `public string PropertyName`

Gets or sets the property name.

- **Type**
  - `string`

---
### `public string ColumnName`

Gets or sets the column name.

- **Type**
  - `string`

## Usage
