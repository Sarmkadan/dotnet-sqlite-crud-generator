# OrderValidation

`OrderValidation` is a static utility class that provides centralized, rule-based validation for `Order` entities. It exposes methods to check whether an order is in a valid state overall, whether its final total meets business constraints, and whether the order can transition to shipped or cancelled statuses. All methods return read-only lists of error messages and offer paired “ensure” methods that throw if validation fails.

## API

### ValidateAll
```csharp
public static IReadOnlyList<string> ValidateAll(Order order)
```
Runs every validation rule applicable to the given `Order` and returns a read-only list of all error messages. An empty list means the order is fully valid. This method does not throw.

### IsValid
```csharp
public static bool IsValid(Order order)
```
Convenience predicate that returns `true` when `ValidateAll(order)` produces zero errors. Returns `false` otherwise. Does not throw.

### EnsureValid
```csharp
public static void EnsureValid(Order order)
```
Calls `ValidateAll(order)` and, if any errors are present, throws an `OrderValidationException` whose message aggregates all error strings. Does nothing when the order is valid.

### ValidateFinalTotal
```csharp
public static IReadOnlyList<string> ValidateFinalTotal(Order order)
```
Validates only the final-total-related rules (e.g., non-negative, within allowed range, matching line-item sums). Returns a read-only list of error messages. An empty list indicates the final total is acceptable. Does not throw.

### EnsureFinalTotalValid
```csharp
public static void EnsureFinalTotalValid(Order order)
```
Calls `ValidateFinalTotal(order)` and throws `OrderValidationException` if any errors exist. Does nothing otherwise.

### ValidateCanShip
```csharp
public static IReadOnlyList<string> ValidateCanShip(Order order)
```
Checks whether the order satisfies all preconditions for shipping (e.g., correct status, payment cleared, items in stock). Returns a read-only list of blocking reasons. An empty list means shipping is allowed. Does not throw.

### EnsureCanShip
```csharp
public static void EnsureCanShip(Order order)
```
Calls `ValidateCanShip(order)` and throws `OrderValidationException` if any blocking reasons are returned. Does nothing when shipping is permitted.

### ValidateCanCancel
```csharp
public static IReadOnlyList<string> ValidateCanCancel(Order order)
```
Checks whether the order is eligible for cancellation (e.g., not already shipped, within cancellation window). Returns a read-only list of reasons cancellation is denied. An empty list means cancellation is allowed. Does not throw.

### EnsureCanCancel
```csharp
public static void EnsureCanCancel(Order order)
```
Calls `ValidateCanCancel(order)` and throws `OrderValidationException` if any denial reasons are returned. Does nothing when cancellation is permitted.

## Usage

### Example 1: Full validation before persisting
```csharp
var order = new Order
{
    Id = 1,
    Status = OrderStatus.Pending,
    FinalTotal = 150.00m,
    Items = new List<OrderItem> { new() { UnitPrice = 75.00m, Quantity = 2 } }
};

if (!OrderValidation.IsValid(order))
{
    var errors = OrderValidation.ValidateAll(order);
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    return;
}

orderRepository.Save(order);
```

### Example 2: Guarding state transitions with ensure methods
```csharp
public void ShipOrder(Order order)
{
    OrderValidation.EnsureCanShip(order);

    order.Status = OrderStatus.Shipped;
    order.ShippedAtUtc = DateTime.UtcNow;
    orderRepository.Save(order);
}

public void CancelOrder(Order order)
{
    OrderValidation.EnsureCanCancel(order);

    order.Status = OrderStatus.Cancelled;
    order.CancelledAtUtc = DateTime.UtcNow;
    orderRepository.Save(order);
}
```

## Notes

- All methods are static and stateless; they are safe to call concurrently from multiple threads without external synchronization.
- The `Validate*` methods never throw; they always return a list, which may be empty. The `Ensure*` methods are the only members that throw `OrderValidationException`.
- Passing `null` for the `order` parameter will cause an `ArgumentNullException` in every method. This is a precondition check distinct from the business validation logic.
- The error lists returned by `ValidateAll`, `ValidateFinalTotal`, `ValidateCanShip`, and `ValidateCanCancel` are read-only snapshots. Modifying the order after a call does not retroactively change a previously returned list.
- `ValidateAll` includes the rules from `ValidateFinalTotal`, `ValidateCanShip`, and `ValidateCanCancel` plus any additional cross-cutting rules. Calling the specific validators is useful when only a subset of rules is relevant to the operation at hand.
- The “ensure” methods are designed for use at domain boundaries (e.g., API controllers, service-layer commands) where an invalid state should immediately halt processing with an exception.
