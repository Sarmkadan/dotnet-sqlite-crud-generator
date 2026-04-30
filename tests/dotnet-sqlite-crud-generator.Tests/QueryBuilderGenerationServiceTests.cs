#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class QueryBuilderGenerationServiceTests : IDisposable
{
    private readonly QueryBuilderGenerationService _sut;
    private readonly string _outputPath;

    private sealed class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public QueryBuilderGenerationServiceTests()
    {
        _outputPath = Path.Combine(Path.GetTempPath(), "QBGenTests", Guid.NewGuid().ToString());
        _sut = new QueryBuilderGenerationService(_outputPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputPath))
            Directory.Delete(_outputPath, true);
    }

    // ---------- BuildQueryBuilderSource ----------

    [Fact]
    public void BuildQueryBuilderSource_ContainsClassName()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public sealed class WidgetQueryBuilder");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsTableName()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("\"Widgets\"");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsStronglyTypedWhereForEachProperty()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("WhereId(");
        source.Should().Contain("WhereName(");
        source.Should().Contain("WherePrice(");
        source.Should().Contain("WhereIsActive(");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsBuildMethod()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build()");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsLimitAndOffset()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder Limit(int limit)");
        source.Should().Contain("public WidgetQueryBuilder Offset(int offset)");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsOrderByMethods()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder OrderBy(string column, bool descending = false)");
        source.Should().Contain("public WidgetQueryBuilder OrderByDescending(string column)");
    }

    [Fact]
    public void BuildQueryBuilderSource_ContainsResetMethod()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder Reset()");
    }

    [Fact]
    public void BuildQueryBuilderSource_ThrowsOnNullEntityType()
    {
        Action act = () => _sut.BuildQueryBuilderSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---------- GenerateQueryBuilderAsync ----------

    [Fact]
    public async Task GenerateQueryBuilderAsync_WritesFileToOutputPath()
    {
        var filePath = await _sut.GenerateQueryBuilderAsync(typeof(Widget));

        File.Exists(filePath).Should().BeTrue();
        Path.GetFileName(filePath).Should().Be("WidgetQueryBuilder.cs");
    }

    [Fact]
    public async Task GenerateQueryBuilderAsync_FileContainsValidSource()
    {
        var filePath = await _sut.GenerateQueryBuilderAsync(typeof(Widget));
        var content = await File.ReadAllTextAsync(filePath);

        content.Should().Contain("public sealed class WidgetQueryBuilder");
        content.Should().Contain("public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build()");
    }

    [Fact]
    public async Task GenerateQueryBuilderAsync_ThrowsOnNullEntityType()
    {
        Func<Task> act = async () => await _sut.GenerateQueryBuilderAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
