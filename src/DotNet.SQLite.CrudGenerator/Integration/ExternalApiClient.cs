#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Client for communicating with external REST APIs.
/// Provides type-safe methods for CRUD operations with built-in error handling.
/// Supports pagination, filtering, and custom headers.
/// </summary>
public sealed class ExternalApiClient
{
    private readonly HttpRequestExecutor _executor;
    private readonly string _baseUrl;

    public ExternalApiClient(HttpClient httpClient, string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));

        _baseUrl = baseUrl.TrimEnd('/');
        _executor = new HttpRequestExecutor(httpClient, maxRetries: 3);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var url = CombineUrl(endpoint);
            return await _executor.GetJsonAsync<T>(url);
        }
        catch (Exception ex)
        {
            throw new ApiClientException($"Failed to GET from {endpoint}", ex);
        }
    }

    public async Task<List<T>?> GetCollectionAsync<T>(string endpoint, int page = 1, int pageSize = 50)
    {
        try
        {
            var url = CombineUrl(endpoint);
            url += $"?page={page}&pageSize={pageSize}";

            var response = await _executor.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var wrapper = JsonSerializer.Deserialize<PaginatedResponse<T>>(content);
                return wrapper?.Items?.ToList();
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new ApiClientException($"Failed to GET collection from {endpoint}", ex);
        }
    }

    public async Task<T?> CreateAsync<T>(string endpoint, object data)
    {
        try
        {
            var url = CombineUrl(endpoint);
            var response = await _executor.PostJsonAsync(url, data);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content);
            }

            throw new ApiClientException($"API returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new ApiClientException($"Failed to POST to {endpoint}", ex);
        }
    }

    public async Task<T?> UpdateAsync<T>(string endpoint, object data)
    {
        try
        {
            var url = CombineUrl(endpoint);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _executor.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent);
            }

            throw new ApiClientException($"API returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new ApiClientException($"Failed to PUT to {endpoint}", ex);
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var url = CombineUrl(endpoint);
            var response = await _executor.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new ApiClientException($"Failed to DELETE from {endpoint}", ex);
        }
    }

    public async Task<ApiResponse<T>> GetWithStatusAsync<T>(string endpoint)
    {
        try
        {
            var url = CombineUrl(endpoint);
            var response = await _executor.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            var data = response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<T>(content)
                : default;

            return new ApiResponse<T>
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Data = data,
                Message = response.ReasonPhrase
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 0,
                Message = ex.Message,
                Data = default
            };
        }
    }

    public async Task<bool> HealthCheckAsync(string healthEndpoint = "/health")
    {
        try
        {
            var response = await _executor.GetAsync(CombineUrl(healthEndpoint));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string CombineUrl(string endpoint)
    {
        if (endpoint.StartsWith("/"))
            return _baseUrl + endpoint;

        return _baseUrl + "/" + endpoint;
    }
}

public sealed class ApiClientException : Exception
{
    public ApiClientException(string message) : base(message) { }
    public ApiClientException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public sealed class PaginatedResponse<T>
{
    public List<T>? Items { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
