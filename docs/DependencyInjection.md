# DependencyInjection

The `DependencyInjection` class serves as the central extension point for configuring service registration and database initialization within the `dotnet-sqlite-crud-generator` runtime. It provides static methods to inject application-specific services into the .NET dependency injection container and to asynchronously provision the underlying SQLite database schema, ensuring that the generated CRUD operations have the necessary infrastructure and context to function correctly upon application startup.

## API

### AddApplicationServices

Registers the core application services required by the generated CRUD operations into the specified service collection. This method is overloaded to support various configuration scenarios, allowing callers to pass different combinations of parameters such as connection strings, configuration sections, or custom options objects to tailor the service lifetime and behavior.

*   **Parameters**: Varies by overload. Accepts an `IServiceCollection` instance as the primary target, followed by optional configuration arguments (e.g., `string`, `IConfiguration`, or specific option types) depending on the specific overload invoked.
*   **Return Value**: Returns the same `IServiceCollection` instance provided in the arguments, enabling fluent chaining of further service registrations.
*   **Throws**: Throws `ArgumentNullException` if the provided `IServiceCollection` is null. May throw `InvalidOperationException` if the service configuration provided in the arguments is invalid or conflicting.

### InitializeDatabaseAsync

Asynchronously initializes the SQLite database, ensuring that the schema exists and is up-to-date with the requirements of the generated entities. This method performs the necessary migrations or table creation logic before the application begins handling requests.

*   **Parameters**: Accepts an `IServiceProvider` instance used to resolve the database context and migration services required for initialization.
*   **Return Value**: Returns a `Task<IServiceProvider>` that completes when the database initialization is finished, yielding the same `IServiceProvider` passed in to allow for continued fluent setup in the host builder.
*   **Throws**: Throws `ArgumentNullException` if the `IServiceProvider` is null. Throws database-specific exceptions (e.g., `SqliteException`) if the connection fails, the file is locked, or the schema migration encounters an error.

## Usage

### Basic Service Registration and Database Initialization

The following example demonstrates the standard pattern for registering services and initializing the database within a minimal API entry point.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetSqliteCrudGenerator;

var builder = WebApplication.CreateBuilder(args);

// Register application services with a default connection string
builder.Services.AddApplicationServices("Data Source=app.db");

var app = builder.Build();

// Initialize the database schema before starting the web host
using (var scope = app.Services.CreateScope())
{
    await DependencyInjection.InitializeDatabaseAsync(scope.ServiceProvider);
}

app.Run();
```

### Fluent Configuration with Custom Options

This example illustrates chaining multiple `AddApplicationServices` overloads to configure specific behaviors before resolving the service provider for initialization.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DotNetSqliteCrudGenerator;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Chain service registrations using different overloads
services
    .AddApplicationServices(configuration)
    .AddApplicationServices("Data Source=production.db", options => 
    {
        options.EnableDetailedErrors = true;
    });

var serviceProvider = services.BuildServiceProvider();

// Perform asynchronous initialization
await DependencyInjection.InitializeDatabaseAsync(serviceProvider);
```

## Notes

*   **Method Overloading**: The `AddApplicationServices` method appears with multiple signatures to accommodate diverse configuration sources. Care must be taken to select the overload that matches the available configuration objects (e.g., raw connection strings vs. `IConfiguration` sections) to avoid runtime ambiguity or missing configuration values.
*   **Initialization Order**: `InitializeDatabaseAsync` must be invoked only after the `IServiceProvider` has been fully built and all services registered via `AddApplicationServices` are available. Calling this method prematurely with an incomplete service collection will result in resolution failures.
*   **Thread Safety**: All members of the `DependencyInjection` class are static. While the methods themselves do not maintain internal mutable state, the `IServiceCollection` and `IServiceProvider` instances passed to them are not thread-safe for modification during the registration and build phase. Ensure that service registration occurs on a single thread during application startup.
*   **Database Locking**: Since the underlying storage is SQLite, `InitializeDatabaseAsync` may fail if the database file is exclusively locked by another process. In high-concurrency deployment scenarios, ensure that file access permissions and locking strategies are configured to allow the initialization routine to acquire the necessary locks.
