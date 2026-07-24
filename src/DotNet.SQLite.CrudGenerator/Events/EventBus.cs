#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// In-process event bus for pub-sub messaging.
/// Supports multiple subscribers per event type with async handler execution.
/// Tracks event history and provides statistics.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();
    private readonly List<EventEnvelope> _eventHistory = new();
    private readonly object _historyLock = new();
    private int _maxHistorySize = 1000;
    private readonly ILogger<EventBus>? _logger;

    /// <summary>
    /// Publishes an event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The event type</typeparam>
    /// <param name="@event">The event to publish</param>
    /// <param name="handlers">The list of handlers to invoke</param>
    /// <param name="eventType">The runtime event type</param>
    private async Task PublishToHandlers<TEvent>(TEvent @event, List<Delegate> handlers, Type eventType) where TEvent : class, IEvent
    {
        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                // Invoke handler - could be Action<T> or Func<T, Task>
                if (handler is Func<TEvent, Task> asyncHandler)
                {
                    tasks.Add(asyncHandler(@event));
                }
                else if (handler is Action<TEvent> syncHandler)
                {
                    syncHandler(@event);
                    tasks.Add(Task.CompletedTask);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in event handler for {EventType}", eventType.Name);
            }
        }

        if (tasks.Any())
            await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Attempts to find a handler for a generic base type when no exact match exists.
    /// This handles the variance issue where EntityCreatedEvent{T} should match handlers for EntityChangedEvent{T}.
    /// </summary>
    /// <param name="eventType">The concrete event type that was published</param>
    /// <param name="handlers">Output parameter for the handlers if found</param>
    /// <returns>True if handlers were found for a base type; false otherwise</returns>
    private bool TryFindGenericBaseHandler(Type eventType, out List<Delegate>? handlers)
    {
        handlers = null;

        // Only check for EntityChangedEvent<T> base types
        if (eventType.IsGenericType &&
            eventType.GetGenericTypeDefinition() == typeof(EntityChangedEvent<>))
        {
            // Already the base type, no need to search
            return false;
        }

        // Check if this is a derived EntityChangedEvent<T> type
        if (eventType.IsGenericType &&
            eventType.GetGenericTypeDefinition().FullName?.StartsWith("DotNet.SQLite.CrudGenerator.Events.Entity") == true)
        {
            var baseType = typeof(EntityChangedEvent<>);
            var genericArgs = eventType.GetGenericArguments();

            if (genericArgs.Length == 1)
            {
                var constructedBaseType = baseType.MakeGenericType(genericArgs[0]);

                if (_subscribers.TryGetValue(constructedBaseType, out handlers))
                {
                    _logger?.LogDebug("Found handler for base type {BaseType} for event {EventType}",
                        constructedBaseType.Name, eventType.Name);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Initializes a new instance of the EventBus class.
    /// </summary>
    /// <param name="logger">Optional logger for event bus operations</param>
    public EventBus(ILogger<EventBus>? logger = null)
    {
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(@event);

        // Validate the event using EventBusValidation
        try
        {
            EventBusValidation.EnsureValid((DomainEvent?)(object)@event);
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Event validation failed for {EventType}", typeof(TEvent).Name);
            throw;
        }

        var eventType = typeof(TEvent);

        // Record event in history
        RecordEvent(@event);

        // Check for handlers using the exact event type first
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            await PublishToHandlers(@event, handlers, eventType);
            return;
        }

        // Handle variance for generic EntityChangedEvent<T> types
        // If no handlers for EntityCreatedEvent<Customer>, check for EntityChangedEvent<Customer>
        if (TryFindGenericBaseHandler(eventType, out var baseHandlers) && baseHandlers != null)
        {
            await PublishToHandlers(@event, baseHandlers, eventType);
            return;
        }

        // Log unhandled events
        _logger?.LogDebug("No subscribers found for event type {EventType}. Event will not be processed.", eventType.Name);
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        var handlers = _subscribers.GetOrAdd(eventType, _ => new List<Delegate>());

        lock (handlers)
        {
            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }

        _logger?.LogDebug("Subscribed async handler to event type {EventType}", eventType.Name);
    }

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        var handlers = _subscribers.GetOrAdd(eventType, _ => new List<Delegate>());

        lock (handlers)
        {
            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }

        _logger?.LogDebug("Subscribed sync handler to event type {EventType}", eventType.Name);
    }

    public bool Unsubscribe<TEvent>(Delegate handler) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            lock (handlers)
            {
                var removed = handlers.Remove(handler);
                if (removed)
                {
                    _logger?.LogDebug("Unsubscribed handler from event type {EventType}", eventType.Name);
                }
                return removed;
            }
        }

        return false;
    }

    public int GetSubscriberCount<TEvent>() where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
            return handlers.Count;

        return 0;
    }

    public IEnumerable<EventEnvelope> GetEventHistory(string? eventTypeName = null)
    {
        lock (_historyLock)
        {
            if (string.IsNullOrEmpty(eventTypeName))
                return _eventHistory.ToList();

            return _eventHistory
                .Where(e => e.EventTypeName == eventTypeName)
                .ToList();
        }
    }

    public void ClearEventHistory()
    {
        lock (_historyLock)
        {
            _eventHistory.Clear();
        }
    }

    public EventBusStatistics GetStatistics()
    {
        var stats = new EventBusStatistics
        {
            RegisteredEventTypes = _subscribers.Count,
            TotalSubscriptions = _subscribers.Values.Sum(h => h.Count),
            TotalEventsPublished = _eventHistory.Count,
            Subscriptions = _subscribers.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value.Count)
        };

        return stats;
    }

    private void RecordEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        lock (_historyLock)
        {
            var envelope = new EventEnvelope
            {
                EventId = Guid.NewGuid(),
                EventTypeName = typeof(TEvent).Name,
                Timestamp = DateTime.UtcNow,
                Data = @event
            };

            _eventHistory.Add(envelope);

            // Remove oldest events if history exceeds max size
            if (_eventHistory.Count > _maxHistorySize)
                _eventHistory.RemoveRange(0, _eventHistory.Count - _maxHistorySize);
        }
    }
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent;
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent;
    bool Unsubscribe<TEvent>(Delegate handler) where TEvent : class, IEvent;
    int GetSubscriberCount<TEvent>() where TEvent : class, IEvent;
}

public interface IEvent
{
    string GetEventName();
}

public sealed class EventEnvelope
{
    public Guid EventId { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Data { get; set; }
}

public sealed class EventBusStatistics
{
    public int RegisteredEventTypes { get; set; }
    public int TotalSubscriptions { get; set; }
    public int TotalEventsPublished { get; set; }
    public Dictionary<string, int> Subscriptions { get; set; } = new();
}

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract class DomainEvent : IEvent
{
    public Guid AggregateId { get; protected set; }
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
    public string EventName { get; protected set; } = string.Empty;

    public virtual string GetEventName() => EventName;
}
