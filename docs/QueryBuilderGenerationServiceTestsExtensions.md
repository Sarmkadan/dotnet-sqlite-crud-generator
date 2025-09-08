# QueryBuilderGenerationServiceTestsExtensions

Utility extension methods designed to streamline the setup and teardown of integration or unit tests that involve `QueryBuilderGenerationService`. This static class provides factory methods for creating fresh service instances, resolving output paths, and verifying cleanup state, reducing boilerplate in test fixtures.

## API

### CreateFresh

```csharp
public static QueryBuilderGenerationServiceTests CreateFresh
```

Provides a factory property that returns a new, fully initialized instance of `QueryBuilderGenerationServiceTests`. Each access constructs a distinct test harness with isolated configuration, ensuring no shared state between test cases.

**Returns:** A ready-to-use `QueryBuilderGenerationServiceTests` object.

**Throws:** May throw `InvalidOperationException` if the underlying test environment cannot be initialized (e.g., missing dependencies or invalid default configuration).

### GetOutputPath

```csharp
public static string GetOutputPath
```

Resolves the absolute file system path where generated query builder artifacts will be written during a test run. The path is derived from the current test context and project structure.

**Returns:** A non-null, non-empty absolute directory path as a `string`.

**Throws:** May throw `DirectoryNotFoundException` if the expected base output directory does not exist or cannot be resolved.

### OutputDirectoryWasCleanedUp

```csharp
public static bool OutputDirectoryWasCleanedUp
```

Indicates whether the output directory was successfully purged of previously generated files before the current test execution. This property allows tests to assert a clean starting state.

**Returns:** `true` if the output directory existed and was emptied, or did not exist and was created empty; `false` if cleanup failed or was skipped.

**Throws:** Does not throw; returns `false` on any cleanup failure rather than propagating exceptions.

### GetService

```csharp
public static QueryBuilderGenerationService GetService
```

Provides a fully configured `QueryBuilderGenerationService` instance suitable for test execution. The returned service is wired with test-specific options and dependencies, isolated from production services.

**Returns:** A non-null `QueryBuilderGenerationService` ready for immediate use in test scenarios.

**Throws:** May throw `InvalidOperationException` if service construction fails due to missing configuration or dependency resolution errors.

## Usage

### Example 1: Basic Test with Fresh Setup and Cleanup Verification

```csharp
[Fact]
public void GenerateQueryBuilder_WithValidSchema_ProducesExpectedOutput()
{
    // Arrange
    var tests = QueryBuilderGenerationServiceTestsExtensions.CreateFresh;
    Assert.True(QueryBuilderGenerationServiceTestsExtensions.OutputDirectoryWasCleanedUp);

    var outputPath = QueryBuilderGenerationServiceTestsExtensions.GetOutputPath;
    var service = QueryBuilderGenerationServiceTestsExtensions.GetService;

    // Act
    service.GenerateQueryBuilder("User", outputPath);

    // Assert
    var generatedFile = Path.Combine(outputPath, "UserQueryBuilder.cs");
    Assert.True(File.Exists(generatedFile));
}
```

### Example 2: Multiple Test Cases with Isolated Service Instances

```csharp
[Theory]
[InlineData("User")]
[InlineData("Product")]
[InlineData("Order")]
public void GenerateQueryBuilder_ForMultipleEntities_CreatesCorrectFiles(string entityName)
{
    // Each theory case gets a completely fresh test harness
    var tests = QueryBuilderGenerationServiceTestsExtensions.CreateFresh;
    var service = QueryBuilderGenerationServiceTestsExtensions.GetService;
    var outputPath = QueryBuilderGenerationServiceTestsExtensions.GetOutputPath;

    service.GenerateQueryBuilder(entityName, outputPath);

    var expectedFile = Path.Combine(outputPath, $"{entityName}QueryBuilder.cs");
    Assert.True(File.Exists(expectedFile));
    Assert.NotEmpty(File.ReadAllText(expectedFile));
}
```

## Notes

- **Isolation:** Each access to `CreateFresh` and `GetService` constructs independent instances. Tests running in parallel do not share output directories or service state, provided each test accesses these members separately.
- **Cleanup Semantics:** `OutputDirectoryWasCleanedUp` reflects the state immediately after `CreateFresh` initializes the test harness. If a test writes files and then re-checks this property without re-creating the harness, it will still return the original cleanup result—it does not track subsequent modifications.
- **Thread Safety:** All members are static and return new instances or immutable values on each access. There is no mutable shared state, making them safe for concurrent test execution without external synchronization.
- **Output Path Stability:** `GetOutputPath` returns a path scoped to the current test run. Consecutive calls within the same test context return the same path; across different `CreateFresh` instances, paths may differ to maintain isolation.
- **Failure Modes:** When cleanup fails, `OutputDirectoryWasCleanedUp` returns `false` rather than throwing. Tests should assert this property when a clean starting state is critical to test validity.
