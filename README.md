// existing content ...

## LoggingMiddleware

`LoggingMiddleware` is a middleware component that logs operation execution time, results, and any exceptions that occur during request processing. It provides detailed insights into application performance and operation flow by tracking:

- Request entry and exit
- Execution time measurements
- Success/failure outcomes
- Detailed request/response data (when enabled)
- Exception details and stack traces (when enabled)

The middleware can be configured to log detailed information including request payloads and stack traces for debugging purposes.

Below is a realistic example of using `LoggingMiddleware` in a request pipeline:

```csharp
// Configure services
services.AddSingleton<LoggingMiddleware>(new LoggingMiddleware(enableDetailedLogging: true));

// Use middleware in pipeline
app.Use(async (context, next) =>
{
    var middleware = context.RequestServices.GetRequiredService<LoggingMiddleware>();
    var result = await middleware.ExecuteAsync<MyRequest, MyResponse>(
        request,
        async req => 
        {
            // Your actual request handling logic here
            return new MiddlewareResult
            {
                Success = true,
                Message = "Request processed successfully",
                Data = responseData
            };
        }
    );
    
    if (!result.Success)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result.Message ?? "Request failed");
    }
});
```

## GenerateGrpcAttribute

`GenerateGrpcAttribute` is an attribute used to mark a class for gRPC service generation. It allows you to customize the generation process by specifying whether to generate async methods, CRUD operations, and streaming methods. 

Below is a realistic example of using `GenerateGrpcAttribute`:

```csharp
[GenerateGrpc(ServiceName = "MyService", GenerateAsync = true, GenerateCrud = true, GenerateStreaming = true, Namespace = "MyNamespace")]
public class MyService
{
    // Service implementation
}
```

## RateLimitingMiddleware

`RateLimitingMiddleware` is a middleware component that implements rate limiting using a sliding window algorithm to track request counts per client. It prevents abuse by limiting requests to a configured threshold per time window, providing protection against excessive API usage.

The middleware tracks request counts per client identity and automatically cleans up expired requests. It can be configured with custom request limits and time windows.

Below is a realistic example of using `RateLimitingMiddleware` in a request pipeline:

```csharp
// Configure middleware with 50 requests per 30-second window
services.AddSingleton<RateLimitingMiddleware>(new RateLimitingMiddleware(requestsPerWindow: 50, windowSeconds: 30));

// Use middleware in pipeline
app.Use(async (context, next) =>
{
    var middleware = context.RequestServices.GetRequiredService<RateLimitingMiddleware>();
    var result = await middleware.ExecuteAsync<MyRequest, MyResponse>(
        request,
        async req =>
        {
            // Your actual request handling logic here
            return new MiddlewareResult
            {
                Success = true,
                Message = "Request processed successfully",
                Data = responseData
            };
        }
    );

    if (!result.Success)
    {
        context.Response.StatusCode = 429; // Too Many Requests
        await context.Response.WriteAsync(result.Message ?? "Rate limit exceeded");
    }
});

// Monitor rate limiting statistics
var stats = middleware.GetStatistics();
Console.WriteLine($"Total clients tracked: {stats.TotalClients}");
foreach (var client in stats.ClientLimits)
{
    Console.WriteLine($"Client {client.Key}: {client.Value.RequestCount} requests, " +
                     $"Allowed: {client.Value.IsAllowed}, " +
                     $"Resets at: {client.Value.ResetTime}");
}

// Reset all rate limits (e.g., during maintenance)
middleware.ResetLimits();
```

## ValidateCommand

`ValidateCommand` validates model definitions and database schema, checking naming conventions, required attributes, and constructor presence. It reports warnings or errors and can run in a strict mode where warnings are treated as errors.

Typical usage creates an instance of the command and invokes `ExecuteAsync`. After execution you can work with `ValidationResult` objects that expose details such as the model name, property name, message, and severity. The custom `RequiredAttribute` can be applied to model properties to indicate mandatory reference‑type fields.

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        // Create the command
        var validateCommand = new ValidateCommand();

        // Run validation (no command‑line arguments in this example)
        int exitCode = await validateCommand.ExecuteAsync(Array.Empty<string>());
        Console.WriteLine($"Validation finished with exit code {exitCode}");

        // Example of inspecting a validation result
        var result = new ValidationResult
        {
            ModelName   = "User",
            PropertyName = "email",
            Message      = "Property name should start with uppercase letter",
            Severity     = ValidationSeverity.Warning
        };

        // Demonstrate the custom RequiredAttribute
        var required = new RequiredAttribute();

        Console.WriteLine(
            $"Result: {result.ModelName}.{result.PropertyName} - {result.Message} ({result.Severity})");
    }
}
```

// ... rest of README content ...
