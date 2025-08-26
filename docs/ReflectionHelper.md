# ReflectionHelper

The `ReflectionHelper` class provides a centralized set of static utility methods for performing common reflection operations within the `dotnet-sqlite-crud-generator` project. It abstracts away the verbosity of the standard `System.Reflection` API, offering type-safe wrappers for object instantiation, property manipulation, method invocation, attribute inspection, and type hierarchy analysis. This helper is designed to streamline the dynamic generation of CRUD operations by simplifying interactions with entity types at runtime.

## API

### `CreateInstance<T>()`
Creates a new instance of the specified type `T`.
*   **Parameters**: None (constrained by generic type).
*   **Returns**: A new instance of `T`.
*   **Throws**: Throws an exception if `T` does not have a public parameterless constructor, despite the generic constraint, or if the instantiation fails at runtime.
*   **Constraints**: `T` must be a class and have a public parameterless constructor (`where T : class, new()`).

### `CreateInstance`
Creates a new instance of a specified `Type` dynamically.
*   **Parameters**: Accepts a `Type` object representing the type to instantiate.
*   **Returns**: An `object?` representing the new instance, or `null` if instantiation fails or the type is abstract.
*   **Throws**: May throw `MissingMethodException` if a suitable constructor is not found.

### `SetProperty<T>`
Sets the value of a specified property on a target object.
*   **Parameters**: Takes the target object instance, the name of the property (string), and the value to set.
*   **Returns**: `void`.
*   **Throws**: Throws if the property does not exist, is write-only, or if the provided value cannot be assigned to the property type.

### `GetProperty<T>`
Retrieves the value of a specified property from a target object.
*   **Parameters**: Takes the target object instance and the name of the property (string).
*   **Returns**: An `object?` containing the property value, or `null` if the property value is null or the property does not exist.
*   **Throws**: Throws if the property exists but is write-only or inaccessible.

### `GetPropertyValues<T>`
Retrieves all readable property values from an instance as a dictionary.
*   **Parameters**: Takes the target object instance.
*   **Returns**: A `Dictionary<string, object?>` where keys are property names and values are the corresponding property values.
*   **Throws**: Generally does not throw unless accessing a specific property getter throws an exception.

### `CopyProperties<T>`
Copies property values from a source object to a target object of the same type where property names match.
*   **Parameters**: Takes a source instance and a target instance.
*   **Returns**: `void`.
*   **Throws**: Throws if either instance is null or if a property exists on the source but cannot be written to the target due to type mismatch or access restrictions.

### `GetMethods`
Retrieves a collection of methods defined on a specific type.
*   **Parameters**: Accepts a `Type` object and optional binding flags or filters (implementation dependent).
*   **Returns**: An `IEnumerable<MethodInfo>` containing the reflected methods.
*   **Throws**: Throws if the provided type is null.

### `InvokeMethod<T>`
Invokes a specified method on a target object dynamically.
*   **Parameters**: Takes the target object instance, the method name (string), and an optional array of arguments.
*   **Returns**: An `object?` representing the return value of the method, or `null` if the method returns void or no value.
*   **Throws**: Throws `MissingMethodException` if the method is not found, or `TargetInvocationException` if the underlying method throws an exception.

### `GetAttributes<TAttribute>`
Retrieves all custom attributes of a specified type applied to a member or type.
*   **Parameters**: Takes the `MemberInfo` or `Type` to inspect.
*   **Returns**: An `IEnumerable<TAttribute>` containing the found attributes.
*   **Throws**: Throws if the generic type `TAttribute` is not a valid Attribute type.

### `InheritsFrom`
Determines if a specific type inherits from a given base type.
*   **Parameters**: Takes the candidate type (`Type`) and the base type (`Type`).
*   **Returns**: A `bool` indicating whether the inheritance relationship exists.
*   **Throws**: Throws if either input type is null.

### `GetTypesInheritingFrom`
Finds all types within the current context or assembly that inherit from a specified base type.
*   **Parameters**: Takes the base `Type` to search for.
*   **Returns**: An `IEnumerable<Type>` containing all matching derived types.
*   **Throws**: Throws if the base type is null or if assembly scanning fails.

