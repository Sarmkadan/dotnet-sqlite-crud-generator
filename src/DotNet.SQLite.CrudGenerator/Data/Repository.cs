// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text.Json;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Generic repository implementation for SQLite with CRUD operations and caching.
/// </summary>
public abstract class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    protected readonly DatabaseConnection _database;
    protected readonly string _tableName;
    protected readonly string _primaryKeyColumn;
    protected List<T> _cache = new();
    protected bool _cacheLoaded = false;

    protected Repository(DatabaseConnection database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _tableName = typeof(T).Name + "s";
        _primaryKeyColumn = "Id";
    }

    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var cached = _cache.FirstOrDefault(e => GetId(e)?.Equals(id) == true);
        if (cached != null) return cached;

        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE {_primaryKeyColumn} = @id LIMIT 1";
        command.Parameters.AddWithValue("@id", id!);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var entity = MapFromReader(reader);
            if (!_cache.Contains(entity))
                _cache.Add(entity);
            return entity;
        }

        return null;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheLoaded) return _cache.AsReadOnly();

        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName}";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<T>();
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapFromReader(reader));

        _cache = results;
        _cacheLoaded = true;
        return _cache.AsReadOnly();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate);
    }

    public virtual async Task<int> CountAsync(Func<T, bool>? predicate = null, CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {_tableName}";

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        if (predicate == null) return count;

        var all = await GetAllAsync(cancellationToken);
        return all.Count(predicate);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _database.OpenAsync(cancellationToken);

        var columns = GetProperties();
        var values = columns.Select(p => GetPropertyValue(entity, p)).ToList();
        var columnNames = string.Join(", ", columns.Select(p => p.Name));
        var placeholders = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({placeholders})";

        for (int i = 0; i < values.Count; i++)
        {
            command.Parameters.AddWithValue($"@p{i}", values[i] ?? DBNull.Value);
        }

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            _cache.Add(entity);
            return entity;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw RepositoryException.DuplicateKey(typeof(T).Name, "Unknown", entity);
        }
        catch (SqliteException ex)
        {
            throw new RepositoryException($"Failed to insert entity: {ex.Message}", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        foreach (var entity in entities)
        {
            results.Add(await AddAsync(entity, cancellationToken));
        }
        return results;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _database.OpenAsync(cancellationToken);

        var properties = GetProperties();
        var updates = string.Join(", ", properties.Select((p, i) => $"{p.Name} = @p{i}"));
        var id = GetId(entity);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"UPDATE {_tableName} SET {updates} WHERE {_primaryKeyColumn} = @id";

        for (int i = 0; i < properties.Count; i++)
        {
            command.Parameters.AddWithValue($"@p{i}", GetPropertyValue(entity, properties[i]) ?? DBNull.Value);
        }

        command.Parameters.AddWithValue("@id", id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
            throw RepositoryException.EntityNotFound(typeof(T).Name, (int)(object)id);

        var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);
        if (cachedIndex >= 0)
            _cache[cachedIndex] = entity;

        return entity;
    }

    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE {_primaryKeyColumn} = @id";
        command.Parameters.AddWithValue("@id", id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected > 0)
            _cache.RemoveAll(e => GetId(e)?.Equals(id) == true);

        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var id = GetId(entity);
        return await DeleteAsync(id!, cancellationToken);
    }

    public virtual async Task<int> DeleteRangeAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
    {
        int deleted = 0;
        foreach (var id in ids)
        {
            if (await DeleteAsync(id, cancellationToken))
                deleted++;
        }
        return deleted;
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken) != null;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);
        return _cache.Count;
    }

    protected virtual List<PropertyInfo> GetProperties() =>
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && p.Name != _primaryKeyColumn)
            .ToList();

    protected virtual object? GetPropertyValue(T entity, PropertyInfo property) =>
        property.PropertyType == typeof(DateTime) ? property.GetValue(entity)?.ToString() : property.GetValue(entity);

    protected virtual TKey? GetId(T entity)
    {
        var prop = typeof(T).GetProperty(_primaryKeyColumn);
        return prop != null ? (TKey?)prop.GetValue(entity) : default;
    }

    protected virtual T MapFromReader(SqliteDataReader reader)
    {
        var entity = Activator.CreateInstance<T>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var fieldName = reader.GetName(i);
            var property = typeof(T).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property != null && !reader.IsDBNull(i))
            {
                var value = reader.GetValue(i);
                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                    value = DateTime.Parse(value.ToString() ?? "");

                property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
            }
        }
        return entity;
    }
}
