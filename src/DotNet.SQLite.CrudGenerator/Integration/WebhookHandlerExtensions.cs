#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookHandler"/> providing additional functionality
/// for webhook management and monitoring.
/// </summary>
public static class WebhookHandlerExtensions
{
    /// <summary>
    /// Filters endpoints by event type and returns only active endpoints.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to filter by.</param>
    /// <returns>Collection of active endpoints that support the specified event type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> is null or empty.</exception>
    public static IEnumerable<WebhookEndpoint> GetActiveEndpointsForEvent(this WebhookHandler handler, string eventType)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(eventType, nameof(eventType));

        return handler.GetEndpoints()
            .Where(e => e.Active && e.EventTypes.Contains(eventType, StringComparer.OrdinalIgnoreCase))
            .OrderBy(e => e.Name);
    }

    /// <summary>
    /// Gets delivery attempts for a specific event type, ordered by most recent first.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="eventType">The event type to filter by.</param>
    /// <returns>Collection of delivery attempts for the specified event type.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> is null or empty.</exception>
    public static IEnumerable<DeliveryAttempt> GetDeliveryAttemptsByEvent(this WebhookHandler handler, string eventType)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(eventType, nameof(eventType));

        return handler.GetDeliveryHistory()
            .Where(attempt => attempt.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(attempt => attempt.AttemptedAt)
            .Take(50);
    }

    /// <summary>
    /// Gets statistics for a specific endpoint by name.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <param name="endpointName">The name of the endpoint.</param>
    /// <returns>Webhook statistics for the specified endpoint, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="endpointName"/> is null or empty.</exception>
    public static WebhookStatistics? GetStatisticsForEndpoint(this WebhookHandler handler, string endpointName)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrEmpty(endpointName, nameof(endpointName));

        var endpoint = handler.GetEndpoints()
            .FirstOrDefault(e => e.Name.Equals(endpointName, StringComparison.Ordinal));

        if (endpoint == null)
            return null;

        var endpointHistory = handler.GetDeliveryHistory(endpointName).ToList();
        var endpointAttempts = endpointHistory.Count;
        var successfulAttempts = endpointHistory.Count(a => a.Success);
        var failedAttempts = endpointHistory.Count(a => !a.Success);

        return new WebhookStatistics
        {
            RegisteredEndpoints = 1,
            ActiveEndpoints = endpoint.Active ? 1 : 0,
            TotalDeliveries = endpointAttempts,
            SuccessfulDeliveries = successfulAttempts,
            FailedDeliveries = failedAttempts
        };
    }

    /// <summary>
    /// Gets the success rate percentage for all deliveries.
    /// </summary>
    /// <param name="handler">The webhook handler instance.</param>
    /// <returns>Success rate as a percentage (0-100), or 0 if no deliveries have been made.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="handler"/> is null.</exception>
    public static double GetSuccessRate(this WebhookHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var stats = handler.GetStatistics();
        return stats.TotalDeliveries == 0
            ? 0
            : Math.Round((double)stats.SuccessfulDeliveries / stats.TotalDeliveries * 100, 2);
    }
}