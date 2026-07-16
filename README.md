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
// ... rest of file content ...
