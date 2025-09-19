#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Concrete implementation of the <see cref="Repository{T, TKey}"/> for the <see cref="Product"/> entity.
/// </summary>
public sealed class ConcreteProductRepository : Repository<Product, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcreteProductRepository"/> class.
    /// </summary>
    /// <param name="database">The database connection to use.</param>
    public ConcreteProductRepository(DatabaseConnection database) : base(database)
    {
    }
}

/// <summary>
/// Concrete implementation of the <see cref="Repository{T, TKey}"/> for the <see cref="User"/> entity.
/// </summary>
public sealed class ConcreteUserRepository : Repository<User, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcreteUserRepository"/> class.
    /// </summary>
    /// <param name="database">The database connection to use.</param>
    public ConcreteUserRepository(DatabaseConnection database) : base(database)
    {
    }
}
