# NamingConventionBenchmarks

The `NamingConventionBenchmarks` class serves as a utility component within the `dotnet-sqlite-crud-generator` project, designed to validate and measure the performance of naming convention transformations between C# domain models and SQLite database schemas. It provides a standardized set of methods to convert property names to table and column identifiers, verify naming validity, and generate API endpoint routes, ensuring consistency across the code generation pipeline while offering diagnostic capabilities for convention compliance.

## API

### `GetTableName`
Converts a given C# class name or identifier into the corresponding SQLite table name based on the configured naming convention (e.g., converting PascalCase to snake_case).
*   **Parameters**: Takes a single `string` representing the source identifier.
*   **Return Value**: Returns a `string` containing the formatted table name.
*   **Exceptions**: May throw an `ArgumentNullException` if the input is null; may throw an `ArgumentException` if the input contains invalid characters for a table identifier.

### `GetColumnNamePlain`
Transforms a C# property name into a standard SQLite column name using the default plain text convention rules.
*   **Parameters**: Takes a single `string` representing the property name.
*   **Return Value**: Returns a `string` with the converted column name.
*   **Exceptions**: Throws `ArgumentNullException` if the input is null.

### `GetColumnNameDecimal`
Transforms a C# property name into a SQLite column name specifically optimized or formatted for decimal precision contexts, potentially applying suffixes or specific casing rules required for numeric handling.
*   **Parameters**: Takes a single `string` representing the property name.
*   **Return Value**: Returns a `string` with the formatted column name.
*   **Exceptions**: Throws `ArgumentNullException` if the input is null.

### `GetConventionInfo`
Retrieves metadata regarding the currently active naming convention configuration.
*   **Parameters**: None.
*   **Return Value**: Returns a `NamingConventionInfo` object containing details such as the convention type, casing rules, and prefix/suffix settings.
*   **Exceptions**: Does not typically throw exceptions unless the internal configuration state is corrupted.

### `IsValidPropertyNameValid`
Validates whether a specific string adheres to the rules for a valid C# property name under the current convention constraints, returning true for compliant inputs.
*   **Parameters**: Takes a single `string` to validate.
*   **Return Value**: Returns a `bool` indicating validity (`true` if valid).
*   **Exceptions**: Does not throw; returns `false` for invalid inputs.

### `IsValidPropertyNameInvalid`
Performs a validation check specifically designed to confirm if a string violates naming conventions, often used in negative test scenarios or sanitization loops.
*   **Parameters**: Takes a single `string` to validate.
*   **Return Value**: Returns a `bool` indicating if the name is explicitly invalid (`true` if invalid).
*   **Exceptions**: Does not throw; returns `false` if the name is actually valid.

### `GetApiEndpoint`
Generates a RESTful API endpoint path string based on the entity name and the configured naming convention.
*   **Parameters**: Takes a single `string` representing the entity or controller name.
*   **Return Value**: Returns a `string` representing the relative URL path (e.g., `/api/users`).
*   **Exceptions**: Throws `ArgumentNullException` if the input is null; may throw `ArgumentException` if the resulting path contains illegal URL characters.

### `CSharpToSql`
Performs a direct mapping transformation from a C# type or identifier string to its equivalent SQLite SQL representation.
*   **Parameters**: Takes a single `string` representing the C# identifier.
*   **Return Value**: Returns a `string` containing the SQL-compatible identifier.
*   **Exceptions**: Throws `ArgumentNullException` if the input is null; may throw `FormatException` if the C# identifier cannot be mapped to a valid SQL token.

## Usage

The following example demonstrates how to utilize the benchmarking utility to convert a C# model name into database identifiers and verify their validity before code generation.

```csharp
using DotNetSqliteCrudGenerator.Benchmarks;

public class GenerationService
{
    private readonly NamingConventionBenchmarks _benchmarks;

    public GenerationService()
    {
        _benchmarks = new NamingConventionBenchmarks();
    }

    public void ProcessEntity(string modelName)
    {
        // Convert model name to table and column formats
        string tableName = _benchmarks.GetTableName(modelName);
        string columnName = _benchmarks.GetColumnNamePlain("TotalAmount");
        string decimalColumn = _benchmarks.GetColumnNameDecimal("Price");

        // Validate property names before generating properties
        if (_benchmarks.IsValidPropertyNameValid("UserId"))
        {
            Console.WriteLine($"Generating table: {tableName}");
        }
        
        // Generate API route
        string endpoint = _benchmarks.GetApiEndpoint(modelName);
        Console.WriteLine($"Endpoint created: {endpoint}");
    }
}
```

This example illustrates retrieving convention metadata and performing a direct C# to SQL transformation for dynamic query construction.

```csharp
using DotNetSqliteCrudGenerator.Benchmarks;
using DotNetSqliteCrudGenerator.Info;

public class SchemaValidator
{
    public void ValidateMapping()
    {
        var benchmarks = new NamingConventionBenchmarks();
        
        // Inspect current convention settings
        NamingConventionInfo info = benchmarks.GetConventionInfo();
        Console.WriteLine($"Active Convention: {info.ConventionType}");

        // Transform a complex property name to SQL
        string cSharpProp = "OrderItemCount";
        string sqlColumn = benchmarks.CSharpToSql(cSharpProp);

        // Check for invalid patterns
        bool isInvalid = benchmarks.IsValidPropertyNameInvalid("123InvalidStart");
        if (isInvalid)
        {
            Console.WriteLine($"Detected invalid property pattern in: {cSharpProp}");
        }
    }
}
```

## Notes

*   **Thread Safety**: The methods exposed in `NamingConventionBenchmarks` appear to be stateless transformations based on the provided signatures. However, if the internal `NamingConventionInfo` relies on mutable static state or shared configuration instances, concurrent calls to `GetConventionInfo` or transformation methods should be synchronized externally. Without explicit immutable guarantees in the signature, treat the instance as not thread-safe for write operations if configuration methods exist outside this benchmark view.
*   **Edge Cases**: 
    *   Inputs containing whitespace, special SQL characters (e.g., `-`, `@`), or reserved keywords may cause `GetTableName` or `CSharpToSql` to throw `ArgumentException` or `FormatException`.
    *   Empty strings passed to validation methods (`IsValidPropertyNameValid`, `IsValidPropertyNameInvalid`) will likely return `false` rather than throwing, but passing `null` to transformation methods will result in `ArgumentNullException`.
    *   The distinction between `GetColumnNamePlain` and `GetColumnNameDecimal` suggests specific handling for numeric types; passing non-numeric semantic names to the latter may result in unexpected suffixes or formatting depending on the internal convention logic.
