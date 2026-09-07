#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DotNet.SQLite.CrudGenerator.Validation;

/// <summary>
/// Represents the result of a validation operation, containing a collection of validation problems.
/// </summary>
public readonly struct ValidationResult : IEnumerable<ValidationProblem>
{
    private readonly List<ValidationProblem>? _problems;

    /// <summary>
    /// Gets whether the validation succeeded (no problems found).
    /// </summary>
    public bool IsValid => _problems is null or { Count: 0 };

    /// <summary>
    /// Gets the number of validation problems.
    /// </summary>
    public int Count => _problems?.Count ?? 0;

    /// <summary>
    /// Initializes a new <see cref="ValidationResult"/> with no problems (valid result).
    /// </summary>
    public ValidationResult()
    {
        // Empty problems list means valid
    }

    /// <summary>
    /// Initializes a new <see cref="ValidationResult"/> with the specified problems.
    /// </summary>
    /// <param name="problems">The collection of validation problems.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="problems"/> is <see langword="null"/>.</exception>
    public ValidationResult(IEnumerable<ValidationProblem> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);

        if (problems.Any())
        {
            _problems = new List<ValidationProblem>(problems);
        }
    }

    /// <summary>
    /// Adds a validation problem to this result.
    /// </summary>
    /// <param name="problem">The validation problem to add.</param>
    /// <returns>A new <see cref="ValidationResult"/> with the added problem.</returns>
    public ValidationResult WithProblem(ValidationProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        var newProblems = _problems?.ToList() ?? new List<ValidationProblem>();
        newProblems.Add(problem);
        return new ValidationResult(newProblems);
    }

    /// <summary>
    /// Adds a validation problem for a specific property.
    /// </summary>
    /// <param name="property">The name of the property that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new <see cref="ValidationResult"/> with the added problem.</returns>
    public ValidationResult WithProblem(string property, string message)
        => WithProblem(ValidationProblem.ForProperty(property, message));

    /// <summary>
    /// Adds a validation problem with a general message (no specific property).
    /// </summary>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new <see cref="ValidationResult"/> with the added problem.</returns>
    public ValidationResult WithProblem(string message)
        => WithProblem(new ValidationProblem(message));

    /// <summary>
    /// Gets the enumerator for iterating over validation problems.
    /// </summary>
    /// <returns>An enumerator over the validation problems.</returns>
    public Enumerator GetEnumerator() => new Enumerator(_problems);

    /// <summary>
    /// Gets the enumerator for iterating over validation problems.
    /// </summary>
    /// <returns>An enumerator over the validation problems.</returns>
    IEnumerator<ValidationProblem> IEnumerable<ValidationProblem>.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the enumerator for iterating over validation problems.
    /// </summary>
    /// <returns>An enumerator over the validation problems.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Converts the validation result to a list of validation problems.
    /// </summary>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    public IReadOnlyList<ValidationProblem> ToList()
        => _problems ?? (IReadOnlyList<ValidationProblem>)Array.Empty<ValidationProblem>();

    /// <summary>
    /// Creates a new validation result from a collection of error messages.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <returns>A new <see cref="ValidationResult"/> containing the errors.</returns>
    public static ValidationResult FromErrors(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var problems = errors.Select(e => new ValidationProblem(e)).ToList();
        return new ValidationResult(problems);
    }

    /// <summary>
    /// Creates a new validation result from a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A new <see cref="ValidationResult"/> containing the error.</returns>
    public static ValidationResult FromError(string error)
        => FromErrors(new[] { error });

    /// <summary>
    /// Throws an exception if this validation result contains any problems.
    /// </summary>
    /// <param name="exceptionCreator">A function that creates an exception from the problems.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exceptionCreator"/> is <see langword="null"/>.</exception>
    /// <exception cref="TException">The exception created by <paramref name="exceptionCreator"/>.</exception>
    public void ThrowIfInvalid<TException>(Func<IReadOnlyList<ValidationProblem>, TException> exceptionCreator)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exceptionCreator);

        if (!IsValid)
        {
            throw exceptionCreator(ToList());
        }
    }

    /// <summary>
    /// Represents an enumerator for <see cref="ValidationResult"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<ValidationProblem>
    {
        private readonly List<ValidationProblem>? _list;
        private int _index;

        internal Enumerator(List<ValidationProblem>? list)
        {
            _list = list;
            _index = -1;
        }

        /// <summary>
        /// Gets the current validation problem.
        /// </summary>
        public readonly ValidationProblem Current => _list is not null && _index >= 0 && _index < _list.Count
            ? _list[_index]
            : throw new InvalidOperationException("Enumeration has not started or has ended.");

        readonly object? IEnumerator.Current => Current;

        /// <summary>
        /// Advances to the next validation problem.
        /// </summary>
        /// <returns><see langword="true"/> if another problem was found; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext()
        {
            if (_list is null)
            {
                return false;
            }

            _index++;
            return _index < _list.Count;
        }

        /// <summary>
        /// Resets the enumerator.
        /// </summary>
        void IEnumerator.Reset() => _index = -1;

        /// <summary>
        /// Disposes the enumerator.
        /// </summary>
        public readonly void Dispose() { }
    }
}