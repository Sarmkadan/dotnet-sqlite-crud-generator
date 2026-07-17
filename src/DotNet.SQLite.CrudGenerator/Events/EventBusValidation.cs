#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.Events;

/// <summary>
/// Validation helpers for domain events and event bus related types.
/// Provides validation, checking, and exception-throwing utilities.
/// </summary>
public static class EventBusValidation
{
    /// <summary>
    /// Validates a domain event for common issues.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate AggregateId
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

        // Validate GetEventName() result
        var eventName = value.GetEventName();
        if (string.IsNullOrWhiteSpace(eventName))
        {
            problems.Add("GetEventName() must return a non-empty string");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a domain event is valid.
    /// </summary>
    /// <param name="value">The domain event to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this DomainEvent? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures a domain event is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The domain event to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this DomainEvent? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Domain event validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates an EventEnvelope for common issues.
    /// </summary>
    /// <param name="value">The event envelope to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EventEnvelope? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EventId
        if (value.EventId == Guid.Empty)
        {
            problems.Add("EventId must be a non-empty GUID");
        }

        // Validate EventTypeName
        if (string.IsNullOrWhiteSpace(value.EventTypeName))
        {
            problems.Add("EventTypeName cannot be null, empty, or whitespace");
        }
        else if (value.EventTypeName.Length > 200)
        {
            problems.Add("EventTypeName cannot exceed 200 characters");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            problems.Add("Timestamp must be set to a non-default DateTime");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add("Timestamp cannot be in the future");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            problems.Add("Timestamp cannot be more than one year in the past");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an event envelope is valid.
    /// </summary>
    /// <param name="value">The event envelope to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this EventEnvelope? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures an event envelope is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The event envelope to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this EventEnvelope? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Event envelope validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates EventBusStatistics for common issues.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EventBusStatistics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate counts
        if (value.RegisteredEventTypes < 0)
        {
            problems.Add("RegisteredEventTypes cannot be negative");
        }

        if (value.TotalSubscriptions < 0)
        {
            problems.Add("TotalSubscriptions cannot be negative");
        }

        if (value.TotalEventsPublished < 0)
        {
            problems.Add("TotalEventsPublished cannot be negative");
        }

        // Validate Subscriptions dictionary
        if (value.Subscriptions is null)
        {
            problems.Add("Subscriptions dictionary cannot be null");
        }
        else
        {
            foreach (var kvp in value.Subscriptions)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Subscription key cannot be null, empty, or whitespace");
                }

                if (kvp.Value < 0)
                {
                    problems.Add("Subscription count cannot be negative");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if event bus statistics are valid.
    /// </summary>
    /// <param name="value">The statistics to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    public static bool IsValid(this EventBusStatistics? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures event bus statistics are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The statistics to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has validation problems.</exception>
    public static void EnsureValid(this EventBusStatistics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Event bus statistics validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}