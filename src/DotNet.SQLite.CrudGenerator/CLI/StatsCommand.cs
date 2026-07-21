#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.IO;
using DotNet.SQLite.CrudGenerator.Utilities;
using DotNet.SQLite.CrudGenerator.Formatters;
using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command for displaying system and application statistics.
/// Shows performance metrics, memory usage, cache stats, and other diagnostics.
/// </summary>
public sealed class StatsCommand : ICommand
{
    private readonly PerformanceMonitor _performanceMonitor = new();
    private bool _verbose = false;
    private bool _json = false;

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            if (_json)
            {
                DisplayJsonStats();
            }
            else
            {
                DisplayApplicationStats();
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Failed to display stats: {ex.Message}");
            return 1;
        }
    }

    private bool ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-v":
                case "--verbose":
                    _verbose = true;
                    break;

                case "--json":
                    _json = true;
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

    private void DisplayApplicationStats()
    {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║     DotNet SQLite CRUD Generator Stats    ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        DisplaySystemInfo();
        Console.WriteLine();

        DisplayPerformanceMetrics();
        Console.WriteLine();

        if (_verbose)
        {
            DisplayDetailedStats();
            Console.WriteLine();
        }

        Console.WriteLine("Legend:");
        Console.WriteLine("  MB    = Megabytes");
        Console.WriteLine("  ms    = milliseconds");
        Console.WriteLine("  %     = percentage");
    }

    private void DisplayJsonStats()
    {
        // Gather basic system and performance information
        var memInfo = _performanceMonitor.GetMemoryInfo();
        var perfReport = _performanceMonitor.GetPerformanceReport();

        // Get row counts per table from the SQLite database (if accessible)
        var tableRowCounts = GetTableRowCounts();

        // Get database file size (if the file exists)
        long databaseFileSizeBytes = GetDatabaseFileSize();

        var statsObject = new
        {
            SystemInfo = new
            {
                memInfo.WorkingSetMB,
                memInfo.PrivateMemoryMB,
                memInfo.ThreadCount,
                memInfo.GCTotalMemoryMB,
                Framework = ".NET 10.0",
                OS = Environment.OSVersion.VersionString,
                Processors = Environment.ProcessorCount
            },
            Performance = new
            {
                Uptime = TimeSpan.FromSeconds(perfReport.UptimeSeconds).ToString(@"hh\:mm\:ss"),
                perfReport.TotalOperations,
                perfReport.TotalSuccessful,
                perfReport.TotalFailed,
                AverageResponseTimeMs = perfReport.AverageResponseTime,
                SlowestOperation = perfReport.SlowestOperation,
                FastestOperation = perfReport.FastestOperation
            },
            Database = new
            {
                FileSizeBytes = databaseFileSizeBytes,
                TableRowCounts = tableRowCounts
            }
        };

        var formatter = new JsonFormatter(pretty: true);
        string json = formatter.Format(statsObject);
        Console.WriteLine(json);
    }

    /// <summary>
    /// Attempts to locate the SQLite database file in the current directory.
    /// Returns its size in bytes, or 0 if not found.
    /// </summary>
    private long GetDatabaseFileSize()
    {
        // Common default database file name – adjust if your project uses a different name.
        const string defaultDbFileName = "app.db";

        string baseDir = AppContext.BaseDirectory;
        string dbPath = Path.Combine(baseDir, defaultDbFileName);

        if (File.Exists(dbPath))
        {
            return new FileInfo(dbPath).Length;
        }

        // If the default file is not present, try to locate any *.db file in the directory.
        var dbFiles = Directory.GetFiles(baseDir, "*.db");
        if (dbFiles.Length > 0)
        {
            return new FileInfo(dbFiles[0]).Length;
        }

        return 0;
    }

    /// <summary>
    /// Retrieves row counts for each user-defined table in the SQLite database.
    /// Returns an empty dictionary if the database cannot be accessed.
    /// </summary>
    private Dictionary<string, long> GetTableRowCounts()
    {
        var result = new Dictionary<string, long>();

        // Locate the database file using the same logic as GetDatabaseFileSize.
        const string defaultDbFileName = "app.db";
        string baseDir = AppContext.BaseDirectory;
        string dbPath = Path.Combine(baseDir, defaultDbFileName);

        if (!File.Exists(dbPath))
        {
            var dbFiles = Directory.GetFiles(baseDir, "*.db");
            if (dbFiles.Length > 0)
                dbPath = dbFiles[0];
            else
                return result; // No database file found.
        }

        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadOnly
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Get list of user tables (exclude SQLite internal tables)
            using var tablesCmd = connection.CreateCommand();
            tablesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

            using var reader = tablesCmd.ExecuteReader();
            var tableNames = new List<string>();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                tableNames.Add(name);
            }

            foreach (var table in tableNames)
            {
                using var countCmd = connection.CreateCommand();
                countCmd.CommandText = $"SELECT COUNT(*) FROM \"{table}\";";
                var count = (long)countCmd.ExecuteScalar()!;
                result[table] = count;
            }
        }
        catch
        {
            // Swallow any exceptions – returning an empty or partial result is acceptable for stats output.
        }

        return result;
    }

    private void DisplaySystemInfo()
    {
        Console.WriteLine("📊 System Information:");
        var memInfo = _performanceMonitor.GetMemoryInfo();

        Console.WriteLine($"  Working Set:      {memInfo.WorkingSetMB:N0} MB");
        Console.WriteLine($"  Private Memory:   {memInfo.PrivateMemoryMB:N0} MB");
        Console.WriteLine($"  Thread Count:     {memInfo.ThreadCount}");
        Console.WriteLine($"  GC Total Memory:  {memInfo.GCTotalMemoryMB:N0} MB");
        Console.WriteLine($"  Framework:        .NET 10.0");
        Console.WriteLine($"  OS:               {Environment.OSVersion.VersionString}");
        Console.WriteLine($"  Processors:       {Environment.ProcessorCount}");
    }

    private void DisplayPerformanceMetrics()
    {
        Console.WriteLine("⚡ Performance Metrics:");
        var report = _performanceMonitor.GetPerformanceReport();

        Console.WriteLine($"  Uptime:           {TimeSpan.FromSeconds(report.UptimeSeconds):hh\\:mm\\:ss}");
        Console.WriteLine($"  Total Operations: {report.TotalOperations}");
        Console.WriteLine($"  Successful:       {report.TotalSuccessful}");
        Console.WriteLine($"  Failed:           {report.TotalFailed}");
        Console.WriteLine($"  Avg Response:     {report.AverageResponseTime:F2} ms");

        if (report.SlowestOperation is not null)
            Console.WriteLine($"  Slowest:          {report.SlowestOperation.OperationName} ({report.SlowestOperation.AverageTime:F2} ms)");

        if (report.FastestOperation is not null)
            Console.WriteLine($"  Fastest:          {report.FastestOperation.OperationName} ({report.FastestOperation.AverageTime:F2} ms)");
    }

    private void DisplayDetailedStats()
    {
        Console.WriteLine("📈 Detailed Operation Metrics:");
        var metrics = _performanceMonitor.GetAllMetrics().ToList();

        if (!metrics.Any())
        {
            Console.WriteLine("  No operation metrics available");
            return;
        }

        Console.WriteLine("  Operation Name                | Count | Avg(ms) | Min(ms) | Max(ms) | Success %");
        Console.WriteLine("  " + new string('-', 80));

        foreach (var metric in metrics.Take(10))
        {
            var operationName = metric.OperationName.Length > 30
                ? metric.OperationName.Substring(0, 27) + "..."
                : metric.OperationName.PadRight(30);

            Console.WriteLine($"  {operationName} | {metric.ExecutionCount,5} | {metric.AverageTime,7:F2} | {metric.MinTime,7} | {metric.MaxTime,7} | {metric.SuccessRate,8:F1}%");
        }

        if (metrics.Count > 10)
            Console.WriteLine($"  ... and {metrics.Count - 10} more operations");
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
stats - Display application statistics and diagnostics

Usage: dotnet run stats [options]

Options:
  -v, --verbose   Show detailed operation metrics
  --json          Output statistics in JSON format
  -h, --help      Show this help message

Examples:
  dotnet run stats
  dotnet run stats -v
  dotnet run stats --json
");
    }
}
