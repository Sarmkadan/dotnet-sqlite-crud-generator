#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Middleware for logging operation execution time, results, and any exceptions.
/// Provides detailed insights into application performance and operation flow.
/// </summary>
public sealed class LoggingMiddleware : IPipelineStep
{
    private readonly bool _enableDetailedLogging;

    public LoggingMiddleware(bool enableDetailedLogging = false)
    {
        _enableDetailedLogging = enableDetailedLogging;
    }

    public async Task<PipelineStepResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        PipelineStepDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();
        var requestType = typeof(TRequest).Name;
        var timestamp = DateTime.UtcNow;

        try
        {
            if (_enableDetailedLogging)
                LogRequest(operationId, timestamp, requestType, request);

            var result = await next(request);

            stopwatch.Stop();
            LogSuccess(operationId, requestType, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(operationId, requestType, ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void LogRequest<T>(string operationId, DateTime timestamp, string requestType, T request)
    {
        var log = $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [OP:{operationId}] Request: {requestType}";
        if (_enableDetailedLogging && request is not null)
            log += Environment.NewLine + $"  Data: {System.Text.Json.JsonSerializer.Serialize(request)}";

        Console.WriteLine(log);
    }

    private void LogSuccess(string operationId, string requestType, long elapsedMs)
    {
        Console.WriteLine($"[OP:{operationId}] ✓ {requestType} completed in {elapsedMs}ms");
    }

    private void LogError(string operationId, string requestType, Exception ex, long elapsedMs)
    {
        Console.Error.WriteLine($"[OP:{operationId}] ✗ {requestType} failed after {elapsedMs}ms");
        Console.Error.WriteLine($"  Exception: {ex.GetType().Name}");
        Console.Error.WriteLine($"  Message: {ex.Message}");

        if (_enableDetailedLogging)
            Console.Error.WriteLine($"  StackTrace: {ex.StackTrace}");
    }
}

public interface IPipelineStep
{
    Task<PipelineStepResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        PipelineStepDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class;
}

public delegate Task<PipelineStepResult> PipelineStepDelegate<TRequest, TResponse>(TRequest request)
    where TRequest : class
    where TResponse : class;

public sealed class PipelineStepResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}
