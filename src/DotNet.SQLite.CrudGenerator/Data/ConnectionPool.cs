// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNet.SQLite.CrudGenerator.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Provides pooled SQLite connections with configurable concurrency limits and idle cleanup.
/// </summary>
public interface IConnectionPool : IAsyncDisposable
{
    /// <summary>
    /// Acquires a connection from the pool.  A new physical connection is opened when the
    /// pool has no idle connections available, up to <see cref="ConnectionPoolConfiguration.MaxPoolSize"/>.
    /// Returning the <see cref="PooledConnection"/> to the pool is done via its <c>Dispose</c>
    /// or <c>DisposeAsync</c> — always use it inside a <c>using</c> statement or declaration.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the wait for an available slot.</param>
    /// <returns>A <see cref="PooledConnection"/> wrapping an open <see cref="SqliteConnection"/>.</returns>
    /// <exception cref="TimeoutException">
    /// Thrown when no connection becomes available within <see cref="ConnectionPoolConfiguration.AcquireTimeout"/>.
    /// </exception>
    Task<PooledConnection> AcquireAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a point-in-time snapshot of pool metrics.
    /// </summary>
    ConnectionPoolStatistics GetStatistics();
}

/// <summary>
/// Thread-safe SQLite connection pool backed by a <see cref="ConcurrentQueue{T}"/>.
/// Idle connections exceeding <see cref="ConnectionPoolConfiguration.IdleTimeout"/> are
/// periodically closed by a background timer, while at least
/// <see cref="ConnectionPoolConfiguration.MinPoolSize"/> connections are always retained.
/// </summary>
public sealed class ConnectionPool : IConnectionPool
{
    private readonly string _connectionString;
    private readonly ConnectionPoolConfiguration _config;
    private readonly ILogger<ConnectionPool> _logger;
    private readonly SemaphoreSlim _poolSemaphore;
    private readonly ConcurrentQueue<PoolEntry> _available = new();
    private readonly CancellationTokenSource _disposeCts = new();
    private readonly Timer _cleanupTimer;
    private int _totalConnections;
    private int _acquireCount;
    private int _timeoutCount;
    private bool _disposed;

    /// <summary>
    /// Initialises the pool and starts the background idle-connection cleanup timer.
    /// </summary>
    /// <param name="connectionString">SQLite connection string used when opening new connections.</param>
    /// <param name="config">Pool configuration.  <see cref="ConnectionPoolConfiguration.Validate"/> is called during construction.</param>
    /// <param name="logger">Logger for lifecycle and diagnostic messages.</param>
    public ConnectionPool(
        string connectionString,
        ConnectionPoolConfiguration config,
        ILogger<ConnectionPool> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(logger);

        config.Validate();

        _connectionString = connectionString;
        _config = config;
        _logger = logger;
        _poolSemaphore = new SemaphoreSlim(config.MaxPoolSize, config.MaxPoolSize);
        _cleanupTimer = new Timer(RunCleanup, null, config.CleanupInterval, config.CleanupInterval);

        _logger.LogInformation(
            "Connection pool started. MinSize={Min}, MaxSize={Max}, IdleTimeout={Idle}, AcquireTimeout={Acquire}",
            config.MinPoolSize, config.MaxPoolSize, config.IdleTimeout, config.AcquireTimeout);
    }

