# OrderService

The `OrderService` class provides the primary business logic layer for managing order entities within the `dotnet-sqlite-crud-generator` project. It encapsulates all data access operations related to orders, including standard CRUD functionality, status transitions such as shipping and delivery, and the aggregation of key performance metrics. This service acts as the central interface for retrieving order details, validating data integrity, and calculating financial summaries like total revenue and average order values.

## API

### Constructors

#### `public OrderService()`
Initializes a new instance of the `OrderService` class. This constructor sets up the necessary internal dependencies required to interact with the underlying SQLite database context.

### Data Retrieval

#### `public async Task<Order?> GetAsync`
Retrieves a single order by its unique identifier.
*   **Parameters**: Accepts the unique identifier (typically `int` or `Guid`, depending on the `Order` definition) of the order to retrieve.
*   **Return Value**: Returns an `Order` object if found; otherwise, returns `null`.
*   **Exceptions**: May throw database-related exceptions if the connection fails or the query times out.

#### `public async Task<IEnumerable<Order>> GetAllAsync`
Retrieves a collection of all orders stored in the database.
*   **Parameters**: None.
*   **Return Value**: Returns an enumerable collection of `Order` objects. Returns an empty collection if no orders exist.
*   **Exceptions**: Throws if the database is inaccessible.

#### `public async Task<IEnumerable<Order>> GetUserOrdersAsync`
Retrieves all orders associated with a specific user.
*   **Parameters**: Accepts the unique identifier of the user.
*   **Return Value**: Returns an enumerable collection of `Order` objects belonging to the specified user.
*   **Exceptions**: Throws if the user ID format is invalid or database access fails.

#### `public async Task<IEnumerable<Order>> GetPendingOrdersAsync`
Retrieves all orders that are currently in a "Pending" status.
*   **Parameters**: None.
*   **Return Value**: Returns an enumerable collection of `Order` objects where the status is pending.
*   **Exceptions**: Throws if the database query cannot be executed.

#### `public async Task<bool> ExistsAsync`
Checks whether an order with a specific identifier exists in the database.
*   **Parameters**: Accepts the unique identifier of the order.
*   **Return Value**: Returns `true` if the order exists; otherwise, `false`.
*   **Exceptions**: Throws on database connectivity issues.

### Data Manipulation

#### `public async Task<Order> CreateAsync`
Creates a new order record in the database.
*   **Parameters**: Accepts an `Order` object containing the data to be persisted.
*   **Return Value**: Returns the created `Order` object, typically including generated fields like the new ID or creation timestamp.
*   **Exceptions**: Throws if validation fails, if a duplicate key exists, or if the database write operation fails.

#### `public async Task<bool> UpdateAsync`
Updates an existing order record.
*   **Parameters**: Accepts an `Order` object with updated values.
*   **Return Value**: Returns `true` if the update was successful; `false` if the order was not found or no changes were applied.
*   **Exceptions**: Throws if the order is in an invalid state for updating or if a concurrency conflict occurs.

#### `public async Task<bool> DeleteAsync`
Deletes an order from the database.
*   **Parameters**: Accepts the unique identifier of the order to delete.
*   **Return Value**: Returns `true` if the deletion was successful; `false` if the order did not exist.
*   **Exceptions**: Throws if foreign key constraints prevent deletion or if the database is unavailable.

#### `public async Task<bool> ShipOrderAsync`
Transitions an order from "Pending" to "Shipped" status.
*   **Parameters**: Accepts the unique identifier of the order.
*   **Return Value**: Returns `true` if the status transition was successful; `false` if the order was not found or was not in a shippable state.
*   **Exceptions**: Throws if the business logic rules for shipping are violated.

#### `public async Task<bool> MarkDeliveredAsync`
Transitions an order from "Shipped" to "Delivered" status.
*   **Parameters**: Accepts the unique identifier of the order.
*   **Return Value**: Returns `true` if the status transition was successful; `false` if the order was not found or was not in a deliverable state.
*   **Exceptions**: Throws if the order has not yet been shipped.

### Validation and Metrics

