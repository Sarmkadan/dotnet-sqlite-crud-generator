#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Xunit;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Enums;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class AuditTrailServiceTests : IDisposable
{
    private readonly DatabaseConnection _db;
    private readonly AuditTrailService _sut;

    public AuditTrailServiceTests()
    {
        _db = new DatabaseConnection("Data Source=:memory:");
        _sut = new AuditTrailService(_db);
        _db.InitializeDatabaseAsync(false).GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    // ---------- RecordAsync ----------

    [Fact]
    public async Task RecordAsync_PersistsEntryToDatabase()
    {
        await _sut.RecordAsync("Product", 1, OperationType.Create, 42, newValues: "{\"Name\":\"Widget\"}");

        var results = await _sut.QueryAsync(new AuditTrailFilter { EntityType = "Product", EntityId = 1 });

        results.Should().HaveCount(1);
        results[0].EntityType.Should().Be("Product");
        results[0].EntityId.Should().Be(1);
        results[0].OperationType.Should().Be(OperationType.Create);
        results[0].ChangedByUserId.Should().Be(42);
        results[0].NewValues.Should().Be("{\"Name\":\"Widget\"}");
    }

    [Fact]
    public async Task RecordAsync_ThrowsOnEmptyEntityType()
    {
        Func<Task> act = async () =>
            await _sut.RecordAsync(string.Empty, 1, OperationType.Update, 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RecordAsync_Generic_SerializesObjectsToJson()
    {
        var before = new Models.Product { Id = 1, Name = "Old", Sku = "S1", CategoryId = 1, Price = 10m, Cost = 5m, StockQuantity = 0, ReorderLevel = 1, IsActive = true };
        var after = new Models.Product { Id = 1, Name = "New", Sku = "S1", CategoryId = 1, Price = 10m, Cost = 5m, StockQuantity = 0, ReorderLevel = 1, IsActive = true };

        await _sut.RecordAsync(1, OperationType.Update, 99, before, after);

        var results = await _sut.GetEntityTrailAsync("Product", 1);
        results.Should().NotBeEmpty();
        results[0].OldValues.Should().Contain("Old");
        results[0].NewValues.Should().Contain("New");
    }

    // ---------- QueryAsync ----------

    [Fact]
    public async Task QueryAsync_FiltersOnOperationType()
    {
        await _sut.RecordAsync("Order", 10, OperationType.Create, 1);
        await _sut.RecordAsync("Order", 10, OperationType.Update, 1);
        await _sut.RecordAsync("Order", 10, OperationType.Delete, 1);

        var creates = await _sut.QueryAsync(new AuditTrailFilter
        {
            EntityType = "Order",
            OperationType = OperationType.Create
        });

        creates.Should().HaveCount(1);
        creates[0].OperationType.Should().Be(OperationType.Create);
    }

    [Fact]
    public async Task QueryAsync_RespectsDateRange()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        await _sut.RecordAsync("User", 5, OperationType.Update, 2);
        var after = DateTime.UtcNow.AddSeconds(1);

        var inRange = await _sut.QueryAsync(new AuditTrailFilter { From = before, To = after });
        var outOfRange = await _sut.QueryAsync(new AuditTrailFilter { From = after.AddMinutes(1) });

        inRange.Should().NotBeEmpty();
        outOfRange.Should().BeEmpty();
    }

    // ---------- GetEntityTrailAsync ----------

    [Fact]
    public async Task GetEntityTrailAsync_ReturnsOnlyMatchingEntity()
    {
        await _sut.RecordAsync("Product", 1, OperationType.Update, 1);
        await _sut.RecordAsync("Product", 2, OperationType.Update, 1);

        var trail = await _sut.GetEntityTrailAsync("Product", 1);

        trail.Should().OnlyContain(e => e.EntityId == 1);
    }

    // ---------- GetRecentAsync ----------

    [Fact]
    public async Task GetRecentAsync_RespectsLimit()
    {
        for (int i = 0; i < 10; i++)
            await _sut.RecordAsync("Thing", i, OperationType.Create, 1);

        var recent = await _sut.GetRecentAsync(limit: 3);

        recent.Should().HaveCount(3);
    }

    // ---------- PurgeAsync ----------

    [Fact]
    public async Task PurgeAsync_DeletesOldEntries()
    {
        await _sut.RecordAsync("Log", 1, OperationType.Read, 1);

        var deleted = await _sut.PurgeAsync(DateTime.UtcNow.AddSeconds(5));

        deleted.Should().Be(1);
        var remaining = await _sut.GetRecentAsync();
        remaining.Should().BeEmpty();
    }

    // ---------- GetSummaryAsync ----------

    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectTotals()
    {
        await _sut.RecordAsync("Product", 1, OperationType.Create, 1);
        await _sut.RecordAsync("Product", 2, OperationType.Update, 1);
        await _sut.RecordAsync("Order", 3, OperationType.Delete, 1);

        var summary = await _sut.GetSummaryAsync();

        summary.TotalEntries.Should().Be(3);
        summary.ByEntityType.Should().ContainKey("Product");
        summary.ByEntityType["Product"].Should().Be(2);
        summary.ByEntityType.Should().ContainKey("Order");
        summary.ByOperation.Should().ContainKey("Create");
        summary.OldestEntry.Should().NotBeNull();
        summary.NewestEntry.Should().NotBeNull();
    }
}
