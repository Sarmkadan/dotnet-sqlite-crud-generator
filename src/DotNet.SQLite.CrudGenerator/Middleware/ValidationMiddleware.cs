// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using DotNet.SQLite.CrudGenerator.Exceptions;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Middleware for validating request objects before processing.
/// Uses reflection and data annotations to validate entity properties.
/// Accumulates all validation errors for comprehensive feedback.
/// </summary>
public class ValidationMiddleware : IMiddleware
{
    public async Task<MiddlewareResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        MiddlewareDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class
    {
        var validationErrors = ValidateRequest(request);

        if (validationErrors.Any())
        {
            return new MiddlewareResult
            {
                Success = false,
                Message = "Request validation failed",
                Data = new
                {
                    code = "VALIDATION_FAILED",
                    errors = validationErrors
                }
            };
        }

        return await next(request);
    }

    private List<ValidationError> ValidateRequest<T>(T request) where T : class
    {
        var errors = new List<ValidationError>();

        if (request == null)
        {
            errors.Add(new ValidationError
            {
                Field = "request",
                Message = "Request cannot be null"
            });
            return errors;
        }

        var properties = request.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            var validationAttributes = (ValidationAttribute[])property
                .GetCustomAttributes(typeof(ValidationAttribute), false);

            foreach (var attribute in validationAttributes)
            {
                var validationContext = new ValidationContext(request)
                {
                    MemberName = property.Name
                };

                var validationResult = attribute.GetValidationResult(value, validationContext);
                if (validationResult != ValidationResult.Success)
                {
                    errors.Add(new ValidationError
                    {
                        Field = property.Name,
                        Message = validationResult?.ErrorMessage ?? "Validation failed"
                    });
                }
            }

            // Check for null reference types without [Required]
            if (property.PropertyType.IsClass &&
                property.PropertyType != typeof(string) &&
                value == null &&
                !validationAttributes.OfType<RequiredAttribute>().Any())
            {
                // Allow null on non-required reference types
                continue;
            }
        }

        return errors;
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Extension methods for validating collections and complex objects.
/// </summary>
public static class ValidationExtensions
{
    public static bool ValidateAll<T>(this IEnumerable<T> items, out List<(T item, List<ValidationError> errors)> validationResults) where T : class
    {
        validationResults = new();
        var middleware = new ValidationMiddleware();

        foreach (var item in items)
        {
            var errors = GetValidationErrors(item);
            if (errors.Any())
                validationResults.Add((item, errors));
        }

        return !validationResults.Any();
    }

    private static List<ValidationError> GetValidationErrors<T>(T request) where T : class
    {
        var errors = new List<ValidationError>();
        var properties = request.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            var validationAttributes = (ValidationAttribute[])property
                .GetCustomAttributes(typeof(ValidationAttribute), false);

            foreach (var attribute in validationAttributes)
            {
                var validationContext = new ValidationContext(request)
                {
                    MemberName = property.Name
                };

                var validationResult = attribute.GetValidationResult(value, validationContext);
                if (validationResult != ValidationResult.Success)
                {
                    errors.Add(new ValidationError
                    {
                        Field = property.Name,
                        Message = validationResult?.ErrorMessage ?? "Validation failed"
                    });
                }
            }
        }

        return errors;
    }
}
