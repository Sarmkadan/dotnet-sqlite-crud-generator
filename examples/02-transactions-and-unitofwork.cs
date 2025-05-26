#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>Demonstrates transaction handling and Unit of Work pattern</summary>
public sealed class TransactionExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Example 2: Transactions & Unit of Work Pattern    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_transactions.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

                // Example 1: Successful transaction
                Console.WriteLine("1️⃣  Successful Transaction - Creating related entities");
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        // Create user
                        var user = new User
                        {
                            Username = "transaction_user",
                            Email = "txn@example.com",
                            FirstName = "Txn",
                            LastName = "User",
                            PasswordHash = "secure_hash",
                            IsActive = true
                        };
                        var createdUser = await userService.CreateAsync(user);
                        Console.WriteLine($"   ✓ User created: {createdUser.Username}");

                        // Create category
                        var category = new Category
                        {
                            Name = "Electronics",
                            Description = "Electronic devices",
                            DisplayOrder = 1,
                            IsActive = true
                        };
                        category.GenerateSlug();
                        var createdCategory = await categoryService.AddAsync(category);
                        Console.WriteLine($"   ✓ Category created: {createdCategory.Name}");

                        // Create product
                        var product = new Product
                        {
                            Name = "Laptop",
                            Description = "High-performance laptop",
                            Price = 999.99m,
                            StockQuantity = 10,
                            CategoryId = createdCategory.Id
                        };
                        var createdProduct = await productService.CreateAsync(product);
                        Console.WriteLine($"   ✓ Product created: {createdProduct.Name}");

                        await transaction.CommitAsync();
                        Console.WriteLine("   ✅ Transaction committed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Transaction error: {ex.Message}");
                        await transaction.RollbackAsync();
                        Console.WriteLine("   ↩️  Transaction rolled back");
                    }
                }
                Console.WriteLine();

                // Example 2: Failed transaction with rollback
                Console.WriteLine("2️⃣  Failed Transaction - Demonstrating rollback");
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var user1 = new User
                        {
                            Username = "failed_user_1",
                            Email = "failed1@example.com",
                            FirstName = "Failed",
                            LastName = "User1",
                            PasswordHash = "hash",
                            IsActive = true
                        };
                        var created1 = await userService.CreateAsync(user1);
                        Console.WriteLine($"   ✓ First user created: {created1.Username}");

                        // Intentionally throw error
                        throw new InvalidOperationException("Simulated error to trigger rollback");

                        // This code won't execute
                        #pragma warning disable CS0162
                        var user2 = new User
                        {
                            Username = "failed_user_2",
                            Email = "failed2@example.com",
                            FirstName = "Failed",
                            LastName = "User2",
                            PasswordHash = "hash",
                            IsActive = true
                        };
                        await userService.CreateAsync(user2);
                        #pragma warning restore CS0162
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Error occurred: {ex.Message}");
                        await transaction.RollbackAsync();
                        Console.WriteLine("   ↩️  Transaction rolled back - changes discarded");
                    }
                }
                Console.WriteLine();

                // Example 3: Nested operations
                Console.WriteLine("3️⃣  Complex Multi-Entity Transaction");
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var user = new User
                        {
                            Username = "order_customer",
                            Email = "customer@example.com",
                            FirstName = "John",
                            LastName = "Customer",
                            PasswordHash = "hash",
                            IsActive = true
                        };
                        var createdUser = await userService.CreateAsync(user);
                        Console.WriteLine($"   ✓ Customer created: {createdUser.Username}");

                        // Create multiple orders for same user
                        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                        var order1 = new Order
                        {
                            UserId = createdUser.Id,
                            OrderDate = DateTime.UtcNow,
                            Status = "Pending",
                            TotalAmount = 99.99m
                        };
                        var createdOrder1 = await orderService.CreateAsync(order1);
                        Console.WriteLine($"   ✓ Order 1 created: ID {createdOrder1.Id}");

                        var order2 = new Order
                        {
                            UserId = createdUser.Id,
                            OrderDate = DateTime.UtcNow.AddDays(1),
                            Status = "Pending",
                            TotalAmount = 149.99m
                        };
                        var createdOrder2 = await orderService.CreateAsync(order2);
                        Console.WriteLine($"   ✓ Order 2 created: ID {createdOrder2.Id}");

                        await transaction.CommitAsync();
                        Console.WriteLine("   ✅ All orders created within transaction");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Error: {ex.Message}");
                        await transaction.RollbackAsync();
                    }
                }
                Console.WriteLine();

                Console.WriteLine("✅ Transaction examples completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
        }

        Console.WriteLine();
    }
}
