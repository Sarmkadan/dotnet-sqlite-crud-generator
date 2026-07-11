# ConnectionPoolConfiguration

Configuration settings for managing a SQLite connection pool, controlling pool size, timeouts, and diagnostic behavior.

## API

### `MinPoolSize`
Gets or sets the minimum number of connections to maintain in the pool. Must be a non-negative integer. Defaults to 1. When the pool size drops below this value due to connection closures, new connections are created to restore the minimum.

### `MaxPoolSize`
Gets or sets the maximum number of connections allowed in the pool. Must be a positive integer greater than or equal to `MinPoolSize`. Defaults to 100. Attempts to exceed this limit will block or fail based on `AcquireTimeout`.

### `IdleTimeout`
Gets or sets the duration after which an idle connection is eligible for removal from the pool. Defaults to 5 minutes. Connections that have been idle longer than this period may be closed during cleanup cycles.

### `AcquireTimeout`
Gets or sets the maximum time to wait for a connection when the pool is at `MaxPoolSize`. Defaults to 30 seconds. If no connection becomes available within this period, an exception is thrown.

### `CleanupInterval`
Gets or sets the frequency at which the pool performs cleanup of idle and expired connections. Defaults to 1 minute. Cleanup runs asynchronously and does not block connection acquisition.

### `EnableDiagnostics`
Gets or sets a value indicating whether diagnostic information about the connection pool is collected and exposed. When enabled, internal metrics such as active connection count and wait times may be logged or surfaced via diagnostics APIs. Defaults to `false`.

### `Validate()`
Validates the current configuration values. Throws an exception if any setting is invalid (e.g., `MinPoolSize` > `MaxPoolSize`, `MaxPoolSize` ≤ 0, or timeout values negative). Does not modify state.

## Usage
