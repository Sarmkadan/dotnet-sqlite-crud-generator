#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using DotNet.SQLite.CrudGenerator.Validation;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Provides validation helpers for <see cref="WebhookHandler"/> instances using the shared <see cref="ValidationResult"/> abstraction.
/// </summary>
public static class WebhookHandlerValidation
{
    /// <summary>
    /// Validates a <see cref="WebhookHandler"/> instance and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing any validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static ValidationResult Validate(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var result = new ValidationResult();

        // Validate endpoints collection
        var endpoints = value.GetEndpoints().ToList();
        if (endpoints.Count == 0)
        {
            result = result.WithProblem("No webhook endpoints are registered. At least one endpoint should be registered for the handler to be useful.");
        }
        else
        {
            // Validate each endpoint
            foreach (var endpoint in endpoints)
            {
                ValidateEndpoint(endpoint, result);
            }
        }

        // Validate delivery history
        var deliveryHistory = value.GetDeliveryHistory().ToList();
        if (deliveryHistory.Count > 0)
        {
            ValidateDeliveryHistory(deliveryHistory, result);
        }

        return result;
    }

    private static void ValidateEndpoint(WebhookEndpoint endpoint, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Name))
        {
            result = result.WithProblem(nameof(endpoint.Name), "Webhook endpoint has empty or whitespace name.");
        }

        if (endpoint.Url == null)
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has null URL."
                    : $"Webhook endpoint '{endpoint.Name}' has null URL.");
        }
        else if (!endpoint.Url.IsAbsoluteUri)
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has relative URL: {endpoint.Url}."
                    : $"Webhook endpoint '{endpoint.Name}' has relative URL: {endpoint.Url}.");
        }
        else if (endpoint.Url.Scheme != "http" && endpoint.Url.Scheme != "https")
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has unsupported URL scheme '{endpoint.Url.Scheme}'. Only http and https are supported."
                    : $"Webhook endpoint '{endpoint.Name}' has unsupported URL scheme '{endpoint.Url.Scheme}'. Only http and https are supported.");
        }

        if (endpoint.EventTypes == null || endpoint.EventTypes.Length == 0)
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? "Webhook endpoint has null or empty event types array."
                    : $"Webhook endpoint '{endpoint.Name}' has null or empty event types array.");
        }
        else
        {
            foreach (var eventType in endpoint.EventTypes)
            {
                if (string.IsNullOrWhiteSpace(eventType))
                {
                    result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                        string.IsNullOrEmpty(endpoint.Name)
                            ? "Webhook endpoint has whitespace event type."
                            : $"Webhook endpoint '{endpoint.Name}' has whitespace event type.");
                }
            }
        }

        if (endpoint.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has creation date in the future: {endpoint.CreatedAt:O}."
                    : $"Webhook endpoint '{endpoint.Name}' has creation date in the future: {endpoint.CreatedAt:O}.");
        }

        if (endpoint.LastDeliveryAt.HasValue && endpoint.LastDeliveryAt > DateTime.UtcNow.AddMinutes(5))
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has last delivery date in the future: {endpoint.LastDeliveryAt:O}."
                    : $"Webhook endpoint '{endpoint.Name}' has last delivery date in the future: {endpoint.LastDeliveryAt:O}.");
        }

        if (endpoint.DeliveryCount < 0)
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has negative delivery count: {endpoint.DeliveryCount}."
                    : $"Webhook endpoint '{endpoint.Name}' has negative delivery count: {endpoint.DeliveryCount}.");
        }

        if (endpoint.FailureCount < 0)
        {
            result = result.WithProblem(string.IsNullOrEmpty(endpoint.Name) ? "webhookEndpoint" : endpoint.Name,
                string.IsNullOrEmpty(endpoint.Name)
                    ? $"Webhook endpoint has negative failure count: {endpoint.FailureCount}."
                    : $"Webhook endpoint '{endpoint.Name}' has negative failure count: {endpoint.FailureCount}.");
        }
    }

    private static void ValidateDeliveryHistory(List<DeliveryAttempt> deliveryHistory, ValidationResult result)
    {
        var attempts = deliveryHistory.ToList();
        var failedAttempts = attempts.Count(a => !a.Success);

        if (failedAttempts > 0)
        {
            result = result.WithProblem("deliveryHistory", $"Found {failedAttempts} failed delivery attempts out of {attempts.Count} total attempts.");
        }

        foreach (var attempt in attempts)
        {
            if (string.IsNullOrWhiteSpace(attempt.WebhookName))
            {
                result = result.WithProblem("deliveryHistory", "Delivery attempt has empty or whitespace webhook name.");
            }

            if (string.IsNullOrWhiteSpace(attempt.EventType))
            {
                result = result.WithProblem(string.IsNullOrEmpty(attempt.WebhookName) ? "deliveryAttempt" : attempt.WebhookName,
                    string.IsNullOrEmpty(attempt.WebhookName)
                        ? "Delivery attempt has empty or whitespace event type."
                        : $"Delivery attempt for webhook '{attempt.WebhookName}' has empty or whitespace event type.");
            }

            if (attempt.AttemptedAt > DateTime.UtcNow.AddMinutes(5))
            {
                result = result.WithProblem(string.IsNullOrEmpty(attempt.WebhookName) ? "deliveryAttempt" : attempt.WebhookName,
                    string.IsNullOrEmpty(attempt.WebhookName)
                        ? $"Delivery attempt has attempted date in the future: {attempt.AttemptedAt:O}."
                        : $"Delivery attempt for webhook '{attempt.WebhookName}' has attempted date in the future: {attempt.AttemptedAt:O}.");
            }

            if (attempt.Success && attempt.StatusCode >= 400)
            {
                result = result.WithProblem(string.IsNullOrEmpty(attempt.WebhookName) ? "deliveryAttempt" : attempt.WebhookName,
                    string.IsNullOrEmpty(attempt.WebhookName)
                        ? $"Successful delivery attempt has error status code: {attempt.StatusCode}."
                        : $"Successful delivery attempt for webhook '{attempt.WebhookName}' has error status code: {attempt.StatusCode}.");
            }

            if (!attempt.Success && string.IsNullOrWhiteSpace(attempt.Error))
            {
                result = result.WithProblem(string.IsNullOrEmpty(attempt.WebhookName) ? "deliveryAttempt" : attempt.WebhookName,
                    string.IsNullOrEmpty(attempt.WebhookName)
                        ? "Failed delivery attempt has no error message."
                        : $"Failed delivery attempt for webhook '{attempt.WebhookName}' has no error message.");
            }
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="WebhookHandler"/> is valid.
    /// </summary>
    /// <param name="value">The webhook handler to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this WebhookHandler value)
        => value.Validate().IsValid;

    /// <summary>
    /// Ensures that the specified <see cref="WebhookHandler"/> is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The webhook handler to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ValidationException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this WebhookHandler value)
    {
        ArgumentNullException.ThrowIfNull(value);
        value.Validate().ThrowIfInvalid(problems => new ValidationException(
            $"WebhookHandler is not valid. Problems:{Environment.NewLine}{string.Join("\n", problems.Select(p => $"- {p.Message}"))}"));
    }
}