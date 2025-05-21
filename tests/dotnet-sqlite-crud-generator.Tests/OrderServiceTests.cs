// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using FluentAssertions.Primitives;
using NSubstitute;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class OrderServiceTests
{
    private readonly IRepository<Order, int> _orderRepoMock = Substitute.For<IRepository<Order, int>>();
    private readonly IRepository<User, int> _userRepoMock = Substitute.For<IRepository<User, int>>();
    private readonly IRepository<AuditLog, int> _auditLogRepoMock = Substitute.For<IRepository<AuditLog, int>>();
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderService = new OrderService(_orderRepoMock, _userRepoMock, _auditLogRepoMock);
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsOrderFromRepository()
    {
        // Arrange
        var expectedOrder = new Order { Id = 1, UserId = 1, TotalAmount = 100m, OrderNumber = "ORD-001" };
        _orderRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedOrder));

        // Act
        var result = await _orderService.GetAsync(1);

        // Assert
        result.Should().Be(expectedOrder);
        await _orderRepoMock.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrdersFromRepository()
    {
        // Arrange
        var orders = new List<Order>
        {
            new() { Id = 1, UserId = 1, TotalAmount = 100m, OrderNumber = "ORD-001" },
            new() { Id = 2, UserId = 2, TotalAmount = 200m, OrderNumber = "ORD-002" }
        };
        _orderRepoMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(orders.AsEnumerable()));

        // Act
        var result = await _orderService.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(orders);
        await _orderRepoMock.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_AddsOrderThroughRepositoryAndLogsAudit()
    {
        // Arrange
        var newOrder = new Order { UserId = 1, TotalAmount = 150m, OrderNumber = "ORD-003" };
        var user = new User { Id = 1, Username = "testuser", Email = "user@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };
        _userRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(user));
        _orderRepoMock.AddAsync(newOrder, Arg.Any<CancellationToken>()).Returns(Task.FromResult(newOrder));
        _auditLogRepoMock.AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AuditLog { EntityType = "Order", EntityId = 1, ChangedByUserId = 1 }));

        // Act
        var result = await _orderService.CreateAsync(newOrder);

        // Assert
        result.Should().Be(newOrder);
        await _orderRepoMock.Received(1).AddAsync(newOrder, Arg.Any<CancellationToken>());
        await _auditLogRepoMock.Received(1).AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesOrderThroughRepositoryAndLogsAudit()
    {
        // Arrange
        var existingOrder = new Order { Id = 1, UserId = 1, TotalAmount = 100m, OrderNumber = "ORD-001" };
        var updatedOrder = new Order { Id = 1, UserId = 1, TotalAmount = 200m, OrderNumber = "ORD-001" };
        var user = new User { Id = 1, Username = "testuser", Email = "user@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };

        _orderRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(existingOrder));
        _userRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(user));
        _orderRepoMock.UpdateAsync(updatedOrder, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _auditLogRepoMock.AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AuditLog { EntityType = "Order", EntityId = 1, ChangedByUserId = 1 }));

        // Act
        var result = await _orderService.UpdateAsync(updatedOrder);

        // Assert
        result.Should().BeTrue();
        await _orderRepoMock.Received(1).UpdateAsync(updatedOrder, Arg.Any<CancellationToken>());
        await _auditLogRepoMock.Received(1).AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DeletesOrderThroughRepositoryAndLogsAudit()
    {
        // Arrange
        var existingOrder = new Order { Id = 1, UserId = 1, TotalAmount = 100m, OrderNumber = "ORD-001" };
        var user = new User { Id = 1, Username = "testuser", Email = "user@example.com", PasswordHash = "hash", FirstName = "Test", LastName = "User" };

        _orderRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(existingOrder));
        _userRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(user));
        _orderRepoMock.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _auditLogRepoMock.AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AuditLog { EntityType = "Order", EntityId = 1, ChangedByUserId = 1 }));

        // Act
        var result = await _orderService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        await _orderRepoMock.Received(1).DeleteAsync(1, Arg.Any<CancellationToken>());
        await _auditLogRepoMock.Received(1).AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentUser_ThrowsArgumentException()
    {
        // Arrange
        var newOrder = new Order { UserId = 99, TotalAmount = 150m, OrderNumber = "ORD-004" };
        _userRepoMock.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(Task.FromResult((User)null!));

        // Act
        var act = async () => await _orderService.CreateAsync(newOrder);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*User with ID 99 not found*");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentOrder_ReturnsFalse()
    {
        // Arrange
        var updatedOrder = new Order { Id = 99, UserId = 1, TotalAmount = 200m, OrderNumber = "ORD-005" };
        _orderRepoMock.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(Task.FromResult((Order)null!));

        // Act
        var result = await _orderService.UpdateAsync(updatedOrder);

        // Assert
        result.Should().BeFalse();
        await _orderRepoMock.Received(1).GetByIdAsync(99, Arg.Any<CancellationToken>());
        await _orderRepoMock.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
