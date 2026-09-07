#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.SQLite.CrudGenerator.Validation;

/// <summary>
/// Exception thrown when validation fails, containing a collection of validation problems.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Gets the collection of validation problems that caused this exception.
    /// </summary>
    public IReadOnlyList<ValidationProblem> Problems { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ValidationException(string message)
        : base(message)
    {
        Problems = Array.Empty<ValidationProblem>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Problems = Array.Empty<ValidationProblem>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="paramName">The name of the parameter that failed validation.</param>
    public ValidationException(string message, string? paramName)
        : base(message)
    {
        ParamName = paramName;
        Problems = Array.Empty<ValidationProblem>();
    }

    /// <summary>
    /// Gets the name of the parameter that failed validation, if any.
    /// </summary>
    public string? ParamName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with validation problems.
    /// </summary>
    /// <param name="problems">The validation problems that caused the exception.</param>
    public ValidationException(IReadOnlyList<ValidationProblem> problems)
        : base(FormatMessage(problems))
    {
        Problems = problems ?? throw new ArgumentNullException(nameof(problems));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a single validation problem.
    /// </summary>
    /// <param name="problem">The validation problem that caused the exception.</param>
    public ValidationException(ValidationProblem problem)
        : base(FormatMessage(new[] { problem ?? throw new ArgumentNullException(nameof(problem)) }))
    {
        Problems = new[] { problem };
    }

    private static string FormatMessage(IReadOnlyList<ValidationProblem> problems)
    {
        if (problems == null || problems.Count == 0)
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

    /// <summary>
    /// Creates a <see cref="ValidationException"/> from a validation result.
    /// </summary>
    /// <param name="result">The validation result containing problems.</param>
    /// <returns>A new <see cref="ValidationException"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ValidationException FromResult(ValidationResult result)
    {
        return new ValidationException(result.ToList());
    }

    /// <summary>
    /// Creates a <see cref="ValidationException"/> from a collection of validation problems.
    /// </summary>
    /// <param name="problems">The collection of validation problems.</param>
    /// <returns>A new <see cref="ValidationException"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="problems"/> is null.</exception>
    public static ValidationException FromProblems(IEnumerable<ValidationProblem> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        var problemList = problems.ToList();
        return new ValidationException(problemList);
    }
}
