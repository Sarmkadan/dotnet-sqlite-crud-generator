#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Provides validation helpers for configuration values that would be used to create <see cref="ConfigurationException"/> instances.
/// </summary>
public static class ConfigurationExceptionValidation
{
    /// <summary>
    /// Validates a configuration name.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(string? configName)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(configName))
        {
            problems.Add("Configuration name cannot be null, empty, or whitespace.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified configuration name is valid.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName) => Validate(configName).Count == 0;

    /// <summary>
    /// Ensures that the specified configuration name is valid.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration name is invalid.</exception>
    public static void EnsureValid(string? configName)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = Validate(configName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(configName));
        }
    }

    /// <summary>
    /// Validates a configuration value string.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(string? configName, string? value)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(configName))
        {
            problems.Add("Configuration name cannot be null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add("Configuration value cannot be null, empty, or whitespace.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified configuration value is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName, string? value) => Validate(configName, value).Count == 0;

    /// <summary>
    /// Ensures that the specified configuration value is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration value is invalid.</exception>
    public static void EnsureValid(string? configName, string? value)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = Validate(configName, value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(value));
        }
    }

    /// <summary>
    /// Validates a connection string name.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateConnectionString(string? connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            problems.Add("Connection string name cannot be null, empty, or whitespace.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified connection string name is valid.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static bool IsValidConnectionString(string? connectionStringName) => ValidateConnectionString(connectionStringName).Count == 0;

    /// <summary>
    /// Ensures that the specified connection string name is valid.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the connection string name is invalid.</exception>
    public static void EnsureValidConnectionString(string? connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var problems = ValidateConnectionString(connectionStringName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(connectionStringName));
        }
    }

    /// <summary>
    /// Validates a file path.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateFilePath(string? filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            problems.Add("File path cannot be null, empty, or whitespace.");
        }
        else if (!System.IO.Path.IsPathRooted(filePath))
        {
            problems.Add("File path must be an absolute path.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    public static bool IsValidFilePath(string? filePath) => ValidateFilePath(filePath).Count == 0;

    /// <summary>
    /// Ensures that the specified file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid.</exception>
    public static void EnsureValidFilePath(string? filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var problems = ValidateFilePath(filePath);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(filePath));
        }
    }

    /// <summary>
    /// Validates a timeout value.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateTimeout(int timeoutValue)
    {
        var problems = new List<string>();

        if (timeoutValue <= 0)
        {
            problems.Add("Timeout value must be greater than 0.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified timeout value is valid.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidTimeout(int timeoutValue) => ValidateTimeout(timeoutValue).Count == 0;

    /// <summary>
    /// Ensures that the specified timeout value is valid.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the timeout value is invalid.</exception>
    public static void EnsureValidTimeout(int timeoutValue)
    {
        var problems = ValidateTimeout(timeoutValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(timeoutValue));
        }
    }

    /// <summary>
    /// Validates a timeout configuration.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(string? configName, int timeoutValue)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(configName))
        {
            problems.Add("Configuration name cannot be null, empty, or whitespace.");
        }

        if (timeoutValue <= 0)
        {
            problems.Add("Timeout value must be greater than 0.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified timeout configuration is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName, int timeoutValue) => Validate(configName, timeoutValue).Count == 0;

    /// <summary>
    /// Ensures that the specified timeout configuration is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the timeout configuration is invalid.</exception>
    public static void EnsureValid(string? configName, int timeoutValue)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var problems = Validate(configName, timeoutValue);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems), nameof(timeoutValue));
        }
    }
}