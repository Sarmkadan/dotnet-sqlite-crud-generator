#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Attributes;

/// <summary>
/// Provides validation helpers for <see cref="GenerateGrpcAttribute"/> using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class GenerateGrpcAttributeValidation
{
    /// <summary>
    /// Validates the specified <see cref="GenerateGrpcAttribute"/>.
    /// </summary>
    /// <param name="value">The attribute to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate(this GenerateGrpcAttribute value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        if (value.ServiceName is not null && string.IsNullOrWhiteSpace(value.ServiceName))
        {
            result = result.WithProblem(nameof(value.ServiceName), "ServiceName cannot be empty or whitespace.");
        }

        if (value.Namespace is not null && string.IsNullOrWhiteSpace(value.Namespace))
        {
            result = result.WithProblem(nameof(value.Namespace), "Namespace cannot be empty or whitespace.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenerateGrpcAttribute"/> is valid.
    /// </summary>
    /// <param name="value">The attribute to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this GenerateGrpcAttribute value)
        => value is not null && Validate(value).IsValid;

    /// <summary>
    /// Ensures that the specified <see cref="GenerateGrpcAttribute"/> is valid.
    /// </summary>
    /// <param name="value">The attribute to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown if the attribute is not valid.</exception>
    public static void EnsureValid(this GenerateGrpcAttribute value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate(value).ThrowIfInvalid(problems => new ValidationException(
            $"GenerateGrpcAttribute is not valid. Problems: {string.Join(" ", problems.Select(p => p.Message))}",
            nameof(value)));
    }
}