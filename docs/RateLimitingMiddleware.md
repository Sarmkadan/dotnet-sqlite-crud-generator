# RateLimitingMiddleware

`RateLimitingMiddleware` is a configurable request-rate limiter designed for use in ASP.NET Core or custom middleware pipelines. It tracks per-client usage via token-bucket–style accounting, exposes statistics for monitoring, and can reject or defer requests when limits are exceeded.

## API

### `RateLimitingMiddleware`
Constructor. Initializes internal storage for client-limit tracking and sets the global reset schedule. Configuration parameters (such as maximum requests and the reset interval) are expected to be supplied at construction.

### `async Task<MiddlewareResult> ExecuteAsync<TRequest, TResponse>`
Processes a request through the rate-limiting pipeline.

- **Type parameters**  
  `TRequest` – the incoming request type.  
  `TResponse` – the outgoing response type.
- **Returns** `MiddlewareResult` indicating whether the request was allowed, rejected, or should be retried later.
- **Exceptions** Throws `ArgumentNullException` when the request context is `null`. May throw `InvalidOperationException` if the middleware has been disposed.

### `void ResetLimits`
Immediately resets all per-client counters and the global request count to zero. The next scheduled automatic reset time is recalculated.

### `RateLimitStatistics GetStatistics`
Returns a snapshot of current rate-limiting state.

- **Returns** `RateLimitStatistics` containing total clients, total requests in the current window, and the time remaining until the next automatic reset.

### `DateTime ResetTime`
Gets the UTC time at which all counters will next be automatically zeroed.

### `RateLimitBucket`
Nested or associated class representing a single client’s token bucket. Exposes:

- `bool IsAllowed` – whether the client currently has capacity for at least one request.
- `void RecordRequest` – consumes one token from the bucket.
- `TimeSpan GetRetryAfter` – the duration the client must wait before a request is likely to be allowed again.
- `int RequestCount` – number of tokens consumed in the current window.
- `DateTime ResetTime` – the UTC time when this bucket’s counters reset.

### `int TotalClients`
The number of distinct clients currently tracked.

### `Dictionary<string, ClientRateLimitInfo> ClientLimits`
A dictionary keyed by client identifier (e.g. IP address or API key) that maps to `ClientRateLimitInfo` objects. Each entry holds the client’s current bucket state and metadata.

## Usage

### Example 1: Basic integration in an ASP.NET Core pipeline

```csharp
var middleware = new RateLimitingMiddleware(
    maxRequests: 100,
    resetInterval: TimeSpan.FromMinutes(1));

app.Use(async (context, next) =>
{
    var result = await middleware.ExecuteAsync<HttpContext, object>(
        context,
        _ => next(),
        rejectedContext =>
        {
            rejectedContext.Response.StatusCode = 429;
            rejectedContext.Response.Headers["Retry-After"] =
                middleware.GetStatistics().NextResetSeconds.ToString();
            return Task.CompletedTask;
        });

    if (result == MiddlewareResult.Allowed)
        await next();
});
```

### Example 2: Manual per-client check before executing business logic

```csharp
string clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

if (!middleware.ClientLimits.TryGetValue(clientId, out var info))
{
    info = new ClientRateLimitInfo();
    middleware.ClientLimits[clientId] = info;
}

if (info.Bucket.IsAllowed)
{
    info.Bucket.RecordRequest();
    // Execute business logic
}
else
{
    var retryAfter = info.Bucket.GetRetryAfter();
    context.Response.StatusCode = 429;
    context.Response.Headers["Retry-After"] = retryAfter.TotalSeconds.ToString("F0");
}
```

## Notes

- **Thread safety**  
  `ExecuteAsync`, `RecordRequest`, and `ResetLimits` use internal locking or atomic operations. The `ClientLimits` dictionary is safe for concurrent reads but external mutation (adding/removing keys) while the middleware is processing requests must be synchronised by the caller.
- **Automatic reset**  
  The global `ResetTime` is recalculated after each manual `ResetLimits` call. Automatic resets occur on a fixed interval; a request arriving exactly at the reset boundary may be counted in either window depending on internal clock granularity.
- **Retry-after calculation**  
  `GetRetryAfter` returns the time until the bucket refills sufficiently for one request. If the bucket is completely exhausted, the value equals the remaining window duration.
- **Disposal**  
  Once disposed, calls to `ExecuteAsync` throw `InvalidOperationException`. Statistics and `ClientLimits` remain readable but no longer update.
- **Client identifiers**  
  The middleware treats the dictionary key as an opaque string. Collisions (e.g. different clients sharing a key) are the caller’s responsibility.
