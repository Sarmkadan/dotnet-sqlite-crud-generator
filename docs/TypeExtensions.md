# TypeExtensions
A collection of static extension methods for `System.Type` that simplify common type inspections and conversions used by the SQLite CRUD generator, such as detecting simple or numeric types, retrieving default values, mapping to SQL types, and enumerating properties with specific attributes.

## API
### GetProperties<TAttribute>
**Purpose**  
Returns all properties of the declaring type that are decorated with the attribute type `TAttribute`.

**Parameters**  
- `TAttribute` – The attribute type to filter properties by. Must derive from `System.Attribute`.

**Return Value**  
`IEnumerable<PropertyInfo>` containing the matching properties; empty enumeration if none match.

**Exceptions**  
- `ArgumentException` – If `TAttribute` is not an attribute type.

### IsSimpleType
**Purpose**  
Determines whether the type is a simple (primitive) type suitable for direct column mapping (e.g., numeric types, `bool`, `char`, `string`, `DateTime`, `DateTimeOffset`, `decimal`, `Guid`).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`true` if the type is simple; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### IsNumericType
**Purpose**  
Determines whether the type represents a numeric value (integral, floating‑point, or `decimal`).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`true` for numeric types; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### ToSqlType
**Purpose**  
Maps a CLR type to its corresponding SQLite type name (e.g., `System.Int32` → `"INTEGER"`).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
A string containing the SQLite type affinity.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.  
- `NotSupportedException` – If the type has no known SQLite mapping.

### GetDefaultValue
**Purpose**  
Retrieves the default value for the type (`default(T)` for value types, `null` for reference types).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
An `object?` holding the default value, or `null` for reference types.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### IsEnumerableType
**Purpose**  
Checks whether the type implements `System.Collections.IEnumerable` (excluding `string` which is treated as a scalar).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`true` if the type is enumerable; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### GetElementType
**Purpose**  
Returns the element type of an array, `IEnumerable<T>`, or `IReadOnlyCollection<T>`; returns `null` for non‑enumerable types.

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
The element `Type` if detectable; otherwise `null`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### GetUnderlyingType
**Purpose**  
Returns the underlying type of a `Nullable<T>`; if the type is not nullable, returns the type itself.

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
The underlying `Type` for nullable types, or the original type.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### IsInterface
**Purpose**  
Indicates whether the type represents an interface.

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`true` for interfaces; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### IsAbstractClass
**Purpose**  
Determines whether the type is a non‑sealed abstract class.

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`true` if the type is an abstract class; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### GetTypeName
**Purpose**  
Provides a user‑friendly name for the type (e.g., removes generic arity suffixes).

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
A string representation of the type name.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

### GetAllInterfaces
**Purpose**  
Enumerates all interfaces implemented by the type, including those inherited from base types.

**Parameters**  
- None (extension method operates on the target `Type` instance).

**Return Value**  
`IEnumerable<Type>` of interfaces; empty if none.

**Exceptions**  
- `ArgumentNullException` – If the type instance is `null`.

## Usage
```csharp
using System;
using System.Linq;
using System.Reflection;

// Example 1: Retrieve all properties marked with a custom attribute
[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute { }

public class Person
{
    [Column] public int Id { get; set; }
    public string Name { get; set; }
    [Column] public DateTime BirthDate { get; set; }
}

var columnProps = typeofPerson.GetProperties<ColumnAttribute>();
foreach (var prop in columnProps)
{
    Console.WriteLine($"{prop.Name} is mapped as a column");
}
// Output:
// Id is mapped as a column
// BirthDate is mapped as a column

// Example 2: Determine if a type is numeric and get its SQLite type
Type t = typeof(double);
if (t.IsNumericType())
{
    string sqlType = t.ToSqlType();   // Returns "REAL"
    Console.WriteLine($"{t.Name} maps to SQLite type {sqlType}");
}
else
{
    Console.WriteLine($"{t.Name} is not numeric");
}
// Output:
// Double maps to SQLite type REAL
```

## Notes
- All methods are pure extensions on `System.Type` and rely only on reflection; they do not modify state and are therefore thread‑safe for concurrent calls.
- `GetProperties<TAttribute>` returns properties defined on the type itself; inherited properties are **not** included unless the attribute is inherited and the property is overridden.
- `IsEnumerableType` deliberately excludes `string`; strings are treated as scalar values for SQLite mapping purposes.
- `GetElementType` returns `null` for types that do not expose a generic element type (e.g., non‑generic `IEnumerable` implementations). Callers should check for `null` before using the result.
- `GetUnderlyingType` will return the same type for non‑nullable value types and reference types, which simplifies handling of `Nullable<T>` in generic code.
- Throwing `ArgumentNullException` protects against accidental null `Type` instances; callers should ensure the source type is not null before invoking any extension method.