### `ShallowCopy<T>`
Creates a shallow copy of an object.
*   **Parameters**: Takes the source object instance.
*   **Returns**: A new instance of `T` with field/property values copied from the source.
*   **Throws**: Throws if `T` cannot be instantiated or if member access fails during copying.

### `GetUnderlyingType`
Retrieves the underlying type of a nullable type or a wrapper.
*   **Parameters**: Takes a `Type` object (e.g., `typeof(int?)`).
*   **Returns**: The `Type` of the inner value (e.g., `typeof(int`), or the original type if it is not a nullable wrapper.
*   **Throws**: Throws if the input type is null.

### `HasAttribute<TAttribute>`
Checks if a specific member or type has a particular custom attribute applied.
*   **Parameters**: Takes the `MemberInfo` or `Type` to inspect.
*   **Returns**: A `bool` indicating the presence of the attribute.
*   **Throws**: Throws if the generic type `TAttribute` is invalid.

## Usage

### Example 1: Dynamic Entity Instantiation and Property Population
This example demonstrates creating an instance of a generic entity type and populating its properties from a data dictionary, a common pattern in CRUD generators.

```csharp
public class UserEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public void HydrateEntity()
{
    var data = new Dictionary<string, object?>
    {
        { "Id", 42 },
        { "Name", "John Doe" }
    };

    // Create a new instance using the generic helper
    var user = ReflectionHelper.CreateInstance<UserEntity>();

    // Populate properties dynamically
    foreach (var kvp in data)
    {
        ReflectionHelper.SetProperty(user, kvp.Key, kvp.Value);
    }

    // Verify the result
    var name = ReflectionHelper.GetProperty<string>(user, "Name");
    Console.WriteLine($"Created user: {name}");
}
```

### Example 2: Attribute Inspection and Method Invocation
This example shows how to scan a type for specific attributes (e.g., mapping attributes) and invoke a method dynamically based on reflection.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class ColumnNameAttribute : Attribute
{
    public string Name { get; }
    public ColumnNameAttribute(string name) => Name = name;
}

public class Product
{
    [ColumnName("product_title")]
    public string Title { get; set; } = "";

    public void NormalizeTitle()
    {
        Title = Title.Trim().ToLower();
    }
}

public void ProcessMetadata()
{
    var product = ReflectionHelper.CreateInstance<Product>();
    
    // Check for attributes on a specific property
    var propInfo = typeof(Product).GetProperty(nameof(Product.Title));
    if (propInfo != null && ReflectionHelper.HasAttribute<ColumnNameAttribute>(propInfo))
    {
        var attrs = ReflectionHelper.GetAttributes<ColumnNameAttribute>(propInfo);
        foreach (var attr in attrs)
        {
            Console.WriteLine($"DB Column: {attr.Name}");
        }
    }

    // Invoke a method dynamically
    ReflectionHelper.InvokeMethod(product, nameof(Product.NormalizeTitle));
}
```

## Notes

*   **Null Handling**: Methods returning `object?` may return `null` both when the underlying value is null and when the reflection operation fails to locate a member, depending on the specific implementation. Consumers should verify existence via `HasAttribute` or try-catch blocks where strict error handling is required.
*   **Performance**: Reflection operations are inherently slower than direct code execution. `GetPropertyValues` and `CopyProperties` iterate over all members; avoid calling these inside tight loops without caching results if performance is critical.
*   **Thread Safety**: As this class consists entirely of static methods that operate on passed-in instances without maintaining internal mutable state, it is thread-safe for concurrent calls provided the objects passed as arguments are themselves handled correctly by the caller.
*   **Accessibility**: These helpers typically utilize `BindingFlags` to access non-public members if necessary for the CRUD generation logic. However, they will throw exceptions if attempting to set read-only properties or invoke methods that are inaccessible in the current context.
*   **Nullable Types**: The `GetUnderlyingType` method is essential when dealing with database schemas where value types are often nullable (e.g., `int?`). Ensure this is used before attempting type conversions on generic parameters derived from database metadata.
