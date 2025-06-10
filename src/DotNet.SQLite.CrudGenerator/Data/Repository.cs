#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Generic repository implementation for SQLite with full CRUD operations and in-memory caching.
/// Uses reflection-based property mapping to automatically generate SQL statements.
/// Table name defaults to "{TypeName}s" and primary key column defaults to "Id".
/// </summary>
/// <typeparam name="T">The entity type. Must have a parameterless constructor and public read/write properties.</typeparam>
/// <typeparam name="TKey">The type of the primary key (e.g., int, string, Guid).</typeparam>
public abstract class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    // Static caches shared across all instances of the same generic instantiation.
    // GetProperties() and GetId() are called on every CRUD operation; caching
    // eliminates repeated Type.GetProperties / Type.GetProperty invocations.
    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _propertiesCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> _idPropertyCache = new();

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

    /// <summary>
    /// Retrieves an entity by its primary key. Returns from cache if available,
    /// otherwise queries the database and caches the result.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var cached = _cache.FirstOrDefault(e => GetId(e)?.Equals(id) == true);
        if (cached is not null) return cached;

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

    /// <summary>
    /// Returns all entities from the table. Results are cached after the first call;
    /// subsequent calls return the cached collection without hitting the database.
    /// </summary>
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

        if (predicate is null) return count;

        var all = await GetAllAsync(cancellationToken);
        return all.Count(predicate);
    }

    /// <summary>
    /// Inserts a new entity into the database and adds it to the cache.
    /// Throws <see cref="RepositoryException"/> with DuplicateKey details on unique constraint violations.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
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

    /// <summary>
    /// Updates an existing entity in the database by primary key.
    /// Throws <see cref="RepositoryException"/> if the entity is not found (zero rows affected).
    /// </summary>
    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
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
            // Fix: Safe casting of potentially null id value
            throw RepositoryException.EntityNotFound(typeof(T).Name, id is null ? 0 : Convert.ToInt32(id));

        var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);
        if (cachedIndex >= 0)
            _cache[cachedIndex] = entity;

        return affected > 0; // Return true if at least one row was affected
    }

    /// <summary>
    /// Deletes an entity by primary key from both the database and cache.
    /// Returns <c>false</c> if the entity was not found.
    /// </summary>
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
        return await GetByIdAsync(id, cancellationToken) is not null;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);
        return _cache.Count;
    }

    protected virtual List<PropertyInfo> GetProperties() =>
        _propertiesCache.GetOrAdd(typeof(T), static t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead && p.CanWrite && p.Name != "Id")
             .ToList());

    protected virtual object? GetPropertyValue(T entity, PropertyInfo property) =>
        property.PropertyType == typeof(DateTime) ? property.GetValue(entity)?.ToString() : property.GetValue(entity);

    protected virtual TKey? GetId(T entity)
    {
        var prop = _idPropertyCache.GetOrAdd(typeof(T), static t => t.GetProperty("Id"));
        return prop is not null ? (TKey?)prop.GetValue(entity) : default;
    }

    /// <summary>
    /// Maps a database row to an entity instance using reflection.
    /// Property names are matched case-insensitively to column names.
    /// DateTime values are parsed from their string representation.
    /// </summary>
    protected virtual T MapFromReader(SqliteDataReader reader)
    {
        var entity = Activator.CreateInstance<T>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var fieldName = reader.GetName(i);
            var property = typeof(T).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property is not null && !reader.IsDBNull(i))
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