#### `public bool Validate`
Validates the data integrity of an order object.
*   **Parameters**: Typically operates on an instance or accepts an `Order` object (signature implies instance or static context usage depending on implementation, but logically validates order data).
*   **Return Value**: Returns `true` if the data meets all business rules; otherwise, `false`.
*   **Exceptions**: Generally does not throw; returns `false` on validation failure.

#### `public async Task<OrderMetrics> GetMetricsAsync`
Calculates and returns aggregate metrics for the entire order dataset.
*   **Parameters**: None.
*   **Return Value**: Returns an `OrderMetrics` object containing calculated statistics.
*   **Exceptions**: Throws if the calculation query fails due to database errors.

### Properties

#### `public int TotalOrders`
Gets the total count of all orders in the system. This property may trigger a database count operation upon access.

#### `public int PendingOrders`
Gets the count of orders currently in the "Pending" status.

#### `public int DeliveredOrders`
Gets the count of orders marked as "Delivered".

#### `public decimal TotalRevenue`
Gets the sum of the total value of all orders. Returned as `decimal` for financial precision.

#### `public double AverageOrderValue`
Gets the arithmetic mean of the order values. Returned as `double`.

#### `public double AverageTaxAmount`
Gets the arithmetic mean of the tax amounts applied across all orders. Returned as `double`.

#### `public decimal TotalDiscounts`
Gets the sum of all discounts applied across all orders. Returned as `decimal`.

## Usage

### Example 1: Creating and Processing an Order
This example demonstrates creating a new order, validating it, and then proceeding to ship it once confirmed.

```csharp
var service = new OrderService();

var newOrder = new Order 
{ 
    UserId = 42, 
    TotalAmount = 150.00m, 
    Status = OrderStatus.Pending 
};

// Validate before creation
if (service.Validate(newOrder))
{
    var createdOrder = await service.CreateAsync(newOrder);
    Console.WriteLine($"Order created with ID: {createdOrder.Id}");

    // Simulate processing time then ship
    var shipped = await service.ShipOrderAsync(createdOrder.Id);
    if (shipped)
    {
        Console.WriteLine("Order successfully shipped.");
    }
}
else
{
    Console.WriteLine("Order validation failed.");
}
```

### Example 2: Retrieving Metrics and Pending Orders
This example shows how to fetch dashboard-style metrics and list specific pending orders for a user.

```csharp
var service = new OrderService();

// Get high-level metrics
var metrics = await service.GetMetricsAsync();
Console.WriteLine($"Total Revenue: {metrics.TotalRevenue:C}");
Console.WriteLine($"Average Order Value: {metrics.AverageOrderValue:C}");

// Get pending orders for a specific user
var userId = 42;
var pendingUserOrders = await service.GetUserOrdersAsync(userId);
var filteredPending = pendingUserOrders.Where(o => o.Status == OrderStatus.Pending);

Console.WriteLine($"User {userId} has {filteredPending.Count()} pending orders.");
```

## Notes

*   **Thread Safety**: As with most Entity Framework or SQLite context-based services, instances of `OrderService` are not guaranteed to be thread-safe. Do not share a single instance across multiple threads concurrently without external synchronization. It is recommended to instantiate a new service per request or operation scope.
*   **Async Consistency**: All data-modifying and data-retrieving methods are asynchronous. Properties such as `TotalOrders` and `TotalRevenue` may perform synchronous database calls or access cached values; ensure these are not called on UI threads if they involve blocking I/O.
*   **State Transitions**: Methods `ShipOrderAsync` and `MarkDeliveredAsync` enforce strict state machine logic. Attempting to ship an order that is already delivered, or marking an unshipped order as delivered, will result in a `false` return value rather than an exception, unless the underlying database constraint is violated.
*   **Null Handling**: `GetAsync` explicitly returns `null` for missing records. Callers must handle nullability to avoid `NullReferenceException`.
*   **Precision**: Financial calculations return `decimal` for totals (`TotalRevenue`, `TotalDiscounts`) to prevent floating-point rounding errors, while averages utilize `double`. Be mindful of type casting when performing further arithmetic on these properties.
