// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DotNet.SQLite.CrudGenerator.Enums;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Represents an audit log entry for tracking entity changes.
/// </summary>
public class AuditLog
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Entity type is required")]
    [StringLength(100, ErrorMessage = "Entity type must not exceed 100 characters")]
    [JsonPropertyName("entityType")]
    public required string EntityType { get; set; }

    [Required(ErrorMessage = "Entity ID is required")]
    [JsonPropertyName("entityId")]
    public int EntityId { get; set; }

    [JsonPropertyName("operationType")]
    public OperationType OperationType { get; set; }

    [Required(ErrorMessage = "Changed by user ID is required")]
    [JsonPropertyName("changedByUserId")]
    public int ChangedByUserId { get; set; }

    [StringLength(1000, ErrorMessage = "Old values must not exceed 1000 characters")]
    [JsonPropertyName("oldValues")]
    public string? OldValues { get; set; }

    [StringLength(1000, ErrorMessage = "New values must not exceed 1000 characters")]
    [JsonPropertyName("newValues")]
    public string? NewValues { get; set; }

    [StringLength(500, ErrorMessage = "Change reason must not exceed 500 characters")]
    [JsonPropertyName("changeReason")]
    public string? ChangeReason { get; set; }

    [StringLength(50, ErrorMessage = "IP address must not exceed 50 characters")]
    [JsonPropertyName("ipAddress")]
    public string? IpAddress { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the audit log entry for consistency.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(EntityType) || EntityId <= 0)
            return false;

        if (ChangedByUserId <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Creates an audit log entry for a new entity.
    /// </summary>
    public static AuditLog CreateForCreate(string entityType, int entityId, int userId, string? newValues, string? reason = null, string? ipAddress = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            OperationType = OperationType.Create,
            ChangedByUserId = userId,
            NewValues = newValues,
            ChangeReason = reason,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an audit log entry for an updated entity.
    /// </summary>
    public static AuditLog CreateForUpdate(string entityType, int entityId, int userId, string? oldValues, string? newValues, string? reason = null, string? ipAddress = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            OperationType = OperationType.Update,
            ChangedByUserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            ChangeReason = reason,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an audit log entry for a deleted entity.
    /// </summary>
    public static AuditLog CreateForDelete(string entityType, int entityId, int userId, string? oldValues, string? reason = null, string? ipAddress = null)
    {
        return new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            OperationType = OperationType.Delete,
            ChangedByUserId = userId,
            OldValues = oldValues,
            ChangeReason = reason,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };
    }
}
