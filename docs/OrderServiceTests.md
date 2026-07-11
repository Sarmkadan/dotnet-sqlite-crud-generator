# OrderServiceTests

The `OrderServiceTests` class provides a suite of unit tests for the `OrderService` component. It validates correct delegation to the underlying repository, proper invocation of audit logging, and appropriate error handling when invalid data (e.g., a non‑existent user or order) is supplied. All tests are asynchronous and rely on mocked dependencies to isolate the service logic.

## API

### `public OrderServiceTests()`
Initializes the test class. Typically sets up mock instances of `IOrderRepository`, `IUserRepository`, and `IAuditLogger`, then creates the `OrderService` under test.

### `public async Task GetAsync_WithValidId_ReturnsOrderFromRepository()`
Verifies that `GetAsync` with a valid order ID returns the order object retrieved from the repository.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

### `public async Task GetAllAsync_ReturnsAllOrdersFromRepository()`
Verifies that `GetAllAsync` returns the complete collection of orders provided by the repository.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

### `public async Task CreateAsync_AddsOrderThroughRepositoryAndLogsAudit()`
Verifies that `CreateAsync` calls the repository’s add method and logs an audit entry upon successful creation.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

### `public async Task UpdateAsync_UpdatesOrderThroughRepositoryAndLogsAudit()`
Verifies that `UpdateAsync` calls the repository’s update method and logs an audit entry when the update succeeds.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

### `public async Task DeleteAsync_DeletesOrderThroughRepositoryAndLogsAudit()`
Verifies that `DeleteAsync` calls the repository’s delete method and logs an audit entry after deletion.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

### `public async Task CreateAsync_WithNonExistentUser_ThrowsArgumentException()`
Verifies that `CreateAsync` throws an `ArgumentException` when the specified user does not exist in the user repository.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** `ArgumentException` – thrown by the service when the user lookup fails.

### `public async Task UpdateAsync_WithNonExistentOrder_ReturnsFalse()`
Verifies that `UpdateAsync` returns `false` when the order to update does not exist in the repository.  
**Parameters:** None.  
**Returns:** `Task`.  
**Throws:** None.

## Usage

The following examples demonstrate typical test arrangements using xUnit and Moq. They assume that `OrderService` depends on `IOrderRepository`, `IUserRepository`, and `IAuditLogger`.

### Example 1: Testing a successful creation with audit logging

```csharp
[Fact]
public async Task CreateAsync_AddsOrderThroughRepositoryAndLogsAudit()
{
    // Arrange
    var orderRepoMock = new Mock<IOrderRepository>();
    var userRepoMock = new Mock<IUserRepository>();
    var auditMock = new Mock<IAuditLogger>();

    userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

    var service = new OrderService(orderRepoMock.Object, userRepoMock.Object, auditMock.Object);
    var order = new Order { UserId = 1, Product = "Widget", Quantity = 5 };

    // Act
    await service.CreateAsync(order);

    // Assert
    orderRepoMock.Verify(r => r.AddAsync(order), Times.Once);
    auditMock.Verify(a => a.LogAsync("OrderCreated", It.IsAny<string>()), Times.Once);
}
```

### Example 2: Testing an exception for a non‑existent user

```csharp
[Fact]
public async Task CreateAsync_WithNonExistentUser_ThrowsArgumentException()
{
    // Arrange
    var orderRepoMock = new Mock<IOrderRepository>();
    var userRepoMock = new Mock<IUserRepository>();
    var auditMock = new Mock<IAuditLogger>();

    userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

    var service = new OrderService(orderRepoMock.Object, userRepoMock.Object, auditMock.Object);
    var order = new Order { UserId = 999, Product = "Widget", Quantity = 1 };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(order));
}
```

## Notes

- **Edge cases:**  
  - `CreateAsync_WithNonExistentUser_ThrowsArgumentException` covers the scenario where the user ID does not match any record in the user repository.  
  - `UpdateAsync_WithNonExistentOrder_ReturnsFalse` ensures the service gracefully returns `false` (rather than throwing) when the order ID is not found.  
  - All other tests assume valid inputs and focus on verifying repository and audit interactions.

- **Thread safety:**  
  These tests are not designed for concurrent execution. Each test creates its own set of mocks and service instance, and shared state is not used. Running tests in parallel with the same fixture may lead to interference if mocks are reused; therefore, a new instance of the test class (or at least fresh mocks) should be used per test. The test runner’s default isolation guarantees are sufficient.

- **Dependencies:**  
  The tests rely on mocked interfaces. Any change to the `OrderService` constructor or the signatures of its dependencies will require corresponding updates to these tests.
