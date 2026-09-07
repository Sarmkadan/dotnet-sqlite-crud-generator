#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Helper methods for file path validation.
/// </summary>
internal static class PathValidationHelper
{
    /// <summary>
    /// Checks if a filename is a Windows reserved device name.
    /// </summary>
    /// <param name="fileName">The filename to check.</param>
    /// <returns><see langword="true"/> if the filename is a reserved device name; otherwise, <see langword="false"/>.</returns>
    internal static bool IsReservedDeviceName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        // List of Windows reserved device names (case-insensitive)
        var reservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        return reservedNames.Contains(fileName);
    }
}

/// <summary>
/// Provides validation helpers for configuration values that would be used to create <see cref="ConfigurationException"/> instances.
/// </summary>
public static class ConfigurationExceptionValidation
{
    /// <summary>
    /// Validates a configuration name.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate(string? configName)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(configName))
        {
            result = result.WithProblem(nameof(configName), "Configuration name cannot be null, empty, or whitespace.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified configuration name is valid.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName) => Validate(configName).IsValid;

    /// <summary>
    /// Ensures that the specified configuration name is valid.
    /// </summary>
    /// <param name="configName">The configuration name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the configuration name is invalid.</exception>
    public static void EnsureValid(string? configName)
    {
        Validate(configName).ThrowIfInvalid(problems => new ValidationException(problems));
    }

    /// <summary>
    /// Validates a configuration value string.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate(string? configName, string? value)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var result = Validate(configName);

        if (string.IsNullOrWhiteSpace(value))
        {
            result = result.WithProblem(nameof(value), "Configuration value cannot be null, empty, or whitespace.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified configuration value is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName, string? value) => Validate(configName, value).IsValid;

    /// <summary>
    /// Ensures that the specified configuration value is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="value">The configuration value to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the configuration value is invalid.</exception>
    public static void EnsureValid(string? configName, string? value)
    {
        Validate(configName, value).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(value)));
    }

    /// <summary>
    /// Validates a connection string name.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static ValidationResult ValidateConnectionString(string? connectionStringName)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            result = result.WithProblem(nameof(connectionStringName), "Connection string name cannot be null, empty, or whitespace.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified connection string name is valid.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static bool IsValidConnectionString(string? connectionStringName) => ValidateConnectionString(connectionStringName).IsValid;

    /// <summary>
    /// Ensures that the specified connection string name is valid.
    /// </summary>
    /// <param name="connectionStringName">The connection string name to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the connection string name is invalid.</exception>
    public static void EnsureValidConnectionString(string? connectionStringName)
    {
        ValidateConnectionString(connectionStringName).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(connectionStringName)));
    }

