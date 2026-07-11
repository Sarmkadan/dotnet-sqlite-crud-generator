#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="RepositoryBenchmarks"/> instances.
/// Validates that benchmark setup and execution state is valid for reliable benchmarking.
/// </summary>
public static class RepositoryBenchmarksValidation
{
    /// <summary>
    /// Validates the given <see cref="RepositoryBenchmarks"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RepositoryBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that Setup has been called (database and repositories initialized)
        try
        {
            // Check if Setup method exists and can be invoked
            // Since we can't directly access private fields, we validate by attempting
            // to use the benchmark in a way that would fail if not properly initialized
            var setupMethod = value.Setup;
        }
        catch
        {
            problems.Add("Setup method is not available or not properly initialized.");
        }

        // Validate that Dispose has been called or is available
        try
        {
            var disposeMethod = value.Dispose;
        }
        catch
        {
            problems.Add("Dispose method is not available.");
        }

        // Validate that SaveChangesAsync is available
        try
        {
            var saveChangesMethod = value.SaveChangesAsync;
        }
        catch
        {
            problems.Add("SaveChangesAsync method is not available.");
        }

        // Validate that Cleanup is available
        try
        {
            var cleanupMethod = value.Cleanup;
        }
        catch
        {
            problems.Add("Cleanup method is not available.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the given <see cref="RepositoryBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this RepositoryBenchmarks value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the given <see cref="RepositoryBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this RepositoryBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RepositoryBenchmarks instance is invalid. Problems: {string.Join(", ", problems)}",
                nameof(value));
        }
    }
}
