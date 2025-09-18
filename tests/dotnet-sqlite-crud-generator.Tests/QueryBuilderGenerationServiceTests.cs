#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Contains unit tests for <see cref="QueryBuilderGenerationService"/>.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the test class.
    /// Creates a unique temporary output directory and instantiates the service under test.
    /// </summary>
    public QueryBuilderGenerationServiceTests()
    {
        _outputPath = Path.Combine(Path.GetTempPath(), "QBGenTests", Guid.NewGuid().ToString());
        _sut = new QueryBuilderGenerationService(_outputPath);
    }

    /// <summary>
    /// Disposes the test instance and removes the temporary output directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_outputPath))
            Directory.Delete(_outputPath, true);
    }

    // ---------- BuildQueryBuilderSource ----------

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains the expected query builder class name.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsClassName()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public sealed class WidgetQueryBuilder");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains the expected table name derived from the entity type.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsTableName()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("\"Widgets\"");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains strongly‑typed <c>Where</c> methods for each property of the entity.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsStronglyTypedWhereForEachProperty()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("WhereId(");
        source.Should().Contain("WhereName(");
        source.Should().Contain("WherePrice(");
        source.Should().Contain("WhereIsActive(");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains a <c>Build</c> method returning the SQL string and parameters.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsBuildMethod()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build()");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains <c>Limit</c> and <c>Offset</c> methods.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsLimitAndOffset()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder Limit(int limit)");
        source.Should().Contain("public WidgetQueryBuilder Offset(int offset)");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains ordering methods for arbitrary columns.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsOrderByMethods()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder OrderBy(string column, bool descending = false)");
        source.Should().Contain("public WidgetQueryBuilder OrderByDescending(string column)");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated source contains a <c>Reset</c> method to clear the builder state.
    /// </summary>
    public void BuildQueryBuilderSource_ContainsResetMethod()
    {
        var source = _sut.BuildQueryBuilderSource(typeof(Widget));

        source.Should().Contain("public WidgetQueryBuilder Reset()");
    }

    [Fact]
    /// <summary>
    /// Verifies that calling <c>BuildQueryBuilderSource</c> with a <c>null</c> entity type throws <see cref="ArgumentNullException"/>.
    /// </summary>
    public void BuildQueryBuilderSource_ThrowsOnNullEntityType()
    {
        Action act = () => _sut.BuildQueryBuilderSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---------- GenerateQueryBuilderAsync ----------

    [Fact]
    /// <summary>
    /// Verifies that <c>GenerateQueryBuilderAsync</c> writes the generated file to the configured output path.
    /// </summary>
    /// <returns>A task that completes when the file has been written.</returns>
    public async Task GenerateQueryBuilderAsync_WritesFileToOutputPath()
    {
        var filePath = await _sut.GenerateQueryBuilderAsync(typeof(Widget));

        File.Exists(filePath).Should().BeTrue();
        Path.GetFileName(filePath).Should().Be("WidgetQueryBuilder.cs");
    }

    [Fact]
    /// <summary>
    /// Verifies that the generated file contains valid source code for the query builder.
    /// </summary>
    /// <returns>A task that completes when the file content has been validated.</returns>
    public async Task GenerateQueryBuilderAsync_FileContainsValidSource()
    {
        var filePath = await _sut.GenerateQueryBuilderAsync(typeof(Widget));
        var content = await File.ReadAllTextAsync(filePath);

        content.Should().Contain("public sealed class WidgetQueryBuilder");
        content.Should().Contain("public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build()");
    }

    [Fact]
    /// <summary>
    /// Verifies that calling <c>GenerateQueryBuilderAsync</c> with a <c>null</c> entity type throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A task that completes when the exception is observed.</returns>
    public async Task GenerateQueryBuilderAsync_ThrowsOnNullEntityType()
    {
        Func<Task> act = async () => await _sut.GenerateQueryBuilderAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
