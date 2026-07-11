# UserServiceExtensions

The `UserServiceExtensions` static class provides a suite of extension methods designed to simplify user-related data operations within the `dotnet-sqlite-crud-generator` framework. By abstracting complex SQLite queries into strongly-typed asynchronous operations, these methods facilitate efficient retrieval, validation, and aggregation of user data, ensuring consistent data access patterns throughout the application.

## API

### GetByEmailAsync
Retrieves a `User` entity matching the specified email address.
*   **Parameters:** `string email` - The email address to search for.
*   **Returns:** A `Task<User?>` containing the matching `User` object, or `null` if no record is found.
*   **Throws:** `ArgumentNullException` if `email` is null or empty.

### GetByUsernameAsync
Retrieves a `User` entity matching the specified username.
*   **Parameters:** `string username` - The username to search for.
*   **Returns:** A `Task<User?>` containing the matching `User` object, or `null` if no record is found.
*   **Throws:** `ArgumentNullException` if `username` is null or empty.

### ExistsByEmailAsync
Determines whether a user record with the provided email exists in the database.
*   **Parameters:** `string email` - The email address to verify.
*   **Returns:** A `Task<bool>` that resolves to `true` if the email exists, otherwise `false`.
*   **Throws:** `ArgumentNullException` if `email` is null or empty.

### GetActiveUsersAsync
Retrieves all users currently marked with an active status.
*   **Returns:** A `Task<IEnumerable<User>>` containing the collection of active users.

### GetVerifiedUsersAsync
Retrieves all users who have successfully completed the verification process.
*   **Returns:** A `Task<IEnumerable<User>>` containing the collection of verified users.

### GetUsersByCreationDateAsync
Retrieves all users created on the specified date.
*   **Parameters:** `DateTime date` - The date to filter users by.
*   **Returns:** A `Task<IEnumerable<User>>` containing the collection of users created on that date.

### CountActiveUsersAsync
Calculates the total number of users currently marked as active.
*   **Returns:** A `Task<int>` representing the count of active users.

### CountVerifiedUsersAsync
Calculates the total number of users who have completed verification.
*   **Returns:** A `Task<int>` representing the count of verified users.

## Usage

### Example 1: Validating user existence and retrieval by email
```csharp
public async Task<User?> GetUserDetailsAsync(string email)
{
    if (await _userService.ExistsByEmailAsync(email))
    {
        return await _userService.GetByEmailAsync(email);
    }
    return null;
}
```

### Example 2: Aggregating user metrics
```csharp
public async Task DisplayUserMetricsAsync()
{
    int activeCount = await _userService.CountActiveUsersAsync();
    int verifiedCount = await _userService.CountVerifiedUsersAsync();
    
    Console.WriteLine($"System Stats: {activeCount} active users, {verifiedCount} verified users.");
}
```

## Notes

*   **Thread Safety:** As these methods are implemented as asynchronous operations utilizing the `Task` pattern, they are thread-safe regarding their own execution context. However, they rely on the underlying database connection provider; ensure that your database context or repository implementation is configured for concurrent access.
*   **Database Exceptions:** While these methods handle standard query execution, they may propagate exceptions related to database connectivity, schema mismatches, or locked database files typical of SQLite environments. Implement appropriate `try-catch` blocks at the caller level to handle `SqliteException` or similar infrastructure-level errors.
*   **Data Consistency:** These methods perform read-only operations. In scenarios where data integrity is critical, ensure the database is not being modified by concurrent write transactions that could lead to non-repeatable reads.
