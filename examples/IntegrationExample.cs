// =============================================================================
// Integration Example for DotNet.SQLite.CrudGenerator
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>
/// Shows how to wire the generator into an ASP.NET Core or Generic Host DI.
/// </summary>
public sealed class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        // Integration point in Startup.cs or Program.cs
        services.AddApplicationServices(connectionString);
        
        // Now you can inject IRepository<T> or specific services into your controllers/services
    }

    public static async Task RunHostAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        
        ConfigureServices(builder.Services, "Data Source=integrated.db");
        
        using IHost host = builder.Build();
        
        // Start the host
        await host.StartAsync();
        
        Console.WriteLine("Application hosted and services registered.");
    }
}
