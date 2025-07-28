#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Extension methods for <see cref="AuditTrailServiceTests"/> providing common test utilities.
/// </summary>
public static class AuditTrailServiceTestsExtensions
{
    /// <summary>
    /// Creates a product entity with test data.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="name">The product name.</param>
    /// <returns>A product instance populated with test data.</returns>
    public static Product CreateTestProduct(this int id, string name = "Test Product")
    {
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
    public static Order CreateTestOrder(this int id)
    {
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
    public static Category CreateTestCategory(this int id, string name = "Test Category")
    {
        return new Category
        {
            Id = id,
            Name = name,
            Description = $"Test category with ID {id}",
            IsActive = true
        };
    }
}