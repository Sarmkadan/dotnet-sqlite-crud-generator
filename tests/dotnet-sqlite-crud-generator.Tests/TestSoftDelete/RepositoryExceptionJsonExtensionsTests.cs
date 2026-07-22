#nullable enable

using System.Text.Json;
using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.TestSoftDelete;

/// <summary>
/// Unit tests for <see cref="RepositoryExceptionJsonExtensions"/> JSON serialization and deserialization methods.
/// </summary>
public class RepositoryExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidRepositoryException_ReturnsValidJson()
    {
        // Arrange
        var exception = new RepositoryException("Test error message")
        {
            EntityType = "User",
            EntityId = 42
        };

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Test error message");
        json.Should().Contain("entityType"); // camelCase property name
        json.Should().Contain("42");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var exception = new RepositoryException("Test error message");

        // Act
        var json = exception.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Test error message");
        json.Should().Contain("{");
        json.Should().Contain("}");
        // Should have newlines and indentation
        json.Should().Match("*\n*  *");
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var exception = new RepositoryException("Test error message");

        // Act
        var json = exception.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Test error message");
        // Should not have newlines
        json.Should().NotMatch("*\n*");
    }

    [Fact]
    public void ToJson_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        RepositoryException? exception = null;

        // Act & Assert
        var act = () => exception!.ToJson();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithValidJson_ThrowsException()
    {
        // Arrange
        var exception = new RepositoryException("Original message")
        {
            EntityType = "Product",
            EntityId = 100
        };
        var json = exception.ToJson();

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.FromJson(json);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.FromJson(json!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        string json = "";

        // Act
        var result = RepositoryExceptionJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsJsonException()
    {
        // Arrange
        string json = "   \n  \t  ";

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.FromJson(json);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        string json = "invalid json {{{";

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.FromJson(json);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void FromJson_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.FromJson(json!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ThrowsException()
    {
        // Arrange
        var exception = new RepositoryException("Test message")
        {
            EntityType = "Order",
            EntityId = 5
        };
        var json = exception.ToJson();

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.TryFromJson(json, out _);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var act = () => RepositoryExceptionJsonExtensions.TryFromJson(json!, out _);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ReturnsTrueAndNull()
    {
        // Arrange
        string json = "";

        // Act
        var result = RepositoryExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeTrue();
        deserialized.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceJson_ReturnsFalseAndNull()
    {
        // Arrange
        string json = "   \n  \t  ";

        // Act
        var result = RepositoryExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeFalse();
        deserialized.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        string json = "invalid json {{{";

        // Act
        var result = RepositoryExceptionJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        result.Should().BeFalse();
        deserialized.Should().BeNull();
    }
}