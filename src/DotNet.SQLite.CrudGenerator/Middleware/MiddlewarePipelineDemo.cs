#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

// This file demonstrates the middleware pipeline usage patterns
// It's not part of the production code but shows how to use the new IMiddlewarePipeline

/*
Example usage:

// In Program.cs or Startup.cs:
services.AddMiddlewarePipelineServices();

// In your service class:
public class MyService
{
    private readonly IMiddlewarePipeline _pipeline;

    public MyService(IMiddlewarePipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<PipelineStepResult> ProcessRequestAsync(MyRequest request)
    {
        // Execute the middleware pipeline
        // Short-circuit behavior: If any middleware returns Success=false,
        // subsequent middleware are NOT executed
        // Error handling: Exceptions are caught and converted to PipelineStepResult
        return await _pipeline.ExecuteAsync<MyRequest, MyResponse>(request);
    }
}

// Middleware implementation example:
public class MyCustomMiddleware : IPipelineStep
{
    public async Task<PipelineStepResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        PipelineStepDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class
    {
        // Pre-processing logic
        Console.WriteLine("Before next middleware");

        // Call next middleware in pipeline
        var result = await next(request);

        // Post-processing logic (runs even if next throws, due to error handling)
        Console.WriteLine("After next middleware");

        return result;
    }
}

// Custom request/response types:
public class MyRequest { public string? Name { get; set; } }
public class MyResponse { public bool Success { get; set; } }

Pipeline semantics:
1. Middleware execute in registration order (Validation -> RateLimiting -> Logging -> ErrorHandling by default)
2. Short-circuit: When Success=false is returned, pipeline stops and returns immediately
3. Error handling: Exceptions are caught, converted to PipelineStepResult, and execution continues with remaining middleware
4. Guaranteed completion: Pipeline always returns a result, never throws

Common patterns:
- ValidationMiddleware: Returns Success=false on validation errors (short-circuits)
- RateLimitingMiddleware: Returns Success=false when rate limit exceeded (short-circuits)
- LoggingMiddleware: Always executes, logs before/after, re-throws exceptions
- ErrorHandlingMiddleware: Catches exceptions, converts to structured errors
*/
