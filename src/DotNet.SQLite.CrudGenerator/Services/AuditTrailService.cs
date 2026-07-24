#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Enums;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Filter criteria for querying audit trail entries.
/// </summary>
public sealed class AuditTrailFilter
{
    /// <summary>Restrict results to a specific entity type name (e.g. "Product").</summary>
    public string? EntityType { get; set; }

    /// <summary>Restrict results to a specific entity primary key.</summary>
    public int? EntityId { get; set; }

    /// <summary>Restrict results to a specific user who made the change.</summary>
    public int? ChangedByUserId { get; set; }

    /// <summary>Restrict results to a specific operation type.</summary>
    public OperationType? OperationType { get; set; }

    /// <summary>Return only entries at or after this timestamp (UTC).</summary>
    public DateTime? From { get; set; }

    /// <summary>Return only entries at or before this timestamp (UTC).</summary>
    public DateTime? To { get; set; }

    /// <summary>Maximum number of rows to return. Defaults to 100.</summary>
    public int Limit { get; set; } = 100;
}

/// <summary>
/// Summary statistics for the audit trail.
/// </summary>
public sealed class AuditTrailSummary
{
    /// <summary>Total number of audit log records.</summary>
    public int TotalEntries { get; set; }

    /// <summary>Breakdown of entry counts by operation type.</summary>
    public Dictionary<string, int> ByOperation { get; set; } = new();

    /// <summary>Breakdown of entry counts by entity type.</summary>
    public Dictionary<string, int> ByEntityType { get; set; } = new();

    /// <summary>Timestamp of the earliest audit record, or null when empty.</summary>
    public DateTime? OldestEntry { get; set; }

    /// <summary>Timestamp of the most recent audit record, or null when empty.</summary>
    public DateTime? NewestEntry { get; set; }
}

/// <summary>
/// Result of an audit trail pruning operation.
/// </summary>
public sealed class AuditTrailPruneResult
{
    /// <summary>Total number of audit log entries pruned.</summary>
    public int TotalPruned { get; set; }

    /// <summary>Timestamp of the oldest entry that was pruned, or null if no entries were pruned.</summary>
    public DateTime? OldestPrunedEntry { get; set; }
}

/// <summary>
/// Persists structured audit trail entries to the <c>AuditLogs</c> SQLite table
/// and provides rich querying capabilities for compliance and debugging.
/// </summary>
/// <remarks>
/// <para>
/// This service writes directly to the underlying database rather than going through the
/// generic <see cref="Repository{T,TKey}"/> so that it can build efficient parameterised
/// queries without loading full entity collections into memory.
/// </para>
/// <para>
/// Use <see cref="RecordAsync"/> to capture before/after state when performing mutations,
/// then use <see cref="QueryAsync"/> or the typed convenience methods to review history.
/// </para>
/// </remarks>
public sealed class AuditTrailService
{
    private readonly DatabaseConnection _database;

