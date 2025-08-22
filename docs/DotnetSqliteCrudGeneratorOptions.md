# DotnetSqliteCrudGeneratorOptions

Configuration container for the SQLite CRUD generator, exposing settings for database access, connection pooling, caching, event bus, HTTP clients, webhooks, and background workers.

## API

### `Database` (property)
Gets or sets the database-specific configuration, including connection strings, schema options, and SQLite provider settings. Defaults to an instance with `ConnectionString` set to `"Data Source=:memory:"` and `Schema` set to `"public"`.

### `ConnectionPool` (property)
Gets or sets the connection pool configuration. Controls pool size, idle timeout, and eviction policies. Defaults to a pool with `MaxPoolSize` of 100, `ConnectionLifetime` of 30 minutes, and `ConnectionIdleTimeout` of 5 minutes.

### `Cache` (property)
Gets or sets the caching configuration. Defines cache duration, size limits, and eviction policies. Defaults to an in-memory cache with `SlidingExpiration` of 10 minutes and `SizeLimit` of 10,000 entries.

### `EventBus` (property)
Gets or sets the event bus configuration. Configures topic names, batch sizes, and retry policies for publishing domain events. Defaults to a bus with `TopicPrefix` of `"crudgen"` and `BatchSize` of 100.

### `HttpClient` (property)
Gets or sets the HTTP client configuration. Controls base addresses, timeouts, and default headers for outgoing HTTP calls. Defaults to a client with `BaseAddress` of `"https://api.example.com"`, `Timeout` of 30 seconds, and `DefaultRequestHeaders` including `"Accept": "application/json"`.

### `Webhook` (property)
Gets or sets the webhook configuration. Defines endpoint URLs, retry counts, and payload formats for outgoing notifications. Defaults to a webhook with `Endpoint` set to `"https://hooks.example.com/crudgen"`, `MaxRetries` of 3, and `ContentType` of `"application/json"`.

### `BackgroundWorker` (property)
Gets or sets the background worker configuration. Controls thread pool size, task timeouts, and retry policies for asynchronous operations. Defaults to a worker with `MaxDegreeOfParallelism` of 4, `TaskTimeout` of 5 minutes, and `RetryCount` of 2.

### `Validate()` (method)
Validates the current configuration. Throws `InvalidOperationException` if any required setting is missing or invalid (e.g., empty connection string, invalid timeout values). Returns `void`.

### `CreateDefault()` (static method)
Creates a new instance of `DotnetSqliteCrudGeneratorOptions` with default values for all properties. Returns a new `DotnetSqliteCrudGeneratorOptions` instance.

## Usage
