#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

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

        // Validate that repositories are initialized (Setup has been called)
        if (value.GetType().GetField("_productRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Product repository is not initialized. Setup() method has not been called.");
        }

        if (value.GetType().GetField("_userRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("User repository is not initialized. Setup() method has not been called.");
        }

        // Validate that sample data is initialized
        if (value.GetType().GetField("_sampleProduct", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Sample product data is not initialized. Setup() method has not been called.");
        }

        if (value.GetType().GetField("_sampleUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Sample user data is not initialized. Setup() method has not been called.");
        }

        // Validate that database connection is initialized
        if (value.GetType().GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Database connection is not initialized. Setup() method has not been called.");
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