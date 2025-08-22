# CacheConfiguration

Central configuration object for cache providers, event sourcing, and retry policies used throughout the SQLite CRUD generator. It consolidates cache behavior, event persistence, and resilience settings into a single, strongly-typed configuration.

## API

### `Enabled`
Gets or sets a value indicating whether the feature governed by this configuration block is enabled.
Type: `bool`
Default: `true`

### `MaxSizeBytes`
Gets or sets the maximum size of the in-memory cache in bytes.
Type: `long`
Default: `10485760` (10 MB)

### `DefaultTTL`
Gets or sets the default time-to-live for cached items when not specified.
Type: `TimeSpan`
Default: `TimeSpan.FromMinutes(5)`

### `CleanupIntervalSeconds`
Gets or sets the interval, in seconds, at which the cache performs cleanup of expired entries.
Type: `int`
Default: `300`

### `MemoryCacheProvider.CreateProvider(CacheConfiguration config)`
Static factory method that creates a new `MemoryCacheProvider` instance using the provided configuration.
Parameters:
- `config`: Configuration object specifying cache behavior.
Return value: A new `MemoryCacheProvider` instance.
Throws: `ArgumentNullException` if `config` is `null`.

### `MaxEventHistory`
Gets or sets the maximum number of events to retain in memory for diagnostics and replay.
Type: `int`
Default: `1000`

### `PersistEvents`
Gets or sets a value indicating whether events should be persisted to disk.
Type: `bool`
Default: `false`

### `EventStoragePath`
Gets or sets the directory path where events are persisted when `PersistEvents` is `true`.
Type: `string?`
Default: `null`

### `ConnectionLimit`
Gets or sets the maximum number of concurrent database connections allowed.
Type: `int`
Default: `100`

### `DefaultTimeout`
Gets or sets the default timeout for database operations.
Type: `TimeSpan`
Default: `TimeSpan.FromSeconds(30)`

### `MaxRetries`
Gets or sets the maximum number of retry attempts for failed operations.
Type: `int`
Default: `3`

### `RetryDelayMs`
Gets or sets the delay, in milliseconds, between retry attempts.
Type: `int`
Default: `100`

### `SigningSecret`
Gets or sets the secret used to sign cache tokens or event payloads.
Type: `string?`
Default: `null`

### `MaxDeliveryHistorySize`
Gets or sets the maximum number of delivery attempts to track for outbound messages.
Type: `int`
Default: `100`

### `WorkerCount`
Gets or sets the number of background worker threads used for cache cleanup and event processing.
Type: `int`
Default: `Environment.ProcessorCount`
