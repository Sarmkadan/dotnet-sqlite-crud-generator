#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class MigrationDiffServiceTests : IDisposable
{
    private readonly DatabaseConnection _db;
    private readonly MigrationDiffService _sut;

    public MigrationDiffServiceTests()
    {
        _db = new DatabaseConnection("Data Source=:memory:");
        _sut = new MigrationDiffService(_db);
    }

    public void Dispose() => _db.Dispose();

    // ---------- helper entity classes ----------

    private sealed class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private sealed class ExtendedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    // ---------- GetExpectedSchema ----------

    [Fact]
    public void GetExpectedSchema_ReturnsCorrectColumnTypes()
    {
        var schema = _sut.GetExpectedSchema(typeof(SimpleEntity));

        schema.Should().ContainKey("Id");
        schema["Id"].SqliteType.Should().Be("INTEGER");
        schema["Id"].IsPrimaryKey.Should().BeTrue();

        schema.Should().ContainKey("Name");
        schema["Name"].SqliteType.Should().Be("TEXT");

        schema.Should().ContainKey("Price");
        schema["Price"].SqliteType.Should().Be("REAL");
    }

    [Fact]
    public void GetExpectedSchema_NonNullableValueTypeIsMarkedNotNull()
    {
        var schema = _sut.GetExpectedSchema(typeof(SimpleEntity));

        schema["Id"].NotNull.Should().BeTrue();
        schema["Price"].NotNull.Should().BeTrue();
    }

    // ---------- ComputeDiffAsync — table does not exist ----------

    [Fact]
    public async Task ComputeDiffAsync_AllColumnsMarkedAdded_WhenTableDoesNotExist()
    {
        // SimpleEntitys table was never created
        var diff = await _sut.ComputeDiffAsync(typeof(SimpleEntity));

        diff.IsUpToDate.Should().BeFalse();
        diff.TableDiff.ColumnDiffs.Should().OnlyContain(d => d.Kind == ColumnDiffKind.Added);
        diff.TableDiff.ColumnDiffs.Select(d => d.ColumnName)
            .Should().Contain(["Id", "Name", "Price"]);
    }

    [Fact]
    public async Task ComputeDiffAsync_AlterScriptContainsAddColumn_WhenTableDoesNotExist()
    {
        var diff = await _sut.ComputeDiffAsync(typeof(SimpleEntity));

        diff.AlterScript.Should().Contain("ALTER TABLE");
        diff.AlterScript.Should().Contain("ADD COLUMN");
    }

    // ---------- ComputeDiffAsync — schema matches ----------

    [Fact]
    public async Task ComputeDiffAsync_IsUpToDate_WhenSchemaMatchesModel()
    {
        await _db.OpenAsync();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE SimpleEntitys (Id INTEGER, Name TEXT, Price REAL)";
        await cmd.ExecuteNonQueryAsync();

        var diff = await _sut.ComputeDiffAsync(typeof(SimpleEntity));

        diff.IsUpToDate.Should().BeTrue();
        diff.AlterScript.Should().Contain("up to date");
    }

    // ---------- ComputeDiffAsync — column added ----------

    [Fact]
    public async Task ComputeDiffAsync_DetectsAddedColumn()
    {
        // Create table without the Description column
        await _db.OpenAsync();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE ExtendedEntitys (Id INTEGER, Name TEXT, Price REAL)";
        await cmd.ExecuteNonQueryAsync();

        var diff = await _sut.ComputeDiffAsync(typeof(ExtendedEntity));

        diff.IsUpToDate.Should().BeFalse();
        diff.TableDiff.ColumnDiffs
            .Should().ContainSingle(d => d.Kind == ColumnDiffKind.Added && d.ColumnName == "Description");
    }

    // ---------- ComputeDiffAsync — column removed ----------

    [Fact]
    public async Task ComputeDiffAsync_DetectsRemovedColumn()
    {
        // Create table with an extra column that is not on the model
        await _db.OpenAsync();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE SimpleEntitys (Id INTEGER, Name TEXT, Price REAL, Obsolete TEXT)";
        await cmd.ExecuteNonQueryAsync();

        var diff = await _sut.ComputeDiffAsync(typeof(SimpleEntity));

        diff.TableDiff.ColumnDiffs
            .Should().ContainSingle(d => d.Kind == ColumnDiffKind.Removed && d.ColumnName == "Obsolete");
    }

    // ---------- ComputeDiffAsync — type changed ----------

    [Fact]
    public async Task ComputeDiffAsync_DetectsTypeChange()
    {
        // Store Price as TEXT but the model expects REAL
        await _db.OpenAsync();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE SimpleEntitys (Id INTEGER, Name TEXT, Price TEXT)";
        await cmd.ExecuteNonQueryAsync();

        var diff = await _sut.ComputeDiffAsync(typeof(SimpleEntity));

        diff.TableDiff.ColumnDiffs
            .Should().ContainSingle(d => d.Kind == ColumnDiffKind.TypeChanged && d.ColumnName == "Price");
    }

    // ---------- GetActualSchemaAsync — empty when table missing ----------

    [Fact]
    public async Task GetActualSchemaAsync_ReturnsEmptyDictionary_WhenTableMissing()
    {
        var schema = await _sut.GetActualSchemaAsync("NonExistentTable");
        schema.Should().BeEmpty();
    }
}
