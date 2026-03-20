#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Caching;

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Configuration for cache provider setup and options.
/// Allows configuring TTL, size limits, and cleanup policies.
/// </summary>
public sealed class CacheConfiguration
{
    public bool Enabled { get; set; } = true;
    public long MaxSizeBytes { get; set; } = 10_000_000; // 10 MB
    public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromMinutes(30);
    public int CleanupIntervalSeconds { get; set; } = 300;

    public static MemoryCacheProvider CreateProvider(CacheConfiguration config)
    {
        var provider = new MemoryCacheProvider(config.MaxSizeBytes);
        return provider;
    }
}

/// <summary>
/// Configuration for event bus setup.
/// </summary>
public sealed class EventBusConfiguration
{
    public bool Enabled { get; set; } = true;
    public int MaxEventHistory { get; set; } = 1000;
    public bool PersistEvents { get; set; } = false;
    public string? EventStoragePath { get; set; }
}

/// <summary>
/// Configuration for HTTP client factory.
/// </summary>
public sealed class HttpClientConfiguration
{
    public int ConnectionLimit { get; set; } = 10;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}

/// <summary>
/// Configuration for webhook handler.
/// </summary>
public sealed class WebhookConfiguration
{
    public bool Enabled { get; set; } = true;
    public string? SigningSecret { get; set; }
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 5000;
    public int MaxDeliveryHistorySize { get; set; } = 1000;
}

/// <summary>
/// Configuration for background workers.
/// </summary>
public sealed class BackgroundWorkerConfiguration
{
    public bool Enabled { get; set; } = true;
    public int WorkerCount { get; set; } = 2;
    public int MaxQueueSize { get; set; } = 1000;
    public int TaskTimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Composite configuration for all features.
/// </summary>
public sealed class ApplicationConfiguration
{
    public DatabaseSettings Database { get; set; } = new();
    public CacheConfiguration Cache { get; set; } = new();
    public EventBusConfiguration EventBus { get; set; } = new();
    public HttpClientConfiguration HttpClient { get; set; } = new();
    public WebhookConfiguration Webhook { get; set; } = new();
    public BackgroundWorkerConfiguration BackgroundWorker { get; set; } = new();

    public static ApplicationConfiguration LoadFromEnvironment()
    {
        return new ApplicationConfiguration
        {
            Database = new DatabaseSettings
            {
                ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? "Data Source=app.db"
            },
            Cache = new CacheConfiguration
            {
                Enabled = bool.TryParse(Environment.GetEnvironmentVariable("CACHE_ENABLED"), out var cacheEnabled) ? cacheEnabled : true,
                MaxSizeBytes = long.TryParse(Environment.GetEnvironmentVariable("CACHE_MAX_SIZE"), out var size) ? size : 10_000_000
            },
            EventBus = new EventBusConfiguration
            {
                Enabled = bool.TryParse(Environment.GetEnvironmentVariable("EVENTBUS_ENABLED"), out var eventBusEnabled) ? eventBusEnabled : true
            },
            BackgroundWorker = new BackgroundWorkerConfiguration
            {
                Enabled = bool.TryParse(Environment.GetEnvironmentVariable("WORKER_ENABLED"), out var workerEnabled) ? workerEnabled : true,
                WorkerCount = int.TryParse(Environment.GetEnvironmentVariable("WORKER_COUNT"), out var count) ? count : 2
            }
        };
    }

    public void Validate()
    {
        if (Database is null)
            throw new InvalidOperationException("Database configuration is required");

        if (string.IsNullOrEmpty(Database.ConnectionString))
            throw new InvalidOperationException("Database connection string is required");

        if (Cache.MaxSizeBytes <= 0)
            throw new InvalidOperationException("Cache max size must be greater than 0");

        if (BackgroundWorker.WorkerCount <= 0)
            throw new InvalidOperationException("Worker count must be at least 1");
    }
}
