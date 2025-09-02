# CategoryExtensions

The `CategoryExtensions` class provides a suite of static extension methods designed to simplify the navigation and representation of hierarchical category structures. By extending the base `Category` entity, these utilities facilitate efficient retrieval of ancestor and descendant chains, as well as the generation of formatted path strings, streamlining the management of tree-based relational data within the application.

## API

### GetPath
Retrieves the complete hierarchical lineage of the specified category, from the root node down to the category itself.

*   **Parameters:** `this Category category`
*   **Returns:** `IEnumerable<Category>` containing the full path, ordered from root to leaf.
*   **Exceptions:** Throws `ArgumentNullException` if the category is null.

### GetPathString
Generates a string representation of the category's hierarchy, concatenating the category names along the path.

*   **Parameters:** `this Category category`, `string separator = " > "`
*   **Returns:** `string` representing the full path hierarchy.
*   **Exceptions:** Throws `ArgumentNullException` if the category is null.

### GetDescendants
Retrieves all nested child, grandchild, and subsequent descendant categories within the hierarchy.

*   **Parameters:** `this Category category`
*   **Returns:** `IEnumerable<Category>` containing all descendants of the category.
*   **Exceptions:** Throws `ArgumentNullException` if the category is null.

### GetAncestors
Retrieves the full chain of parent categories for the specified category, moving upwards toward the root node, excluding the category itself.

*   **Parameters:** `this Category category`
*   **Returns:** `IEnumerable<Category>` containing all ancestors of the category.
*   **Exceptions:** Throws `ArgumentNullException` if the category is null.

## Usage

### Generating a breadcrumb string
```csharp
Category electronics = categoryRepository.GetById(101);
// Generates output like: "Home > Electronics > Computers"
string breadcrumb = electronics.GetPathString(separator: " > ");
Console.WriteLine(breadcrumb);
```

### Processing all descendants
```csharp
Category rootCategory = categoryRepository.GetById(1);
// Traverse all child and grandchild categories to update status
foreach (var descendant in rootCategory.GetDescendants())
{
    descendant.IsActive = false;
    categoryRepository.Update(descendant);
}
```

## Notes

*   **Hierarchy Integrity:** These methods assume the underlying `Category` entities are structured in a valid tree hierarchy. Behavior in the presence of circular references is undefined and may lead to infinite recursion.
*   **Thread Safety:** The extension methods themselves are static and stateless, making them thread-safe for concurrent read operations. However, they rely on the properties of the `Category` instance; if the hierarchy structure is being modified concurrently on other threads, the results returned by these methods may be inconsistent.
*   **Null Handling:** As an extension method, calling these methods on a null reference will not work as expected. The implementation explicitly validates the `this` parameter and will throw an `ArgumentNullException` if the target category instance is null.
