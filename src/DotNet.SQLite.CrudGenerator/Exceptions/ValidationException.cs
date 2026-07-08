#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Exception thrown when entity validation fails.
/// </summary>
public sealed class ValidationException : DotnetSqliteCrudGeneratorException
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }

    public List<ValidationError> Errors { get; } = new();

    /// <summary>
    /// Adds a validation error to the exception.
    /// </summary>
    public void AddError(string property, string message)
    {
        Errors.Add(new ValidationError { Property = property, Message = message });
    }

    /// <summary>
    /// Creates a validation exception from multiple errors.
    /// </summary>
    public static ValidationException FromErrors(List<ValidationError> errors)
    {
        var messages = string.Join("; ", errors.Select(e => $"{e.Property}: {e.Message}"));
        var exception = new ValidationException($"Validation failed: {messages}");
        exception.Errors.AddRange(errors);
        return exception;
    }

    /// <summary>
    /// Represents a single validation error.
    /// </summary>
    public sealed class ValidationError
    {
        public string? Property { get; set; }
        public string? Message { get; set; }
    }
}
