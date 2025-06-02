// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DotNet.SQLite.CrudGenerator.Integration;

/// <summary>
/// Webhook handler for sending event notifications to external endpoints.
/// Supports payload signing, retry logic, and delivery tracking.
/// </summary>
public class WebhookHandler
{
    private readonly HttpRequestExecutor _httpExecutor;
    private readonly ConcurrentDictionary<string, WebhookEndpoint> _endpoints = new();
    private readonly ConcurrentDictionary<string, DeliveryAttempt> _deliveryHistory = new();
    private readonly string _signingSecret;

    public WebhookHandler(HttpClient httpClient, string? signingSecret = null)
    {
        _httpExecutor = new HttpRequestExecutor(httpClient, maxRetries: 3, retryDelayMs: 5000);
        _signingSecret = signingSecret ?? Guid.NewGuid().ToString();
    }

    public void RegisterEndpoint(string name, Uri url, string[] eventTypes, bool active = true)
    {
        var endpoint = new WebhookEndpoint
        {
            Name = name,
            Url = url,
            EventTypes = eventTypes,
            Active = active,
            CreatedAt = DateTime.UtcNow
        };

        _endpoints[name] = endpoint;
    }

    public async Task<bool> SendWebhookAsync<T>(string eventType, T payload)
    {
        var activeEndpoints = _endpoints.Values
            .Where(e => e.Active && e.EventTypes.Contains(eventType))
            .ToList();

        if (!activeEndpoints.Any())
            return false;

        var json = JsonSerializer.Serialize(payload);
        var success = true;

        foreach (var endpoint in activeEndpoints)
        {
            var attempt = new DeliveryAttempt
            {
                WebhookName = endpoint.Name,
                EventType = eventType,
                AttemptedAt = DateTime.UtcNow,
                DeliveryId = Guid.NewGuid()
            };

            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add signature header
                var signature = ComputeSignature(json);
                content.Headers.Add("X-Webhook-Signature", signature);
                content.Headers.Add("X-Webhook-Event", eventType);
                content.Headers.Add("X-Delivery-ID", attempt.DeliveryId.ToString());

                var response = await _httpExecutor.PostAsync(endpoint.Url.ToString(), content);

                attempt.Success = response.IsSuccessStatusCode;
                attempt.StatusCode = (int)response.StatusCode;
                attempt.ResponseContent = await response.Content.ReadAsStringAsync();

                if (!attempt.Success)
                    success = false;

                endpoint.LastDeliveryAt = DateTime.UtcNow;
                endpoint.DeliveryCount++;
            }
            catch (Exception ex)
            {
                attempt.Success = false;
                attempt.Error = ex.Message;
                success = false;
                endpoint.FailureCount++;
            }

            _deliveryHistory[attempt.DeliveryId.ToString()] = attempt;
        }

        return success;
    }

    public void DisableEndpoint(string name)
    {
        if (_endpoints.TryGetValue(name, out var endpoint))
            endpoint.Active = false;
    }

    public void EnableEndpoint(string name)
    {
        if (_endpoints.TryGetValue(name, out var endpoint))
            endpoint.Active = true;
    }

    public IEnumerable<WebhookEndpoint> GetEndpoints()
    {
        return _endpoints.Values.ToList();
    }

    public IEnumerable<DeliveryAttempt> GetDeliveryHistory(string? webhookName = null)
    {
        IEnumerable<DeliveryAttempt> history = _deliveryHistory.Values;

        if (!string.IsNullOrEmpty(webhookName))
            history = history.Where(h => h.WebhookName == webhookName);

        return history.OrderByDescending(h => h.AttemptedAt).Take(100);
    }

    public WebhookStatistics GetStatistics()
    {
        var deliveries = _deliveryHistory.Values.ToList();

        return new WebhookStatistics
        {
            RegisteredEndpoints = _endpoints.Count,
            ActiveEndpoints = _endpoints.Values.Count(e => e.Active),
            TotalDeliveries = deliveries.Count,
            SuccessfulDeliveries = deliveries.Count(d => d.Success),
            FailedDeliveries = deliveries.Count(d => !d.Success)
        };
    }

    private string ComputeSignature(string payload)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingSecret)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}

public class WebhookEndpoint
{
    public string Name { get; set; } = string.Empty;
    public Uri Url { get; set; } = null!;
    public string[] EventTypes { get; set; } = Array.Empty<string>();
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastDeliveryAt { get; set; }
    public int DeliveryCount { get; set; }
    public int FailureCount { get; set; }
}

public class DeliveryAttempt
{
    public Guid DeliveryId { get; set; }
    public string WebhookName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseContent { get; set; }
    public string? Error { get; set; }
}

public class WebhookStatistics
{
    public int RegisteredEndpoints { get; set; }
    public int ActiveEndpoints { get; set; }
    public int TotalDeliveries { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
}
