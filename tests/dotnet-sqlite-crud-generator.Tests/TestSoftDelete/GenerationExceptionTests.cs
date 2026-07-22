using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.TestSoftDelete;

public class GenerationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageCorrectly()
    {
        // Arrange
        var message = "Test generation error message";

        // Act
        var exception = new GenerationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
        exception.GenerationType.Should().BeNull();
        exception.SourceEntity.Should().BeNull();
        exception.LineNumber.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
    {
        // Arrange
        var message = "Test generation error with inner exception";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new GenerationException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(innerException);
        exception.GenerationType.Should().BeNull();
        exception.SourceEntity.Should().BeNull();
        exception.LineNumber.Should().BeNull();
    }

    [Fact]
    public void MissingConfiguration_WithConfigName_CreatesCorrectException()
    {
        // Arrange
        var configName = "ConnectionString";

        // Act
        var exception = GenerationException.MissingConfiguration(configName);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Required configuration '{configName}' is missing.");
        exception.GenerationType.Should().Be("Configuration");
        exception.SourceEntity.Should().BeNull();
        exception.LineNumber.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingConfiguration_WithVariousConfigNames_HandlesCorrectly(string configName)
    {
        // Act
        var exception = GenerationException.MissingConfiguration(configName);

        // Assert
        exception.Should().NotBeNull();
        if (!string.IsNullOrWhiteSpace(configName))
        {
            exception.Message.Should().Contain(configName);
        }
        exception.GenerationType.Should().Be("Configuration");
    }

    [Fact]
    public void InvalidModel_WithModelNameAndReason_CreatesCorrectException()
    {
        // Arrange
        var modelName = "User";
        var reason = "has no primary key defined";

        // Act
        var exception = GenerationException.InvalidModel(modelName, reason);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Model '{modelName}' is invalid: {reason}");
        exception.GenerationType.Should().Be("Model");
        exception.SourceEntity.Should().Be(modelName);
        exception.LineNumber.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Theory]
    [InlineData("", "reason")]
    [InlineData("   ", "reason")]
    [InlineData("User", "")]
    public void InvalidModel_WithEdgeCaseInputs_HandlesCorrectly(string modelName, string reason)
    {
        // Act
        var exception = GenerationException.InvalidModel(modelName, reason);

        // Assert
        exception.Should().NotBeNull();
        if (!string.IsNullOrWhiteSpace(modelName))
        {
            exception.Message.Should().Contain(modelName);
        }
        if (!string.IsNullOrWhiteSpace(reason))
        {
            exception.Message.Should().Contain(reason);
        }
        exception.GenerationType.Should().Be("Model");
        exception.SourceEntity.Should().Be(modelName);
    }

    [Fact]
    public void CodeGenerationFailed_WithAllParameters_CreatesCorrectException()
    {
        // Arrange
        var generationType = "Repository";
        var sourceEntity = "UserRepository";
        var innerException = new InvalidOperationException("Code generation failed");

        // Act
        var exception = GenerationException.CodeGenerationFailed(generationType, sourceEntity, innerException);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be($"Failed to generate {generationType} for {sourceEntity}");
        exception.GenerationType.Should().Be(generationType);
        exception.SourceEntity.Should().Be(sourceEntity);
        exception.InnerException.Should().BeSameAs(innerException);
        exception.LineNumber.Should().BeNull();
    }

    [Theory]
    [InlineData("", "UserRepository")]
    [InlineData("Repository", "")]
    public void CodeGenerationFailed_WithNullOrEmptyParameters_HandlesCorrectly(string generationType, string sourceEntity)
    {
        // Act
        var exception = GenerationException.CodeGenerationFailed(generationType, sourceEntity, null);

        // Assert
        exception.Should().NotBeNull();
        exception.GenerationType.Should().Be(generationType);
        exception.SourceEntity.Should().Be(sourceEntity);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        // Arrange
        var exception = new GenerationException("Test message");

        // Act
        exception.GenerationType = "TestGenerationType";
        exception.SourceEntity = "TestEntity";
        exception.LineNumber = 42;

        // Assert
        exception.GenerationType.Should().Be("TestGenerationType");
        exception.SourceEntity.Should().Be("TestEntity");
        exception.LineNumber.Should().Be(42);
    }

    [Fact]
    public void Properties_AreMutable()
    {
        // Arrange
        var exception = GenerationException.MissingConfiguration("TestConfig");

        // Act
        exception.SourceEntity = "TestEntity";
        exception.LineNumber = 100;

        // Assert
        exception.GenerationType.Should().Be("Configuration");
        exception.SourceEntity.Should().Be("TestEntity");
        exception.LineNumber.Should().Be(100);
    }
}