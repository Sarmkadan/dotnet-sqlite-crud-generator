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
using DotNet.SQLite.CrudGenerator.Constants;
using Microsoft.Data.Sqlite;

using Microsoft.Extensions.Logging;
namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Generic repository implementation for SQLite with CRUD operations and caching.
/// </summary>
public abstract class Repository<T, TKey> : IRepository<T, TKey> where T : class
{
    private const string DefaultPrimaryKeyColumn = "Id";
    private const string TableNameSuffix = "s";
    private const string IdParameterName = "@id";
    private const string PositionalParameterPrefix = "@p";
    private const string ColumnSeparator = ", ";
    private const string UnknownKeyValue = "Unknown";
    private const int SqliteConstraintErrorCode = 19;

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
    private readonly object _cacheLock = new();
    private long _cacheVersion;

    protected Repository(DatabaseConnection database, ILogger<Repository<T, TKey>>? logger = null)
    {
        _logger = logger;
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _tableName = typeof(T).Name + TableNameSuffix;
        _primaryKeyColumn = DefaultPrimaryKeyColumn;
    }

    /// <summary>
    /// Retrieves an entity by its ID from the database.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when a database error occurs.</exception>
    public virtual async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (id is null)
            throw new ArgumentNullException(nameof(id));

        _logger?.LogDebug("Retrieving entity {EntityType} with ID {EntityId} from database", typeof(T).Name, id);

