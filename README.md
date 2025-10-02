// existing content ...

## RepositoryException

The `RepositoryException` class represents an exception that occurs during repository operations. It provides additional information about the type of repository error, the entity type, and the entity ID.

Example usage:
```csharp
try
{
    // Code that may throw a RepositoryException
}
catch (RepositoryException ex)
{
    Console.WriteLine($"Repository error: {ex.Message}");
    Console.WriteLine($"Entity type: {ex.EntityType}");
    Console.WriteLine($"Entity ID: {ex.EntityId}");

    if (ex is RepositoryException.EntityNotFoundException)
    {
        // Handle entity not found
    }
    else if (ex is RepositoryException.DuplicateKeyException)
    {
        // Handle duplicate key
    }
}

// Creating a RepositoryException
var repositoryException = RepositoryException.EntityNotFound("Product", 123);
Console.WriteLine(repositoryException.Message); // Output: Entity of type 'Product' with ID 123 was not found.

// Creating a RepositoryException for duplicate key violations
var duplicateKeyException = RepositoryException.DuplicateKey("Product", "Name", "Test Product");
Console.WriteLine(duplicateKeyException.Message); // Output: An entity of type 'Product' with Name = 'Test Product' already exists.

// Creating a RepositoryException for constraint violations
var constraintViolationException = RepositoryException.ConstraintViolation("Product", "UniqueConstraint");
Console.WriteLine(constraintViolationException.Message); // Output: Constraint violation in entity 'Product': UniqueConstraint
```

## EventBus

The `EventBus` class is a pub-sub messaging system that allows you to publish events and subscribe to them. It provides a way to decouple event producers and consumers, making it easier to manage complex event-driven systems.

Here's an example of how to use the `EventBus` class:
```csharp
// Create an instance of the EventBus
var eventBus = new EventBus();

// Define an event class
public class MyEvent : IEvent
{
    public string Data { get; set; }
}

// Subscribe to the event
eventBus.Subscribe<MyEvent>(async (event) =>
{
    Console.WriteLine($"Received event: {event.Data}");
});

// Publish the event
await eventBus.PublishAsync(new MyEvent { Data = "Hello, World!" });

// Get the event history
var eventHistory = eventBus.GetEventHistory();
foreach (var eventEnvelope in eventHistory)
{
    Console.WriteLine($"Event ID: {eventEnvelope.EventId}");
    Console.WriteLine($"Event Type: {eventEnvelope.EventTypeName}");
    Console.WriteLine($"Timestamp: {eventEnvelope.Timestamp}");
    Console.WriteLine($"Data: {eventEnvelope.Data}");
}

// Get the event statistics
var statistics = eventBus.GetStatistics();
Console.WriteLine($"Registered event types: {statistics.RegisteredEventTypes}");
Console.WriteLine($"Total subscriptions: {statistics.TotalSubscriptions}");
Console.WriteLine($"Total events published: {statistics.TotalEventsPublished}");
```

## EntityChangedEvent

The `EntityChangedEvent` is an abstract base class that represents domain events for entity lifecycle changes. It serves as the foundation for tracking entity creation, updates, and deletions throughout the application. This event system enables auditing, notifications, and cross-cutting concerns like logging and caching invalidation.

Here's a realistic example of using `EntityChangedEvent` with a Product entity:

```csharp
// Define your entity
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

// Subscribe to entity creation events
var eventBus = new EventBus();
eventBus.Subscribe<EntityCreatedEvent<Product>>(async (createdEvent) =>
{
    Console.WriteLine($"Product created: {createdEvent.Entity?.Name}");
    Console.WriteLine($"Entity type: {createdEvent.EntityType}");
    Console.WriteLine($"Event ID: {createdEvent.EventId}");
});

// Subscribe to entity update events
eventBus.Subscribe<EntityUpdatedEvent<Product>>(async (updatedEvent) =>
{
    Console.WriteLine($"Product updated: {updatedEvent.Entity?.Name}");
    Console.WriteLine($"Changes: {string.Join(", ", updatedEvent.Changes.Select(kvp => $"{kvp.Key}: {kvp.Value.OldValue} → {kvp.Value.NewValue}"))}");
});

// Subscribe to entity deletion events
eventBus.Subscribe<EntityDeletedEvent<Product>>(async (deletedEvent) =>
{
    Console.WriteLine($"Product deleted: {deletedEvent.Entity?.Name}");
    Console.WriteLine($"Entity type: {deletedEvent.EntityType}");
});

// Create and publish a product
var product = new Product { Id = 1, Name = "Laptop", Price = 999.99m, StockQuantity = 10 };
await eventBus.PublishAsync(new EntityCreatedEvent<Product>(product.Id, product));

// Update the product
var oldProduct = new Product { Id = 1, Name = "Laptop", Price = 999.99m, StockQuantity = 10 };
var updatedProduct = new Product { Id = 1, Name = "Laptop Pro", Price = 1099.99m, StockQuantity = 10 };
var updateEvent = new EntityUpdatedEvent<Product>(updatedProduct.Id, updatedProduct, oldProduct);
updateEvent.Changes.Add("Name", ("Laptop", "Laptop Pro"));
updateEvent.Changes.Add("Price", (999.99m, 1099.99m));
await eventBus.PublishAsync(updateEvent);

// Delete the product
await eventBus.PublishAsync(new EntityDeletedEvent<Product>(product.Id, product));

// Bulk operations
var products = new List<Product> { product, updatedProduct };
await eventBus.PublishAsync(new BulkEntityChangedEvent<Product>(2, "BulkUpdate", products));
```
```