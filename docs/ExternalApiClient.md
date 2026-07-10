# ExternalApiClient

`ExternalApiClient` is a lightweight HTTP client for interacting with external RESTful APIs. It provides strongly-typed request methods for common CRUD operations and includes built-in error handling, response status inspection, and health-check capabilities. The client supports JSON serialization/deserialization and handles common HTTP status codes gracefully.

## API

### Constructors

#### `public ExternalApiClient(string baseUrl)`
Initializes a new instance of the `ExternalApiClient` with the specified base URL for API requests.

- **Parameters**
  - `baseUrl`: The base URL of the external API (e.g., `https://api.example.com/v1`).

---

### Methods

#### `public async Task<T?> GetAsync<T>(string endpoint, string? query = null)`
Sends a GET request to the specified endpoint and deserializes the JSON response into an instance of type `T`.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users/123`).
  - `query` (optional): Query string parameters to append to the URL (e.g., `?fields=id,name`).
- **Return Value**
  - `Task<T?>`: The deserialized response object of type `T`, or `null` if the response is empty or the request fails.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails or the response cannot be deserialized.

#### `public async Task<List<T>?> GetCollectionAsync<T>(string endpoint, string? query = null)`
Sends a GET request to retrieve a collection of items from the specified endpoint and deserializes the JSON response into a list of type `T`.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users`).
  - `query` (optional): Query string parameters to append to the URL.
- **Return Value**
  - `Task<List<T>?>`: A list of deserialized objects of type `T`, or `null` if the response is empty or the request fails.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails or the response cannot be deserialized.

#### `public async Task<T?> CreateAsync<T>(string endpoint, T payload)`
Sends a POST request to the specified endpoint with the provided payload and deserializes the JSON response into an instance of type `T`.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users`).
  - `payload`: The object to serialize and send as the request body.
- **Return Value**
  - `Task<T?>`: The deserialized response object of type `T`, or `null` if the response is empty or the request fails.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails or the response cannot be deserialized.

#### `public async Task<T?> UpdateAsync<T>(string endpoint, T payload)`
Sends a PUT request to the specified endpoint with the provided payload and deserializes the JSON response into an instance of type `T`.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users/123`).
  - `payload`: The object to serialize and send as the request body.
- **Return Value**
  - `Task<T?>`: The deserialized response object of type `T`, or `null` if the response is empty or the request fails.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails or the response cannot be deserialized.

#### `public async Task<bool> DeleteAsync(string endpoint)`
Sends a DELETE request to the specified endpoint and returns a boolean indicating whether the operation succeeded.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users/123`).
- **Return Value**
  - `Task<bool>`: `true` if the request returns a successful HTTP status code (2xx), otherwise `false`.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails (e.g., network error).

#### `public async Task<ApiResponse<T>> GetWithStatusAsync<T>(string endpoint, string? query = null)`
Sends a GET request to the specified endpoint and returns a structured response containing the deserialized data along with metadata about the HTTP status and pagination.

- **Parameters**
  - `endpoint`: The API endpoint path (e.g., `/users`).
  - `query` (optional): Query string parameters to append to the URL.
- **Return Value**
  - `Task<ApiResponse<T>>`: An `ApiResponse<T>` object containing:
    - `Success`: `true` if the HTTP status code is 2xx.
    - `StatusCode`: The HTTP status code from the response.
    - `Message`: A descriptive message (e.g., error details).
    - `Data`: The deserialized response object of type `T`, or `null`.
    - `Items`: A list of deserialized objects of type `T`, or `null`.
    - `Total`: The total number of items available (for paginated responses).
    - `Page`: The current page number (for paginated responses).
    - `PageSize`: The number of items per page (for paginated responses).
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails (e.g., network error).

#### `public async Task<bool> HealthCheckAsync()`
Sends a lightweight GET request to the API root or a health-check endpoint to verify connectivity and basic responsiveness.

- **Return Value**
  - `Task<bool>`: `true` if the health check returns a successful HTTP status code (2xx), otherwise `false`.
- **Exceptions**
  - Throws `ApiClientException` if the HTTP request fails (e.g., network error).

---

### Exceptions

#### `public ApiClientException(string message) : base(message)`
Initializes a new instance of the `ApiClientException` with the specified error message.

- **Parameters**
  - `message`: A string describing the error.

#### `public ApiClientException(string message, Exception innerException) : base(message, innerException)`
Initializes a new instance of the `ApiClientException` with the specified error message and inner exception.

- **Parameters**
  - `message`: A string describing the error.
  - `innerException`: The exception that is the cause of the current exception.

---

### Properties (from `ApiResponse<T>`)

#### `public bool Success`
Gets a value indicating whether the API request was successful (HTTP status code 2xx).

#### `public int StatusCode`
Gets the HTTP status code returned by the API.

#### `public string? Message`
Gets a descriptive message associated with the response (e.g., error details).

#### `public T? Data`
Gets the deserialized response object of type `T`.

#### `public List<T>? Items`
Gets the deserialized list of objects of type `T` (used for collection responses).

#### `public int Total`
Gets the total number of items available (used for paginated responses).

#### `public int Page`
Gets the current page number (used for paginated responses).

#### `public int PageSize`
Gets the number of items per page (used for paginated responses).

## Usage

### Example 1: Basic CRUD Operations
