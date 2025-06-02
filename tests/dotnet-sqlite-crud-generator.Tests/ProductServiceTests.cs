// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Events;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class ProductServiceTests
{
    private readonly Mock<IRepository<Product, int>> _productRepoMock = new();
    private readonly Mock<IRepository<Category, int>> _categoryRepoMock = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_productRepoMock.Object, _categoryRepoMock.Object);
    }

    [Fact]
    public async Task GetAsync_WithIdOfZero_ThrowsArgumentException()
    {
        // Act
        var act = async () => await _service.GetAsync(0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Product ID must be greater than 0*");
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsProductReturnedByRepository()
    {
        // Arrange
        var expected = new Product { Id = 7, Name = "Widget Pro", Sku = "WP-007", CategoryId = 2, Price = 49.99m };
        _productRepoMock
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetAsync(7);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
        result.Name.Should().Be("Widget Pro");
        _productRepoMock.Verify(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ReturnsAllProductsFromRepository()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Name = "Alpha", Sku = "A-001", CategoryId = 1, Price = 5.00m },
            new() { Name = "Beta",  Sku = "B-002", CategoryId = 1, Price = 12.50m },
        };
        _productRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Alpha", "Beta" });
        _productRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenRepositoryConfirmsExistence_ReturnsTrue()
    {
        // Arrange
        _productRepoMock
            .Setup(r => r.ExistsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.ExistsAsync(42);

        // Assert
        exists.Should().BeTrue();
        _productRepoMock.Verify(r => r.ExistsAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class EventBusTests
{
    [Fact]
    public async Task PublishAsync_WithRegisteredAsyncHandler_HandlerReceivesPublishedEvent()
    {
        // Arrange
        var bus = new EventBus();
        StockDepletedEvent? captured = null;

        bus.Subscribe<StockDepletedEvent>(async e =>
        {
            captured = e;
            await Task.CompletedTask;
        });

        var published = new StockDepletedEvent { ProductId = 99, Remaining = 0 };

        // Act
        await bus.PublishAsync(published);

        // Assert
        captured.Should().NotBeNull();
        captured!.ProductId.Should().Be(99);
        captured.Remaining.Should().Be(0);
    }

    [Fact]
    public async Task GetEventHistory_AfterPublishingMultipleEvents_ContainsAllPublishedEventTypes()
    {
        // Arrange
        var bus = new EventBus();
        var firstEvent  = new StockDepletedEvent { ProductId = 1, Remaining = 0 };
        var secondEvent = new StockDepletedEvent { ProductId = 2, Remaining = 3 };

        // Act
        await bus.PublishAsync(firstEvent);
        await bus.PublishAsync(secondEvent);
        var history = bus.GetEventHistory().ToList();

        // Assert
        history.Should().HaveCount(2);
        history.Should().AllSatisfy(e => e.EventTypeName.Should().Be(nameof(StockDepletedEvent)));
    }

    [Fact]
    public void GetSubscriberCount_AfterSubscribing_ReflectsRegisteredHandlers()
    {
        // Arrange
        var bus = new EventBus();

        Func<StockDepletedEvent, Task> handlerA = _ => Task.CompletedTask;
        Func<StockDepletedEvent, Task> handlerB = _ => Task.CompletedTask;

        // Act
        bus.Subscribe(handlerA);
        bus.Subscribe(handlerB);

        // Assert
        bus.GetSubscriberCount<StockDepletedEvent>().Should().Be(2);
    }

    [Fact]
    public void ClearEventHistory_AfterPublishingEvents_LeavesHistoryEmpty()
    {
        // Arrange
        var bus = new EventBus();
        bus.PublishAsync(new StockDepletedEvent { ProductId = 1, Remaining = 5 }).GetAwaiter().GetResult();

        // Act
        bus.ClearEventHistory();

        // Assert
        bus.GetEventHistory().Should().BeEmpty();
    }

    private sealed class StockDepletedEvent : IEvent
    {
        public int ProductId { get; init; }
        public int Remaining { get; init; }
        public string GetEventName() => "StockDepleted";
    }
}
