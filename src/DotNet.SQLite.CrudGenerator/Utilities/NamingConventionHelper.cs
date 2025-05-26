#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Helper for applying naming conventions to model properties.
/// Supports C#, SQL, gRPC, and other naming conventions.
/// </summary>
public static class NamingConventionHelper
{
    // Reflection results are stable per type — cache once, reuse forever.
    private static readonly ConcurrentDictionary<Type, string> _tableNameCache = new();
    private static readonly ConcurrentDictionary<Type, string> _tableNameSingularCache = new();
    private static readonly ConcurrentDictionary<PropertyInfo, string> _columnNameCache = new();

    // FrozenDictionary: immutable after construction, 30-40 % faster lookup than Dictionary.
    private static readonly FrozenDictionary<Type, string> _sqlTypeMappings =
        new Dictionary<Type, string>
        {
            [typeof(bool)]           = "INTEGER",
            [typeof(byte)]           = "INTEGER",
            [typeof(short)]          = "INTEGER",
            [typeof(int)]            = "INTEGER",
            [typeof(long)]           = "INTEGER",
            [typeof(float)]          = "REAL",
            [typeof(double)]         = "REAL",
            [typeof(decimal)]        = "REAL",
            [typeof(string)]         = "TEXT",
            [typeof(char)]           = "TEXT",
            [typeof(DateTime)]       = "TEXT",
            [typeof(DateTimeOffset)] = "TEXT",
            [typeof(Guid)]           = "TEXT",
            [typeof(byte[])]         = "BLOB",
        }.ToFrozenDictionary();

    /// <summary>
    /// Returns the SQLite column type for a given CLR type.
    /// Nullable&lt;T&gt; unwraps to its underlying type before lookup.
    /// </summary>
    public static string GetSqlType(Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return _sqlTypeMappings.TryGetValue(underlying, out var sqlType) ? sqlType : "TEXT";
    }

    /// <summary>
    /// Converts C# property names to SQL table/column names.
    /// Example: UserId -> user_id, FirstName -> first_name
    /// </summary>
    public static string ToCSharpToSqlConvention(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return propertyName.ToSnakeCase();
    }

    /// <summary>
    /// Converts SQL column names to C# property names.
    /// Example: user_id -> UserId, first_name -> FirstName
    /// </summary>
    public static string ToSqlToCSharpConvention(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return columnName;

        return columnName.ToPascalCase();
    }

    /// <summary>
    /// Gets the database table name for a type.
    /// Pluralizes class name and applies naming convention.
    /// Results are cached per type after the first call.
    /// </summary>
    public static string GetTableName(Type entityType, bool pluralize = true)
    {
        if (pluralize)
            return _tableNameCache.GetOrAdd(entityType, static t => t.Name.Pluralize().ToSnakeCase());

        return _tableNameSingularCache.GetOrAdd(entityType, static t => t.Name.ToSnakeCase());
    }

    /// <summary>
    /// Gets the database column name for a property.
    /// Results are cached per PropertyInfo after the first call.
    /// </summary>
    public static string GetColumnName(PropertyInfo property)
    {
        return _columnNameCache.GetOrAdd(property, static p =>
        {
            var columnAttr = p.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr is not null && !string.IsNullOrEmpty(columnAttr.Name))
                return columnAttr.Name;
            return p.Name.ToSnakeCase();
        });
    }

    /// <summary>
    /// Gets the gRPC service name for a type.
    /// Example: ProductService -> product_service or ProductService depending on convention
    /// </summary>
    public static string GetGrpcServiceName(string className)
    {
        if (className.EndsWith("Service"))
            return className; // Keep as-is for service names

        return className + "Service";
    }

    /// <summary>
    /// Gets the gRPC message name for a type.
    /// </summary>
    public static string GetGrpcMessageName(string className)
    {
        if (className.EndsWith("Message"))
            return className;

        return className + "Message";
    }

    /// <summary>
    /// Gets the API endpoint for an entity.
    /// Example: Product -> /api/products
    /// </summary>
    public static string GetApiEndpoint(Type entityType, string apiVersion = "v1")
    {
        var entityName = entityType.Name.Pluralize().ToLower();
        return $"/api/{apiVersion}/{entityName}";
    }

    /// <summary>
    /// Validates a property name follows C# naming conventions.
    /// </summary>
    public static bool IsValidPropertyName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return false;

        var span = propertyName.AsSpan();
        if (!char.IsLetter(span[0]) && span[0] != '_')
            return false;

        // Span loop avoids the LINQ delegate allocation per character.
        foreach (char c in span)
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;

        return true;
    }

    /// <summary>
    /// Gets naming convention info for display purposes.
    /// </summary>
    public static NamingConventionInfo GetConventionInfo(Type entityType)
    {
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return new NamingConventionInfo
        {
            EntityName = entityType.Name,
            TableName = GetTableName(entityType),
            ApiEndpoint = GetApiEndpoint(entityType),
            GrpcServiceName = GetGrpcServiceName(entityType.Name),
            Properties = properties.Select(p => new PropertyConventionInfo
            {
                PropertyName = p.Name,
                ColumnName = GetColumnName(p),
                Type = p.PropertyType.Name
            }).ToList()
        };
    }
}

public sealed class ColumnAttribute : Attribute
{
    public string? Name { get; set; }

    public ColumnAttribute() { }
    public ColumnAttribute(string name)
    {
        Name = name;
    }
}

public sealed class NamingConventionInfo
{
    public string EntityName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string GrpcServiceName { get; set; } = string.Empty;
    public List<PropertyConventionInfo> Properties { get; set; } = new();
}

public sealed class PropertyConventionInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
