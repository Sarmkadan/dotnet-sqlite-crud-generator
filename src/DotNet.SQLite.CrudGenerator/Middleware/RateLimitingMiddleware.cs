// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace DotNet.SQLite.CrudGenerator.Middleware;

/// <summary>
/// Middleware for implementing rate limiting based on client identity.
/// Uses a sliding window algorithm to track request counts per client.
/// Prevents abuse by limiting requests to a configured threshold per time window.
/// </summary>
public class RateLimitingMiddleware : IMiddleware
{
    private readonly int _requestsPerWindow;
    private readonly TimeSpan _timeWindow;
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();

    public RateLimitingMiddleware(int requestsPerWindow = 100, int windowSeconds = 60)
    {
        _requestsPerWindow = requestsPerWindow;
        _timeWindow = TimeSpan.FromSeconds(windowSeconds);
    }

    public async Task<MiddlewareResult> ExecuteAsync<TRequest, TResponse>(
        TRequest request,
        MiddlewareDelegate<TRequest, TResponse> next)
        where TRequest : class
        where TResponse : class
    {
        var clientId = GetClientIdentity(request);
        var bucket = GetOrCreateBucket(clientId);

        if (!bucket.IsAllowed())
        {
            return new MiddlewareResult
            {
                Success = false,
                Message = "Rate limit exceeded",
                Data = new
                {
                    error = "Too many requests",
                    code = "RATE_LIMIT_EXCEEDED",
                    retryAfterSeconds = (int)bucket.GetRetryAfter().TotalSeconds
                }
            };
        }

        bucket.RecordRequest();
        return await next(request);
    }

    private string GetClientIdentity<T>(T request) where T : class
    {
        if (request is IClientIdentified identified)
            return identified.GetClientId();

        // Fallback to request type as default client
        return request.GetType().Name;
    }

    private RateLimitBucket GetOrCreateBucket(string clientId)
    {
        return _buckets.AddOrUpdate(
            clientId,
            _ => new RateLimitBucket(_requestsPerWindow, _timeWindow),
            (_, existing) => existing);
    }

    public void ResetLimits()
    {
        _buckets.Clear();
    }

    public RateLimitStatistics GetStatistics()
    {
        return new RateLimitStatistics
        {
            TotalClients = _buckets.Count,
            ClientLimits = _buckets.ToDictionary(
                kvp => kvp.Key,
                kvp => new ClientRateLimitInfo
                {
                    RequestCount = kvp.Value.RequestCount,
                    ResetTime = kvp.Value.ResetTime,
                    IsAllowed = kvp.Value.IsAllowed()
                })
        };
    }
}

public interface IClientIdentified
{
    string GetClientId();
}

public class RateLimitBucket
{
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private Queue<DateTime> _requestTimes = new();

    public DateTime ResetTime { get; private set; }
    public int RequestCount => _requestTimes.Count;

    public RateLimitBucket(int maxRequests, TimeSpan timeWindow)
    {
        _maxRequests = maxRequests;
        _timeWindow = timeWindow;
        ResetTime = DateTime.UtcNow.Add(timeWindow);
    }

    public bool IsAllowed()
    {
        CleanupExpiredRequests();
        return _requestTimes.Count < _maxRequests;
    }

    public void RecordRequest()
    {
        CleanupExpiredRequests();
        _requestTimes.Enqueue(DateTime.UtcNow);
        ResetTime = DateTime.UtcNow.Add(_timeWindow);
    }

    public TimeSpan GetRetryAfter()
    {
        CleanupExpiredRequests();
        if (_requestTimes.Count == 0)
            return TimeSpan.Zero;

        var oldestRequest = _requestTimes.Peek();
        var retryAfter = oldestRequest.Add(_timeWindow) - DateTime.UtcNow;
        return retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero;
    }

    private void CleanupExpiredRequests()
    {
        var cutoff = DateTime.UtcNow.Subtract(_timeWindow);
        while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
            _requestTimes.Dequeue();
    }
}

public class RateLimitStatistics
{
    public int TotalClients { get; set; }
    public Dictionary<string, ClientRateLimitInfo> ClientLimits { get; set; } = new();
}

public class ClientRateLimitInfo
{
    public int RequestCount { get; set; }
    public DateTime ResetTime { get; set; }
    public bool IsAllowed { get; set; }
}
