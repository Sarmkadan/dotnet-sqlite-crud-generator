# EventBus

A lightweight in-memory event bus for .NET applications that supports asynchronous event publishing and synchronous subscription handling with optional event history tracking. Designed for decoupling components in domain-driven designs and CQRS patterns, particularly in applications using SQLite for persistence.

## API

### `public async Task PublishAsync<TEvent>(TEvent @event)`

Publishes an event to all subscribers of type `TEvent`. The event is delivered to subscribers in the order they were registered.

- **Parameters**
  - `@event`: The event payload to publish. Must be a reference or value type.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `@event` is `null`.
  - `InvalidOperationException`: Thrown if the event type `TEvent` has not been subscribed to by any handler.

### `public void Subscribe<TEvent>(Action<TEvent> handler)`

Registers a synchronous handler for events of type `TEvent`. The handler is invoked immediately when an event of the same type is published.

- **Parameters**
  - `handler`: The delegate to invoke when an event of type `TEvent` is published.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `handler` is `null`.

### `public void Subscribe<TEvent>(Func<TEvent, Task> asyncHandler)`

Registers an asynchronous handler for events of type `TEvent`. The handler is invoked immediately when an event of the same type is published.

- **Parameters**
  - `asyncHandler`: The asynchronous delegate to invoke when an event of type `TEvent` is published.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `asyncHandler` is `null`.

### `public bool Unsubscribe<TEvent>(Action<TEvent> handler)`

Removes a previously registered synchronous handler for events of type `TEvent`.

- **Parameters**
  - `handler`: The handler to remove.
- **Return value**: `true` if the handler was found and removed; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `handler` is `null`.

### `public bool Unsubscribe<TEvent>(Func<TEvent, Task> asyncHandler)`

Removes a previously registered asynchronous handler for events of type `TEvent`.

- **Parameters**
  - `asyncHandler`: The handler to remove.
- **Return value**: `true` if the handler was found and removed; otherwise, `false`.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `asyncHandler` is `null`.

### `public int GetSubscriberCount<TEvent>()`

Returns the number of handlers currently subscribed to events of type `TEvent`.

- **Return value**: The count of registered handlers for `TEvent`.
- **Exceptions**
  - `InvalidOperationException`: Thrown if the event type `TEvent` has not been subscribed to by any handler.

### `public IEnumerable<EventEnvelope> GetEventHistory()`

Returns a read-only snapshot of all published events in the order they were published. Each event is wrapped in an `EventEnvelope` containing metadata.

- **Return value**: An `IEnumerable<EventEnvelope>` of published events. Returns an empty sequence if no events have been published.

### `public void ClearEventHistory()`

Removes all events from the event history log. This does not affect active subscriptions or future event publishing.

### `public EventBusStatistics GetStatistics()`

Returns a snapshot of current bus statistics including counts of registered event types, total subscriptions, and total events published.

- **Return value**: An `EventBusStatistics` object containing:
  - `RegisteredEventTypes`: Number of distinct event types with at least one subscriber.
  - `TotalSubscriptions`: Total number of handler registrations across all event types.
  - `TotalEventsPublished`: Total number of events published since the bus was created.

### `public Guid EventId`

Gets the unique identifier for the current event envelope or published event. Used as a correlation ID for tracing event flows.

### `public string EventTypeName`

Gets the fully qualified type name of the event (e.g., `Namespace.EventName`). Used for serialization and routing.

### `public DateTime Timestamp`

Gets the UTC timestamp when the event was published.

### `public object? Data`

Gets the event payload. May be `null` for metadata-only events.

### `public int RegisteredEventTypes`

Gets the number of distinct event types with at least one subscriber.

### `public int TotalSubscriptions`

Gets the total number of handler registrations across all event types.

### `public int TotalEventsPublished`

Gets the total number of events published since the bus was created.

### `public Dictionary<string, int> Subscriptions`

Gets a snapshot of the current subscription counts per event type. The key is the event type name, and the value is the number of handlers for that type.

### `public Guid AggregateId`

Gets the identifier of the aggregate root or entity associated with the event.

### `public DateTime OccurredAt`

Gets the UTC timestamp when the event occurred (typically when the aggregate state changed).

### `public string EventName`

Gets the logical name of the event (e.g., `OrderCreated`). Used for display and routing.

### `public virtual string GetEventName()`

Returns the logical event name. Can be overridden to customize the name returned for a specific event type.

- **Return value**: The event name string.

## Usage

### Example 1: Basic Event Publishing and Subscription
