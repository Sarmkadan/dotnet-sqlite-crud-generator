#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Extension methods for <see cref="AuditTrailServiceTests"/> providing common test utilities.
/// </summary>
/// <remarks>
/// These extension methods simplify the creation of test entities for audit trail testing scenarios.
/// All methods validate their input parameters and throw appropriate exceptions for invalid values.
/// </remarks>
public static class AuditTrailServiceTestsExtensions
{
    /// <summary>
    /// Creates a product entity with test data.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="name">The product name.</param>
    /// <returns>A product instance populated with test data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static Product CreateTestProduct(this int id, string name = "Test Product")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Product
        {
            Id = id,
            Name = name,
            Sku = $"SKU{id:D6}",
            CategoryId = 1,
            Price = 100m,
            Cost = 50m,
            StockQuantity = 100,
            ReorderLevel = 10,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates an order entity with test data.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>An order instance populated with test data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than 1.</exception>
    public static Order CreateTestOrder(this int id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);

        return new Order
        {
            Id = id,
            UserId = 1,
            OrderNumber = $"ORD-{id:D6}",
            Status = global::DotNet.SQLite.CrudGenerator.Enums.EntityStatus.Pending,
            TotalAmount = 250m,
            TaxAmount = 25m,
            DiscountAmount = 0m,
            ItemCount = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a category entity with test data.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="name">The category name.</param>
    /// <returns>A category instance populated with test data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="id"/> is less than 1.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static Category CreateTestCategory(this int id, string name = "Test Category")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category
        {
            Id = id,
            Name = name,
            Description = $"Test category with ID {id}",
            IsActive = true
        };
    }
}