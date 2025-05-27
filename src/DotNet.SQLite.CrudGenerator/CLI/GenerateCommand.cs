// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command to generate CRUD operations, migrations, and gRPC services from C# models.
/// Supports filtering by model name and customizable output directories.
/// </summary>
public class GenerateCommand : ICommand
{
    private string? _modelName;
    private string _outputDirectory = "./Generated";
    private bool _generateGrpc = true;
    private bool _generateMigrations = true;
    private bool _verbose = false;

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            Console.WriteLine($"Generating CRUD operations...");
            if (_verbose)
                Console.WriteLine($"Model: {_modelName ?? "All"}, Output: {_outputDirectory}");

            var generationService = new GenerationService();
            var models = LoadModels();

            if (_modelName != null)
                models = models.Where(m => m.Name.Equals(_modelName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!models.Any())
            {
                Console.Error.WriteLine($"No models found matching: {_modelName}");
                return 1;
            }

            Directory.CreateDirectory(_outputDirectory);

            foreach (var model in models)
            {
                if (_verbose)
                    Console.WriteLine($"Processing model: {model.Name}");

                var repositoryCode = generationService.GenerateRepository(model);
                var serviceCode = generationService.GenerateService(model);

                WriteFile(Path.Combine(_outputDirectory, $"{model.Name}Repository.cs"), repositoryCode);
                WriteFile(Path.Combine(_outputDirectory, $"{model.Name}Service.cs"), serviceCode);

                if (_generateGrpc)
                {
                    var grpcCode = generationService.GenerateGrpcService(model);
                    WriteFile(Path.Combine(_outputDirectory, $"{model.Name}.proto"), grpcCode);
                }

                if (_generateMigrations)
                {
                    var migrationCode = generationService.GenerateMigration(model);
                    var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    WriteFile(Path.Combine(_outputDirectory, $"{timestamp}_Create{model.Name}.sql"), migrationCode);
                }

                Console.WriteLine($"✓ Generated for model: {model.Name}");
            }

            Console.WriteLine($"✓ Generation completed successfully. Output: {Path.GetFullPath(_outputDirectory)}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Generation failed: {ex.Message}");
            if (_verbose)
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private bool ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-m":
                case "--model":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--model requires a value");
                        return false;
                    }
                    _modelName = args[++i];
                    break;

                case "-o":
                case "--output":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("--output requires a value");
                        return false;
                    }
                    _outputDirectory = args[++i];
                    break;

                case "--no-grpc":
                    _generateGrpc = false;
                    break;

                case "--no-migrations":
                    _generateMigrations = false;
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
        var modelTypes = new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();
        var modelsNamespace = "DotNet.SQLite.CrudGenerator.Models";

        var types = assembly.GetTypes()
            .Where(t => t.Namespace == modelsNamespace && !t.IsAbstract)
            .ToList();

        return types;
    }

    private void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, content);
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
generate - Generate CRUD operations, migrations, and gRPC services

Usage: dotnet run generate [options]

Options:
  -m, --model <name>       Generate for specific model name
  -o, --output <path>      Output directory (default: ./Generated)
  --no-grpc                Skip gRPC service generation
  --no-migrations          Skip migration script generation
  -v, --verbose            Enable verbose output
  -h, --help               Show this help message

Examples:
  dotnet run generate --model User --output ./src/Generated
  dotnet run generate -m Product --no-migrations
  dotnet run generate --verbose
");
    }
}
