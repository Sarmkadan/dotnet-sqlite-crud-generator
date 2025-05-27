// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Represents a product entity with inventory and pricing information.
/// </summary>
public class Product
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 255 characters")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(100, ErrorMessage = "SKU must not exceed 100 characters")]
    [JsonPropertyName("sku")]
    public required string Sku { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be non-negative")]
    [JsonPropertyName("reorderLevel")]
    public int ReorderLevel { get; set; } = 10;

    [StringLength(50, ErrorMessage = "Unit must not exceed 50 characters")]
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "piece";

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the product model for consistency.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Sku))
            return false;

        if (Price <= 0 || Cost < 0)
            return false;

        if (StockQuantity < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates the profit margin percentage for the product.
    /// </summary>
    public decimal GetProfitMarginPercentage()
    {
        if (Cost == 0) return 0;
        return ((Price - Cost) / Cost) * 100;
    }

    /// <summary>
    /// Determines if the product stock is below reorder level.
    /// </summary>
    public bool IsLowStock() => StockQuantity <= ReorderLevel;

    /// <summary>
    /// Adds quantity to the product stock.
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative");

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes quantity from the product stock.
    /// </summary>
    public void RemoveStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative");

        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the product.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
