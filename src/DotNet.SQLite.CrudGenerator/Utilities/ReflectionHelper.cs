// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Helper class for advanced reflection operations.
/// Provides utilities for dynamic object creation, property access, and type analysis.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Creates an instance of a type using its parameterless constructor.
    /// </summary>
    public static T CreateInstance<T>() where T : class, new()
    {
        return new T();
    }

    /// <summary>
    /// Creates an instance of a type dynamically.
    /// </summary>
    public static object? CreateInstance(Type type, params object[] args)
    {
        try
        {
            if (args.Length == 0)
                return Activator.CreateInstance(type);

            return Activator.CreateInstance(type, args);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create instance of type: {type.Name}", ex);
        }
    }

    /// <summary>
    /// Sets a property value on an object using reflection.
    /// </summary>
    public static void SetProperty<T>(T obj, string propertyName, object? value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property == null || !property.CanWrite)
            throw new InvalidOperationException($"Property '{propertyName}' not found or is read-only");

        property.SetValue(obj, value);
    }

    /// <summary>
    /// Gets a property value from an object using reflection.
    /// </summary>
    public static object? GetProperty<T>(T obj, string propertyName) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase);
        if (property == null || !property.CanRead)
            throw new InvalidOperationException($"Property '{propertyName}' not found or is write-only");

        return property.GetValue(obj);
    }

    /// <summary>
    /// Gets all properties of an object with their current values.
    /// </summary>
    public static Dictionary<string, object?> GetPropertyValues<T>(T obj) where T : class
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead)
                dictionary[property.Name] = property.GetValue(obj);
        }

        return dictionary;
    }

    /// <summary>
    /// Copies all properties from one object to another of the same type.
    /// </summary>
    public static void CopyProperties<T>(T source, T destination) where T : class
    {
        if (source == null || destination == null)
            throw new ArgumentNullException(source == null ? nameof(source) : nameof(destination));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(source);
                property.SetValue(destination, value);
            }
        }
    }

    /// <summary>
    /// Gets all methods of a type that match a specific name.
    /// </summary>
    public static IEnumerable<MethodInfo> GetMethods(Type type, string methodName)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.IgnoreCase)
            .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Invokes a method on an object with the given parameters.
    /// </summary>
    public static object? InvokeMethod<T>(T obj, string methodName, params object[] args) where T : class
    {
        var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.IgnoreCase);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found");

        return method.Invoke(obj, args);
    }

    /// <summary>
    /// Gets all attributes of a specific type applied to a member.
    /// </summary>
    public static IEnumerable<TAttribute> GetAttributes<TAttribute>(MemberInfo member) where TAttribute : Attribute
    {
        return member.GetCustomAttributes<TAttribute>();
    }

    /// <summary>
    /// Checks if a type inherits from or implements a specific base type/interface.
    /// </summary>
    public static bool InheritsFrom(Type type, Type baseType)
    {
        if (type == baseType)
            return true;

        return baseType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets all types in an assembly that inherit from a specific base type.
    /// </summary>
    public static IEnumerable<Type> GetTypesInheritingFrom(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t) && t != baseType);
    }

    /// <summary>
    /// Creates a shallow copy of an object.
    /// </summary>
    public static T ShallowCopy<T>(T obj) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var copy = (T)Activator.CreateInstance(typeof(T))!;
        CopyProperties(obj, copy);
        return copy;
    }

    /// <summary>
    /// Gets the underlying type of a nullable type, or the type itself if not nullable.
    /// </summary>
    public static Type GetUnderlyingType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Checks if a property has a specific attribute.
    /// </summary>
    public static bool HasAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute
    {
        return property.GetCustomAttribute<TAttribute>() != null;
    }
}
