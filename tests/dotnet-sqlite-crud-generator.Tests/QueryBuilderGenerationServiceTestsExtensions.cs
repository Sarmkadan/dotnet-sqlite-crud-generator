#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public static class QueryBuilderGenerationServiceTestsExtensions
{
    /// <summary>
    /// Creates a new instance of QueryBuilderGenerationServiceTests with a temporary output directory.
    /// Useful for testing scenarios where you need a fresh instance without shared state.
    /// </summary>
    /// <param name="test">The test instance (unused, for extension method syntax).</param>
    /// <returns>A new QueryBuilderGenerationServiceTests instance.</returns>
    public static QueryBuilderGenerationServiceTests CreateFresh(this QueryBuilderGenerationServiceTests test)
    {
        return new QueryBuilderGenerationServiceTests();
    }

    /// <summary>
    /// Gets the output path used by this test instance.
    /// Useful for cleanup or path validation scenarios.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>The output directory path.</returns>
    public static string GetOutputPath(this QueryBuilderGenerationServiceTests test)
    {
        var outputPathField = typeof(QueryBuilderGenerationServiceTests).GetField("_outputPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return outputPathField?.GetValue(test) as string ?? string.Empty;
    }

    /// <summary>
    /// Verifies that the output directory was cleaned up by the Dispose method.
    /// Useful for testing cleanup behavior.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>True if directory was cleaned up, false otherwise.</returns>
    public static bool OutputDirectoryWasCleanedUp(this QueryBuilderGenerationServiceTests test)
    {
        var outputPath = test.GetOutputPath();
        return !Directory.Exists(outputPath);
    }

    /// <summary>
    /// Gets the QueryBuilderGenerationService instance used by this test.
    /// Useful for direct service testing without going through test methods.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>The QueryBuilderGenerationService instance.</returns>
    public static QueryBuilderGenerationService GetService(this QueryBuilderGenerationServiceTests test)
    {
        var sutField = typeof(QueryBuilderGenerationServiceTests).GetField("_sut", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return sutField?.GetValue(test) as QueryBuilderGenerationService ?? throw new InvalidOperationException("Service field not found");
    }
}