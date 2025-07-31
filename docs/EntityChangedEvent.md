# EntityChangedEvent

Generic event payload emitted whenever one or more entities of type `T` are inserted, updated, or deleted in the SQLite data store. The event carries the affected entity or entities, the type of change performed, and a detailed change-set when applicable.

## API

### `public T? Entity`
The single entity that was created, updated, or deleted. `null` when the event represents a bulk change affecting multiple entities.

### `public string EntityType`
Fully qualified .NET type name (e.g., `MyApp.Models.Product`) of the entity or entities involved in the change.

### `public EntityCreatedEvent`
Convenience property that returns the same instance cast to `EntityCreatedEvent<T>` when the operation was an insert. Throws `InvalidCastException` if the operation was not a creation.

### `public T? OldEntity`
The previous state of the entity before an update or deletion. `null` for insert operations.

### `public Dictionary<string, (object? OldValue, object? NewValue)> Changes`
Key-value pairs describing every property that changed during an update. Keys are property names; values are tuples of the old and new values. Empty for insert or delete operations.

### `public EntityUpdatedEvent`
Convenience property that returns the same instance cast to `EntityUpdatedEvent<T>` when the operation was an update. Throws `InvalidCastException` if the operation was not an update.

### `public EntityDeletedEvent`
Convenience property that returns the same instance cast to `EntityDeletedEvent<T>` when the operation was a deletion. Throws `InvalidCastException` if the operation was not a deletion.

### `public int Count`
Number of entities affected by the change. Always `1` for single-entity events; greater than `1` for bulk operations.

### `public string Operation`
One of `"Create"`, `"Update"`, or `"Delete"` indicating the kind of change that occurred.

### `public List<T> Entities`
Collection of entities involved in a bulk change. Empty for single-entity events.

### `public BulkEntityChangedEvent`
Convenience property that returns the same instance cast to `BulkEntityChangedEvent<T>` when the operation affected multiple entities. Throws `InvalidCastException` if the operation was not a bulk change.

### `public int ProductId`
Identifier of the product involved in a product-specific event (`ProductRestockedEvent` or `ProductSoldEvent`).

### `public int QuantityAdded`
Amount by which inventory was increased in a `ProductRestockedEvent`.

### `public int NewQuantity`
Inventory level after a restock or sale.

### `public ProductRestockedEvent`
Convenience property that returns the same instance cast to `ProductRestockedEvent` when the event describes a restock. Throws `InvalidCastException` if the event is unrelated to a restock.

### `public int QuantitySold`
Number of units sold in a `ProductSoldEvent`.

### `public decimal Revenue`
Total monetary value of the sale in a `ProductSoldEvent`.

### `public int RemainingQuantity`
Inventory level remaining after a sale in a `ProductSoldEvent`.

### `public ProductSoldEvent`
Convenience property that returns the same instance cast to `ProductSoldEvent` when the event describes a sale. Throws `InvalidCastException` if the event is unrelated to a sale.

## Usage
