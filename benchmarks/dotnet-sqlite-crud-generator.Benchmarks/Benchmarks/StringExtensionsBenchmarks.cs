// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Utilities;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for string manipulation hot paths used in naming convention
/// resolution and SQL schema generation.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringExtensionsBenchmarks
{
    private const string SnakeCaseInput = "user_profile_settings";
    private const string PascalCaseInput = "UserProfileSettings";
    private const string MixedWhitespaceInput = "  hello  world  foo  bar  ";
    private const string SlugInput = "My Product Name & Special Chars 2024";
    private const string RepeatInput = "item-";

    [Benchmark(Description = "ToPascalCase (snake → Pascal)")]
    public string ToPascalCase() => SnakeCaseInput.ToPascalCase();

    [Benchmark(Description = "ToCamelCase (snake → camel)")]
    public string ToCamelCase() => SnakeCaseInput.ToCamelCase();

    [Benchmark(Description = "ToSnakeCase (Pascal → snake)")]
    public string ToSnakeCase() => PascalCaseInput.ToSnakeCase();

    [Benchmark(Description = "ToKebabCase (Pascal → kebab)")]
    public string ToKebabCase() => PascalCaseInput.ToKebabCase();

    [Benchmark(Description = "RemoveWhitespace (LINQ-free span)")]
    public string RemoveWhitespace() => MixedWhitespaceInput.RemoveWhitespace();

    [Benchmark(Description = "ToSlug (compiled regex)")]
    public string ToSlug() => SlugInput.ToSlug();

    [Benchmark(Description = "Repeat ×8 (string.Create)")]
    public string Repeat() => RepeatInput.Repeat(8);

    [Benchmark(Description = "Pluralize (suffix check)")]
    public string Pluralize() => "category".Pluralize();

    [Benchmark(Description = "RoundTrip: snake → Pascal → snake")]
    public string RoundTrip() => SnakeCaseInput.ToPascalCase().ToSnakeCase();
}
