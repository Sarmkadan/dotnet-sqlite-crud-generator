#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for type inspection and manipulation.
/// Provides utilities for working with reflection to analyze types and their properties.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Gets all public properties of a type, optionally filtered by attribute.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetProperties<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<TAttribute>() is not null);
    }

    /// <summary>
    /// Checks if a type is a simple/primitive type (int, string, bool, etc.).
    /// </summary>
    public static bool IsSimpleType(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid) ||
               underlyingType == typeof(byte[]);
    }

    /// <summary>
    /// Checks if a type is a numeric type.
    /// </summary>
    public static bool IsNumericType(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(byte) ||
               underlyingType == typeof(sbyte) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(ushort) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(uint) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(ulong) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(decimal);
    }

    /// <summary>
    /// Gets the SQL data type name for a given C# type.
    /// Useful for code generation and migration scripts.
    /// </summary>
    public static string ToSqlType(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            nameof(Int32) => "INTEGER",
            nameof(Int64) => "INTEGER",
            nameof(Int16) => "INTEGER",
            nameof(Boolean) => "BOOLEAN",
            nameof(String) => "TEXT",
            nameof(Decimal) => "REAL",
            nameof(Double) => "REAL",
            nameof(Single) => "REAL",
            nameof(DateTime) => "DATETIME",
            nameof(DateTimeOffset) => "DATETIME",
            nameof(TimeSpan) => "TEXT",
            nameof(Guid) => "TEXT",
            "Byte[]" => "BLOB",
            _ => "TEXT"
        };
    }

    /// <summary>
    /// Gets the C# default value for a given type.
    /// </summary>
    public static object? GetDefaultValue(this Type type)
    {
        if (type is null)
            return null;

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }

    /// <summary>
    /// Checks if a type is an enumerable collection (but not string).
    /// </summary>
    public static bool IsEnumerableType(this Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>)) ||
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Gets the element type of an enumerable collection.
    /// </summary>
    public static Type? GetElementType(this Type type)
    {
        if (!type.IsEnumerableType())
            return null;

        return type.GetGenericArguments().FirstOrDefault() ?? type.GetElementType();
    }

    /// <summary>
    /// Gets the underlying type for nullable types.
    /// </summary>
    public static Type GetUnderlyingType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Checks if a type is an interface.
    /// </summary>
    public static bool IsInterface(this Type type)
    {
        return type.IsInterface;
    }

    /// <summary>
    /// Checks if a type is abstract.
    /// </summary>
    public static bool IsAbstractClass(this Type type)
    {
        return type.IsAbstract && !type.IsInterface;
    }

    /// <summary>
    /// Gets the C# source code representation of a type name.
    /// </summary>
    public static string GetTypeName(this Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericArguments = type.GetGenericArguments()
            .Select(t => t.Name)
            .ToList();

        return $"{type.Name.Substring(0, type.Name.IndexOf('`'))}<{string.Join(", ", genericArguments)}>";
    }

    /// <summary>
    /// Gets all interfaces implemented by a type, including inherited ones.
    /// </summary>
    public static IEnumerable<Type> GetAllInterfaces(this Type type)
    {
        var interfaces = new HashSet<Type>(type.GetInterfaces());
        var baseType = type.BaseType;

        while (baseType is not null)
        {
            foreach (var iface in baseType.GetInterfaces())
                interfaces.Add(iface);
            baseType = baseType.BaseType;
        }

        return interfaces;
    }
}
