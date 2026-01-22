#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Configuration options for the SQLite connection pool.
/// Controls pool size, idle connection lifetime, and acquisition behaviour.
/// </summary>
public sealed class ConnectionPoolConfiguration
{
    /// <summary>The configuration section name used when binding from IConfiguration.</summary>
    public const string SectionName = "ConnectionPool";

    /// <summary>
    /// Gets or sets the minimum number of connections to keep alive during idle cleanup.
    /// These connections are preserved regardless of their idle duration.
    /// </summary>
    public int MinPoolSize { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of simultaneous connections the pool will allow.
    /// Attempts to acquire beyond this limit will block until a connection is returned
    /// or <see cref="AcquireTimeout"/> elapses.
    /// </summary>
    public int MaxPoolSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets how long an idle connection may sit in the pool before being closed.
    /// Connections used more recently than this threshold are retained (subject to
    /// <see cref="MinPoolSize"/>).
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets how long <see cref="Data.IConnectionPool.AcquireAsync"/> will wait for
    /// an available connection before throwing <see cref="TimeoutException"/>.
    /// </summary>
    public TimeSpan AcquireTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the interval between idle-connection cleanup sweeps.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets whether verbose diagnostics (per-connection lifecycle events)
    /// are written to the logger at Debug level.
    /// </summary>
    public bool EnableDiagnostics { get; set; } = false;

    /// <summary>
    /// Validates all settings and throws <see cref="InvalidOperationException"/> on
    /// any constraint violation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when any value is out of range.</exception>
    public void Validate()
    {
        if (MinPoolSize < 0)
            throw new InvalidOperationException($"{nameof(MinPoolSize)} must be non-negative.");

        if (MaxPoolSize < 1)
            throw new InvalidOperationException($"{nameof(MaxPoolSize)} must be at least 1.");

        if (MinPoolSize > MaxPoolSize)
            throw new InvalidOperationException($"{nameof(MinPoolSize)} cannot exceed {nameof(MaxPoolSize)}.");

        if (IdleTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException($"{nameof(IdleTimeout)} must be a positive duration.");

        if (AcquireTimeout <= TimeSpan.Zero)
            throw new InvalidOperationException($"{nameof(AcquireTimeout)} must be a positive duration.");

        if (CleanupInterval <= TimeSpan.Zero)
            throw new InvalidOperationException($"{nameof(CleanupInterval)} must be a positive duration.");
    }
}
