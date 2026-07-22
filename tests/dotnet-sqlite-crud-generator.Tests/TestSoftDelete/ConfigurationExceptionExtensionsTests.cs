#nullable enable

using System;
using System.Collections.Generic;
using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.TestSoftDelete;

/// <summary>
/// Unit tests for <see cref="ConfigurationExceptionExtensions"/> extension methods.
/// </summary>
public class ConfigurationExceptionExtensionsTests
{
    [Fact]
    public void WithContext_SingleKeyValuePair_AddsContextToMessage()
    {
        // Arrange
        var exception = new ConfigurationException("Required configuration 'DatabasePath' is missing or invalid.");
        const string contextKey = "FilePath";
        const string contextValue = "/app/config/database.db";

        // Act
        var result = exception.WithContext(contextKey, contextValue);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(exception);
        result.Message.Should().Contain("Required configuration 'DatabasePath' is missing or invalid.");
        result.Message.Should().Contain("Context: FilePath=/app/config/database.db");
        result.InnerException.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithContext_SingleKeyValuePair_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        ConfigurationException? exception = null;
        const string contextKey = "Key";
        const string contextValue = "Value";

        // Act
        Action act = () => exception!.WithContext(contextKey, contextValue);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithContext_SingleKeyValuePair_WithNullContextKey_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        string? contextKey = null;
        const string contextValue = "Value";

        // Act
        Action act = () => exception.WithContext(contextKey!, contextValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithContext_SingleKeyValuePair_WithEmptyContextKey_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        const string contextKey = "";
        const string contextValue = "Value";

        // Act
        Action act = () => exception.WithContext(contextKey, contextValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithContext_MultipleKeyValuePairs_AddsAllContextToMessage()
    {
        // Arrange
        var exception = new ConfigurationException("Configuration 'Timeout' is missing or invalid.");
        var context = new Dictionary<string, string>
        {
            { "TimeoutValue", "30" },
            { "MaxRetries", "5" },
            { "RetryDelay", "1000" }
        };

        // Act
        var result = exception.WithContext(context);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("Configuration 'Timeout' is missing or invalid.");
        result.Message.Should().Contain("Context:");
        result.Message.Should().Contain("TimeoutValue=30");
        result.Message.Should().Contain("MaxRetries=5");
        result.Message.Should().Contain("RetryDelay=1000");
        result.InnerException.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithContext_MultipleKeyValuePairs_WithEmptyDictionary_ReturnsSameException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        var context = new Dictionary<string, string>();

        // Act
        var result = exception.WithContext(context);

        // Assert
        result.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithContext_MultipleKeyValuePairs_WithNullDictionary_ReturnsSameException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        IReadOnlyDictionary<string, string>? context = null;

        // Act
        var result = exception.WithContext(context!);

        // Assert
        result.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithContext_MultipleKeyValuePairs_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        ConfigurationException? exception = null;
        var context = new Dictionary<string, string> { { "Key", "Value" } };

        // Act
        Action act = () => exception!.WithContext(context);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMessage_CreatesNewExceptionWithCustomMessage()
    {
        // Arrange
        var exception = new ConfigurationException("Original error message");
        const string customMessage = "Custom error message with more details";

        // Act
        var result = exception.WithMessage(customMessage);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeSameAs(exception);
        result.Message.Should().Be(customMessage);
        result.InnerException.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithMessage_WithNullException_ThrowsArgumentNullException()
    {
        // Arrange
        ConfigurationException? exception = null;
        const string customMessage = "Custom message";

        // Act
        Action act = () => exception!.WithMessage(customMessage);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithMessage_WithNullCustomMessage_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        string? customMessage = null;

        // Act
        Action act = () => exception.WithMessage(customMessage!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithMessage_WithEmptyCustomMessage_ThrowsArgumentException()
    {
        // Arrange
        var exception = new ConfigurationException("Test message");
        const string customMessage = "";

        // Act
        Action act = () => exception.WithMessage(customMessage);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsMissingConfiguration_ReturnsTrue_WhenMessageContainsMissingOrInvalid()
    {
        // Arrange
        var exception = new ConfigurationException("Required configuration 'DatabasePath' is missing or invalid.");

        // Act
        var result = exception.IsMissingConfiguration();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMissingConfiguration_ReturnsFalse_WhenMessageDoesNotContainMissingOrInvalid()
    {
        // Arrange
        var exception = new ConfigurationException("Configuration 'Timeout' has invalid value: '30'. Expected format or range not met.");

        // Act
        var result = exception.IsMissingConfiguration();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMissingConfiguration_ReturnsFalse_WhenMessageContainsMissingButNotInvalid()
    {
        // Arrange
        var exception = new ConfigurationException("Configuration 'Timeout' is missing.");

        // Act
        var result = exception.IsMissingConfiguration();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMissingConfiguration_ReturnsFalse_WhenMessageContainsInvalidButNotMissing()
    {
        // Arrange
        var exception = new ConfigurationException("Configuration 'Timeout' is invalid.");

        // Act
        var result = exception.IsMissingConfiguration();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMissingConfiguration_WithNullException_ReturnsFalse()
    {
        // Arrange
        ConfigurationException? exception = null;

        // Act
        var result = exception!.IsMissingConfiguration();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMissingConfiguration_WithCaseSensitiveCheck_ReturnsTrue()
    {
        // Arrange
        var exception = new ConfigurationException("Required configuration 'DatabasePath' is MISSING or INVALID.");

        // Act
        var result = exception.IsMissingConfiguration();

        // Assert
        result.Should().BeFalse(); // Should be case-sensitive
    }

    [Fact]
    public void WithContext_SingleKeyValuePair_ChainingWorks()
    {
        // Arrange
        var exception = new ConfigurationException("Base error message");

        // Act
        var result = exception
            .WithContext("Key1", "Value1")
            .WithContext("Key2", "Value2");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("Base error message");
        result.Message.Should().Contain("Key1=Value1");
        result.Message.Should().Contain("Key2=Value2");
        result.InnerException.Should().BeOfType<ConfigurationException>();
        result.InnerException!.InnerException.Should().BeSameAs(exception);
    }

    [Fact]
    public void WithContext_MultipleKeyValuePairs_WithSpecialCharactersInValues_HandlesCorrectly()
    {
        // Arrange
        var exception = new ConfigurationException("Configuration error");
        var context = new Dictionary<string, string>
        {
            { "Path", "/app/data/file.db" },
            { "Connection", "Server=localhost;Database=test;" },
            { "Pattern", "*.txt" }
        };

        // Act
        var result = exception.WithContext(context);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("Path=/app/data/file.db");
        result.Message.Should().Contain("Connection=Server=localhost;Database=test;");
        result.Message.Should().Contain("Pattern=*.txt");
    }
}