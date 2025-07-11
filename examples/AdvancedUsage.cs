// =============================================================================
// Advanced Usage Example for DotNet.SQLite.CrudGenerator
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>
/// Demonstrates configuration, custom options, and error handling.
/// </summary>
public sealed class AdvancedUsage
{
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        
        // Configure with custom options (e.g., caching)
        var settings = new DatabaseSettings { FilePath = "advanced_example.db" };
        services.AddApplicationServices(settings.ConnectionString, options => 
        {
            options.EnableCaching = true;
            options.CacheDurationMinutes = 30;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.InitializeDatabaseAsync();

        using (var scope = serviceProvider.CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();

            try
            {
                // Attempt to create a user with invalid data
                var invalidUser = new User { Username = "", Email = "invalid" };
                await userService.CreateAsync(invalidUser);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Caught expected validation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught unexpected error: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }
}
