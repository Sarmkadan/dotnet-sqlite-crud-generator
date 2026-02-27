// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DotNet.SQLite.CrudGenerator.Enums;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Represents an order entity with items and transaction details.
/// </summary>
public class Order
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Order number is required")]
    [StringLength(50, ErrorMessage = "Order number must not exceed 50 characters")]
    [JsonPropertyName("orderNumber")]
    public required string OrderNumber { get; set; }

    [JsonPropertyName("status")]
    public EntityStatus Status { get; set; } = EntityStatus.Pending;

    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Tax amount must be non-negative")]
    [JsonPropertyName("taxAmount")]
    public decimal TaxAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
    [JsonPropertyName("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [StringLength(500, ErrorMessage = "Notes must not exceed 500 characters")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [StringLength(255, ErrorMessage = "Shipping address must not exceed 255 characters")]
    [JsonPropertyName("shippingAddress")]
    public string? ShippingAddress { get; set; }

    [StringLength(255, ErrorMessage = "Billing address must not exceed 255 characters")]
    [JsonPropertyName("billingAddress")]
    public string? BillingAddress { get; set; }

    [JsonPropertyName("itemCount")]
    public int ItemCount { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("shippedAt")]
    public DateTime? ShippedAt { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Validates the order model for consistency.
    /// </summary>
    public bool Validate()
    {
        if (UserId <= 0 || string.IsNullOrWhiteSpace(OrderNumber))
            return false;

        if (TotalAmount <= 0 || TaxAmount < 0 || DiscountAmount < 0)
            return false;

        if (ItemCount <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates the final order total including tax and discount.
    /// </summary>
    public decimal CalculateFinalTotal()
    {
        var subtotal = TotalAmount - TaxAmount + DiscountAmount;
        return subtotal > 0 ? subtotal : 0;
    }

    /// <summary>
    /// Updates the order status and records the timestamp.
    /// </summary>
    public void UpdateStatus(EntityStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == EntityStatus.Shipped)
            ShippedAt = DateTime.UtcNow;
        else if (newStatus == EntityStatus.Delivered)
            DeliveredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies a discount to the order.
    /// </summary>
    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative");

        DiscountAmount = discountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the order if it's still pending.
    /// </summary>
    public bool CancelOrder()
    {
        if (Status != EntityStatus.Pending)
            return false;

        Status = EntityStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Checks if the order can be shipped.
    /// </summary>
    public bool CanShip() => Status == EntityStatus.Pending && !string.IsNullOrWhiteSpace(ShippingAddress);
}
