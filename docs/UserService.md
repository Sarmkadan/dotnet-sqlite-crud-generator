# UserService

`UserService` provides a high-level API for managing user entities in the SQLite-backed CRUD generator. It encapsulates all database operations related to users—creation, retrieval, update, deletion, validation, authentication, and activity reporting—exposing asynchronous methods that return domain objects or operation status indicators.

## API

### public UserService

Constructor. Initializes a new instance of the service, typically accepting an injected database context or connection factory. Exact parameters depend on the underlying implementation but are not part of the public surface documented here.

### public async Task<User?> GetAsync

Retrieves a single user by its primary identifier.

- **Parameters:** `id` (type inferred as `int` or `string` based on the model)
- **Returns:** The `User` object if found; `null` otherwise.
- **Throws:** `InvalidOperationException` if the underlying data source is unavailable.

### public async Task<IEnumerable<User>> GetAllAsync

Returns all users in the system without filtering.

- **Returns:** A collection of all `User` entities. May be empty if no users exist.
- **Throws:** `InvalidOperationException` on data source failure.

### public async Task<User> CreateAsync

Persists a new user and returns the created entity with any server-generated fields populated (e.g., ID, timestamps).

- **Parameters:** A `User` object containing at least the required fields.
- **Returns:** The fully populated `User` entity.
- **Throws:** `ArgumentException` when required fields are missing or validation fails. `InvalidOperationException` on database errors.

### public async Task<bool> UpdateAsync

Updates an existing user record.

- **Parameters:** A `User` object with the identifier set and modified fields populated.
- **Returns:** `true` if the update affected at least one row; `false` if the user was not found.
- **Throws:** `ArgumentException` when the identifier is invalid. `InvalidOperationException` on database errors.

### public async Task<bool> DeleteAsync

Removes a user by identifier.

- **Parameters:** `id` of the user to delete.
- **Returns:** `true` if deletion succeeded; `false` if the user did not exist.
- **Throws:** `InvalidOperationException` on database errors.

### public bool Validate

Checks whether a user object satisfies all business rules and constraints without persisting.

- **Parameters:** A `User` object to validate.
- **Returns:** `true` if the user is valid; `false` otherwise.
- **Throws:** Does not throw. Validation failures are communicated through the return value.

### public async Task<bool> ExistsAsync

Determines whether a user with the given identifier exists in the store.

- **Parameters:** `id` to check.
- **Returns:** `true` if the user exists; `false` otherwise.
- **Throws:** `InvalidOperationException` on data source failure.

### public Task<IEnumerable<User>> FindAsync

Searches for users matching a predicate or filter expression.

- **Parameters:** A filter criteria object or lambda (implementation-specific).
- **Returns:** A collection of matching `User` entities. Empty if no matches.
- **Throws:** `ArgumentException` if the filter is malformed. `InvalidOperationException` on database errors.

### public Task<int> CountAsync

Returns the total number of users in the store.

- **Returns:** A non-negative integer count.
- **Throws:** `InvalidOperationException` on database errors.

### public async Task<User?> AuthenticateAsync

Verifies credentials and returns the authenticated user.

- **Parameters:** Typically a username/email and password combination.
- **Returns:** The `User` if authentication succeeds; `null` if credentials are invalid or the user is deactivated.
- **Throws:** `ArgumentException` when credential parameters are null or empty. `InvalidOperationException` on database errors.

### public async Task<bool> ResetPasswordAsync

Initiates or completes a password reset for the specified user.

- **Parameters:** User identifier and a new password or reset token.
- **Returns:** `true` if the reset was applied successfully; `false` if the user does not exist or the token is invalid/expired.
- **Throws:** `ArgumentException` when the new password fails strength validation. `InvalidOperationException` on database errors.

### public async Task<bool> DeactivateUserAsync

Marks a user account as inactive without deleting it.

- **Parameters:** `id` of the user to deactivate.
- **Returns:** `true` if the user was deactivated; `false` if the user was not found or already inactive.
- **Throws:** `InvalidOperationException` on database errors.

### public async Task<bool> VerifyEmailAsync

Marks the user’s email address as verified.

- **Parameters:** User identifier and a verification token.
- **Returns:** `true` if verification succeeded; `false` if the user does not exist or the token is invalid/expired.
- **Throws:** `ArgumentException` when the token is null or empty. `InvalidOperationException` on database errors.

### public async Task<(int TotalUsers, int ActiveUsers, int VerifiedEmails)> GetActivitySummaryAsync

Aggregates user statistics in a single call.

- **Returns:** A tuple containing:
  - `TotalUsers`: total number of user records.
  - `ActiveUsers`: count of users not marked as deactivated.
  - `VerifiedEmails`: count of users with verified email status.
- **Throws:** `InvalidOperationException` on database errors.

## Usage

### Example 1: Create, authenticate, and retrieve activity summary

```csharp
var service = new UserService(dbContext);

// Create a new user
var newUser = new User
{
    Email = "alice@example.com",
    Password = "StrongP@ssw0rd!",
    IsActive = true
};

User created = await service.CreateAsync(newUser);

// Authenticate
User? authenticated = await service.AuthenticateAsync(created.Email, "StrongP@ssw0rd!");
if (authenticated is not null)
{
    Console.WriteLine($"Authenticated as {authenticated.Email}");
}

// Get summary
var (total, active, verified) = await service.GetActivitySummaryAsync();
Console.WriteLine($"Total: {total}, Active: {active}, Verified: {verified}");
```

### Example 2: Validate, deactivate, and verify email

```csharp
var service = new UserService(dbContext);

var user = await service.GetAsync(42);
if (user is null) return;

// Validate before changes
if (!service.Validate(user))
{
    Console.WriteLine("User state is invalid");
    return;
}

// Deactivate
bool deactivated = await service.DeactivateUserAsync(user.Id);
Console.WriteLine(deactivated ? "User deactivated" : "Deactivation failed");

// Verify email with a token
bool verified = await service.VerifyEmailAsync(user.Id, "abc-123-token");
Console.WriteLine(verified ? "Email verified" : "Verification failed");
```

## Notes

- All `async` methods that return `Task<bool>` use the boolean result to distinguish between “operation performed” and “target not found / precondition not met.” They do not throw for missing entities.
- `Validate` is a synchronous, side-effect-free method. It does not guarantee that a subsequent `CreateAsync` or `UpdateAsync` will succeed, as race conditions with other callers may change the data store state between validation and persistence.
- `AuthenticateAsync` returns `null` for both invalid credentials and deactivated accounts. Callers that need to differentiate should first check existence or active status separately.
- `GetActivitySummaryAsync` computes counts at a single point in time. In high-concurrency scenarios, the three values may reflect slightly inconsistent snapshots unless the underlying query executes under a transaction or snapshot isolation.
- Methods accepting identifiers assume the identifier is immutable once assigned by `CreateAsync`. Passing an identifier that was never returned by the service yields `false` or `null` rather than throwing.
- Thread safety is not guaranteed at the service level. Instances should be scoped per unit-of-work (e.g., per request in web applications) to avoid shared state corruption across concurrent operations.
