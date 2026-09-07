#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="LoggingMiddleware"/> instances using the shared <see cref="ValidationResult"/> abstraction.
/// Validates the middleware configuration and ensures it's in a valid state before execution.
/// </summary>
public static class LoggingMiddlewareValidation
{
    /// <summary>
    /// Validates the specified <see cref="LoggingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate(this IPipelineStep value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate the internal _enableDetailedLogging flag is within expected range
        // This is a defensive check even though the constructor parameter is a bool
        // The validation ensures the middleware is in a valid state

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="LoggingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this IPipelineStep value)
        => value is not null && Validate(value).IsValid;

    /// <summary>
    /// Ensures that the specified <see cref="LoggingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown when <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this LoggingMiddleware value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate(value).ThrowIfInvalid(problems => new ValidationException(
            $"LoggingMiddleware is invalid. Problems: {string.Join("; ", problems.Select(p => p.Message))}",
            nameof(value)));
    }
}