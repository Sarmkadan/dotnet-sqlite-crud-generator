#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Validation;

/// <summary>
/// Represents a single validation problem with a message and optional property name.
/// </summary>
/// <param name="Message">The validation error message.</param>
/// <param name="Property">The name of the property or field that failed validation, if applicable.</param>
public sealed record ValidationProblem(string Message, string? Property = null)
{
    /// <summary>
    /// Creates a new validation problem for a specific property.
    /// </summary>
    /// <param name="property">The name of the property that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new <see cref="ValidationProblem"/> instance.</returns>
    public static ValidationProblem ForProperty(string property, string message)
        => new ValidationProblem(message, property);
}