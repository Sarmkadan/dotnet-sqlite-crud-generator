#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using DotNet.SQLite.CrudGenerator.Data;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Describes a column as seen in the database or derived from a model type.
/// </summary>
public sealed record ColumnInfo(string Name, string SqliteType, bool NotNull, bool IsPrimaryKey);

/// <summary>
/// Classifies the kind of difference found between model and database column.
/// </summary>
public enum ColumnDiffKind
{
    /// <summary>Column exists in the model but is absent from the database table.</summary>
    Added,
    /// <summary>Column exists in the database but is absent from the model.</summary>
    Removed,
    /// <summary>Column exists in both, but the SQLite type differs.</summary>
    TypeChanged
}

/// <summary>
/// Represents a single column-level difference between a model and its database table.
/// </summary>
/// <param name="ColumnName">Name of the column.</param>
/// <param name="Kind">Kind of difference.</param>
/// <param name="Expected">Column definition derived from the model (null when <see cref="ColumnDiffKind.Removed"/>).</param>
/// <param name="Actual">Column definition read from the database (null when <see cref="ColumnDiffKind.Added"/>).</param>
public sealed record ColumnDiff(string ColumnName, ColumnDiffKind Kind, ColumnInfo? Expected, ColumnInfo? Actual);

/// <summary>
/// Aggregates all column-level differences for a single table.
/// </summary>
public sealed record TableDiff(string TableName, IReadOnlyList<ColumnDiff> ColumnDiffs)
{
    /// <summary>Returns <see langword="true"/> when there are no differences.</summary>
    public bool IsUpToDate => ColumnDiffs.Count == 0;
}

/// <summary>
/// Full diff result for an entity type, including the generated ALTER TABLE script.
/// </summary>
public sealed record MigrationDiff(
    string EntityTypeName,
    string TableName,
    TableDiff TableDiff,
    string AlterScript)
{
    /// <summary>Returns <see langword="true"/> when the schema matches the model exactly.</summary>
    public bool IsUpToDate => TableDiff.IsUpToDate;
}

/// <summary>
/// Compares a C# entity model against the actual SQLite schema and produces a diff
/// with an ALTER TABLE script that brings the database in sync with the model.
/// </summary>
public sealed class MigrationDiffService
{
    private readonly DatabaseConnection _database;

    /// <summary>
    /// Initialises the service with an existing <see cref="DatabaseConnection"/>.
    /// </summary>
    public MigrationDiffService(DatabaseConnection database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Computes the schema diff between <paramref name="entityType"/> and its database table.
    /// The table name is derived using the same convention as <see cref="Repository{T,TKey}"/>:
    /// <c>typeof(T).Name + "s"</c>.
    /// </summary>
    /// <param name="entityType">The C# entity type to compare.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="MigrationDiff"/> describing all detected differences.</returns>
    public async Task<MigrationDiff> ComputeDiffAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        var tableName = entityType.Name + "s";
        var expected = GetExpectedSchema(entityType);
        var actual = await GetActualSchemaAsync(tableName, cancellationToken);

        var diffs = new List<ColumnDiff>();

        foreach (var kvp in expected)
        {
            if (!actual.TryGetValue(kvp.Key, out var actualCol))
                diffs.Add(new ColumnDiff(kvp.Key, ColumnDiffKind.Added, kvp.Value, null));
            else if (!string.Equals(actualCol.SqliteType, kvp.Value.SqliteType, StringComparison.OrdinalIgnoreCase))
                diffs.Add(new ColumnDiff(kvp.Key, ColumnDiffKind.TypeChanged, kvp.Value, actualCol));
        }

        foreach (var kvp in actual)
        {
            if (!expected.ContainsKey(kvp.Key))
                diffs.Add(new ColumnDiff(kvp.Key, ColumnDiffKind.Removed, null, kvp.Value));
        }

        var tableDiff = new TableDiff(tableName, diffs.AsReadOnly());
        var script = BuildAlterScript(tableName, diffs);

        return new MigrationDiff(entityType.Name, tableName, tableDiff, script);
    }

    /// <summary>
    /// Reads the current column definitions for <paramref name="tableName"/> via
    /// <c>PRAGMA table_info</c>. Returns an empty dictionary when the table does not exist.
    /// </summary>
    public async Task<Dictionary<string, ColumnInfo>> GetActualSchemaAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);

        await _database.OpenAsync(cancellationToken);

        using var cmd = _database.Connection.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{tableName}\")";
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(1);
            // SQLite stores types like "INTEGER", "TEXT", "REAL"; strip any precision suffix.
            var rawType = reader.GetString(2);
            var sqliteType = rawType.Split('(')[0].Trim().ToUpperInvariant();
            var notNull = reader.GetInt32(3) == 1;
            var isPk = reader.GetInt32(5) > 0;

            result[name] = new ColumnInfo(name, sqliteType, notNull, isPk);
        }

        return result;
    }

    /// <summary>
    /// Derives the expected column schema from the public properties of <paramref name="entityType"/>.
    /// </summary>
    public Dictionary<string, ColumnInfo> GetExpectedSchema(Type entityType)
    {
        var result = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
        var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              .Where(p => p.CanRead && p.CanWrite);

        foreach (var prop in props)
        {
            var sqliteType = MapToSqliteType(prop);
            var notNull = IsNotNullable(prop);
            var isPk = string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase);
            result[prop.Name] = new ColumnInfo(prop.Name, sqliteType, notNull, isPk);
        }

        return result;
    }

    private static string MapToSqliteType(PropertyInfo prop)
    {
        var resolved = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        if (resolved == typeof(int) || resolved == typeof(long) ||
            resolved == typeof(bool) || resolved == typeof(byte) || resolved == typeof(short))
            return "INTEGER";

        if (resolved == typeof(decimal) || resolved == typeof(float) || resolved == typeof(double))
            return "REAL";

        return "TEXT";
    }

    private static bool IsNotNullable(PropertyInfo prop)
    {
        if (Nullable.GetUnderlyingType(prop.PropertyType) is not null)
            return false;

        if (prop.PropertyType.IsValueType)
            return true;

        return prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() is not null;
    }

    private static string BuildAlterScript(string tableName, List<ColumnDiff> diffs)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"-- Migration diff for table: {tableName}");
        sb.AppendLine($"-- Generated: {DateTime.UtcNow:O}");
        sb.AppendLine();

        if (diffs.Count == 0)
        {
            sb.AppendLine("-- No differences detected. Schema is up to date.");
            return sb.ToString();
        }

        foreach (var diff in diffs.Where(d => d.Kind == ColumnDiffKind.Added))
        {
            var notNull = (diff.Expected!.NotNull && !diff.Expected.IsPrimaryKey) ? " NOT NULL" : "";
            sb.AppendLine($"ALTER TABLE \"{tableName}\" ADD COLUMN \"{diff.ColumnName}\" {diff.Expected.SqliteType}{notNull};");
        }

        foreach (var diff in diffs.Where(d => d.Kind == ColumnDiffKind.Removed))
            sb.AppendLine($"-- WARNING: Column \"{diff.ColumnName}\" exists in the database but is missing from the model. Drop manually if no longer needed.");

        foreach (var diff in diffs.Where(d => d.Kind == ColumnDiffKind.TypeChanged))
            sb.AppendLine($"-- WARNING: Column \"{diff.ColumnName}\" type mismatch — database has {diff.Actual!.SqliteType}, model expects {diff.Expected!.SqliteType}. Recreate the table to apply this change.");

        return sb.ToString();
    }
}
