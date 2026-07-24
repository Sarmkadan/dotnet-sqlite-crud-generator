#nullable enable

// =============================================================================
// Author: Automated Test Generation
// Tests for EntityChangedEvent<T> variance and event bus dispatch behavior
// =============================================================================

using System;
using System.Threading.Tasks;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestSoftDelete;

public sealed class EntityChangedEventVarianceTests
{
    // Test entity for testing generic event operations
    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    // ------------------------------------------------------------------------
    // Variance bug tests: EntityCreatedEvent<Customer> should match EntityChangedEvent<Customer> handlers
    // ------------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_EntityCreatedEvent_MatchesEntityChangedEventHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();

        // Subscribe to the base generic type (EntityChangedEvent<TestEntity>)
        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(async @event =>
        {
            receivedEvents.Add(@event);
            await Task.CompletedTask;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act - Publish the concrete event type
        await eventBus.PublishAsync(createdEvent);

        // Assert - The handler should have received the event despite type mismatch
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].Should().BeSameAs(createdEvent);
        receivedEvents[0].Entity.Should().BeSameAs(entity);
    }

    [Fact]
    public async Task PublishAsync_EntityUpdatedEvent_MatchesEntityChangedEventHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(async @event =>
        {
            receivedEvents.Add(@event);
            await Task.CompletedTask;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var oldEntity = new TestEntity { Id = 1, Name = "Old Customer" };
        var updatedEvent = new EntityUpdatedEvent<TestEntity>(Guid.NewGuid(), entity, oldEntity);

        // Act
        await eventBus.PublishAsync(updatedEvent);

        // Assert
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].Should().BeSameAs(updatedEvent);
    }

    [Fact]
    public async Task PublishAsync_EntityDeletedEvent_MatchesEntityChangedEventHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(async @event =>
        {
            receivedEvents.Add(@event);
            await Task.CompletedTask;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var deletedEvent = new EntityDeletedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act
        await eventBus.PublishAsync(deletedEvent);

        // Assert
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].Should().BeSameAs(deletedEvent);
    }

    [Fact]
    public async Task PublishAsync_EntityCreatedEvent_MatchesSpecificConcreteHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityCreatedEvent<TestEntity>>();

        eventBus.Subscribe<EntityCreatedEvent<TestEntity>>(async @event =>
        {
            receivedEvents.Add(@event);
            await Task.CompletedTask;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act
        await eventBus.PublishAsync(createdEvent);

        // Assert
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].Should().BeSameAs(createdEvent);
    }

    [Fact]
    public async Task PublishAsync_EntityCreatedEvent_WithNoHandlers_DoesNotThrow()
    {
        // Arrange
        var eventBus = new EventBus();
        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act & Assert - Should not throw even with no handlers
        Func<Task> act = async () => await eventBus.PublishAsync(createdEvent);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act
        Func<Task> act = async () => await eventBus.PublishAsync<EntityCreatedEvent<TestEntity>>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ------------------------------------------------------------------------
    // EventBusValidation integration tests
    // ------------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_InvalidEvent_ThrowsValidationException()
    {
        // Arrange
        var eventBus = new EventBus();
        var invalidEvent = new EntityCreatedEvent<TestEntity>(Guid.Empty, null!);

        // Act
        Func<Task> act = async () => await eventBus.PublishAsync(invalidEvent);

        // Assert - Validation should catch the empty AggregateId
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PublishAsync_ValidEvent_PassesValidation()
    {
        // Arrange
        var eventBus = new EventBus();
        var validEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), new TestEntity { Id = 1, Name = "Valid" });

        // Act & Assert - Should not throw for valid event
        Func<Task> act = async () => await eventBus.PublishAsync(validEvent);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_EventWithNoHandlers_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EventBus>>();
        var eventBus = new EventBus(mockLogger.Object);
        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act
        await eventBus.PublishAsync(createdEvent);

        // Assert - Verify debug logging was called
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No subscribers found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    // ------------------------------------------------------------------------
    // Multiple handler registration tests
    // ------------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_MultipleHandlers_AllHandlersInvoked()
    {
        // Arrange
        var eventBus = new EventBus();
        var handler1Events = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();
        var handler2Events = new System.Collections.Generic.List<EntityCreatedEvent<TestEntity>>();

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(async @event =>
        {
            handler1Events.Add(@event);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<EntityCreatedEvent<TestEntity>>(async @event =>
        {
            handler2Events.Add(@event);
            await Task.CompletedTask;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act
        await eventBus.PublishAsync(createdEvent);

        // Assert - Both handlers should have received the event
        handler1Events.Should().HaveCount(1);
        handler2Events.Should().HaveCount(1);
        handler1Events[0].Should().BeSameAs(createdEvent);
        handler2Events[0].Should().BeSameAs(createdEvent);
    }

    [Fact]
    public async Task PublishAsync_SyncHandler_InvokedCorrectly()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(@event =>
        {
            receivedEvents.Add(@event);
            return;
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act
        await eventBus.PublishAsync(createdEvent);

        // Assert
        receivedEvents.Should().HaveCount(1);
        receivedEvents[0].Should().BeSameAs(createdEvent);
    }

    // ------------------------------------------------------------------------
    // Handler exception handling tests
    // ------------------------------------------------------------------------

    [Fact]
    public async Task PublishAsync_HandlerThrows_ContinuesProcessingOtherHandlers()
    {
        // Arrange
        var eventBus = new EventBus();
        var successfulEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();
        var failingHandlerCalled = false;

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(async @event =>
        {
            successfulEvents.Add(@event);
            await Task.CompletedTask;
        });

        eventBus.Subscribe<EntityChangedEvent<TestEntity>>(@event =>
        {
            failingHandlerCalled = true;
            throw new InvalidOperationException("Handler failed");
        });

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act & Assert - Should not throw even though one handler fails
        Func<Task> act = async () => await eventBus.PublishAsync(createdEvent);
        await act.Should().NotThrowAsync();

        // Assert - Successful handler should still have been called
        successfulEvents.Should().HaveCount(1);
        failingHandlerCalled.Should().BeTrue();
    }

    // ------------------------------------------------------------------------
    // Unsubscribe tests
    // ------------------------------------------------------------------------

    [Fact]
    public void Unsubscribe_RemovesHandler()
    {
        // Arrange
        var eventBus = new EventBus();
        var receivedEvents = new System.Collections.Generic.List<EntityChangedEvent<TestEntity>>();

        Action<EntityChangedEvent<TestEntity>> handler = @event =>
        {
            receivedEvents.Add(@event);
        };

        eventBus.Subscribe(handler);

        var entity = new TestEntity { Id = 1, Name = "Test Customer" };
        var createdEvent = new EntityCreatedEvent<TestEntity>(Guid.NewGuid(), entity);

        // Act - Publish before unsubscribe
        eventBus.PublishAsync(createdEvent).Wait();
        receivedEvents.Should().HaveCount(1);

        // Act - Unsubscribe
        var result = eventBus.Unsubscribe<EntityChangedEvent<TestEntity>>(handler);

        // Assert
        result.Should().BeTrue();

        // Publish after unsubscribe
        eventBus.PublishAsync(createdEvent).Wait();
        receivedEvents.Should().HaveCount(1); // Still 1, not 2
    }

    [Fact]
    public void Unsubscribe_NullHandler_ReturnsFalse()
    {
        // Arrange
        var eventBus = new EventBus();

        // Act
        var result = eventBus.Unsubscribe<EntityChangedEvent<TestEntity>>(null!);

        // Assert
        result.Should().BeFalse();
    }
}