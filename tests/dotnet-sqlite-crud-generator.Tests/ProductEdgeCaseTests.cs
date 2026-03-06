// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Models;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class ProductEdgeCaseTests
{
    [Fact]
    public void Validate_WithNullName_ReturnsFalse()
    {
        // Arrange
        var product = new Product
        {
            Name = null,
            Sku = "SKU123",
            CategoryId = 1,
            Price = 10.00m,
            Cost = 5.00m,
            StockQuantity = 10,
            ReorderLevel = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithEmptySku_ReturnsFalse()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Sku = "",
            CategoryId = 1,
            Price = 10.00m,
            Cost = 5.00m,
            StockQuantity = 10,
            ReorderLevel = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNegativePrice_ReturnsFalse()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Sku = "SKU123",
            CategoryId = 1,
            Price = -1.00m,
            Cost = 5.00m,
            StockQuantity = 10,
            ReorderLevel = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithZeroCategoryId_ReturnsFalse()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Sku = "SKU123",
            CategoryId = 0,
            Price = 10.00m,
            Cost = 5.00m,
            StockQuantity = 10,
            ReorderLevel = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeFalse();
    }
}
