// existing content ...

## StringExtensionsBenchmarks

The `StringExtensionsBenchmarks` class provides a set of benchmarking methods to evaluate the performance of string extension methods. It allows you to measure the execution time of various string operations, such as converting between different case formats, removing whitespace, and repeating strings.

Example usage:
```csharp
public class StringExtensionsBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var benchmarks = new StringExtensionsBenchmarks();
        var input = "Hello World";
        var pascalCase = await benchmarks.ToPascalCaseAsync(input);
        var camelCase = await benchmarks.ToCamelCaseAsync(input);
        var snakeCase = await benchmarks.ToSnakeCaseAsync(input);
        var kebabCase = await benchmarks.ToKebabCaseAsync(input);
        var whitespaceRemoved = await benchmarks.RemoveWhitespaceAsync(input);
        var slug = await benchmarks.ToSlugAsync(input);
        var repeated = await benchmarks.RepeatAsync(input, 3);
        var pluralized = await benchmarks.PluralizeAsync(input);
        var roundTripped = await benchmarks.RoundTripAsync(input);
        Console.WriteLine(pascalCase);
        Console.WriteLine(camelCase);
        Console.WriteLine(snakeCase);
        Console.WriteLine(kebabCase);
        Console.WriteLine(whitespaceRemoved);
        Console.WriteLine(slug);
        Console.WriteLine(repeated);
        Console.WriteLine(pluralized);
        Console.WriteLine(roundTripped);
    }
}
```

