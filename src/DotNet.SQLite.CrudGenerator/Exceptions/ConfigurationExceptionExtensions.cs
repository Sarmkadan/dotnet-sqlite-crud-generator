#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ConfigurationException"/> to enhance error handling and reporting.
/// </summary>
public static class ConfigurationExceptionExtensions
{
    /// <summary>
    /// Adds additional context information to the exception.
    /// </summary>
    /// <param name="exception">The configuration exception to enhance.</param>
    /// <param name="contextKey">The key identifying the context information.</param>
    /// <param name="contextValue">The context value to add.</param>
    /// <returns>A new ConfigurationException with the additional context.</returns>
    public static ConfigurationException WithContext(this ConfigurationException exception, string contextKey, string contextValue)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(contextKey))
        {
            throw new ArgumentException("Context key cannot be null or whitespace.", nameof(contextKey));
        }

        var newMessage = $"{exception.Message} | Context: {contextKey}={contextValue}";
        return new ConfigurationException(newMessage, exception);
    }

    /// <summary>
    /// Adds multiple context information entries to the exception.
    /// </summary>
    /// <param name="exception">The configuration exception to enhance.</param>
    /// <param name="context">A dictionary of context key-value pairs.</param>
    /// <returns>A new ConfigurationException with the additional context.</returns>
    public static ConfigurationException WithContext(this ConfigurationException exception, IReadOnlyDictionary<string, string> context)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (context is null || context.Count == 0)
        {
            return exception;
        }

        var contextParts = new List<string>();
        foreach (var kvp in context)
        {
            contextParts.Add($"{kvp.Key}={kvp.Value}");
        }

        var newMessage = $"{exception.Message} | Context: {string.Join(", ", contextParts)}";
        return new ConfigurationException(newMessage, exception);
    }

    /// <summary>
    /// Creates a new ConfigurationException that wraps this exception with a custom message.
    /// </summary>
    /// <param name="exception">The configuration exception to wrap.</param>
    /// <param name="customMessage">The custom message to use for the new exception.</param>
    /// <returns>A new ConfigurationException with the custom message and the original exception as inner exception.</returns>
    public static ConfigurationException WithMessage(this ConfigurationException exception, string customMessage)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (string.IsNullOrWhiteSpace(customMessage))
        {
            throw new ArgumentException("Custom message cannot be null or whitespace.", nameof(customMessage));
        }

        return new ConfigurationException(customMessage, exception);
    }

    /// <summary>
    /// Determines if this exception represents a missing configuration error.
    /// </summary>
    /// <param name="exception">The configuration exception to check.</param>
    /// <returns>True if the exception is a missing configuration error; otherwise false.</returns>
    public static bool IsMissingConfiguration(this ConfigurationException exception)
    {
        return exception?.Message.Contains("is missing or invalid", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}