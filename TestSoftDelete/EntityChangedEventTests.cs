#nullable enable

// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Events;

namespace TestSoftDelete;

public sealed class EntityChangedEventTests
{
    // ------------------------------------------------------------------------
    // EntityCreatedEvent<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void EntityCreatedEvent_Constructor_WithGuidAndEntity_SetsPropertiesCorrectly()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var entity = new TestEntity { Id = 1, Name = "Test Product" };

        // Act
        var evt = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.AggregateId.Should().Be(entityId);
        evt.Entity.Should().BeSameAs(entity);
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Created");
    }

    [Fact]
    public void EntityCreatedEvent_Constructor_WithIntegerIdAndEntity_GeneratesGuid()
    {
        // Arrange
        var entityId = 123;
        var entity = new TestEntity { Id = 1, Name = "Test Product" };

        // Act
        var evt = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.AggregateId.Should().NotBe(Guid.Empty);
        evt.Entity.Should().BeSameAs(entity);
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Created");
    }

    [Fact]
    public void EntityCreatedEvent_Constructor_WithNullEntity_SetsEntityToNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        TestEntity? entity = null;

        // Act
        var evt = new EntityCreatedEvent<TestEntity>(entityId, entity!);

        // Assert
        evt.Entity.Should().BeNull();
    }

    // ------------------------------------------------------------------------
    // EntityUpdatedEvent<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void EntityUpdatedEvent_Constructor_WithGuidAndEntity_SetsPropertiesCorrectly()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var entity = new TestEntity { Id = 1, Name = "Updated Product" };
        var oldEntity = new TestEntity { Id = 1, Name = "Old Product" };

        // Act
        var evt = new EntityUpdatedEvent<TestEntity>(entityId, entity, oldEntity);

        // Assert
        evt.AggregateId.Should().Be(entityId);
        evt.Entity.Should().BeSameAs(entity);
        evt.OldEntity.Should().BeSameAs(oldEntity);
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Updated");
        evt.Changes.Should().NotBeNull();
        evt.Changes.Should().BeEmpty();
    }

    [Fact]
    public void EntityUpdatedEvent_Constructor_WithIntegerIdAndEntity_GeneratesGuid()
    {
        // Arrange
        var entityId = 456;
        var entity = new TestEntity { Id = 1, Name = "Updated Product" };

        // Act
        var evt = new EntityUpdatedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.AggregateId.Should().NotBe(Guid.Empty);
        evt.Entity.Should().BeSameAs(entity);
        evt.OldEntity.Should().BeNull();
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Updated");
    }

    [Fact]
    public void EntityUpdatedEvent_Constructor_WithNullOldEntity_SetsOldEntityToNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        TestEntity? oldEntity = null;

        // Act
        var evt = new EntityUpdatedEvent<TestEntity>(entityId, entity, oldEntity);

        // Assert
        evt.OldEntity.Should().BeNull();
    }

    [Fact]
    public void EntityUpdatedEvent_ChangesProperty_IsInitializedAsEmptyDictionary()
    {
        // Arrange & Act
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };

        var evt = new EntityUpdatedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.Changes.Should().NotBeNull();
        evt.Changes.Should().BeEmpty();
        evt.Changes.Should().BeOfType<Dictionary<string, (object? OldValue, object? NewValue)>>();
    }

    [Fact]
    public void EntityUpdatedEvent_ChangesProperty_CanBeModified()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var oldEntity = new TestEntity { Id = 1, Name = "Old Product" };

        var evt = new EntityUpdatedEvent<TestEntity>(entityId, entity, oldEntity);

        // Act
        evt.Changes.Add("Name", ("Old Product", "New Product"));

        // Assert
        evt.Changes.Should().ContainKey("Name");
        evt.Changes["Name"].OldValue.Should().Be("Old Product");
        evt.Changes["Name"].NewValue.Should().Be("New Product");
    }

    // ------------------------------------------------------------------------
    // EntityDeletedEvent<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void EntityDeletedEvent_Constructor_WithGuidAndEntity_SetsPropertiesCorrectly()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var entity = new TestEntity { Id = 1, Name = "Deleted Product" };

        // Act
        var evt = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.AggregateId.Should().Be(entityId);
        evt.Entity.Should().BeSameAs(entity);
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Deleted");
    }

    [Fact]
    public void EntityDeletedEvent_Constructor_WithIntegerIdAndEntity_GeneratesGuid()
    {
        // Arrange
        var entityId = 789;
        var entity = new TestEntity { Id = 1, Name = "Deleted Product" };

        // Act
        var evt = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.AggregateId.Should().NotBe(Guid.Empty);
        evt.Entity.Should().BeSameAs(entity);
        evt.EntityType.Should().Be(nameof(TestEntity));
        evt.EventName.Should().Be($"{nameof(TestEntity)}Deleted");
    }

    [Fact]
    public void EntityDeletedEvent_Constructor_WithNullEntity_SetsEntityToNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        TestEntity? entity = null;

        // Act
        var evt = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Assert
        evt.Entity.Should().BeNull();
    }

    [Fact]
    public void EntityDeletedEvent_Constructor_WithOnlyAggregateId_SetsEntityToNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var evt = new EntityDeletedEvent<TestEntity>(entityId);

        // Assert
        evt.Entity.Should().BeNull();
        evt.AggregateId.Should().Be(entityId);
    }

    // ------------------------------------------------------------------------
    // BulkEntityChangedEvent<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void BulkEntityChangedEvent_Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        const int count = 5;
        const string operation = "BulkUpdate";
        var entities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Product 1" },
            new() { Id = 2, Name = "Product 2" },
            new() { Id = 3, Name = "Product 3" }
        };

        // Act
        var evt = new BulkEntityChangedEvent<TestEntity>(count, operation, entities);

        // Assert
        evt.Count.Should().Be(count);
        evt.Operation.Should().Be(operation);
        evt.Entities.Should().BeSameAs(entities);
        evt.Entities.Should().HaveCount(3);
        evt.AggregateId.Should().NotBe(Guid.Empty);
        evt.EventName.Should().Be($"Bulk{nameof(TestEntity)}Changed");
    }

    [Fact]
    public void BulkEntityChangedEvent_Constructor_WithEmptyList_CreatesValidEvent()
    {
        // Arrange
        const int count = 0;
        const string operation = "BulkDelete";
        var entities = new List<TestEntity>();

        // Act
        var evt = new BulkEntityChangedEvent<TestEntity>(count, operation, entities);

        // Assert
        evt.Count.Should().Be(count);
        evt.Operation.Should().Be(operation);
        evt.Entities.Should().BeEmpty();
        evt.AggregateId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void BulkEntityChangedEvent_Constructor_WithNullList_ThrowsArgumentNullException()
    {
        // Arrange
        const int count = 3;
        const string operation = "BulkUpdate";
        List<TestEntity>? entities = null;

        // Act
        Action act = () => new BulkEntityChangedEvent<TestEntity>(count, operation, entities!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BulkEntityChangedEvent_EntitiesProperty_CanBeModified()
    {
        // Arrange
        var entities = new List<TestEntity> { new() { Id = 1, Name = "Product 1" } };
        var evt = new BulkEntityChangedEvent<TestEntity>(1, "Test", entities);

        // Act
        evt.Entities.Add(new TestEntity { Id = 2, Name = "Product 2" });

        // Assert
        evt.Entities.Should().HaveCount(2);
    }

    // ------------------------------------------------------------------------
    // Test entity class for testing
    // ------------------------------------------------------------------------

    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
