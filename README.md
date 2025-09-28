// existing content ...

## ServiceBenchmarks

The `ServiceBenchmarks` class provides a set of benchmarking methods to evaluate the performance of CRUD operations on a SQLite database. It allows you to measure the execution time of various database operations, such as getting, creating, updating, and deleting products and users.

Example usage:
```csharp
public class ServiceBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var serviceBenchmarks = new ServiceBenchmarks();
        await serviceBenchmarks.Setup();
        var product = await serviceBenchmarks.GetProductByIdAsync(1);
        var user = await serviceBenchmarks.GetUserByIdAsync(1);
        var products = await serviceBenchmarks.GetAllProductsAsync();
        var users = await serviceBenchmarks.GetAllUsersAsync();
        var expensiveProducts = await serviceBenchmarks.FindExpensiveProductsAsync();
        var usersByEmail = await serviceBenchmarks.FindUsersByEmailAsync("email@example.com");
        var count = await serviceBenchmarks.CountProductsAsync();
        var exists = await serviceBenchmarks.ExistsProductAsync(1);
        var createdProduct = await serviceBenchmarks.CreateProductAsync();
        var updatedProduct = await serviceBenchmarks.UpdateProductAsync(1);
        var deletedProduct = await serviceBenchmarks.DeleteProductAsync(1);
        var productsByCategory = await serviceBenchmarks.GetProductsByCategoryAsync(1);
        await serviceBenchmarks.Cleanup();
    }
}
```

## RepositoryBenchmarks

The `RepositoryBenchmarks` class provides a set of benchmarking methods to evaluate the performance of CRUD operations on a repository. It allows you to measure the execution time of various repository operations, such as getting, creating, updating, and deleting products and users.

Example usage:
```csharp
public class RepositoryBenchmarksExample
{
    public async Task RunBenchmarks()
    {
        var repositoryBenchmarks = new RepositoryBenchmarks();
        await repositoryBenchmarks.Setup();
        var product = await repositoryBenchmarks.GetByIdProductAsync(1);
        var user = await repositoryBenchmarks.GetByIdUserAsync(1);
        var products = await repositoryBenchmarks.GetAllProductsAsync();
        var users = await repositoryBenchmarks.GetAllUsersAsync();
        var productsFound = await repositoryBenchmarks.FindProductsAsync();
        var usersFound = await repositoryBenchmarks.FindUsersAsync();
        var count = await repositoryBenchmarks.CountProductsAsync();
        var exists = await repositoryBenchmarks.ExistsProductAsync(1);
        var createdProduct = await repositoryBenchmarks.AddProductAsync();
        var updatedProduct = await repositoryBenchmarks.UpdateProductAsync(1);
        var deletedProduct = await repositoryBenchmarks.DeleteProductAsync(1);
        var productsByCategory = await repositoryBenchmarks.GetProductsByCategoryAsync(1);
        await repositoryBenchmarks.Cleanup();
    }
}
```
