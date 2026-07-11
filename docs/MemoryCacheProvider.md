# MemoryCacheProvider

`MemoryCacheProvider` is an in-process, time-aware cache implementation backed by a concurrent dictionary. It stores arbitrary objects keyed by string, tracks creation time, optional expiration, last access time, and estimated size per entry, and exposes both individual item operations and aggregate cache statistics. It is designed for use in scenarios where a fast, thread-safe, non-distributed cache with explicit lifetime management and size monitoring is required.

## API

### public MemoryCacheProvider

Constructor. Creates a new empty cache instance with no predefined capacity limit. All statistics counters are initialized to zero.

### public ValueTask<T?> GetAsync<T>

Retrieves a cached value by its string key, cast to type `T`. Returns `null` if the key is not present, the entry has expired, or the stored value is not assignable to `T`. Updates the entry’s `LastAccessed` timestamp and increments `AccessCount` on a successful hit.

- **Parameters:** `string key` — the cache key to look up.
- **Returns:** `ValueTask<T?>` — the cached object or `null`.
- **Throws:** No exceptions are thrown; type mismatches and misses return `null` silently.

### public ValueTask SetAsync<T>

Stores a value under the given key with an optional absolute expiration time. If the key already exists, the existing entry is overwritten (creation time resets, access count resets to zero). The entry’s size is estimated by the cache implementation (e.g., via serialization or reflection-based approximation) and contributes to `TotalSizeBytes`.

- **Parameters:**
  - `string key` — the key to store under.
  - `T value` — the object to cache.
  - `DateTime? expiresAt` — optional absolute expiration time in UTC; `null` means no expiration.
- **Returns:** `ValueTask` — completes when the entry has been inserted or replaced.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### public ValueTask<bool> RemoveAsync

Removes the entry with the specified key from the cache. Returns `true` if the key existed and was removed; returns `false` if the key was not present. Size counters are decremented accordingly.

- **Parameters:** `string key` — the key to remove.
- **Returns:** `ValueTask<bool>` — `true` if removed, `false` if not found.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### public ValueTask ClearAsync

Removes all entries from the cache and resets all aggregate statistics to zero (total items, total size, access counts). The operation is atomic with respect to the internal data structure.

- **Returns:** `ValueTask` — completes when the cache is empty.
- **Throws:** No exceptions thrown.

### public ValueTask<bool> ExistsAsync

Checks whether a key exists in the cache and has not expired. Does not update `LastAccessed` or `AccessCount`. Returns `false` for expired entries even if they have not yet been physically evicted.

- **Parameters:** `string key` — the key to check.
- **Returns:** `ValueTask<bool>` — `true` if a non-expired entry exists, `false` otherwise.
- **Throws:** `ArgumentNullException` if `key` is `null`.

### public async ValueTask<T?> GetOrSetAsync<T>

Atomically retrieves a value by key, or creates and stores it if absent or expired. Accepts a factory delegate that produces the value and an optional expiration. If the key is missing or expired, the factory is invoked, the result is stored via `SetAsync`, and then returned. If the key exists and is valid, behaves identically to `GetAsync<T>`.

- **Parameters:**
  - `string key` — the cache key.
  - `Func<ValueTask<T>> factory` — asynchronous factory that produces the value when needed.
  - `DateTime? expiresAt` — optional expiration assigned to a newly created entry.
- **Returns:** `ValueTask<T?>` — the existing or newly created value, or `null` if the factory returns `null`.
- **Throws:** Exceptions from the factory propagate to the caller. `ArgumentNullException` if `key` or `factory` is `null`.

### public CacheStatistics GetStatistics

Returns a snapshot of current cache metrics. The returned `CacheStatistics` object is a point-in-time copy and does not reflect subsequent changes.

- **Returns:** `CacheStatistics` — contains `TotalItems`, `TotalSizeBytes`, `MaxSizeBytes`, and aggregate hit/miss counters.
- **Throws:** No exceptions thrown.

### public void CleanupExpired

Iterates over all cache entries and physically removes those whose `ExpiresAt` timestamp is in the past. This is a synchronous, blocking operation that locks the internal collection for the duration of the scan. Intended for periodic maintenance rather than per-request use.

- **Throws:** No exceptions thrown; expired entries are silently discarded.

### public object Value

Gets the raw stored object for a cache entry when accessed through an `CacheEntryInfo` instance. This member is exposed on `CacheEntryInfo`, not directly on the provider.

- **Type:** `object` — the cached value, may be `null` if the entry was stored with a `null` value.

### public DateTime CreatedAt

The UTC timestamp when the entry was first inserted or last overwritten via `SetAsync`.

### public DateTime? ExpiresAt

