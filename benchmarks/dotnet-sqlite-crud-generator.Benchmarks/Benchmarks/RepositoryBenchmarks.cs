#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for repository CRUD operations covering the most common database operations.
/// Measures throughput and memory allocations for basic repository methods.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class RepositoryBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private ProductRepository _productRepository = null!;
    private UserRepository _userRepository = null!;
    private Product _sampleProduct = null!;
    private User _sampleUser = null!;
    private const int BatchSize = 1000;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _productRepository = new ProductRepository(_database);
        _userRepository = new UserRepository(_database);

        // Create sample data
        _sampleProduct = new Product
        {
            Name = "Benchmark Product",
            Description = "High-performance benchmarking product",
            Sku = "BM-PROD-001",
            CategoryId = 1,
            Price = 999.99m,
            Cost = 799.99m,
            StockQuantity = 1000,
            ReorderLevel = 100,
            Unit = "piece",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _sampleUser = new User
        {
            Username = "benchmark_user",
            Email = "benchmark@example.com",
            PasswordHash = "hashed_password_12345",
            FirstName = "Benchmark",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Bio = "Benchmark user for performance testing",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Pre-populate database with sample data
        await _productRepository.AddAsync(_sampleProduct);
        await _userRepository.AddAsync(_sampleUser);
        await _database.SaveChangesAsync();
    }

    [Benchmark(Description = "Repository: GetByIdAsync (Product)")]
    public async Task<Product?> GetByIdProductAsync()
        => await _productRepository.GetByIdAsync(_sampleProduct.Id);

    [Benchmark(Description = "Repository: GetByIdAsync (User)")]
    public async Task<User?> GetByIdUserAsync()
        => await _userRepository.GetByIdAsync(_sampleUser.Id);

    [Benchmark(Description = "Repository: GetAllAsync (Product)")]
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _productRepository.GetAllAsync();

    [Benchmark(Description = "Repository: GetAllAsync (User)")]
    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _userRepository.GetAllAsync();

    [Benchmark(Description = "Repository: FindAsync with predicate (Product)")]
    public async Task<IEnumerable<Product>> FindProductsAsync()
        => await _productRepository.FindAsync(p => p.Price > 500);

    [Benchmark(Description = "Repository: FindAsync with predicate (User)")]
    public async Task<IEnumerable<User>> FindUsersAsync()
        => await _userRepository.FindAsync(u => u.Email.Contains("example"));

    [Benchmark(Description = "Repository: CountAsync (Product)")]
    public async Task<int> CountProductsAsync()
        => await _productRepository.CountAsync();

    [Benchmark(Description = "Repository: CountAsync with predicate (User)")]
    public async Task<int> CountUsersAsync()
        => await _userRepository.CountAsync(u => u.IsActive);

    [Benchmark(Description = "Repository: ExistsAsync (Product)")]
    public async Task<bool> ExistsProductAsync()
        => await _productRepository.ExistsAsync(_sampleProduct.Id);

    [Benchmark(Description = "Repository: ExistsAsync (User)")]
    public async Task<bool> ExistsUserAsync()
        => await _userRepository.ExistsAsync(_sampleUser.Id);

    [Benchmark(Description = "Repository: AddAsync (Product)")]
    public async Task<Product> AddProductAsync()
    {
        var product = new Product
        {
            Name = "New Product",
            Sku = "NEW-PROD-001",
            CategoryId = 1,
            Price = 49.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return await _productRepository.AddAsync(product);
    }

    [Benchmark(Description = "Repository: AddAsync (User)")]
    public async Task<User> AddUserAsync()
    {
        var user = new User
        {
            Username = "new_user",
            Email = "new@example.com",
            PasswordHash = "hashed_password_new",
            FirstName = "New",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return await _userRepository.AddAsync(user);
    }

    [Benchmark(Description = "Repository: UpdateAsync (Product)")]
    public async Task<bool> UpdateProductAsync()
    {
        _sampleProduct.Price = 1099.99m;
        _sampleProduct.StockQuantity = 1500;
        _sampleProduct.UpdatedAt = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(_sampleProduct);
    }

    [Benchmark(Description = "Repository: UpdateAsync (User)")]
    public async Task<bool> UpdateUserAsync()
    {
        _sampleUser.PhoneNumber = "+1987654321";
        _sampleUser.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(_sampleUser);
    }

    [Benchmark(Description = "Repository: DeleteAsync (Product)")]
    public async Task<bool> DeleteProductAsync()
        => await _productRepository.DeleteAsync(_sampleProduct.Id);

    [Benchmark(Description = "Repository: DeleteAsync (User)")]
    public async Task<bool> DeleteUserAsync()
        => await _userRepository.DeleteAsync(_sampleUser.Id);

    [Benchmark(Description = "Repository: SaveChangesAsync")]
    public async Task<int> SaveChangesAsync()
        => await _database.SaveChangesAsync();

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

/// <summary>
/// Concrete repository implementation for Product used in benchmarks.
/// </summary>
public sealed class ProductRepository : Repository<Product, int>
{
    public ProductRepository(DatabaseConnection database) : base(database) { }
}

/// <summary>
/// Concrete repository implementation for User used in benchmarks.
/// </summary>
public sealed class UserRepository : Repository<User, int>
{
    public UserRepository(DatabaseConnection database) : base(database) { }
}