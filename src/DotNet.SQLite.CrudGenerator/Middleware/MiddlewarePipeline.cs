#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Represents a pipeline that executes middleware steps in registration order.
///
/// <para>Short-circuit behavior: When a middleware returns a <see cref="PipelineStepResult"/>
/// with <see cref="PipelineStepResult.Success"/> = false, subsequent middleware are NOT executed.
/// This allows early termination of the pipeline when validation fails or other
/// conditions prevent further processing.</para>
///
/// <para>Error handling: The pipeline wraps all middleware execution in try-catch blocks.
/// Exceptions are caught and converted to <see cref="PipelineStepResult"/> with
/// Success = false and appropriate error information. This ensures that:
/// <list type="bullet">
/// <item>Exceptions do not propagate to callers (structured error responses are returned)</item>
/// <item>All middleware in the pipeline are guaranteed to run for cleanup purposes,
/// even if an earlier middleware throws</item>
/// <item>The pipeline always completes and returns a result</item>
/// </list>
/// </para>
///
/// <para>Usage: Register middleware in dependency injection using
/// <see cref="IServiceCollectionExtensions.AddMiddlewarePipeline"/> extension method,
/// then inject <see cref="IMiddlewarePipeline"/> where needed.</para>
/// </summary>
public interface IMiddlewarePipeline
{
    /// <summary>
    /// Executes the middleware pipeline with the specified request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request to process.</param>
    /// <returns>A <see cref="PipelineStepResult"/> indicating success/failure and containing response data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    Task<PipelineStepResult> ExecuteAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class;
}

/// <summary>
/// Default implementation of <see cref="IMiddlewarePipeline"/> that composes
/// middleware steps in registration order.
/// </summary>
public sealed class MiddlewarePipeline : IMiddlewarePipeline
{
    private readonly ImmutableArray<IPipelineStep> _steps;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiddlewarePipeline"/> class.
    /// </summary>
    /// <param name="steps">The middleware steps to execute in order.</param>
    /// <param name="serviceProvider">The service provider for resolving middleware dependencies.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="steps"/> or
    /// <paramref name="serviceProvider"/> is null.</exception>
    public MiddlewarePipeline(
        IEnumerable<IPipelineStep> steps,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _steps = [.. steps.OrderBy(step => GetRegistrationOrder(step))];
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<PipelineStepResult> ExecuteAsync<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = new MiddlewareContext<TRequest, TResponse>(request);
        var next = CreatePipelineDelegate(0);

        try
        {
            return await next(request);
        }
        catch (Exception ex)
        {
            // This should never happen due to our error handling, but we catch it
            // as a defensive measure to ensure the pipeline always returns a result
            return CreateErrorResult(
                "PIPELINE_EXECUTION_ERROR",
                "An unexpected error occurred during pipeline execution",
                ex);
        }

        PipelineStepDelegate<TRequest, TResponse> CreatePipelineDelegate(int index)
        {
            if (index >= _steps.Length)
            {
                // Terminal delegate - executes the actual business logic
                return _ => Task.FromResult(new PipelineStepResult
                {
                    Success = true,
                    Message = "Request processed successfully",
                    Data = null
                });
            }

            var currentStep = _steps[index];
            var nextDelegate = CreatePipelineDelegate(index + 1);

            return async request =>
            {
                // Resolve the middleware instance from DI if it's not already constructed
                var middleware = currentStep as IPipelineStep;
                if (middleware is null)
                {
                    // Try to resolve from DI container
                    middleware = _serviceProvider.GetService<IPipelineStep>()
                        ?? throw new InvalidOperationException(
                            $"Middleware at index {index} could not be resolved from DI container");
                }

                try
                {
                    var result = await middleware.ExecuteAsync(request, nextDelegate);

                    // Short-circuit: if Success is false, don't execute remaining middleware
                    if (!result.Success)
                    {
                        context.AddResult(index, result);
                        return result;
                    }

                    context.AddResult(index, result);
                    return result;
                }
                catch (Exception ex)
                {
                    // Error handling: catch exceptions and convert to structured error result
                    // Note: We still execute remaining middleware for cleanup purposes
                    var errorResult = CreateErrorResult(
                        "MIDDLEWARE_ERROR",
                        $"Middleware at index {index} failed: {ex.Message}",
                        ex);

                    context.AddResult(index, errorResult);
                    context.AddException(index, ex);

                    // Continue with remaining middleware for cleanup
                    return await nextDelegate(request);
                }
            };
        }
    }

