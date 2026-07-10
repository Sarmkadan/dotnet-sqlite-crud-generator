# CacheBenchmarksExtensions

Extension methods for benchmarking cache operations in a SQLite-based CRUD generator context. These utilities measure performance characteristics of cache hits and misses, cache insertion throughput, and verify cache state consistency.

## API

### `MeasureGetHitAsync`

Measures the elapsed time for a cache hit operation on a product entity.

- **Parameters**:
  - `cache`: The cache instance implementing `ICache<Product>`.
  - `productId`: The identifier of the product to retrieve.
- **Return value**: A tuple containing:
  - `Result`: The product instance if found (`null` if not found).
  - `ElapsedMilliseconds`: The duration of the cache hit operation in milliseconds.
- **Exceptions**: Throws `ArgumentNullException` if `cache` is `null`.

### `MeasureSetAsync`

Measures the elapsed time for inserting or updating a product in the cache.

- **Parameters**:
  - `cache`: The cache instance implementing `ICache<Product>`.
  - `product`: The product to store in the cache.
- **Return value**: The duration of the cache set operation in milliseconds.
- **Exceptions**: Throws `ArgumentNullException` if `cache` or `product` is `null`.

### `VerifyCacheCountAsync`

Verifies that the cache contains the expected number of products.

- **Parameters**:
  - `cache`: The cache instance implementing `ICache<Product>`.
  - `expectedCount`: The number of products expected to be in the cache.
- **Return value**: `true` if the cache contains exactly `expectedCount` products; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `cache` is `null`.

### `CreateBenchmarkProduct`

Creates a synthetic product instance suitable for benchmarking cache operations.

- **Parameters**:
  - `id`: The identifier for the product.
  - `name`: The name of the product.
- **Return value**: A new `Product` instance with the specified `id` and `name`.
- **Exceptions**: None.

## Usage
