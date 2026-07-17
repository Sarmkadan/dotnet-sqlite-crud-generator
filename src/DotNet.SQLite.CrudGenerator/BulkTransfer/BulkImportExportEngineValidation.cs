#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.BulkTransfer;

/// <summary>
/// Provides validation helpers for <see cref="BulkImportExportEngine{T}"/> instances.
/// </summary>
/// <remarks>
/// Validates the configuration and state of bulk import/export engines to ensure
/// operations can be performed safely without runtime failures.
/// </remarks>
public static class BulkImportExportEngineValidation
{
    /// <summary>
    /// Adds an error message if the specified value is negative.
    /// </summary>
    /// <param name="errors">The list of error messages to add to.</param>
    /// <param name="value">The value to check.</param>
    /// <param name="propertyName">The name of the property being validated.</param>
    private static void AddIfNegative(ICollection<string> errors, long value, string propertyName)
    {
        if (value < 0)
        {
            errors.Add($"{propertyName} cannot be negative.");
        }
    }

    /// <summary>
    /// Validates the specified bulk import/export engine and returns a list of human-readable
    /// validation problems. Returns an empty list if the engine is valid.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to validate.</param>
    /// <returns>A read-only list of validation error messages; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this BulkImportExportEngine<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();


        var stats = value.GetStatistics();

        // Validate statistics (should have reasonable values)
        AddIfNegative(errors, stats.TotalImports, nameof(stats.TotalImports));
        AddIfNegative(errors, stats.TotalExports, nameof(stats.TotalExports));
        AddIfNegative(errors, stats.TotalRecordsImported, nameof(stats.TotalRecordsImported));
        AddIfNegative(errors, stats.TotalRecordsExported, nameof(stats.TotalRecordsExported));
        AddIfNegative(errors, stats.TotalErrors, nameof(stats.TotalErrors));
        AddIfNegative(errors, stats.TotalBytesTransferred, nameof(stats.TotalBytesTransferred));

        // Validate last progress (if available)
        if (stats.LastProgress is not null)
        {
            AddIfNegative(errors, stats.LastProgress.ProcessedCount, nameof(stats.LastProgress.ProcessedCount));
            AddIfNegative(errors, stats.LastProgress.TotalCount, nameof(stats.LastProgress.TotalCount));
            AddIfNegative(errors, stats.LastProgress.SucceededCount, nameof(stats.LastProgress.SucceededCount));
            AddIfNegative(errors, stats.LastProgress.FailedCount, nameof(stats.LastProgress.FailedCount));
            AddIfNegative(errors, stats.LastProgress.BytesTransferred, nameof(stats.LastProgress.BytesTransferred));

            if (stats.LastProgress.StartedAt == default)
            {
                errors.Add("Statistics.LastProgress.StartedAt cannot be default(DateTime).");
            }

            AddIfNegative(errors, stats.LastProgress.CurrentBatch, nameof(stats.LastProgress.CurrentBatch));
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified bulk import/export engine is valid.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to check.</param>
    /// <returns><see langword="true"/> if the engine is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid<T>(this BulkImportExportEngine<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified bulk import/export engine is valid, throwing an
    /// <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the engine is invalid, containing a list of problems.</exception>
    public static void EnsureValid<T>(this BulkImportExportEngine<T> value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

            ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"The bulk import/export engine is invalid. Problems: {string.Join(" ", errors)}",
            nameof(value));
    }
}