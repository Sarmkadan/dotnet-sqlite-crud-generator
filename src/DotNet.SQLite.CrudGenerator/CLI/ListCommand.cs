#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using DotNet.SQLite.CrudGenerator.Utilities;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command for listing available models and their properties.
/// Displays naming conventions and relationship information.
/// </summary>
public sealed class ListCommand : ICommand
{
    private bool _verbose = false;
    private string? _filter = null;
    private string _format = "text"; // text or json

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            Console.WriteLine("Available Models:");
            Console.WriteLine();

            var models = LoadModels();

            if (_filter is not null)
                models = models.Where(m => m.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!models.Any())
            {
                Console.WriteLine("No models found");
                return 0;
            }

            if (_format == "json")
            {
                DisplayAsJson(models);
            }
            else
            {
                DisplayAsText(models);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Failed to list models: {ex.Message}");
            return 1;
        }
    }

    private bool ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-f":
                case "--filter":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--filter requires a value");
                        return false;
                    }
                    _filter = args[++i];
                    break;

                case "--format":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--format requires a value");
                        return false;
                    }
                    _format = args[++i].ToLower();
                    if (_format != "text" && _format != "json")
                    {
                        Console.Error.WriteLine("Format must be 'text' or 'json'");
                        return false;
                    }
                    break;

                case "-v":
                case "--verbose":
                    _verbose = true;
                    break;

                case "-h":
                case "--help":
                    PrintHelp();
                    return false;

                default:
                    Console.Error.WriteLine($"Unknown option: {args[i]}");
                    return false;
            }
        }

        return true;
    }

    private List<Type> LoadModels()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var modelsNamespace = "DotNet.SQLite.CrudGenerator.Models";

        return assembly.GetTypes()
            .Where(t => t.Namespace == modelsNamespace && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    private void DisplayAsText(List<Type> models)
    {
        foreach (var model in models)
        {
            var info = NamingConventionHelper.GetConventionInfo(model);

            Console.WriteLine($"📦 {info.EntityName}");
            Console.WriteLine($"   Table: {info.TableName}");
            Console.WriteLine($"   Endpoint: {info.ApiEndpoint}");
            Console.WriteLine($"   gRPC Service: {info.GrpcServiceName}");

            if (_verbose)
            {
                Console.WriteLine("   Properties:");
                foreach (var prop in info.Properties)
                {
                    Console.WriteLine($"     - {prop.PropertyName} ({prop.Type}) → {prop.ColumnName}");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Total models: {models.Count}");
    }

    private void DisplayAsJson(List<Type> models)
    {
        var jsonModels = models.Select(m => NamingConventionHelper.GetConventionInfo(m)).ToList();
        var json = System.Text.Json.JsonSerializer.Serialize(new { models = jsonModels }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
list - List available models and their properties

Usage: dotnet run list [options]

Options:
  -f, --filter <name>  Filter models by name
  --format <text|json> Output format (default: text)
  -v, --verbose        Show detailed property information
  -h, --help           Show this help message

Examples:
  dotnet run list
  dotnet run list --filter Product
  dotnet run list --format json
  dotnet run list -v
");
    }
}
