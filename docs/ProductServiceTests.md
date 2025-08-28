# ProductServiceTests

`ProductServiceTests` is a test class that contains unit tests for the `ProductService` component in the `dotnet-sqlite-crud-generator` project. The tests validate the service’s interaction with its repository, event publishing mechanisms, and helper state used across test scenarios.

## API

### ProductServiceTests
The test class itself. It groups all tests related to `ProductService` and provides shared test fixtures such as identifiers and counters.

### GetAsync_WithIdOfZero_ThrowsArgumentException
**Purpose:** Confirms that invoking `ProductService.GetAsync` with an identifier of zero results in an `ArgumentException`.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** If the method under test does not throw an `ArgumentException` when supplied with `0`, the test will fail (typically via an `Xunit` assertion).

### GetAsync_WithValidId_ReturnsProductReturnedByRepository
**Purpose:** Verifies that a valid product identifier causes `GetAsync` to return the product object supplied by the mocked repository.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** May surface exceptions from the test framework if the returned product does not match the expected value or if the repository setup fails.

### GetAllAsync_WhenCalled_ReturnsAllProductsFromRepository
**Purpose:** Ensures that `GetAllAsync` returns the complete collection of products as provided by the repository mock.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** Fails if the returned collection differs from the mocked data.

### ExistsAsync_WhenRepositoryConfirmsExistence_ReturnsTrue
**Purpose:** Checks that `ExistsAsync` returns `true` when the repository reports that a product with the given identifier exists.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** Fails if the result is `false` or an unexpected exception is thrown.

### PublishAsync_WithRegisteredAsyncHandler_HandlerReceivesPublishedEvent
**Purpose:** Validates that publishing an event via `PublishAsync` invokes any registered asynchronous handlers with the correct event.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** Fails if the handler is not called or receives an incorrect event type.

### GetEventHistory_AfterPublishingMultipleEvents_ContainsAllPublishedEventTypes
**Purpose:** After publishing several events, asserts that the event history collected by the service contains each distinct event type that was published.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** Fails if any published event type is missing from the history.

### GetSubscriberCount_AfterSubscribing_ReflectsRegisteredHandlers
**Purpose:** Confirms that the subscriber count reported by the service matches the number of handlers registered after a subscription operation.  
**Parameters:** None.  
**Return Value:** `void` (synchronous test method).  
**Throws:** Fails if the count does not equal the expected number of handlers.

### ClearEventHistory_AfterPublishingEvents_LeavesHistoryEmpty
**Purpose:** After publishing events, verifies that calling `ClearEventHistory` removes all entries from the event history.  
**Parameters:** None.  
**Return Value:** A `Task` that completes when the assertion passes.  
**Throws:** Fails if any history items remain after the clear operation.

### ProductId
**Purpose:** Holds a sample product identifier used across multiple test methods to represent a valid entity key.  
**Type:** `int`.  
**Parameters:** None.  
**Remarks:** This field is mutable; tests that modify it should re‑initialize it if isolation is required.

### Remaining
**Purpose:** Stores a count of remaining items (e.g., stock quantity) utilized in test scenarios that depend on inventory logic.  
**Type:** `int`.  
**Parameters:** None.  
**Remarks:** Like `ProductId`, this field is shared across tests unless a new instance of the test class is created.

### GetEventName
**Purpose:** Contains the name of the method or property used to retrieve an event’s name during event‑history assertions.  
**Type:** `string`.  
**Parameters:** None.  
**Remarks:** The value is typically set to match the actual event‑naming convention employed by `ProductService`.

## Usage

```csharp
// Example 1: Executing a test method that validates successful retrieval.
var testInstance = new ProductServiceTests();
// Arrange (typically done inside the test method, omitted for brevity)
// Act & Assert
await testInstance.GetAsync_WithValidId_ReturnsProductReturnedByRepository();
```

```csharp
// Example 2: Using the shared fields to set up test data for a custom scenario.
var testInstance = new ProductServiceTests();
testInstance.ProductId = 42;
testInstance.Remaining = 0;
// The fields can now be passed to helper methods or arranged mocks.
```

## Notes

- The `GetAsync_WithIdOfZero_ThrowsArgumentException` test specifically targets the value `0`; other invalid identifiers (e.g., negative numbers) are not covered by the existing members and would require additional tests.
- Fields `ProductId`, `Remaining`, and `GetEventName` are instance members; if a single `ProductServiceTests` instance is reused across multiple test invocations, changes to these fields will be visible to subsequent tests. To ensure isolation, either create a new instance per test or reset the fields before each test.
- The test class does not contain any static state, so thread‑safety concerns arise only from sharing an instance across threads. Running the test methods concurrently on the same instance may lead to race conditions on the mutable fields. It is recommended to execute tests sequentially or to instantiate a fresh test class per thread.
