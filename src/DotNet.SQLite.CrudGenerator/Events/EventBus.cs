// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// In-process event bus for pub-sub messaging.
/// Supports multiple subscribers per event type with async handler execution.
/// Tracks event history and provides statistics.
/// </summary>
public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();
    private readonly List<EventEnvelope> _eventHistory = new();
    private readonly object _historyLock = new();
    private int _maxHistorySize = 1000;

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);

        // Record event in history
        RecordEvent(@event);

        if (_subscribers.TryGetValue(eventType, out var handlers))
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
                        tasks.Add(Task.FromResult(syncHandler(@event)));
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}");
                }
            }

            if (tasks.Any())
                await Task.WhenAll(tasks);
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);
        var handlers = _subscribers.GetOrAdd(eventType, _ => new List<Delegate>());

        lock (handlers)
        {
            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }
    }

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);
        var handlers = _subscribers.GetOrAdd(eventType, _ => new List<Delegate>());

        lock (handlers)
        {
            if (!handlers.Contains(handler))
                handlers.Add(handler);
        }
    }

    public bool Unsubscribe<TEvent>(Delegate handler) where TEvent : class, IEvent
    {
        if (handler == null)
            return false;

        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            lock (handlers)
            {
                return handlers.Remove(handler);
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

public class EventEnvelope
{
    public Guid EventId { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Data { get; set; }
}

public class EventBusStatistics
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
