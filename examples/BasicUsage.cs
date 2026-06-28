// =============================================================================
// Basic Usage Example for DotNet.SQLite.CrudGenerator
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>
/// Minimal setup and first call demonstrating CRUD operations.
/// </summary>
public sealed class BasicUsage
{
    public static async Task RunAsync()
    {
        // 1. Setup DI container
        var services = new ServiceCollection();
        
        // 2. Configure database path
        var settings = new DatabaseSettings { FilePath = "basic_example.db" };
        services.AddApplicationServices(settings.ConnectionString);
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 3. Initialize database
        await serviceProvider.InitializeDatabaseAsync();

        using (var scope = serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            // 4. Perform operation
            var user = new User { Username = "jdoe", Email = "jdoe@example.com" };
            var createdUser = await userService.CreateAsync(user);
            
            Console.WriteLine($"User created with ID: {createdUser.Id}");
        }
    }
}
