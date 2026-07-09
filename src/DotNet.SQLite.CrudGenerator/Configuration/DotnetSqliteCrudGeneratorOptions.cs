#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Root options class for the DotNet SQLite CRUD Generator application.
/// This class consolidates all configuration options and provides centralized access
/// to application settings through the IOptions pattern.
/// </summary>
/// <remarks>
/// All configuration properties are optional with sensible defaults to ensure
/// the application runs without requiring extensive configuration in development.
/// Production deployments should override these defaults with appropriate values.
/// </remarks>
public sealed class DotnetSqliteCrudGeneratorOptions
{
    /// <summary>
    /// Gets or sets the database configuration.
    /// </summary>
    [Required]
    public DatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// Gets or sets the connection pool configuration.
    /// </summary>
    public ConnectionPoolConfiguration ConnectionPool { get; set; } = new();

    /// <summary>
    /// Gets or sets the cache configuration.
    /// </summary>
    public CacheConfiguration Cache { get; set; } = new();

    /// <summary>
    /// Gets or sets the event bus configuration.
    /// </summary>
    public EventBusConfiguration EventBus { get; set; } = new();

    /// <summary>
    /// Gets or sets the HTTP client configuration.
    /// </summary>
    public HttpClientConfiguration HttpClient { get; set; } = new();

    /// <summary>
    /// Gets or sets the webhook configuration.
    /// </summary>
    public WebhookConfiguration Webhook { get; set; } = new();

    /// <summary>
    /// Gets or sets the background worker configuration.
    /// </summary>
    public BackgroundWorkerConfiguration BackgroundWorker { get; set; } = new();

    /// <summary>
    /// Validates all configuration options using DataAnnotations.
    /// Throws <see cref="ValidationException"/> if any validation fails.
    /// </summary>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public void Validate()
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this);

        if (!Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation failed").ToList();
            throw new ValidationException(string.Join(Environment.NewLine, errors));
        }
    }

    /// <summary>
    /// Creates a default configuration with sensible development defaults.
    /// </summary>
    public static DotnetSqliteCrudGeneratorOptions CreateDefault()
    {
        return new DotnetSqliteCrudGeneratorOptions
        {
            Database = new DatabaseSettings
            {
                FilePath = "crudgenerator.db",
                ConnectionTimeout = 30,
                AutoCreateDatabase = true,
                EnableLogging = false
            },
            ConnectionPool = new ConnectionPoolConfiguration
            {
                MinPoolSize = 1,
                MaxPoolSize = 10,
                IdleTimeout = TimeSpan.FromMinutes(5),
                AcquireTimeout = TimeSpan.FromSeconds(30),
                CleanupInterval = TimeSpan.FromMinutes(1),
                EnableDiagnostics = false
            },
            Cache = new CacheConfiguration
            {
                Enabled = true,
                MaxSizeBytes = 10_000_000, // 10 MB
                DefaultTTL = TimeSpan.FromMinutes(30),
                CleanupIntervalSeconds = 300
            },
            EventBus = new EventBusConfiguration
            {
                Enabled = true,
                MaxEventHistory = 1000,
                PersistEvents = false
            },
            HttpClient = new HttpClientConfiguration
            {
                ConnectionLimit = 10,
                DefaultTimeout = TimeSpan.FromSeconds(30),
                MaxRetries = 3,
                RetryDelayMs = 1000
            },
            Webhook = new WebhookConfiguration
            {
                Enabled = true,
                MaxRetries = 3,
                RetryDelayMs = 5000,
                MaxDeliveryHistorySize = 1000
            },
            BackgroundWorker = new BackgroundWorkerConfiguration
            {
                Enabled = true,
                WorkerCount = 2,
                MaxQueueSize = 1000,
                TaskTimeoutSeconds = 300,
                MaxRetries = 3
            }
        };
    }
}
