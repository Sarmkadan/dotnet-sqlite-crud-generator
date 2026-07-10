# HttpClientFactory

A factory class for creating and managing `HttpClient` instances with support for configuration, default headers, and JSON serialization. Designed to simplify HTTP operations in .NET applications, particularly for CRUD operations against RESTful APIs.

## API

### `public HttpClientFactory`

Initializes a new instance of the `HttpClientFactory` with optional configuration parameters. The factory manages a pool of named `HttpClient` instances and applies default settings such as base address, timeout, headers, and authorization.

### `public HttpClient CreateClient()`

Creates a new `HttpClient` instance with the current factory configuration (base address, timeout, default headers, and authorization). The returned client is independent and not tracked by the factory's internal pool.

**Returns:** A new `HttpClient` instance configured with the factory's settings.

### `public HttpClient GetOrCreateClient()`

Retrieves an existing `HttpClient` from the internal pool if available, or creates and registers a new one if not. The same client instance may be reused across calls, reducing connection overhead.

**Returns:** An `HttpClient` instance from the pool or a newly created one.

**Throws:** `InvalidOperationException` if the factory has been disposed.

### `public bool RemoveClient(string name)`

Removes a named `HttpClient` from the internal pool. If the client is currently in use, it will not be immediately disposed but will be removed from future reuse.

**Parameters:**
- `name` – The name of the client to remove.

**Returns:** `true` if the client was found and removed; otherwise, `false`.

### `public void Dispose()`

Releases all managed resources used by the factory, including all pooled `HttpClient` instances and their underlying handlers. After disposal, the factory cannot be used.

### `public string? BaseAddress`

Gets or sets the base address (URI) used as a prefix for all HTTP requests made by clients created or managed by this factory. Changing this value affects subsequent requests.

### `public TimeSpan Timeout`

Gets or sets the default timeout for HTTP requests. If not set, defaults to the system default timeout (typically 100 seconds).

### `public Dictionary<string, string>? DefaultHeaders`

Gets or sets a dictionary of default HTTP headers to be added to every request. Headers are case-insensitive and applied before any request-specific headers.

### `public string? AuthorizationScheme`

Gets or sets the authorization scheme (e.g., "Bearer", "Basic") used for requests that include an authorization token.

### `public string? AuthorizationToken`

Gets or sets the authorization token (e.g., JWT, API key) used for authenticated requests. Combined with `AuthorizationScheme` to form the `Authorization` header.

### `public HttpRequestExecutor`

Gets or sets a delegate responsible for executing HTTP requests. Allows customization of the request pipeline, such as adding middleware, logging, or retry logic.

### `public async Task<HttpResponseMessage> GetAsync(string requestUri)`

Sends a GET request to the specified URI using a client from the pool.

**Parameters:**
- `requestUri` – The URI of the request.

**Returns:** A task that represents the asynchronous operation. The task result contains the HTTP response message.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null`.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

### `public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent? content)`

Sends a POST request to the specified URI with optional content using a client from the pool.

**Parameters:**
- `requestUri` – The URI of the request.
- `content` – The content to send, or `null`.

**Returns:** A task that represents the asynchronous operation. The task result contains the HTTP response message.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null`.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

### `public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent? content)`

Sends a PUT request to the specified URI with optional content using a client from the pool.

**Parameters:**
- `requestUri` – The URI of the request.
- `content` – The content to send, or `null`.

**Returns:** A task that represents the asynchronous operation. The task result contains the HTTP response message.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null`.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

### `public async Task<HttpResponseMessage> DeleteAsync(string requestUri)`

Sends a DELETE request to the specified URI using a client from the pool.

**Parameters:**
- `requestUri` – The URI of the request.

**Returns:** A task that represents the asynchronous operation. The task result contains the HTTP response message.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null`.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

### `public async Task<T?> GetJsonAsync<T>(string requestUri)`

Sends a GET request to the specified URI and deserializes the JSON response into an object of type `T`.

**Parameters:**
- `requestUri` – The URI of the request.

**Returns:** A task that represents the asynchronous operation. The task result contains the deserialized object, or `null` if the response is empty or deserialization fails.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null`.
- `JsonException` if the response cannot be deserialized.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

### `public async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T data)`

Sends a POST request to the specified URI with JSON-serialized content and returns the response.

**Parameters:**
- `requestUri` – The URI of the request.
- `data` – The object to serialize and send as JSON.

**Returns:** A task that represents the asynchronous operation. The task result contains the HTTP response message.

**Throws:**
- `ArgumentNullException` if `requestUri` is `null` or `data` is `null`.
- `JsonException` if the data cannot be serialized.
- `HttpRequestException` if the request fails.
- `InvalidOperationException` if the factory has been disposed.

## Usage

### Example 1: Basic CRUD with JSON