The absolute UTC expiration time assigned to the entry, or `null` if the entry has no expiration.

### public DateTime LastAccessed

The UTC timestamp of the most recent successful `GetAsync` or `GetOrSetAsync` hit on this entry. Not updated by `ExistsAsync`.

### public long Size

The estimated size in bytes of this entry’s serialized or reflected value. Used in `TotalSizeBytes` aggregation.

### public int AccessCount

The number of times this entry has been successfully retrieved via `GetAsync` or `GetOrSetAsync`. Reset to zero on overwrite.

### public int TotalItems

The current number of non-expired entries in the cache. Part of `CacheStatistics`.

### public long TotalSizeBytes

The sum of `Size` for all non-expired entries currently in the cache. Part of `CacheStatistics`.

### public long MaxSizeBytes

A configurable soft limit on `TotalSizeBytes`. The default implementation does not enforce eviction based on this value; it is provided for monitoring and external enforcement logic.

### public List<CacheEntryInfo> Entries

Returns a snapshot list of `CacheEntryInfo` objects representing all entries currently in the cache, including expired entries that have not yet been cleaned up by `CleanupExpired`. Each `CacheEntryInfo` exposes `Key`, `Value`, `CreatedAt`, `ExpiresAt`, `LastAccessed`, `Size`, and `AccessCount`.

### public string Key

The string key of a cache entry, exposed on `CacheEntryInfo` instances returned by `Entries`.

## Usage

### Example 1: Basic get, set, and expiration

```csharp
var cache = new MemoryCacheProvider();

// Store a value with a 5-minute expiration
await cache.SetAsync("user:42", new { Name = "Alice" }, DateTime.UtcNow.AddMinutes(5));

// Retrieve it
var user = await cache.GetAsync<dynamic>("user:42");
Console.WriteLine(user?.Name); // "Alice"

// After expiration, the entry is treated as missing
await Task.Delay(TimeSpan.FromMinutes(6));
var expired = await cache.GetAsync<dynamic>("user:42");
Console.WriteLine(expired == null); // True
```

### Example 2: Get-or-set pattern with statistics

```csharp
var cache = new MemoryCacheProvider();

async Task<Order> LoadOrderAsync(string orderId)
{
    return await cache.GetOrSetAsync<Order>(
        $"order:{orderId}",
        async () => await FetchFromDatabaseAsync(orderId),
        DateTime.UtcNow.AddMinutes(10)
    );
}

// First call fetches from database and caches
var order1 = await LoadOrderAsync("ORD-001");

// Second call within 10 minutes returns cached value
var order2 = await LoadOrderAsync("ORD-001");

// Inspect cache state
var stats = cache.GetStatistics();
Console.WriteLine($"Items: {stats.TotalItems}, Size: {stats.TotalSizeBytes} bytes");

// Periodic cleanup
cache.CleanupExpired();
```

## Notes

- **Thread safety:** All public `ValueTask`-returning methods (`GetAsync`, `SetAsync`, `RemoveAsync`, `ClearAsync`, `ExistsAsync`, `GetOrSetAsync`) are safe to call concurrently from multiple threads. The underlying storage uses fine-grained locking or a concurrent collection to ensure consistency. `CleanupExpired` and `GetStatistics` take a broader lock and may block other operations briefly.
- **Expiration handling:** Expired entries are not automatically evicted on access by default. `GetAsync` and `GetOrSetAsync` filter them out at read time, but the stale entry remains in the internal collection and in `Entries` until `CleanupExpired` or `RemoveAsync` is called. This means `TotalItems` and `TotalSizeBytes` may include expired entries that have not been physically removed.
- **Size estimation:** The `Size` property per entry is an estimate. The accuracy depends on the internal sizing strategy (e.g., JSON serialization length, reflection-based field summation). Do not rely on it for precise memory accounting.
- **`MaxSizeBytes` enforcement:** The provider tracks `MaxSizeBytes` as a configurable field, but the default implementation does not perform automatic eviction when the limit is exceeded. Callers must monitor `TotalSizeBytes` and invoke `RemoveAsync` or `CleanupExpired` as needed.
- **`GetOrSetAsync` atomicity:** The factory delegate is invoked at most once per key miss. Concurrent callers for the same missing key will block on the first caller’s factory execution and then receive the same stored result, preventing cache stampedes.
- **`Entries` snapshot cost:** Accessing `Entries` creates a copy of the current entry list. On large caches, this allocates a list proportional to the total entry count (including expired entries). Avoid calling it on hot paths.
- **Null values:** Storing `null` via `SetAsync` is permitted. `GetAsync<T>` returns `null` both for missing keys and for keys whose stored value is `null`. Use `ExistsAsync` to distinguish between the two cases if necessary.
