#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Caching;
using DotNet.SQLite.CrudGenerator.Configuration;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Integration tests for the <see cref="MemoryCacheProvider"/> caching implementation.
/// Tests verify that the cache provider correctly stores, retrieves, expires, and manages items
/// according to the configured eviction policy and TTL settings.
/// </summary>
public sealed class CachingIntegrationTests : IDisposable
{
	private MemoryCacheProvider _cacheProvider;
	private CacheConfiguration _cacheConfiguration;

	public CachingIntegrationTests()
	{
		_cacheConfiguration = new CacheConfiguration
		{
			Enabled = true,
			MaxSizeBytes = 1024 * 10, // 10 KB for testing eviction
			DefaultTTL = TimeSpan.FromMinutes(1)
		};
		_cacheProvider = new MemoryCacheProvider(_cacheConfiguration.MaxSizeBytes);
	}

	/// <summary>
	/// Cleans up the cache by clearing all items after each test.
	/// </summary>
	public void Dispose()
	{
		_cacheProvider.ClearAsync().GetAwaiter().GetResult();
	}

	/// <summary>
	/// Tests that items can be stored in the cache and retrieved successfully.
	/// </summary>
	[Fact]
	public async Task SetAndGetAsync_ShouldCacheAndRetrieveItem()
	{
		// Arrange
		var key = "testKey";
		var value = "testValue";

		// Act
		await _cacheProvider.SetAsync(key, value);
		var retrievedValue = await _cacheProvider.GetAsync<string>(key);

		// Assert
		retrievedValue.Should().Be(value);
	}

	/// <summary>
	/// Tests that items with explicit expiration are automatically removed from cache
	/// when the expiration time elapses.
	/// </summary>
	[Fact]
	public async Task SetAsync_WithExpiration_ShouldExpireItem()
	{
		// Arrange
		var key = "expiringKey";
		var value = "expiringValue";
		var expiration = TimeSpan.FromMilliseconds(100);

		// Act
		await _cacheProvider.SetAsync(key, value, expiration);
		var retrievedValueBeforeExpiration = await _cacheProvider.GetAsync<string>(key);
		await Task.Delay(expiration + TimeSpan.FromMilliseconds(50)); // Wait for expiration
		var retrievedValueAfterExpiration = await _cacheProvider.GetAsync<string>(key);

		// Assert
		retrievedValueBeforeExpiration.Should().Be(value);
		retrievedValueAfterExpiration.Should().BeNull();
	}

	/// <summary>
	/// Tests that items can be explicitly removed from the cache.
	/// </summary>
	[Fact]
	public async Task RemoveAsync_ShouldRemoveItem()
	{
		// Arrange
		var key = "removeKey";
		var value = "removeValue";
		await _cacheProvider.SetAsync(key, value);

		// Act
		var removed = await _cacheProvider.RemoveAsync(key);
		var retrievedValue = await _cacheProvider.GetAsync<string>(key);

		// Assert
		removed.Should().BeTrue();
		retrievedValue.Should().BeNull();
	}

	/// <summary>
	/// Tests that all items can be cleared from the cache in a single operation.
	/// </summary>
	[Fact]
	public async Task ClearAsync_ShouldClearAllItems()
	{
		// Arrange
		await _cacheProvider.SetAsync("key1", "value1");
		await _cacheProvider.SetAsync("key2", "value2");

		// Act
		await _cacheProvider.ClearAsync();
		var item1 = await _cacheProvider.GetAsync<string>("key1");
		var item2 = await _cacheProvider.GetAsync<string>("key2");

		// Assert
		item1.Should().BeNull();
		item2.Should().BeNull();
		var stats = _cacheProvider.GetStatistics();
		stats.TotalItems.Should().Be(0);
	}

	/// <summary>
	/// Tests that the cache correctly identifies when an item exists.
	/// </summary>
	[Fact]
	public async Task ExistsAsync_ShouldReturnTrueForExistingItem()
	{
		// Arrange
		var key = "existsKey";
		await _cacheProvider.SetAsync(key, "existsValue");

		// Act
		var exists = await _cacheProvider.ExistsAsync(key);

		// Assert
		exists.Should().BeTrue();
	}

	/// <summary>
	/// Tests that the cache correctly identifies when an item does not exist.
	/// </summary>
	[Fact]
	public async Task ExistsAsync_ShouldReturnFalseForNonExistingItem()
	{
		// Arrange
		var key = "nonExistentKey";

		// Act
		var exists = await _cacheProvider.ExistsAsync(key);

		// Assert
		exists.Should().BeFalse();
	}

	/// <summary>
	/// Tests that <see cref="MemoryCacheProvider.GetOrSetAsync"/> returns the cached value
	/// when the item already exists in the cache, without invoking the factory function.
	/// </summary>
	[Fact]
	public async Task GetOrSetAsync_ShouldReturnCachedValueWhenExists()
	{
		// Arrange
		var key = "getOrSetKey";
		var initialValue = "initial";
		await _cacheProvider.SetAsync(key, initialValue);
		var factoryCalled = false;
		Func<Task<string>> factory = () =>
		{
			factoryCalled = true;
			return Task.FromResult("new");
		};

		// Act
		var retrievedValue = await _cacheProvider.GetOrSetAsync(key, factory);

		// Assert
		retrievedValue.Should().Be(initialValue);
		factoryCalled.Should().BeFalse();
	}

