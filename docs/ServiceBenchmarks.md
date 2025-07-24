# ServiceBenchmarks

A utility class providing benchmarking and performance measurement for CRUD operations against SQLite databases, primarily used to evaluate data access patterns in the `dotnet-sqlite-crud-generator` project. It exposes asynchronous methods to measure the execution time of common database operations such as retrieval, creation, update, and deletion of entities like `Product`, `User`, and `Category`.

## API

### `public async Task Setup()`
Initializes the benchmarking environment by preparing the in-memory SQLite database, schema, and seed data. This method must be called before any other public method to ensure a consistent starting state.
- **Parameters**: None
- **Return value**: `Task` (completes when setup is done)
- **Exceptions**: Throws if database initialization or seeding fails (e.g., schema conflicts, constraint violations).

---

### `public async Task<Product?> GetProductByIdAsync(int id)`
Retrieves a single product by its unique identifier and measures the operation duration.
- **Parameters**: `id` – the product identifier
- **Return value**: `Task<Product?>` – the product if found, otherwise `null`
- **Exceptions**: Throws if the database operation fails (e.g., connection issues, SQL errors).

---

### `public async Task<User?> GetUserByIdAsync(int id)`
Retrieves a single user by their unique identifier and measures the operation duration.
- **Parameters**: `id` – the user identifier
- **Return value**: `Task<User?>` – the user if found, otherwise `null`
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<Category?> GetCategoryByIdAsync(int id)`
Retrieves a single category by its unique identifier and measures the operation duration.
- **Parameters**: `id` – the category identifier
- **Return value**: `Task<Category?>` – the category if found, otherwise `null`
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<IEnumerable<Product>> GetAllProductsAsync()`
Retrieves all products from the database and measures the operation duration.
- **Parameters**: None
- **Return value**: `Task<IEnumerable<Product>>` – an enumerable of all products
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<IEnumerable<User>> GetAllUsersAsync()`
Retrieves all users from the database and measures the operation duration.
- **Parameters**: None
- **Return value**: `Task<IEnumerable<User>>` – an enumerable of all users
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<IEnumerable<Category>> GetAllCategoriesAsync()`
Retrieves all categories from the database and measures the operation duration.
- **Parameters**: None
- **Return value**: `Task<IEnumerable<Category>>` – an enumerable of all categories
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<IEnumerable<Product>> FindExpensiveProductsAsync(decimal threshold)`
Finds products with a price above the specified threshold and measures the operation duration.
- **Parameters**: `threshold` – the minimum price to filter by
- **Return value**: `Task<IEnumerable<Product>>` – an enumerable of matching products
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<IEnumerable<User>> FindUsersByEmailAsync(string emailPattern)`
Finds users whose email addresses match the given pattern (e.g., domain filter) and measures the operation duration.
- **Parameters**: `emailPattern` – a pattern to match against user emails
- **Return value**: `Task<IEnumerable<User>>` – an enumerable of matching users
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<int> CountProductsAsync()`
Counts the total number of products in the database and measures the operation duration.
- **Parameters**: None
- **Return value**: `Task<int>` – the total count of products
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<int> CountUsersAsync()`
Counts the total number of users in the database and measures the operation duration.
- **Parameters**: None
- **Return value**: `Task<int>` – the total count of users
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<bool> ExistsProductAsync(int id)`
Checks whether a product with the specified identifier exists and measures the operation duration.
- **Parameters**: `id` – the product identifier
- **Return value**: `Task<bool>` – `true` if the product exists, otherwise `false`
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<bool> ExistsUserAsync(int id)`
Checks whether a user with the specified identifier exists and measures the operation duration.
- **Parameters**: `id` – the user identifier
- **Return value**: `Task<bool>` – `true` if the user exists, otherwise `false`
- **Exceptions**: Throws if the database operation fails.

---
### `public async Task<Product> CreateProductAsync(Product product)`
Inserts a new product into the database and measures the operation duration.
- **Parameters**: `product` – the product to create
- **Return value**: `Task<Product>` – the created product (with updated identity)
- **Exceptions**: Throws if the insertion fails (e.g., constraint violations, invalid data).

---
### `public async Task<User> CreateUserAsync(User user)`
Inserts a new user into the database and measures the operation duration.
- **Parameters**: `user` – the user to create
- **Return value**: `Task<User>` – the created user (with updated identity)
- **Exceptions**: Throws if the insertion fails.

---
### `public async Task<bool> UpdateProductAsync(Product product)`
Updates an existing product in the database and measures the operation duration.
- **Parameters**: `product` – the product with updated values
- **Return value**: `Task<bool>` – `true` if the update affected exactly one row, otherwise `false`
- **Exceptions**: Throws if the update fails (e.g., concurrency issues, invalid data).

---
### `public async Task<bool> UpdateUserAsync(User user)`
Updates an existing user in the database and measures the operation duration.
- **Parameters**: `user` – the user with updated values
- **Return value**: `Task<bool>` – `true` if the update affected exactly one row, otherwise `false`
- **Exceptions**: Throws if the update fails.

---
### `public async Task<bool> DeleteProductAsync(int id)`
Deletes a product by its identifier and measures the operation duration.
- **Parameters**: `id` – the product identifier
- **Return value**: `Task<bool>` – `true` if the deletion affected exactly one row, otherwise `false`
- **Exceptions**: Throws if the deletion fails (e.g., foreign key violations).

---
### `public async Task<bool> DeleteUserAsync(int id)`
Deletes a user by their identifier and measures the operation duration.
- **Parameters**: `id` – the user identifier
- **Return value**: `Task<bool>` – `true` if the deletion affected exactly one row, otherwise `false`
- **Exceptions**: Throws if the deletion fails.

---
### `public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)`
Retrieves all products belonging to the specified category and measures the operation duration.
- **Parameters**: `categoryId` – the category identifier
- **Return value**: `Task<IEnumerable<Product>>` – an enumerable of matching products
- **Exceptions**: Throws if the database operation fails.

## Usage

### Example 1: Benchmarking CRUD operations
