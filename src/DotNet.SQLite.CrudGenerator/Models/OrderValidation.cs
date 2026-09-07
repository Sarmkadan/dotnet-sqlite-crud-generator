#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Enums;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Provides validation helpers for the <see cref="Order"/> entity using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class OrderValidation
{
    /// <summary>
    /// Validates an order and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static ValidationResult ValidateAll(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate required fields
        if (value.UserId <= 0)
        {
            result = result.WithProblem(nameof(value.UserId), "User ID must be a positive integer.");
        }

        if (string.IsNullOrWhiteSpace(value.OrderNumber))
        {
            result = result.WithProblem(nameof(value.OrderNumber), "Order number is required and cannot be empty or whitespace.");
        }
        else if (value.OrderNumber.Length > 50)
        {
            result = result.WithProblem(nameof(value.OrderNumber), "Order number must not exceed 50 characters.");
        }

        // Validate monetary values
        if (value.TotalAmount <= 0)
        {
            result = result.WithProblem(nameof(value.TotalAmount), "Total amount must be greater than 0.");
        }

        if (value.TaxAmount < 0)
        {
            result = result.WithProblem(nameof(value.TaxAmount), "Tax amount cannot be negative.");
        }

        if (value.DiscountAmount < 0)
        {
            result = result.WithProblem(nameof(value.DiscountAmount), "Discount amount cannot be negative.");
        }

        // Validate item count
        if (value.ItemCount <= 0)
        {
            result = result.WithProblem(nameof(value.ItemCount), "Item count must be a positive integer.");
        }

        // Validate timestamps
        if (value.CreatedAt == default)
        {
            result = result.WithProblem(nameof(value.CreatedAt), "Created date must be set.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.CreatedAt), "Created date cannot be in the future.");
        }

        if (value.UpdatedAt == default)
        {
            result = result.WithProblem(nameof(value.UpdatedAt), "Updated date must be set.");
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.UpdatedAt), "Updated date cannot be in the future.");
        }

        if (value.CreatedAt > value.UpdatedAt)
        {
            result = result.WithProblem(nameof(value.UpdatedAt), "Updated date cannot be earlier than created date.");
        }

        // Validate status-specific timestamp rules using pattern matching for clarity
        if (value.Status is EntityStatus.Shipped)
        {
            if (value.ShippedAt == null)
            {
                result = result.WithProblem(nameof(value.ShippedAt), "Shipped status requires a shipped date.");
            }
            else if (value.ShippedAt > DateTime.UtcNow.AddMinutes(5))
            {
                result = result.WithProblem(nameof(value.ShippedAt), "Shipped date cannot be in the future.");
            }
            else if (value.ShippedAt < value.CreatedAt)
            {
                result = result.WithProblem(nameof(value.ShippedAt), "Shipped date cannot be earlier than created date.");
            }
        }

        if (value.Status is EntityStatus.Delivered)
        {
            if (value.DeliveredAt == null)
            {
                result = result.WithProblem(nameof(value.DeliveredAt), "Delivered status requires a delivered date.");
            }
            else if (value.DeliveredAt > DateTime.UtcNow.AddMinutes(5))
            {
                result = result.WithProblem(nameof(value.DeliveredAt), "Delivered date cannot be in the future.");
            }
            else if (value.DeliveredAt < value.CreatedAt)
            {
                result = result.WithProblem(nameof(value.DeliveredAt), "Delivered date cannot be earlier than created date.");
            }
        }

        // Validate address fields when needed
        if ((value.Status is EntityStatus.Shipped or EntityStatus.Delivered) &&
            string.IsNullOrWhiteSpace(value.ShippingAddress))
        {
            result = result.WithProblem(nameof(value.ShippingAddress), "Shipping address is required for shipped or delivered orders.");
        }

        if ((value.Status is EntityStatus.Shipped or EntityStatus.Delivered) &&
            string.IsNullOrWhiteSpace(value.BillingAddress))
        {
            result = result.WithProblem(nameof(value.BillingAddress), "Billing address is required for shipped or delivered orders.");
        }

        // Validate notes length
        if (!string.IsNullOrEmpty(value.Notes) && value.Notes.Length > 500)
        {
            result = result.WithProblem(nameof(value.Notes), "Notes must not exceed 500 characters.");
        }

        // Validate address lengths
        if (!string.IsNullOrEmpty(value.ShippingAddress) && value.ShippingAddress.Length > 255)
        {
            result = result.WithProblem(nameof(value.ShippingAddress), "Shipping address must not exceed 255 characters.");
        }

        if (!string.IsNullOrEmpty(value.BillingAddress) && value.BillingAddress.Length > 255)
        {
            result = result.WithProblem(nameof(value.BillingAddress), "Billing address must not exceed 255 characters.");
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified order is valid.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns>True if the order is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Order value)
        => value is not null && ValidateAll(value).IsValid;

    /// <summary>
    /// Ensures that the specified order is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the order is invalid.</exception>
    public static void EnsureValid(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateAll(value).ThrowIfInvalid(problems => new ValidationException(
            $"Order validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"- {p.Message}"))}"));
    }

    /// <summary>
    /// Validates that the order's final total calculation is consistent with its components.
    /// </summary>
    /// <param name="value">The order to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static ValidationResult ValidateFinalTotal(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        var calculatedTotal = value.CalculateFinalTotal();
        var expectedTotal = Math.Max(0, value.TotalAmount - value.TaxAmount + value.DiscountAmount);

        if (Math.Abs(calculatedTotal - expectedTotal) > 0.01m)
        {
            result = result.WithProblem("finalTotalCalculation",
                $"Final total calculation mismatch. Calculated: {calculatedTotal:C}, " +
                $"Expected: {expectedTotal:C} (Total: {value.TotalAmount:C}, Tax: {value.TaxAmount:C}, Discount: {value.DiscountAmount:C}).");
        }

        return result;
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
        ValidateFinalTotal(value).ThrowIfInvalid(problems => new ValidationException(
            $"Order final total validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"- {p.Message}"))}"));
    }

    /// <summary>
    /// Validates that an order can be shipped based on its current state.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static ValidationResult ValidateCanShip(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        if (value.Status != EntityStatus.Pending)
        {
            result = result.WithProblem(nameof(value.Status), "Only pending orders can be shipped.");
        }

        if (string.IsNullOrWhiteSpace(value.ShippingAddress))
        {
            result = result.WithProblem(nameof(value.ShippingAddress), "Shipping address is required to ship an order.");
        }

        if (value.ItemCount <= 0)
        {
            result = result.WithProblem(nameof(value.ItemCount), "Order must contain items to be shipped.");
        }

        if (value.TotalAmount <= 0)
        {
            result = result.WithProblem(nameof(value.TotalAmount), "Order must have a positive total amount to be shipped.");
        }

        return result;
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
        ValidateCanShip(value).ThrowIfInvalid(problems => new ValidationException(
            $"Order cannot be shipped:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"- {p.Message}"))}"));
    }

    /// <summary>
    /// Validates that an order can be cancelled based on its current state.
    /// </summary>
    /// <param name="value">The order to check.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static ValidationResult ValidateCanCancel(this Order value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        if (value.Status != EntityStatus.Pending)
        {
            result = result.WithProblem(nameof(value.Status), "Only pending orders can be cancelled.");
        }

        return result;
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
        ValidateCanCancel(value).ThrowIfInvalid(problems => new ValidationException(
            $"Order cannot be cancelled:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $"- {p.Message}"))}"));
    }
}