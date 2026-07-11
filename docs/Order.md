# Order

Represents a customer order within the system, tracking line items, financials, fulfillment status, and lifecycle timestamps. The type encapsulates core order validation, total calculation, status transitions, discount application, and cancellation logic.

## API

### public int Id

Unique identifier for the order record. Assigned by the persistence layer upon insertion.

### public int UserId

Foreign key referencing the user who placed the order.

### public required string OrderNumber

A required, human-readable order reference string. Must be supplied at creation time; null or empty values are not accepted.

### public EntityStatus Status

Current state of the order in the fulfillment pipeline. The underlying `EntityStatus` enumeration defines the valid states (e.g., Pending, Confirmed, Shipped, Delivered, Cancelled).

### public decimal TotalAmount

Gross monetary sum of all line items before tax and discounts.

### public decimal TaxAmount

Total tax applied to the order.

### public decimal DiscountAmount

Aggregate discount applied to the order, expressed as a positive value subtracted from the total.

### public string? Notes

Optional free-text notes associated with the order. Nullable.

### public string? ShippingAddress

Optional shipping destination address. Nullable.

### public string? BillingAddress

Optional billing address. Nullable.

### public int ItemCount

Number of distinct line items (or total quantity, depending on domain interpretation) contained in the order.

### public DateTime CreatedAt

Timestamp recording when the order was initially created. Set at insertion time.

### public DateTime UpdatedAt

Timestamp recording the most recent modification to the order record. Updated on every state mutation.

### public DateTime? ShippedAt

Timestamp recording when the order transitioned into a shipped state. Null if the order has not yet shipped.

### public DateTime? DeliveredAt

Timestamp recording when the order transitioned into a delivered state. Null if delivery has not been confirmed.

### public bool Validate()

Validates the integrity and business-rule compliance of the order. Returns `true` if the order passes all validation checks; otherwise `false`. Typical checks include verifying `OrderNumber` is present, amounts are non-negative, `ItemCount` is positive, and the current status is consistent with populated timestamp fields. Does not throw.

### public decimal CalculateFinalTotal()

Computes the net payable amount by summing `TotalAmount` and `TaxAmount`, then subtracting `DiscountAmount`. Returns the resulting decimal value. Does not throw.

### public void UpdateStatus(EntityStatus newStatus)

Transitions the order to the specified `newStatus`. Side effects include updating `UpdatedAt` and conditionally setting `ShippedAt` or `DeliveredAt` when the corresponding statuses are applied. Throws an `InvalidOperationException` if the requested transition is not permitted from the current status (e.g., moving from Delivered back to Pending, or Cancelling an already Shipped order depending on business rules).

### public void ApplyDiscount(decimal discountAmount)

Applies a discount to the order by setting `DiscountAmount` to the provided value and updating `UpdatedAt`. Throws an `ArgumentOutOfRangeException` if `discountAmount` is negative or exceeds the current `TotalAmount` plus `TaxAmount`, preventing the final total from dropping below zero.

### public bool CancelOrder()

Attempts to cancel the order. Returns `true` if cancellation succeeds (status set to Cancelled, `UpdatedAt` refreshed); returns `false` if the order is already in a terminal state (e.g., Delivered, already Cancelled) where cancellation is not allowed. Does not throw.

## Usage

### Creating, validating, and finalizing an order

```csharp
var order = new Order
{
    UserId = 42,
    OrderNumber = "ORD-2025-001",
    Status = EntityStatus.Pending,
    TotalAmount = 150.00m,
    TaxAmount = 12.00m,
    DiscountAmount = 0.00m,
    ItemCount = 3,
    ShippingAddress = "123 Main St.",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

if (!order.Validate())
{
    Console.WriteLine("Order validation failed — check required fields and amounts.");
    return;
}

decimal final = order.CalculateFinalTotal();
Console.WriteLine($"Final total: {final:C}");

order.UpdateStatus(EntityStatus.Confirmed);
```

### Applying a discount and cancelling

```csharp
var order = existingOrder; // retrieved from persistence

try
{
    order.ApplyDiscount(20.00m);
    Console.WriteLine($"Discount applied. New final total: {order.CalculateFinalTotal():C}");
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Discount error: {ex.Message}");
}

bool cancelled = order.CancelOrder();
if (cancelled)
{
    Console.WriteLine("Order cancelled successfully.");
}
else
{
    Console.WriteLine($"Cannot cancel order in status: {order.Status}");
}
```

## Notes

- **Validation scope**: `Validate()` performs local consistency checks only. It does not verify external dependencies such as user existence or inventory availability. Callers should combine it with broader service-layer validation.
- **Status transition rules**: The allowed state graph is enforced inside `UpdateStatus`. Invoking it with an invalid transition throws `InvalidOperationException`. Consult the `EntityStatus` documentation for the exact legal transitions.
- **Discount boundaries**: `ApplyDiscount` enforces that the final total cannot become negative. Passing a discount greater than `TotalAmount + TaxAmount` throws `ArgumentOutOfRangeException`.
- **Cancellation idempotency**: `CancelOrder` returns `false` without side effects if the order is already in a terminal state. It does not throw in this case.
- **Timestamp management**: `UpdatedAt` is always refreshed on `UpdateStatus`, `ApplyDiscount`, and successful `CancelOrder` calls. `ShippedAt` and `DeliveredAt` are set only when transitioning into their respective statuses and are left unchanged otherwise.
- **Thread safety**: This type is not inherently thread-safe. Concurrent mutations from multiple threads—especially around status transitions and discount application—must be synchronized externally to avoid race conditions on `UpdatedAt` and status-dependent timestamp fields.