    /// <summary>
    /// Validates a file path.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    public static ValidationResult ValidateFilePath(string? filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            result = result.WithProblem(nameof(filePath), "File path cannot be null, empty, or whitespace.");
        }
        else if (!System.IO.Path.IsPathRooted(filePath))
        {
            result = result.WithProblem(nameof(filePath), "File path must be an absolute path.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    public static bool IsValidFilePath(string? filePath) => ValidateFilePath(filePath).IsValid;

    /// <summary>
    /// Ensures that the specified file path is valid.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the file path is invalid.</exception>
    public static void EnsureValidFilePath(string? filePath)
    {
        ValidateFilePath(filePath).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(filePath)));
    }

    /// <summary>
    /// Validates a timeout value.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    public static ValidationResult ValidateTimeout(int timeoutValue)
    {
        var result = new ValidationResult();

        if (timeoutValue <= 0)
        {
            result = result.WithProblem(nameof(timeoutValue), "Timeout value must be greater than 0.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified timeout value is valid.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValidTimeout(int timeoutValue) => ValidateTimeout(timeoutValue).IsValid;

    /// <summary>
    /// Ensures that the specified timeout value is valid.
    /// </summary>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <exception cref="ValidationException">Thrown when the timeout value is invalid.</exception>
    public static void EnsureValidTimeout(int timeoutValue)
    {
        ValidateTimeout(timeoutValue).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(timeoutValue)));
    }

    /// <summary>
    /// Validates a timeout configuration.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static ValidationResult Validate(string? configName, int timeoutValue)
    {
        ArgumentNullException.ThrowIfNull(configName);

        var result = Validate(configName);

        if (timeoutValue <= 0)
        {
            result = result.WithProblem(nameof(timeoutValue), "Timeout value must be greater than 0.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified timeout configuration is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    public static bool IsValid(string? configName, int timeoutValue) => Validate(configName, timeoutValue).IsValid;

    /// <summary>
    /// Ensures that the specified timeout configuration is valid.
    /// </summary>
    /// <param name="configName">The name of the configuration.</param>
    /// <param name="timeoutValue">The timeout value in milliseconds to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the timeout configuration is invalid.</exception>
    public static void EnsureValid(string? configName, int timeoutValue)
    {
        Validate(configName, timeoutValue).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(timeoutValue)));
    }

    /// <summary>
    /// Validates a connection string value.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration.</param>
    /// <param name="connectionStringValue">The connection string value to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static ValidationResult ValidateConnectionString(string? connectionStringName, string? connectionStringValue)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var result = ValidateConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            result = result.WithProblem(nameof(connectionStringValue), "Connection string value cannot be null, empty, or whitespace.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified connection string value is valid.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration.</param>
    /// <param name="connectionStringValue">The connection string value to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static bool IsValidConnectionString(string? connectionStringName, string? connectionStringValue)
        => ValidateConnectionString(connectionStringName, connectionStringValue).IsValid;

    /// <summary>
    /// Ensures that the specified connection string value is valid.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration.</param>
    /// <param name="connectionStringValue">The connection string value to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the connection string value is invalid.</exception>
    public static void EnsureValidConnectionString(string? connectionStringName, string? connectionStringValue)
    {
        ValidateConnectionString(connectionStringName, connectionStringValue).ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(connectionStringValue)));
    }

    /// <summary>
    /// Validates a connection string value and returns a sanitized version for error reporting.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration.</param>
    /// <param name="connectionStringValue">The connection string value to validate.</param>
    /// <param name="sanitizedValue">Outputs a sanitized version of the connection string with secrets redacted.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    public static ValidationResult ValidateConnectionString(string? connectionStringName, string? connectionStringValue, out string? sanitizedValue)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var result = ValidateConnectionString(connectionStringName, connectionStringValue);
        sanitizedValue = connectionStringValue;

        if (string.IsNullOrWhiteSpace(connectionStringValue))
        {
            sanitizedValue = null;
        }
        else
        {
            sanitizedValue = SanitizeConnectionString(connectionStringValue);

            if (!IsConnectionStringSafe(connectionStringValue))
            {
                result = result.WithProblem(nameof(connectionStringValue), "Connection string contains invalid characters or format.");
            }
        }

        return result;
    }

    /// <summary>
    /// Sanitizes a connection string by redacting credential-bearing segments.
    /// </summary>
    /// <param name="connectionString">The connection string to sanitize.</param>
    /// <returns>A sanitized connection string with secrets redacted.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is <see langword="null"/>.</exception>
    public static string SanitizeConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var sanitized = new StringBuilder(connectionString.Length);
        int startIndex = 0;

        while (startIndex < connectionString.Length)
        {
            // Find the next credential-bearing segment
            int passwordIndex = connectionString.IndexOf("Password=", startIndex, StringComparison.OrdinalIgnoreCase);
            int pwdIndex = connectionString.IndexOf("Pwd=", startIndex, StringComparison.OrdinalIgnoreCase);
            int keyIndex = connectionString.IndexOf("Key=", startIndex, StringComparison.OrdinalIgnoreCase);

            // Determine which credential segment comes first
            int nextCredentialIndex = -1;
            string credentialKey = string.Empty;

            if (passwordIndex >= 0 && (nextCredentialIndex == -1 || passwordIndex < nextCredentialIndex))
            {
                nextCredentialIndex = passwordIndex;
                credentialKey = "Password=";
            }

            if (pwdIndex >= 0 && (nextCredentialIndex == -1 || pwdIndex < nextCredentialIndex))
            {
                nextCredentialIndex = pwdIndex;
                credentialKey = "Pwd=";
            }

            if (keyIndex >= 0 && (nextCredentialIndex == -1 || keyIndex < nextCredentialIndex))
            {
                nextCredentialIndex = keyIndex;
                credentialKey = "Key=";
            }

            if (nextCredentialIndex == -1)
            {
                // No more credential segments found, append remaining text
                sanitized.Append(connectionString, startIndex, connectionString.Length - startIndex);
                break;
            }

            // Append text before the credential segment
            sanitized.Append(connectionString, startIndex, nextCredentialIndex - startIndex);

            // Find the end of the credential value (next semicolon or end of string)
            int valueStart = nextCredentialIndex + credentialKey.Length;
            int valueEnd = connectionString.IndexOf(';', valueStart);
            if (valueEnd == -1)
            {
                valueEnd = connectionString.Length;
            }

            // Append the credential key and redact the value
            sanitized.Append(credentialKey);
            sanitized.Append("***REDACTED***");

            // If there's a semicolon after the credential, append it
            if (valueEnd < connectionString.Length)
            {
                sanitized.Append(';');
            }

            startIndex = valueEnd + 1;
        }

        return sanitized.ToString();
    }

    /// <summary>
    /// Determines whether a connection string contains only safe characters and format.
    /// </summary>
    /// <param name="connectionString">The connection string to check.</param>
    /// <returns><see langword="true"/> if the connection string appears safe; otherwise, <see langword="false"/>.</returns>
    private static bool IsConnectionStringSafe(string connectionString)
    {
        // Basic validation: connection string should contain "Data Source" or similar key
        // and should not contain obviously malicious patterns
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        // Check for basic connection string structure
        if (!connectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Check for obviously dangerous patterns
        if (connectionString.Contains("\0", StringComparison.Ordinal) ||
            connectionString.Contains("\x1f", StringComparison.Ordinal) ||
            connectionString.Contains("\x7f", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures that the specified connection string value is valid and throws a sanitized exception.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string configuration.</param>
    /// <param name="connectionStringValue">The connection string value to validate.</param>
    /// <param name="problemList">When this method returns, contains the list of validation problems if any; otherwise, <see langword="null"/>.</param>
    /// <returns>The sanitized connection string for error reporting.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionStringName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ValidationException">Thrown when the connection string value is invalid.</exception>
    public static string EnsureValidConnectionString(string? connectionStringName, string? connectionStringValue, out IReadOnlyList<ValidationProblem>? problemList)
    {
        ArgumentNullException.ThrowIfNull(connectionStringName);

        var result = ValidateConnectionString(connectionStringName, connectionStringValue, out string? sanitizedValue);
        problemList = result.IsValid ? null : result.ToList();

        result.ThrowIfInvalid(problems => new ValidationException(
            string.Join(" ", problems.Select(p => p.Message)),
            nameof(connectionStringValue)));

        return sanitizedValue ?? string.Empty;
    }
}