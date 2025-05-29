// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Formatters;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>Demonstrates data export to multiple formats (JSON, CSV, XML)</summary>
public class DataExportExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Example 4: Data Export (JSON/CSV/XML)        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_export.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();

                // Create sample data
                Console.WriteLine("1️⃣  Creating sample data...");
                await CreateExportSampleData(userService, productService, categoryService);
                Console.WriteLine("   ✓ Sample data created");
                Console.WriteLine();

                // Example 1: Export users to JSON
                Console.WriteLine("2️⃣  Exporting users to JSON");
                var allUsers = await userService.GetAllAsync();
                var jsonFormatter = new JsonFormatter();
                string usersJson = await jsonFormatter.FormatAsync(allUsers);

                string usersJsonPath = "users_export.json";
                await File.WriteAllTextAsync(usersJsonPath, usersJson);
                Console.WriteLine($"   ✓ Exported {allUsers.Count} users to {usersJsonPath}");
                Console.WriteLine($"   File size: {new FileInfo(usersJsonPath).Length} bytes");
                Console.WriteLine("   Sample (first 200 chars):");
                Console.WriteLine($"   {usersJson.Substring(0, Math.Min(200, usersJson.Length))}...");
                Console.WriteLine();

                // Example 2: Export products to CSV
                Console.WriteLine("3️⃣  Exporting products to CSV");
                var allProducts = await productService.GetAllAsync();
                var csvFormatter = new CsvFormatter();
                string productsCsv = await csvFormatter.FormatAsync(allProducts);

                string productsCsvPath = "products_export.csv";
                await File.WriteAllTextAsync(productsCsvPath, productsCsv);
                Console.WriteLine($"   ✓ Exported {allProducts.Count} products to {productsCsvPath}");
                Console.WriteLine($"   File size: {new FileInfo(productsCsvPath).Length} bytes");
                Console.WriteLine("   Sample (first 3 lines):");
                foreach (var line in productsCsv.Split('\n').Take(3))
                {
                    Console.WriteLine($"   {line}");
                }
                Console.WriteLine();

                // Example 3: Export categories to XML
                Console.WriteLine("4️⃣  Exporting categories to XML");
                var allCategories = await categoryService.GetAllAsync();
                var xmlFormatter = new XmlFormatter();
                string categoriesXml = await xmlFormatter.FormatAsync(allCategories);

                string categoriesXmlPath = "categories_export.xml";
                await File.WriteAllTextAsync(categoriesXmlPath, categoriesXml);
                Console.WriteLine($"   ✓ Exported {allCategories.Count} categories to {categoriesXmlPath}");
                Console.WriteLine($"   File size: {new FileInfo(categoriesXmlPath).Length} bytes");
                Console.WriteLine("   Sample (first 300 chars):");
                Console.WriteLine($"   {categoriesXml.Substring(0, Math.Min(300, categoriesXml.Length))}...");
                Console.WriteLine();

                // Example 4: Filtered export
                Console.WriteLine("5️⃣  Filtered export - Only active users");
                var activeUsers = await userService.FindAsync(u => u.IsActive);
                var activeUsersJson = await jsonFormatter.FormatAsync(activeUsers);

                string activeUsersPath = "active_users_export.json";
                await File.WriteAllTextAsync(activeUsersPath, activeUsersJson);
                Console.WriteLine($"   ✓ Exported {activeUsers.Count} active users to {activeUsersPath}");
                Console.WriteLine();

                // Example 5: Price range export
                Console.WriteLine("6️⃣  Filtered export - Products under $50");
                var affordableProducts = await productService.GetByPriceRangeAsync(0, 50);
                var affordableCsv = await csvFormatter.FormatAsync(affordableProducts);

                string affordablePath = "affordable_products.csv";
                await File.WriteAllTextAsync(affordablePath, affordableCsv);
                Console.WriteLine($"   ✓ Exported {affordableProducts.Count} affordable products to {affordablePath}");
                Console.WriteLine();

                // Summary
                Console.WriteLine("7️⃣  Export Summary");
                Console.WriteLine($"   Total files created: 5");
                Console.WriteLine($"   • {usersJsonPath}: {allUsers.Count} users");
                Console.WriteLine($"   • {productsCsvPath}: {allProducts.Count} products");
                Console.WriteLine($"   • {categoriesXmlPath}: {allCategories.Count} categories");
                Console.WriteLine($"   • {activeUsersPath}: {activeUsers.Count} active users");
                Console.WriteLine($"   • {affordablePath}: {affordableProducts.Count} products <$50");
                Console.WriteLine();

                // Cleanup
                Console.WriteLine("8️⃣  File verification");
                foreach (var file in new[] { usersJsonPath, productsCsvPath, categoriesXmlPath, activeUsersPath, affordablePath })
                {
                    if (File.Exists(file))
                    {
                        var fileInfo = new FileInfo(file);
                        Console.WriteLine($"   ✓ {file} ({fileInfo.Length} bytes, {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss})");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("✅ Data export examples completed!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task CreateExportSampleData(
        UserService userService,
        ProductService productService,
        CategoryRepository categoryService)
    {
        // Create users
        var users = new[]
        {
            new User { Username = "john.doe", Email = "john@example.com", FirstName = "John", LastName = "Doe", PasswordHash = "h1", IsActive = true },
            new User { Username = "jane.smith", Email = "jane@example.com", FirstName = "Jane", LastName = "Smith", PasswordHash = "h2", IsActive = true },
            new User { Username = "bob.wilson", Email = "bob@example.com", FirstName = "Bob", LastName = "Wilson", PasswordHash = "h3", IsActive = false },
            new User { Username = "alice.brown", Email = "alice@example.com", FirstName = "Alice", LastName = "Brown", PasswordHash = "h4", IsActive = true },
        };

        foreach (var user in users)
            await userService.CreateAsync(user);

        // Create categories
        var categories = new[]
        {
            new Category { Name = "Electronics", Description = "Electronic devices", DisplayOrder = 1, IsActive = true },
            new Category { Name = "Books", Description = "Books and publications", DisplayOrder = 2, IsActive = true },
        };

        foreach (var cat in categories)
        {
            cat.GenerateSlug();
            await categoryService.AddAsync(cat);
        }

        var catList = await categoryService.GetAllAsync();

        // Create products
        var products = new[]
        {
            new Product { Name = "Laptop", Price = 999.99m, StockQuantity = 10, CategoryId = catList[0].Id, Description = "High performance" },
            new Product { Name = "Mouse", Price = 25.99m, StockQuantity = 50, CategoryId = catList[0].Id, Description = "Wireless mouse" },
            new Product { Name = "C# Guide", Price = 45.99m, StockQuantity = 30, CategoryId = catList[1].Id, Description = "Programming book" },
            new Product { Name = "Keyboard", Price = 79.99m, StockQuantity = 20, CategoryId = catList[0].Id, Description = "Mechanical keyboard" },
        };

        foreach (var product in products)
            await productService.CreateAsync(product);
    }
}
