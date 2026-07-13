#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Data.Sqlite;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using FluentAssertions;
using FluentAssertions.Primitives; // Added
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Provides integration tests for repository operations against an in-memory SQLite database.
/// </summary>
public sealed class RepositoryIntegrationTests : IDisposable
{
    private DatabaseConnection _databaseConnection;
    private ConcreteProductRepository _productRepository;
    private ConcreteUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryIntegrationTests"/> class,
    /// setting up an in-memory database and repositories for products and users.
    /// </summary>
    public RepositoryIntegrationTests()
    {
        _databaseConnection = new DatabaseConnection("Data Source=:memory:");
        _productRepository = new ConcreteProductRepository(_databaseConnection);
        _userRepository = new ConcreteUserRepository(_databaseConnection);
        _databaseConnection.InitializeDatabaseAsync(true).GetAwaiter().GetResult();
        SeedCategories();
    }

    /// <summary>
    /// Seeds the categories referenced by product fixtures, since Products.CategoryId
    /// has a foreign key constraint against Categories(Id).
    /// </summary>
    private void SeedCategories()
    {
        using var command = _databaseConnection.Connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Categories (Id, Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES
                (1, 'Category 1', 0, 1, datetime('now'), datetime('now')),
                (2, 'Category 2', 0, 1, datetime('now'), datetime('now')),
                (3, 'Category 3', 0, 1, datetime('now'), datetime('now'));";
        command.ExecuteNonQuery();
    }

    private void CreateTables()
    {
        // Tables are now created by DatabaseConnection.InitializeDatabaseAsync()
        // This method can be empty or removed.
    }

    /// <summary>
    /// Disposes the in-memory database connection.
    /// </summary>
    public void Dispose()
    {
        _databaseConnection.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Verifies that adding a product persists it in the database and that the returned entity contains a generated Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Sku = "TP-001",
            CategoryId = 1,
            Price = 10.00m,
            Cost = 5.00m,
            StockQuantity = 100,
            ReorderLevel = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var addedProduct = await _productRepository.AddAsync(product);

        // Assert
        addedProduct.Should().NotBeNull();
        addedProduct.Id.Should().BeGreaterThan(0);
        var retrievedProduct = await _productRepository.GetByIdAsync(addedProduct.Id);
        retrievedProduct.Should().BeEquivalentTo(addedProduct, options => options.Excluding(p => p.CreatedAt).Excluding(p => p.UpdatedAt));
    }

    /// <summary>
    /// Verifies that retrieving a product by its Id returns the correct entity.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_RetrievesCorrectProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "Another Product",
            Sku = "AP-002",
            CategoryId = 2,
            Price = 20.00m,
            Cost = 10.00m,
            StockQuantity = 50,
            ReorderLevel = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var addedProduct = await _productRepository.AddAsync(product);

        // Act
        var retrievedProduct = await _productRepository.GetByIdAsync(addedProduct.Id);

        // Assert
        retrievedProduct.Should().NotBeNull();
        retrievedProduct.Id.Should().Be(addedProduct.Id);
        retrievedProduct.Name.Should().Be("Another Product");
    }

    /// <summary>
    /// Verifies that updating a product modifies its fields in the database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_UpdatesProductInDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Old Name",
            Sku = "ON-003",
            CategoryId = 1,
            Price = 15.00m,
            Cost = 7.50m,
            StockQuantity = 75,
            ReorderLevel = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var addedProduct = await _productRepository.AddAsync(product);
        addedProduct.Name = "New Name";
        addedProduct.Price = 25.00m;

        // Act
        var result = await _productRepository.UpdateAsync(addedProduct);

        // Assert
        result.Should().BeTrue();
        var updatedProduct = await _productRepository.GetByIdAsync(addedProduct.Id);
        updatedProduct.Name.Should().Be("New Name");
        updatedProduct.Price.Should().Be(25.00m);
    }

    /// <summary>
    /// Verifies that deleting a product removes it from the database.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesProductFromDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product to Delete",
            Sku = "PD-004",
            CategoryId = 3,
            Price = 50.00m,
            Cost = 25.00m,
            StockQuantity = 20,
            ReorderLevel = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var addedProduct = await _productRepository.AddAsync(product);

        // Act
        var result = await _productRepository.DeleteAsync(addedProduct.Id);

        // Assert
        result.Should().BeTrue();
        var retrievedProduct = await _productRepository.GetByIdAsync(addedProduct.Id);
        retrievedProduct.Should().BeNull();
    }

    /// <summary>
    /// Verifies that retrieving all products returns the correct number of entities.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        await _productRepository.AddAsync(new Product { Name = "P1", Sku = "S1", CategoryId = 1, Price = 1, Cost = 1, StockQuantity = 1, ReorderLevel = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _productRepository.AddAsync(new Product { Name = "P2", Sku = "S2", CategoryId = 1, Price = 1, Cost = 1, StockQuantity = 1, ReorderLevel = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });

        // Act
        var products = await _productRepository.GetAllAsync();

        // Assert
        products.Should().HaveCount(2);
    }
}
