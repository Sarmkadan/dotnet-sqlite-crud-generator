#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Interfaces;

/// <summary>
/// Unit of work pattern for managing multiple repositories and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IRepository<User, int> Users { get; }
    IRepository<Product, int> Products { get; }
    IRepository<Order, int> Orders { get; }
    IRepository<Category, int> Categories { get; }
    IRepository<AuditLog, int> AuditLogs { get; }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
