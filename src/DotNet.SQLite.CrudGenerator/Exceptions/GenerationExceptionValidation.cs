#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="GenerationException"/> instances using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class GenerationExceptionValidation
{
    /// <summary>
    /// Validates the specified <see cref="GenerationException"/> instance.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate(this GenerationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new ValidationResult();

        if (string.IsNullOrEmpty(value.GenerationType))
        {
            errors = errors.WithProblem(nameof(value.GenerationType), "GenerationType must be specified.");
        }

        if (string.IsNullOrEmpty(value.SourceEntity))
        {
            errors = errors.WithProblem(nameof(value.SourceEntity), "SourceEntity must be specified.");
        }

        if (value.LineNumber is <= 0)
        {
            errors = errors.WithProblem(nameof(value.LineNumber), "LineNumber must be a positive integer if specified.");
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenerationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns><see langword="true"/> if the exception is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this GenerationException? value) => value is not null && value.Validate().IsValid;

    /// <summary>
    /// Ensures that the specified <see cref="GenerationException"/> is valid.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the exception is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this GenerationException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"GenerationException is invalid. Validation errors: {string.Join("; ", problems.Select(p => p.Message))}",
            nameof(value)));
    }
}