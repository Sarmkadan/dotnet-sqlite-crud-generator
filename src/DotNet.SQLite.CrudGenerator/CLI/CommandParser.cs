#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using DotNet.SQLite.CrudGenerator.Exceptions;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Parses command-line arguments into structured command objects.
/// Supports global options, subcommands, and argument validation.
/// </summary>
public sealed class CommandParser
{
    private readonly Dictionary<string, Type> _commands = new(StringComparer.OrdinalIgnoreCase);

    public CommandParser RegisterCommand<T>(string name) where T : ICommand
    {
        _commands[name] = typeof(T);
        return this;
    }

    public async Task<int> ParseAndExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 0;
            }

            if (args[0] is "-h" or "--help" or "help")
            {
                PrintHelp();
                return 0;
            }

            if (args[0] is "-v" or "--version" or "version")
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"DotNet.SQLite.CrudGenerator v{version}");
                return 0;
            }

            var commandName = args[0];
            if (!_commands.TryGetValue(commandName, out var commandType))
            {
                Console.Error.WriteLine($"Unknown command: '{commandName}'");

                // Suggest the closest matching command if any
                var suggestion = FindClosestCommand(commandName);
                if (suggestion is not null)
                    Console.Error.WriteLine($"Did you mean '{suggestion}'?");

                PrintHelp();
                return 1;
            }

            var command = (ICommand)Activator.CreateInstance(commandType)!;
            var result = await command.ExecuteAsync(args.Skip(1).ToArray());
            return result;
        }
        catch (ValidationException ex)
        {
            Console.Error.WriteLine($"Validation Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Finds the closest matching registered command using Levenshtein distance.
    /// Returns null if no command is within a reasonable edit distance (3).
    /// </summary>
    private string? FindClosestCommand(string input)
    {
        const int maxDistance = 3;
        string? bestMatch = null;
        var bestDist = int.MaxValue;

        foreach (var cmd in _commands.Keys)
        {
            var dist = LevenshteinDistance(input.ToLowerInvariant(), cmd.ToLowerInvariant());
            if (dist < bestDist && dist <= maxDistance)
            {
                bestDist = dist;
                bestMatch = cmd;
            }
        }

        return bestMatch;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        var n = a.Length;
        var m = b.Length;
        var d = new int[n + 1, m + 1];

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }

    private void PrintHelp()
    {
        var helpText = @"
DotNet.SQLite.CrudGenerator - Generate CRUD operations from C# models

Usage: dotnet run <command> [options]

Commands:
  generate      Generate CRUD operations, migrations, and gRPC services
  migrate       Execute database migrations
  diff          Compare entity models against the live database schema
  grpc          Generate gRPC service definitions
  list          List all available models for generation
  validate      Validate model definitions
  stats         Show database statistics

Global Options:
  -h, --help        Show this help message
  -v, --version     Show version information
  -o, --output      Output directory for generated files
  -v, --verbose     Enable verbose logging

Examples:
  dotnet run generate --model User --output ./Generated
  dotnet run migrate --direction up
  dotnet run diff --verbose
  dotnet run grpc --service ProductService

For more information, visit: https://github.com/sarmkadan/dotnet-sqlite-crud-generator
";
        Console.WriteLine(helpText);
    }
}

public interface ICommand
{
    Task<int> ExecuteAsync(string[] args);
}
