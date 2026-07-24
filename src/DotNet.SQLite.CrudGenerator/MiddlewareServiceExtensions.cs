#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering middleware pipeline services in the dependency injection container.
/// </summary>
public static class MiddlewareServiceExtensions
{
    /// <summary>
    /// Adds middleware pipeline services to the service collection with default middleware configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddMiddlewarePipelineServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddMiddlewarePipeline(options =>
        {
            options.Add<ValidationMiddleware>();
            options.Add<RateLimitingMiddleware>();
            options.Add<LoggingMiddleware>();
            options.Add<ErrorHandlingMiddleware>();
        });
    }

    /// <summary>
    /// Adds middleware pipeline services to the service collection with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for the middleware pipeline.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddMiddlewarePipelineServices(
        this IServiceCollection services,
        Action<MiddlewarePipelineOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        return services.AddMiddlewarePipeline(configure);
    }

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

        services.TryAddSingleton<IMiddlewarePipeline>(provider =>
        {
            var options = new MiddlewarePipelineOptions();
            configure?.Invoke(options);

            var middlewareSteps = options.MiddlewareTypes
                .Select(type => provider.GetRequiredService(type))
                .Cast<IPipelineStep>()
                .ToList();

            return new MiddlewarePipeline(middlewareSteps, provider);
        });

        // Register individual middleware as singletons
        services.TryAddSingleton<ValidationMiddleware>();
        services.TryAddSingleton<RateLimitingMiddleware>();
        services.TryAddSingleton<LoggingMiddleware>();
        services.TryAddSingleton<ErrorHandlingMiddleware>();

        return services;
    }
}
