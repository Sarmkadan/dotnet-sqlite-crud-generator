# Category

Represents a hierarchical category entity for organizing data with support for parent-child relationships, slug generation for URL-friendly identifiers, and activation state management.

## API

### Id
```csharp
public int Id
```
Unique identifier for the category. Assigned by the database upon insertion.

### Name
```csharp
public required string Name
```
Display name of the category. Required; cannot be null or empty.

### Description
```csharp
public string? Description
```
Optional detailed description of the category. Nullable.

### ParentCategoryId
```csharp
public int? ParentCategoryId
```
Foreign key referencing the parent category. Null indicates a root-level category. Nullable.

### Slug
```csharp
public string? Slug
```
URL-friendly identifier derived from the name. Nullable; typically generated via `GenerateSlug`.

### DisplayOrder
```csharp
public int DisplayOrder
```
Sort order for display purposes. Defaults to 0. Higher values appear later.

### IsActive
```csharp
public bool IsActive
```
Indicates whether the category is active and visible. Defaults to true.

### CreatedAt
```csharp
public DateTime CreatedAt
```
Timestamp when the category was created. Set automatically on insertion.

### UpdatedAt
```csharp
public DateTime UpdatedAt
```
Timestamp when the category was last modified. Updated automatically on changes.

### Validate
```csharp
public bool Validate()
```
Validates the category state. Returns true if `Name` is not null or whitespace and `DisplayOrder` is non-negative; otherwise false. Does not throw.

### GenerateSlug
```csharp
public void GenerateSlug()
```
Generates a URL-friendly slug from `Name` by lowercasing, replacing spaces with hyphens, and removing non-alphanumeric characters. Sets the `Slug` property. Throws `InvalidOperationException` if `Name` is null or whitespace.

### IsRootCategory
```csharp
public bool IsRootCategory()
```
Returns true if `ParentCategoryId` is null; otherwise false. Does not throw.

### Deactivate
```csharp
public void Deactivate()
```
Sets `IsActive` to false and updates `UpdatedAt` to the current UTC time. Does not throw.

## Usage

### Creating a root category with automatic slug generation
```csharp
var category = new Category
{
    Name = "Electronics",
    Description = "Electronic devices and accessories",
    DisplayOrder = 1
};
category.GenerateSlug();
// category.Slug == "electronics"
// category.IsRootCategory() == true
```

### Creating a child category and deactivating it
```csharp
var parent = new Category { Name = "Computers", DisplayOrder = 1 };
parent.GenerateSlug();

var child = new Category
{
    Name = "Laptops",
    ParentCategoryId = parent.Id,
    DisplayOrder = 2
};
child.GenerateSlug();

if (child.Validate())
{
    child.Deactivate();
    // child.IsActive == false
    // child.UpdatedAt updated to current UTC time
}
```

## Notes

- `GenerateSlug` mutates the `Slug` property in place; call it after setting `Name` to ensure consistency.
- `Validate` performs only basic structural validation; it does not verify uniqueness of `Name` or `Slug` within the database.
- `Deactivate` updates `UpdatedAt` to `DateTime.UtcNow`; callers relying on precise timestamps should be aware of potential clock skew in distributed environments.
- The type is not thread-safe. Concurrent mutation of properties or invocation of methods from multiple threads without external synchronization may result in inconsistent state.
- `ParentCategoryId` does not enforce referential integrity at the object level; database constraints or application logic must prevent cycles in the category hierarchy.
- `CreatedAt` and `UpdatedAt` use `DateTime` (not `DateTimeOffset`); ensure consistent UTC handling across the application to avoid timezone ambiguity.
