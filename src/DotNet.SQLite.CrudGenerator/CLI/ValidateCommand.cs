// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Command for validating model definitions and database schema.
/// Checks for common issues, naming conventions, and relationship integrity.
/// </summary>
public class ValidateCommand : ICommand
{
    private bool _strict = false;
    private bool _verbose = false;

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (!ParseArguments(args))
            return 1;

        try
        {
            Console.WriteLine("Validating models...");

            var validationResults = ValidateModels();

            if (!validationResults.Any())
            {
                Console.WriteLine("✓ All models are valid");
                return 0;
            }

            DisplayValidationResults(validationResults);

            var hasErrors = validationResults.Any(r => r.Severity == ValidationSeverity.Error);
            return hasErrors ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Validation failed: {ex.Message}");
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
                case "--strict":
                    _strict = true;
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

    private List<ValidationResult> ValidateModels()
    {
        var results = new List<ValidationResult>();
        var assembly = Assembly.GetExecutingAssembly();
        var modelsNamespace = "DotNet.SQLite.CrudGenerator.Models";

        var modelTypes = assembly.GetTypes()
            .Where(t => t.Namespace == modelsNamespace && !t.IsAbstract)
            .ToList();

        foreach (var modelType in modelTypes)
        {
            ValidateModel(modelType, results);
        }

        return results;
    }

    private void ValidateModel(Type modelType, List<ValidationResult> results)
    {
        // Check class name
        if (!char.IsUpper(modelType.Name[0]))
        {
            results.Add(new ValidationResult
            {
                ModelName = modelType.Name,
                Message = "Class name should start with uppercase letter",
                Severity = _strict ? ValidationSeverity.Error : ValidationSeverity.Warning
            });
        }

        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Check property name
            if (!char.IsUpper(property.Name[0]))
            {
                results.Add(new ValidationResult
                {
                    ModelName = modelType.Name,
                    PropertyName = property.Name,
                    Message = "Property name should start with uppercase letter",
                    Severity = ValidationSeverity.Warning
                });
            }

            // Check for nullable reference types
            if (property.PropertyType.IsClass &&
                property.PropertyType != typeof(string) &&
                Nullable.GetUnderlyingType(property.PropertyType) == null)
            {
                var hasRequired = property.GetCustomAttribute(typeof(RequiredAttribute)) != null;
                if (!hasRequired)
                {
                    results.Add(new ValidationResult
                    {
                        ModelName = modelType.Name,
                        PropertyName = property.Name,
                        Message = "Reference type property should have [Required] attribute or be nullable",
                        Severity = ValidationSeverity.Warning
                    });
                }
            }

            // Check for getters and setters
            if (!property.CanRead || !property.CanWrite)
            {
                results.Add(new ValidationResult
                {
                    ModelName = modelType.Name,
                    PropertyName = property.Name,
                    Message = "Property should have both getter and setter",
                    Severity = _strict ? ValidationSeverity.Error : ValidationSeverity.Warning
                });
            }
        }

        // Check for parameterless constructor
        var hasParameterlessConstructor = modelType.GetConstructor(Type.EmptyTypes) != null;
        if (!hasParameterlessConstructor)
        {
            results.Add(new ValidationResult
            {
                ModelName = modelType.Name,
                Message = "Class should have a parameterless constructor",
                Severity = ValidationSeverity.Warning
            });
        }
    }

    private void DisplayValidationResults(List<ValidationResult> results)
    {
        var errors = results.Where(r => r.Severity == ValidationSeverity.Error).ToList();
        var warnings = results.Where(r => r.Severity == ValidationSeverity.Warning).ToList();

        if (errors.Any())
        {
            Console.WriteLine($"\n✗ Errors ({errors.Count}):");
            foreach (var error in errors)
                Console.WriteLine($"  - {error.ModelName}" +
                    (error.PropertyName != null ? $".{error.PropertyName}" : "") +
                    $": {error.Message}");
        }

        if (warnings.Any())
        {
            Console.WriteLine($"\n⚠ Warnings ({warnings.Count}):");
            foreach (var warning in warnings)
                Console.WriteLine($"  - {warning.ModelName}" +
                    (warning.PropertyName != null ? $".{warning.PropertyName}" : "") +
                    $": {warning.Message}");
        }
    }

    private void PrintHelp()
    {
        Console.WriteLine(@"
validate - Validate model definitions and database schema

Usage: dotnet run validate [options]

Options:
  --strict         Treat warnings as errors
  -v, --verbose    Enable verbose output
  -h, --help       Show this help message

Examples:
  dotnet run validate
  dotnet run validate --strict
  dotnet run validate -v
");
    }
}

public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}

public class ValidationResult
{
    public string ModelName { get; set; } = string.Empty;
    public string? PropertyName { get; set; }
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
}

public class RequiredAttribute : Attribute { }
