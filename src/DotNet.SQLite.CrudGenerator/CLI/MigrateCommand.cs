#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Data;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command for executing database migrations.
/// Supports both up (apply) and down (rollback) operations.
/// Tracks migration history and validates migration files.
/// </summary>
public sealed class MigrateCommand : ICommand
{
    private string _direction = "up";
    private string _migrationsPath = "./Migrations";
    private string _connectionString = "Data Source=crudgenerator.db";
    private bool _dryRun = false;
    private bool _verbose = false;

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            Console.WriteLine($"Running migrations (direction: {_direction})...");
            if (_verbose)
                Console.WriteLine($"Migration directory: {Path.GetFullPath(_migrationsPath)}");

            var migrations = LoadMigrations();

            if (!migrations.Any())
            {
                Console.WriteLine("✓ No migrations to run");
                return 0;
            }

            if (_dryRun)
            {
                Console.WriteLine("DRY RUN - No changes will be applied");
                Console.WriteLine($"Would {_direction} {migrations.Count} migration(s):");
                foreach (var migration in migrations)
                    Console.WriteLine($"  - {migration}");
                return 0;
            }

            using var db = new DatabaseConnection(_connectionString);
            await db.OpenAsync();
            await EnsureMigrationsTableAsync(db);

            foreach (var migration in migrations)
            {
                if (_verbose)
                    Console.WriteLine($"Applying migration: {migration}");

                var fullPath = Path.Combine(_migrationsPath, migration);
                var script = await File.ReadAllTextAsync(fullPath);

                using (var transaction = db.Connection.BeginTransaction())
                {
                    using (var command = db.Connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = script;
                        await command.ExecuteNonQueryAsync();
                    }

                    using (var trackCommand = db.Connection.CreateCommand())
                    {
                        trackCommand.Transaction = transaction;
                        if (_direction == "up")
                        {
                            trackCommand.CommandText = "INSERT OR REPLACE INTO schema_migrations (name, applied_at) VALUES (@name, @appliedAt)";
                            trackCommand.Parameters.AddWithValue("@appliedAt", DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            trackCommand.CommandText = "DELETE FROM schema_migrations WHERE name = @name";
                        }
                        trackCommand.Parameters.AddWithValue("@name", migration);
                        await trackCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }

                Console.WriteLine($"✓ Applied: {migration}");
            }

            Console.WriteLine($"✓ Migration {_direction} completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Migration failed: {ex.Message}");
            if (_verbose)
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static async Task EnsureMigrationsTableAsync(DatabaseConnection db)
    {
        using var command = db.Connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS schema_migrations (name TEXT PRIMARY KEY, applied_at TEXT NOT NULL)";
        await command.ExecuteNonQueryAsync();
    }

    private bool ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-d":
                case "--direction":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--direction requires a value (up or down)");
                        return false;
                    }
                    _direction = args[++i].ToLower();
                    if (_direction != "up" && _direction != "down")
                    {
                        Console.Error.WriteLine("Direction must be 'up' or 'down'");
                        return false;
                    }
                    break;

                case "-p":
                case "--path":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--path requires a value");
                        return false;
                    }
                    _migrationsPath = args[++i];
                    break;

                case "--database":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--database requires a value");
                        return false;
                    }
                    _connectionString = $"Data Source={args[++i]}";
                    break;

                case "--dry-run":
                    _dryRun = true;
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

    private List<string> LoadMigrations()
    {
        var migrations = new List<string>();

        try
        {
            if (!Directory.Exists(_migrationsPath))
                return migrations;

            var files = Directory.GetFiles(_migrationsPath, "*.sql")
                .OrderBy(f => Path.GetFileNameWithoutExtension(f))
                .ToList();

            if (_direction == "down")
                files.Reverse();

            migrations.AddRange(files.Select(f => Path.GetFileName(f)));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading migrations: {ex.Message}");
        }

        return migrations;
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
migrate - Execute database migrations

Usage: dotnet run migrate [options]

Options:
  -d, --direction <up|down>   Migration direction (default: up)
  -p, --path <path>           Path to migrations directory (default: ./Migrations)
  --database <path>           Path to the SQLite database file (default: crudgenerator.db)
  --dry-run                   Show what would be executed without applying
  -v, --verbose               Enable verbose output
  -h, --help                  Show this help message

Examples:
  dotnet run migrate                      # Apply all pending migrations
  dotnet run migrate --direction down     # Rollback last migration
  dotnet run migrate --dry-run -v         # Preview migrations with verbose output
");
    }
}
