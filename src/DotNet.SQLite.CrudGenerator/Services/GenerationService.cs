#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using DotNet.SQLite.CrudGenerator.Attributes;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for generating CRUD operations, migrations, and gRPC services from C# models.
/// </summary>
public sealed class GenerationService
{
    private readonly string _outputPath;
    private readonly SoftDeleteOptions _softDeleteOptions;

    public GenerationService(string outputPath = "./Generated", SoftDeleteOptions? softDeleteOptions = null)
    {
        _outputPath = outputPath;
        _softDeleteOptions = softDeleteOptions ?? new SoftDeleteOptions { Enabled = false };
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Generates a repository interface for a given entity type.
    /// Supports both single-column primary keys and composite primary keys declared via
    /// <see cref="CompositeKeyAttribute"/>.
    /// </summary>
    public async Task<string> GenerateRepositoryInterfaceAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var compositeKeyProps = properties
            .Where(p => p.GetCustomAttribute<CompositeKeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<CompositeKeyAttribute>()!.Order)
            .ToList();

        // Build key parameter list: "int id" for single-key, or "Guid userId, Guid roleId" for composite.
        string keyParams;
        string keyArgs;
        if (compositeKeyProps.Count >= 2)
        {
            keyParams = string.Join(", ", compositeKeyProps.Select(p => $"{GetFriendlyTypeName(p.PropertyType)} {char.ToLowerInvariant(p.Name[0]) + p.Name[1..]}"));
            keyArgs = string.Join(", ", compositeKeyProps.Select(p => $"{char.ToLowerInvariant(p.Name[0]) + p.Name[1..]}"));
        }
        else
        {
            keyParams = "int id";
            keyArgs = "id";
        }

        var sb = new StringBuilder();
        sb.AppendLine("// =============================================================================");
        sb.AppendLine("// Author: Vladyslav Zaiets | https://sarmkadan.com");
        sb.AppendLine("// CTO & Software Architect");
        sb.AppendLine("// =============================================================================");
        sb.AppendLine();
        sb.AppendLine("namespace DotNet.SQLite.CrudGenerator.Repositories;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Repository interface for {entityType.Name} entities.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public interface I{entityType.Name}Repository");
        sb.AppendLine("{");
        sb.AppendLine($" Task<{entityType.Name}?> GetByIdAsync({keyParams}, CancellationToken cancellationToken = default);");

        sb.AppendLine($" Task<IEnumerable<{entityType.Name}>> GetAllAsync(CancellationToken cancellationToken = default);");

        sb.AppendLine($" Task<{entityType.Name}> AddAsync({entityType.Name} entity, CancellationToken cancellationToken = default);");
        sb.AppendLine($" Task<{entityType.Name}> UpdateAsync({entityType.Name} entity, CancellationToken cancellationToken = default);");

        // Update DeleteAsync to use soft-delete when enabled
        if (_softDeleteOptions.Enabled)
        {
            sb.AppendLine($" Task<bool> DeleteAsync({keyParams}, CancellationToken cancellationToken = default);");
        }
        else
        {
            sb.AppendLine($" Task<bool> DeleteAsync({keyParams}, CancellationToken cancellationToken = default);");
        }
        sb.AppendLine("}");

        var filePath = Path.Combine(_outputPath, $"I{entityType.Name}Repository.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Generates a migration SQL script for a given entity type.
    /// Supports both single-column primary keys (conventional <c>Id</c> property) and
    /// composite primary keys declared via <see cref="CompositeKeyAttribute"/>.
    /// </summary>
    public async Task<string> GenerateMigrationAsync(Type entityType, string migrationName, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

        var sb = new StringBuilder();
        sb.AppendLine($"-- Migration: {migrationName}");
        sb.AppendLine($"-- Entity: {entityType.Name}");
        sb.AppendLine($"-- Generated: {DateTime.UtcNow:O}");
        sb.AppendLine();

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var compositeKeyProps = properties
            .Where(p => p.GetCustomAttribute<CompositeKeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<CompositeKeyAttribute>()!.Order)
            .ToList();

        var columns = compositeKeyProps.Count >= 2
            ? GenerateCompositeKeyColumnDefinitions(properties, compositeKeyProps)
            : GenerateColumnDefinitions(properties);

        // Add IsDeleted column when soft-delete is enabled
        if (_softDeleteOptions.Enabled && !columns.Any(c => c.Contains(_softDeleteOptions.ColumnName)))
        {
            columns.Insert(1, $"{_softDeleteOptions.ColumnName} INTEGER NOT NULL DEFAULT {_softDeleteOptions.ActiveValue}");
        }

        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {entityType.Name}s (");
        sb.AppendLine(string.Join(",\n    ", columns));
        sb.AppendLine(");");

        var filePath = Path.Combine(_outputPath, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{migrationName}.sql");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Generates a gRPC service definition for a given entity type.
    /// Nullable reference type properties (e.g. <c>string?</c>) and nullable value types
    /// (e.g. <c>int?</c>) are emitted with the proto3 <c>optional</c> modifier so that absent
    /// fields are not confused with default values and do not cause <see cref="NullReferenceException"/>
    /// in generated service stubs.
    /// </summary>
    public async Task<string> GenerateGrpcServiceAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

        var sb = new StringBuilder();
        sb.AppendLine("syntax = \"proto3\";");
        sb.AppendLine();
        sb.AppendLine($"package generated.{entityType.Name.ToLower()};");
        sb.AppendLine();
        sb.AppendLine($"message {entityType.Name}Request {{");
        sb.AppendLine("    int32 id = 1;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"message {entityType.Name}Response {{");

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        int fieldNumber = 1;

        foreach (var prop in properties)
        {
            var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlying is not null;
            var resolvedType = underlying ?? prop.PropertyType;
            var protoType = MapCSharpToProtoType(resolvedType);
            var modifier = isNullable ? "optional " : "";
            sb.AppendLine($"    {modifier}{protoType} {prop.Name.ToLower()} = {fieldNumber};");
            fieldNumber++;
        }

        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"service {entityType.Name}Service {{");
        sb.AppendLine($"    rpc Get({entityType.Name}Request) returns ({entityType.Name}Response);");
        sb.AppendLine($"    rpc Create({entityType.Name}Response) returns ({entityType.Name}Response);");
        sb.AppendLine($"    rpc Update({entityType.Name}Response) returns ({entityType.Name}Response);");
        sb.AppendLine($"    rpc Delete({entityType.Name}Request) returns ({entityType.Name}Response);");
        sb.AppendLine("}");

        var filePath = Path.Combine(_outputPath, $"{entityType.Name}.proto");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Validates that a type is a valid entity.
    /// </summary>
    private void ValidateEntityType(Type entityType)
    {
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var hasCompositeKey = properties.Any(p =>
            p.GetCustomAttribute<CompositeKeyAttribute>() is not null);

        if (!hasCompositeKey && !properties.Any(p => p.Name == "Id"))
            throw new GenerationException($"Entity must have an 'Id' property. (Parameter '{entityType.Name}')");

        if (properties.Length < 2)
            throw new GenerationException($"Entity must have at least 2 properties. (Parameter '{entityType.Name}')");
    }

    private List<string> GenerateColumnDefinitions(PropertyInfo[] properties)
    {
        var columns = new List<string>();

        // Id column must come first as the primary key.
        var idProp = Array.Find(properties, p => p.Name == "Id");
        if (idProp is not null)
            columns.Add("Id INTEGER PRIMARY KEY AUTOINCREMENT");

        // Add IsDeleted column when soft-delete is enabled
        if (_softDeleteOptions.Enabled && !columns.Any(c => c.Contains(_softDeleteOptions.ColumnName)))
        {
            columns.Add($"{_softDeleteOptions.ColumnName} INTEGER NOT NULL DEFAULT {_softDeleteOptions.ActiveValue}");
        }

        foreach (var prop in properties)
        {
            if (prop.Name == "Id") continue;

            var notNull = IsNotNullable(prop) ? " NOT NULL" : "";

            var columnDef = prop.Name switch
            {
                "CreatedAt" or "UpdatedAt" => $"{prop.Name} TEXT NOT NULL",
                _ when prop.PropertyType == typeof(string) => $"{prop.Name} TEXT{notNull}",
                _ when prop.PropertyType == typeof(int) => $"{prop.Name} INTEGER{notNull}",
                _ when prop.PropertyType == typeof(decimal) => $"{prop.Name} REAL{notNull}",
                _ when prop.PropertyType == typeof(bool) => $"{prop.Name} INTEGER{notNull}",
                _ when prop.PropertyType == typeof(DateTime) => $"{prop.Name} TEXT{notNull}",
                _ when Nullable.GetUnderlyingType(prop.PropertyType) is not null => $"{prop.Name} TEXT",
                _ => $"{prop.Name} TEXT{notNull}"
            };

            columns.Add(columnDef);
        }

        return columns;
    }

    private List<string> GenerateCompositeKeyColumnDefinitions(PropertyInfo[] properties, List<PropertyInfo> compositeKeyProps)
    {
        var columns = new List<string>();
        var compositeKeyNames = compositeKeyProps.Select(p => p.Name).ToHashSet();

        // Emit column definitions for all properties (no single PRIMARY KEY inline).
        foreach (var prop in properties)
        {
            var notNull = IsNotNullable(prop) ? " NOT NULL" : "";

            string columnDef;
            if (compositeKeyNames.Contains(prop.Name))
            {
                // Composite key columns are always NOT NULL.
                columnDef = prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long)
                    ? $"{prop.Name} INTEGER NOT NULL"
                    : $"{prop.Name} TEXT NOT NULL";
            }
            else
            {
                columnDef = prop.Name switch
                {
                    "CreatedAt" or "UpdatedAt" => $"{prop.Name} TEXT NOT NULL",
                    _ when prop.PropertyType == typeof(string) => $"{prop.Name} TEXT{notNull}",
                    _ when prop.PropertyType == typeof(int) => $"{prop.Name} INTEGER{notNull}",
                    _ when prop.PropertyType == typeof(decimal) => $"{prop.Name} REAL{notNull}",
                    _ when prop.PropertyType == typeof(bool) => $"{prop.Name} INTEGER{notNull}",
                    _ when prop.PropertyType == typeof(DateTime) => $"{prop.Name} TEXT{notNull}",
                    _ when Nullable.GetUnderlyingType(prop.PropertyType) is not null => $"{prop.Name} TEXT",
                    _ => $"{prop.Name} TEXT{notNull}"
                };
            }

            columns.Add(columnDef);
        }

        // Append the composite PRIMARY KEY constraint.
        var pkCols = string.Join(", ", compositeKeyProps.Select(p => p.Name));
        columns.Add($"PRIMARY KEY ({pkCols})");

        return columns;
    }

    /// <summary>
    /// Returns a C# friendly type name suitable for use in generated source files.
    /// </summary>
    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(string)) return "string";
        if (type == typeof(Guid)) return "Guid";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(DateTime)) return "DateTime";
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null) return $"{GetFriendlyTypeName(underlying)}?";
        return type.Name;
    }


    /// <summary>
    /// Returns <see langword="true"/> when a property cannot hold a null value.
    /// Value types are always non-nullable; reference types are non-nullable when they carry
    /// a <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/>.
    /// Nullable value types (<c>int?</c>, <c>decimal?</c>, etc.) are treated as nullable.
    /// </summary>
    private static bool IsNotNullable(PropertyInfo prop)
    {
        // Nullable<T> value types (int?, decimal?, …) are explicitly nullable.
        if (Nullable.GetUnderlyingType(prop.PropertyType) is not null)
            return false;

        // Non-nullable value types are never null.
        if (prop.PropertyType.IsValueType)
            return true;

        // Reference types are non-nullable when decorated with [Required].
        return prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() is not null;
    }

    private string MapCSharpToProtoType(Type type) => type switch
    {
        _ when type == typeof(string) => "string",
        _ when type == typeof(int) => "int32",
        _ when type == typeof(long) => "int64",
        _ when type == typeof(decimal) => "double",
        _ when type == typeof(bool) => "bool",
        _ when type == typeof(DateTime) => "string",
        _ => "string"
    };

    /// <summary>
    /// Generates a repository implementation for a given entity type.
    /// Supports both single-column primary keys and composite primary keys declared via
    /// <see cref="CompositeKeyAttribute"/>.
    /// </summary>
    public async Task<string> GenerateRepositoryImplementationAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var compositeKeyProps = properties
            .Where(p => p.GetCustomAttribute<CompositeKeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<CompositeKeyAttribute>()!.Order)
            .ToList();

        // Build key parameter list: "int id" for single-key, or "Guid userId, Guid roleId" for composite.
        string keyParams;
        string keyArgs;
        if (compositeKeyProps.Count >= 2)
        {
            keyParams = string.Join(", ", compositeKeyProps.Select(p => $"{GetFriendlyTypeName(p.PropertyType)} {char.ToLowerInvariant(p.Name[0]) + p.Name[1..]}"));
            keyArgs = string.Join(", ", compositeKeyProps.Select(p => $"{char.ToLowerInvariant(p.Name[0]) + p.Name[1..]}"));
        }
        else
        {
            keyParams = "int id";
            keyArgs = "id";
        }

        var sb = new StringBuilder();
        sb.AppendLine("// =============================================================================");
        sb.AppendLine("// Author: Vladyslav Zaiets | https://sarmkadan.com");
        sb.AppendLine("// CTO & Software Architect");
        sb.AppendLine("// =============================================================================");
        sb.AppendLine();
        sb.AppendLine("using System.Data.Common;");
        sb.AppendLine("using DotNet.SQLite.CrudGenerator.Data;");
        sb.AppendLine("using DotNet.SQLite.CrudGenerator.Interfaces;");
        sb.AppendLine("using Microsoft.Data.Sqlite;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine("namespace DotNet.SQLite.CrudGenerator.Repositories;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Repository implementation for {entityType.Name} entities.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public sealed class {entityType.Name}Repository : Repository<{entityType.Name}, {(compositeKeyProps.Count >= 2 ? "(int, int)" : "int")}>, I{entityType.Name}Repository");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Initializes a new instance of the <see cref=\\\"{entityType.Name}Repository\\\"/> class.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    /// <param name=\\\"database\\\">The database connection.</param>");
        sb.AppendLine($"    /// <param name=\\\"logger\\\">The logger.</param>");
        sb.AppendLine($"    public {entityType.Name}Repository(DatabaseConnection database, ILogger<{entityType.Name}Repository>? logger = null)");
        sb.AppendLine("        : base(database, logger)");
        sb.AppendLine("    {");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate GetByIdAsync
        sb.AppendLine($"    public override async Task<{entityType.Name}?> GetByIdAsync({keyParams}, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        if (_softDeleteOptions.Enabled)
        {
            sb.AppendLine($"        return await base.GetByIdAsync({keyArgs}, cancellationToken);");
        }
        else
        {
            sb.AppendLine($"        return await base.GetByIdAsync({keyArgs}, cancellationToken);");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate GetAllAsync
        if (_softDeleteOptions.Enabled)
        {
            sb.AppendLine($"    public override async Task<IEnumerable<{entityType.Name}>> GetAllAsync(CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (_cacheLoaded)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return _cache.AsReadOnly().Where(e => EF.Property<int>(e, _softDeleteOptions.ColumnName) == _softDeleteOptions.ActiveValue);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        await _database.OpenAsync(cancellationToken);");
            sb.AppendLine();
            sb.AppendLine("        using var command = _database.Connection.CreateCommand();");
            sb.AppendLine($"        command.CommandText = $\"SELECT * FROM {entityType.Name}s WHERE {_softDeleteOptions.ColumnName} = {_softDeleteOptions.ActiveValue}\";");
            sb.AppendLine();
            sb.AppendLine("        using var reader = await command.ExecuteReaderAsync(cancellationToken);");
            sb.AppendLine($"        var results = new List<{entityType.Name}>();");
            sb.AppendLine("        while (await reader.ReadAsync(cancellationToken))");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = MapFromReader(reader);");
            sb.AppendLine("            if (!_cache.Contains(entity))");
            sb.AppendLine("                _cache.Add(entity);");
            sb.AppendLine("            results.Add(entity);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        _cache = results;");
            sb.AppendLine("        _cacheLoaded = true;");
            sb.AppendLine("        return _cache.AsReadOnly();");
            sb.AppendLine("    }");
        }
        else
        {
            sb.AppendLine($"    public override async Task<IEnumerable<{entityType.Name}>> GetAllAsync(CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        return await base.GetAllAsync(cancellationToken);");
            sb.AppendLine("    }");
        }
        sb.AppendLine();

        // Generate AddAsync
        sb.AppendLine($"    public override async Task<{entityType.Name}> AddAsync({entityType.Name} entity, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        return await base.AddAsync(entity, cancellationToken);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate UpdateAsync
        sb.AppendLine($"    public override async Task<{entityType.Name}> UpdateAsync({entityType.Name} entity, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        return await base.UpdateAsync(entity, cancellationToken);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate DeleteAsync
        if (_softDeleteOptions.Enabled)
        {
            sb.AppendLine($"    public override async Task<bool> DeleteAsync({keyParams}, CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        await _database.OpenAsync(cancellationToken);");
            sb.AppendLine();
            sb.AppendLine("        using var command = _database.Connection.CreateCommand();");
            sb.AppendLine($"        command.CommandText = $\"UPDATE {entityType.Name}s SET {_softDeleteOptions.ColumnName} = {_softDeleteOptions.DeletedValue} WHERE Id = @id\";");
            sb.AppendLine("        command.Parameters.AddWithValue(\"@id\", id!);");
            sb.AppendLine();
            sb.AppendLine("        var affected = await command.ExecuteNonQueryAsync(cancellationToken);");
            sb.AppendLine("        if (affected > 0)");
            sb.AppendLine("        {");
            sb.AppendLine("            var cachedEntity = _cache.FirstOrDefault(e => GetId(e)?.Equals(id) == true);");
            sb.AppendLine("            if (cachedEntity is not null)");
            sb.AppendLine("            {");
            sb.AppendLine("                var cachedIndex = _cache.FindIndex(e => GetId(e)?.Equals(id) == true);");
            sb.AppendLine("                if (cachedIndex >= 0)");
            sb.AppendLine("                    _cache[cachedIndex] = entity;");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return affected > 0;");
            sb.AppendLine("    }");
        }
        else
        {
            sb.AppendLine($"    public override async Task<bool> DeleteAsync({keyParams}, CancellationToken cancellationToken = default)");
            sb.AppendLine("    {");
            sb.AppendLine("        return await base.DeleteAsync(id, cancellationToken);");
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");

        var filePath = Path.Combine(_outputPath, $"{entityType.Name}Repository.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }
}