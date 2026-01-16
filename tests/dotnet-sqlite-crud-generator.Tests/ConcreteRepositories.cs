// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class ConcreteProductRepository : Repository<Product, int>
{
    public ConcreteProductRepository(DatabaseConnection database) : base(database)
    {
    }
}

public class ConcreteUserRepository : Repository<User, int>
{
    public ConcreteUserRepository(DatabaseConnection database) : base(database)
    {
    }
}