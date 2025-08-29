#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// Validation helpers for entity change events.
/// Provides validation, checking, and exception-throwing utilities for all entity change event types.
/// </summary>
public static class EntityChangedEventValidation
{
    /// <summary>
    /// Validates an EntityChangedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this EntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EntityType
        if (string.IsNullOrWhiteSpace(value.EntityType))
        {
            problems.Add("EntityType cannot be null, empty, or whitespace");
        }
        else if (value.EntityType.Length > 200)
        {
            problems.Add("EntityType cannot exceed 200 characters");
        }

        // Validate Entity (nullable, but if set should be non-null)
        if (value.Entity is null)
        {
            problems.Add("Entity cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an EntityCreatedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this EntityCreatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base EntityChangedEvent
        problems.AddRange(value.Validate());

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an EntityUpdatedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this EntityUpdatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base EntityChangedEvent
        problems.AddRange(value.Validate());

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        // Validate OldEntity (nullable, but if set should be non-null)
        if (value.OldEntity is null)
        {
            problems.Add("OldEntity cannot be null");
        }

        // Validate Changes dictionary
        if (value.Changes is null)
        {
            problems.Add("Changes dictionary cannot be null");
        }
        else
        {
            if (value.Changes.Count == 0)
            {
                problems.Add("Changes dictionary cannot be empty");
            }

            foreach (var kvp in value.Changes)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Change key cannot be null, empty, or whitespace");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an EntityDeletedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this EntityDeletedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate base EntityChangedEvent
        problems.AddRange(value.Validate());

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a BulkEntityChangedEvent for common issues.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate<T>(this BulkEntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Count
        if (value.Count <= 0)
        {
            problems.Add("Count must be a positive integer");
        }

        // Validate Operation
        if (string.IsNullOrWhiteSpace(value.Operation))
        {
            problems.Add("Operation cannot be null, empty, or whitespace");
        }
        else if (value.Operation.Length > 100)
        {
            problems.Add("Operation cannot exceed 100 characters");
        }

        // Validate Entities list
        if (value.Entities is null)
        {
            problems.Add("Entities list cannot be null");
        }
        else
        {
            if (value.Entities.Count == 0)
            {
                problems.Add("Entities list cannot be empty when Count is positive");
            }

            if (value.Entities.Count != value.Count)
            {
                problems.Add("Entities list count must match Count property");
            }

            // Check for null entities in the list
            for (int i = 0; i < value.Entities.Count; i++)
            {
                if (value.Entities[i] is null)
                {
                    problems.Add($"Entities[{i}] cannot be null");
                }
            }
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a ProductRestockedEvent for common issues.
    /// </summary>
    /// <param name="value">The product restocked event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ProductRestockedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ProductId
        if (value.ProductId <= 0)
        {
            problems.Add("ProductId must be a positive integer");
        }

        // Validate QuantityAdded
        if (value.QuantityAdded < 0)
        {
            problems.Add("QuantityAdded cannot be negative");
        }

        // Validate NewQuantity
        if (value.NewQuantity < 0)
        {
            problems.Add("NewQuantity cannot be negative");
        }
        else if (value.NewQuantity < value.QuantityAdded)
        {
            problems.Add("NewQuantity cannot be less than QuantityAdded");
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a ProductSoldEvent for common issues.
    /// </summary>
    /// <param name="value">The product sold event to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ProductSoldEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ProductId
        if (value.ProductId <= 0)
        {
            problems.Add("ProductId must be a positive integer");
        }

        // Validate QuantitySold
        if (value.QuantitySold <= 0)
        {
            problems.Add("QuantitySold must be a positive integer");
        }

        // Validate Revenue
        if (value.Revenue < 0m)
        {
            problems.Add("Revenue cannot be negative");
        }

        // Validate RemainingQuantity
        if (value.RemainingQuantity < 0)
        {
            problems.Add("RemainingQuantity cannot be negative");
        }

        // Validate AggregateId (should be non-empty)
        if (value.AggregateId == Guid.Empty)
        {
            problems.Add("AggregateId must be a non-empty GUID");
        }

        // Validate OccurredAt
        if (value.OccurredAt == default)
        {
            problems.Add("OccurredAt must be set to a non-default DateTime");
        }
        else if (value.OccurredAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("OccurredAt cannot be in the future");
        }
        else if (value.OccurredAt < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("OccurredAt cannot be more than one year in the past");
        }

        // Validate EventName
        if (string.IsNullOrWhiteSpace(value.EventName))
        {
            problems.Add("EventName cannot be null, empty, or whitespace");
        }
        else if (value.EventName.Length > 200)
        {
            problems.Add("EventName cannot exceed 200 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an EntityChangedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityChangedEvent<T>? value) where T : class
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if an EntityCreatedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityCreatedEvent<T>? value) where T : class
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if an EntityUpdatedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityUpdatedEvent<T>? value) where T : class
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if an EntityDeletedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this EntityDeletedEvent<T>? value) where T : class
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a BulkEntityChangedEvent is valid.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid<T>(this BulkEntityChangedEvent<T>? value) where T : class
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a ProductRestockedEvent is valid.
    /// </summary>
    /// <param name="value">The product restocked event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ProductRestockedEvent? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Checks if a ProductSoldEvent is valid.
    /// </summary>
    /// <param name="value">The product sold event to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ProductSoldEvent? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures an EntityChangedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity change event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EntityChangedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures an EntityCreatedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity created event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityCreatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EntityCreatedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures an EntityUpdatedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity updated event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityUpdatedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EntityUpdatedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures an EntityDeletedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The entity deleted event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this EntityDeletedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EntityDeletedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a BulkEntityChangedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="value">The bulk entity changed event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid<T>(this BulkEntityChangedEvent<T>? value) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BulkEntityChangedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a ProductRestockedEvent is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The product restocked event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid(this ProductRestockedEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProductRestockedEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures a ProductSoldEvent is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The product sold event to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value has validation problems</exception>
    public static void EnsureValid(this ProductSoldEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProductSoldEvent validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
