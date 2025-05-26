#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Events;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>Demonstrates error handling and event-driven architecture</summary>
public sealed class ErrorHandlingAndEventsExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Example 5: Error Handling & Event-Driven Arch    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_errors.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var eventBus = scope.ServiceProvider.GetRequiredService<EventBus>();

                // Setup event handlers
                SubscribeToEvents(eventBus);

                // Example 1: Successful operation with events
                Console.WriteLine("1️⃣  Successful Operation - Events will be published");
                var user = new User
                {
                    Username = "event_user",
                    Email = "events@example.com",
                    FirstName = "Event",
                    LastName = "User",
                    PasswordHash = "hash",
                    IsActive = true
                };
                var createdUser = await userService.CreateAsync(user);
                Console.WriteLine($"   ✓ User created (should have triggered event)");
                Console.WriteLine();

                // Example 2: Validation exception
                Console.WriteLine("2️⃣  Validation Error - Invalid user data");
                try
                {
                    var invalidUser = new User
                    {
                        Username = "",  // Empty username
                        Email = "invalid",  // Invalid email
                        FirstName = "Invalid",
                        LastName = "User",
                        PasswordHash = "hash"
                    };
                    await userService.CreateAsync(invalidUser);
                }
                catch (ValidationException ex)
                {
                    Console.WriteLine($"   ⚠️  ValidationException caught");
                    Console.WriteLine($"   Message: {ex.Message}");
                }
                Console.WriteLine();

                // Example 3: Repository exception
                Console.WriteLine("3️⃣  Repository Error - Non-existent entity");
                try
                {
                    var nonExistent = await userService.GetByIdAsync(999999);
                    if (nonExistent is null)
                    {
                        Console.WriteLine($"   ℹ️  User not found (returns null instead of exception)");
                    }
                }
                catch (RepositoryException ex)
                {
                    Console.WriteLine($"   ⚠️  RepositoryException caught");
                    Console.WriteLine($"   Message: {ex.Message}");
                }
                Console.WriteLine();

                // Example 4: Multiple error types
                Console.WriteLine("4️⃣  Handling Multiple Error Types");
                var testCases = new (string name, Func<Task> operation)[]
                {
                    ("Valid user", async () =>
                    {
                        var u = new User { Username = "valid", Email = "v@e.c", FirstName = "V", LastName = "U", PasswordHash = "h", IsActive = true };
                        await userService.CreateAsync(u);
                    }),
                    ("Null operation", async () =>
                    {
                        var result = await userService.GetByIdAsync(-1);
                    }),
                    ("Update non-existent", async () =>
                    {
                        var u = new User { Id = 99999, Username = "test", Email = "t@e.c", FirstName = "T", LastName = "U", PasswordHash = "h" };
                        try
                        {
                            await userService.UpdateAsync(u);
                        }
                        catch (RepositoryException ex)
                        {
                            Console.WriteLine($"      └─ RepositoryException: {ex.Message}");
                        }
                    }),
                };

                foreach (var (name, operation) in testCases)
                {
                    try
                    {
                        await operation();
                        Console.WriteLine($"   ✓ {name}: Success");
                    }
                    catch (ValidationException ex)
                    {
                        Console.WriteLine($"   ⚠️  {name}: ValidationException - {ex.Message}");
                    }
                    catch (RepositoryException ex)
                    {
                        Console.WriteLine($"   ⚠️  {name}: RepositoryException - {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ {name}: {ex.GetType().Name} - {ex.Message}");
                    }
                }
                Console.WriteLine();

                // Example 5: Event-driven operations
                Console.WriteLine("5️⃣  Event-Driven Operations");
                var eventUser = new User
                {
                    Username = "event_test",
                    Email = "event@test.com",
                    FirstName = "Event",
                    LastName = "Test",
                    PasswordHash = "hash",
                    IsActive = true
                };

                Console.WriteLine("   Creating user (events will fire):");
                var createdEventUser = await userService.CreateAsync(eventUser);
                Console.WriteLine();

                Console.WriteLine("   Updating user (events will fire):");
                createdEventUser.Email = "newevent@test.com";
                var updatedUser = await userService.UpdateAsync(createdEventUser);
                Console.WriteLine();

                Console.WriteLine("   Deleting user (events will fire):");
                await userService.DeleteAsync(createdEventUser.Id);
                Console.WriteLine();

                // Example 6: Exception chain
                Console.WriteLine("6️⃣  Exception Chain Analysis");
                try
                {
                    // Simulate nested operation that fails
                    var users = await userService.GetAllAsync();
                    // Force an error
                    throw new InvalidOperationException("Simulated business logic error");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"   Exception Type: {ex.GetType().Name}");
                    Console.WriteLine($"   Message: {ex.Message}");
                    Console.WriteLine($"   Stack Trace (first 3 lines):");
                    var stackLines = ex.StackTrace?.Split('\n').Take(3) ?? Enumerable.Empty<string>();
                    foreach (var line in stackLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            Console.WriteLine($"     {line.Trim()}");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("✅ Error handling and event examples completed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static void SubscribeToEvents(EventBus eventBus)
    {
        eventBus.Subscribe<EntityChangedEvent>(async @event =>
        {
            Console.WriteLine($"   📣 Event received: {@event.EntityName} - {@event.OperationType}");
            await Task.CompletedTask;
        });
    }
}
