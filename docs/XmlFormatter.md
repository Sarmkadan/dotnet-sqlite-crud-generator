# XmlFormatter

Provides utilities for converting .NET objects to XML strings and parsing XML back into strongly‑typed instances. The API is generic, allowing any type that can be represented as XML to be formatted or parsed without requiring concrete serializer implementations.

## API

### Format<T>(T value)
Serializes the supplied instance to an XML string using default formatting settings.  
- **Parameters**  
  - `value`: The object to serialize. Must not be null; otherwise an `ArgumentNullException` is thrown.  
- **Return value**  
  - A string containing the XML representation of `value`.  
- **Exceptions**  
  - `ArgumentNullException` if `value` is null.  
  - `InvalidOperationException` if the type `T` cannot be serialized under the default settings.

### Format<T>(T value, object formattingOptions)
Serializes the supplied instance to an XML string, allowing customization of the output via the `formattingOptions` parameter. The exact type of `formattingOptions` is implementation‑specific (e.g., indentation, encoding).  
- **Parameters**  
  - `value`: The object to serialize. Must not be null; otherwise an `ArgumentNullException` is thrown.  
  - `formattingOptions`: An object that influences how the XML is generated (e.g., indentation, newline handling). May be null to use defaults.  
- **Return value**  
  - A string containing the XML representation of `value` formatted according to `formattingOptions`.  
- **Exceptions**  
  - `ArgumentNullException` if `value` is null.  
  - `InvalidOperationException` if serialization fails with the supplied options.

### FormatAsync<T>(T value)
Asynchronously serializes the supplied instance to an XML string using default formatting settings.  
- **Parameters**  
  - `value`: The object to serialize. Must not be null; otherwise an `ArgumentNullException` is thrown.  
- **Return value**  
  - A `Task<string>` that completes with the XML representation of `value`.  
- **Exceptions**  
  - `ArgumentNullException` if `value` is null.  
  - `InvalidOperationException` if the type `T` cannot be serialized under the default settings.

### FormatAsync<T>(T value, object formattingOptions)
Asynchronously serializes the supplied instance to an XML string, allowing customization via `formattingOptions`.  
- **Parameters**  
  - `value`: The object to serialize. Must not be null; otherwise an `ArgumentNullException` is thrown.  
  - `formattingOptions`: An object that influences how the XML is generated (e.g., indentation, encoding). May be null to use defaults.  
- **Return value**  
  - A `Task<string>` that completes with the XML representation of `value` formatted according to `formattingOptions`.  
- **Exceptions**  
  - `ArgumentNullException` if `value` is null.  
  - `InvalidOperationException` if serialization fails with the supplied options.

### Parse<T>(string xml)
Deserializes an XML string into an instance of type `T`.  
- **Parameters**  
  - `xml`: The XML input. If null or empty, the method returns null.  
- **Return value**  
  - An object of type `T` populated from the XML, or null if the input is null/empty or deserialization fails.  
- **Exceptions**  
  - `ArgumentException` if `xml` is not well‑formed XML.  
  - `InvalidOperationException` if the XML cannot be mapped to type `T`.

### ParseCollection<T>(string xml)
Deserializes an XML string that represents a collection of `T` objects into an `IEnumerable<T>`.  
- **Parameters**  
  - `xml`: The XML input containing a collection. If null or empty, the method returns null.  
- **Return value**  
  - An `IEnumerable<T>` containing the deserialized items, or null if the input is null/empty or deserialization fails.  
- **Exceptions**  
  - `ArgumentException` if `xml` is not well‑formed XML.  
  - `InvalidOperationException` if the XML does not represent a collection compatible with `T`.

### GetXmlValue<T>(string xml, string xpath)
Extracts a single value of type `T` from the supplied XML using an XPath expression.  
- **Parameters**  
  - `xml`: The XML source. If null or empty, returns null.  
  - `xpath`: An XPath expression that selects the node or attribute whose text content is to be converted to `T`.  
- **Return value**  
  - The value converted to type `T`, or null if the node is not found, the input is null/empty, or conversion fails.  
- **Exceptions**  
  - `ArgumentException` if `xml` is not well‑formed XML or `xpath` is invalid.  
  - `FormatException` if the selected node’s text cannot be converted to `T`.

### AddXmlAttribute<T>(XmlElement element, string name, T value)
Adds an attribute with the specified name and value to the supplied XML element.  
- **Parameters**  
  - `element`: The XML element to modify. Must not be null; otherwise an `ArgumentNullException` is thrown.  
  - `name`: The name of the attribute to add. Must not be null or empty; otherwise an `ArgumentException` is thrown.  
  - `value`: The value to assign to the attribute. If the value is null, the attribute is added with an empty string value.  
- **Return value**  
  - None (void).  
- **Exceptions**  
  - `ArgumentNullException` if `element` is null.  
  - `ArgumentException` if `name` is null or empty.  
  - `InvalidOperationException` if the underlying XML implementation rejects the attribute addition.

## Usage

```csharp
var formatter = new XmlFormatter();

// Synchronous serialization and deserialization
var person = new Person { Id = 1, Name = "Ada Lovelace" };
string xml = formatter.Format(person);
Person? parsed = formatter.Parse<Person>(xml);
Console.WriteLine(parsed?.Name); // Ada Lovelace
```

```csharp
// Asynchronous formatting of a collection and value extraction
var people = new List<Person>
{
    new Person { Id = 1, Name = "Ada" },
    new Person { Id = 2, Name = "Alan" }
};

string collectionXml = await formatter.FormatAsync(people);
IEnumerable<Person>? parsedPeople = await formatter.ParseCollectionAsync(collectionXml);
// Note: ParseCollectionAsync is not shown in the public surface; this example assumes an async variant exists.
// For the synchronous API:
IEnumerable<Person>? syncPeople = formatter.ParseCollection(collectionXml);

string idXml = formatter.GetXmlValue<string>(collectionXml, "//Person[1]/Id");
Console.WriteLine(idXml); // 1
```

## Notes

- All methods that accept an input string treat null or empty strings as a signal to return null (or default) rather than throw, except where the string is required for well‑formedness checks.  
- The class does not maintain any internal state; therefore instances are safe to use concurrently from multiple threads.  
- When using the overloads that accept `formattingOptions`, passing null results in the same behavior as the default‑options overload.  
- XPath expressions used with `GetXmlValue<T>` are evaluated against the root of the supplied XML document; namespaces must be declared in the XML itself if required.  
- If a type `T` contains properties or fields that cannot be represented as XML (e.g., circular references, unsupported types), the formatting methods will throw an `InvalidOperationException`.  
- Parsing methods return null on failure rather than throwing, allowing callers to handle missing data gracefully; however, malformed XML still results in an `ArgumentException`.
