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

## WebhookHandler

The `WebhookHandler` class provides a robust mechanism for sending event notifications to external webhook endpoints. It handles payload serialization, HTTP request execution with retry logic, response validation, and comprehensive delivery tracking with statistics.


Here's a realistic example of configuring and using the `WebhookHandler`:

```csharp
// Create a WebhookHandler with optional signing secret for payload verification
var httpClient = new HttpClient();
var webhookHandler = new WebhookHandler(httpClient, signingSecret: "your-secret-key");

// Define your payload model
public class OrderCreatedPayload
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Register webhook endpoints
webhookHandler.RegisterEndpoint(
    name: "order-service",
    url: new Uri("https://orders.example.com/api/webhooks"),
    eventTypes: ["order.created", "order.updated"],
    active: true
);

webhookHandler.RegisterEndpoint(
    name: "inventory-service",
    url: new Uri("https://inventory.example.com/webhooks"),
    eventTypes: ["product.stock.updated"],
    active: true
);

// Send a webhook notification
var orderPayload = new OrderCreatedPayload
{
    OrderId = 12345,
    CustomerEmail = "customer@example.com",
    TotalAmount = 99.99m,
    CreatedAt = DateTime.UtcNow
};

var success = await webhookHandler.SendWebhookAsync(
    eventType: "order.created",
    payload: orderPayload
);

Console.WriteLine(success ? "Webhook sent successfully" : "Failed to send webhook");

// Get all registered endpoints
var endpoints = webhookHandler.GetEndpoints();
foreach (var endpoint in endpoints)
{
    Console.WriteLine($"Endpoint: {endpoint.Name}");
    Console.WriteLine($"  URL: {endpoint.Url}");
    Console.WriteLine($"  Active: {endpoint.Active}");
    Console.WriteLine($"  Events: {string.Join(", ", endpoint.EventTypes)}");
    Console.WriteLine($"  Created: {endpoint.CreatedAt}");
    Console.WriteLine($"  Deliveries: {endpoint.DeliveryCount}");
    Console.WriteLine($"  Failures: {endpoint.FailureCount}");
}

// Get delivery history for a specific endpoint
var deliveryHistory = webhookHandler.GetDeliveryHistory("order-service");
foreach (var attempt in deliveryHistory)
{
    Console.WriteLine($"Delivery: {attempt.DeliveryId}");
    Console.WriteLine($"  Event: {attempt.EventType}");
    Console.WriteLine($"  Status: {(attempt.Success ? "Success" : "Failed")}");
    Console.WriteLine($"  Status Code: {attempt.StatusCode}");
    Console.WriteLine($"  Attempted: {attempt.AttemptedAt}");
    if (!attempt.Success && !string.IsNullOrEmpty(attempt.Error))
    {
        Console.WriteLine($"  Error: {attempt.Error}");
    }
}

// Get statistics
var stats = webhookHandler.GetStatistics();
Console.WriteLine($"Registered endpoints: {stats.RegisteredEndpoints}");
Console.WriteLine($"Active endpoints: {stats.ActiveEndpoints}");
Console.WriteLine($"Total deliveries: {stats.TotalDeliveries}");
Console.WriteLine($"Successful: {stats.SuccessfulDeliveries}");
Console.WriteLine($"Failed: {stats.FailedDeliveries}");

// Manage endpoint state
webhookHandler.DisableEndpoint("inventory-service");
webhookHandler.EnableEndpoint("order-service");
```

## EntityChangedEvent

The `EntityChangedEvent` is an abstract base class that represents domain events for entity lifecycle changes. It serves as the foundation for tracking entity creation, updates, and deletions throughout the application. This event system enables auditing, notifications, and cross-cutting concerns like logging and caching invalidation.


## ExternalApiClient

The `ExternalApiClient` provides a type-safe HTTP client for interacting with external REST APIs. It simplifies CRUD operations with built-in error handling, supports pagination through `GetCollectionAsync`, and provides detailed response information via `GetWithStatusAsync`. The client automatically handles URL construction and includes a health check method for verifying API availability.

Here's a realistic example of using the `ExternalApiClient` to interact with a REST API:

```csharp
// Create an instance of ExternalApiClient
var httpClient = new HttpClient();
var apiClient = new ExternalApiClient(httpClient, "https://api.example.com/v1");

// Define a model for the API response
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Get a single resource
var user = await apiClient.GetAsync<User>("/users/42");
Console.WriteLine(user?.Name); // Output: John Doe

// Get a collection with pagination
var usersPage = await apiClient.GetCollectionAsync<User>("/users", page: 1, pageSize: 20);
if (usersPage != null)
{
    Console.WriteLine($"Total users: {usersPage.Count}");
    foreach (var user in usersPage)
    {
        Console.WriteLine($"- {user.Name} ({user.Email})");
    }
}

// Create a new resource
var newUser = new { Name = "Jane Smith", Email = "jane@example.com" };
var createdUser = await apiClient.CreateAsync<User>("/users", newUser);
Console.WriteLine(createdUser?.Id); // Output: 43

// Update a resource
var updatedData = new { Name = "John Doe Updated", Email = "john.updated@example.com" };
var updatedUser = await apiClient.UpdateAsync<User>("/users/42", updatedData);
Console.WriteLine(updatedUser?.Name); // Output: John Doe Updated

// Delete a resource
var isDeleted = await apiClient.DeleteAsync("/users/99");
Console.WriteLine(isDeleted ? "User deleted" : "Failed to delete user");

// Get detailed response information
var response = await apiClient.GetWithStatusAsync<User>("/users/42");
if (response.Success)
{
    Console.WriteLine($"Status: {response.StatusCode}");
    Console.WriteLine($"User: {response.Data?.Name}");
}
else
{
    Console.WriteLine($"Error: {response.Message}");
}

// Check API health
var isHealthy = await apiClient.HealthCheckAsync();
Console.WriteLine(isHealthy ? "API is healthy" : "API is unavailable");
```

## HttpClientFactory

The `HttpClientFactory` class provides a centralized mechanism for creating and managing `HttpClient` instances, implementing a connection pool pattern to optimize resource usage. It supports custom configuration via `HttpClientOptions` and works seamlessly with the `HttpRequestExecutor` to simplify HTTP requests with built-in retry policies.

Here's an example of using the `HttpClientFactory` and `HttpRequestExecutor`:

```csharp
// Create the factory
var factory = new HttpClientFactory(connectionLimit: 5);

// Define client options
var options = new HttpClientOptions
{
    BaseAddress = "https://api.example.com/",
    Timeout = TimeSpan.FromSeconds(10)
};

// Get or create a named client
var httpClient = factory.GetOrCreateClient("api-client", options);

// Create an executor for the client
var executor = new HttpRequestExecutor(httpClient, maxRetries: 2);

// Perform an asynchronous JSON request
var data = await executor.GetJsonAsync<Dictionary<string, string>>("/status");

// Clean up resources when done
factory.RemoveClient("api-client");
factory.Dispose();
```


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