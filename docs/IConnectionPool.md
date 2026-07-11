# IConnectionPool
The `IConnectionPool` interface provides a way to manage a pool of database connections, allowing for efficient reuse and management of connections. It enables the acquisition of connections, tracking of connection statistics, and proper disposal of connections when they are no longer needed.

## API
* `ConnectionPool`: The connection pool instance.
* `AcquireAsync`: Asynchronously acquires a pooled connection. Returns a `PooledConnection` instance. Throws if the acquisition fails.
* `GetStatistics`: Retrieves the connection pool statistics, including total, available, and active connections, as well as acquire and timeout counts.
* `DisposeAsync`: Asynchronously disposes of the connection pool, releasing any resources held by the pool.
* `PoolEntry`: Represents an entry in the connection pool, containing a `SqliteConnection` and a `LastUsed` timestamp.
* `Connection`: Gets the underlying `SqliteConnection` instance.
* `LastUsed`: Gets the timestamp when the connection was last used.
* `Dispose`: Disposes of the connection pool, releasing any resources held by the pool.
* `TotalConnections`, `AvailableConnections`, `ActiveConnections`, `MaxPoolSize`, `MinPoolSize`, `TotalAcquireCount`, `TotalTimeoutCount`: Get the respective connection pool statistics.

## Usage
```csharp
// Example 1: Acquiring a connection from the pool
var connectionPool = new ConnectionPool();
var pooledConnection = await connectionPool.AcquireAsync();
using (var connection = pooledConnection.Connection)
{
    // Use the connection
}

// Example 2: Retrieving connection pool statistics
var statistics = connectionPool.GetStatistics();
Console.WriteLine($"Total Connections: {statistics.TotalConnections}");
Console.WriteLine($"Available Connections: {statistics.AvailableConnections}");
Console.WriteLine($"Active Connections: {statistics.ActiveConnections}");
```

## Notes
The `IConnectionPool` interface is designed to be thread-safe, allowing for concurrent access and management of connections. However, it is essential to properly dispose of connections when they are no longer needed to avoid resource leaks. Additionally, the `AcquireAsync` method may throw if the acquisition fails, and the `DisposeAsync` method should be used to ensure proper cleanup of resources. The connection pool statistics provide valuable insights into the pool's performance and can be used to optimize the pool's configuration.
