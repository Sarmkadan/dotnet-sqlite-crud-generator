#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.CLI;

/// <summary>
/// Extension methods for <see cref="ValidateCommand"/> that provide additional validation utilities.
/// </summary>
public static class ValidateCommandExtensions
{
    /// <summary>
    /// Validates a specific model type using the same logic as <see cref="ValidateCommand"/>.
    /// </summary>
    /// <param name="command">The validate command instance.</param>
    /// <param name="modelType">The model type to validate.</param>
    /// <param name="strict">Whether to use strict validation mode.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> or <paramref name="modelType"/> is <see langword="null"/>.</exception>
    /// <returns>List of validation results for the specified model.</returns>
    public static List<ValidationResult> ValidateModel(this ValidateCommand command, Type modelType, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(modelType);

        var results = new List<ValidationResult>();

        // Check class name
        if (!char.IsUpper(modelType.Name[0]))
        {
            results.Add(new ValidationResult
            {
                ModelName = modelType.Name,
                Message = "Class name should start with uppercase letter",
                Severity = strict ? ValidationSeverity.Error : ValidationSeverity.Warning
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
                Nullable.GetUnderlyingType(property.PropertyType) is null)
            {
                var hasRequired = property.GetCustomAttribute(typeof(RequiredAttribute)) is not null;
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
                    Severity = strict ? ValidationSeverity.Error : ValidationSeverity.Warning
                });
            }
        }

        // Check for parameterless constructor
        var hasParameterlessConstructor = modelType.GetConstructor(Type.EmptyTypes) is not null;
        if (!hasParameterlessConstructor)
        {
            results.Add(new ValidationResult
            {
                ModelName = modelType.Name,
                Message = "Class should have a parameterless constructor",
                Severity = ValidationSeverity.Warning
            });
        }

        return results;
    }

    /// <summary>
    /// Validates all models in the executing assembly using the same logic as <see cref="ValidateCommand"/>.
    /// </summary>
    /// <param name="command">The validate command instance.</param>
    /// <param name="strict">Whether to use strict validation mode.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is <see langword="null"/>.</exception>
    /// <returns>List of all validation results.</returns>
    public static List<ValidationResult> ValidateAllModels(this ValidateCommand command, bool strict = false)
    {
        ArgumentNullException.ThrowIfNull(command);

        var results = new List<ValidationResult>();
        var assembly = Assembly.GetExecutingAssembly();
        var modelsNamespace = "DotNet.SQLite.CrudGenerator.Models";

        var modelTypes = assembly.GetTypes()
            .Where(t => t.Namespace == modelsNamespace && !t.IsAbstract)
            .ToList();

        foreach (var modelType in modelTypes)
        {
            results.AddRange(command.ValidateModel(modelType, strict));
        }

        return results;
    }

    /// <summary>
    /// Filters validation results to only include those with Error severity.
    /// </summary>
    /// <param name="results">The validation results to filter.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is <see langword="null"/>.</exception>
    /// <returns>List of validation results with Error severity.</returns>
    public static List<ValidationResult> GetErrors(this List<ValidationResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Where(r => r.Severity == ValidationSeverity.Error).ToList();
    }

    /// <summary>
    /// Filters validation results to only include those with Warning severity.
    /// </summary>
    /// <param name="results">The validation results to filter.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is <see langword="null"/>.</exception>
    /// <returns>List of validation results with Warning severity.</returns>
    public static List<ValidationResult> GetWarnings(this List<ValidationResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Where(r => r.Severity == ValidationSeverity.Warning).ToList();
    }
}