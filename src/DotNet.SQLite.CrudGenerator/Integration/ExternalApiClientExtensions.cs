#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    /// <param name="client">The API client</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="maxRetries">Maximum retry attempts (default: 3)</param>
    /// <returns>Response data or null if failed after retries</returns>
    public static async Task<T?> GetWithRetryAsync<T>(this ExternalApiClient client, string endpoint, int maxRetries = 3)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        int retryCount = 0;
        while (retryCount <= maxRetries)
        {
            try
            {
                return await client.GetAsync<T>(endpoint);
            }
            catch (ApiClientException) when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(100 * retryCount); // Exponential backoff
            }
        }

        return default;
    }

    /// <summary>
    /// Safely executes a GET collection request with automatic retry on transient failures.
    /// Returns an empty list instead of throwing on failure.
    /// </summary>
    /// <typeparam name="T">Item type in collection</typeparam>
    /// <param name="client">The API client</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 50)</param>
    /// <param name="maxRetries">Maximum retry attempts (default: 3)</param>
    /// <returns>Collection of items or empty list if failed</returns>
    public static async Task<List<T>> GetCollectionWithRetryAsync<T>(this ExternalApiClient client, string endpoint, int page = 1, int pageSize = 50, int maxRetries = 3)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        int retryCount = 0;
        while (retryCount <= maxRetries)
        {
            try
            {
                var result = await client.GetCollectionAsync<T>(endpoint, page, pageSize);
                return result ?? new List<T>();
            }
            catch (ApiClientException) when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(100 * retryCount); // Exponential backoff
            }
        }

        return new List<T>();
    }

    /// <summary>
    /// Executes a GET request and automatically handles pagination to retrieve all items.
    /// </summary>
    /// <typeparam name="T">Item type in collection</typeparam>
    /// <param name="client">The API client</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="pageSize">Items per page (default: 100)</param>
    /// <returns>Flattened list of all items across all pages</returns>
    public static async Task<List<T>> GetAllPagesAsync<T>(this ExternalApiClient client, string endpoint, int pageSize = 100)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        var allItems = new List<T>();
        int currentPage = 1;
        bool hasMore = true;

        while (hasMore)
        {
            var items = await client.GetCollectionAsync<T>(endpoint, currentPage, pageSize);

            if (items == null || items.Count == 0)
            {
                hasMore = false;
            }
            else
            {
                allItems.AddRange(items);
                currentPage++;
            }
        }

        return allItems;
    }


    /// <summary>
    /// Executes a health check with custom timeout and returns detailed status.
    /// </summary>
    /// <param name="client">The API client</param>
    /// <param name="healthEndpoint">Health check endpoint (default: "/health")</param>
    /// <param name="timeoutMilliseconds">Request timeout in milliseconds</param>
    /// <returns>Health check result with status and response time</returns>
    public static async Task<HealthCheckResult> DetailedHealthCheckAsync(this ExternalApiClient client, string healthEndpoint = "/health", int timeoutMilliseconds = 5000)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

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