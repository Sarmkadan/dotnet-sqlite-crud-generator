// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Configuration Reference

Complete reference for all configuration options in SQLite CRUD Generator.

## Configuration Files

### appsettings.json

Main configuration file for application settings.

```json
{
  "DatabaseSettings": {
    "FilePath": "crudgenerator.db",
    "ConnectionTimeout": 30,
    "MaxPoolSize": 10
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 30,
    "MaxCacheSize": 100000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### appsettings.Development.json

Development-specific overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  },
  "DatabaseSettings": {
    "FilePath": "development.db"
  }
}
```

### appsettings.Production.json

Production-specific overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "DatabaseSettings": {
    "MaxPoolSize": 50,
    "ConnectionTimeout": 60
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 120
  }
}
```

## Configuration Sections

### DatabaseSettings

Controls database connection and behavior.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `FilePath` | string | `crudgenerator.db` | SQLite database file path (relative or absolute) |
| `ConnectionTimeout` | int | 30 | Connection timeout in seconds |
| `MaxPoolSize` | int | 10 | Maximum database connections in pool |
| `MinPoolSize` | int | 1 | Minimum database connections in pool |
| `IdleTimeout` | int | 300 | Idle connection timeout in seconds |
| `JournalMode` | string | `WAL` | SQLite journal mode (OFF, DELETE, TRUNCATE, PERSIST, MEMORY, WAL) |
| `SynchronousMode` | string | `NORMAL` | Data synchronization level (OFF, NORMAL, FULL, EXTRA) |

**Example**:
```csharp
var settings = new DatabaseSettings
{
    FilePath = "/data/production.db",
    ConnectionTimeout = 60,
    MaxPoolSize = 50
};
```

### CacheSettings

Controls application caching behavior.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Enabled` | bool | true | Enable/disable caching |
| `DefaultExpirationMinutes` | int | 60 | Default cache TTL in minutes |
| `SlidingExpirationMinutes` | int | 30 | Sliding expiration window |
| `MaxCacheSize` | int | 100000 | Maximum cache entries |
| `AbsoluteExpirationMinutes` | int | 120 | Absolute expiration limit |

**Caching Strategies**:
- **Absolute**: Expires after fixed time from insertion
- **Sliding**: Resets on each access
- **Hybrid**: Combination of both

**Example**:
```json
{
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 30,
    "MaxCacheSize": 50000
  }
}
```

### Logging

Controls application logging.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LogLevel.Default` | string | `Information` | Default log level |
| `LogLevel.{Category}` | string | - | Category-specific level |
| `Console.IncludeScopes` | bool | false | Include scope info |
| `Console.TimestampFormat` | string | - | Log timestamp format |

**Log Levels** (highest to lowest):
1. Critical - Unrecoverable errors
2. Error - Recoverable errors
3. Warning - Potential problems
4. Information - General information
5. Debug - Detailed debugging info
6. Trace - Very detailed information
7. None - Disable logging

**Example**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "DotNet.SQLite.CrudGenerator": "Debug"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss"
    }
  }
}
```

## Environment Variables

Override settings via environment variables:

```bash
# Database settings
export DATABASE_PATH=/var/lib/app/db.sqlite
export CONNECTION_TIMEOUT=60
export MAX_POOL_SIZE=50

# Cache settings
export CACHE_ENABLED=true
export CACHE_EXPIRATION=120

# Logging
export LOG_LEVEL=Information
```

**Variable Naming Convention**: `SECTION_SETTING` (e.g., `DATABASE_PATH` for `DatabaseSettings.FilePath`)

## Dependency Injection Configuration

### Default Configuration

```csharp
services.AddApplicationServices(connectionString);
```

Registers:
- Database connection
- All repositories
- All services
- Event bus
- Caching layer
- Middleware components

### Custom Configuration

```csharp
var settings = new DatabaseSettings
{
    FilePath = "custom.db",
    MaxPoolSize = 25
};

services.AddApplicationServices(settings.ConnectionString);

// Add custom services
services.AddSingleton<CustomService>();
services.AddScoped<CustomRepository>();
```

### Conditional Registration

```csharp
if (environment.IsProduction())
{
    services.Configure<CacheConfiguration>(options =>
    {
        options.Enabled = true;
        options.DefaultExpirationMinutes = 120;
    });
}
else
{
    services.Configure<CacheConfiguration>(options =>
    {
        options.Enabled = false;
    });
}
```

## Docker Configuration

### Environment-Based Configuration

