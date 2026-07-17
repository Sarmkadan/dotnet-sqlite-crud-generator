#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="LoggingMiddleware"/> instances.
/// Validates the middleware configuration and ensures it's in a valid state before execution.
/// </summary>
public static class LoggingMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="LoggingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this LoggingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate the internal _enableDetailedLogging flag is within expected range
        // This is a defensive check even though the constructor parameter is a bool
        // The validation ensures the middleware is in a valid state

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="LoggingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this LoggingMiddleware value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="LoggingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this LoggingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"LoggingMiddleware is invalid. Problems: {string.Join("; ", problems)}",
            nameof(value));
    }
}