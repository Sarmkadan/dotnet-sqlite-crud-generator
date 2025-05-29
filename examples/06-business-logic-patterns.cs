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

/// <summary>Demonstrates real-world business logic patterns and operations</summary>
public class BusinessLogicPatternExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Example 6: Real-World Business Logic Patterns    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_business.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Pattern 1: Business Rule - Duplicate Detection
                Console.WriteLine("1️⃣  Business Logic: Duplicate Detection");
                var users = new[]
                {
                    new User { Username = "alice", Email = "alice@shop.com", FirstName = "Alice", LastName = "Smith", PasswordHash = "h1", IsActive = true },
                    new User { Username = "bob", Email = "bob@shop.com", FirstName = "Bob", LastName = "Johnson", PasswordHash = "h2", IsActive = true },
                };

                foreach (var user in users)
                    await userService.CreateAsync(user);

                // Check for duplicate
                var existing = await userService.FindAsync(u => u.Username == "alice");
                if (existing.Count > 0)
                {
                    Console.WriteLine($"   ⚠️  Duplicate user detected: {existing[0].Username}");
                    Console.WriteLine($"   Action: Skipped creation");
                }
                Console.WriteLine();

                // Pattern 2: Business Rule - Inventory Management
                Console.WriteLine("2️⃣  Business Logic: Inventory Management");
                var category = new Category
                {
                    Name = "Electronics",
                    Description = "Electronic equipment",
                    DisplayOrder = 1,
                    IsActive = true
                };
                category.GenerateSlug();
                var createdCat = await categoryService.AddAsync(category);

                var products = new[]
                {
                    new Product { Name = "Laptop", Price = 999m, StockQuantity = 5, CategoryId = createdCat.Id, Description = "High-end" },
                    new Product { Name = "Mouse", Price = 25m, StockQuantity = 2, CategoryId = createdCat.Id, Description = "Wireless" },
                    new Product { Name = "Monitor", Price = 299m, StockQuantity = 0, CategoryId = createdCat.Id, Description = "4K" },
                };

                foreach (var product in products)
                    await productService.CreateAsync(product);

                var lowStock = await productService.FindAsync(p => p.StockQuantity < 3);
                Console.WriteLine($"   ⚠️  Low stock items: {lowStock.Count}");
                foreach (var product in lowStock)
                {
                    Console.WriteLine($"      • {product.Name}: {product.StockQuantity} units");
                }

                var outOfStock = await productService.FindAsync(p => p.StockQuantity == 0);
                Console.WriteLine($"   ❌ Out of stock: {outOfStock.Count}");
                foreach (var product in outOfStock)
                {
                    Console.WriteLine($"      • {product.Name}");
                }
                Console.WriteLine();

                // Pattern 3: Business Rule - Revenue Calculation
                Console.WriteLine("3️⃣  Business Logic: Revenue Calculation");
                var allProducts = await productService.GetAllAsync();
                decimal totalRevenue = allProducts.Sum(p => p.Price * p.StockQuantity);
                Console.WriteLine($"   💰 Total inventory value: ${totalRevenue:F2}");

                var expensiveProducts = allProducts.Where(p => p.Price > 100).ToList();
                decimal expensiveValue = expensiveProducts.Sum(p => p.Price * p.StockQuantity);
                Console.WriteLine($"   💎 High-value items (>$100): {expensiveProducts.Count}");
                Console.WriteLine($"   💰 High-value inventory: ${expensiveValue:F2}");
                Console.WriteLine();

                // Pattern 4: Business Rule - Customer Purchase History
                Console.WriteLine("4️⃣  Business Logic: Customer Purchase History");
                using (var transaction = unitOfWork.BeginTransaction())
                {
                    try
                    {
                        var customer = new User
                        {
                            Username = "customer_001",
                            Email = "customer@shop.com",
                            FirstName = "John",
                            LastName = "Buyer",
                            PasswordHash = "secure_hash",
                            IsActive = true
                        };
                        var createdCustomer = await userService.CreateAsync(customer);

                        // Simulate multiple purchases
                        var orderDates = new[] { -30, -20, -10, 0 };
                        foreach (var daysAgo in orderDates)
                        {
                            var order = new Order
                            {
                                UserId = createdCustomer.Id,
                                OrderDate = DateTime.UtcNow.AddDays(daysAgo),
                                Status = "Completed",
                                TotalAmount = 50m + (daysAgo * -1)
                            };
                            await orderService.CreateAsync(order);
                        }

                        await transaction.CommitAsync();

                        var customerOrders = await orderService.GetUserOrdersAsync(createdCustomer.Id);
                        decimal totalSpent = customerOrders.Sum(o => o.TotalAmount);

                        Console.WriteLine($"   📊 Customer: {createdCustomer.Username}");
                        Console.WriteLine($"   🛒 Total orders: {customerOrders.Count}");
                        Console.WriteLine($"   💸 Total spent: ${totalSpent:F2}");
                        Console.WriteLine($"   📈 Average order: ${totalSpent / customerOrders.Count:F2}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"   ❌ Error: {ex.Message}");
                    }
                }
                Console.WriteLine();

                // Pattern 5: Business Rule - Promotion Logic
                Console.WriteLine("5️⃣  Business Logic: Promotion Eligibility");
                var bulkProducts = allProducts.Where(p => p.StockQuantity >= 10).ToList();
                Console.WriteLine($"   🎁 Eligible for bulk discount (stock >= 10): {bulkProducts.Count}");
                foreach (var product in bulkProducts)
                {
                    decimal discountedPrice = product.Price * 0.9m;
                    Console.WriteLine($"      • {product.Name}: ${product.Price:F2} → ${discountedPrice:F2} (10% off)");
                }
                Console.WriteLine();

                // Pattern 6: Business Rule - Data Quality Checks
                Console.WriteLine("6️⃣  Business Logic: Data Quality Validation");
                var allUsers = await userService.GetAllAsync();
                var inactiveUsers = allUsers.Where(u => !u.IsActive).ToList();
                var neverLoggedIn = allUsers.Where(u => u.LastLoginAt == null).ToList();

                Console.WriteLine($"   ✓ Total users: {allUsers.Count}");
                Console.WriteLine($"   ✓ Active users: {allUsers.Count - inactiveUsers.Count}");
                Console.WriteLine($"   ⚠️  Inactive users: {inactiveUsers.Count}");
                Console.WriteLine($"   ⚠️  Never logged in: {neverLoggedIn.Count}");
                Console.WriteLine();

                // Pattern 7: Business Rule - Archival Eligibility
                Console.WriteLine("7️⃣  Business Logic: Archive Candidates");
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var oldOrders = await orderService.FindAsync(o => o.CreatedAt < thirtyDaysAgo);
                Console.WriteLine($"   📦 Orders older than 30 days: {oldOrders.Count}");
                Console.WriteLine($"   📋 These are candidates for archival");
                Console.WriteLine();

                Console.WriteLine("✅ Business logic patterns demonstrated successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }
}
