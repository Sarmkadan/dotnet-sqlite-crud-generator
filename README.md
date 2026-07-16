// existing content ...

## MigrationDiffServiceTests

`MigrationDiffServiceTests` is a test class that contains unit tests for `MigrationDiffService`. It provides various test methods to verify the correctness of database schema generation, diff computation, and actual schema retrieval against an in-memory SQLite database. 

Here's an example of using `MigrationDiffServiceTests` in a test class:

```csharp
using DotNet.SQLite.CrudGenerator.Tests;

public class MigrationDiffServiceTestsExample : IDisposable
{
    private readonly MigrationDiffServiceTests _test;

    public MigrationDiffServiceTestsExample()
    {
        _test = new MigrationDiffServiceTests();
    }

    public void Dispose()
    {
        _test.Dispose();
    }

    [Fact]
    public void TestGetExpectedSchema_ReturnsCorrectColumnTypes()
    {
        // Arrange
        // Act
        var schema = _test.GetExpectedSchema(typeof(_test.SimpleEntity));

        // Assert
        // assertions...
    }
}
```

## UserServiceTests

`UserServiceTests` is a test class that verifies the behavior of **UserService** by using a mocked `IRepository<User, int>`.  
It contains async test methods that cover the most common service operations: retrieving a user by id (both existing and non‑existent), fetching all users, creating a new user, updating an existing user, and deleting a user.

```csharp
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Interfaces;
using NSubstitute;

public class UserServiceTestsExample
{
    public static async Task Main()
    {
        var tests = new UserServiceTests();

        await tests.GetAsync_WithValidId_ReturnsUserFromRepository();
        await tests.GetAsync_WithNonExistentId_ReturnsNull();
        await tests.GetAllAsync_ReturnsAllUsersFromRepository();
        await tests.CreateAsync_AddsUserThroughRepository();
        await tests.UpdateAsync_UpdatesUserThroughRepository();
        await tests.DeleteAsync_DeletesUserThroughRepository();
    }
}
```

## GenerationServiceTests

`GenerationServiceTests` is a test class that contains unit tests for the `GenerationService`.  
It demonstrates how to instantiate the test class, run a test method, and clean up resources. The example below shows a simple usage pattern that compiles and runs the test methods.

```csharp
using DotNet.SQLite.CrudGenerator.Tests;
using System.Threading.Tasks;

public class GenerationServiceTestsExample
{
    public static async Task Main()
    {
        var tests = new GenerationServiceTests();
        await tests.GenerateRepositoryInterfaceAsync_GeneratesCorrectInterfaceFile();
        await tests.GenerateMigrationAsync_GeneratesCorrectSqlMigrationFile();
        await tests.GenerateGrpcServiceAsync_GeneratesCorrectProtoFile();
        tests.Dispose();
    }
}
```
