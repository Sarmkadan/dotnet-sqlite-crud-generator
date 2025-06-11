#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command that computes the schema diff between registered entity models and the
/// live SQLite database and optionally writes an ALTER TABLE script to disk.
/// </summary>
public sealed class DiffCommand : ICommand
{
    private string _connectionString = "Data Source=crudgenerator.db";
    private string? _outputFile;
    private bool _verbose = false;

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            using var db = new DatabaseConnection(_connectionString);
            var svc = new MigrationDiffService(db);

            var entityTypes = new[]
            {
                typeof(DotNet.SQLite.CrudGenerator.Models.User),
                typeof(DotNet.SQLite.CrudGenerator.Models.Product),
                typeof(DotNet.SQLite.CrudGenerator.Models.Order),
                typeof(DotNet.SQLite.CrudGenerator.Models.Category),
                typeof(DotNet.SQLite.CrudGenerator.Models.AuditLog)
            };

            Console.WriteLine("Computing schema diffs...");
            Console.WriteLine();

            var allScripts = new System.Text.StringBuilder();
            var hasChanges = false;

            foreach (var entityType in entityTypes)
            {
                var diff = await svc.ComputeDiffAsync(entityType);

                if (diff.IsUpToDate)
                {
                    Console.WriteLine($"✓ {diff.TableName}: up to date");
                    continue;
                }

                hasChanges = true;
                Console.WriteLine($"⚠ {diff.TableName}: {diff.TableDiff.ColumnDiffs.Count} difference(s) found");

                if (_verbose)
                {
                    foreach (var col in diff.TableDiff.ColumnDiffs)
                    {
                        var label = col.Kind switch
                        {
                            ColumnDiffKind.Added => "+ ADDED",
                            ColumnDiffKind.Removed => "- REMOVED",
                            ColumnDiffKind.TypeChanged => "~ CHANGED",
                            _ => "?"
                        };
                        Console.WriteLine($"  {label}: {col.ColumnName}");
                    }
                }

                allScripts.AppendLine(diff.AlterScript);
            }

            Console.WriteLine();

            if (!hasChanges)
            {
                Console.WriteLine("✓ All tables are up to date.");
                return 0;
            }

            if (_outputFile is not null)
            {
                await File.WriteAllTextAsync(_outputFile, allScripts.ToString());
                Console.WriteLine($"✓ ALTER script written to: {_outputFile}");
            }
            else
            {
                Console.WriteLine("--- Generated ALTER Script ---");
                Console.WriteLine(allScripts.ToString());
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Diff failed: {ex.Message}");
            if (_verbose)
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private bool ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-c":
                case "--connection":
                    if (i + 1 >= args.Length) { Console.Error.WriteLine("--connection requires a value"); return false; }
                    _connectionString = args[++i];
                    break;

                case "-o":
                case "--output":
                    if (i + 1 >= args.Length) { Console.Error.WriteLine("--output requires a value"); return false; }
                    _outputFile = args[++i];
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

    private static void PrintHelp()
    {
        Console.WriteLine(@"
diff - Compare entity models against the live database schema

Usage: dotnet run diff [options]

Options:
  -c, --connection <cs>   SQLite connection string (default: Data Source=crudgenerator.db)
  -o, --output <file>     Write generated ALTER script to this file
  -v, --verbose           Show per-column diff details
  -h, --help              Show this help message

Examples:
  dotnet run diff
  dotnet run diff --verbose
  dotnet run diff --output ./Migrations/diff.sql
");
    }
}
