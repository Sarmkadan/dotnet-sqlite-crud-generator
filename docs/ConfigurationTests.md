# ConfigurationTests

Unit tests for validating configuration models in the `dotnet-sqlite-crud-generator` project. These tests ensure that configuration settings for database connections, connection pooling, caching, and application behavior are correctly validated and throw appropriate exceptions when invalid.

## API

### `void DatabaseSettings_WithFilePathContainingSpaces_GeneratesQuotedConnectionString()`
Validates that a `DatabaseSettings` instance with a file path containing spaces generates a properly quoted connection string. Ensures that paths with spaces are correctly escaped in the generated connection string to prevent SQL syntax errors.

### `void DatabaseSettings_WithFilePathContainingUnicode_GeneratesQuotedConnectionString()`
Validates that a `DatabaseSettings` instance with a file path containing Unicode characters generates a properly quoted connection string. Ensures that non-ASCII characters in paths are correctly handled in the connection string.

### `void DatabaseSettings_WithValidConnectionString_IsValid()`
Validates that a `DatabaseSettings` instance with a valid connection string is considered valid. Ensures that the configuration model correctly identifies a properly formatted connection string as valid.

### `void DatabaseSettings_WithEmptyConnectionString_ThrowsArgumentException()`
Validates that a `DatabaseSettings` instance with an empty connection string throws an `ArgumentException`. Ensures that the configuration model enforces non-empty connection strings.

### `void ConnectionPoolConfiguration_WithValidSettings_IsValid()`
Validates that a `ConnectionPoolConfiguration` instance with valid settings (e.g., positive `MaxPoolSize`, non-zero `IdleTimeout`) is considered valid. Ensures that the configuration model correctly identifies valid connection pool settings.

### `void ConnectionPoolConfiguration_WithZeroMaxPoolSize_ThrowsInvalidOperationException()`
Validates that a `ConnectionPoolConfiguration` instance with a `MaxPoolSize` of zero throws an `InvalidOperationException`. Ensures that the configuration model enforces a positive `MaxPoolSize`.

### `void ConnectionPoolConfiguration_WithMinPoolSizeGreaterThanMaxPoolSize_ThrowsInvalidOperationException()`
Validates that a `ConnectionPoolConfiguration` instance where `MinPoolSize` is greater than `MaxPoolSize` throws an `InvalidOperationException`. Ensures that the configuration model enforces logical pool size constraints.

### `void ConnectionPoolConfiguration_WithZeroIdleTimeout_ThrowsInvalidOperationException()`
Validates that a `ConnectionPoolConfiguration` instance with an `IdleTimeout` of zero throws an `InvalidOperationException`. Ensures that the configuration model enforces a non-zero `IdleTimeout`.

### `void CacheConfiguration_WithValidSettings_IsValid()`
Validates that a `CacheConfiguration` instance with valid settings (e.g., positive `MaxSizeBytes`) is considered valid. Ensures that the configuration model correctly identifies valid cache settings.

### `void ApplicationConfiguration_Validate_ThrowsForInvalidCacheMaxSizeBytes()`
Validates that an `ApplicationConfiguration` instance with an invalid `CacheMaxSizeBytes` (e.g., negative value) throws an exception during validation. Ensures that the configuration model enforces non-negative cache size limits.

### `void ApplicationConfiguration_Validate_ThrowsForMissingDatabaseConnectionString()`
Validates that an `ApplicationConfiguration` instance with a missing or null `DatabaseConnectionString` throws an exception during validation. Ensures that the configuration model enforces a required database connection string.

### `void ApplicationConfiguration_Validate_ThrowsForNullDatabaseSettings()`
Validates that an `ApplicationConfiguration` instance with null `DatabaseSettings` throws an exception during validation. Ensures that the configuration model enforces non-null database settings.

### `void ApplicationConfiguration_Validate_ThrowsForInvalidWorkerCount()`
Validates that an `ApplicationConfiguration` instance with an invalid `WorkerCount` (e.g., zero or negative) throws an exception during validation. Ensures that the configuration model enforces a positive worker count.

## Usage

### Example 1: Validating Database Settings
