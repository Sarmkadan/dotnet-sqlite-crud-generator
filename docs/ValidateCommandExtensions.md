# ValidateCommandExtensions

The `ValidateCommandExtensions` class provides a suite of static utility methods designed to streamline the validation of domain models within the `dotnet-sqlite-crud-generator` framework. These extensions integrate with standard .NET data annotation validation, allowing developers to perform robust integrity checks on single entities or collections of entities before proceeding with CRUD operations.

## API

### ValidateModel

Performs validation on a single model instance based on applied data annotations.

*   **Parameters:** `object model` – The entity instance to validate.
*   **Returns:** `List<ValidationResult>` – A list of all discovered validation issues.
*   **Throws:** `ArgumentNullException` if the provided model is null.

### ValidateAllModels

Validates a collection of model instances.

*   **Parameters:** `IEnumerable<object> models` – The collection of entities to validate.
*   **Returns:** `List<ValidationResult>` – An aggregated list of validation issues found across all models in the collection.
*   **Throws:** `ArgumentNullException` if the provided collection is null.

### GetErrors

Filters a set of validation results, returning only those categorized as validation errors.

*   **Parameters:** `List<ValidationResult> results` – The full list of validation results to filter.
*   **Returns:** `List<ValidationResult>` – A subset containing only items classified as errors.
*   **Throws:** `ArgumentNullException` if the results list is null.

### GetWarnings

Filters a set of validation results, returning only those categorized as validation warnings.

*   **Parameters:** `List<ValidationResult> results` – The full list of validation results to filter.
*   **Returns:** `List<ValidationResult>` – A subset containing only items classified as warnings.
*   **Throws:** `ArgumentNullException` if the results list is null.

## Usage

### Single Model Validation

```csharp
var newUser = new User { Username = string.Empty }; // Assumes [Required] attribute
var results = ValidateCommandExtensions.ValidateModel(newUser);

if (results.Any())
{
    var errors = ValidateCommandExtensions.GetErrors(results);
    // Handle validation errors
}
```

### Batch Processing and Filtering

```csharp
var users = new List<User> { /* ... */ };
var allResults = ValidateCommandExtensions.ValidateAllModels(users);

var errors = ValidateCommandExtensions.GetErrors(allResults);
var warnings = ValidateCommandExtensions.GetWarnings(allResults);

Console.WriteLine($"Found {errors.Count} errors and {warnings.Count} warnings.");
```

## Notes

*   **Thread Safety:** The methods in this class are stateless and static, making them thread-safe for concurrent use, provided that the objects passed into them are not being modified by other threads during the validation process.
*   **Data Annotations:** These methods rely on standard .NET `System.ComponentModel.DataAnnotations`. Ensure your models are appropriately decorated with attributes (e.g., `[Required]`, `[StringLength]`) for validation to yield results.
*   **Performance:** Validation of large collections using `ValidateAllModels` may incur significant CPU overhead depending on the complexity of the validation rules applied to the models.
*   **Edge Cases:** Passing an object that has no validation attributes or a null collection will result in an empty list being returned, rather than a failure, unless the input itself is null, which triggers an `ArgumentNullException`.
