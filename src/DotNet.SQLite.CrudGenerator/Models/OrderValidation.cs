#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Enums;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Provides validation helpers for the <see cref="Order"/> entity.
/// </summary>
public static class OrderValidation
{
    /// <summary>
    /// Validates an order and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the order is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateAll(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required fields
        if (value.UserId <= 0)
            errors.Add("User ID must be a positive integer.");

        if (string.IsNullOrWhiteSpace(value.OrderNumber))
            errors.Add("Order number is required and cannot be empty or whitespace.");
        else if (value.OrderNumber.Length > 50)
            errors.Add("Order number must not exceed 50 characters.");

        // Validate monetary values
        if (value.TotalAmount <= 0)
            errors.Add("Total amount must be greater than 0.");

        if (value.TaxAmount < 0)
            errors.Add("Tax amount cannot be negative.");

        if (value.DiscountAmount < 0)
            errors.Add("Discount amount cannot be negative.");

        // Validate item count
        if (value.ItemCount <= 0)
            errors.Add("Item count must be a positive integer.");

        // Validate timestamps
        if (value.CreatedAt == default)
            errors.Add("Created date must be set.");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Created date cannot be in the future.");

        if (value.UpdatedAt == default)
            errors.Add("Updated date must be set.");
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Updated date cannot be in the future.");

        if (value.CreatedAt > value.UpdatedAt)
            errors.Add("Updated date cannot be earlier than created date.");

        // Validate status-specific timestamp rules using pattern matching for clarity
        if (value.Status is EntityStatus.Shipped)
        {
            if (value.ShippedAt == null)
                errors.Add("Shipped status requires a shipped date.");
            else if (value.ShippedAt > DateTime.UtcNow.AddMinutes(5))
                errors.Add("Shipped date cannot be in the future.");
            else if (value.ShippedAt < value.CreatedAt)
                errors.Add("Shipped date cannot be earlier than created date.");
        }

        if (value.Status is EntityStatus.Delivered)
        {
            if (value.DeliveredAt == null)
                errors.Add("Delivered status requires a delivered date.");
            else if (value.DeliveredAt > DateTime.UtcNow.AddMinutes(5))
                errors.Add("Delivered date cannot be in the future.");
            else if (value.DeliveredAt < value.CreatedAt)
                errors.Add("Delivered date cannot be earlier than created date.");
        }

        // Validate address fields when needed
        if ((value.Status is EntityStatus.Shipped or EntityStatus.Delivered) &&
            string.IsNullOrWhiteSpace(value.ShippingAddress))
            errors.Add("Shipping address is required for shipped or delivered orders.");

        if ((value.Status is EntityStatus.Shipped or EntityStatus.Delivered) &&
            string.IsNullOrWhiteSpace(value.BillingAddress))
            errors.Add("Billing address is required for shipped or delivered orders.");

        // Validate notes length
        if (!string.IsNullOrEmpty(value.Notes) && value.Notes.Length > 500)
            errors.Add("Notes must not exceed 500 characters.");

        // Validate address lengths
        if (!string.IsNullOrEmpty(value.ShippingAddress) && value.ShippingAddress.Length > 255)
            errors.Add("Shipping address must not exceed 255 characters.");

        if (!string.IsNullOrEmpty(value.BillingAddress) && value.BillingAddress.Length > 255)
            errors.Add("Billing address must not exceed 255 characters.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified order is valid.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns><see langword="true"/> if the order is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ValidateAll().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified order is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the order is invalid, containing the validation errors.</exception>
    public static void EnsureValid(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateAll();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Order validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates that the order's final total calculation is consistent with its components.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <returns>A list of validation messages, or empty if the calculation is consistent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateFinalTotal(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        var calculatedTotal = value.CalculateFinalTotal();
        var expectedTotal = Math.Max(0, value.TotalAmount - value.TaxAmount + value.DiscountAmount);

        if (Math.Abs(calculatedTotal - expectedTotal) > 0.01m)
        {
            errors.Add(
                $"Final total calculation mismatch. Calculated: {calculatedTotal:C}, " +
                $"Expected: {expectedTotal:C} (Total: {value.TotalAmount:C}, Tax: {value.TaxAmount:C}, Discount: {value.DiscountAmount:C}).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Ensures that the order's final total calculation is consistent, throwing if not.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the final total calculation is inconsistent.</exception>
    public static void EnsureFinalTotalValid(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateFinalTotal();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Order final total validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates that an order can be shipped based on its current state.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns>A list of validation messages, or empty if the order can be shipped.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateCanShip(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.Status != EntityStatus.Pending)
            errors.Add("Only pending orders can be shipped.");

        if (string.IsNullOrWhiteSpace(value.ShippingAddress))
            errors.Add("Shipping address is required to ship an order.");

        if (value.ItemCount <= 0)
            errors.Add("Order must contain items to be shipped.");

        if (value.TotalAmount <= 0)
            errors.Add("Order must have a positive total amount to be shipped.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Ensures that an order can be shipped, throwing if it cannot.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the order cannot be shipped.</exception>
    public static void EnsureCanShip(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateCanShip();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Order cannot be shipped:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates that an order can be cancelled based on its current state.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns>A list of validation messages, or empty if the order can be cancelled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> ValidateCanCancel(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.Status != EntityStatus.Pending)
            errors.Add("Only pending orders can be cancelled.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Ensures that an order can be cancelled, throwing if it cannot.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the order cannot be cancelled.</exception>
    public static void EnsureCanCancel(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateCanCancel();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Order cannot be cancelled:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}