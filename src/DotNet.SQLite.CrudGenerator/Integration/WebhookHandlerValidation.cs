#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Provides validation helpers for <see cref="WebhookHandler"/> instances.
/// </summary>
public static class WebhookHandlerValidation
{
    /// <summary>
    /// Validates a <see cref="WebhookHandler"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate endpoints collection
        var endpoints = value.GetEndpoints().ToList();
        if (endpoints.Count == 0)
        {
            problems.Add("No webhook endpoints are registered. At least one endpoint should be registered for the handler to be useful.");
        }
        else
        {
            // Validate each endpoint
            foreach (var endpoint in endpoints)
            {
                ValidateEndpoint(endpoint, problems);
            }
        }

        // Validate delivery history
        var deliveryHistory = value.GetDeliveryHistory().ToList();
        if (deliveryHistory.Count > 0)
        {
            ValidateDeliveryHistory(deliveryHistory, problems);
        }

        return problems.AsReadOnly();
    }

    private static void ValidateEndpoint(WebhookEndpoint endpoint, List<string> problems)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Name))
        {
            problems.Add($"Webhook endpoint has empty or whitespace name.");
        }

        if (endpoint.Url == null)
        {
            problems.Add($"Webhook endpoint '{endpoint.Name}' has null URL.");
        }
        else if (!endpoint.Url.IsAbsoluteUri)
        {
            problems.Add($"Webhook endpoint '{endpoint.Name}' has relative URL: {endpoint.Url}.");
        }
        else if (endpoint.Url.Scheme != "http" && endpoint.Url.Scheme != "https")
        {
            problems.Add($"Webhook endpoint '{endpoint.Name}' has unsupported URL scheme '{endpoint.Url.Scheme}'. Only http and https are supported.");
        }

        if (endpoint.EventTypes == null || endpoint.EventTypes.Length == 0)
        {
            problems.Add(string.IsNullOrEmpty(endpoint.Name)
                ? "Webhook endpoint has null or empty event types array."
                : $"Webhook endpoint '{endpoint.Name}' has null or empty event types array.");
        }
        else
        {
            foreach (var eventType in endpoint.EventTypes)
            {
                if (string.IsNullOrWhiteSpace(eventType))
                {
                    problems.Add(string.IsNullOrEmpty(endpoint.Name)
                        ? "Webhook endpoint has whitespace event type."
                        : $"Webhook endpoint '{endpoint.Name}' has whitespace event type.");
                }
            }
        }

        if (endpoint.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add(string.IsNullOrEmpty(endpoint.Name)
                ? $"Webhook endpoint has creation date in the future: {endpoint.CreatedAt:O}."
                : $"Webhook endpoint '{endpoint.Name}' has creation date in the future: {endpoint.CreatedAt:O}.");
        }

        if (endpoint.LastDeliveryAt.HasValue && endpoint.LastDeliveryAt > DateTime.UtcNow.AddMinutes(5))
        {
            problems.Add(string.IsNullOrEmpty(endpoint.Name)
                ? $"Webhook endpoint has last delivery date in the future: {endpoint.LastDeliveryAt:O}."
                : $"Webhook endpoint '{endpoint.Name}' has last delivery date in the future: {endpoint.LastDeliveryAt:O}.");
        }

        if (endpoint.DeliveryCount < 0)
        {
            problems.Add(string.IsNullOrEmpty(endpoint.Name)
                ? $"Webhook endpoint has negative delivery count: {endpoint.DeliveryCount}."
                : $"Webhook endpoint '{endpoint.Name}' has negative delivery count: {endpoint.DeliveryCount}.");
        }

        if (endpoint.FailureCount < 0)
        {
            problems.Add(string.IsNullOrEmpty(endpoint.Name)
                ? $"Webhook endpoint has negative failure count: {endpoint.FailureCount}."
                : $"Webhook endpoint '{endpoint.Name}' has negative failure count: {endpoint.FailureCount}.");
        }
    }

    private static void ValidateDeliveryHistory(IEnumerable<DeliveryAttempt> deliveryHistory, List<string> problems)
    {
        var attempts = deliveryHistory.ToList();
        var failedAttempts = attempts.Count(a => !a.Success);

        if (failedAttempts > 0)
        {
            problems.Add($"Found {failedAttempts} failed delivery attempts out of {attempts.Count} total attempts.");
        }

        foreach (var attempt in attempts)
        {
            if (string.IsNullOrWhiteSpace(attempt.WebhookName))
            {
                problems.Add("Delivery attempt has empty or whitespace webhook name.");
            }

            if (string.IsNullOrWhiteSpace(attempt.EventType))
            {
                problems.Add($"Delivery attempt for webhook '{attempt.WebhookName}' has empty or whitespace event type.");
            }

            if (attempt.AttemptedAt > DateTime.UtcNow.AddMinutes(5))
            {
                problems.Add(string.IsNullOrEmpty(attempt.WebhookName)
                    ? $"Delivery attempt has attempted date in the future: {attempt.AttemptedAt:O}."
                    : $"Delivery attempt for webhook '{attempt.WebhookName}' has attempted date in the future: {attempt.AttemptedAt:O}.");
            }

            if (attempt.Success && attempt.StatusCode >= 400)
            {
                problems.Add(string.IsNullOrEmpty(attempt.WebhookName)
                    ? $"Successful delivery attempt has error status code: {attempt.StatusCode}."
                    : $"Successful delivery attempt for webhook '{attempt.WebhookName}' has error status code: {attempt.StatusCode}.");
            }

            if (!attempt.Success && string.IsNullOrWhiteSpace(attempt.Error))
            {
                problems.Add(string.IsNullOrEmpty(attempt.WebhookName)
                    ? "Failed delivery attempt has no error message."
                    : $"Failed delivery attempt for webhook '{attempt.WebhookName}' has no error message.");
            }
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="WebhookHandler"/> is valid.
    /// </summary>
    /// <param name="value">The webhook handler to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this WebhookHandler value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="WebhookHandler"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookHandler is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}