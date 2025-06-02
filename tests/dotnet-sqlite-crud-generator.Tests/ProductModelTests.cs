// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class ProductModelTests
{
    private static Product CreateValidProduct() => new()
    {
        Name = "Test Widget",
        Sku = "TW-001",
        CategoryId = 1,
        Price = 20.00m,
        Cost = 10.00m,
        StockQuantity = 50,
        ReorderLevel = 10
    };

    [Fact]
    public void Validate_WithAllRequiredFieldsPopulated_ReturnsTrue()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenPriceIsZero_ReturnsFalse()
    {
        // Arrange
        var product = CreateValidProduct();
        product.Price = 0m;

        // Act
        var isValid = product.Validate();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GetProfitMarginPercentage_WithKnownCostAndPrice_ReturnsExpectedMarginPercent()
    {
        // Arrange — cost: $10, price: $15 => margin = (15 - 10) / 10 * 100 = 50 %
        var product = CreateValidProduct();
        product.Price = 15.00m;
        product.Cost = 10.00m;

        // Act
        var margin = product.GetProfitMarginPercentage();

        // Assert
        margin.Should().Be(50m);
    }

    [Fact]
    public void IsLowStock_WhenStockEqualsReorderLevel_ReturnsTrue()
    {
        // Arrange
        var product = CreateValidProduct();
        product.StockQuantity = product.ReorderLevel; // exactly at threshold

        // Act
        var isLow = product.IsLowStock();

        // Assert
        isLow.Should().BeTrue();
    }

    [Fact]
    public void AddStock_WithNegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        var act = () => product.AddStock(-5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void RemoveStock_WhenQuantityExceedsAvailableStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = CreateValidProduct(); // StockQuantity = 50

        // Act
        var act = () => product.RemoveStock(100);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock. Available: 50, Requested: 100");
    }
}

public class DateTimeExtensionsTests
{
    [Fact]
    public void BeginningOfDay_ForAnyDateTime_ReturnsMidnightOnTheSameDate()
    {
        // Arrange
        var dateTime = new DateTime(2024, 6, 15, 14, 30, 45);

        // Act
        var result = dateTime.BeginningOfDay();

        // Assert
        result.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
        result.TimeOfDay.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void IsBetween_WhenDateFallsWithinRange_ReturnsTrue()
    {
        // Arrange
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);
        var midYear = new DateTime(2024, 6, 15);

        // Act
        var result = midYear.IsBetween(start, end);

        // Assert
        result.Should().BeTrue();
    }
}
