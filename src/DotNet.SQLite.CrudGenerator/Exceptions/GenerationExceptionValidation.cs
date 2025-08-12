#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="GenerationException"/> instances.
/// </summary>
public static class GenerationExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="GenerationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation errors; empty if the exception is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this GenerationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.GenerationType is { Length: 0 })
        {
            errors.Add("GenerationType cannot be an empty string.");
        }

        if (value.SourceEntity is { Length: 0 })
        {
            errors.Add("SourceEntity cannot be an empty string.");
        }

        if (value.LineNumber is <= 0)
        {
            errors.Add("LineNumber must be a positive integer if specified.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenerationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this GenerationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="GenerationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this GenerationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GenerationException is invalid. Validation errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}