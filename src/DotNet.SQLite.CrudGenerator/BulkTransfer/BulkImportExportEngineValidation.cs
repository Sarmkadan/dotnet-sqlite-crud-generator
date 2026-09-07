#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Provides validation helpers for <see cref="BulkImportExportEngine{T}"/> instances using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
/// <remarks>
/// Validates the configuration and state of bulk import/export engines to ensure
/// operations can be performed safely without runtime failures.
/// </remarks>
public static class BulkImportExportEngineValidation
{
    /// <summary>
    /// Adds a validation problem if the specified value is negative.
    /// </summary>
    /// <param name="result">The validation result to add problems to.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    private static void AddIfNegative(ref ValidationResult result, long value, string propertyName)
    {
        if (value < 0)
        {
            result = result.WithProblem(propertyName, $"{propertyName} cannot be negative.");
        }
    }

    /// <summary>
    /// Validates the specified bulk import/export engine and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate<T>(this BulkImportExportEngine<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        var stats = value.GetStatistics();

        // Validate statistics (should have reasonable values)
        AddIfNegative(ref result, stats.TotalImports, nameof(stats.TotalImports));
        AddIfNegative(ref result, stats.TotalExports, nameof(stats.TotalExports));
        AddIfNegative(ref result, stats.TotalRecordsImported, nameof(stats.TotalRecordsImported));
        AddIfNegative(ref result, stats.TotalRecordsExported, nameof(stats.TotalRecordsExported));
        AddIfNegative(ref result, stats.TotalErrors, nameof(stats.TotalErrors));
        AddIfNegative(ref result, stats.TotalBytesTransferred, nameof(stats.TotalBytesTransferred));

        // Validate last progress (if available)
        if (stats.LastProgress is not null)
        {
            AddIfNegative(ref result, stats.LastProgress.ProcessedCount, nameof(stats.LastProgress.ProcessedCount));
            AddIfNegative(ref result, stats.LastProgress.TotalCount, nameof(stats.LastProgress.TotalCount));
            AddIfNegative(ref result, stats.LastProgress.SucceededCount, nameof(stats.LastProgress.SucceededCount));
            AddIfNegative(ref result, stats.LastProgress.FailedCount, nameof(stats.LastProgress.FailedCount));
            AddIfNegative(ref result, stats.LastProgress.BytesTransferred, nameof(stats.LastProgress.BytesTransferred));

            if (stats.LastProgress.StartedAt == default)
            {
                result = result.WithProblem(nameof(stats.LastProgress.StartedAt), "Statistics.LastProgress.StartedAt cannot be default(DateTime).");
            }

            AddIfNegative(ref result, stats.LastProgress.CurrentBatch, nameof(stats.LastProgress.CurrentBatch));
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified bulk import/export engine is valid.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to check.</param>
    /// <returns>True if the engine is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid<T>(this BulkImportExportEngine<T> value) where T : class
        => value is not null && Validate(value).IsValid;

    /// <summary>
    /// Ensures that the specified bulk import/export engine is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown if the engine is invalid.</exception>
    public static void EnsureValid<T>(this BulkImportExportEngine<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate(value).ThrowIfInvalid(problems => new ValidationException(
            $"The bulk import/export engine is invalid. Problems: {string.Join(" ", problems.Select(p => p.Message))}",
            nameof(value)));
    }
}