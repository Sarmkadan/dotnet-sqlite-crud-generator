// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace DotNet.SQLite.CrudGenerator.Attributes;

/// <summary>
/// Provides validation helpers for <see cref="GenerateGrpcAttribute"/>.
/// </summary>
public static class GenerateGrpcAttributeValidation
{
    /// <summary>
    /// Validates the specified <see cref="GenerateGrpcAttribute"/>.
    /// </summary>
    /// <param name="value">The attribute to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this GenerateGrpcAttribute value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.ServiceName is not null && string.IsNullOrWhiteSpace(value.ServiceName))
        {
            errors.Add("ServiceName cannot be empty or whitespace.");
        }

        if (value.Namespace is not null && string.IsNullOrWhiteSpace(value.Namespace))
        {
            errors.Add("Namespace cannot be empty or whitespace.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="GenerateGrpcAttribute"/> is valid.
    /// </summary>
    /// <param name="value">The attribute to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this GenerateGrpcAttribute value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.ServiceName is null || !string.IsNullOrWhiteSpace(value.ServiceName)
            && (value.Namespace is null || !string.IsNullOrWhiteSpace(value.Namespace));
    }

    /// <summary>
    /// Ensures that the specified <see cref="GenerateGrpcAttribute"/> is valid.
    /// </summary>
    /// <param name="value">The attribute to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the attribute is not valid.</exception>
    public static void EnsureValid(this GenerateGrpcAttribute value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"GenerateGrpcAttribute is not valid. Problems: {string.Join(" ", errors)}",
            nameof(value));
    }
}
