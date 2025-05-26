#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNet.SQLite.CrudGenerator.Exceptions;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Middleware for consistent error handling and transformation.
/// Catches exceptions and converts them to structured error responses.
/// Tracks error statistics for monitoring and alerting.
/// </summary>
public sealed class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ConcurrentDictionary<string, int> _errorCounts = new();

    public async Task<MiddlewareResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        MiddlewareDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class
    {
        try
        {
            return await next(request);
        }
        catch (ValidationException ex)
        {
            return HandleValidationError(ex);
        }
        catch (RepositoryException ex)
        {
            return HandleRepositoryError(ex);
        }
        catch (ArgumentException ex)
        {
            return HandleArgumentError(ex);
        }
        catch (OperationCanceledException ex)
        {
            return HandleOperationCanceledError(ex);
        }
        catch (Exception ex)
        {
            return HandleUnexpectedError(ex);
        }
    }

    private MiddlewareResult HandleValidationError(ValidationException ex)
    {
        TrackError(nameof(ValidationException));
        return new MiddlewareResult
        {
            Success = false,
            Message = "Validation failed",
            Data = new { error = ex.Message, code = "VALIDATION_ERROR" }
        };
    }

    private MiddlewareResult HandleRepositoryError(RepositoryException ex)
    {
        TrackError(nameof(RepositoryException));
        return new MiddlewareResult
        {
            Success = false,
            Message = "Database operation failed",
            Data = new { error = ex.Message, code = "REPOSITORY_ERROR" }
        };
    }

    private MiddlewareResult HandleArgumentError(ArgumentException ex)
    {
        TrackError(nameof(ArgumentException));
        return new MiddlewareResult
        {
            Success = false,
            Message = "Invalid argument",
            Data = new { error = ex.Message, paramName = ex.ParamName, code = "ARGUMENT_ERROR" }
        };
    }

    private MiddlewareResult HandleOperationCanceledError(OperationCanceledException ex)
    {
        TrackError(nameof(OperationCanceledException));
        return new MiddlewareResult
        {
            Success = false,
            Message = "Operation was canceled",
            Data = new { error = "The operation was canceled", code = "OPERATION_CANCELED" }
        };
    }

    private MiddlewareResult HandleUnexpectedError(Exception ex)
    {
        TrackError(ex.GetType().Name);
        Console.Error.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);

        return new MiddlewareResult
        {
            Success = false,
            Message = "An unexpected error occurred",
            Data = new { error = ex.Message, type = ex.GetType().Name, code = "INTERNAL_ERROR" }
        };
    }

    private void TrackError(string errorType)
    {
        _errorCounts.AddOrUpdate(errorType, 1, (key, count) => count + 1);
    }

    public Dictionary<string, int> GetErrorStatistics()
    {
        return new Dictionary<string, int>(_errorCounts);
    }

    public void ResetErrorStatistics()
    {
        _errorCounts.Clear();
    }
}
