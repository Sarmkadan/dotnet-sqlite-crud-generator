#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// Validation helpers for domain events and event bus related types.
/// Provides validation, checking, and exception-throwing utilities using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class EventBusValidation
{
    /// <summary>
    /// Validates a domain event for common issues.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate AggregateId
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

        // Validate GetEventName() result
        var eventName = value.GetEventName();
        if (string.IsNullOrWhiteSpace(eventName))
        {
            result = result.WithProblem(nameof(value.GetEventName), "GetEventName() must return a non-empty string");
        }

        return result;
    }

    /// <summary>
    /// Checks if a domain event is valid.
    /// </summary>
    /// <param name="value">The domain event to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this DomainEvent? value) => value.Validate().IsValid;

    /// <summary>
    /// Ensures a domain event is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"Domain event validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Validates an EventEnvelope for common issues.
    /// </summary>
    /// <param name="value">The event envelope to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate(this EventEnvelope? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate EventId
        if (value.EventId == Guid.Empty)
        {
            result = result.WithProblem(nameof(value.EventId), "EventId must be a non-empty GUID");
        }

        // Validate EventTypeName
        if (string.IsNullOrWhiteSpace(value.EventTypeName))
        {
            result = result.WithProblem(nameof(value.EventTypeName), "EventTypeName cannot be null, empty, or whitespace");
        }
        else if (value.EventTypeName.Length > 200)
        {
            result = result.WithProblem(nameof(value.EventTypeName), "EventTypeName cannot exceed 200 characters");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            result = result.WithProblem(nameof(value.Timestamp), "Timestamp must be set to a non-default DateTime");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(nameof(value.Timestamp), "Timestamp cannot be in the future");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            result = result.WithProblem(nameof(value.Timestamp), "Timestamp cannot be more than one year in the past");
        }

        return result;
    }

    /// <summary>
    /// Checks if an event envelope is valid.
    /// </summary>
    /// <param name="value">The event envelope to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this EventEnvelope? value) => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Ensures an event envelope is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The event envelope to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this EventEnvelope? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"Event envelope validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }

    /// <summary>
    /// Validates EventBusStatistics for common issues.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate(this EventBusStatistics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate counts
        if (value.RegisteredEventTypes < 0)
        {
            result = result.WithProblem(nameof(value.RegisteredEventTypes), "RegisteredEventTypes cannot be negative");
        }

        if (value.TotalSubscriptions < 0)
        {
            result = result.WithProblem(nameof(value.TotalSubscriptions), "TotalSubscriptions cannot be negative");
        }

        if (value.TotalEventsPublished < 0)
        {
            result = result.WithProblem(nameof(value.TotalEventsPublished), "TotalEventsPublished cannot be negative");
        }

        // Validate Subscriptions dictionary
        if (value.Subscriptions is null)
        {
            result = result.WithProblem(nameof(value.Subscriptions), "Subscriptions dictionary cannot be null");
        }
        else
        {
            foreach (var kvp in value.Subscriptions)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    result = result.WithProblem(nameof(value.Subscriptions), "Subscription key cannot be null, empty, or whitespace");
                }

                if (kvp.Value < 0)
                {
                    result = result.WithProblem(nameof(value.Subscriptions), "Subscription count cannot be negative");
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if event bus statistics are valid.
    /// </summary>
    /// <param name="value">The statistics to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this EventBusStatistics? value) => value?.Validate().IsValid ?? false;

    /// <summary>
    /// Ensures event bus statistics are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this EventBusStatistics? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"Event bus statistics validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems.Select(p => p.Message))}"));
    }
}