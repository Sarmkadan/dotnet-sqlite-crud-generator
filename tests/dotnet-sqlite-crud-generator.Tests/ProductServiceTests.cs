#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Events;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class ProductServiceTests
{
    private readonly IRepository<Product, int> _productRepoMock = Substitute.For<IRepository<Product, int>>();
    private readonly IRepository<Category, int> _categoryRepoMock = Substitute.For<IRepository<Category, int>>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_productRepoMock, _categoryRepoMock);
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
            .GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _service.GetAsync(7);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
        result.Name.Should().Be("Widget Pro");
        await _productRepoMock.Received(1).GetByIdAsync(7, Arg.Any<CancellationToken>());
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
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(products);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Alpha", "Beta" });
        await _productRepoMock.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsAsync_WhenRepositoryConfirmsExistence_ReturnsTrue()
    {
        // Arrange
        _productRepoMock
            .ExistsAsync(42, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var exists = await _service.ExistsAsync(42);

        // Assert
        exists.Should().BeTrue();
        await _productRepoMock.Received(1).ExistsAsync(42, Arg.Any<CancellationToken>());
    }
}

public sealed class EventBusTests
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
    public async Task ClearEventHistory_AfterPublishingEvents_LeavesHistoryEmpty()
    {
        // Arrange
        var bus = new EventBus();
        await bus.PublishAsync(new StockDepletedEvent { ProductId = 1, Remaining = 5 });

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
