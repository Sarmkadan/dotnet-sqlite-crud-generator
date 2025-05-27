// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for generating CRUD operations, migrations, and gRPC services from C# models.
/// </summary>
public class GenerationService
{
    private readonly string _outputPath;

    public GenerationService(string outputPath = "./Generated")
    {
        _outputPath = outputPath;
        Directory.CreateDirectory(_outputPath);
    }

    /// <summary>
    /// Generates a repository interface for a given entity type.
    /// </summary>
    public async Task<string> GenerateRepositoryInterfaceAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

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
        sb.AppendLine($"    Task<{entityType.Name}?> GetByIdAsync(int id, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task<IEnumerable<{entityType.Name}>> GetAllAsync(CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task<{entityType.Name}> AddAsync({entityType.Name} entity, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task<{entityType.Name}> UpdateAsync({entityType.Name} entity, CancellationToken cancellationToken = default);");
        sb.AppendLine($"    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);");
        sb.AppendLine("}");

        var filePath = Path.Combine(_outputPath, $"I{entityType.Name}Repository.cs");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Generates a migration SQL script for a given entity type.
    /// </summary>
    public async Task<string> GenerateMigrationAsync(Type entityType, string migrationName, CancellationToken cancellationToken = default)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType));

        ValidateEntityType(entityType);

        var sb = new StringBuilder();
        sb.AppendLine($"-- Migration: {migrationName}");
        sb.AppendLine($"-- Entity: {entityType.Name}");
        sb.AppendLine($"-- Generated: {DateTime.UtcNow:O}");
        sb.AppendLine();

        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var columns = GenerateColumnDefinitions(properties);

        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {entityType.Name}s (");
        sb.AppendLine(string.Join(",\n    ", columns));
        sb.AppendLine(");");

        var filePath = Path.Combine(_outputPath, $"{DateTime.UtcNow:yyyyMMddHHmmss}_{migrationName}.sql");
        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);

        return filePath;
    }

    /// <summary>
    /// Generates a gRPC service definition for a given entity type.
    /// </summary>
    public async Task<string> GenerateGrpcServiceAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType == null)
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
            var protoType = MapCSharpToProtoType(prop.PropertyType);
            sb.AppendLine($"    {protoType} {prop.Name.ToLower()} = {fieldNumber};");
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

        if (!properties.Any(p => p.Name == "Id"))
            throw GenerationException.InvalidModel(entityType.Name, "Entity must have an 'Id' property");

        if (properties.Length < 2)
            throw GenerationException.InvalidModel(entityType.Name, "Entity must have at least 2 properties");
    }

    private List<string> GenerateColumnDefinitions(PropertyInfo[] properties)
    {
        var columns = new List<string>();

        foreach (var prop in properties)
        {
            var columnDef = prop.Name switch
            {
                "Id" => "Id INTEGER PRIMARY KEY AUTOINCREMENT",
                "CreatedAt" or "UpdatedAt" => $"{prop.Name} TEXT NOT NULL",
                _ when prop.PropertyType == typeof(string) => $"{prop.Name} TEXT",
                _ when prop.PropertyType == typeof(int) => $"{prop.Name} INTEGER",
                _ when prop.PropertyType == typeof(decimal) => $"{prop.Name} REAL",
                _ when prop.PropertyType == typeof(bool) => $"{prop.Name} INTEGER",
                _ when prop.PropertyType == typeof(DateTime) => $"{prop.Name} TEXT",
                _ => $"{prop.Name} TEXT"
            };

            if (prop.Name == "Id") continue;
            columns.Add(columnDef);
        }

        return columns;
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
}
