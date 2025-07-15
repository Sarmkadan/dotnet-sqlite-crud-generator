## Performance

Microbenchmarks are located in `benchmarks/` and use [BenchmarkDotNet](https://benchmarkdotnet.org/).
Run them with:

```bash
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks \
--configuration Release -- --filter '*'
```

To run specific benchmark categories:

```bash
# Run all benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release

# Run only repository benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Repository*'

# Run only service benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Service*'

# Run only bulk operations benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Bulk*'

# Run only audit trail benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*Audit*'

# Run only migration diff benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*MigrationDiff*'

# Run only query builder benchmarks
dotnet run --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks --configuration Release -- --filter '*QueryBuilder*'
```

Results below were measured on an AMD Ryzen 9 5900X, .NET 10, Release build.

## Configuration

The DotNet SQLite CRUD Generator supports flexible configuration through multiple sources with the following priority order (highest to lowest):

1. Environment variables
2. Command-line arguments
3. User secrets (development only)
4. `appsettings.{Environment}.json`
5. `appsettings.json`
6. Hard-coded defaults

### Configuration Options

All configuration options are defined in the `DotnetSqliteCrudGeneratorOptions` class with DataAnnotations validation. See the complete list of available settings below:

| Section | Property | Type | Default | Description |
|---------|----------|------|---------|-------------|
| **Database** | FilePath | string | `crudgenerator.db` | SQLite database file path |
|  | ConnectionTimeout | int | 30 | Connection timeout in seconds |
|  | AutoCreateDatabase | bool | true | Create database automatically |
|  | EnableLogging | bool | false | Enable detailed logging |
|  | JournalMode | enum | Wal | SQLite journal mode (Off, Delete, Truncate, Persist, Memory, Wal) |
|  | SynchronousMode | enum | Normal | Data synchronization level (Off, Normal, Full, Extra) |
|  | MaxPoolSize | int | 10 | Maximum database connections |
|  | MinPoolSize | int | 1 | Minimum database connections |
|  | IdleTimeoutSeconds | int | 300 | Idle connection timeout in seconds |
| **ConnectionPool** | MinPoolSize | int | 1 | Minimum pool connections |
|  | MaxPoolSize | int | 10 | Maximum pool connections |
|  | IdleTimeoutSeconds | int | 300 | Idle connection timeout |
|  | AcquireTimeoutSeconds | int | 30 | Timeout for acquiring connections |
|  | CleanupIntervalSeconds | int | 60 | Pool cleanup interval |
|  | EnableDiagnostics | bool | false | Enable verbose diagnostics |
| **Cache** | Enabled | bool | true | Enable caching |
|  | MaxSizeBytes | long | 10,000,000 | Maximum cache size in bytes |
|  | DefaultTTL | int | 1800 | Default cache TTL in seconds |
|  | CleanupIntervalSeconds | int | 300 | Cache cleanup interval |
| **EventBus** | Enabled | bool | true | Enable event bus |
|  | MaxEventHistory | int | 1000 | Maximum event history to retain |
|  | PersistEvents | bool | false | Persist events to storage |
|  | EventStoragePath | string | `./eventstore` | Event storage directory |
| **HttpClient** | ConnectionLimit | int | 10 | Maximum concurrent connections |
|  | DefaultTimeout | int | 30 | Default request timeout in seconds |
|  | MaxRetries | int | 3 | Maximum retry attempts |
|  | RetryDelayMs | int | 1000 | Delay between retries in ms |
| **Webhook** | Enabled | bool | true | Enable webhooks |
|  | MaxRetries | int | 3 | Maximum retry attempts |
|  | RetryDelayMs | int | 5000 | Delay between retries in ms |
|  | MaxDeliveryHistorySize | int | 1000 | Maximum delivery history size |
|  | SigningSecret | string | - | Webhook signing secret |
| **BackgroundWorker** | Enabled | bool | true | Enable background workers |
|  | WorkerCount | int | 2 | Number of worker threads |
|  | MaxQueueSize | int | 1000 | Maximum queue size |
|  | TaskTimeoutSeconds | int | 300 | Task timeout in seconds |
|  | MaxRetries | int | 3 | Maximum retry attempts |
| **Migration** | MigrationsPath | string | `./migrations` | Migrations directory |
|  | BackupPath | string | `./backups` | Backup directory |
|  | CreateBackupBeforeMigration | bool | true | Create backup before migration |
|  | VerifyAfterMigration | bool | true | Verify after migration |
| **Validation** | ValidateOnCreate | bool | true | Validate on entity creation |
|  | ValidateOnUpdate | bool | true | Validate on entity update |
|  | MaxValidationErrors | int | 10 | Maximum validation errors to collect |
|  | ThrowOnValidationError | bool | true | Throw on validation errors |
| **Security** | EncryptConnectionString | bool | false | Encrypt connection string |
|  | RequireHttps | bool | false | Require HTTPS |
|  | AllowCors | array | [] | Allowed CORS origins |
|  | MaxRequestSize | long | 10,485,760 | Maximum request size in bytes |
| **Logging** | LogLevel.Default | string | Information | Default log level |
|  | LogLevel.Microsoft | string | Warning | Microsoft log level |
|  | LogLevel.MicrosoftEntityFrameworkCore | string | Warning | EF Core log level |
|  | Console.IncludeScopes | bool | false | Include scopes in console logs |
|  | Console.TimestampFormat | string | `yyyy-MM-dd HH:mm:ss` | Log timestamp format |

### Configuration Examples

#### Using appsettings.json

```json
{
  "Database": {
    "FilePath": "/var/lib/app/database.db",
    "ConnectionTimeout": 60,
    "JournalMode": "Wal",
    "MaxPoolSize": 50
  },
  "Cache": {
    "Enabled": true,
    "MaxSizeBytes": 100000000,
    "DefaultTTL": 3600
  }
}
```

#### Using Environment Variables

```bash
# Database settings
export DATABASE_FILEPATH="/data/app.db"
export DATABASE_CONNECTIONTIMEOUT=60
export DATABASE_MAXPOOLSIZE=50

# Cache settings
export CACHE_ENABLED=true
export CACHE_MAXSIZEBYTES=100000000
export CACHE_DEFAULTTTL=3600

# Logging
export LOGGING_LOGLEVEL_DEFAULT=Information
```

#### Using IOptions Pattern (Recommended)

```csharp
// In Program.cs or Startup.cs
builder.Services.AddApplicationServices(options =>
{
    options.Database.FilePath = "/data/production.db";
    options.Database.ConnectionTimeout = 60;
    options.Database.JournalMode = JournalMode.Wal;
    options.Cache.Enabled = true;
    options.Cache.MaxSizeBytes = 100_000_000;
});

// Then inject IOptions<DotnetSqliteCrudGeneratorOptions> where needed
```

#### Using IConfiguration

```csharp
// In Program.cs
builder.Services.AddApplicationServices(builder.Configuration);
```

### Validation


All configuration options are validated using DataAnnotations. Invalid configurations will throw a `ValidationException` during application startup.

```csharp
try
{
    var options = new DotnetSqliteCrudGeneratorOptions();
    options.Validate(); // Will throw if invalid
}
catch (ValidationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```