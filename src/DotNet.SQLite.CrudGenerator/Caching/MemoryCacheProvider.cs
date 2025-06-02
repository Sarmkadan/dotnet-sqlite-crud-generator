// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotNet.SQLite.CrudGenerator.Caching;

/// <summary>
/// In-memory cache provider implementation using concurrent dictionary.
/// Supports TTL expiration, statistics tracking, and size management.
/// Thread-safe and suitable for high-concurrency scenarios.
/// </summary>
public class MemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly object _lockObject = new();
    private readonly long _maxSize;
    private long _currentSize;

    public MemoryCacheProvider(long maxSizeBytes = 10_000_000)
    {
        _maxSize = maxSizeBytes;
    }

    public ValueTask<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                Interlocked.Add(ref _currentSize, -entry.Size);
                return ValueTask.FromResult<T?>(null);
            }

            entry.LastAccessed = DateTime.UtcNow;
            entry.AccessCount++;

            return ValueTask.FromResult((T?)entry.Value);
        }

        return ValueTask.FromResult<T?>(null);
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var size = EstimateSize(value);

        if (Volatile.Read(ref _currentSize) + size > _maxSize)
            EvictLRUItems(size);

        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
            Size = size,
            LastAccessed = DateTime.UtcNow
        };

        if (_cache.TryGetValue(key, out var oldEntry))
            Interlocked.Add(ref _currentSize, -oldEntry.Size);

        _cache[key] = entry;
        Interlocked.Add(ref _currentSize, size);

        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return ValueTask.FromResult(false);

        if (_cache.TryRemove(key, out var entry))
        {
            Interlocked.Add(ref _currentSize, -entry.Size);
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask ClearAsync()
    {
        _cache.Clear();
        Volatile.Write(ref _currentSize, 0);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return ValueTask.FromResult(false);

        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                return ValueTask.FromResult(false);
            }
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(false);
    }

    public async ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
            await SetAsync(key, value, expiration);

        return value;
    }

    public CacheStatistics GetStatistics()
    {
        var stats = new CacheStatistics
        {
            TotalItems = _cache.Count,
            TotalSizeBytes = Volatile.Read(ref _currentSize),
            MaxSizeBytes = _maxSize,
            Entries = _cache.Select(kvp => new CacheEntryInfo
            {
                Key = kvp.Key,
                Size = kvp.Value.Size,
                CreatedAt = kvp.Value.CreatedAt,
                ExpiresAt = kvp.Value.ExpiresAt,
                LastAccessed = kvp.Value.LastAccessed,
                AccessCount = kvp.Value.AccessCount,
                IsExpired = kvp.Value.IsExpired
            }).ToList()
        };

        return stats;
    }

    public void CleanupExpired()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.IsExpired)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            if (_cache.TryRemove(key, out var entry))
                Interlocked.Add(ref _currentSize, -entry.Size);
        }
    }

    private void EvictLRUItems(long requiredSpace)
    {
        lock (_lockObject)
        {
            var lruItems = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .ToList();

            long freedSpace = 0;

            foreach (var item in lruItems)
            {
                if (freedSpace >= requiredSpace)
                    break;

                if (_cache.TryRemove(item.Key, out var entry))
                {
                    freedSpace += entry.Size;
                    Interlocked.Add(ref _currentSize, -entry.Size);
                }
            }
        }
    }

    // Type-based estimation: avoids Marshal.SizeOf which throws for non-blittable types.
    private static long EstimateSize(object obj) => obj switch
    {
        string s  => 50L + (long)s.Length * 2,
        byte[] b  => 50L + b.Length,
        _         => 178L,
    };

    private class CacheEntry
    {
        public object Value { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public long Size { get; set; }
        public int AccessCount { get; set; }
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
}

public interface ICacheProvider
{
    ValueTask<T?> GetAsync<T>(string key) where T : class;
    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    ValueTask<bool> RemoveAsync(string key);
    ValueTask ClearAsync();
    ValueTask<bool> ExistsAsync(string key);
    ValueTask<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
}

public class CacheStatistics
{
    public int TotalItems { get; set; }
    public long TotalSizeBytes { get; set; }
    public long MaxSizeBytes { get; set; }
    public List<CacheEntryInfo> Entries { get; set; } = new();
}

public class CacheEntryInfo
{
    public string Key { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime LastAccessed { get; set; }
    public int AccessCount { get; set; }
    public bool IsExpired { get; set; }
}
