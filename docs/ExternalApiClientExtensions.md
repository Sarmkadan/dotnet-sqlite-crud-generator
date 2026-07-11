# ExternalApiClientExtensions

`ExternalApiClientExtensions` provides a robust set of static utility methods designed to streamline interaction with external REST APIs. These extensions encapsulate common patterns such as resilient data retrieval with automatic retry mechanisms, automated pagination handling across multiple request cycles, and detailed health monitoring, ensuring reliable communication in distributed systems.

## API

### Methods

* `public static async Task<T?> GetWithRetryAsync<T>(HttpClient client, string requestUri)`
  Retrieves a single resource of type `T` from the specified URI. Implements automatic retry policies for transient HTTP failures. Returns `null` if the resource cannot be found or the request fails after all retries.

* `public static async Task<List<T>> GetCollectionWithRetryAsync<T>(HttpClient client, string requestUri)`
  Fetches a collection of objects of type `T` from the specified endpoint, utilizing retry logic to maintain data consistency during intermittent network instability.

* `public static async Task<List<T>> GetAllPagesAsync<T>(HttpClient client, string baseUri)`
  Orchestrates the retrieval of all pages for a paginated resource. This method iteratively follows pagination links from the `baseUri` and aggregates all results into a single `List<T>`.

* `public static async Task<HealthCheckResult> DetailedHealthCheckAsync(HttpClient client, string healthUri)`
  Executes a comprehensive health check against the target endpoint, returning a `HealthCheckResult` object that encapsulates the outcome, latency, and any associated error information.

### HealthCheckResult Members

The `HealthCheckResult` type, returned by `DetailedHealthCheckAsync`, exposes the following properties:

* `public bool Success`: Indicates whether the health check operation was successful.
* `public long ResponseTimeMs`: The latency of the request in milliseconds.
* `public DateTime Timestamp`: The exact timestamp of the health check execution.
* `public string? Error`: Contains error details if the operation failed (`Success` is `false`); otherwise, `null`.

## Usage

### Retrieving a Single Resource with Retries
```csharp
using System.Net.Http;

var client = new HttpClient();
var product = await ExternalApiClientExtensions.GetWithRetryAsync<Product>(client, "https://api.example.com/products/1");

if (product != null)
{
    Console.WriteLine($"Successfully retrieved product: {product.Name}");
}
```

### Performing a Detailed Health Check
```csharp
using System.Net.Http;

var client = new HttpClient();
var result = await ExternalApiClientExtensions.DetailedHealthCheckAsync(client, "https://api.example.com/health");

if (result.Success)
{
    Console.WriteLine($"Service is healthy. Response time: {result.ResponseTimeMs}ms");
}
else
{
    Console.WriteLine($"Health check failed at {result.Timestamp}. Error: {result.Error}");
}
```

## Notes

* **Thread Safety**: All methods are stateless static utilities and are inherently thread-safe, assuming they are used with thread-safe `HttpClient` instances.
* **Async Patterns**: As these methods are inherently asynchronous, they must be properly awaited to avoid potential thread starvation or deadlocks.
* **Exceptions**: While these methods handle transient failures via retry logic, they will throw `HttpRequestException` if the request fails terminally after all retry attempts are exhausted.
* **Pagination**: The `GetAllPagesAsync<T>` method assumes standard pagination patterns; ensure the API implementation strictly adheres to expected pagination headers or body structures.
