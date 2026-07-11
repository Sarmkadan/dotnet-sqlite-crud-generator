# RepositoryIntegrationTests

Integration tests for the `Repository` class, validating CRUD operations against a SQLite in-memory database. These tests ensure that the repository correctly persists, retrieves, updates, and deletes product entities in a real database context.

## API

### `RepositoryIntegrationTests`
Initializes a new instance of the `RepositoryIntegrationTests` class, setting up an in-memory SQLite database and seeding it with test data. The test fixture ensures a clean database state for each test run.

### `Dispose`
Releases all resources used by the current test, including the in-memory SQLite database connection and any seeded test data. This method is called automatically after each test execution.

### `AddAsync_AddsProductToDatabase`
Validates that the `AddAsync` method of the repository correctly inserts a new product into the database. The test asserts that the product is retrievable immediately after insertion.

- **Parameters**: None.
- **Return value**: `Task` (asynchronous completion).
- **Throws**: Propagates any exceptions thrown by the underlying database operations.

### `GetByIdAsync_RetrievesCorrectProduct`
Ensures that the `GetByIdAsync` method retrieves the correct product from the database by its unique identifier. The test verifies that the returned product matches the expected values.

- **Parameters**: None.
- **Return value**: `Task` (asynchronous completion).
- **Throws**: `InvalidOperationException` if the product with the specified ID does not exist.

### `UpdateAsync_UpdatesProductInDatabase`
Confirms that the `UpdateAsync` method correctly modifies an existing product in the database. The test updates a product's properties and verifies that the changes are persisted.

- **Parameters**: None.
- **Return value**: `Task` (asynchronous completion).
- **Throws**: `InvalidOperationException` if the product with the specified ID does not exist.

### `DeleteAsync_RemovesProductFromDatabase`
Checks that the `DeleteAsync` method removes a product from the database. The test verifies that the product is no longer retrievable after deletion.

- **Parameters**: None.
- **Return value**: `Task` (asynchronous completion).
- **Throws**: `InvalidOperationException` if the product with the specified ID does not exist.

### `GetAllAsync_ReturnsAllProducts`
Validates that the `GetAllAsync` method retrieves all products from the database. The test asserts that the returned collection contains all seeded products.

- **Parameters**: None.
- **Return value**: `Task` (asynchronous completion).
- **Throws**: Propagates any exceptions thrown by the underlying database operations.

## Usage

### Example 1: Basic CRUD Workflow
