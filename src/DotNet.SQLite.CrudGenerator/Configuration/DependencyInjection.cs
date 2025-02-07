// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Configuration;

/// <summary>
/// Dependency injection configuration for the application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application services and repositories.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Register database connection
        services.AddSingleton(new DatabaseConnection(connectionString));

        // Register unit of work
        services.AddScoped<IUnitOfWork, DbContextProvider>();

        // Register repositories
        services.AddScoped<IRepository<User, int>, UserRepository>();
        services.AddScoped<IRepository<Product, int>, ProductRepository>();
        services.AddScoped<IRepository<Order, int>, OrderRepository>();
        services.AddScoped<IRepository<Category, int>, CategoryRepository>();
        services.AddScoped<IRepository<AuditLog, int>, AuditLogRepository>();

        // Register services
        services.AddScoped<UserService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<GenerationService>();

        // Register factory methods
        services.AddScoped(provider =>
        {
            var db = provider.GetRequiredService<DatabaseConnection>();
            return new UserRepository(db);
        });

        services.AddScoped(provider =>
        {
            var db = provider.GetRequiredService<DatabaseConnection>();
            return new ProductRepository(db);
        });

        services.AddScoped(provider =>
        {
            var db = provider.GetRequiredService<DatabaseConnection>();
            return new OrderRepository(db);
        });

        services.AddScoped(provider =>
        {
            var db = provider.GetRequiredService<DatabaseConnection>();
            return new CategoryRepository(db);
        });

        services.AddScoped(provider =>
        {
            var db = provider.GetRequiredService<DatabaseConnection>();
            return new AuditLogRepository(db);
        });

        return services;
    }

    /// <summary>
    /// Initializes the database with required tables and indexes.
    /// </summary>
    public static async Task<IServiceProvider> InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseConnection>();
        await database.InitializeDatabaseAsync(cancellationToken);
        return serviceProvider;
    }
}
