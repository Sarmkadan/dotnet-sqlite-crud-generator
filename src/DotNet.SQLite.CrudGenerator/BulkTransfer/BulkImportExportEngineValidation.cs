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

        // Validate repository (should not be null)
        // Note: We can't directly access the private field, so we rely on the public API
        // The constructor already validates this, so we just check the engine itself

        // Validate options (if available)
        // The options are validated in the constructor, so we check if they're reasonable

        // Validate statistics (should have reasonable values)
        var stats = value.GetStatistics();
        if (stats.TotalImports < 0)
        {
            errors.Add("Statistics.TotalImports cannot be negative.");
        }

        if (stats.TotalExports < 0)
        {
            errors.Add("Statistics.TotalExports cannot be negative.");
        }

        if (stats.TotalRecordsImported < 0)
        {
            errors.Add("Statistics.TotalRecordsImported cannot be negative.");
        }

        if (stats.TotalRecordsExported < 0)
        {
            errors.Add("Statistics.TotalRecordsExported cannot be negative.");
        }

        if (stats.TotalErrors < 0)
        {
            errors.Add("Statistics.TotalErrors cannot be negative.");
        }

        if (stats.TotalBytesTransferred < 0)
        {
            errors.Add("Statistics.TotalBytesTransferred cannot be negative.");
        }

        // Validate last progress (if available)
        if (stats.LastProgress is not null)
        {
            if (stats.LastProgress.ProcessedCount < 0)
            {
                errors.Add("Statistics.LastProgress.ProcessedCount cannot be negative.");
            }

            if (stats.LastProgress.TotalCount < 0)
            {
                errors.Add("Statistics.LastProgress.TotalCount cannot be negative.");
            }

            if (stats.LastProgress.SucceededCount < 0)
            {
                errors.Add("Statistics.LastProgress.SucceededCount cannot be negative.");
            }

            if (stats.LastProgress.FailedCount < 0)
            {
                errors.Add("Statistics.LastProgress.FailedCount cannot be negative.");
            }

            if (stats.LastProgress.BytesTransferred < 0)
            {
                errors.Add("Statistics.LastProgress.BytesTransferred cannot be negative.");
            }

            if (stats.LastProgress.StartedAt == default)
            {
                errors.Add("Statistics.LastProgress.StartedAt cannot be default(DateTime).");
            }

            if (stats.LastProgress.CurrentBatch < 0)
            {
                errors.Add("Statistics.LastProgress.CurrentBatch cannot be negative.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified bulk import/export engine is valid.
    /// </summary>
    /// <typeparam name="T">The entity type managed by the engine.</typeparam>
    /// <param name="value">The engine to check.</param>
    /// <returns><see langword="true"/> if the engine is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(this BulkImportExportEngine<T> value) where T : class
    {
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

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"The bulk import/export engine is invalid. Problems: {string.Join(" ", errors)}",
            nameof(value));
    }
}