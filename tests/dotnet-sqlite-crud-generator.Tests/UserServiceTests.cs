// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using FluentAssertions.Primitives; // Added
using NSubstitute;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class UserServiceTests
{
    private readonly IRepository<User, int> _userRepoMock = Substitute.For<IRepository<User, int>>();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(_userRepoMock);
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsUserFromRepository()
    {
        // Arrange
        var expectedUser = new User { Id = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "passwordhash", FirstName = "Test", LastName = "User" };
        _userRepoMock.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(expectedUser));

        // Act
        var result = await _userService.GetAsync(1);

        // Assert
        result.Should().Be(expectedUser);
        await _userRepoMock.Received(1).GetByIdAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _userRepoMock.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(Task.FromResult((User)null!)); // Fix NSubstitute return for async

        // Act
        var result = await _userService.GetAsync(99);

        // Assert
        result.Should().BeNull();
        await _userRepoMock.Received(1).GetByIdAsync(99, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsersFromRepository()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "user1@example.com", PasswordHash = "hash1", FirstName = "User", LastName = "One" },
            new() { Id = 2, Username = "user2", Email = "user2@example.com", PasswordHash = "hash2", FirstName = "User", LastName = "Two" }
        };
        _userRepoMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(users.AsEnumerable()));

        // Act
        var result = await _userService.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(users);
        await _userRepoMock.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_AddsUserThroughRepository()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Email = "new@example.com", PasswordHash = "newpasswordhash", FirstName = "New", LastName = "User" };
        _userRepoMock.AddAsync(newUser, Arg.Any<CancellationToken>()).Returns(Task.FromResult(newUser));

        // Act
        var result = await _userService.CreateAsync(newUser);

        // Assert
        result.Should().Be(newUser);
        await _userRepoMock.Received(1).AddAsync(newUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUserThroughRepository()
    {
        // Arrange
        var existingUser = new User { Id = 1, Username = "oldname", Email = "old@example.com", PasswordHash = "oldhash", FirstName = "Old", LastName = "User" };
        var updatedUser = new User { Id = 1, Username = "newname", Email = "old@example.com", PasswordHash = "oldhash", FirstName = "Old", LastName = "User" };
        _userRepoMock.UpdateAsync(updatedUser, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        // Act
        var result = await _userService.UpdateAsync(updatedUser);

        // Assert
        result.Should().BeTrue();
        await _userRepoMock.Received(1).UpdateAsync(updatedUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DeletesUserThroughRepository()
    {
        // Arrange
        _userRepoMock.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        // Act
        var result = await _userService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        await _userRepoMock.Received(1).DeleteAsync(1, Arg.Any<CancellationToken>());
    }
}
