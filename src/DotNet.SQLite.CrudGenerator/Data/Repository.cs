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

using Microsoft.Extensions.Logging;
namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Generic repository implementation for SQLite with CRUD operations and caching.
/// </summary>
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
    protected readonly ILogger<Repository<T, TKey>>? _logger;

    protected Repository(DatabaseConnection database, ILogger<Repository<T, TKey>>? logger = null)
    {
        _logger = logger;
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _tableName = typeof(T).Name + "s";
        _primaryKeyColumn = "Id";
    }

    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        _logger?.LogDebug("Attempting to retrieve entity {EntityType} with ID {EntityId} from cache", typeof(T).Name, id);

        var cached = _cache.FirstOrDefault(e => GetId(e)?.Equals(id) == true);
        if (cached is not null)
        {
            _logger?.LogDebug("Entity {EntityType} with ID {EntityId} found in cache", typeof(T).Name, id);
            return cached;
        }

        _logger?.LogDebug("Entity {EntityType} with ID {EntityId} not found in cache, querying database", typeof(T).Name, id);

        try
        {
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
                _logger?.LogInformation("Successfully retrieved entity {EntityType} with ID {EntityId} from database", typeof(T).Name, id);
                return entity;
            }

            _logger?.LogDebug("Entity {EntityType} with ID {EntityId} not found in database", typeof(T).Name, id);
            return null;
        }
        catch (SqliteException ex)
        {
            _logger?.LogError(ex, "Database error while retrieving entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);
            throw new RepositoryException($"Database error while retrieving entity with ID {id} from table {_tableName}: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not RepositoryException)
        {
            _logger?.LogError(ex, "Unexpected error while retrieving entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);
            throw new RepositoryException($"Unexpected error while retrieving entity with ID {id} from table {_tableName}: {ex.Message}", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Retrieving all entities {EntityType} from table {TableName}", typeof(T).Name, _tableName);

        if (_cacheLoaded)
        {
            _logger?.LogDebug("Returning {EntityCount} cached entities {EntityType} from table {TableName}", _cache.Count, typeof(T).Name, _tableName);
            return _cache.AsReadOnly();
        }

        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName}";

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var results = new List<T>();
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapFromReader(reader));

        _cache = results;
        _cacheLoaded = true;
        _logger?.LogInformation("Successfully retrieved {EntityCount} entities {EntityType} from table {TableName}", _cache.Count, typeof(T).Name, _tableName);
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

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        _logger?.LogDebug("Adding new entity {EntityType} to table {TableName}", typeof(T).Name, _tableName);

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

            // Retrieve the last inserted row ID
            using var lastIdCommand = _database.Connection.CreateCommand();
            lastIdCommand.CommandText = "SELECT last_insert_rowid();";
            var lastId = await lastIdCommand.ExecuteScalarAsync(cancellationToken);

            // Set the Id property of the entity
            var idProperty = typeof(T).GetProperty(_primaryKeyColumn);
            if (idProperty is not null && idProperty.CanWrite)
            {
                var convertedId = Convert.ChangeType(lastId, typeof(TKey));
                idProperty.SetValue(entity, convertedId);
            }

            _cache.Add(entity);
            _logger?.LogInformation("Successfully added entity {EntityType} with ID {EntityId} to table {TableName}", typeof(T).Name, lastId, _tableName);
            return entity;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger?.LogWarning(ex, "Duplicate key error while adding entity {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw RepositoryException.DuplicateKey(typeof(T).Name, "Unknown", entity);
        }
        catch (SqliteException ex)
        {
            _logger?.LogError(ex, "Failed to insert entity {EntityType} into table {TableName}", typeof(T).Name, _tableName);
            throw new RepositoryException($"Failed to insert entity into table {_tableName}: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not RepositoryException)
        {
            _logger?.LogError(ex, "Unexpected error while adding entity {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw new RepositoryException($"Unexpected error while adding entity to table {_tableName}: {ex.Message}", ex);
        }
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities is null) throw new ArgumentNullException(nameof(entities));

        var results = new List<T>();
        var entityList = entities.ToList();

        if (entityList.Count == 0)
            return results;

        await _database.OpenAsync(cancellationToken);

        try
        {
            // Begin transaction for the entire batch
            using var transaction = _database.Connection.BeginTransaction();

            // Build parameterized insert command once
            var columns = GetProperties();
            var columnNames = string.Join(", ", columns.Select(p => p.Name));
            var placeholders = string.Join(", ", columns.Select((_, i) => $"@p{i}"));

            var command = _database.Connection.CreateCommand();
            command.CommandText = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({placeholders}) RETURNING *";

            // Add parameters once
            for (int i = 0; i < columns.Count; i++)
            {
                var param = new SqliteParameter();
                param.ParameterName = $"@p{i}";
                command.Parameters.Add(param);
            }

            // Insert all entities in the transaction
            foreach (var entity in entityList)
            {
                // Set parameter values
                for (int i = 0; i < columns.Count; i++)
                {
                    var value = GetPropertyValue(entity, columns[i]);
                    command.Parameters[i].Value = value ?? DBNull.Value;
                }

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var insertedEntity = MapFromReader(reader);
                    results.Add(insertedEntity);
                    _cache.Add(insertedEntity);
                }
            }

            transaction.Commit();
            return results;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger?.LogWarning(ex, "Duplicate key error while adding batch of entities {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw RepositoryException.DuplicateKey(typeof(T).Name, "Unknown", entityList.FirstOrDefault() ?? entityList.First());
        }
        catch (SqliteException ex)
        {
            _logger?.LogError(ex, "Failed to insert batch of entities {EntityType} into table {TableName}", typeof(T).Name, _tableName);
            throw new RepositoryException($"Failed to insert batch of entities into table {_tableName}: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not RepositoryException)
        {
            _logger?.LogError(ex, "Unexpected error while adding batch of entities {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw new RepositoryException($"Unexpected error while adding batch of entities to table {_tableName}: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        _logger?.LogDebug("Attempting to update entity {EntityType} with ID {EntityId} in table {TableName}", typeof(T).Name, id, _tableName);

        await _database.OpenAsync(cancellationToken);

        var properties = GetProperties();
        var updates = string.Join(", ", properties.Select((p, i) => $"{p.Name} = @p{i}"));

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"UPDATE {_tableName} SET {updates} WHERE {_primaryKeyColumn} = @id";

        for (int i = 0; i < properties.Count; i++)
        {
            command.Parameters.AddWithValue($"@p{i}", GetPropertyValue(entity, properties[i]) ?? DBNull.Value);
        }

        command.Parameters.AddWithValue("@id", id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            _logger?.LogWarning("Entity {EntityType} with ID {EntityId} not found for update in table {TableName}", typeof(T).Name, id, _tableName);
            // Fix: Safe casting of potentially null id value
            throw RepositoryException.EntityNotFound(typeof(T).Name, id is null ? 0 : Convert.ToInt32(id));
        }

        var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);
        if (cachedIndex >= 0)
            _cache[cachedIndex] = entity;

        _logger?.LogInformation("Successfully updated entity {EntityType} with ID {EntityId} in table {TableName}", typeof(T).Name, id, _tableName);
        return affected > 0; // Return true if at least one row was affected
    }

    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Attempting to delete entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);

        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE {_primaryKeyColumn} = @id";
        command.Parameters.AddWithValue("@id", id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected > 0)
        {
            _cache.RemoveAll(e => GetId(e)?.Equals(id) == true);
            _logger?.LogInformation("Successfully deleted entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);
        }
        else
        {
            _logger?.LogDebug("Entity {EntityType} with ID {EntityId} not found for deletion in table {TableName}", typeof(T).Name, id, _tableName);
        }

        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var id = GetId(entity);
        _logger?.LogDebug("Deleting entity {EntityType} with ID {EntityId} via entity reference", typeof(T).Name, id);
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
        property.PropertyType == typeof(DateTime)
            ? ((DateTime?)property.GetValue(entity))?.ToString("O", System.Globalization.CultureInfo.InvariantCulture)
            : property.GetValue(entity);

    protected virtual TKey? GetId(T entity)
    {
        var prop = _idPropertyCache.GetOrAdd(typeof(T), static t => t.GetProperty("Id"));
        return prop is not null ? (TKey?)prop.GetValue(entity) : default;
    }

    protected virtual T MapFromReader(SqliteDataReader reader)
    {
        var entity = Activator.CreateInstance<T>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var fieldName = reader.GetName(i);
            var property = typeof(T).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property is null || !property.CanWrite)
                continue;

            if (reader.IsDBNull(i))
            {
                // For nullable properties assign null; non-nullable properties keep their default.
                var underlying = Nullable.GetUnderlyingType(property.PropertyType);
                if (underlying is not null || !property.PropertyType.IsValueType)
                    property.SetValue(entity, null);
                continue;
            }

            var value = reader.GetValue(i);
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (targetType == typeof(DateTime))
                value = DateTime.Parse(value.ToString() ?? "", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);

            property.SetValue(entity, Convert.ChangeType(value, targetType));
        }
        return entity;
    }
}
