#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Extension methods for <see cref="DotnetSqliteCrudGeneratorOptions"/> that provide
/// convenient configuration helpers and fluent APIs for common scenarios.
/// </summary>
public static class DotnetSqliteCrudGeneratorOptionsExtensions
{
    /// <summary>
    /// Configures the database connection string with a custom value.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <param name="connectionString">The custom connection string to use.</param>
    /// <returns>The configured options for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public static DotnetSqliteCrudGeneratorOptions WithConnectionString(
        this DotnetSqliteCrudGeneratorOptions options,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Database.ConnectionString = connectionString;
        options.Database.FilePath = null;
        return options;
    }

    /// <summary>
    /// Configures the database to use an in-memory SQLite instance.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <returns>The configured options for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public static DotnetSqliteCrudGeneratorOptions WithInMemoryDatabase(
        this DotnetSqliteCrudGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Database.ConnectionString = "Data Source=:memory:";
        options.Database.FilePath = null;
        return options;
    }

    /// <summary>
    /// Configures the connection pool for development scenarios with more aggressive pooling.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <param name="maxPoolSize">Maximum pool size (default: 20).</param>
    /// <param name="idleTimeoutMinutes">Idle timeout in minutes (default: 2).</param>
    /// <returns>The configured options for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxPoolSize is less than 1.</exception>
    public static DotnetSqliteCrudGeneratorOptions WithDevelopmentPoolSettings(
        this DotnetSqliteCrudGeneratorOptions options,
        int maxPoolSize = 20,
        int idleTimeoutMinutes = 2)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (maxPoolSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPoolSize), "Max pool size must be at least 1");
        }

        if (idleTimeoutMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(idleTimeoutMinutes), "Idle timeout must be at least 1 minute");
        }

        options.ConnectionPool.MaxPoolSize = maxPoolSize;
        options.ConnectionPool.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutMinutes);
        options.ConnectionPool.MinPoolSize = Math.Min(options.ConnectionPool.MinPoolSize, maxPoolSize);
        return options;
    }

    /// <summary>
    /// Configures the cache to be disabled for testing or performance-critical scenarios.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <returns>The configured options for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public static DotnetSqliteCrudGeneratorOptions WithCacheDisabled(
        this DotnetSqliteCrudGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Cache.Enabled = false;
        return options;
    }

    /// <summary>
    /// Configures the background worker pool size based on available processor count.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <param name="multiplier">Worker count multiplier (default: 1).</param>
    /// <returns>The configured options for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when multiplier is less than 1.</exception>
    public static DotnetSqliteCrudGeneratorOptions WithProcessorBasedWorkerCount(
        this DotnetSqliteCrudGeneratorOptions options,
        int multiplier = 1)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (multiplier < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be at least 1");
        }

        var processorCount = Environment.ProcessorCount;
        options.BackgroundWorker.WorkerCount = processorCount * multiplier;
        return options;
    }

    /// <summary>
    /// Validates the configuration and throws a <see cref="ValidationException"/>
    /// with a formatted error message containing all validation failures.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ValidationException">Thrown when validation fails with all error messages.</exception>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public static void ValidateWithDetails(this DotnetSqliteCrudGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(options);

        if (!Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation failed").ToList();
            throw new ValidationException(
                $"Configuration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        // Validate nested configurations that don't use DataAnnotations
        options.Database.Validate();
        options.ConnectionPool.Validate();
    }

    /// <summary>
    /// Creates a new instance with all settings copied from the current instance.
    /// Useful for creating variations of a base configuration.
    /// </summary>
    /// <param name="options">The source options to clone.</param>
    /// <returns>A deep copy of the options.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public static DotnetSqliteCrudGeneratorOptions Clone(this DotnetSqliteCrudGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new DotnetSqliteCrudGeneratorOptions
        {
            Database = new DatabaseSettings
            {
                FilePath = options.Database.FilePath,
                ConnectionTimeout = options.Database.ConnectionTimeout,
                EnableLogging = options.Database.EnableLogging,
                AutoCreateDatabase = options.Database.AutoCreateDatabase
            },
            ConnectionPool = new ConnectionPoolConfiguration
            {
                MinPoolSize = options.ConnectionPool.MinPoolSize,
                MaxPoolSize = options.ConnectionPool.MaxPoolSize,
                IdleTimeout = options.ConnectionPool.IdleTimeout,
                AcquireTimeout = options.ConnectionPool.AcquireTimeout,
                CleanupInterval = options.ConnectionPool.CleanupInterval,
                EnableDiagnostics = options.ConnectionPool.EnableDiagnostics
            },
            Cache = new CacheConfiguration
            {
                Enabled = options.Cache.Enabled,
                MaxSizeBytes = options.Cache.MaxSizeBytes,
                DefaultTTL = options.Cache.DefaultTTL,
                CleanupIntervalSeconds = options.Cache.CleanupIntervalSeconds
            },
            EventBus = new EventBusConfiguration
            {
                Enabled = options.EventBus.Enabled,
                MaxEventHistory = options.EventBus.MaxEventHistory,
                PersistEvents = options.EventBus.PersistEvents,
                EventStoragePath = options.EventBus.EventStoragePath
            },
            HttpClient = new HttpClientConfiguration
            {
                ConnectionLimit = options.HttpClient.ConnectionLimit,
                DefaultTimeout = options.HttpClient.DefaultTimeout,
                MaxRetries = options.HttpClient.MaxRetries,
                RetryDelayMs = options.HttpClient.RetryDelayMs
            },
            Webhook = new WebhookConfiguration
            {
                Enabled = options.Webhook.Enabled,
                SigningSecret = options.Webhook.SigningSecret,
                MaxRetries = options.Webhook.MaxRetries,
                RetryDelayMs = options.Webhook.RetryDelayMs,
                MaxDeliveryHistorySize = options.Webhook.MaxDeliveryHistorySize
            },
            BackgroundWorker = new BackgroundWorkerConfiguration
            {
                Enabled = options.BackgroundWorker.Enabled,
                WorkerCount = options.BackgroundWorker.WorkerCount,
                MaxQueueSize = options.BackgroundWorker.MaxQueueSize,
                TaskTimeoutSeconds = options.BackgroundWorker.TaskTimeoutSeconds,
                MaxRetries = options.BackgroundWorker.MaxRetries
            }
        };
    }
}
