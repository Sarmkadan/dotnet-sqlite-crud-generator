// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Helper for applying naming conventions to model properties.
/// Supports C#, SQL, gRPC, and other naming conventions.
/// </summary>
public static class NamingConventionHelper
{
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
    /// </summary>
    public static string GetTableName(Type entityType, bool pluralize = true)
    {
        var tableName = entityType.Name;
        if (pluralize)
            tableName = tableName.Pluralize();

        return tableName.ToSnakeCase();
    }

    /// <summary>
    /// Gets the database column name for a property.
    /// </summary>
    public static string GetColumnName(PropertyInfo property)
    {
        // Check for [Column] attribute if available
        var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
        if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.Name))
            return columnAttr.Name;

        return property.Name.ToSnakeCase();
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

        if (!char.IsLetter(propertyName[0]) && propertyName[0] != '_')
            return false;

        return propertyName.All(c => char.IsLetterOrDigit(c) || c == '_');
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

public class ColumnAttribute : Attribute
{
    public string? Name { get; set; }

    public ColumnAttribute() { }
    public ColumnAttribute(string name)
    {
        Name = name;
    }
}

public class NamingConventionInfo
{
    public string EntityName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string GrpcServiceName { get; set; } = string.Empty;
    public List<PropertyConventionInfo> Properties { get; set; } = new();
}

public class PropertyConventionInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
