#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for managing product operations with inventory and pricing logic.
/// </summary>
public sealed class ProductService : IService<Product, int>
{
    private readonly IRepository<Product, int> _productRepository;
    private readonly IRepository<Category, int> _categoryRepository;

    public ProductService(IRepository<Product, int> productRepository, IRepository<Category, int> categoryRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }

    public async Task<Product?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(id));

        return await _productRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Product> CreateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("Product validation failed. Check required fields and constraints.");

        var existingSku = await ((ProductRepository)_productRepository).GetBySkuAsync(entity.Sku, cancellationToken);
        if (existingSku is not null)
            throw RepositoryException.DuplicateKey(nameof(Product), nameof(Product.Sku), entity.Sku);

        var category = await _categoryRepository.GetByIdAsync(entity.CategoryId, cancellationToken);
        if (category is null)
            throw new ValidationException($"Category with ID {entity.CategoryId} does not exist.");

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        return await _productRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id <= 0)
            throw new ArgumentException("Invalid product ID", nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("Product validation failed. Check required fields and constraints.");

        var existing = await GetAsync(entity.Id, cancellationToken);
        if (existing is null)
            throw RepositoryException.EntityNotFound(nameof(Product), entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;
        return await _productRepository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0", nameof(id));

        var product = await GetAsync(id, cancellationToken);
        if (product is null)
            throw RepositoryException.EntityNotFound(nameof(Product), id);

        return await _productRepository.DeleteAsync(id, cancellationToken);
    }

    public bool Validate(Product entity)
    {
        if (entity is null) return false;
        return entity.Validate();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productRepository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets products in a specific category.
    /// </summary>
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await ((ProductRepository)_productRepository).GetByCategoryAsync(categoryId, cancellationToken);
    }

    /// <summary>
    /// Gets products with low stock levels.
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await ((ProductRepository)_productRepository).GetLowStockAsync(cancellationToken);
    }

    /// <summary>
    /// Restocks a product by adding inventory.
    /// </summary>
    public async Task<Product> RestockProductAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Restock quantity must be greater than 0", nameof(quantity));

        var product = await GetAsync(productId, cancellationToken);
        if (product is null)
            throw RepositoryException.EntityNotFound(nameof(Product), productId);

        product.AddStock(quantity);
        var updated = await UpdateAsync(product, cancellationToken);
        if (!updated)
        {
            throw new InvalidOperationException($"Failed to restock product with ID {productId}");
        }
        return (await GetAsync(productId, cancellationToken))!;
    }

    /// <summary>
    /// Sells product inventory.
    /// </summary>
    public async Task<Product> SellProductAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Sale quantity must be greater than 0", nameof(quantity));

        var product = await GetAsync(productId, cancellationToken);
        if (product is null)
            throw RepositoryException.EntityNotFound(nameof(Product), productId);

        if (product.StockQuantity < quantity)
            throw new ValidationException($"Insufficient stock. Available: {product.StockQuantity}, Requested: {quantity}");

        product.RemoveStock(quantity);
        var updated = await UpdateAsync(product, cancellationToken);
        if (!updated)
        {
            throw new InvalidOperationException($"Failed to sell product with ID {productId}");
        }
        return (await GetAsync(productId, cancellationToken))!;
    }

    /// <summary>
    /// Calculates inventory value.
    /// </summary>
    public async Task<decimal> GetInventoryValueAsync(CancellationToken cancellationToken = default)
    {
        var products = await GetAllAsync(cancellationToken);
        return products.Sum(p => p.Cost * p.StockQuantity);
    }

    /// <summary>
    /// Gets inventory statistics.
    /// </summary>
    public async Task<ProductInventoryStats> GetInventoryStatsAsync(CancellationToken cancellationToken = default)
    {
        var products = (await GetAllAsync(cancellationToken)).ToList();
        var lowStock = await GetLowStockProductsAsync();

        return new ProductInventoryStats
        {
            TotalProducts = products.Count,
            TotalUnitsInStock = products.Sum(p => p.StockQuantity),
            TotalInventoryValue = await GetInventoryValueAsync(),
            LowStockCount = lowStock.Count(),
            AverageStockLevel = products.Any() ? products.Average(p => p.StockQuantity) : 0,
            HighestPricedProduct = products.OrderByDescending(p => p.Price).FirstOrDefault(),
            LowestStockProduct = products.OrderBy(p => p.StockQuantity).FirstOrDefault()
        };
    }
}

/// <summary>
/// Statistics about product inventory.
/// </summary>
public sealed class ProductInventoryStats
{
    public int TotalProducts { get; set; }
    public int TotalUnitsInStock { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public double AverageStockLevel { get; set; }
    public Product? HighestPricedProduct { get; set; }
    public Product? LowestStockProduct { get; set; }
}
