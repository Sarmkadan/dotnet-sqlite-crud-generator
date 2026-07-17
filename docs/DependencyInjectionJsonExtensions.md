# DependencyInjectionJsonExtensions

The `DependencyInjectionJsonExtensions` class is a configuration data-transfer object (DTO) designed to hold settings for dependency injection in the `dotnet-sqlite-crud-generator` library. It is typically deserialized from a JSON configuration file (e.g., `appsettings.json`) or constructed programmatically. The class exposes both top-level connection parameters and nested configuration objects for database, connection pooling, caching, event bus, HTTP client, webhooks, and background workers. All properties are read-write and nullable where indicated.

## API

### `Database`  
`DatabaseSettingsDto?`  
Gets or sets the database-specific configuration (e.g., provider options, schema settings). `null` if not provided.

### `ConnectionPool`  
`ConnectionPoolConfigurationDto?`  
Gets or sets the connection pool configuration. `null` if pooling is not explicitly configured.

### `Cache`  
`CacheConfigurationDto?`  
Gets or sets the caching configuration. `null` if caching is not used.

### `EventBus`  
`EventBusConfigurationDto?`  
Gets or sets the event bus configuration. `null` if no event bus is configured.

### `HttpClient`  
`HttpClientConfigurationDto?`  
Gets or sets the HTTP client configuration. `null` if no HTTP client is needed.

### `Webhook`  
`WebhookConfigurationDto?`  
Gets or sets the webhook configuration. `null` if webhooks are not used.

### `BackgroundWorker`  
`BackgroundWorkerConfigurationDto?`  
Gets or sets the background worker configuration. `null` if no background workers are required.

### `FilePath`  
`string?`  
Gets or sets the file path to the SQLite database file. When set, a connection string is derived from this path unless `ConnectionString` is also provided. `null` indicates no file path is specified.

### `ConnectionString`  
`string?`  
Gets or sets the full connection string for the database. If both `FilePath` and `ConnectionString` are set, `ConnectionString` takes precedence. `null` if not specified.

### `EnableLogging`  
`bool`  
Gets or sets whether logging is enabled. Default is `false`.

### `ConnectionTimeout`  
`int`  
Gets or sets the connection timeout in seconds. The default value is applied by the consuming code if this property is not explicitly set.

### `AutoCreateDatabase`  
`bool`  
Gets or sets whether the database should be automatically created if it does not exist. Default is `false`.

### `MinPoolSize`  
`int`  
Gets or sets the minimum number of connections maintained in the connection pool. Default is applied by the pool implementation.

### `MaxPoolSize`  
`int`  
Gets or sets the maximum number of connections allowed in the connection pool. Default is applied by the pool implementation.

### `IdleTimeoutMs`  
`long`  
Gets or sets the idle timeout in milliseconds after which an unused connection is closed. Default is implementation-specific.

### `AcquireTimeoutMs`  
`long`  
Gets or sets the maximum time in milliseconds to wait when acquiring a connection from the pool. Default is implementation-specific.

### `CleanupIntervalMs`  
`long`  
Gets or sets the interval in milliseconds between pool cleanup cycles. Default is implementation-specific.

### `EnableDiagnostics`  
`bool`  
Gets or sets whether diagnostic logging for the connection pool is enabled. Default is `false`.

### `Enabled`  
`bool`  
Gets or sets whether the cache is enabled. Default is `false`.

### `MaxSizeBytes`  
`long`  
Gets or sets the maximum size of the cache in bytes. Default is implementation-specific.

None of these properties throw exceptions on get or set. They are simple storage properties with no validation logic.

## Usage

### Example 1: Deserializing from `appsettings.json`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqliteCrudGenerator;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var diConfig = configuration
    .GetSection("SqliteCrudGenerator")
    .Get<DependencyInjectionJsonExtensions>();

var services = new ServiceCollection();
services.AddSqliteCrudGenerator(options =>
{
    options.FilePath = diConfig.FilePath;
    options.ConnectionString = diConfig.ConnectionString;
    options.EnableLogging = diConfig.EnableLogging;
    options.ConnectionTimeout = diConfig.ConnectionTimeout;
    options.AutoCreateDatabase = diConfig.AutoCreateDatabase;
    options.MinPoolSize = diConfig.MinPoolSize;
    options.MaxPoolSize = diConfig.MaxPoolSize;
    options.IdleTimeoutMs = diConfig.IdleTimeoutMs;
    options.AcquireTimeoutMs = diConfig.AcquireTimeoutMs;
    options.CleanupIntervalMs = diConfig.CleanupIntervalMs;
    options.EnableDiagnostics = diConfig.EnableDiagnostics;
    options.Cache = diConfig.Cache;
    options.EventBus = diConfig.EventBus;
    options.HttpClient = diConfig.HttpClient;
    options.Webhook = diConfig.Webhook;
    options.BackgroundWorker = diConfig.BackgroundWorker;
    options.Database = diConfig.Database;
    options.ConnectionPool = diConfig.ConnectionPool;
});
```

### Example 2: Programmatic configuration

```csharp
using Microsoft.Extensions.DependencyInjection;
using SqliteCrudGenerator;

var config = new DependencyInjectionJsonExtensions
{
    FilePath = "inventory.db",
    EnableLogging = true,
    ConnectionTimeout = 30,
    AutoCreateDatabase = true,
    MinPoolSize = 2,
    MaxPoolSize = 10,
    IdleTimeoutMs = 30000,
    AcquireTimeoutMs = 5000,
    CleanupIntervalMs = 60000,
    EnableDiagnostics = false,
    Cache = new CacheConfigurationDto
    {
        Enabled = true,
        MaxSizeBytes = 1048576
    },
    EventBus = new EventBusConfigurationDto
    {
        // Event bus specific properties
    },
    ConnectionPool = new ConnectionPoolConfigurationDto
    {
        // Pool specific overrides (if any)
    }
};

var services = new ServiceCollection();
services.AddSqliteCrudGenerator(config);
```

## Notes

- **Null semantics**: When a sub-configuration property (e.g., `Database`, `ConnectionPool`) is `null`, the corresponding feature is not configured and will use library defaults. Setting a sub-configuration to a non-null instance enables that feature with the provided settings.
- **FilePath vs ConnectionString**: If both `FilePath` and `ConnectionString` are non-null, `ConnectionString` takes precedence. If only `FilePath` is set, the library constructs a connection string from it. If neither is set, the library may throw or use a default location depending on the implementation.
- **Default values**: The class itself does not assign default values to numeric or boolean properties. Defaults are applied by the consuming registration code when a property is not explicitly set (e.g., `ConnectionTimeout` defaults to 30 seconds in the library). Always check the library documentation for the effective defaults.
- **Thread safety**: Instances of `DependencyInjectionJsonExtensions` are not thread-safe for concurrent writes. After the instance is used to configure dependency injection, it should not be modified. Concurrent reads are safe as long as no writes occur simultaneously. For multi-threaded scenarios, treat the instance as immutable after initialization.
- **Validation**: The class performs no input validation. Invalid values (e.g., negative timeouts) will be passed to the underlying library, which may throw at configuration or runtime.
