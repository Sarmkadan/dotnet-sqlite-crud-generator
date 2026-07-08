#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Enums;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for audit trail operations measuring performance of change tracking and querying.
/// Critical for compliance, debugging, and audit requirements in production systems.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class AuditTrailBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private AuditTrailService _auditService = null!;
    private ProductService _productService = null!;
    private UserService _userService = null!;
    private Product _sampleProduct = null!;
    private User _sampleUser = null!;
    private const int TotalOperations = 1000;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _auditService = new AuditTrailService(_database);
        _productService = new ProductService(
            new ProductRepository(_database),
            new CategoryRepository(_database)
        );
        _userService = new UserService(new UserRepository(_database));

        // Create sample entities
        _sampleProduct = new Product
        {
            Name = "Audit Product",
            Sku = "AUDIT-001",
            CategoryId = 1,
            Price = 99.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _sampleUser = new User
        {
            Username = "audit_user",
            Email = "audit@example.com",
            PasswordHash = "hashed_password",
            FirstName = "Audit",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create entities and record initial creation
        await _productService.CreateAsync(_sampleProduct);
        await _userService.CreateAsync(_sampleUser);
        await _database.SaveChangesAsync();

        // Record initial audit events
        await _auditService.RecordAsync(nameof(Product), _sampleProduct.Id, OperationType.Create, 1);
        await _auditService.RecordAsync(nameof(User), _sampleUser.Id, OperationType.Create, 1);
    }

    [Benchmark(Description = "AuditTrail: RecordAsync (Create operation)")]
    public async Task RecordCreateOperationAsync()
        => await _auditService.RecordAsync(nameof(Product), _sampleProduct.Id, OperationType.Create, 1);

    [Benchmark(Description = "AuditTrail: RecordAsync (Update operation)")]
    public async Task RecordUpdateOperationAsync()
    {
        _sampleProduct.Price = 109.99m;
        _sampleProduct.UpdatedAt = DateTime.UtcNow;
        await _productService.UpdateAsync(_sampleProduct);
        await _database.SaveChangesAsync();
        await _auditService.RecordAsync(nameof(Product), _sampleProduct.Id, OperationType.Update, 1);
    }

    [Benchmark(Description = "AuditTrail: RecordAsync (Delete operation)")]
    public async Task RecordDeleteOperationAsync()
    {
        await _productService.DeleteAsync(_sampleProduct.Id);
        await _database.SaveChangesAsync();
        await _auditService.RecordAsync(nameof(Product), _sampleProduct.Id, OperationType.Delete, 1);
    }

    [Benchmark(Description = "AuditTrail: GetEntityTrailAsync")]
    public async Task GetEntityTrailAsync()
        => await _auditService.GetEntityTrailAsync("Product", _sampleProduct.Id);

    [Benchmark(Description = "AuditTrail: GetUserTrailAsync")]
    public async Task GetUserTrailAsync()
        => await _auditService.GetUserTrailAsync(1, limit: 100);

    [Benchmark(Description = "AuditTrail: GetRecentAsync")]
    public async Task GetRecentAsync()
        => await _auditService.GetRecentAsync(50);

    [Benchmark(Description = "AuditTrail: QueryAsync")]
    public async Task QueryAsync()
    {
        var filter = new AuditTrailFilter
        {
            EntityType = "Product",
            OperationType = OperationType.Create,
            From = DateTime.UtcNow.AddDays(-1),
            Limit = 100
        };
        await _auditService.QueryAsync(filter);
    }

    [Benchmark(Description = "AuditTrail: GetSummaryAsync")]
    public async Task GetSummaryAsync()
        => await _auditService.GetSummaryAsync();

    [Benchmark(Description = "AuditTrail: BulkRecordAsync (100 operations)")]
    public async Task BulkRecordAsync()
    {
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_auditService.RecordAsync(nameof(User), _sampleUser.Id, OperationType.Update, 1));
        }
        await Task.WhenAll(tasks);
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _database.DisposeAsync();
    }

    public void Dispose()
    {
        _database?.Dispose();
    }
}