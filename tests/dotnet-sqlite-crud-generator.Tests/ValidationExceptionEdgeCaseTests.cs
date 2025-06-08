#nullable enable
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Exceptions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Edge-case tests for ValidationException - error aggregation, factory methods, and message formatting.
/// </summary>
public sealed class ValidationExceptionEdgeCaseTests
{
    [Fact]
    public void Constructor_Message_SetsMessageCorrectly()
    {
        var ex = new ValidationException("test error");

        ex.Message.Should().Be("test error");
        ex.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ValidationException("outer", inner);

        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AddError_SingleError_AddsToCollection()
    {
        var ex = new ValidationException("validation failed");

        ex.AddError("Name", "Name is required");

        ex.Errors.Should().HaveCount(1);
        ex.Errors[0].Property.Should().Be("Name");
        ex.Errors[0].Message.Should().Be("Name is required");
    }

    [Fact]
    public void AddError_MultipleErrors_AccumulatesAll()
    {
        var ex = new ValidationException("validation failed");

        ex.AddError("Name", "Required");
        ex.AddError("Email", "Invalid format");
        ex.AddError("Age", "Must be positive");

        ex.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void FromErrors_EmptyList_CreatesExceptionWithEmptyErrors()
    {
        var ex = ValidationException.FromErrors([]);

        ex.Errors.Should().BeEmpty();
        ex.Message.Should().Contain("Validation failed");
    }

    [Fact]
    public void FromErrors_MultipleErrors_ConcatenatesMessages()
    {
        var errors = new List<ValidationException.ValidationError>
        {
            new() { Property = "Name", Message = "Required" },
            new() { Property = "Email", Message = "Invalid" }
        };

        var ex = ValidationException.FromErrors(errors);

        ex.Message.Should().Contain("Name: Required");
        ex.Message.Should().Contain("Email: Invalid");
        ex.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void FromErrors_SingleError_FormatsMessageCorrectly()
    {
        var errors = new List<ValidationException.ValidationError>
        {
            new() { Property = "Id", Message = "Cannot be zero" }
        };

        var ex = ValidationException.FromErrors(errors);

        ex.Message.Should().Contain("Id: Cannot be zero");
    }
}
