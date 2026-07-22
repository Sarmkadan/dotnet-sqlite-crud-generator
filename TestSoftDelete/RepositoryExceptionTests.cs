#nullable enable
// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Exceptions;

namespace TestSoftDelete;

public sealed class RepositoryExceptionTests
{
    // ------------------------------------------------------------------------
    // Constructor tests
    // ------------------------------------------------------------------------

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndLeavesPropertiesNull()
    {
        // Arrange
        const string message = "Something went wrong";

        // Act
        var ex = new RepositoryException(message);

        // Assert
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
        ex.EntityType.Should().BeNull();
        ex.EntityId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsAllProperties()
    {
        // Arrange
        const string message = "Outer exception";
        var inner = new InvalidOperationException("Inner cause");

        // Act
        var ex = new RepositoryException(message, inner);

        // Assert
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
        ex.EntityType.Should().BeNull();
        ex.EntityId.Should().BeNull();
    }

    // ------------------------------------------------------------------------
    // Static factory method tests
    // ------------------------------------------------------------------------

    [Fact]
    public void EntityNotFound_ReturnsException_WithCorrectMessageAndProperties()
    {
        // Arrange
        const string entityType = "Product";
        const int entityId = 42;

        // Act
        var ex = RepositoryException.EntityNotFound(entityType, entityId);

        // Assert
        ex.Message.Should().Be($"Entity of type '{entityType}' with ID {entityId} was not found.");
        ex.EntityType.Should().Be(entityType);
        ex.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void EntityNotFound_AllowsNullOrEmptyEntityType()
    {
        // Arrange
        string? entityType = null;
        const int entityId = 0;

        // Act
        var ex = RepositoryException.EntityNotFound(entityType!, entityId);

        // Assert
        ex.Message.Should().Contain("Entity of type ''");
        ex.EntityType.Should().BeNull();
        ex.EntityId.Should().Be(entityId);
    }

    [Fact]
    public void DuplicateKey_ReturnsException_WithCorrectMessageAndEntityType()
    {
        // Arrange
        const string entityType = "User";
        const string property = "Email";
        const string value = "test@example.com";

        // Act
        var ex = RepositoryException.DuplicateKey(entityType, property, value);

        // Assert
        ex.Message.Should().Be($"An entity of type '{entityType}' with {property} = '{value}' already exists.");
        ex.EntityType.Should().Be(entityType);
        ex.EntityId.Should().BeNull();
    }

    [Fact]
    public void DuplicateKey_AllowsNullPropertyOrValue()
    {
        // Arrange
        const string entityType = "Order";
        string? property = null;
        object? value = null;

        // Act
        var ex = RepositoryException.DuplicateKey(entityType, property!, value!);

        // Assert
        ex.Message.Should().Contain($"with {property} = '{value}'");
        ex.EntityType.Should().Be(entityType);
    }

    [Fact]
    public void ConstraintViolation_ReturnsException_WithCorrectMessageAndEntityType()
    {
        // Arrange
        const string entityType = "Invoice";
        const string constraint = "CHECK (Amount > 0)";

        // Act
        var ex = RepositoryException.ConstraintViolation(entityType, constraint);

        // Assert
        ex.Message.Should().Be($"Constraint violation in entity '{entityType}': {constraint}");
        ex.EntityType.Should().Be(entityType);
        ex.EntityId.Should().BeNull();
    }

    [Fact]
    public void ConstraintViolation_AllowsNullOrEmptyConstraint()
    {
        // Arrange
        const string entityType = "Customer";
        string? constraint = null;

        // Act
        var ex = RepositoryException.ConstraintViolation(entityType, constraint!);

        // Assert
        ex.Message.Should().Contain($"Constraint violation in entity '{entityType}': ");
        ex.EntityType.Should().Be(entityType);
    }
}
