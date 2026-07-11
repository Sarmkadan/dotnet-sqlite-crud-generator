# LoggingMiddleware

A middleware component designed to log request/response cycles in .NET applications, capturing execution outcomes and associated data for diagnostic purposes.

## API

### `LoggingMiddleware`

The middleware class that wraps and logs execution of subsequent middleware delegates.

### `public async Task<MiddlewareResult> ExecuteAsync<TRequest, TResponse>(MiddlewareDelegate<TRequest, TResponse> next)`

Executes the provided middleware delegate and logs the outcome.

- **Parameters**
  - `next`: The next middleware delegate in the pipeline to invoke.
- **Return Value**
  - A `Task<MiddlewareResult>` representing the outcome of the middleware execution, including success status and any associated data or message.
- **Exceptions**
  - Throws `ArgumentNullException` if `next` is `null`.

### `public delegate Task<MiddlewareResult> MiddlewareDelegate<TRequest, TResponse>`

Defines the signature for middleware delegates that process requests and produce responses.

- **Parameters**
  - `TRequest`: The request type.
  - `TResponse`: The response type.
- **Return Value**
  - A `Task<MiddlewareResult>` indicating the outcome of the middleware execution.

### `public bool Success`

Indicates whether the middleware execution completed successfully.

- **Value**
  - `true` if the execution completed without error; otherwise, `false`.

### `public string? Message`

Provides a human-readable message describing the outcome of the middleware execution.

- **Value**
  - A string containing additional context about the execution, or `null` if no message is available.

### `public object? Data`

Contains additional data associated with the middleware execution.

- **Value**
  - An object representing supplementary data, or `null` if no data is available.

## Usage
