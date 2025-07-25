# StringExtensionsBenchmarks

The `StringExtensionsBenchmarks` class serves as a dedicated harness for performance testing and validation of string manipulation extension methods within the `dotnet-sqlite-crud-generator` project. It encapsulates a suite of standardized transformation operations—ranging from case conversion and formatting to linguistic pluralization and serialization round-trips—allowing developers to benchmark execution time and memory allocation across different input scenarios. This type ensures that string utility functions used throughout the code generation pipeline maintain optimal efficiency and consistent behavior under load.

## API

The following public members are exposed by the `StringExtensionsBenchmarks` class for execution and measurement:

### `ToPascalCase`
*   **Purpose**: Transforms the input string into PascalCase format, where the first letter of each word is capitalized and no separators are retained.
*   **Parameters**: Implicitly operates on the target string instance provided during benchmark initialization.
*   **Return Value**: Returns a `string` representing the transformed PascalCase value.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `ToCamelCase`
*   **Purpose**: Converts the input string into camelCase format, ensuring the first letter of the initial word is lowercase while subsequent words start with an uppercase letter.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` representing the transformed camelCase value.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `ToSnakeCase`
*   **Purpose**: Reformats the input string into snake_case, converting all characters to lowercase and inserting underscores between words.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` representing the transformed snake_case value.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `ToKebabCase`
*   **Purpose**: Reformats the input string into kebab-case, converting all characters to lowercase and inserting hyphens between words.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` representing the transformed kebab-case value.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `RemoveWhitespace`
*   **Purpose**: Eliminates all whitespace characters (spaces, tabs, newlines) from the input string.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` containing only non-whitespace characters.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `ToSlug`
*   **Purpose**: Generates a URL-friendly slug from the input string by lowercasing, removing special characters, and replacing spaces with hyphens.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` suitable for use in URLs.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `Repeat`
*   **Purpose**: Constructs a new string by repeating the input sequence a specified number of times.
*   **Parameters**: Implicitly operates on the target string instance; typically accepts a repetition count configuration within the benchmark context.
*   **Return Value**: Returns a `string` containing the concatenated repetitions.
*   **Exceptions**: May throw `ArgumentOutOfRangeException` if the repetition count is negative.

### `Pluralize`
*   **Purpose**: Applies English pluralization rules to the input noun string.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` representing the plural form of the input.
*   **Exceptions**: May throw `ArgumentNullException` if the input string is null.

### `RoundTrip`
*   **Purpose**: Validates data integrity by serializing and deserializing the string (or performing a reversible transformation) to ensure the original value is preserved.
*   **Parameters**: Implicitly operates on the target string instance.
*   **Return Value**: Returns a `string` identical to the original input if the operation is successful.
*   **Exceptions**: May throw exceptions related to serialization failures or invalid format if the transformation logic encounters unparsable data.

## Usage

The following examples demonstrate how `StringExtensionsBenchmarks` might be utilized within a BenchmarkDotNet context to evaluate performance of specific string transformations.

**Example 1: Benchmarking Case Conversions**
This example illustrates setting up a benchmark to compare the performance of converting database column names to C# property naming conventions.

```csharp
using BenchmarkDotNet.Attributes;
using DotNetSqliteCrudGenerator.Benchmarks;

[MemoryDiagnoser]
public class NamingConventionBenchmarks
{
    private readonly StringExtensionsBenchmarks _benchmarks;

    public NamingConventionBenchmarks()
    {
        // Initialize with a representative sample string
        _benchmarks = new StringExtensionsBenchmarks("user_account_status");
    }

    [Benchmark]
    public string ConvertToPascalCase()
    {
        return _benchmarks.ToPascalCase;
    }

    [Benchmark]
    public string ConvertToCamelCase()
    {
        return _benchmarks.ToCamelCase;
    }
}
```

**Example 2: Benchmarking Slug Generation and Cleaning**
This example evaluates the cost of generating URL slugs and removing whitespace from user-generated content.

```csharp
using BenchmarkDotNet.Attributes;
using DotNetSqliteCrudGenerator.Benchmarks;

[MemoryDiagnoser]
public class ContentProcessingBenchmarks
{
    private readonly StringExtensionsBenchmarks _benchmarks;

    public ContentProcessingBenchmarks()
    {
        // Initialize with a complex string containing spaces and special chars
        _benchmarks = new StringExtensionsBenchmarks("  Hello World! This is a Test.  ");
    }

    [Benchmark]
    public string GenerateSlug()
    {
        return _benchmarks.ToSlug;
    }

    [Benchmark]
    public string StripWhitespace()
    {
        return _benchmarks.RemoveWhitespace;
    }
}
```

## Notes

*   **Null Handling**: As these members operate on string instances, passing a `null` reference to the underlying extension methods will typically result in an `ArgumentNullException`. Implementations should ensure input validation prior to invoking these benchmark targets if null inputs are possible in the test dataset.
*   **Thread Safety**: The `StringExtensionsBenchmarks` class itself is stateless regarding the transformation logic, as strings in .NET are immutable. However, if the class maintains internal mutable state for configuration (e.g., repetition counts), concurrent access to the same instance from multiple threads without external synchronization is not guaranteed to be safe. It is recommended to instantiate a new instance per benchmark iteration or thread.
*   **Edge Cases**:
    *   **Empty Strings**: Operations like `ToPascalCase`, `ToSnakeCase`, and `ToSlug` should gracefully handle empty strings, typically returning an empty string rather than throwing.
    *   **Already Formatted Input**: Passing a string that is already in the target format (e.g., passing a PascalCase string to `ToPascalCase`) should result in minimal allocation and processing overhead.
    *   **Unicode Characters**: Transformations relying on case conversion (e.g., `ToLower`, `ToUpper`) are culture-sensitive. Behaviors may vary depending on the current thread culture unless invariant culture is explicitly enforced within the extension implementation.
    *   **Pluralization Limits**: The `Pluralize` method likely relies on heuristic rules or a dictionary; irregular nouns or non-English inputs may not yield linguistically correct results.