        try
        {
            await _database.OpenAsync(cancellationToken);

            using var command = _database.Connection.CreateCommand();
            command.CommandText = string.Format(SqlConstants.QueryTemplates.SelectByIdWithLimit, _tableName, _primaryKeyColumn, IdParameterName);
            command.Parameters.AddWithValue(IdParameterName, id!);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var entity = MapFromReader(reader);
                lock (_cacheLock)
                {
                    var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);
                    if (cachedIndex >= 0)
                        _cache[cachedIndex] = entity;
                    else
                        _cache.Add(entity);
                    _cacheVersion++;
                }
                _logger?.LogInformation("Successfully retrieved entity {EntityType} with ID {EntityId} from database", typeof(T).Name, id);
                return entity;
            }

            lock (_cacheLock)
            {
                if (_cache.RemoveAll(e => GetId(e)?.Equals(id) == true) > 0)
                    _cacheVersion++;
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

    /// <summary>
    /// Retrieves all entities from the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of all entities.</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Retrieving all entities {EntityType} from table {TableName}", typeof(T).Name, _tableName);

        lock (_cacheLock)
        {
            if (_cacheLoaded)
            {
                var cached = _cache.ToList().AsReadOnly();
                _logger?.LogDebug("Returning {EntityCount} cached entities {EntityType} from table {TableName}", cached.Count, typeof(T).Name, _tableName);
                return cached;
            }
        }

        while (true)
        {
            long cacheVersion;
            lock (_cacheLock)
                cacheVersion = _cacheVersion;

            await _database.OpenAsync(cancellationToken);

            using var command = _database.Connection.CreateCommand();
            command.CommandText = string.Format(SqlConstants.QueryTemplates.SelectAllFromTable, _tableName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = new List<T>();
            while (await reader.ReadAsync(cancellationToken))
                results.Add(MapFromReader(reader));

            lock (_cacheLock)
            {
                if (_cacheLoaded)
                    return _cache.ToList().AsReadOnly();

                if (_cacheVersion != cacheVersion)
                    continue;

                _cache = results;
                _cacheLoaded = true;
                _cacheVersion++;
                _logger?.LogInformation("Successfully retrieved {EntityCount} entities {EntityType} from table {TableName}", _cache.Count, typeof(T).Name, _tableName);
                return _cache.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The function to test each element for a condition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A collection of entities that match the predicate.</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate);
    }

    /// <summary>
    /// Counts entities in the database, optionally filtered by a predicate.
    /// </summary>
    /// <param name="predicate">The function to test each element for a condition. If null, counts all entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities matching the predicate.</returns>
    public virtual async Task<int> CountAsync(Func<T, bool>? predicate = null, CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = string.Format(SqlConstants.QueryTemplates.CountAllFromTable, _tableName);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        if (predicate is null) return count;

        var all = await GetAllAsync(cancellationToken);
        return all.Count(predicate);
    }

    /// <summary>
    /// Adds a new entity to the database.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The added entity with its ID populated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when a database error occurs.</exception>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        _logger?.LogDebug("Adding new entity {EntityType} to table {TableName}", typeof(T).Name, _tableName);

        await _database.OpenAsync(cancellationToken);

        var columns = GetProperties();
        var values = columns.Select(p => GetPropertyValue(entity, p)).ToList();
        var columnNames = string.Join(ColumnSeparator, columns.Select(p => p.Name));
        var placeholders = string.Join(ColumnSeparator, columns.Select((_, i) => $"{PositionalParameterPrefix}{i}"));

        using var command = _database.Connection.CreateCommand();
        command.CommandText = string.Format(SqlConstants.QueryTemplates.InsertIntoTable, _tableName, columnNames, placeholders);

        for (int i = 0; i < values.Count; i++)
        {
            command.Parameters.AddWithValue($"{PositionalParameterPrefix}{i}", values[i] ?? DBNull.Value);
        }

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);

            // Retrieve the last inserted row ID
            using var lastIdCommand = _database.Connection.CreateCommand();
            lastIdCommand.CommandText = SqlConstants.QueryTemplates.LastInsertRowId;
            var lastId = await lastIdCommand.ExecuteScalarAsync(cancellationToken);

            // Set the Id property of the entity
            var idProperty = typeof(T).GetProperty(_primaryKeyColumn);
            if (idProperty is not null && idProperty.CanWrite)
            {
                var convertedId = Convert.ChangeType(lastId, typeof(TKey));
                idProperty.SetValue(entity, convertedId);
            }

            lock (_cacheLock)
            {
                _cache.RemoveAll(e => GetId(e)?.Equals(GetId(entity)) == true);
                _cache.Add(entity);
                _cacheVersion++;
            }
            _logger?.LogInformation("Successfully added entity {EntityType} with ID {EntityId} to table {TableName}", typeof(T).Name, lastId, _tableName);
            return entity;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraintErrorCode)
        {
            _logger?.LogWarning(ex, "Duplicate key error while adding entity {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw RepositoryException.DuplicateKey(typeof(T).Name, UnknownKeyValue, entity);
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

    /// <summary>
    /// Adds a collection of entities to the database in a single transaction.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The added entities with their IDs populated.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when a database error occurs.</exception>
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
            var columnNames = string.Join(ColumnSeparator, columns.Select(p => p.Name));
            var placeholders = string.Join(ColumnSeparator, columns.Select((_, i) => $"{PositionalParameterPrefix}{i}"));

            var command = _database.Connection.CreateCommand();
            command.CommandText = string.Format(SqlConstants.QueryTemplates.InsertIntoTableReturning, _tableName, columnNames, placeholders);

            // Add parameters once
            for (int i = 0; i < columns.Count; i++)
            {
                var param = new SqliteParameter();
                param.ParameterName = $"{PositionalParameterPrefix}{i}";
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
                }
            }

            transaction.Commit();
            lock (_cacheLock)
            {
                foreach (var insertedEntity in results)
                {
                    var insertedId = GetId(insertedEntity);
                    _cache.RemoveAll(e => GetId(e)?.Equals(insertedId) == true);
                    _cache.Add(insertedEntity);
                }
                _cacheVersion++;
            }
            return results;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraintErrorCode)
        {
            _logger?.LogWarning(ex, "Duplicate key error while adding batch of entities {EntityType} to table {TableName}", typeof(T).Name, _tableName);
            throw RepositoryException.DuplicateKey(typeof(T).Name, UnknownKeyValue, entityList.FirstOrDefault() ?? entityList.First());
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

    /// <summary>
    /// Updates an existing entity in the database.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the entity was updated; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="RepositoryException">Thrown when a database error occurs.</exception>
    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetId(entity);
        _logger?.LogDebug("Attempting to update entity {EntityType} with ID {EntityId} in table {TableName}", typeof(T).Name, id, _tableName);

        await _database.OpenAsync(cancellationToken);

        var properties = GetProperties();
        var updates = string.Join(ColumnSeparator, properties.Select((p, i) => $"{p.Name} = {PositionalParameterPrefix}{i}"));

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"UPDATE {_tableName} SET {updates} WHERE {_primaryKeyColumn} = {IdParameterName}";

        for (int i = 0; i < properties.Count; i++)
        {
            command.Parameters.AddWithValue($"{PositionalParameterPrefix}{i}", GetPropertyValue(entity, properties[i]) ?? DBNull.Value);
        }

        command.Parameters.AddWithValue(IdParameterName, id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
        {
            _logger?.LogWarning("Entity {EntityType} with ID {EntityId} not found for update in table {TableName}", typeof(T).Name, id, _tableName);
            // Fix: Safe casting of potentially null id value
            throw RepositoryException.EntityNotFound(typeof(T).Name, id is null ? 0 : Convert.ToInt32(id));
        }

        lock (_cacheLock)
        {
            var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);
            if (cachedIndex >= 0)
                _cache[cachedIndex] = entity;
            else
                _cache.Add(entity);
            _cacheVersion++;
        }

        _logger?.LogInformation("Successfully updated entity {EntityType} with ID {EntityId} in table {TableName}", typeof(T).Name, id, _tableName);
        return affected > 0; // Return true if at least one row was affected
    }

    /// <summary>
    /// Deletes an entity with the specified ID from the database.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the entity was deleted; otherwise, false.</returns>
    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Attempting to delete entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);

        await _database.OpenAsync(cancellationToken);

        using var command = _database.Connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE {_primaryKeyColumn} = {IdParameterName}";
        command.Parameters.AddWithValue(IdParameterName, id!);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected > 0)
        {
            lock (_cacheLock)
            {
                _cache.RemoveAll(e => GetId(e)?.Equals(id) == true);
                _cacheVersion++;
            }
            _logger?.LogInformation("Successfully deleted entity {EntityType} with ID {EntityId} from table {TableName}", typeof(T).Name, id, _tableName);
        }
        else
        {
            _logger?.LogDebug("Entity {EntityType} with ID {EntityId} not found for deletion in table {TableName}", typeof(T).Name, id, _tableName);
        }

        return affected > 0;
    }

    /// <summary>
    /// Deletes an entity from the database by entity reference.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the entity was deleted; otherwise, false.</returns>
    public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var id = GetId(entity);
        _logger?.LogDebug("Deleting entity {EntityType} with ID {EntityId} via entity reference", typeof(T).Name, id);
        return await DeleteAsync(id!, cancellationToken);
    }

    /// <summary>
    /// Deletes a collection of entities with the specified IDs from the database.
    /// </summary>
    /// <param name="ids">The collection of entity IDs to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities that were deleted.</returns>
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

    /// <summary>
    /// Determines whether an entity with the specified ID exists in the database.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the entity exists; otherwise, false.</returns>
    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken) is not null;
    }

    /// <summary>
    /// Gets the number of entities in the cache.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of entities in the cache.</returns>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);
        lock (_cacheLock)
            return _cache.Count;
    }

    protected virtual List<PropertyInfo> GetProperties() =>
        _propertiesCache.GetOrAdd(typeof(T), static t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanRead && p.CanWrite && p.Name != DefaultPrimaryKeyColumn)
             .ToList());

    protected virtual object? GetPropertyValue(T entity, PropertyInfo property) =>
        property.PropertyType == typeof(DateTime)
            ? ((DateTime?)property.GetValue(entity))?.ToString("O", System.Globalization.CultureInfo.InvariantCulture)
            : property.GetValue(entity);

    protected virtual TKey? GetId(T entity)
    {
        var prop = _idPropertyCache.GetOrAdd(typeof(T), static t => t.GetProperty(DefaultPrimaryKeyColumn));
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
