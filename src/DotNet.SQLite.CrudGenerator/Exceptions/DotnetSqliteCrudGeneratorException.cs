#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions in the DotNet.SQLite.CrudGenerator library.
/// </summary>
public abstract class DotnetSqliteCrudGeneratorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotnetSqliteCrudGeneratorException"/> class.
    /// </summary>
    protected DotnetSqliteCrudGeneratorException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DotnetSqliteCrudGeneratorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    protected DotnetSqliteCrudGeneratorException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Gets the type of the exception for categorization purposes.
    /// </summary>
    public virtual string ExceptionType => GetType().Name.Replace("Exception", "");
}