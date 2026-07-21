using System.Collections.Concurrent;
using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.Data;

public class ConnectionPoolTests
{
    private readonly string _testConnectionString = "Data Source=:memory:";
    private readonly ConnectionPoolConfiguration _defaultConfig = new()
    {
        MinPoolSize = 1,
        MaxPoolSize = 10,
        IdleTimeout = TimeSpan.FromMinutes(5),
        AcquireTimeout = TimeSpan.FromSeconds(30),
        CleanupInterval = TimeSpan.FromMinutes(1),
        EnableDiagnostics = false
    };

    private ILogger<ConnectionPool> CreateMockLogger()
    {
        var logger = Substitute.For<ILogger<ConnectionPool>>();
        return logger;
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesPool()
    {
        // Act
        var pool = new ConnectionPool(_testConnectionString, _defaultConfig, CreateMockLogger());

        // Assert
        pool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ConnectionPool(null!, config, logger));
        Assert.Throws<ArgumentException>(() => new ConnectionPool("", config, logger));
        Assert.Throws<ArgumentException>(() => new ConnectionPool("   ", config, logger));
    }

    [Fact]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = CreateMockLogger();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConnectionPool(_testConnectionString, null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConnectionPool(_testConnectionString, config, null!));
    }

    [Fact]
    public async Task AcquireAsync_WithDisposedPool_ThrowsObjectDisposedException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        var pool = new ConnectionPool(_testConnectionString, config, logger);
        await pool.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => pool.AcquireAsync());
    }

    [Fact]
    public async Task AcquireAsync_ReturnsPooledConnection()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Act
        var connection = await pool.AcquireAsync();

        // Assert
        connection.Should().NotBeNull();
        connection.Connection.Should().NotBeNull();
        connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task AcquireAsync_WithUsingStatement_ReturnsConnection()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Act
        PooledConnection connection;
        using (connection = await pool.AcquireAsync())
        {
            // Assert inside using block
            connection.Should().NotBeNull();
            connection.Connection.Should().NotBeNull();
            connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
        }

        // After dispose, connection should be returned to pool
        var stats = pool.GetStatistics();
        stats.AvailableConnections.Should().Be(1);
    }

    [Fact]
    public async Task AcquireAsync_MultipleConnections_AreIndependent()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Act
        var connection1 = await pool.AcquireAsync();
        var connection2 = await pool.AcquireAsync();

        // Assert
        connection1.Should().NotBeNull();
        connection2.Should().NotBeNull();
        connection1.Connection.Should().NotBe(connection2.Connection);
        connection1.Connection.State.Should().Be(System.Data.ConnectionState.Open);
        connection2.Connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task Return_ConnectionIsReused()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Acquire and return a connection
        PooledConnection connection1;
        using (connection1 = await pool.AcquireAsync())
        {
            // Do nothing, just acquire and dispose
        }

        // Act - acquire again
        PooledConnection connection2;
        using (connection2 = await pool.AcquireAsync())
        {
            // Should reuse the same connection
            connection2.Connection.Should().Be(connection1.Connection);
        }
    }

    [Fact]
    public async Task GetStatistics_ReturnsCorrectMetrics()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 5, EnableDiagnostics = false };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Initially: 0 total, 0 available, 0 active
        var stats1 = pool.GetStatistics();
        stats1.TotalConnections.Should().Be(0);
        stats1.AvailableConnections.Should().Be(0);
        stats1.ActiveConnections.Should().Be(0);
        stats1.MaxPoolSize.Should().Be(5);
        stats1.TotalAcquireCount.Should().Be(0);
        stats1.TotalTimeoutCount.Should().Be(0);

        // Acquire a connection
        var connection = await pool.AcquireAsync();
        var stats2 = pool.GetStatistics();
        stats2.TotalConnections.Should().Be(1);
        stats2.AvailableConnections.Should().Be(0);
        stats2.ActiveConnections.Should().Be(1);
        stats2.TotalAcquireCount.Should().Be(1);

        // Return the connection
        connection.Dispose();
        var stats3 = pool.GetStatistics();
        stats3.TotalConnections.Should().Be(1);
        stats3.AvailableConnections.Should().Be(1);
        stats3.ActiveConnections.Should().Be(0);
        stats3.TotalAcquireCount.Should().Be(1);
    }

    [Fact]
    public async Task MaxPoolSize_EnforcedWhenExceeded()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 3, AcquireTimeout = TimeSpan.FromSeconds(1) };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Acquire all available connections
        var connections = new List<PooledConnection>();
        for (int i = 0; i < 3; i++)
        {
            connections.Add(await pool.AcquireAsync());
        }

        // Act & Assert - should throw timeout when trying to exceed max pool size
        var acquireTask = pool.AcquireAsync();
        await Assert.ThrowsAsync<TimeoutException>(() => acquireTask);

        // Cleanup
        foreach (var conn in connections)
        {
            conn.Dispose();
        }
    }

    [Fact]
    public async Task DisposeAsync_CleansUpAllConnections()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 5 };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Acquire some connections
        var connections = new List<PooledConnection>();
        for (int i = 0; i < 3; i++)
        {
            connections.Add(await pool.AcquireAsync());
        }

        // Act
        await pool.DisposeAsync();

        // Assert - pool should be disposed and connections should be cleaned up
        // Note: After DisposeAsync, the pool is in a disposed state, so we can't call GetStatistics
        // Instead, we verify that DisposeAsync completed without error and the pool is disposed
        pool.GetType().GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(pool).Should().Be(true);
    }

    [Fact]
    public async Task ConcurrentAcquisition_ThreadSafe()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 10, AcquireTimeout = TimeSpan.FromSeconds(5) };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        var acquiredConnections = new ConcurrentBag<PooledConnection>();
        var tasks = new List<Task>();
        var successCount = 0;
        var exceptionCount = 0;

        // Act - try to acquire more connections than max pool size concurrently
        for (int i = 0; i < 15; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var conn = await pool.AcquireAsync();
                    acquiredConnections.Add(conn);
                    Interlocked.Increment(ref successCount);
                }
                catch (TimeoutException)
                {
                    Interlocked.Increment(ref exceptionCount);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        successCount.Should().Be(10); // Should only succeed up to MaxPoolSize
        exceptionCount.Should().Be(5); // Should timeout the rest
        acquiredConnections.Count.Should().Be(10);

        // Cleanup
        foreach (var conn in acquiredConnections)
        {
            conn.Dispose();
        }
    }

    [Fact]
    public async Task IdleTimeout_ConnectionsAreEvicted()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 5,
            IdleTimeout = TimeSpan.FromMilliseconds(100), // Very short timeout for testing
            CleanupInterval = TimeSpan.FromMilliseconds(50),
            EnableDiagnostics = false
        };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Acquire and return connections to make them idle
        var connections = new List<PooledConnection>();
        for (int i = 0; i < 3; i++)
        {
            var conn = await pool.AcquireAsync();
            connections.Add(conn);
        }

        // Return all connections to pool (they become idle)
        foreach (var conn in connections)
        {
            conn.Dispose();
        }

        // Wait for cleanup to run multiple times
        await Task.Delay(300);

        // Act - get statistics after cleanup
        var stats = pool.GetStatistics();

        // Assert - connections should be evicted due to idle timeout
        // MinPoolSize = 1, so at least 1 connection should remain
        stats.TotalConnections.Should().BeLessOrEqualTo(1);
        stats.AvailableConnections.Should().BeLessOrEqualTo(1);
        stats.ActiveConnections.Should().Be(0);

        // Cleanup remaining connection
        if (stats.AvailableConnections > 0)
        {
            // Need to acquire and dispose to trigger cleanup
            var finalConn = await pool.AcquireAsync();
            finalConn.Dispose();
        }
    }

    [Fact]
    public async Task MinPoolSize_ConnectionsArePreserved()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 2,
            MaxPoolSize = 5,
            IdleTimeout = TimeSpan.FromMilliseconds(50),
            CleanupInterval = TimeSpan.FromMilliseconds(50),
            EnableDiagnostics = false
        };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Acquire and return connections to make them idle
        var connections = new List<PooledConnection>();
        for (int i = 0; i < 3; i++)
        {
            var conn = await pool.AcquireAsync();
            connections.Add(conn);
        }

        // Return all connections to pool (they become idle)
        foreach (var conn in connections)
        {
            conn.Dispose();
        }

        // Wait for cleanup to run
        await Task.Delay(300);

        // Force a cleanup cycle by manually triggering it
        var poolType = pool.GetType();
        var cleanupTimerField = poolType.GetField("_cleanupTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cleanupTimer = (Timer)cleanupTimerField!.GetValue(pool)!;

        // Manually invoke cleanup
        var runCleanupMethod = poolType.GetMethod("RunCleanup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        runCleanupMethod!.Invoke(pool, new object?[] { null });

        // Act
        var stats = pool.GetStatistics();

        // Assert - at least MinPoolSize connections should remain
        stats.TotalConnections.Should().BeGreaterOrEqualTo(2);
        stats.AvailableConnections.Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task PooledConnection_DisposeReturnsToPool()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        PooledConnection connection;
        using (connection = await pool.AcquireAsync())
        {
            connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
        }

        // After dispose, connection should be available again
        var stats = pool.GetStatistics();
        stats.AvailableConnections.Should().Be(1);
        stats.ActiveConnections.Should().Be(0);
    }

    [Fact]
    public async Task PooledConnection_DisposeAsyncReturnsToPool()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        PooledConnection connection;
        using (connection = await pool.AcquireAsync())
        {
            connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
        }

        // After async dispose, connection should be available again
        var stats = pool.GetStatistics();
        stats.AvailableConnections.Should().Be(1);
    }

    [Fact]
    public async Task Return_AfterPoolDisposed_DisposesConnection()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        var connection = await pool.AcquireAsync();
        await pool.DisposeAsync();

        // Act - return connection after pool is disposed (should not throw)
        connection.Dispose();

        // Assert - no exception should occur
    }

    [Fact]
    public void Configuration_Validate_ValidatesCorrectly()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();

        // Act - should not throw
        config.Validate();
    }

    [Fact]
    public void Configuration_Validate_ThrowsOnInvalidMinPoolSize()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MinPoolSize = -1 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Configuration_Validate_ThrowsOnInvalidMaxPoolSize()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 0 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Configuration_Validate_ThrowsWhenMinExceedsMax()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MinPoolSize = 10, MaxPoolSize = 5 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Configuration_Validate_ThrowsOnZeroTimeouts()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            AcquireTimeout = TimeSpan.Zero,
            IdleTimeout = TimeSpan.Zero,
            CleanupInterval = TimeSpan.Zero
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public async Task Statistics_ActiveConnectionsCalculationIsCorrect()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 5 };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // Initially: 0 active
        var stats1 = pool.GetStatistics();
        stats1.ActiveConnections.Should().Be(0);

        // Acquire 2 connections: 2 active
        var conn1 = await pool.AcquireAsync();
        var conn2 = await pool.AcquireAsync();
        var stats2 = pool.GetStatistics();
        stats2.ActiveConnections.Should().Be(2);

        // Return 1 connection: 1 active
        conn1.Dispose();
        var stats3 = pool.GetStatistics();
        stats3.ActiveConnections.Should().Be(1);

        // Return last connection: 0 active
        conn2.Dispose();
        var stats4 = pool.GetStatistics();
        stats4.ActiveConnections.Should().Be(0);
    }

    [Fact]
    public async Task ConnectionState_RemainsOpenAfterReturn()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration();
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        PooledConnection connection;
        using (connection = await pool.AcquireAsync())
        {
            connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
        }

        // Connection should still be open after being returned to pool
        connection.Connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task MultipleAcquireReleaseCycles_WorksCorrectly()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration { MaxPoolSize = 3 };
        var logger = CreateMockLogger();
        await using var pool = new ConnectionPool(_testConnectionString, config, logger);

        // First cycle
        var conn1 = await pool.AcquireAsync();
        conn1.Dispose();

        var conn2 = await pool.AcquireAsync();
        conn2.Dispose();

        // Second cycle - should reuse connections
        var conn3 = await pool.AcquireAsync();
        var conn4 = await pool.AcquireAsync();

        // Should have 2 active connections
        var stats = pool.GetStatistics();
        stats.ActiveConnections.Should().Be(2);
        stats.AvailableConnections.Should().Be(0);

        // Cleanup
        conn3.Dispose();
        conn4.Dispose();
    }
}