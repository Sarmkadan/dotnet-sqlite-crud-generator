#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Extension methods for registering <see cref="IConnectionPool"/> with the DI container.
/// </summary>
public static class ConnectionPoolExtensions
{
    /// <summary>
    /// Registers <see cref="ConnectionPool"/> as a singleton <see cref="IConnectionPool"/> using
    /// an optional inline configuration delegate.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="connectionString">SQLite connection string for all pooled connections.</param>
    /// <param name="configure">
    /// Optional delegate to customise pool settings.  When omitted the defaults defined in
    /// <see cref="ConnectionPoolConfiguration"/> are used.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddConnectionPool("Data Source=app.db", pool =>
    /// {
    ///     pool.MinPoolSize = 2;
    ///     pool.MaxPoolSize = 20;
    ///     pool.IdleTimeout = TimeSpan.FromMinutes(10);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddConnectionPool(
        this IServiceCollection services,
        string connectionString,
        Action<ConnectionPoolConfiguration>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        var config = new ConnectionPoolConfiguration();
        configure?.Invoke(config);

        return services.AddConnectionPool(connectionString, config);
    }

    /// <summary>
    /// Registers <see cref="ConnectionPool"/> as a singleton <see cref="IConnectionPool"/> using
    /// a pre-built <see cref="ConnectionPoolConfiguration"/> instance.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="connectionString">SQLite connection string for all pooled connections.</param>
    /// <param name="config">Fully populated pool configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddConnectionPool(
        this IServiceCollection services,
        string connectionString,
        ConnectionPoolConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        ArgumentNullException.ThrowIfNull(config);

        services.AddSingleton<IConnectionPool>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ConnectionPool>>();
            return new ConnectionPool(connectionString, config, logger);
        });

        return services;
    }

    /// <summary>
    /// Warms up the pool by pre-opening <paramref name="connectionCount"/> connections so that
    /// early requests do not pay the cold-start cost.  Call this once during application startup
    /// after the DI container is built.
    /// </summary>
    /// <param name="serviceProvider">The built service provider.</param>
    /// <param name="connectionCount">
    /// Number of connections to open.  Clamped to the pool's configured
    /// <see cref="ConnectionPoolConfiguration.MaxPoolSize"/>.
    /// </param>
    /// <param name="cancellationToken">Token to abort the warm-up.</param>
    public static async Task WarmUpConnectionPoolAsync(
        this IServiceProvider serviceProvider,
        int connectionCount = 1,
        CancellationToken cancellationToken = default)
    {
        var pool = serviceProvider.GetService<IConnectionPool>();
        if (pool is null)
            return;

        if (connectionCount < 1)
            connectionCount = 1;

        var leases = new List<PooledConnection>(connectionCount);

        try
        {
            for (int i = 0; i < connectionCount; i++)
            {
                var lease = await pool.AcquireAsync(cancellationToken);
                leases.Add(lease);
            }
        }
        finally
        {
            foreach (var lease in leases)
                await lease.DisposeAsync();
        }
    }
}
