// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Utilities;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for NamingConventionHelper and property-reflection paths
/// that fire on every schema generation and SQL parameter binding call.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class NamingConventionBenchmarks
{
    private static readonly Type ProductType = typeof(Product);
    private static readonly PropertyInfo NameProperty =
        typeof(Product).GetProperty(nameof(Product.Name))!;
    private static readonly PropertyInfo PriceProperty =
        typeof(Product).GetProperty(nameof(Product.Price))!;

    [Benchmark(Description = "GetTableName (pluralise + snake_case)")]
    public string GetTableName() =>
        NamingConventionHelper.GetTableName(ProductType);

    [Benchmark(Description = "GetColumnName — no attribute")]
    public string GetColumnNamePlain() =>
        NamingConventionHelper.GetColumnName(NameProperty);

    [Benchmark(Description = "GetColumnName — decimal property")]
    public string GetColumnNameDecimal() =>
        NamingConventionHelper.GetColumnName(PriceProperty);

    [Benchmark(Description = "GetConventionInfo — full model scan")]
    public NamingConventionInfo GetConventionInfo() =>
        NamingConventionHelper.GetConventionInfo(ProductType);

    [Benchmark(Description = "IsValidPropertyName — valid input")]
    public bool IsValidPropertyNameValid() =>
        NamingConventionHelper.IsValidPropertyName("StockQuantity");

    [Benchmark(Description = "IsValidPropertyName — invalid input")]
    public bool IsValidPropertyNameInvalid() =>
        NamingConventionHelper.IsValidPropertyName("123invalid");

    [Benchmark(Description = "GetApiEndpoint — v1 default")]
    public string GetApiEndpoint() =>
        NamingConventionHelper.GetApiEndpoint(ProductType);

    [Benchmark(Description = "ToCSharpToSqlConvention — round trip")]
    public string CSharpToSql() =>
        NamingConventionHelper.ToCSharpToSqlConvention("StockQuantity");
}
