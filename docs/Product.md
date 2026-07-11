# Product
The `Product` type represents a single product entity in an e-commerce or inventory management system, encapsulating its properties and behaviors. It provides a structured way to store and manipulate product data, including name, description, pricing, stock levels, and activation status.

## API
### Properties
* `Id`: A unique identifier for the product, represented as an integer.
* `Name`: The name of the product, represented as a required string.
* `Description`: An optional description of the product, represented as a nullable string.
* `Sku`: The stock-keeping unit identifier for the product, represented as a required string.
* `CategoryId`: The category identifier for the product, represented as an integer.
* `Price`: The selling price of the product, represented as a decimal.
* `Cost`: The cost of the product, represented as a decimal.
* `StockQuantity`: The current stock quantity of the product, represented as an integer.
* `ReorderLevel`: The reorder level for the product, represented as an integer.
* `Unit`: The unit of measurement for the product, represented as a string.
* `IsActive`: A boolean indicating whether the product is active or not.
* `CreatedAt`: The date and time when the product was created, represented as a DateTime.
* `UpdatedAt`: The date and time when the product was last updated, represented as a DateTime.
### Methods
* `Validate`: A method to validate the product, returning a boolean indicating whether the product is valid or not.
* `GetProfitMarginPercentage`: A method to calculate the profit margin percentage of the product, returning a decimal value.
* `IsLowStock`: A method to check if the product is low in stock, returning a boolean indicating whether the stock quantity is below the reorder level or not.
* `AddStock`: A method to add stock to the product, taking no parameters and returning no value.
* `RemoveStock`: A method to remove stock from the product, taking no parameters and returning no value.
* `Deactivate`: A method to deactivate the product, taking no parameters and returning no value.

## Usage
The following examples demonstrate how to use the `Product` type:
```csharp
// Create a new product
var product = new Product
{
    Name = "Example Product",
    Sku = "EXMPL",
    Price = 9.99m,
    Cost = 5.00m,
    StockQuantity = 100,
    ReorderLevel = 20,
    Unit = "units"
};

// Validate the product and check its profit margin
if (product.Validate())
{
    var profitMargin = product.GetProfitMarginPercentage;
    Console.WriteLine($"Profit margin: {profitMargin}%");
}
else
{
    Console.WriteLine("Product is invalid");
}

// Add stock to the product
product.AddStock();
Console.WriteLine($"New stock quantity: {product.StockQuantity}");
```

## Notes
When using the `Product` type, consider the following edge cases:
* The `Validate` method may throw an exception if the product data is invalid or incomplete.
* The `GetProfitMarginPercentage` method may return a negative value if the product's cost is greater than its price.
* The `IsLowStock` method may return `true` if the stock quantity is below the reorder level, even if the product is not active.
* The `AddStock` and `RemoveStock` methods may throw an exception if the stock quantity would exceed the maximum allowed value or go below zero, respectively.
* The `Deactivate` method may throw an exception if the product is already deactivated.
Regarding thread-safety, the `Product` type is not designed to be thread-safe, and concurrent access to its properties and methods may result in unexpected behavior or data corruption. It is recommended to use synchronization mechanisms, such as locks or semaphores, to ensure thread-safe access to the `Product` type.
