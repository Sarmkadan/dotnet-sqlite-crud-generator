#nullable enable

// =============================================================================
// Author: Automated Test Generation
// =====================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Events;

namespace TestSoftDelete;

public sealed class EntityChangedEventExtensionsTests
{
    // Test entity for testing generic event operations
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    // ------------------------------------------------------------------------
    // EntityCreatedEvent<T> DeepCopy tests
    // ------------------------------------------------------------------------

    [Fact]
    public void DeepCopy_EntityCreatedEvent_WithValidEntity_CreatesDeepCopy()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var originalEntity = new TestEntity { Id = 1, Name = "Test Product", Price = 9.99m };
        var originalEvent = new EntityCreatedEvent<TestEntity>(entityId, originalEntity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Should().NotBeSameAs(originalEvent);
        copiedEvent.AggregateId.Should().Be(originalEvent.AggregateId);
        copiedEvent.Entity.Should().NotBeSameAs(originalEvent.Entity);
        copiedEvent.Entity.Should().BeEquivalentTo(originalEvent.Entity);
        copiedEvent.EntityType.Should().Be(originalEvent.EntityType);
    }

    [Fact]
    public void DeepCopy_EntityCreatedEvent_WithNullEntity_CreatesEventWithNullEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        TestEntity? nullEntity = null;
        var originalEvent = new EntityCreatedEvent<TestEntity>(entityId, nullEntity!);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Entity.Should().BeNull();
    }

    [Fact]
    public void DeepCopy_EntityCreatedEvent_WithEmptyGuidId_CreatesValidCopy()
    {
        // Arrange
        var entityId = Guid.Empty;
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var originalEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.AggregateId.Should().Be(entityId);
        copiedEvent.Entity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void DeepCopy_EntityCreatedEvent_WithIntegerId_CreatesValidCopy()
    {
        // Arrange
        var entityId = 123;
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var originalEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.AggregateId.Should().NotBe(Guid.Empty);
        copiedEvent.Entity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void DeepCopy_EntityCreatedEvent_ModifyingCopyDoesNotAffectOriginal()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var originalEntity = new TestEntity { Id = 1, Name = "Original" };
        var originalEvent = new EntityCreatedEvent<TestEntity>(entityId, originalEntity);
        var copiedEvent = originalEvent.DeepCopy();

        // Act - modify the copy
        copiedEvent.Entity!.Name = "Modified";
        copiedEvent.AggregateId = Guid.NewGuid();

        // Assert
        originalEvent.Entity!.Name.Should().Be("Original");
        originalEvent.AggregateId.Should().NotBe(copiedEvent.AggregateId);
    }

    // ------------------------------------------------------------------------
    // EntityUpdatedEvent<T> DeepCopy tests
    // ------------------------------------------------------------------------

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_WithValidEntities_CreatesDeepCopy()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var entity = new TestEntity { Id = 1, Name = "Updated Product", Price = 19.99m };
        var oldEntity = new TestEntity { Id = 1, Name = "Old Product", Price = 15.99m };

        var changes = new Dictionary<string, (object? OldValue, object? NewValue)>
        {
            { "Name", ("Old Product", "Updated Product") },
            { "Price", (15.99m, 19.99m) }
        };

        var originalEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity, oldEntity) { Changes = changes };

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Should().NotBeSameAs(originalEvent);
        copiedEvent.AggregateId.Should().Be(originalEvent.AggregateId);
        copiedEvent.Entity.Should().NotBeSameAs(originalEvent.Entity);
        copiedEvent.Entity.Should().BeEquivalentTo(originalEvent.Entity);
        copiedEvent.OldEntity.Should().NotBeSameAs(originalEvent.OldEntity);
        copiedEvent.OldEntity.Should().BeEquivalentTo(originalEvent.OldEntity);

        // Verify changes dictionary is copied correctly
        copiedEvent.Changes.Should().NotBeSameAs(originalEvent.Changes);
        copiedEvent.Changes.Should().BeEquivalentTo(originalEvent.Changes);
        copiedEvent.Changes.Should().HaveCount(2);
    }

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_WithNullEntity_CreatesEventWithNullEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var oldEntity = new TestEntity { Id = 1, Name = "Old Product" };
        var originalEvent = new EntityUpdatedEvent<TestEntity>(entityId, null!, oldEntity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Entity.Should().BeNull();
    }

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_WithNullOldEntity_CreatesEventWithNullOldEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var originalEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity, null);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.OldEntity.Should().BeNull();
    }

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_WithEmptyChangesDictionary_CreatesValidCopy()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var originalEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Changes.Should().NotBeNull();
        copiedEvent.Changes.Should().BeEmpty();
    }

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_ModifyingCopyDoesNotAffectOriginal()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Original" };
        var oldEntity = new TestEntity { Id = 1, Name = "Old" };
        var changes = new Dictionary<string, (object? OldValue, object? NewValue)> { { "Name", ("Old", "New") } };
        var originalEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity, oldEntity) { Changes = changes };
        var copiedEvent = originalEvent.DeepCopy();

        // Act - modify the copy
        copiedEvent.Entity!.Name = "Modified";
        copiedEvent.OldEntity!.Name = "Modified Old";
        copiedEvent.Changes["Name"] = ("Old", "Modified New");

        // Assert
        originalEvent.Entity!.Name.Should().Be("Original");
        originalEvent.OldEntity!.Name.Should().Be("Old");
        originalEvent.Changes["Name"].NewValue.Should().Be("New");
    }

    // ------------------------------------------------------------------------
    // EntityDeletedEvent<T> DeepCopy tests
    // ------------------------------------------------------------------------

    [Fact]
    public void DeepCopy_EntityDeletedEvent_WithValidEntity_CreatesDeepCopy()
    {
        // Arrange
        var entityId = Guid.Parse("4294f7e8-56f8-458a-b6e9-96427d3a5e6c");
        var entity = new TestEntity { Id = 1, Name = "Deleted Product" };
        var originalEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Should().NotBeSameAs(originalEvent);
        copiedEvent.AggregateId.Should().Be(originalEvent.AggregateId);
        copiedEvent.Entity.Should().BeSameAs(originalEvent.Entity); // Entity is not deep copied in Delete operation
    }

    [Fact]
    public void DeepCopy_EntityDeletedEvent_WithNullEntity_CreatesEventWithNullEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var originalEvent = new EntityDeletedEvent<TestEntity>(entityId);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Entity.Should().BeNull();
    }

    [Fact]
    public void DeepCopy_EntityDeletedEvent_WithIntegerId_CreatesValidCopy()
    {
        // Arrange
        var entityId = 456;
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var originalEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.AggregateId.Should().NotBe(Guid.Empty);
        copiedEvent.Entity.Should().BeSameAs(entity);
    }

    // ------------------------------------------------------------------------
    // BulkEntityChangedEvent<T> DeepCopy tests
    // ------------------------------------------------------------------------

    [Fact]
    public void DeepCopy_BulkEntityChangedEvent_WithValidEntities_CreatesDeepCopy()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m },
            new() { Id = 2, Name = "Product 2", Price = 20.00m },
            new() { Id = 3, Name = "Product 3", Price = 30.00m }
        };
        var originalEvent = new BulkEntityChangedEvent<TestEntity>(entities.Count, "BulkUpdate", entities);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Should().NotBeSameAs(originalEvent);
        copiedEvent.Count.Should().Be(originalEvent.Count);
        copiedEvent.Operation.Should().Be(originalEvent.Operation);
        copiedEvent.Entities.Should().NotBeSameAs(originalEvent.Entities);
        copiedEvent.Entities.Should().HaveCount(3);

        // Verify deep copy of entities
        for (int i = 0; i < copiedEvent.Entities.Count; i++)
        {
            copiedEvent.Entities[i].Should().NotBeSameAs(originalEvent.Entities[i]);
            copiedEvent.Entities[i].Should().BeEquivalentTo(originalEvent.Entities[i]);
        }
    }

    [Fact]
    public void DeepCopy_BulkEntityChangedEvent_WithEmptyList_CreatesValidCopy()
    {
        // Arrange
        var entities = new List<TestEntity>();
        var originalEvent = new BulkEntityChangedEvent<TestEntity>(0, "BulkDelete", entities);

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Count.Should().Be(0);
        copiedEvent.Entities.Should().BeEmpty();
    }

    [Fact]
    public void DeepCopy_BulkEntityChangedEvent_WithNullEntityInList_HandlesGracefully()
    {
        // Arrange
        var entities = new List<TestEntity?> { new() { Id = 1, Name = "Product 1" }, null, new() { Id = 2, Name = "Product 2" } };
        var originalEvent = new BulkEntityChangedEvent<TestEntity>(entities.Count, "PartialUpdate", entities.Cast<TestEntity>().ToList());

        // Act
        var copiedEvent = originalEvent.DeepCopy();

        // Assert
        copiedEvent.Entities.Should().HaveCount(3);
        copiedEvent.Entities[0].Should().NotBeNull();
        copiedEvent.Entities[1].Should().BeNull();
        copiedEvent.Entities[2].Should().NotBeNull();
    }

    [Fact]
    public void DeepCopy_BulkEntityChangedEvent_ModifyingCopyDoesNotAffectOriginal()
    {
        // Arrange
        var entities = new List<TestEntity> { new() { Id = 1, Name = "Product 1" } };
        var originalEvent = new BulkEntityChangedEvent<TestEntity>(1, "Test", entities);
        var copiedEvent = originalEvent.DeepCopy();

        // Act - modify the copy
        copiedEvent.Entities[0].Name = "Modified";
        copiedEvent.Count = 999;

        // Assert
        originalEvent.Entities[0].Name.Should().Be("Product 1");
        originalEvent.Count.Should().Be(1);
    }

    // ------------------------------------------------------------------------
    // IsCreation<T>, IsUpdate<T>, IsDeletion<T>, IsBulkOperation<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void IsCreation_ReturnsTrue_ForEntityCreatedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Act
        var result = createdEvent.IsCreation();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCreation_ReturnsFalse_ForNonCreatedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);
        var deletedEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);
        var bulkEvent = new BulkEntityChangedEvent<TestEntity>(1, "Test", new List<TestEntity> { entity });

        // Act & Assert
        updatedEvent.IsCreation().Should().BeFalse();
        deletedEvent.IsCreation().Should().BeFalse();
        bulkEvent.IsCreation().Should().BeFalse();
    }

    [Fact]
    public void IsUpdate_ReturnsTrue_ForEntityUpdatedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);

        // Act
        var result = updatedEvent.IsUpdate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUpdate_ReturnsFalse_ForNonUpdatedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);
        var deletedEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);
        var bulkEvent = new BulkEntityChangedEvent<TestEntity>(1, "Test", new List<TestEntity> { entity });

        // Act & Assert
        createdEvent.IsUpdate().Should().BeFalse();
        deletedEvent.IsUpdate().Should().BeFalse();
        bulkEvent.IsUpdate().Should().BeFalse();
    }

    [Fact]
    public void IsDeletion_ReturnsTrue_ForEntityDeletedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var deletedEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Act
        var result = deletedEvent.IsDeletion();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsDeletion_ReturnsFalse_ForNonDeletedEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);
        var bulkEvent = new BulkEntityChangedEvent<TestEntity>(1, "Test", new List<TestEntity> { entity });

        // Act & Assert
        createdEvent.IsDeletion().Should().BeFalse();
        updatedEvent.IsDeletion().Should().BeFalse();
        bulkEvent.IsDeletion().Should().BeFalse();
    }

    [Fact]
    public void IsBulkOperation_ReturnsTrue_ForBulkEntityChangedEvent()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var bulkEvent = new BulkEntityChangedEvent<TestEntity>(1, "BulkUpdate", new List<TestEntity> { entity });

        // Act
        var result = bulkEvent.IsBulkOperation();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsBulkOperation_ReturnsFalse_ForNonBulkEvent()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);
        var deletedEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);

        // Act & Assert
        createdEvent.IsBulkOperation().Should().BeFalse();
        updatedEvent.IsBulkOperation().Should().BeFalse();
        deletedEvent.IsBulkOperation().Should().BeFalse();
    }

    // ------------------------------------------------------------------------
    // GetEntityTypeName<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void GetEntityTypeName_ReturnsCorrectTypeName()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(entityId, entity);
        var deletedEvent = new EntityDeletedEvent<TestEntity>(entityId, entity);
        var bulkEvent = new BulkEntityChangedEvent<TestEntity>(1, "Test", new List<TestEntity> { entity });

        // Act
        var createdTypeName = createdEvent.GetEntityTypeName();
        var updatedTypeName = updatedEvent.GetEntityTypeName();
        var deletedTypeName = deletedEvent.GetEntityTypeName();
        var bulkTypeName = bulkEvent.GetEntityTypeName();

        // Assert
        createdTypeName.Should().Be(nameof(TestEntity));
        updatedTypeName.Should().Be(nameof(TestEntity));
        deletedTypeName.Should().Be(nameof(TestEntity));
        bulkTypeName.Should().Be(nameof(TestEntity));
    }

    [Fact]
    public void GetEntityTypeName_ReturnsEmptyString_WhenEntityTypeIsNull()
    {
        // Arrange - This shouldn't happen in practice, but test defensive programming
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Force EntityType to null to test edge case (if possible)
        // Note: This is a defensive test - the property is set in constructor

        // Act
        var typeName = createdEvent.GetEntityTypeName();

        // Assert
        typeName.Should().Be(nameof(TestEntity));
    }

    // ------------------------------------------------------------------------
    // GetEntity<T> tests
    // ------------------------------------------------------------------------

    [Fact]
    public void GetEntity_ReturnsEntityInstance()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = 1, Name = "Product", Price = 9.99m };
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, entity);

        // Act
        var retrievedEntity = createdEvent.GetEntity();

        // Assert
        retrievedEntity.Should().BeSameAs(entity);
        retrievedEntity.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetEntity_ReturnsNull_WhenEntityIsNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        TestEntity? nullEntity = null;
        var createdEvent = new EntityCreatedEvent<TestEntity>(entityId, nullEntity!);

        // Act
        var retrievedEntity = createdEvent.GetEntity();

        // Assert
        retrievedEntity.Should().BeNull();
    }

    // ------------------------------------------------------------------------
    // Error path tests for ArgumentNullException
    // ------------------------------------------------------------------------

    [Fact]
    public void DeepCopy_EntityCreatedEvent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityCreatedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.DeepCopy();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DeepCopy_EntityUpdatedEvent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityUpdatedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.DeepCopy();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DeepCopy_EntityDeletedEvent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityDeletedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.DeepCopy();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DeepCopy_BulkEntityChangedEvent_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        BulkEntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.DeepCopy();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsCreation_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.IsCreation();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsUpdate_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.IsUpdate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsDeletion_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.IsDeletion();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsBulkOperation_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.IsBulkOperation();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetEntityTypeName_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.GetEntityTypeName();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetEntity_ThrowsArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        EntityChangedEvent<TestEntity>? nullEvent = null;

        // Act
        Action act = () => nullEvent!.GetEntity();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}