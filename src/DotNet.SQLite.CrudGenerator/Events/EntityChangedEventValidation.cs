#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// Validation helpers for entity change events.
/// Provides validation, checking, and exception-throwing utilities for all entity change event types using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class EntityChangedEventValidation
{
    /// <summary>
    /// Validates an EntityChangedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate<T>(this EntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate EntityType
        if (string.IsNullOrWhiteSpace(value.EntityType))
        {
            result = result.WithProblem(nameof(value.EntityType), "EntityType cannot be null, empty, or whitespace");
        }
        else if (value.EntityType.Length > 200)
        {
            result = result.WithProblem(nameof(value.EntityType), "EntityType cannot exceed 200 characters");
        }

        // Validate Entity (nullable, but if set should be non-null)
        if (value.Entity is null)
        {
            result = result.WithProblem(nameof(value.Entity), "Entity cannot be null");
        }

        return result;
    }

    /// <summary>
    /// Validates an EntityCreatedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate<T>(this EntityCreatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = Validate((EntityChangedEvent<T>?)value);

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        return result;
    }

    /// <summary>
    /// Validates an EntityUpdatedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate<T>(this EntityUpdatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = Validate((EntityChangedEvent<T>?)value);

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        // Validate OldEntity (nullable, but if set should be non-null)
        if (value.OldEntity is null)
        {
            result = result.WithProblem(nameof(value.OldEntity), "OldEntity cannot be null");
        }

        // Validate Changes dictionary
        if (value.Changes is null)
        {
            result = result.WithProblem(nameof(value.Changes), "Changes dictionary cannot be null");
        }
        else
        {
            if (value.Changes.Count == 0)
            {
                result = result.WithProblem(nameof(value.Changes), "Changes dictionary cannot be empty");
            }

            foreach (var kvp in value.Changes)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    result = result.WithProblem(nameof(value.Changes), "Change key cannot be null, empty, or whitespace");
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Validates an EntityDeletedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate<T>(this EntityDeletedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = Validate((EntityChangedEvent<T>?)value);

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        return result;
    }

    /// <summary>
    /// Validates a BulkEntityChangedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate<T>(this BulkEntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate Count
        if (value.Count <= 0)
        {
            result = result.WithProblem(nameof(value.Count), "Count must be a positive integer");
        }

        // Validate Operation
        if (string.IsNullOrWhiteSpace(value.Operation))
        {
            result = result.WithProblem(nameof(value.Operation), "Operation cannot be null, empty, or whitespace");
        }
        else if (value.Operation.Length > 100)
        {
            result = result.WithProblem(nameof(value.Operation), "Operation cannot exceed 100 characters");
        }

        // Validate Entities list
        if (value.Entities is null)
        {
            result = result.WithProblem(nameof(value.Entities), "Entities list cannot be null");
        }
        else
        {
            if (value.Entities.Count == 0)
            {
                result = result.WithProblem(nameof(value.Entities), "Entities list cannot be empty when Count is positive");
            }

            if (value.Entities.Count != value.Count)
            {
                result = result.WithProblem(nameof(value.Entities), "Entities list count must match Count property");
            }

            // Check for null entities in the list
            for (int i = 0; i < value.Entities.Count; i++)
            {
                if (value.Entities[i] is null)
                {
                    result = result.WithProblem(nameof(value.Entities), $"Entities[{i}] cannot be null");
                    break;
                }
            }
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        return result;
    }

    /// <summary>
    /// Validates a ProductRestockedEvent for common issues.
    /// </summary>
    /// <param name="value">The product restocked event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate(this ProductRestockedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate ProductId
        if (value.ProductId <= 0)
        {
            result = result.WithProblem(nameof(value.ProductId), "ProductId must be a positive integer");
        }

        // Validate QuantityAdded
        if (value.QuantityAdded < 0)
        {
            result = result.WithProblem(nameof(value.QuantityAdded), "QuantityAdded cannot be negative");
        }

        // Validate NewQuantity
        if (value.NewQuantity < 0)
        {
            result = result.WithProblem(nameof(value.NewQuantity), "NewQuantity cannot be negative");
        }
        else if (value.NewQuantity < value.QuantityAdded)
        {
            result = result.WithProblem(nameof(value.NewQuantity), "NewQuantity cannot be less than QuantityAdded");
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        return result;
    }

    /// <summary>
    /// Validates a ProductSoldEvent for common issues.
    /// </summary>
    /// <param name="value">The product sold event to validate</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static ValidationResult Validate(this ProductSoldEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate ProductId
        if (value.ProductId <= 0)
        {
            result = result.WithProblem(nameof(value.ProductId), "ProductId must be a positive integer");
        }

        // Validate QuantitySold
        if (value.QuantitySold <= 0)
        {
            result = result.WithProblem(nameof(value.QuantitySold), "QuantitySold must be a positive integer");
        }

        // Validate Revenue
        if (value.Revenue < 0m)
        {
            result = result.WithProblem(nameof(value.Revenue), "Revenue cannot be negative");
        }

        // Validate RemainingQuantity
        if (value.RemainingQuantity < 0)
        {
            result = result.WithProblem(nameof(value.RemainingQuantity), "RemainingQuantity cannot be negative");
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.AggregateId), "AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.OccurredAt), "OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventName), "EventName cannot exceed 200 characters");
        }

        return result;
    }

    /// <summary>
    /// Checks if an EntityChangedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityChangedEvent<T>? value) where T : class
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if an EntityCreatedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityCreatedEvent<T>? value) where T : class
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if an EntityUpdatedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityUpdatedEvent<T>? value) where T : class
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if an EntityDeletedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityDeletedEvent<T>? value) where T : class
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if a BulkEntityChangedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this BulkEntityChangedEvent<T>? value) where T : class
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if a ProductRestockedEvent is valid.
    /// </summary>
    /// <param name="value">The product restocked event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ProductRestockedEvent? value)
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Checks if a ProductSoldEvent is valid.
    /// </summary>
    /// <param name="value">The product sold event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ProductSoldEvent? value)
        => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Ensures an EntityChangedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"EntityChangedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures an EntityCreatedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityCreatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"EntityCreatedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures an EntityUpdatedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityUpdatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"EntityUpdatedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures an EntityDeletedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityDeletedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"EntityDeletedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures a BulkEntityChangedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this BulkEntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"BulkEntityChangedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures a ProductRestockedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The product restocked event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid(this ProductRestockedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"ProductRestockedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Ensures a ProductSoldEvent is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The product sold event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ValidationException">Thrown if value has validation problems</exception>
    public static void EnsureValid(this ProductSoldEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"ProductSoldEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }
}