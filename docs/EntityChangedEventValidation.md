# EntityChangedEventValidation

Provides static validation methods for `EntityChangedEvent<T>` instances, ensuring that event payloads conform to expected structural rules before they are published or consumed. This type centralizes all preconditions related to entity change events, allowing callers to check validity, retrieve lists of validation errors, or assert validity with exceptions.

## API

All members are static and operate on `EntityChangedEvent<T>` objects or non-generic `EntityChangedEvent` instances.

### Validate

```csharp
public static IReadOnlyList<string> Validate<T>(EntityChangedEvent<T> event)
public static IReadOnlyList<string> Validate(EntityChangedEvent event)
```

Returns a read-only list of validation error messages for the given event. An empty list indicates the event is valid. The generic overloads accept events with a typed entity payload; the non-generic overloads accept untyped events. Parameters are the event instance to inspect. No exceptions are thrown by these methods themselves—they accumulate and return errors.

### IsValid

```csharp
public static bool IsValid<T>(EntityChangedEvent<T> event)
public static bool IsValid(EntityChangedEvent event)
```

Returns `true` if the event passes all validation rules; otherwise `false`. These are convenience wrappers around `Validate` that simply check whether the returned error list is empty. Parameters are the event instance to test. No exceptions are thrown.

### EnsureValid

```csharp
public static void EnsureValid<T>(EntityChangedEvent<T> event)
public static void EnsureValid(EntityChangedEvent event)
```

Asserts that the event is valid. Internally calls `Validate` and, if any errors are present, throws an `EntityChangedEventValidationException` (or a similarly named exception type from the same project) whose message aggregates the validation errors. Parameters are the event instance to check. Throws on invalid input; does nothing if valid.

## Usage

**Example 1: Checking validity before publishing**

```csharp
var entity = new Customer { Id = 42, Name = "Acme" };
var changeEvent = new EntityChangedEvent<Customer>(
    entity,
    ChangeType.Updated,
    DateTimeOffset.UtcNow,
    "user-abc"
);

if (EntityChangedEventValidation.IsValid(changeEvent))
{
    eventBus.Publish(changeEvent);
}
else
{
    var errors = EntityChangedEventValidation.Validate(changeEvent);
    logger.LogWarning("Invalid event suppressed: {Errors}", errors);
}
```

**Example 2: Asserting validity at the subscriber boundary**

```csharp
public void Handle(EntityChangedEvent<Product> event)
{
    EntityChangedEventValidation.EnsureValid(event);

    // At this point the event is guaranteed structurally correct.
    inventoryService.UpdateStock(event.Entity.Id, event.ChangeType);
}
```

## Notes

- The validation rules are determined by the internal implementation and typically cover required fields (entity reference, change type, timestamp, originator) and logical consistency. Consult the source or XML comments for the exact rule set.
- All methods are static and maintain no mutable state; they are safe to call concurrently from multiple threads without external synchronization.
- `EnsureValid` throws an exception that derives from the project's configuration/validation exception hierarchy. Catch it explicitly if you need to distinguish validation failures from other runtime errors.
- The non-generic overloads exist to support scenarios where the entity type is not known at compile time, such as deserialized events or dynamic dispatch. They apply the same structural rules but cannot perform type-specific checks on the entity payload beyond its presence.
