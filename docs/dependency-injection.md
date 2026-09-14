# Dependency Injection

The `DependencyInjection` static class in
`src/DotNet.SQLite.CrudGenerator/Configuration/DependencyInjection.cs` centralizes
all service and repository registration for the application. It exposes several
overloads of the `AddApplicationServices` extension method so the container can be
wired up from a connection string, a `DatabaseSettings` object, an
`DotnetSqliteCrudGeneratorOptions` object, an `IConfiguration`, or a configuration
delegate. It also provides `InitializeDatabaseAsync` to create the SQLite schema at
startup.

## What gets registered

Every overload ultimately delegates to the `AddApplicationServices(IServiceCollection, string connectionString)`
overload, which registers the following:

### Database connection (singleton)

| Type | Lifetime | Notes |
|------|----------|-------|
| `DatabaseConnection` | Singleton | Constructed from the resolved connection string. |

### Logging

`services.AddLogging(logging => logging.AddConsole())` registers console logging.

### Unit of work

| Type | Lifetime | Notes |
|------|----------|-------|
| `IUnitOfWork` → `DbContextProvider` | Scoped | |

### Repositories

| Type | Lifetime |
|------|----------|
| `IRepository<User, int>` → `UserRepository` | Scoped |
| `IRepository<Product, int>` → `ProductRepository` | Scoped |
| `IRepository<Order, int>` → `OrderRepository` | Scoped |
| `IRepository<Category, int>` → `CategoryRepository` | Scoped |
| `IRepository<AuditLog, int>` → `AuditLogRepository` | Scoped |

### Services

| Type | Lifetime |
|------|----------|
| `UserService` | Scoped |
| `ProductService` | Scoped |
| `OrderService` | Scoped |
| `GenerationService` | Scoped |
| `MigrationDiffService` | Scoped |
| `QueryBuilderGenerationService` | Scoped |
| `AuditTrailService` | Scoped |

### Factory methods

Each repository is additionally registered through an explicit factory method that
resolves the singleton `DatabaseConnection` from the provider and constructs the
concrete repository with it:

```csharp
services.AddScoped(provider =>
{
    var db = provider.GetRequiredService<DatabaseConnection>();
    return new UserRepository(db);
});
```

## Extension methods

### `AddApplicationServices(this IServiceCollection services, string connectionString)`

The core overload. Registers everything described above. Throws
`ArgumentException` if the connection string is null or whitespace.

### `AddApplicationServices(this IServiceCollection services, DatabaseSettings settings)`

Recommended when configuring the database path from `appsettings.json` or another
`IConfiguration` source. Validates the settings (throws `ArgumentNullException` if
`settings` is null, and `ValidationException` if validation fails) and delegates
using `settings.ConnectionString`.

### `AddApplicationServices(this IServiceCollection services, DotnetSqliteCrudGeneratorOptions options)`

Recommended when using the `IOptions` pattern. Validates the options and delegates
using `options.Database.ConnectionString`.

### `AddApplicationServices(this IServiceCollection services, IConfiguration configuration)`

Reads the database configuration from the `Database` section of the supplied
`IConfiguration`. Binds the root configuration to `DotnetSqliteCrudGeneratorOptions`
(falling back to `DotnetSqliteCrudGeneratorOptions.CreateDefault()` if binding
yields null), validates it, registers the options for the `IOptions` pattern via
`services.Configure<DotnetSqliteCrudGeneratorOptions>(configuration)`, and delegates
using `options.Database.ConnectionString`.

### `AddApplicationServices(this IServiceCollection services, Action<DotnetSqliteCrudGeneratorOptions> configureOptions)`

Registers the options via `services.Configure(configureOptions)`, then builds a
temporary options instance to extract and validate the connection string before
delegating.

### `InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)`

Creates a scope, resolves the singleton `DatabaseConnection`, and calls
`InitializeDatabaseAsync(false, cancellationToken)` to create the required tables
and indexes. Returns the same `IServiceProvider`. Call this once at startup after
building the provider.

## Usage in Program.cs / Startup

### Minimal hosting model (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// From configuration (Database section of appsettings.json)
builder.Services.AddApplicationServices(builder.Configuration);

// Or from a DatabaseSettings object
var settings = builder.Configuration
    .GetSection(DatabaseSettings.SectionName)
    .Get<DatabaseSettings>() ?? new DatabaseSettings();
builder.Services.AddApplicationServices(settings);

// Or with an options delegate
builder.Services.AddApplicationServices(options =>
{
    options.Database.FilePath = "/var/data/myapp.db";
});

var app = builder.Build();

// Create the schema at startup
await app.Services.InitializeDatabaseAsync();

app.Run();
```

### Classic Startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApplicationServices(Configuration);
}
```

### appsettings.json

```json
{
  "Database": {
    "FilePath": "/var/data/myapp.db",
    "ConnectionTimeout": 30,
    "AutoCreateDatabase": true,
    "EnableLogging": false
  }
}
```

## Notes

*   **Null handling**: Every overload that accepts a reference type throws
    `ArgumentNullException` when the argument is null. The `string` overload throws
    `ArgumentException` for a null or whitespace connection string.
*   **Validation**: `DatabaseSettings.Validate()` and
    `DotnetSqliteCrudGeneratorOptions.Validate()` are invoked before registration;
    failures surface as `ValidationException` (from DataAnnotations) or the
    `ArgumentNullException`/`ArgumentException` guards above.
*   **Lifetimes**: The `DatabaseConnection` is a singleton; repositories, services,
    and the unit of work are scoped. Resolve scoped services from a scope (for
    example, an HTTP request or an explicit `CreateScope`) rather than the root
    provider.
*   **Connection string precedence**: In `DatabaseSettings`, an explicitly set
    `ConnectionString` takes precedence over `FilePath`. When only `FilePath` is
    set, the connection string is derived as
    `Data Source="{FilePath}";Version=3;`.