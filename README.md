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
```