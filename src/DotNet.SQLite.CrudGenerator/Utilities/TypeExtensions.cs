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
/// <remarks>
/// All extension methods validate their <see langword="this"/> parameter and throw appropriate exceptions for null values.
/// </remarks>
public static class TypeExtensions
{
    /// <summary>
    /// Gets all public properties of a type, optionally filtered by attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute type to filter by.</typeparam>
    /// <param name="type">The type to inspect.</param>
    /// <returns>An enumerable of <see cref="PropertyInfo"/> instances that have the specified attribute.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static IEnumerable<PropertyInfo> GetProperties<TAttribute>(this Type type) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<TAttribute>() is not null);
    }

    /// <summary>
    /// Checks if a type is a simple/primitive type (int, string, bool, etc.).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a simple type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool IsSimpleType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive
            || underlyingType == typeof(string)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(DateTime)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(TimeSpan)
            || underlyingType == typeof(Guid)
            || underlyingType == typeof(byte[]);
    }

    /// <summary>
    /// Checks if a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is a numeric type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool IsNumericType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(byte)
            || underlyingType == typeof(sbyte)
            || underlyingType == typeof(short)
            || underlyingType == typeof(ushort)
            || underlyingType == typeof(int)
            || underlyingType == typeof(uint)
            || underlyingType == typeof(long)
            || underlyingType == typeof(ulong)
            || underlyingType == typeof(float)
            || underlyingType == typeof(double)
            || underlyingType == typeof(decimal);
    }

    /// <summary>
    /// Gets the SQL data type name for a given C# type.
    /// Useful for code generation and migration scripts.
    /// </summary>
    /// <param name="type">The type to convert to SQL type.</param>
    /// <returns>The SQL type name as a string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToSqlType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

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
    /// <param name="type">The type to get the default value for.</param>
    /// <returns>The default value for the type, or <see langword="null"/> for reference types.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static object? GetDefaultValue(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
    }

    /// <summary>
    /// Checks if a type is an enumerable collection (but not string).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an enumerable collection; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool IsEnumerableType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type == typeof(string))
            return false;

        return type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition())
            || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    /// <summary>
    /// Gets the element type of an enumerable collection.
    /// </summary>
    /// <param name="type">The enumerable type.</param>
    /// <returns>The element type, or <see langword="null"/> if the type is not an enumerable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static Type? GetElementType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsEnumerableType())
            return null;

        return type.GetGenericArguments().FirstOrDefault()
            ?? type.GetElementType();
    }

    /// <summary>
    /// Gets the underlying type for nullable types.
    /// </summary>
    /// <param name="type">The type to get the underlying type for.</param>
    /// <returns>The underlying type if nullable; otherwise, the original type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static Type GetUnderlyingType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Checks if a type is an interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an interface; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool IsInterface(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsInterface;
    }

    /// <summary>
    /// Checks if a type is abstract.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the type is an abstract class; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static bool IsAbstractClass(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsAbstract && !type.IsInterface;
    }

    /// <summary>
    /// Gets the C# source code representation of a type name.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>The type name as it would appear in C# source code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="type"/> is a generic type but does not have the expected format.</exception>
    public static string GetTypeName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsGenericType)
            return type.Name;

        var backtickIndex = type.Name.IndexOf('`');
        if (backtickIndex < 0)
            return type.Name;

        var genericArguments = type.GetGenericArguments()
            .Select(t => t.Name)
            .ToList();

        return $"{type.Name.Substring(0, backtickIndex)}<{string.Join(", ", genericArguments)}>";
    }

    /// <summary>
    /// Gets all interfaces implemented by a type, including inherited ones.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>An enumerable of all interfaces implemented by the type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static IEnumerable<Type> GetAllInterfaces(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

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