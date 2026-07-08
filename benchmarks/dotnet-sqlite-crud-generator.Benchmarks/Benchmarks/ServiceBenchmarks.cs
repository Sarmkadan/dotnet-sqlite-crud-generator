#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;

namespace DotNet.SQLite.CrudGenerator.Benchmarks;

/// <summary>
/// Benchmarks for service layer operations covering business logic and service methods.
/// Measures throughput and memory allocations for service operations that wrap repository calls.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public sealed class ServiceBenchmarks : IDisposable
{
    private DatabaseConnection _database = null!;
    private ProductService _productService = null!;
    private UserService _userService = null!;
    private CategoryService _categoryService = null!;
    private Product _sampleProduct = null!;
    private User _sampleUser = null!;
    private Category _sampleCategory = null!;
    private const int BatchSize = 1000;
    private const int TotalItems = 10000;

    [GlobalSetup]
    public async Task Setup()
    {
        // Use in-memory SQLite database for benchmarks
        _database = new DatabaseConnection("Data Source=:memory:");
        await _database.InitializeAsync();

        _productService = new ProductService(
            new ProductRepository(_database),
            new CategoryRepository(_database)
        );

        _userService = new UserService(new UserRepository(_database));
        _categoryService = new CategoryService(new CategoryRepository(_database));

        // Create sample data
        _sampleCategory = new Category
        {
            Name = "Benchmark Category",
            Description = "Category for performance testing",
            DisplayOrder = 1,
            IsActive = true
        };

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

        // Pre-populate database
        await _categoryService.CreateAsync(_sampleCategory);
        await _productService.CreateAsync(_sampleProduct);
        await _userService.CreateAsync(_sampleUser);
        await _database.SaveChangesAsync();
    }

    [Benchmark(Description = "Service: GetByIdAsync (Product)")]
    public async Task<Product?> GetProductByIdAsync()
        => await _productService.GetAsync(_sampleProduct.Id);

    [Benchmark(Description = "Service: GetByIdAsync (User)")]
    public async Task<User?> GetUserByIdAsync()
        => await _userService.GetAsync(_sampleUser.Id);

    [Benchmark(Description = "Service: GetByIdAsync (Category)")]
    public async Task<Category?> GetCategoryByIdAsync()
        => await _categoryService.GetAsync(_sampleCategory.Id);

    [Benchmark(Description = "Service: GetAllAsync (Product)")]
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
        => await _productService.GetAllAsync();

    [Benchmark(Description = "Service: GetAllAsync (User)")]
    public async Task<IEnumerable<User>> GetAllUsersAsync()
        => await _userService.GetAllAsync();

    [Benchmark(Description = "Service: GetAllAsync (Category)")]
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        => await _categoryService.GetAllAsync();

    [Benchmark(Description = "Service: FindAsync (Product by price)")]
    public async Task<IEnumerable<Product>> FindExpensiveProductsAsync()
        => await _productService.FindAsync(p => p.Price > 500);

    [Benchmark(Description = "Service: FindAsync (User by email)")]
    public async Task<IEnumerable<User>> FindUsersByEmailAsync()
        => await _userService.FindAsync(u => u.Email.Contains("example"));

    [Benchmark(Description = "Service: CountAsync (Product)")]
    public async Task<int> CountProductsAsync()
        => await _productService.CountAsync();

    [Benchmark(Description = "Service: CountAsync (User)")]
    public async Task<int> CountUsersAsync()
        => await _userService.CountAsync(u => u.IsActive);

    [Benchmark(Description = "Service: ExistsAsync (Product)")]
    public async Task<bool> ExistsProductAsync()
        => await _productService.ExistsAsync(_sampleProduct.Id);

    [Benchmark(Description = "Service: ExistsAsync (User)")]
    public async Task<bool> ExistsUserAsync()
        => await _userService.ExistsAsync(_sampleUser.Id);

    [Benchmark(Description = "Service: CreateAsync (Product)")]
    public async Task<Product> CreateProductAsync()
    {
        var product = new Product
        {
            Name = "New Product",
            Sku = "NEW-PROD-001",
            CategoryId = _sampleCategory.Id,
            Price = 49.99m,
            Cost = 39.99m,
            StockQuantity = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return await _productService.CreateAsync(product);
    }

    [Benchmark(Description = "Service: CreateAsync (User)")]
    public async Task<User> CreateUserAsync()
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
        return await _userService.CreateAsync(user);
    }

    [Benchmark(Description = "Service: UpdateAsync (Product)")]
    public async Task<bool> UpdateProductAsync()
    {
        _sampleProduct.Price = 1099.99m;
        _sampleProduct.StockQuantity = 1500;
        _sampleProduct.UpdatedAt = DateTime.UtcNow;
        return await _productService.UpdateAsync(_sampleProduct);
    }

    [Benchmark(Description = "Service: UpdateAsync (User)")]
    public async Task<bool> UpdateUserAsync()
    {
        _sampleUser.PhoneNumber = "+1987654321";
        _sampleUser.UpdatedAt = DateTime.UtcNow;
        return await _userService.UpdateAsync(_sampleUser);
    }

    [Benchmark(Description = "Service: DeleteAsync (Product)")]
    public async Task<bool> DeleteProductAsync()
        => await _productService.DeleteAsync(_sampleProduct.Id);

    [Benchmark(Description = "Service: DeleteAsync (User)")]
    public async Task<bool> DeleteUserAsync()
        => await _userService.DeleteAsync(_sampleUser.Id);

    [Benchmark(Description = "Service: GetByCategoryAsync")]
    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync()
        => await _productService.GetByCategoryAsync(_sampleCategory.Id);

    [Benchmark(Description = "Service: GetLowStockProductsAsync")]
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        => await _productService.GetLowStockProductsAsync();

    [Benchmark(Description = "Service: RestockProductAsync")]
    public async Task<Product> RestockProductAsync()
        => await _productService.RestockProductAsync(_sampleProduct.Id, 100);

    [Benchmark(Description = "Service: SellProductAsync")]
    public async Task<Product> SellProductAsync()
        => await _productService.SellProductAsync(_sampleProduct.Id, 10);

    [Benchmark(Description = "Service: GetInventoryValueAsync")]
    public async Task<decimal> GetInventoryValueAsync()
        => await _productService.GetInventoryValueAsync();

    [Benchmark(Description = "Service: GetInventoryStatsAsync")]
    public async Task<ProductInventoryStats> GetInventoryStatsAsync()
        => await _productService.GetInventoryStatsAsync();

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
/// Concrete repository implementation for Category used in benchmarks.
/// </summary>
public sealed class CategoryRepository : Repository<Category, int>
{
    public CategoryRepository(DatabaseConnection database) : base(database) { }
}

/// <summary>
/// Service for managing category operations.
/// </summary>
public sealed class CategoryService : IService<Category, int>
{
    private readonly IRepository<Category, int> _categoryRepository;

    public CategoryService(IRepository<Category, int> categoryRepository)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public Task<Category?> GetAsync(int id, CancellationToken cancellationToken = default)
        => _categoryRepository.GetByIdAsync(id, cancellationToken);

    public Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.GetAllAsync(cancellationToken);

    public Task<Category> CreateAsync(Category entity, CancellationToken cancellationToken = default)
        => _categoryRepository.AddAsync(entity, cancellationToken);

    public Task<bool> UpdateAsync(Category entity, CancellationToken cancellationToken = default)
        => _categoryRepository.UpdateAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _categoryRepository.DeleteAsync(id, cancellationToken);

    public bool Validate(Category entity) => entity?.Validate() ?? false;

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => _categoryRepository.ExistsAsync(id, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.CountAsync(predicate: null, cancellationToken);

    public Task<IEnumerable<Category>> FindAsync(Func<Category, bool> predicate, CancellationToken cancellationToken = default)
        => _categoryRepository.FindAsync(predicate, cancellationToken);
}