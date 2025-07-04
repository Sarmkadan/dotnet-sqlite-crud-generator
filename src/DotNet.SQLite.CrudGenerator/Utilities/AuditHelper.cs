#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Helper for tracking and logging audit information.
/// Records entity changes, user actions, and system events.
/// </summary>
public static class AuditHelper
{
    private static readonly List<AuditLogEntry> _auditLog = new();
    private static readonly object _lockObject = new();
    private static int _maxLogSize = 5000;

    /// <summary>
    /// Logs an audit entry for entity operations.
    /// </summary>
    public static void LogEntityChange(string entityType, string entityId, string operation, string? userId = null, string? details = null)
    {
        var entry = new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            UserId = userId,
            Details = details,
            IpAddress = GetClientIpAddress()
        };

        lock (_lockObject)
        {
            _auditLog.Add(entry);

            // Trim log if exceeds max size
            if (_auditLog.Count > _maxLogSize)
                _auditLog.RemoveRange(0, _auditLog.Count - _maxLogSize);
        }
    }

    /// <summary>
    /// Logs property changes for an entity.
    /// </summary>
    public static void LogPropertyChange(
        string entityType,
        string entityId,
        string propertyName,
        object? oldValue,
        object? newValue,
        string? userId = null)
    {
        var details = $"Property '{propertyName}' changed from '{oldValue ?? "null"}' to '{newValue ?? "null"}'";
        LogEntityChange(entityType, entityId, "UPDATE", userId, details);
    }

    /// <summary>
    /// Gets audit entries for a specific entity.
    /// </summary>
    public static IEnumerable<AuditLogEntry> GetEntityAuditTrail(string entityType, string entityId)
    {
        lock (_lockObject)
        {
            return _auditLog
                .Where(entry => entry.EntityType == entityType && entry.EntityId == entityId)
                .OrderByDescending(entry => entry.Timestamp)
                .ToList();
        }
    }

    /// <summary>
    /// Gets audit entries for a specific operation.
    /// </summary>
    public static IEnumerable<AuditLogEntry> GetOperationLog(string operation, int limit = 100)
    {
        lock (_lockObject)
        {
            return _auditLog
                .Where(entry => entry.Operation == operation)
                .OrderByDescending(entry => entry.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Gets audit entries for a specific user.
    /// </summary>
    public static IEnumerable<AuditLogEntry> GetUserAuditTrail(string userId, int limit = 100)
    {
        lock (_lockObject)
        {
            return _auditLog
                .Where(entry => entry.UserId == userId)
                .OrderByDescending(entry => entry.Timestamp)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Gets all audit entries within a date range.
    /// </summary>
    public static IEnumerable<AuditLogEntry> GetAuditEntriesInRange(DateTime from, DateTime to)
    {
        lock (_lockObject)
        {
            return _auditLog
                .Where(entry => entry.Timestamp >= from && entry.Timestamp <= to)
                .OrderByDescending(entry => entry.Timestamp)
                .ToList();
        }
    }

    /// <summary>
    /// Gets audit statistics.
    /// </summary>
    public static AuditStatistics GetStatistics()
    {
        lock (_lockObject)
        {
            return new AuditStatistics
            {
                TotalEntries = _auditLog.Count,
                EntryCount = _auditLog.Count,
                Operations = _auditLog
                    .GroupBy(e => e.Operation)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EntityTypes = _auditLog
                    .GroupBy(e => e.EntityType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                UniqueUsers = _auditLog
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count(u => !string.IsNullOrEmpty(u)),
                OldestEntry = _auditLog.OrderBy(e => e.Timestamp).FirstOrDefault()?.Timestamp,
                NewestEntry = _auditLog.OrderByDescending(e => e.Timestamp).FirstOrDefault()?.Timestamp
            };
        }
    }

    /// <summary>
    /// Clears all audit logs (use with caution).
    /// </summary>
    public static void ClearAuditLog()
    {
        lock (_lockObject)
        {
            _auditLog.Clear();
        }
    }

    /// <summary>
    /// Exports audit log to CSV format.
    /// </summary>
    public static string ExportToCsv()
    {
        lock (_lockObject)
        {
            var csv = "ID,Timestamp,EntityType,EntityId,Operation,UserId,IpAddress,Details\n";

            foreach (var entry in _auditLog)
            {
                csv += $"\"{entry.Id}\",\"{entry.Timestamp:O}\",\"{entry.EntityType}\",\"{entry.EntityId}\",";
                csv += $"\"{entry.Operation}\",\"{entry.UserId ?? ""}\",\"{entry.IpAddress ?? ""}\",\"{(entry.Details ?? "").Replace("\"", "\"\"")}\"\n";
            }

            return csv;
        }
    }

    private static string? GetClientIpAddress()
    {
        // Placeholder for getting client IP from HTTP context
        // In a real application, this would use HttpContext
        return null;
    }
}

public sealed class AuditLogEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
}

public sealed class AuditStatistics
{
    public int TotalEntries { get; set; }
    public int EntryCount { get; set; }
    public Dictionary<string, int> Operations { get; set; } = new();
    public Dictionary<string, int> EntityTypes { get; set; } = new();
    public int UniqueUsers { get; set; }
    public DateTime? OldestEntry { get; set; }
    public DateTime? NewestEntry { get; set; }
}
