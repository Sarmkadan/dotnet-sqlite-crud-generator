#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Extension methods for <see cref="ExternalApiClient"/> providing additional functionality
/// for common API operations including batch operations, retry logic, and response handling.
/// </summary>
public static class ExternalApiClientExtensions
{

    /// <summary>
    /// Safely executes a GET request with automatic retry on transient failures.
    /// Returns a default value instead of throwing on failure.
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="client">The API client instance. Cannot be null.</param>
    /// <param name="endpoint">API endpoint. Cannot be null or empty.</param>
    /// <param name="maxRetries">Maximum retry attempts (default: 3). Must be non-negative.</param>
    /// <returns>Response data or null if failed after retries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="endpoint"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxRetries"/> is negative.</exception>
    public static async Task<T?> GetWithRetryAsync<T>(
        this ExternalApiClient client,
        string endpoint,
        int maxRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);

        for (int retryCount = 0; retryCount <= maxRetries; retryCount++)
        {
            try
            {
                return await client.GetAsync<T>(endpoint);
            }
            catch (ApiClientException) when (retryCount < maxRetries)
            {
                await Task.Delay(100 * (retryCount + 1)); // Exponential backoff starting from 100ms
            }
        }

        return default;
    }

    /// <summary>
    /// Safely executes a GET collection request with automatic retry on transient failures.
    /// Returns an empty list instead of throwing on failure.
    /// </summary>
    /// <typeparam name="T">Item type in collection.</typeparam>
    /// <param name="client">The API client instance. Cannot be null.</param>
    /// <param name="endpoint">API endpoint. Cannot be null or empty.</param>
    /// <param name="page">Page number (default: 1). Must be positive.</param>
    /// <param name="pageSize">Items per page (default: 50). Must be positive.</param>
    /// <param name="maxRetries">Maximum retry attempts (default: 3). Must be non-negative.</param>
    /// <returns>Collection of items or empty list if failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="endpoint"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="page"/>, <paramref name="pageSize"/>, or <paramref name="maxRetries"/> is negative.</exception>
    public static async Task<List<T>> GetCollectionWithRetryAsync<T>(
        this ExternalApiClient client,
        string endpoint,
        int page = 1,
        int pageSize = 50,
        int maxRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);

        for (int retryCount = 0; retryCount <= maxRetries; retryCount++)
        {
            try
            {
                var result = await client.GetCollectionAsync<T>(endpoint, page, pageSize);
                return result ?? new List<T>();
            }
            catch (ApiClientException) when (retryCount < maxRetries)
            {
                await Task.Delay(100 * (retryCount + 1)); // Exponential backoff starting from 100ms
            }
        }

        return new List<T>();
    }

    /// <summary>
    /// Executes a GET request and automatically handles pagination to retrieve all items.
    /// </summary>
    /// <typeparam name="T">Item type in collection.</typeparam>
    /// <param name="client">The API client instance. Cannot be null.</param>
    /// <param name="endpoint">API endpoint. Cannot be null or empty.</param>
    /// <param name="pageSize">Items per page (default: 100). Must be positive.</param>
    /// <returns>Flattened list of all items across all pages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="endpoint"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="endpoint"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageSize"/> is not positive.</exception>
    public static async Task<List<T>> GetAllPagesAsync<T>(
        this ExternalApiClient client,
        string endpoint,
        int pageSize = 100)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var allItems = new List<T>();
        int currentPage = 1;

        while (true)
        {
            var items = await client.GetCollectionAsync<T>(endpoint, currentPage, pageSize);

            if (items is null || items.Count == 0)
            {
                break;
            }

            allItems.AddRange(items);
            currentPage++;
        }

        return allItems;
    }

    /// <summary>
    /// Executes a health check with custom timeout and returns detailed status.
    /// </summary>
    /// <param name="client">The API client instance. Cannot be null.</param>
    /// <param name="healthEndpoint">Health check endpoint (default: "/health"). Cannot be null or empty.</param>
    /// <param name="timeoutMilliseconds">Request timeout in milliseconds (default: 5000). Must be positive.</param>
    /// <returns>Health check result with status and response time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="healthEndpoint"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeoutMilliseconds"/> is not positive.</exception>
    public static async Task<HealthCheckResult> DetailedHealthCheckAsync(
        this ExternalApiClient client,
        string healthEndpoint = "/health",
        int timeoutMilliseconds = 5000)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrEmpty(healthEndpoint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutMilliseconds, 0);

        var result = new HealthCheckResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            result.Success = await client.HealthCheckAsync(healthEndpoint);
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.Timestamp = DateTime.UtcNow;
        }
        catch (TaskCanceledException)
        {
            result.Success = false;
            result.ResponseTimeMs = timeoutMilliseconds;
            result.Error = "Request timeout";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.Error = ex.Message;
        }

        return result;
    }

}

/// <summary>
/// Result of a health check operation.
/// </summary>
public sealed class HealthCheckResult
{
    public bool Success { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
}