    /// <inheritdoc />
    public async Task<PooledConnection> AcquireAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _disposeCts.Token);

        bool slotAcquired = await _poolSemaphore.WaitAsync(_config.AcquireTimeout, linkedCts.Token);

        if (!slotAcquired)
        {
            Interlocked.Increment(ref _timeoutCount);
            throw new TimeoutException(
                $"Could not acquire a connection from the pool within {_config.AcquireTimeout.TotalSeconds:0.#}s " +
                $"(MaxPoolSize={_config.MaxPoolSize}).");
        }

        SqliteConnection connection;

        if (_available.TryDequeue(out var entry))
        {
            connection = entry.Connection;

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(linkedCts.Token);

            if (_config.EnableDiagnostics)
                _logger.LogDebug("Reusing idle connection from pool. Available={Count}", _available.Count);
        }
        else
        {
            connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(linkedCts.Token);
            Interlocked.Increment(ref _totalConnections);

            if (_config.EnableDiagnostics)
                _logger.LogDebug("Opened new connection. Total={Total}", _totalConnections);
        }

        Interlocked.Increment(ref _acquireCount);
        return new PooledConnection(connection, this);
    }

    /// <inheritdoc />
    public ConnectionPoolStatistics GetStatistics() => new()
    {
        TotalConnections = _totalConnections,
        AvailableConnections = _available.Count,
        ActiveConnections = _totalConnections - _available.Count,
        MaxPoolSize = _config.MaxPoolSize,
        MinPoolSize = _config.MinPoolSize,
        TotalAcquireCount = _acquireCount,
        TotalTimeoutCount = _timeoutCount
    };

    internal void Return(SqliteConnection connection)
    {
        if (_disposed)
        {
            connection.Dispose();
            Interlocked.Decrement(ref _totalConnections);
            return;
        }

        _available.Enqueue(new PoolEntry(connection));
        _poolSemaphore.Release();

        if (_config.EnableDiagnostics)
            _logger.LogDebug("Connection returned to pool. Available={Count}", _available.Count);
    }

    private void RunCleanup(object? state)
    {
        if (_disposed) return;

        // Drain the queue so we can inspect each entry atomically.
        var snapshot = new List<PoolEntry>();
        while (_available.TryDequeue(out var entry))
            snapshot.Add(entry);

        var now = DateTime.UtcNow;
        int evicted = 0;
        int kept = 0;

        // Favour the most recently used connections when deciding what to keep.
        foreach (var entry in snapshot.OrderByDescending(e => e.LastUsed))
        {
            bool idle = (now - entry.LastUsed) > _config.IdleTimeout;
            bool belowMin = kept < _config.MinPoolSize;

            if (!idle || belowMin)
            {
                _available.Enqueue(entry);
                kept++;
            }
            else
            {
                entry.Connection.Dispose();
                Interlocked.Decrement(ref _totalConnections);
                evicted++;
            }
        }

        if (evicted > 0)
            _logger.LogInformation(
                "Connection pool cleanup evicted {Evicted} idle connection(s). Remaining={Kept}",
                evicted, kept);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _disposeCts.CancelAsync();
        await _cleanupTimer.DisposeAsync();

        while (_available.TryDequeue(out var entry))
        {
            entry.Connection.Dispose();
            Interlocked.Decrement(ref _totalConnections);
        }

        _poolSemaphore.Dispose();
        _disposeCts.Dispose();

        _logger.LogInformation(
            "Connection pool disposed. TotalAcquired={Acquired}, Timeouts={Timeouts}",
            _acquireCount, _timeoutCount);
    }

    private sealed class PoolEntry
    {
        public PoolEntry(SqliteConnection connection)
        {
            Connection = connection;
            LastUsed = DateTime.UtcNow;
        }

        public SqliteConnection Connection { get; }
        public DateTime LastUsed { get; }
    }
}

/// <summary>
/// A leased connection from <see cref="ConnectionPool"/>.
/// Disposing returns the underlying connection to the pool rather than closing it.
/// Always use within a <c>using</c> statement or <c>await using</c> declaration.
/// </summary>
public sealed class PooledConnection : IAsyncDisposable, IDisposable
{
    private readonly ConnectionPool _pool;
    private bool _disposed;

    internal PooledConnection(SqliteConnection connection, ConnectionPool pool)
    {
        Connection = connection;
        _pool = pool;
    }

    /// <summary>Gets the open <see cref="SqliteConnection"/> for executing commands.</summary>
    public SqliteConnection Connection { get; }

    /// <summary>Returns the connection to the pool synchronously.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pool.Return(Connection);
    }

    /// <summary>Returns the connection to the pool asynchronously.</summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Point-in-time snapshot of <see cref="ConnectionPool"/> metrics.
/// </summary>
public sealed class ConnectionPoolStatistics
{
    /// <summary>Gets the number of physical connections currently managed by the pool.</summary>
    public int TotalConnections { get; init; }

    /// <summary>Gets the number of connections currently idle and ready to be leased.</summary>
    public int AvailableConnections { get; init; }

    /// <summary>Gets the number of connections currently checked out by callers.</summary>
    public int ActiveConnections { get; init; }

    /// <summary>Gets the configured maximum pool size.</summary>
    public int MaxPoolSize { get; init; }

    /// <summary>Gets the configured minimum pool size kept alive during idle cleanup.</summary>
    public int MinPoolSize { get; init; }

    /// <summary>Gets the total number of successful connection acquisitions since pool creation.</summary>
    public int TotalAcquireCount { get; init; }

    /// <summary>Gets the number of acquisition attempts that exceeded <see cref="ConnectionPoolConfiguration.AcquireTimeout"/>.</summary>
    public int TotalTimeoutCount { get; init; }
}
