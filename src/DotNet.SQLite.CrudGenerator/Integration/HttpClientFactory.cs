#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Factory for creating and managing HttpClient instances.
/// Implements a connection pool pattern and provides retry policies.
/// Supports custom headers, authentication, and timeout configuration.
/// </summary>
public sealed class HttpClientFactory
{
    private readonly Dictionary<string, HttpClient> _clients = new();
    private readonly HttpClientHandler _defaultHandler;
    private readonly object _lockObject = new();

    public HttpClientFactory(int connectionLimit = 10)
    {
        _defaultHandler = new HttpClientHandler
        {
            MaxConnectionsPerServer = connectionLimit,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
    }

    public HttpClient CreateClient(string name = "default", HttpClientOptions? options = null)
    {
        lock (_lockObject)
        {
            if (_clients.TryGetValue(name, out var existingClient))
                return existingClient;

            var client = new HttpClient(_defaultHandler, disposeHandler: false)
            {
                Timeout = options?.Timeout ?? TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrEmpty(options?.BaseAddress))
                client.BaseAddress = new Uri(options.BaseAddress);

            if (options?.DefaultHeaders is not null)
            {
                foreach (var header in options.DefaultHeaders)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            client.DefaultRequestHeaders.Add("User-Agent", "DotNet.SQLite.CrudGenerator/1.0");

            _clients[name] = client;
            return client;
        }
    }

    public HttpClient GetOrCreateClient(string name, HttpClientOptions? options = null)
    {
        return CreateClient(name, options);
    }

    public bool RemoveClient(string name)
    {
        lock (_lockObject)
        {
            // Fix: Replaced invalid TryRemove with TryGetValue and Remove for thread safety under lock
            if (_clients.TryGetValue(name, out var client))
            {
                _clients.Remove(name);
                client?.Dispose();
                return true;
            }
        }

        return false;
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            foreach (var client in _clients.Values)
                client?.Dispose();

            _clients.Clear();
            _defaultHandler?.Dispose();
        }
    }
}

public sealed class HttpClientOptions
{
    public string? BaseAddress { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, string>? DefaultHeaders { get; set; }
    public string? AuthorizationScheme { get; set; }
    public string? AuthorizationToken { get; set; }
}

/// <summary>
/// Wrapper for making HTTP requests with retry and error handling.
/// </summary>
public sealed class HttpRequestExecutor
{
    private readonly HttpClient _httpClient;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryDelay;

    public HttpRequestExecutor(HttpClient httpClient, int maxRetries = 3, int retryDelayMs = 1000)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _maxRetries = maxRetries;
        _retryDelay = TimeSpan.FromMilliseconds(retryDelayMs);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await ExecuteWithRetryAsync(() => _httpClient.GetAsync(url));
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        return await ExecuteWithRetryAsync(() => _httpClient.PostAsync(url, content));
    }

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
    {
        return await ExecuteWithRetryAsync(() => _httpClient.PutAsync(url, content));
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await ExecuteWithRetryAsync(() => _httpClient.DeleteAsync(url));
    }

    public async Task<T?> GetJsonAsync<T>(string url)
    {
        try
        {
            var response = await GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(content);
            }

            return default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return await PostAsync(url, content);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to serialize JSON for POST request: {ex.Message}", ex);
        }
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> requestFunc)
    {
        HttpResponseMessage? lastResponse = null;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var response = await requestFunc();

                if (response.IsSuccessStatusCode || attempt == _maxRetries)
                    return response;

                lastResponse = response;

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    (int)response.StatusCode >= 500)
                {
                    if (attempt < _maxRetries)
                        await Task.Delay(_retryDelay);
                }
                else
                {
                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < _maxRetries)
                    await Task.Delay(_retryDelay);
            }
        }

        if (lastException is not null)
            throw lastException;

        return lastResponse ?? throw new HttpRequestException("Request failed and no response was returned");
    }
}