```dockerfile
ENV DATABASE_PATH=/data/app.db
ENV CONNECTION_TIMEOUT=30
ENV LOG_LEVEL=Information
```

### Docker Compose

```yaml
environment:
  DATABASE_PATH: /data/app.db
  LOG_LEVEL: Information
  MAX_POOL_SIZE: 50
```

### Compose with Config Files

```yaml
volumes:
  - ./appsettings.json:/app/appsettings.json:ro
  - ./appsettings.Production.json:/app/appsettings.Production.json:ro
```

## Performance Tuning

### For Small Applications

```json
{
  "DatabaseSettings": {
    "MaxPoolSize": 5,
    "ConnectionTimeout": 15
  },
  "CacheSettings": {
    "Enabled": false
  }
}
```

### For Medium Applications

```json
{
  "DatabaseSettings": {
    "MaxPoolSize": 20,
    "ConnectionTimeout": 30
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 60,
    "MaxCacheSize": 50000
  }
}
```

### For Large Applications

```json
{
  "DatabaseSettings": {
    "MaxPoolSize": 50,
    "ConnectionTimeout": 60,
    "JournalMode": "WAL",
    "SynchronousMode": "NORMAL"
  },
  "CacheSettings": {
    "Enabled": true,
    "DefaultExpirationMinutes": 120,
    "SlidingExpirationMinutes": 60,
    "MaxCacheSize": 500000
  }
}
```

## SQLite Optimization

### Journal Mode

```json
"DatabaseSettings": {
  "JournalMode": "WAL"
}
```

- **WAL** (Write-Ahead Logging): Best for concurrent access
- **DELETE**: Default, slower for concurrent writes
- **MEMORY**: Fastest but requires sufficient RAM
- **TRUNCATE**: Moderate performance

### Synchronous Mode

```json
"DatabaseSettings": {
  "SynchronousMode": "NORMAL"
}
```

- **OFF**: Fastest, potential data loss
- **NORMAL**: Balanced performance and safety
- **FULL**: Maximum safety, slower writes
- **EXTRA**: Extra checks, slowest

## Migration Configuration

```csharp
public class MigrationSettings
{
    public string MigrationsPath { get; set; } = "./migrations";
    public string BackupPath { get; set; } = "./backups";
    public bool CreateBackupBeforeMigration { get; set; } = true;
    public bool VerifyAfterMigration { get; set; } = true;
}
```

## Validation Configuration

```csharp
public class ValidationSettings
{
    public bool ValidateOnCreate { get; set; } = true;
    public bool ValidateOnUpdate { get; set; } = true;
    public int MaxValidationErrors { get; set; } = 10;
    public bool ThrowOnValidationError { get; set; } = true;
}
```

## Security Configuration

```json
{
  "SecuritySettings": {
    "EncryptConnectionString": true,
    "RequireHttps": true,
    "AllowCors": ["https://trusted-domain.com"],
    "MaxRequestSize": 10485760
  }
}
```

## Configuration Loading Order

Configurations are loaded in this priority order (highest to lowest):

1. Environment variables
2. Command-line arguments
3. User secrets (development only)
4. appsettings.{Environment}.json
5. appsettings.json
6. Hard-coded defaults

**Example**:
```csharp
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();
```

## Common Configurations

### Development

- Logging: Debug level
- Cache: Disabled for rapid testing
- Database: In-memory or local file
- Validators: All enabled

### Testing

- Database: In-memory (`:memory:`)
- Cache: Disabled
- Logging: Warning or Error
- Timeout: Short (10s)

### Production

- Logging: Warning or Error
- Cache: Enabled with long TTL
- Database: Robust settings (WAL mode)
- Timeout: Long (60s)
- MaxPoolSize: Large (50+)

## Troubleshooting Configuration

### Config Not Loading

```csharp
// Verify configuration
var config = builder.Build();
Console.WriteLine($"FilePath: {config["DatabaseSettings:FilePath"]}");
Console.WriteLine($"CacheEnabled: {config["CacheSettings:Enabled"]}");
```

### Environment Variables Not Applied

```bash
# Check environment variables
printenv | grep DATABASE
printenv | grep CACHE

# Set correctly
export DATABASE_PATH=/var/lib/app.db
```

### Settings Not Taking Effect

1. Verify configuration loading order
2. Check for conflicting overrides
3. Ensure application restart
4. Clear cache if using IMemoryCache

## Configuration Validation

```csharp
var settings = new DatabaseSettings();
configuration.GetSection("DatabaseSettings").Bind(settings);

if (!settings.Validate())
{
    foreach (var error in settings.ValidationErrors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
}
```