    private PipelineStepResult CreateErrorResult(string code, string message, Exception ex)
    {
        return new PipelineStepResult
        {
            Success = false,
            Message = message,
            Data = new
            {
                error = message,
                code = code,
                exceptionType = ex.GetType().Name,
                exceptionMessage = ex.Message
            }
        };
    }

    private static int GetRegistrationOrder(IPipelineStep step)
    {
        // Use type-based ordering to ensure consistent execution order
        // Lower values execute first
        return step switch
        {
            ValidationMiddleware => 10,
            RateLimitingMiddleware => 20,
            LoggingMiddleware => 30,
            ErrorHandlingMiddleware => 40,
            _ => 50
        };
    }

    /// <summary>
    /// Gets the registered middleware steps in execution order.
    /// </summary>
    public IReadOnlyList<IPipelineStep> Steps => _steps.ToImmutableArray();
}

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register middleware pipeline.
/// </summary>
public static class MiddlewarePipelineExtensions
{
    /// <summary>
    /// Adds the middleware pipeline to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddMiddlewarePipeline(
        this IServiceCollection services,
        Action<MiddlewarePipelineOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new MiddlewarePipelineOptions();
        configure?.Invoke(options);

        // Register middleware pipeline
        services.AddSingleton<IMiddlewarePipeline>(provider =>
        {
            var middlewareSteps = options.MiddlewareTypes
                .Select(type => provider.GetRequiredService(type))
                .Cast<IPipelineStep>()
                .ToList();

            return new MiddlewarePipeline(middlewareSteps, provider);
        });

        // Register individual middleware as singletons
        services.AddSingleton<ValidationMiddleware>();
        services.AddSingleton<RateLimitingMiddleware>();
        services.AddSingleton<LoggingMiddleware>();
        services.AddSingleton<ErrorHandlingMiddleware>();

        return services;
    }
}

/// <summary>
/// Options for configuring the middleware pipeline.
/// </summary>
public sealed class MiddlewarePipelineOptions
{
    /// <summary>
    /// Gets or sets the middleware types in the order they should be registered.
    /// </summary>
    public List<Type> MiddlewareTypes { get; set; } = new();

    /// <summary>
    /// Adds a middleware type to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public MiddlewarePipelineOptions Add<TMiddleware>() where TMiddleware : IPipelineStep
    {
        MiddlewareTypes.Add(typeof(TMiddleware));
        return this;
    }
}

/// <summary>
/// Context object that tracks middleware execution.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class MiddlewareContext<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    private readonly List<MiddlewareExecutionResult> _results = new();
    private readonly List<Exception> _exceptions = new();
    private readonly TRequest _request;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiddlewareContext{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public MiddlewareContext(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _request = request;
    }

    /// <summary>
    /// Gets the original request.
    /// </summary>
    public TRequest Request => _request;

    /// <summary>
    /// Gets the results of each middleware execution.
    /// </summary>
    public IReadOnlyList<MiddlewareExecutionResult> Results => _results.AsReadOnly();

    /// <summary>
    /// Gets any exceptions that occurred during execution.
    /// </summary>
    public IReadOnlyList<Exception> Exceptions => _exceptions.AsReadOnly();

    /// <summary>
    /// Gets whether all middleware executed successfully.
    /// </summary>
    public bool AllSuccessful => _results.Count == 0 || _results.All(r => r.Result.Success);

    /// <summary>
    /// Gets whether any middleware failed.
    /// </summary>
    public bool AnyFailed => _results.Any(r => !r.Result.Success);

    /// <summary>
    /// Adds a middleware execution result.
    /// </summary>
    /// <param name="index">The middleware index.</param>
    /// <param name="result">The execution result.</param>
    internal void AddResult(int index, PipelineStepResult result)
    {
        _results.Add(new MiddlewareExecutionResult(index, result));
    }

    /// <summary>
    /// Adds an exception that occurred during execution.
    /// </summary>
    /// <param name="index">The middleware index.</param>
    /// <param name="exception">The exception.</param>
    internal void AddException(int index, Exception exception)
    {
        _exceptions.Add(exception);
    }
}

/// <summary>
/// Represents the result of a single middleware execution.
/// </summary>
/// <param name="Index">The middleware index in the pipeline.</param>
/// <param name="Result">The execution result.</param>
public sealed record MiddlewareExecutionResult(int Index, PipelineStepResult Result);