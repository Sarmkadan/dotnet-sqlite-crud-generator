#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Unit of work implementation managing multiple repositories and database transactions.
/// </summary>
public sealed class DbContextProvider : IUnitOfWork
{
    private readonly DatabaseConnection _database;
    private UserRepository? _userRepository;
    private ProductRepository? _productRepository;
    private OrderRepository? _orderRepository;
    private CategoryRepository? _categoryRepository;
    private AuditLogRepository? _auditLogRepository;
    private bool _disposed;

    public DbContextProvider(DatabaseConnection database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public IRepository<User, int> Users => _userRepository ??= new UserRepository(_database);
    public IRepository<Product, int> Products => _productRepository ??= new ProductRepository(_database);
    public IRepository<Order, int> Orders => _orderRepository ??= new OrderRepository(_database);
    public IRepository<Category, int> Categories => _categoryRepository ??= new CategoryRepository(_database);
    public IRepository<AuditLog, int> AuditLogs => _auditLogRepository ??= new AuditLogRepository(_database);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int changeCount = 0;
        if (_userRepository is not null) changeCount += await _userRepository.SaveChangesAsync(cancellationToken);
        if (_productRepository is not null) changeCount += await _productRepository.SaveChangesAsync(cancellationToken);
        if (_orderRepository is not null) changeCount += await _orderRepository.SaveChangesAsync(cancellationToken);
        if (_categoryRepository is not null) changeCount += await _categoryRepository.SaveChangesAsync(cancellationToken);
        if (_auditLogRepository is not null) changeCount += await _auditLogRepository.SaveChangesAsync(cancellationToken);
        return changeCount;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _database.OpenAsync(cancellationToken);
        using var command = _database.Connection.CreateCommand();
        command.CommandText = "BEGIN TRANSACTION";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        using var command = _database.Connection.CreateCommand();
        command.CommandText = "COMMIT";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        using var command = _database.Connection.CreateCommand();
        command.CommandText = "ROLLBACK";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _database?.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_database is not null)
            await _database.DisposeAsync();
        _disposed = true;
    }
}

/// <summary>
/// Repository for User entities.
/// </summary>
public sealed class UserRepository : Repository<User, int>
{
    public UserRepository(DatabaseConnection database) : base(database) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return (await FindAsync(u => u.Email == email, cancellationToken)).FirstOrDefault();
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return (await FindAsync(u => u.Username == username, cancellationToken)).FirstOrDefault();
    }
}

/// <summary>
/// Repository for Product entities.
/// </summary>
public sealed class ProductRepository : Repository<Product, int>
{
    public ProductRepository(DatabaseConnection database) : base(database) { }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return (await FindAsync(p => p.Sku == sku, cancellationToken)).FirstOrDefault();
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.IsLowStock(), cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(p => p.CategoryId == categoryId && p.IsActive, cancellationToken);
    }
}

/// <summary>
/// Repository for Order entities.
/// </summary>
public sealed class OrderRepository : Repository<Order, int>
{
    public OrderRepository(DatabaseConnection database) : base(database) { }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return (await FindAsync(o => o.OrderNumber == orderNumber, cancellationToken)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await FindAsync(o => o.Status == Enums.EntityStatus.Pending, cancellationToken);
    }
}

/// <summary>
/// Repository for Category entities.
/// </summary>
public sealed class CategoryRepository : Repository<Category, int>
{
    public CategoryRepository(DatabaseConnection database) : base(database) { }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await FindAsync(c => c.IsRootCategory() && c.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(c => c.ParentCategoryId == parentId && c.IsActive, cancellationToken);
    }
}

/// <summary>
/// Repository for AuditLog entities.
/// </summary>
public sealed class AuditLogRepository : Repository<AuditLog, int>
{
    public AuditLogRepository(DatabaseConnection database) : base(database) { }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(a => a.EntityType == entityType && a.EntityId == entityId, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(a => a.ChangedByUserId == userId, cancellationToken);
    }
}
