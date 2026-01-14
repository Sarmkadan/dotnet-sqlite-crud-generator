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

/// <summary>Demonstrates pagination and advanced search capabilities</summary>
public class PaginationSearchExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║       Example 3: Pagination & Advanced Search        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_pagination.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();

                // Create sample data
                Console.WriteLine("1️⃣  Creating sample data (25 users, 50 products)...");
                await CreateSampleData(userService, productService, categoryService);
                Console.WriteLine("   ✓ Sample data created");
                Console.WriteLine();

                // Example 1: User pagination
                Console.WriteLine("2️⃣  User Pagination - Page size 10");
                int pageSize = 10;
                int pageNumber = 1;
                int totalPages = 0;

                while (true)
                {
                    var (users, total) = await userService.GetPagedAsync(pageNumber, pageSize);
                    totalPages = (int)Math.Ceiling((double)total / pageSize);

                    Console.WriteLine($"   Page {pageNumber} of {totalPages} (Total: {total} users)");
                    for (int i = 0; i < users.Count; i++)
                    {
                        var user = users[i];
                        int itemNumber = (pageNumber - 1) * pageSize + i + 1;
                        Console.WriteLine($"     {itemNumber:D2}. {user.Username.PadRight(20)} - {user.Email}");
                    }

                    if (pageNumber >= totalPages) break;
                    pageNumber++;
                    Console.WriteLine();
                }
                Console.WriteLine();

                // Example 2: Search by criteria
                Console.WriteLine("3️⃣  Search - Finding users by criteria");
                var activeUsers = await userService.FindAsync(u => u.IsActive);
                Console.WriteLine($"   ✓ Active users: {activeUsers.Count}");

                var exampleComUsers = await userService.FindAsync(u => u.Email.Contains("@example.com"));
                Console.WriteLine($"   ✓ Users with @example.com: {exampleComUsers.Count}");

                var recentUsers = await userService.FindAsync(u =>
                    u.CreatedAt >= DateTime.UtcNow.AddDays(-7));
                Console.WriteLine($"   ✓ Users created in last 7 days: {recentUsers.Count}");
                Console.WriteLine();

                // Example 3: Product pagination and search
                Console.WriteLine("4️⃣  Product Pagination & Search - Page size 15");
                pageSize = 15;
                pageNumber = 1;

                var (firstPageProducts, totalProducts) = await productService.GetPagedAsync(pageNumber, pageSize);
                totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                Console.WriteLine($"   Showing products {(pageNumber - 1) * pageSize + 1} to {pageNumber * pageSize} of {totalProducts}");
                foreach (var product in firstPageProducts)
                {
                    Console.WriteLine($"     • {product.Name.PadRight(25)} - ${product.Price:F2} (Stock: {product.StockQuantity})");
                }
                Console.WriteLine();

                // Example 4: Price range search
                Console.WriteLine("5️⃣  Product Search by Price Range");
                var budget = await productService.GetByPriceRangeAsync(10m, 50m);
                Console.WriteLine($"   ✓ Products $10-$50: {budget.Count}");

                var premium = await productService.GetByPriceRangeAsync(100m, 1000m);
                Console.WriteLine($"   ✓ Premium products $100-$1000: {premium.Count}");

                var expensive = await productService.FindAsync(p => p.Price > 500m);
                Console.WriteLine($"   ✓ Very expensive (>$500): {expensive.Count}");
                Console.WriteLine();

                // Example 5: Low stock search
                Console.WriteLine("6️⃣  Inventory Analysis - Low Stock Items");
                var lowStock = await productService.FindAsync(p => p.StockQuantity < 5);
                Console.WriteLine($"   ✓ Low stock items (<5): {lowStock.Count}");

                if (lowStock.Count > 0)
                {
                    Console.WriteLine("     Low stock products:");
                    foreach (var product in lowStock.Take(5))
                    {
                        Console.WriteLine($"       • {product.Name} - Only {product.StockQuantity} left");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("✅ Pagination and search examples completed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task CreateSampleData(
        UserService userService,
        ProductService productService,
        CategoryRepository categoryService)
    {
        // Create users
        for (int i = 1; i <= 25; i++)
        {
            var user = new User
            {
                Username = $"user_{i:D3}",
                Email = $"user{i:D3}@example.com",
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                PasswordHash = "hash",
                IsActive = i % 3 != 0 // Make some inactive
            };
            await userService.CreateAsync(user);
        }

        // Create category
        var category = new Category
        {
            Name = "General",
            Description = "General products",
            DisplayOrder = 1,
            IsActive = true
        };
        category.GenerateSlug();
        var createdCategory = await categoryService.AddAsync(category);

        // Create products
        var prices = new[] { 9.99m, 19.99m, 29.99m, 49.99m, 99.99m, 199.99m, 499.99m };
        for (int i = 1; i <= 50; i++)
        {
            var product = new Product
            {
                Name = $"Product {i:D3}",
                Description = $"Description for product {i}",
                Price = prices[i % prices.Length],
                StockQuantity = (i % 20) + 1, // 1-20
                CategoryId = createdCategory.Id
            };
            await productService.CreateAsync(product);
        }
    }
}
