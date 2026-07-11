#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Provides validation helpers for <see cref="WebhookHandler"/> instances.
/// </summary>
public static class WebhookHandlerValidation
{
    /// <summary>
    /// Validates a <see cref="WebhookHandler"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // WebhookHandler has no public properties to validate
        // All validation is handled by the internal state management of the class

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="WebhookHandler"/> is valid.
    /// </summary>
    /// <param name="value">The webhook handler to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this WebhookHandler value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="WebhookHandler"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookHandler is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}