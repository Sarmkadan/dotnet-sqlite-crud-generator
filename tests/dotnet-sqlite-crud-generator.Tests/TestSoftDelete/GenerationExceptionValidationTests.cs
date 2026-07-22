using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.TestSoftDelete;

public class GenerationExceptionValidationTests
{
    [Fact]
    public void Validate_WithValidGenerationException_ReturnsEmptyList()
    {
        // Arrange
        var validException = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors = validException.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullGenerationType_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = null,
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("GenerationType must be specified.");
    }

    [Fact]
    public void Validate_WithEmptyGenerationType_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("GenerationType must be specified.");
    }

    [Fact]
    public void Validate_WithWhitespaceGenerationType_IsValid()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "   ",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert - IsNullOrEmpty doesn't catch whitespace-only strings
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullSourceEntity_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = null,
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("SourceEntity must be specified.");
    }

    [Fact]
    public void Validate_WithEmptySourceEntity_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("SourceEntity must be specified.");
    }

    [Fact]
    public void Validate_WithWhitespaceSourceEntity_IsValid()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "   ",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert - IsNullOrEmpty doesn't catch whitespace-only strings
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithInvalidLineNumber_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 0
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("LineNumber must be a positive integer if specified.");
    }

    [Fact]
    public void Validate_WithNegativeLineNumber_ReturnsError()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = -1
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().Contain("LineNumber must be a positive integer if specified.");
    }

    [Fact]
    public void Validate_WithMultipleInvalidProperties_ReturnsMultipleErrors()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "",
            SourceEntity = null,
            LineNumber = -5
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain("GenerationType must be specified.");
        errors.Should().Contain("SourceEntity must be specified.");
        errors.Should().Contain("LineNumber must be a positive integer if specified.");
    }

    [Fact]
    public void IsValid_WithNullValue_ReturnsFalse()
    {
        // Arrange
        GenerationException? exception = null;

        // Act
        var isValid = exception.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var isValid = exception.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidGenerationType_ReturnsFalse()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var isValid = exception.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidSourceEntity_ReturnsFalse()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = null,
            LineNumber = 42
        };

        // Act
        var isValid = exception.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithInvalidLineNumber_ReturnsFalse()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 0
        };

        // Act
        var isValid = exception.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        GenerationException? exception = null;

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_WithValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_WithInvalidGenerationType_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("GenerationException is invalid. Validation errors: GenerationType must be specified.*");
    }

    [Fact]
    public void EnsureValid_WithInvalidSourceEntity_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = null,
            LineNumber = 42
        };

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("GenerationException is invalid. Validation errors: SourceEntity must be specified.*");
    }

    [Fact]
    public void EnsureValid_WithInvalidLineNumber_ThrowsArgumentException()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = -1
        };

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("GenerationException is invalid. Validation errors: LineNumber must be a positive integer if specified.*");
    }

    [Fact]
    public void EnsureValid_WithMultipleInvalidProperties_ThrowsArgumentExceptionWithAllErrors()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "",
            SourceEntity = null,
            LineNumber = 0
        };

        // Act
        Action act = () => exception.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("GenerationException is invalid. Validation errors: GenerationType must be specified.*SourceEntity must be specified.*LineNumber must be a positive integer if specified.*");
    }

    [Fact]
    public void Validate_ReturnsReadOnlyList()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors = exception.Validate();

        // Assert
        errors.Should().BeAssignableTo<IReadOnlyList<string>>();
        errors.Should().NotBeNull();
    }

    [Fact]
    public void Validate_WithValidException_ReturnsSameListOnMultipleCalls()
    {
        // Arrange
        var exception = new GenerationException("Test message")
        {
            GenerationType = "Repository",
            SourceEntity = "UserRepository",
            LineNumber = 42
        };

        // Act
        var errors1 = exception.Validate();
        var errors2 = exception.Validate();

        // Assert - Validate returns a new list each time, not the same instance
        errors1.Should().NotBeSameAs(errors2);
        errors1.Should().BeEmpty();
        errors2.Should().BeEmpty();
    }
}