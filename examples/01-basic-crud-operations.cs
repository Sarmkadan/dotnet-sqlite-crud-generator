// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>Demonstrates basic CRUD operations with UserService</summary>
public class BasicCrudExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Example 1: Basic CRUD Operations                   ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_basic.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                // CREATE: Add new users
                Console.WriteLine("1️⃣  CREATE - Adding users to database...");
                var user1 = new User
                {
                    Username = "alice.smith",
                    Email = "alice@example.com",
                    FirstName = "Alice",
                    LastName = "Smith",
                    PasswordHash = "hashed_pwd_123",
                    IsActive = true
                };

                var createdUser1 = await userService.CreateAsync(user1);
                Console.WriteLine($"   ✓ Created user: {createdUser1.Username} (ID: {createdUser1.Id})");

                var user2 = new User
                {
                    Username = "bob.johnson",
                    Email = "bob@example.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    PasswordHash = "hashed_pwd_456",
                    IsActive = true
                };

                var createdUser2 = await userService.CreateAsync(user2);
                Console.WriteLine($"   ✓ Created user: {createdUser2.Username} (ID: {createdUser2.Id})");
                Console.WriteLine();

                // READ: Get single user by ID
                Console.WriteLine("2️⃣  READ - Retrieving user by ID...");
                var retrievedUser = await userService.GetByIdAsync(createdUser1.Id);
                if (retrievedUser != null)
                {
                    Console.WriteLine($"   ✓ Retrieved: {retrievedUser.Username}");
                    Console.WriteLine($"     Email: {retrievedUser.Email}");
                    Console.WriteLine($"     Active: {retrievedUser.IsActive}");
                }
                Console.WriteLine();

                // READ: Get all users
                Console.WriteLine("3️⃣  READ - Retrieving all users...");
                var allUsers = await userService.GetAllAsync();
                Console.WriteLine($"   ✓ Found {allUsers.Count} users");
                foreach (var u in allUsers)
                {
                    Console.WriteLine($"     - {u.Username} ({u.Email})");
                }
                Console.WriteLine();

                // UPDATE: Modify user
                Console.WriteLine("4️⃣  UPDATE - Updating user information...");
                retrievedUser!.Email = "alice.new@example.com";
                retrievedUser.LastName = "Williams";
                retrievedUser.UpdatedAt = DateTime.UtcNow;

                var updatedUser = await userService.UpdateAsync(retrievedUser);
                Console.WriteLine($"   ✓ Updated user: {updatedUser.Username}");
                Console.WriteLine($"     New email: {updatedUser.Email}");
                Console.WriteLine($"     Updated at: {updatedUser.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine();

                // DELETE: Remove user (soft delete)
                Console.WriteLine("5️⃣  DELETE - Removing user (soft delete)...");
                var deleted = await userService.DeleteAsync(createdUser2.Id);
                if (deleted)
                {
                    Console.WriteLine($"   ✓ Deleted user ID: {createdUser2.Id}");
                }

                var remainingUsers = await userService.GetAllAsync();
                Console.WriteLine($"   ✓ Remaining active users: {remainingUsers.Count}");
                Console.WriteLine();

                // SEARCH: Find users by criteria
                Console.WriteLine("6️⃣  SEARCH - Finding users by criteria...");
                var activeUsers = await userService.FindAsync(u => u.IsActive);
                Console.WriteLine($"   ✓ Found {activeUsers.Count} active users");

                var smithUsers = await userService.FindAsync(u => u.LastName == "Williams");
                Console.WriteLine($"   ✓ Found {smithUsers.Count} users with last name 'Williams'");
                Console.WriteLine();

                Console.WriteLine("✅ All CRUD operations completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
        }

        Console.WriteLine();
    }
}
