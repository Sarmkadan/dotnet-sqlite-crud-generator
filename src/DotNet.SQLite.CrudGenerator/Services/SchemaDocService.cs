#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;
using DotNet.SQLite.CrudGenerator.Attributes;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for generating markdown documentation from entity metadata.
/// Emits markdown documents describing table schemas, columns, and indexes.
/// </summary>
public sealed class SchemaDocService
{
    /// <summary>
    /// Generates a markdown schema document for a given entity type.
    /// The document includes:
    /// - Table name heading
    /// - Column table (name/type/nullable/pk)
    /// - Generated index list
    /// </summary>
    /// <param name="entityType">The entity type to generate schema documentation for.</param>
    /// <returns>Markdown document as a string.</returns>
    public string GenerateSchemaDocument(Type entityType)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        var tableName = entityType.Name + "s";
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var compositeKeyProps = properties
            .Where(p => p.GetCustomAttribute<CompositeKeyAttribute>() is not null)
            .OrderBy(p => p.GetCustomAttribute<CompositeKeyAttribute>()!.Order)
            .ToList();

        var sb = new StringBuilder();

        // Header
        sb.AppendLine("# Database Schema Documentation");
        sb.AppendLine();
        sb.AppendLine($"## Table: `{tableName}`");
        sb.AppendLine();

        // Columns table
        sb.AppendLine("### Columns");
        sb.AppendLine();
        sb.AppendLine("| Name | Type | Nullable | PK |");
        sb.AppendLine("|------|------|----------|----|");

        // Primary key column(s)
        if (compositeKeyProps.Count >= 2)
        {
            foreach (var prop in compositeKeyProps)
            {
                var typeName = GetFriendlyTypeName(prop.PropertyType);
                var isNullable = IsNullableType(prop.PropertyType);
                sb.AppendLine($"| {prop.Name} | {typeName} | {isNullable} | ✓ (composite) |");
            }
        }
        else
        {
            // Single-column primary key (Id)
            var idProp = properties.FirstOrDefault(p => p.Name == "Id");
            if (idProp is not null)
            {
                var typeName = GetFriendlyTypeName(idProp.PropertyType);
                sb.AppendLine($"| Id | {typeName} | false | ✓ |");
            }
        }

        // Other columns
        foreach (var prop in properties)
        {
            // Skip Id if already shown as PK
            if (prop.Name == "Id" && compositeKeyProps.Count < 2)
                continue;

            // Skip composite key columns if already shown
            if (compositeKeyProps.Any(p => p.Name == prop.Name))
                continue;

            var typeName = GetFriendlyTypeName(prop.PropertyType);
            var isNullable = IsNullableType(prop.PropertyType);
            sb.AppendLine($"| {prop.Name} | {typeName} | {isNullable} | - |");
        }

        sb.AppendLine();

        // Indexes section
        sb.AppendLine("### Indexes");
        sb.AppendLine();

        // Primary key index
        if (compositeKeyProps.Count >= 2)
        {
            var pkCols = string.Join(", ", compositeKeyProps.Select(p => p.Name));
            sb.AppendLine($"- **Primary Key**: `{pkCols}`");
        }
        else
        {
            sb.AppendLine("- **Primary Key**: `Id`");
        }

        // Foreign key indexes (based on property names ending with "Id")
        var foreignKeyColumns = properties
            .Where(p => p.Name.EndsWith("Id", StringComparison.Ordinal) && p.Name != "Id")
            .ToList();

        if (foreignKeyColumns.Count > 0)
        {
            sb.AppendLine("- **Foreign Key Indexes**:");
            foreach (var fkCol in foreignKeyColumns)
            {
                sb.AppendLine($"  - `{tableName}.{fkCol.Name}`");
            }
        }

        // Unique indexes (based on Required attribute)
        var uniqueColumns = properties
            .Where(p => p.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() is not null
                       && p.PropertyType == typeof(string))
            .ToList();

        if (uniqueColumns.Count > 0)
        {
            sb.AppendLine("- **Unique Indexes**:");
            foreach (var uniqueCol in uniqueColumns)
            {
                sb.AppendLine($"  - `{tableName}.{uniqueCol.Name}`");
            }
        }

        // Soft delete column if enabled
        var softDeleteOptions = new SoftDeleteOptions { Enabled = false };
        if (properties.Any(p => p.Name == softDeleteOptions.ColumnName))
        {
            sb.AppendLine($"- **Soft Delete Column**: `{softDeleteOptions.ColumnName}`");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a friendly type name suitable for documentation.
    /// </summary>
    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "INTEGER";
        if (type == typeof(long)) return "INTEGER";
        if (type == typeof(string)) return "TEXT";
        if (type == typeof(Guid)) return "TEXT";
        if (type == typeof(decimal)) return "REAL";
        if (type == typeof(bool)) return "INTEGER";
        if (type == typeof(DateTime)) return "TEXT";

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            return GetFriendlyTypeName(underlying) + "?";
        }

        return type.Name;
    }

    /// <summary>
    /// Returns whether a type is nullable.
    /// </summary>
    private static string IsNullableType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return "✓";

        if (type.IsValueType)
            return "false";

        return "✓";
    }
}