#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// Extension methods for entity change events providing common operations and utilities.
/// </summary>
public static class EntityChangedEventExtensions
{
    /// <summary>
    /// Creates a deep copy of the entity change event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The source event</param>
    /// <returns>A new instance with copied data</returns>
    public static EntityCreatedEvent<T> DeepCopy<T>(this EntityCreatedEvent<T> @event) where T : class
    {
        if (@event.Entity is null)
        {
            return new EntityCreatedEvent<T>(@event.AggregateId, null!);
        }

        var copiedEntity = JsonSerializer.Deserialize<T>(
            JsonSerializer.Serialize(@event.Entity),
            new JsonSerializerOptions { WriteIndented = false }
        );

        return new EntityCreatedEvent<T>(@event.AggregateId, copiedEntity!);
    }

    /// <summary>
    /// Creates a deep copy of the entity change event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The source event</param>
    /// <returns>A new instance with copied data</returns>
    public static EntityUpdatedEvent<T> DeepCopy<T>(this EntityUpdatedEvent<T> @event) where T : class
    {
        var copiedEntity = @event.Entity is null
            ? null
            : JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(@event.Entity),
                new JsonSerializerOptions { WriteIndented = false }
            );

        var copiedOldEntity = @event.OldEntity is null
            ? null
            : JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(@event.OldEntity),
                new JsonSerializerOptions { WriteIndented = false }
            );

        var copiedChanges = new Dictionary<string, (object? OldValue, object? NewValue)>();
        foreach (var change in @event.Changes)
        {
            copiedChanges[change.Key] = (change.Value.OldValue, change.Value.NewValue);
        }

        return new EntityUpdatedEvent<T>(@event.AggregateId, copiedEntity!, copiedOldEntity)
        {
            Changes = copiedChanges
        };
    }

    /// <summary>
    /// Creates a deep copy of the entity change event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The source event</param>
    /// <returns>A new instance with copied data</returns>
    public static EntityDeletedEvent<T> DeepCopy<T>(this EntityDeletedEvent<T> @event) where T : class
    {
        return new EntityDeletedEvent<T>(@event.AggregateId, @event.Entity);
    }

    /// <summary>
    /// Creates a deep copy of the bulk entity change event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The source event</param>
    /// <returns>A new instance with copied data</returns>
    public static BulkEntityChangedEvent<T> DeepCopy<T>(this BulkEntityChangedEvent<T> @event) where T : class
    {
        var copiedEntities = new List<T>();
        foreach (var entity in @event.Entities)
        {
            if (entity is not null)
            {
                var copiedEntity = JsonSerializer.Deserialize<T>(
                    JsonSerializer.Serialize(entity),
                    new JsonSerializerOptions { WriteIndented = false }
                );
                copiedEntities.Add(copiedEntity!);
            }
        }

        return new BulkEntityChangedEvent<T>(@event.Count, @event.Operation, copiedEntities);
    }

    /// <summary>
    /// Determines whether the event represents a creation operation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event to check</param>
    /// <returns>True if the event is a creation event; otherwise, false</returns>
    public static bool IsCreation<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event is EntityCreatedEvent<T>;
    }

    /// <summary>
    /// Determines whether the event represents an update operation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event to check</param>
    /// <returns>True if the event is an update event; otherwise, false</returns>
    public static bool IsUpdate<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event is EntityUpdatedEvent<T>;
    }

    /// <summary>
    /// Determines whether the event represents a deletion operation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event to check</param>
    /// <returns>True if the event is a deletion event; otherwise, false</returns>
    public static bool IsDeletion<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event is EntityDeletedEvent<T>;
    }

    /// <summary>
    /// Determines whether the event represents a bulk operation.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event to check</param>
    /// <returns>True if the event is a bulk event; otherwise, false</returns>
    public static bool IsBulkOperation<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event is BulkEntityChangedEvent<T>;
    }

    /// <summary>
    /// Gets the entity type name from the event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The entity type name, or empty string if not available</returns>
    public static string GetEntityTypeName<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event.EntityType;
    }

    /// <summary>
    /// Gets the entity instance from the event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The entity instance, or null if not available</returns>
    public static T? GetEntity<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event.Entity;
    }

    /// <summary>
    /// Gets the old entity instance from the event (for update events).
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The old entity instance, or null if not available or not an update event</returns>
    public static T? GetOldEntity<T>(this EntityUpdatedEvent<T> @event) where T : class
    {
        return @event.OldEntity;
    }

    /// <summary>
    /// Gets the changes dictionary from the event (for update events).
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The changes dictionary, or empty dictionary if not available</returns>
    public static Dictionary<string, (object? OldValue, object? NewValue)> GetChanges<T>(this EntityUpdatedEvent<T> @event) where T : class
    {
        return @event.Changes;
    }

    /// <summary>
    /// Gets the count of entities affected by the event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The count of entities, or 0 if not applicable</returns>
    public static int GetCount<T>(this BulkEntityChangedEvent<T> @event) where T : class
    {
        return @event.Count;
    }

    /// <summary>
    /// Gets the operation type from the event.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The operation type ("Create", "Update", "Delete", "Bulk"), or empty string if not available</returns>
    public static string GetOperationType<T>(this EntityChangedEvent<T> @event) where T : class
    {
        return @event switch
        {
            EntityCreatedEvent<T> => "Create",
            EntityUpdatedEvent<T> => "Update",
            EntityDeletedEvent<T> => "Delete",
            BulkEntityChangedEvent<T> typedEvent => string.IsNullOrEmpty(typedEvent.Operation) ? "Bulk" : typedEvent.Operation,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the entity type name from a product-specific event.
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>"Product"</returns>
    public static string GetEntityTypeName(this ProductRestockedEvent @event)
    {
        return "Product";
    }

    /// <summary>
    /// Gets the entity type name from a product-specific event.
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>"Product"</returns>
    public static string GetEntityTypeName(this ProductSoldEvent @event)
    {
        return "Product";
    }

    /// <summary>
    /// Gets the count of entities affected (always 1 for single entity events).
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>1</returns>
    public static int GetCount(this ProductRestockedEvent @event)
    {
        return 1;
    }

    /// <summary>
    /// Gets the count of entities affected (always 1 for single entity events).
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>1</returns>
    public static int GetCount(this ProductSoldEvent @event)
    {
        return 1;
    }

    /// <summary>
    /// Gets the operation type from a product-specific event.
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>The operation type</returns>
    public static string GetOperationType(this ProductRestockedEvent @event)
    {
        return "Restock";
    }

    /// <summary>
    /// Gets the operation type from a product-specific event.
    /// </summary>
    /// <param name="event">The product event</param>
    /// <returns>The operation type</returns>
    public static string GetOperationType(this ProductSoldEvent @event)
    {
        return "Sale";
    }
}