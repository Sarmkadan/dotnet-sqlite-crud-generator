#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Utilities;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command for displaying system and application statistics.
/// Shows performance metrics, memory usage, cache stats, and other diagnostics.
/// </summary>
public sealed class StatsCommand : ICommand
{
    private readonly PerformanceMonitor _performanceMonitor = new();
    private bool _verbose = false;

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            DisplayApplicationStats();
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
  -h, --help      Show this help message

Examples:
  dotnet run stats
  dotnet run stats -v
");
    }
}
