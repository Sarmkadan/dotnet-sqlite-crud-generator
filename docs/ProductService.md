# ProductService

A service class that encapsulates business logic and data access for managing `Product` entities in a SQLite-backed inventory system. It provides CRUD operations, inventory analytics, and stock management features while maintaining separation between the domain model and persistence concerns.

## API

### `ProductService`
Initializes a new instance of the service with required dependencies for data access and validation.

### `async Task<Product?> GetAsync(int id)`
Retrieves a single product by its unique identifier.
- **Parameters**: `id` – The product identifier.
- **Returns**: A `Product` instance if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `id` is not positive.

### `async Task<IEnumerable<Product>> GetAllAsync()`
Retrieves all products stored in the system.
- **Returns**: An enumerable sequence of all `Product` instances.
- **Throws**: No exceptions.

### `async Task<Product> CreateAsync(Product product)`
Creates a new product in the inventory.
- **Parameters**: `product` – The product to add; must be valid.
- **Returns**: The created `Product` instance with updated identifiers.
- **Throws**: `ArgumentNullException` if `product` is `null`; `ValidationException` if the product fails validation.

### `async Task<bool> UpdateAsync(Product product)`
Updates an existing product in the inventory.
- **Parameters**: `product` – The updated product; must be valid and exist.
- **Returns**: `true` if the update succeeded; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `product` is `null`; `ValidationException` if the product fails validation.

### `async Task<bool> DeleteAsync(int id)`
Removes a product from the inventory by its identifier.
- **Parameters**: `id` – The product identifier to remove.
- **Returns**: `true` if the product existed and was deleted; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is not positive.

### `bool Validate(Product product)`
Validates a product against business rules.
- **Parameters**: `product` – The product to validate.
- **Returns**: `true` if the product is valid; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `product` is `null`.

### `async Task<bool> ExistsAsync(int id)`
Checks whether a product with the specified identifier exists.
- **Parameters**: `id` – The product identifier to check.
- **Returns**: `true` if the product exists; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is not positive.

### `Task<IEnumerable<Product>> FindAsync(string query)`
Searches products using a free-text query across name and description.
- **Parameters**: `query` – The search term; may be empty.
- **Returns**: An enumerable sequence of matching `Product` instances.
- **Throws**: `ArgumentNullException` if `query` is `null`.

### `Task<int> CountAsync()`
Returns the total number of products in the inventory.
- **Returns**: The count of products.
- **Throws**: No exceptions.

### `async Task<IEnumerable<Product>> GetByCategoryAsync(string category)`
Retrieves all products belonging to a specific category.
- **Parameters**: `category` – The category name; case-insensitive.
- **Returns**: An enumerable sequence of products in the category.
- **Throws**: `ArgumentNullException` if `category` is `null`.

### `async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)`
Retrieves products whose stock quantity is below a specified threshold.
- **Parameters**: `threshold` – The minimum stock level; must be non-negative.
- **Returns**: An enumerable sequence of low-stock products.
- **Throws**: `ArgumentException` if `threshold` is negative.

### `async Task<Product> RestockProductAsync(int id, int quantity)`
Increases the stock quantity of a product by the specified amount.
- **Parameters**:
  - `id` – The product identifier.
  - `quantity` – The amount to add; must be positive.
- **Returns**: The updated `Product` instance.
- **Throws**:
  - `ArgumentException` if `id` is not positive or `quantity` is not positive.
  - `InvalidOperationException` if the product does not exist.

### `async Task<Product> SellProductAsync(int id, int quantity)`
Decreases the stock quantity of a product by the specified amount.
- **Parameters**:
  - `id` – The product identifier.
  - `quantity` – The amount to subtract; must be positive.
- **Returns**: The updated `Product` instance.
- **Throws**:
  - `ArgumentException` if `id` is not positive or `quantity` is not positive.
  - `InvalidOperationException` if the product does not exist or stock is insufficient.

### `async Task<decimal> GetInventoryValueAsync()`
Calculates the total monetary value of all products in stock.
- **Returns**: The sum of (unit price × stock quantity) across all products.
- **Throws**: No exceptions.

### `async Task<ProductInventoryStats> GetInventoryStatsAsync()`
Aggregates key inventory metrics into a single report.
- **Returns**: A `ProductInventoryStats` object containing counts, stock levels, and total value.
- **Throws**: No exceptions.

### `int TotalProducts`
Gets the total number of products in the inventory.
- **Returns**: The count of products.
- **Throws**: No exceptions.

### `int TotalUnitsInStock`
Gets the sum of all product stock quantities.
- **Returns**: The total units available across all products.
- **Throws**: No exceptions.

### `decimal TotalInventoryValue`
Gets the total monetary value of all products in stock.
- **Returns**: The sum of (unit price × stock quantity) across all products.
- **Throws**: No exceptions.

### `int LowStockCount`
Gets the number of products currently below the low-stock threshold.
- **Returns**: The count of low-stock products.
- **Throws**: No exceptions.

## Usage