	/// <summary>
	/// Tests that <see cref="MemoryCacheProvider.GetOrSetAsync"/> invokes the factory function
	/// to create a new value when the item does not exist in the cache, then caches the result.
	/// </summary>
	[Fact]
	public async Task GetOrSetAsync_ShouldFactoryNewValueWhenNotExists()
	{
		// Arrange
		var key = "newGetOrSetKey";
		var factoryValue = "factoryValue";
		var factoryCalled = false;
		Func<Task<string>> factory = () =>
		{
			factoryCalled = true;
			return Task.FromResult(factoryValue);
		};

		// Act
		var retrievedValue = await _cacheProvider.GetOrSetAsync(key, factory);

		// Assert
		retrievedValue.Should().Be(factoryValue);
		factoryCalled.Should().BeTrue();
		var cachedValue = await _cacheProvider.GetAsync<string>(key);
		cachedValue.Should().Be(factoryValue);
	}

	/// <summary>
	/// Helper class used to test cache eviction policy with large objects.
	/// </summary>
	private class LargeObject
	{
		public byte[] Data { get; set; }
		public LargeObject(int size) => Data = new byte[size];
	}

	/// <summary>
	/// Tests that the LRU (Least Recently Used) eviction policy correctly removes
	/// the least recently accessed items when the cache reaches its size limit.
	/// </summary>
	[Fact]
	public async Task EvictionPolicy_ShouldRemoveLeastRecentlyUsedItemsWhenCacheFull()
	{
		// This test needs a cache capped tightly enough to force eviction with
		// only 3 small items, unlike the shared 10 KB fixture cache.
		var cacheProvider = new MemoryCacheProvider(1000);

		// Add 3 items, each 400 bytes, total 1200 bytes. Cache should evict oldest.
		var item1 = new LargeObject(400);
		var item2 = new LargeObject(400);
		var item3 = new LargeObject(400);

		await cacheProvider.SetAsync("key1", item1); // Size: 400. Current: 400
		await Task.Delay(10); // Ensure LastAccessed differs
		await cacheProvider.SetAsync("key2", item2); // Size: 400. Current: 800
		await Task.Delay(10); // Ensure LastAccessed differs
		await cacheProvider.GetAsync<LargeObject>("key1"); // Access key1 to make it recently used

		// Act
		// Adding item3 (400 bytes) should cause eviction because 800 + 400 = 1200 > 1000
		// key2 should be evicted (LRU among non-accessed)
		await cacheProvider.SetAsync("key3", item3); // Size: 400. Current: 800 (key2 evicted)

		// Assert
		var stats = cacheProvider.GetStatistics();
		stats.TotalItems.Should().Be(2); // Should have 2 items: key1 (accessed) and key3 (new)

		(await cacheProvider.ExistsAsync("key1")).Should().BeTrue();
		(await cacheProvider.ExistsAsync("key2")).Should().BeFalse(); // key2 should be evicted
		(await cacheProvider.ExistsAsync("key3")).Should().BeTrue();
		stats.TotalSizeBytes.Should().Be(800);
	}

	/// <summary>
	/// Tests that <see cref="MemoryCacheProvider.GetStatistics"/> returns accurate statistics
	/// about the current state of the cache, including item count, sizes, and access counts.
	/// </summary>
	[Fact]
	public async Task GetStatistics_ShouldReturnCorrectStats()
	{
		// Arrange
		await _cacheProvider.ClearAsync();
		var key1 = "statKey1";
		var value1 = "value1";
		var key2 = "statKey2";
		var value2 = "value2";

		await _cacheProvider.SetAsync(key1, value1, TimeSpan.FromMinutes(1));
		await Task.Delay(10);
		await _cacheProvider.SetAsync(key2, value2);
		await _cacheProvider.GetAsync<string>(key1); // Access key1 once

		// Act
		var stats = _cacheProvider.GetStatistics();

		// Assert
		stats.Should().NotBeNull();
		stats.TotalItems.Should().Be(2);
		stats.MaxSizeBytes.Should().Be(_cacheConfiguration.MaxSizeBytes);
		stats.TotalSizeBytes.Should().BeGreaterThan(0); // Should be approximate based on string sizes

		var entry1 = stats.Entries.FirstOrDefault(e => e.Key == key1);
		entry1.Should().NotBeNull();
		entry1!.AccessCount.Should().Be(1); // One get call + the set itself, depends on internal implementation.
		// The GetAsync in this test increments access count.
		entry1.IsExpired.Should().BeFalse();

		var entry2 = stats.Entries.FirstOrDefault(e => e.Key == key2);
		entry2.Should().NotBeNull();
		entry2!.AccessCount.Should().Be(0); // No get call for key2
		entry2.IsExpired.Should().BeFalse();
	}
}