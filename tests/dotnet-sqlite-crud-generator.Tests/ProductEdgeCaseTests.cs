#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Models;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Contains unit tests that verify the <see cref="Product.Validate"/> method
/// correctly handles various edge cases such as null or empty properties,
/// negative values, and invalid identifiers.
/// </summary>
public sealed class ProductEdgeCaseTests
{
    /// <summary>
    /// Verifies that <see cref="Product.Validate"/> returns <c>false</c> when the
    /// <c>Name</c> property is <c>null</c>.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="Product.Validate"/> returns <c>false</c> when the
    /// <c>Sku</c> property is an empty string.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="Product.Validate"/> returns <c>false</c> when the
    /// <c>Price</c> property is negative.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="Product.Validate"/> returns <c>false</c> when the
    /// <c>CategoryId</c> property is zero.
    /// </summary>
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
