# ValidationException

The `ValidationException` class represents an error that occurs during validation of data in the dotnet-sqlite-crud-generator project. It extends the standard `Exception` class and carries a collection of `ValidationError` objects, each describing a specific validation failure. The exception can be constructed with a general message, or built from a set of errors using the static factory method. It also exposes optional `Property` and `Message` properties to indicate which property failed and the associated error text.

## API

### `public ValidationException()`

Initializes a new instance of the `ValidationException` class with no message or errors.

### `public ValidationException(string message) : base(message)`

Initializes a new instance of the `ValidationException` class with a specified error message.

- **Parameters**  
  `message` – A string that describes the overall validation error.

### `public List<ValidationError> Errors { get; }`

Gets the list of `ValidationError` objects associated with this exception. Each error represents an individual validation failure.

- **Type** – `List<ValidationError>`

### `public void AddError()`

Adds a validation error to the `Errors` collection. The exact signature of the error object is not exposed by this method; it is intended to be called with a `ValidationError` instance.

### `public static ValidationException FromErrors()`

Creates a new `ValidationException` populated with a set of validation errors. The errors are provided as parameters to the method.

- **Returns** – A `ValidationException` instance containing the supplied errors.

### `public string? Property { get; set; }`

Gets or sets the name of the property that caused the validation failure. This value is `null` if the error is not property-specific.

### `public string? Message { get; set; }`

Gets or sets the human-readable message describing the validation error. This property overrides the inherited `Exception.Message` property.

## Usage

### Example 1: Throwing a validation exception with a single error

```csharp
public void ValidateName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
    {
        var ex = new ValidationException("Name is required.");
        ex.Property = "Name";
        ex.Message = "The name field cannot be empty.";
        ex.AddError(); // adds a ValidationError (details omitted)
        throw ex;
    }
}
```

### Example 2: Creating a validation exception from multiple errors

```csharp
public ValidationException ValidateUser(User user)
{
    var errors = new List<ValidationError>();
    if (string.IsNullOrEmpty(user.Name))
        errors.Add(new ValidationError("Name", "Name is required."));
    if (user.Age < 0)
        errors.Add(new ValidationError("Age", "Age cannot be negative."));

    if (errors.Count > 0)
        return ValidationException.FromErrors(); // errors are passed internally
    return null;
}
```

## Notes

- **Edge cases**  
  - The `Errors` collection is initially empty. Calling `AddError()` without first adding a `ValidationError` instance may result in a null reference or unexpected behavior, depending on the implementation.  
  - The `Property` and `Message` properties are nullable; consumers should check for `null` before using them.  
  - The `FromErrors()` static method expects at least one error to be provided; passing no arguments may produce an exception with an empty `Errors` list.

- **Thread safety**  
  Instances of `ValidationException` are not thread-safe. If the same exception object is accessed or modified concurrently from multiple threads, external synchronization is required. The `Errors` list is mutable and should not be modified after the exception is thrown unless the caller guarantees single-threaded access.
