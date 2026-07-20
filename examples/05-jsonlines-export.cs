#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Formatters;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Examples;

/// <summary>
/// Demonstrates JSON Lines export (one JSON object per line) for efficient streaming and processing
/// </summary>
public sealed class JsonLinesExportExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Example 5: JSON Lines Export (Streaming-friendly format)       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        try
        {
            // Setup
            var exportService = new DataExportService();

            // Create sample data
            Console.WriteLine("1️⃣ Creating sample user data...");
            var users = new List<User>
            {
                new User { Username = "alice.johnson", Email = "alice@company.com", FirstName = "Alice", LastName = "Johnson", IsActive = true },
                new User { Username = "bob.smith", Email = "bob@company.com", FirstName = "Bob", LastName = "Smith", IsActive = true },
                new User { Username = "charlie.brown", Email = "charlie@company.com", FirstName = "Charlie", LastName = "Brown", IsActive = false },
                new User { Username = "diana.prince", Email = "diana@company.com", FirstName = "Diana", LastName = "Prince", IsActive = true }
            };
            Console.WriteLine($" ✓ Created {users.Count} users");
            Console.WriteLine();

            // Example 1: Export to JSON Lines string
            Console.WriteLine("2️⃣ Exporting to JSON Lines string format...");
            var jsonLines = await exportService.ExportAsJsonLinesAsync(users);
            Console.WriteLine($" ✓ Exported {users.Count} users as JSON Lines");
            Console.WriteLine($" Output size: {jsonLines.Length} characters");
            Console.WriteLine();
            Console.WriteLine(" Sample (first 300 characters):");
            Console.WriteLine("---");
            Console.WriteLine(jsonLines.Substring(0, Math.Min(300, jsonLines.Length)));
            Console.WriteLine("...");
            Console.WriteLine("---");
            Console.WriteLine();

            // Example 2: Export to file
            Console.WriteLine("3️⃣ Exporting to JSON Lines file...");
            var filePath = "users_export.jsonl";
            await exportService.ExportAsJsonLinesToFileAsync(users, filePath);
            Console.WriteLine($" ✓ Exported to {filePath}");
            Console.WriteLine($" File size: {new FileInfo(filePath).Length} bytes");

            // Show file content
            Console.WriteLine();
            Console.WriteLine(" File content preview:");
            var fileLines = File.ReadAllLines(filePath);
            for (int i = 0; i < Math.Min(3, fileLines.Length); i++)
            {
                Console.WriteLine($"  Line {i + 1}: {fileLines[i]}");
            }
            if (fileLines.Length > 3)
            {
                Console.WriteLine($"  ... and {fileLines.Length - 3} more lines");
            }
            Console.WriteLine();

            // Example 3: Export to stream (for HTTP responses, etc.)
            Console.WriteLine("4️⃣ Exporting to stream (simulating HTTP response)...");
            using (var memoryStream = new MemoryStream())
            {
                await exportService.ExportAsJsonLinesToStreamAsync(users, memoryStream);
                memoryStream.Position = 0;
                using (var reader = new StreamReader(memoryStream))
                {
                    var streamContent = await reader.ReadToEndAsync();
                    Console.WriteLine($" ✓ Stream export successful ({streamContent.Length} characters)");
                }
            }
            Console.WriteLine();

            // Example 4: Compare with regular JSON
            Console.WriteLine("5️⃣ Comparing JSON Lines vs Regular JSON...");
            var regularJson = await exportService.ExportAsJsonAsync(users);
            var jsonLinesSize = jsonLines.Length;
            var regularJsonSize = regularJson.Length;
            var savings = regularJsonSize - jsonLinesSize;
            var savingsPercent = (double)savings / regularJsonSize * 100;

            Console.WriteLine($" Regular JSON: {regularJsonSize} bytes");
            Console.WriteLine($" JSON Lines:    {jsonLinesSize} bytes");
            Console.WriteLine($" Size savings:  {savings} bytes ({savingsPercent:F1}%)");
            Console.WriteLine();

            // Example 5: Processing JSON Lines efficiently
            Console.WriteLine("6️⃣ Processing JSON Lines efficiently (line-by-line)...");
            var lines = jsonLines.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int activeCount = 0;
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var user = System.Text.Json.JsonSerializer.Deserialize<User>(line);
                    if (user?.IsActive == true)
                    {
                        activeCount++;
                    }
                }
            }
            Console.WriteLine($" ✓ Processed {lines.Length} lines, found {activeCount} active users");
            Console.WriteLine();

            // Example 6: Filtered export
            Console.WriteLine("7️⃣ Filtered export - Only active users...");
            var activeUsers = users.Where(u => u.IsActive).ToList();
            var activeJsonLines = await exportService.ExportAsJsonLinesAsync(activeUsers);
            Console.WriteLine($" ✓ Exported {activeUsers.Count} active users as JSON Lines");
            Console.WriteLine();

            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("8️⃣ Cleanup: Test file removed");
            }

            Console.WriteLine();
            Console.WriteLine("✅ JSON Lines export examples completed successfully!");
            Console.WriteLine();
            Console.WriteLine("📝 Key Benefits of JSON Lines:");
            Console.WriteLine("   • Stream-friendly: Each line is a complete JSON object");
            Console.WriteLine("   • Fault-tolerant: One bad record doesn't break the entire file");
            Console.WriteLine("   • Efficient: No need to load entire array into memory");
            Console.WriteLine("   • Parseable: Process one record at a time");
            Console.WriteLine("   • Standard: RFC 8259 compliant");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
        }

        Console.WriteLine();
    }
}
