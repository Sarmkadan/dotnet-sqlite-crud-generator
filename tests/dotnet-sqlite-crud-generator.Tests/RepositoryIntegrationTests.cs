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

public sealed class RepositoryIntegrationTests : IDisposable
{
    private DatabaseConnection _databaseConnection;
    private ConcreteProductRepository _productRepository;
    private ConcreteUserRepository _userRepository;

    public RepositoryIntegrationTests()
    {
        _databaseConnection = new DatabaseConnection("Data Source=:memory:");
        _productRepository = new ConcreteProductRepository(_databaseConnection);
        _userRepository = new ConcreteUserRepository(_databaseConnection);
        _databaseConnection.InitializeDatabaseAsync(true).GetAwaiter().GetResult();
    }

    private void CreateTables()
    {
        // Tables are now created by DatabaseConnection.InitializeDatabaseAsync()
        // This method can be empty or removed.
    }

    public void Dispose()
    {
        _databaseConnection.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

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