    /// <summary>
    /// Initialises the service with an existing <see cref="DatabaseConnection"/>.
    /// </summary>
    public AuditTrailService(DatabaseConnection database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Records a single audit event to the database.
    /// </summary>
    /// <param name="entityType">Name of the entity type (e.g. <c>"Product"</c>).</param>
    /// <param name="entityId">Primary key of the affected entity.</param>
    /// <param name="operation">The type of operation performed.</param>
    /// <param name="userId">ID of the user who performed the operation.</param>
    /// <param name="oldValues">JSON-serialised state before the change (null for creates).</param>
    /// <param name="newValues">JSON-serialised state after the change (null for deletes).</param>
    /// <param name="reason">Optional free-text reason for the change.</param>
    /// <param name="ipAddress">Optional IP address of the originating request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RecordAsync(
        string entityType,
        int entityId,
        OperationType operation,
        int userId,
        string? oldValues = null,
        string? newValues = null,
        string? reason = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty.", nameof(entityType));

        await _database.OpenAsync(cancellationToken);

        using var cmd = _database.Connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO AuditLogs
                (EntityType, EntityId, OperationType, ChangedByUserId,
                 OldValues, NewValues, ChangeReason, IpAddress, Timestamp)
            VALUES
                (@et, @eid, @op, @uid, @old, @new, @reason, @ip, @ts)";

        cmd.Parameters.AddWithValue("@et", entityType);
        cmd.Parameters.AddWithValue("@eid", entityId);
        cmd.Parameters.AddWithValue("@op", (int)operation);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.Parameters.AddWithValue("@old", (object?)oldValues ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@new", (object?)newValues ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@reason", (object?)reason ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("O"));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Convenience overload that serialises <paramref name="before"/> and <paramref name="after"/>
    /// objects to JSON automatically.
    /// </summary>
    public Task RecordAsync<T>(
        int entityId,
        OperationType operation,
        int userId,
        T? before,
        T? after,
        string? reason = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var oldJson = before is null ? null : JsonSerializer.Serialize(before);
        var newJson = after is null ? null : JsonSerializer.Serialize(after);
        return RecordAsync(typeof(T).Name, entityId, operation, userId, oldJson, newJson, reason, ipAddress, cancellationToken);
    }

    /// <summary>
    /// Returns all audit entries that match the given <paramref name="filter"/>.
    /// </summary>
    public async Task<IReadOnlyList<AuditLog>> QueryAsync(AuditTrailFilter filter, CancellationToken cancellationToken = default)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter));

        await _database.OpenAsync(cancellationToken);

        var wheres = new List<string>();
        using var cmd = _database.Connection.CreateCommand();

        if (filter.EntityType is not null)
        {
            wheres.Add("EntityType = @et");
            cmd.Parameters.AddWithValue("@et", filter.EntityType);
        }
        if (filter.EntityId.HasValue)
        {
            wheres.Add("EntityId = @eid");
            cmd.Parameters.AddWithValue("@eid", filter.EntityId.Value);
        }
        if (filter.ChangedByUserId.HasValue)
        {
            wheres.Add("ChangedByUserId = @uid");
            cmd.Parameters.AddWithValue("@uid", filter.ChangedByUserId.Value);
        }
        if (filter.OperationType.HasValue)
        {
            wheres.Add("OperationType = @op");
            cmd.Parameters.AddWithValue("@op", (int)filter.OperationType.Value);
        }
        if (filter.From.HasValue)
        {
            wheres.Add("Timestamp >= @from");
            cmd.Parameters.AddWithValue("@from", filter.From.Value.ToString("O"));
        }
        if (filter.To.HasValue)
        {
            wheres.Add("Timestamp <= @to");
            cmd.Parameters.AddWithValue("@to", filter.To.Value.ToString("O"));
        }

        var whereClause = wheres.Count > 0 ? " WHERE " + string.Join(" AND ", wheres) : string.Empty;
        cmd.CommandText = $"SELECT * FROM AuditLogs{whereClause} ORDER BY Timestamp DESC LIMIT @limit";
        cmd.Parameters.AddWithValue("@limit", filter.Limit);

        var results = new List<AuditLog>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapAuditLog(reader));

        return results.AsReadOnly();
    }

    /// <summary>
    /// Returns the complete audit history for a single entity instance.
    /// </summary>
    public Task<IReadOnlyList<AuditLog>> GetEntityTrailAsync(
        string entityType,
        int entityId,
        int limit = 100,
        CancellationToken cancellationToken = default) =>
        QueryAsync(new AuditTrailFilter
        {
            EntityType = entityType,
            EntityId = entityId,
            Limit = limit
        }, cancellationToken);

    /// <summary>
    /// Returns all audit entries created by a specific user.
    /// </summary>
    public Task<IReadOnlyList<AuditLog>> GetUserTrailAsync(
        int userId,
        int limit = 100,
        CancellationToken cancellationToken = default) =>
        QueryAsync(new AuditTrailFilter { ChangedByUserId = userId, Limit = limit }, cancellationToken);

    /// <summary>
    /// Returns the most recently recorded audit entries across all entities.
    /// </summary>
    public Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int limit = 50,
        CancellationToken cancellationToken = default) =>
        QueryAsync(new AuditTrailFilter { Limit = limit }, cancellationToken);

    /// <summary>
    /// Deletes audit log entries older than <paramref name="olderThan"/> (UTC).
    /// </summary>
    /// <returns>Number of rows deleted.</returns>
    public async Task<int> PurgeAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);

        using var cmd = _database.Connection.CreateCommand();
        cmd.CommandText = "DELETE FROM AuditLogs WHERE Timestamp < @cutoff";
        cmd.Parameters.AddWithValue("@cutoff", olderThan.ToString("O"));

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Returns aggregate statistics about the audit trail.
    /// </summary>
    public async Task<AuditTrailSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);

        var summary = new AuditTrailSummary();

        using var totalCmd = _database.Connection.CreateCommand();
        totalCmd.CommandText = "SELECT COUNT(*) FROM AuditLogs";
        summary.TotalEntries = Convert.ToInt32(await totalCmd.ExecuteScalarAsync(cancellationToken));

        using var byOpCmd = _database.Connection.CreateCommand();
        byOpCmd.CommandText = "SELECT OperationType, COUNT(*) FROM AuditLogs GROUP BY OperationType";
        using var byOpReader = await byOpCmd.ExecuteReaderAsync(cancellationToken);
        while (await byOpReader.ReadAsync(cancellationToken))
        {
            var op = (OperationType)byOpReader.GetInt32(0);
            summary.ByOperation[op.ToString()] = byOpReader.GetInt32(1);
        }

        using var byEtCmd = _database.Connection.CreateCommand();
        byEtCmd.CommandText = "SELECT EntityType, COUNT(*) FROM AuditLogs GROUP BY EntityType";
        using var byEtReader = await byEtCmd.ExecuteReaderAsync(cancellationToken);
        while (await byEtReader.ReadAsync(cancellationToken))
            summary.ByEntityType[byEtReader.GetString(0)] = byEtReader.GetInt32(1);

        using var rangeCmd = _database.Connection.CreateCommand();
        rangeCmd.CommandText = "SELECT MIN(Timestamp), MAX(Timestamp) FROM AuditLogs";
        using var rangeReader = await rangeCmd.ExecuteReaderAsync(cancellationToken);
        if (await rangeReader.ReadAsync(cancellationToken) && !rangeReader.IsDBNull(0))
        {
            summary.OldestEntry = DateTime.Parse(rangeReader.GetString(0), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
            summary.NewestEntry = DateTime.Parse(rangeReader.GetString(1), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        return summary;
    }

    private static AuditLog MapAuditLog(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new AuditLog
        {
            Id = reader.GetInt32(0),
            EntityType = reader.GetString(1),
            EntityId = reader.GetInt32(2),
            OperationType = (OperationType)reader.GetInt32(3),
            ChangedByUserId = reader.GetInt32(4),
            OldValues = reader.IsDBNull(5) ? null : reader.GetString(5),
            NewValues = reader.IsDBNull(6) ? null : reader.GetString(6),
            ChangeReason = reader.IsDBNull(7) ? null : reader.GetString(7),
            IpAddress = reader.IsDBNull(8) ? null : reader.GetString(8),
            Timestamp = DateTime.Parse(reader.GetString(9), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
