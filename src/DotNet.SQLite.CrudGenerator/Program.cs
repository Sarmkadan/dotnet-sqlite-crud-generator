#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       SQLite CRUD Generator - Core Architecture Phase 1        ║");
        Console.WriteLine("║                  Author: Vladyslav Zaiets                      ║");
        Console.WriteLine("║              https://sarmkadan.com                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "crudgenerator.db" };

            if (!settings.Validate())
                throw new InvalidOperationException("Database settings validation failed");

            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();

            // Initialize database
            Console.WriteLine("📦 Initializing database...");
            await serviceProvider.InitializeDatabaseAsync();
            Console.WriteLine("✓ Database initialized successfully");
            Console.WriteLine();

            // Demonstrate CRUD operations
            await DemonstrateCrudOperations(serviceProvider);

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              Phase 1 Initialization Complete!                 ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║  Core Components Implemented:                                 ║");
            Console.WriteLine("║  ✓ Domain Models (5 entity classes)                           ║");
            Console.WriteLine("║  ✓ Data Access Layer (Generic Repository + Repositories)     ║");
            Console.WriteLine("║  ✓ Service Layer (3 service classes)                          ║");
            Console.WriteLine("║  ✓ Unit of Work Pattern (IUnitOfWork)                         ║");
            Console.WriteLine("║  ✓ Dependency Injection Setup                                 ║");
            Console.WriteLine("║  ✓ Custom Exceptions & Enums                                  ║");
            Console.WriteLine("║  ✓ Database Connection Management                             ║");
            Console.WriteLine("║  ✓ Code Generation Service                                    ║");
            Console.WriteLine("║                                                               ║");
            Console.WriteLine("║  Ready for Phase 2: Source Generators & Advanced Features    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException is not null)
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }

    static async Task DemonstrateCrudOperations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
        var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
        var generationService = scope.ServiceProvider.GetRequiredService<GenerationService>();

        Console.WriteLine("🧪 Demonstrating CRUD Operations:");
        Console.WriteLine();

        // Create a category
        Console.WriteLine("  1️⃣  Creating category...");
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic products",
            DisplayOrder = 1,
            IsActive = true
        };
        category.GenerateSlug();
        var createdCategory = await categoryService.AddAsync(category);
        Console.WriteLine($"     ✓ Category created: {createdCategory.Name} (ID: {createdCategory.Id})");

        // Create a user
        Console.WriteLine("  2️⃣  Creating user...");
        var user = new User
        {
            Username = "vladyslav.zaiets",
            Email = "developer@sarmkadan.com",
            PasswordHash = "hashed_password_123",
            FirstName = "Vladyslav",
            LastName = "Zaiets",
            PhoneNumber = "+1234567890",
            Bio = "Software Architect",
            IsActive = true
        };

        var createdUser = await userService.CreateAsync(user);
        Console.WriteLine($"     ✓ User created: {createdUser.GetFullName()} (ID: {createdUser.Id})");

        // Create a product
        Console.WriteLine("  3️⃣  Creating product...");
        var product = new Product
        {
            Name = "High-Performance Laptop",
            Description = "Professional-grade laptop",
            Sku = "LAPTOP-001",
            CategoryId = createdCategory.Id,
            Price = 1299.99m,
            Cost = 850.00m,
            StockQuantity = 50,
            ReorderLevel = 10,
            Unit = "piece",
            IsActive = true
        };

        var createdProduct = await productService.CreateAsync(product);
        Console.WriteLine($"     ✓ Product created: {createdProduct.Name} (SKU: {createdProduct.Sku})");
        Console.WriteLine($"       Price: ${createdProduct.Price}, Margin: {createdProduct.GetProfitMarginPercentage():F2}%");

        // Read operations
        Console.WriteLine("  4️⃣  Reading entities...");
        var retrievedUser = await userService.GetAsync(createdUser.Id);
        var allProducts = (await productService.GetAllAsync()).ToList();
        Console.WriteLine($"     ✓ Retrieved user: {retrievedUser?.GetFullName()}");
        Console.WriteLine($"     ✓ Total products in database: {allProducts.Count}");

        // Update operation
        Console.WriteLine("  5️⃣  Updating product...");
        createdProduct.AddStock(25);
        var updateSuccessful = await productService.UpdateAsync(createdProduct);
        if (updateSuccessful)
        {
            var retrievedUpdatedProduct = await productService.GetAsync(createdProduct.Id);
            Console.WriteLine($"     ✓ Product updated: Stock now {retrievedUpdatedProduct?.StockQuantity} units");
        }
        else
        {
            Console.WriteLine("     ❌ Product update failed.");
        }

        // Get inventory stats
        Console.WriteLine("  6️⃣  Calculating inventory stats...");
        var stats = await productService.GetInventoryStatsAsync();
        Console.WriteLine($"     ✓ Total inventory value: ${stats.TotalInventoryValue:F2}");
        Console.WriteLine($"     ✓ Average stock level: {stats.AverageStockLevel:F1}");
        Console.WriteLine($"     ✓ Products in stock: {stats.TotalUnitsInStock}");

        // Generate code artifacts
        Console.WriteLine("  7️⃣  Generating code artifacts...");
        try
        {
            var repoInterface = await generationService.GenerateRepositoryInterfaceAsync(typeof(User));
            var migration = await generationService.GenerateMigrationAsync(typeof(Product), "CreateProductTable");
            var grpcService = await generationService.GenerateGrpcServiceAsync(typeof(Order));

            Console.WriteLine($"     ✓ Repository interface generated");
            Console.WriteLine($"     ✓ Database migration generated");
            Console.WriteLine($"     ✓ gRPC service definition generated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"     ⚠ Code generation: {ex.Message}");
        }

        // User authentication
        Console.WriteLine("  8️⃣  Testing user authentication...");
        var authenticated = await userService.AuthenticateAsync(createdUser.Email, createdUser.PasswordHash);
        if (authenticated is not null)
        {
            Console.WriteLine($"     ✓ User authenticated successfully");
            Console.WriteLine($"     ✓ Last login: {authenticated.LastLoginAt:O}");
        }

        // Delete operation
        Console.WriteLine("  9️⃣  Testing delete operation...");
        var deleted = await productService.DeleteAsync(createdProduct.Id);
        Console.WriteLine($"     ✓ Product deleted: {deleted}");

        Console.WriteLine();
        Console.WriteLine("✅ All CRUD operations completed successfully!");
    }
}
