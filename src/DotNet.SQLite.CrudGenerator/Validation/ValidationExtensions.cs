#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Exceptions;

namespace DotNet.SQLite.CrudGenerator.Validation;

/// <summary>
/// Provides extension methods for validation operations using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates the specified value and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">A function that performs the validation and returns a <see cref="ValidationResult"/>.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate<T>(this T? value, Func<T?, ValidationResult> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return validator(value);
    }

    /// <summary>
    /// Determines whether the specified value is valid.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">A function that performs the validation and returns a <see cref="ValidationResult"/>.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(this T? value, Func<T?, ValidationResult> validator)
        => value.Validate(validator).IsValid;

    /// <summary>
    /// Ensures that the specified value is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <typeparam name="TException">The type of exception to throw when validation fails.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">A function that performs the validation and returns a <see cref="ValidationResult"/>.</param>
    /// <param name="exceptionCreator">A function that creates an exception from validation problems.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> or <paramref name="exceptionCreator"/> is <see langword="null"/>.</exception>
    /// <exception cref="TException">The exception created by <paramref name="exceptionCreator"/>.</exception>
    public static T EnsureValid<T, TException>(
        this T? value,
        Func<T?, ValidationResult> validator,
        Func<IReadOnlyList<ValidationProblem>, TException> exceptionCreator)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(exceptionCreator);

        var result = value.Validate(validator);
        result.ThrowIfInvalid(exceptionCreator);
        return value!;
    }

    /// <summary>
    /// Ensures that the specified value is valid, throwing a <see cref="ValidationException"/> if not.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">A function that performs the validation and returns a <see cref="ValidationResult"/>.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <see langword="null"/>.</exception>
    public static T EnsureValid<T>(
        this T? value,
        Func<T?, ValidationResult> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var result = value.Validate(validator);
        result.ThrowIfInvalid(problems => new ValidationException(FormatValidationExceptionMessage(problems)));
        return value!;
    }

    /// <summary>
    /// Ensures that the specified value is valid, throwing a <see cref="ValidationException"/> if not.
    /// </summary>
    /// <typeparam name="T">The type of value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">A function that performs the validation and returns a <see cref="ValidationResult"/>.</param>
    /// <param name="context">Optional context to include in the exception message.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public static T EnsureValid<T>(this T? value, Func<T?, ValidationResult> validator, string? context = null)
    {
        ArgumentNullException.ThrowIfNull(validator);

        var result = value.Validate(validator);
        if (!result.IsValid)
        {
            if (string.IsNullOrEmpty(context))
            {
                result.ThrowIfInvalid(problems => new ValidationException(problems));
            }
            else
            {
                result.ThrowIfInvalid(problems => new ValidationException($"{context} validation failed:\n- {string.Join("\n- ", problems.Select(p => p.Message))}"));
            }
        }

        return value!;
    }

    private static string FormatValidationExceptionMessage(IReadOnlyList<ValidationProblem> problems)
    {
        if (problems.Count == 0)
        {
            return "Validation failed.";
        }

        if (problems.Count == 1)
        {
            return problems[0].Message;
        }

        var message = "Validation failed:\n";
        for (int i = 0; i < problems.Count; i++)
        {
            message += $"- {problems[i].Message}\n";
        }
        return message.TrimEnd('\n');
    }

    private static string FormatValidationMessage(IReadOnlyList<ValidationProblem> problems)
    {
        if (problems.Count == 0)
        {
            return "Validation failed.";
        }

        var message = string.Join(" ", problems.Select(p => p.Message));
        return message;
    }
}