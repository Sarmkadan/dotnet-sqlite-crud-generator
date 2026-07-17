# EventBusValidation

Provides static validation logic for event bus configurations within the `dotnet-sqlite-crud-generator` project. This type exposes overloads that allow consumers to check whether a given set of event bus definitions is valid, retrieve a list of validation errors, and enforce validity by throwing an exception when the configuration is invalid.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(/* parameters omitted – see overloads */)
```

Evaluates the supplied event bus configuration and returns a read-only list of human-readable error messages. An empty list indicates a valid configuration. The exact overloads accept different representations of the event bus setup (e.g., type collections, descriptor objects). No exceptions are thrown by this method itself; all problems are reported through the returned list.

### IsValid

```csharp
public static bool IsValid(/* parameters omitted – see overloads */)
```

Convenience wrapper around `Validate`. Returns `true` when the configuration produces zero validation errors; otherwise returns `false`. Accepts the same parameter sets as the corresponding `Validate` overloads. This method never throws.

### EnsureValid

```csharp
public static void EnsureValid(/* parameters omitted – see overloads */)
```

Invokes `Validate` and, if the returned error list is non-empty, throws a `ConfigurationException` whose message aggregates the detected errors. Use this to fail fast during application startup or configuration loading. The overloads mirror those of `Validate` and `IsValid`.

## Usage

### Example 1: Checking validity before registration

```csharp
var eventTypes = new[] { typeof(OrderCreatedEvent), typeof(OrderShippedEvent) };

if (EventBusValidation.IsValid(eventTypes))
{
    EventBus.Register(eventTypes);
    Console.WriteLine("Event bus registered successfully.");
}
else
{
    foreach (var error in EventBusValidation.Validate(eventTypes))
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Failing fast during startup

```csharp
try
{
    var subscriptionMap = LoadSubscriptionMappings();
    EventBusValidation.EnsureValid(subscriptionMap);
    EventBus.Initialize(subscriptionMap);
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Event bus configuration is invalid: {ex.Message}");
    throw; // prevent application from starting with a broken event bus
}
```

## Notes

- All members are static and stateless; they are safe to call concurrently from multiple threads without external synchronization.
- The returned `IReadOnlyList<string>` from `Validate` is a snapshot. Mutating the underlying configuration after the call does not affect the list.
- `EnsureValid` throws `ConfigurationException` (defined elsewhere in the project) rather than a built-in .NET exception. Catch that specific type when you need to distinguish configuration failures from other errors.
- Edge cases such as null or empty input collections are handled and reported as validation errors rather than causing `NullReferenceException`. An empty valid configuration typically returns zero errors and `IsValid` returns `true`.
- The overloads exist to support different internal representations of event bus contracts. Always use the overload that matches the data you have at the call site to avoid unnecessary conversions.
