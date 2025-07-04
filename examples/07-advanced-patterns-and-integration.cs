#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>Demonstrates advanced patterns and integration scenarios</summary>
public sealed class AdvancedPatternsExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Example 7: Advanced Patterns & Integration        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            var services = new ServiceCollection();
            var settings = new DatabaseSettings { FilePath = "example_advanced.db" };
            services.AddApplicationServices(settings.ConnectionString);

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.InitializeDatabaseAsync();

            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
                var categoryService = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
                var auditLogRepo = scope.ServiceProvider.GetRequiredService<AuditLogRepository>();

                // Pattern 1: Bulk Operations with Progress Tracking
                Console.WriteLine("1️⃣  Advanced: Bulk Operations with Progress");
                await PerformBulkOperations(userService, categoryService, productService);
                Console.WriteLine();

                // Pattern 2: Performance Benchmarking
                Console.WriteLine("2️⃣  Advanced: Performance Benchmarking");
                await BenchmarkQueryPerformance(userService, productService);
                Console.WriteLine();

                // Pattern 3: Audit Trail Analysis
                Console.WriteLine("3️⃣  Advanced: Audit Trail Analysis");
                await AnalyzeAuditTrail(auditLogRepo);
                Console.WriteLine();

                // Pattern 4: Data Consistency Validation
                Console.WriteLine("4️⃣  Advanced: Data Consistency Validation");
                await ValidateDataConsistency(userService, productService);
                Console.WriteLine();

                // Pattern 5: Complex Filtering with Aggregation
                Console.WriteLine("5️⃣  Advanced: Complex Filtering & Aggregation");
                await PerformComplexAnalysis(productService, userService);
                Console.WriteLine();

                // Pattern 6: Batch Processing with Error Recovery
                Console.WriteLine("6️⃣  Advanced: Batch Processing with Error Recovery");
                await ProcessBatchWithRecovery(userService);
                Console.WriteLine();

                Console.WriteLine("✅ Advanced patterns demonstrated successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static async Task PerformBulkOperations(
        UserService userService,
        CategoryRepository categoryService,
        ProductService productService)
    {
        var stopwatch = Stopwatch.StartNew();
        var batchSize = 50;
        var totalItems = 200;

        Console.WriteLine($"   Inserting {totalItems} items in batches of {batchSize}");

        for (int batch = 0; batch < totalItems; batch += batchSize)
        {
            var items = new List<User>();
            for (int i = batch; i < Math.Min(batch + batchSize, totalItems); i++)
            {
                items.Add(new User
                {
                    Username = $"bulk_user_{i:D4}",
                    Email = $"bulk{i:D4}@example.com",
                    FirstName = $"Bulk{i}",
                    LastName = "User",
                    PasswordHash = "hash",
                    IsActive = true
                });
            }

            foreach (var item in items)
                await userService.CreateAsync(item);

            int progress = Math.Min(batch + batchSize, totalItems);
            Console.WriteLine($"   ✓ Progress: {progress}/{totalItems} ({(progress * 100 / totalItems):D3}%)");
        }

        stopwatch.Stop();
        Console.WriteLine($"   ⏱️  Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   ⚡ Throughput: {(totalItems * 1000.0 / stopwatch.ElapsedMilliseconds):F0} items/sec");
    }

    private static async Task BenchmarkQueryPerformance(
        UserService userService,
        ProductService productService)
    {
        var operations = new (string name, Func<Task<long>> operation)[]
        {
            ("GetAllAsync (users)", async () =>
            {
                var sw = Stopwatch.StartNew();
                var result = await userService.GetAllAsync();
                sw.Stop();
                Console.WriteLine($"     Result: {result.Count} items");
                return sw.ElapsedMilliseconds;
            }),
            ("FindAsync with predicate", async () =>
            {
                var sw = Stopwatch.StartNew();
                var result = await userService.FindAsync(u => u.IsActive);
                sw.Stop();
                Console.WriteLine($"     Result: {result.Count} items");
                return sw.ElapsedMilliseconds;
            }),
            ("GetPagedAsync (products)", async () =>
            {
                var sw = Stopwatch.StartNew();
                var (result, total) = await productService.GetPagedAsync(1, 10);
                sw.Stop();
                Console.WriteLine($"     Result: {result.Count} items (of {total} total)");
                return sw.ElapsedMilliseconds;
            }),
        };

        foreach (var (name, operation) in operations)
        {
            Console.WriteLine($"   Benchmarking: {name}");
            var elapsed = await operation();
            Console.WriteLine($"   ⏱️  Elapsed: {elapsed}ms");
        }
    }

    private static async Task AnalyzeAuditTrail(AuditLogRepository auditLogRepo)
    {
        var allLogs = await auditLogRepo.GetAllAsync();
        if (allLogs.Count == 0)
        {
            Console.WriteLine("   ℹ️  No audit logs available yet");
            return;
        }

        var operationCounts = allLogs
            .GroupBy(l => l.OperationType)
            .Select(g => (type: g.Key, count: g.Count()));

        Console.WriteLine($"   📊 Total audit entries: {allLogs.Count}");
        foreach (var (type, count) in operationCounts)
        {
            Console.WriteLine($"      • {type}: {count} entries");
        }

        var entityTypeCounts = allLogs
            .GroupBy(l => l.EntityName)
            .Select(g => (entity: g.Key, count: g.Count()));

        Console.WriteLine($"   📋 Entries by entity type:");
        foreach (var (entity, count) in entityTypeCounts.Take(5))
        {
            Console.WriteLine($"      • {entity}: {count} changes");
        }
    }

    private static async Task ValidateDataConsistency(
        UserService userService,
        ProductService productService)
    {
        var users = await userService.GetAllAsync();
        var products = await productService.GetAllAsync();

        Console.WriteLine($"   ✓ User count: {users.Count}");
        Console.WriteLine($"   ✓ Product count: {products.Count}");

        var validUsers = users.Where(u =>
            !string.IsNullOrWhiteSpace(u.Username) &&
            !string.IsNullOrWhiteSpace(u.Email) &&
            u.CreatedAt > DateTime.MinValue).Count();

        Console.WriteLine($"   ✓ Valid users: {validUsers}/{users.Count}");

        var validProducts = products.Where(p =>
            !string.IsNullOrWhiteSpace(p.Name) &&
            p.Price > 0 &&
            p.CreatedAt > DateTime.MinValue).Count();

        Console.WriteLine($"   ✓ Valid products: {validProducts}/{products.Count}");

        if (validUsers == users.Count && validProducts == products.Count)
        {
            Console.WriteLine($"   ✅ All data is consistent");
        }
        else
        {
            Console.WriteLine($"   ⚠️  Data consistency issues detected");
        }
    }

    private static async Task PerformComplexAnalysis(
        ProductService productService,
        UserService userService)
    {
        var products = await productService.GetAllAsync();
        var users = await userService.GetAllAsync();

        if (products.Count == 0 || users.Count == 0)
        {
            Console.WriteLine("   ℹ️  Insufficient data for analysis");
            return;
        }

        // Price analysis
        var priceStats = new
        {
            Average = products.Average(p => p.Price),
            Min = products.Min(p => p.Price),
            Max = products.Max(p => p.Price),
            StdDev = CalculateStdDev(products.Select(p => (double)p.Price))
        };

        Console.WriteLine($"   💰 Price Statistics:");
        Console.WriteLine($"      Average: ${priceStats.Average:F2}");
        Console.WriteLine($"      Min: ${priceStats.Min:F2}");
        Console.WriteLine($"      Max: ${priceStats.Max:F2}");
        Console.WriteLine($"      Std Dev: ${priceStats.StdDev:F2}");

        // Stock analysis
        var totalStock = products.Sum(p => p.StockQuantity);
        var avgStock = products.Average(p => p.StockQuantity);
        var lowStockCount = products.Count(p => p.StockQuantity < 5);

        Console.WriteLine($"   📦 Inventory Statistics:");
        Console.WriteLine($"      Total stock: {totalStock} units");
        Console.WriteLine($"      Average per product: {avgStock:F1} units");
        Console.WriteLine($"      Low stock items: {lowStockCount}");

        // User demographics
        var activeCount = users.Count(u => u.IsActive);
        var inactiveCount = users.Count - activeCount;

        Console.WriteLine($"   👥 User Demographics:");
        Console.WriteLine($"      Active: {activeCount} ({(activeCount * 100.0 / users.Count):F1}%)");
        Console.WriteLine($"      Inactive: {inactiveCount} ({(inactiveCount * 100.0 / users.Count):F1}%)");
    }

    private static async Task ProcessBatchWithRecovery(UserService userService)
    {
        var batchItems = new[]
        {
            new { username = "batch_1", email = "batch1@test.com" },
            new { username = "batch_2", email = "batch2@test.com" },
            new { username = "batch_3", email = "batch3@test.com" },
        };

        int successCount = 0;
        int errorCount = 0;

        Console.WriteLine($"   Processing {batchItems.Length} items with error recovery");

        foreach (var item in batchItems)
        {
            try
            {
                var user = new User
                {
                    Username = item.username,
                    Email = item.email,
                    FirstName = item.username,
                    LastName = "Batch",
                    PasswordHash = "hash",
                    IsActive = true
                };

                await userService.CreateAsync(user);
                successCount++;
                Console.WriteLine($"   ✓ {item.username}: Success");
            }
            catch (Exception ex)
            {
                errorCount++;
                Console.WriteLine($"   ❌ {item.username}: Failed - {ex.Message}");
                // Continue processing despite error
            }
        }

        Console.WriteLine($"   📊 Results: {successCount} succeeded, {errorCount} failed");
    }

    private static double CalculateStdDev(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return 0;

        double avg = list.Average();
        double sumOfSquares = list.Sum(v => (v - avg) * (v - avg));
        return Math.Sqrt(sumOfSquares / list.Count);
    }
}
