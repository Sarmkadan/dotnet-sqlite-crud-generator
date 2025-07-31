# EntityChangedEventExtensions

Provides extension methods for working with entity change events (`EntityCreatedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityDeletedEvent<T>`, and `BulkEntityChangedEvent<T>`) in a type-safe manner. These utilities simplify common operations like copying events, determining operation types, extracting entity data, and analyzing changes.

## API

### `EntityCreatedEvent<T> DeepCopy<T>(this EntityCreatedEvent<T> source)`
Creates a deep copy of an `EntityCreatedEvent<T>` instance.
- **Parameters**: `source` – The event to copy.
- **Return value**: A new `EntityCreatedEvent<T>` with copied entity data.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `EntityUpdatedEvent<T> DeepCopy<T>(this EntityUpdatedEvent<T> source)`
Creates a deep copy of an `EntityUpdatedEvent<T>` instance.
- **Parameters**: `source` – The event to copy.
- **Return value**: A new `EntityUpdatedEvent<T>` with copied entity and old entity data.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `EntityDeletedEvent<T> DeepCopy<T>(this EntityDeletedEvent<T> source)`
Creates a deep copy of an `EntityDeletedEvent<T>` instance.
- **Parameters**: `source` – The event to copy.
- **Return value**: A new `EntityDeletedEvent<T>` with copied entity data.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `BulkEntityChangedEvent<T> DeepCopy<T>(this BulkEntityChangedEvent<T> source)`
Creates a deep copy of a `BulkEntityChangedEvent<T>` instance.
- **Parameters**: `source` – The event to copy.
- **Return value**: A new `BulkEntityChangedEvent<T>` with copied entity data.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `bool IsCreation<T>(this EntityChangedEvent<T> @event)`
Determines whether the given event represents an entity creation.
- **Parameters**: `@event` – The event to check.
- **Return value**: `true` if the event is a creation; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `bool IsUpdate<T>(this EntityChangedEvent<T> @event)`
Determines whether the given event represents an entity update.
- **Parameters**: `@event` – The event to check.
- **Return value**: `true` if the event is an update; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `bool IsDeletion<T>(this EntityChangedEvent<T> @event)`
Determines whether the given event represents an entity deletion.
- **Parameters**: `@event` – The event to check.
- **Return value**: `true` if the event is a deletion; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `bool IsBulkOperation<T>(this EntityChangedEvent<T> @event)`
Determines whether the given event represents a bulk operation.
- **Parameters**: `@event` – The event to check.
- **Return value**: `true` if the event is a bulk operation; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `string GetEntityTypeName<T>(this EntityChangedEvent<T> @event)`
Gets the name of the entity type associated with the event.
- **Parameters**: `@event` – The event from which to extract the type name.
- **Return value**: The full type name of the entity.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `T? GetEntity<T>(this EntityChangedEvent<T> @event)`
Gets the current entity from the event.
- **Parameters**: `@event` – The event from which to extract the entity.
- **Return value**: The current entity, or `null` if not available.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `T? GetOldEntity<T>(this EntityChangedEvent<T> @event)`
Gets the previous entity state from the event (applies to updates and deletions).
- **Parameters**: `@event` – The event from which to extract the old entity.
- **Return value**: The previous entity state, or `null` if not available.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `Dictionary<string, (object? OldValue, object? NewValue)> GetChanges<T>(this EntityChangedEvent<T> @event)`
Extracts the changed property values from an update event.
- **Parameters**: `@event` – The event from which to extract changes.
- **Return value**: A dictionary mapping property names to their old and new values. Empty if not an update event.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `int GetCount<T>(this BulkEntityChangedEvent<T> @event)`
Gets the number of entities affected by a bulk operation.
- **Parameters**: `@event` – The bulk event.
- **Return value**: The count of affected entities.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `string GetOperationType<T>(this EntityChangedEvent<T> @event)`
Gets a string representing the type of operation (e.g., "Created", "Updated", "Deleted").
- **Parameters**: `@event` – The event.
- **Return value**: A string describing the operation type.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `string GetEntityTypeName(this EntityChangedEvent @event)`
Gets the name of the entity type associated with the event (non-generic overload).
- **Parameters**: `@event` – The event from which to extract the type name.
- **Return value**: The full type name of the entity.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `int GetCount(this BulkEntityChangedEvent @event)`
Gets the number of entities affected by a bulk operation (non-generic overload).
- **Parameters**: `@event` – The bulk event.
- **Return value**: The count of affected entities.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

### `string GetOperationType(this EntityChangedEvent @event)`
Gets a string representing the type of operation (non-generic overload).
- **Parameters**: `@event` – The event.
- **Return value**: A string describing the operation type.
- **Throws**: `ArgumentNullException` if `@event` is `null`.

## Usage

### Example 1: Handling Entity Creation
