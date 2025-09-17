#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Reflection;
using DotNet.SQLite.CrudGenerator.Services;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Provides extension methods for <see cref="QueryBuilderGenerationServiceTests"/> to facilitate testing
/// and access internal members for verification purposes.
/// </summary>
public static class QueryBuilderGenerationServiceTestsExtensions
{
    private const BindingFlags NonPublicInstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Creates a new instance of <see cref="QueryBuilderGenerationServiceTests"/> with a temporary output directory.
    /// Useful for testing scenarios where you need a fresh instance without shared state.
    /// </summary>
    /// <param name="test">The test instance. Cannot be null.</param>
    /// <returns>A new <see cref="QueryBuilderGenerationServiceTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static QueryBuilderGenerationServiceTests CreateFresh(this QueryBuilderGenerationServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        return new QueryBuilderGenerationServiceTests();
    }

    /// <summary>
    /// Gets the output path used by this test instance.
    /// Useful for cleanup or path validation scenarios.
    /// </summary>
    /// <param name="test">The test instance. Cannot be null.</param>
    /// <returns>The output directory path. Returns empty string if the field cannot be accessed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static string GetOutputPath(this QueryBuilderGenerationServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var outputPathField = typeof(QueryBuilderGenerationServiceTests).GetField("_outputPath", NonPublicInstanceFlags);
        return outputPathField?.GetValue(test) as string ?? string.Empty;
    }

    /// <summary>
    /// Verifies that the output directory was cleaned up by the Dispose method.
    /// Useful for testing cleanup behavior.
    /// </summary>
    /// <param name="test">The test instance. Cannot be null.</param>
    /// <returns>True if directory was cleaned up, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    public static bool OutputDirectoryWasCleanedUp(this QueryBuilderGenerationServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);
        var outputPath = test.GetOutputPath();
        return !Directory.Exists(outputPath);
    }

    /// <summary>
    /// Gets the QueryBuilderGenerationService instance used by this test.
    /// Useful for direct service testing without going through test methods.
    /// </summary>
    /// <param name="test">The test instance. Cannot be null.</param>
    /// <returns>The QueryBuilderGenerationService instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the service field cannot be found or accessed.</exception>
    public static QueryBuilderGenerationService GetService(this QueryBuilderGenerationServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var sutField = typeof(QueryBuilderGenerationServiceTests).GetField("_sut", NonPublicInstanceFlags);
        return sutField?.GetValue(test) as QueryBuilderGenerationService
            ?? throw new InvalidOperationException("Service field '_sut' not found or could not be accessed");
    }
}