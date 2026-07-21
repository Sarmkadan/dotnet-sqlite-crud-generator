#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Contains unit tests for <see cref="GenerationService"/>.
/// Tests generation of repository interfaces, SQL migrations, and gRPC services.
/// </summary>
public sealed class GenerationServiceTests : IDisposable
{
    private readonly GenerationService _sut; // System Under Test
    private readonly string _testOutputPath;

    /// <summary>
    /// A simple model for testing purposes, representing a product.
    /// </summary>
    public sealed class TestProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// A simple model for testing purposes, representing a user.
    /// </summary>
    public sealed class TestUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// An invalid model for testing purposes, missing an 'Id' property.
    /// </summary>
    public sealed class InvalidModelMissingId
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// An invalid model for testing purposes, with too few properties.
    /// </summary>
    public sealed class InvalidModelTooFewProperties
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// A model with nullable properties for testing type mapping.
    /// </summary>
    public sealed class TestProductWithNullable
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the GenerationServiceTests class.
    /// </summary>
    public GenerationServiceTests()
    {
        _testOutputPath = Path.Combine(Path.GetTempPath(), "GeneratedTests", Guid.NewGuid().ToString());
        _sut = new GenerationService(_testOutputPath);
    }

    /// <summary>
    /// Disposes of the test resources.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testOutputPath))
        {
            Directory.Delete(_testOutputPath, true);
        }
    }

    /// <summary>
    /// Tests the GenerateRepositoryInterfaceAsync method, verifying it generates the correct interface file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateRepositoryInterfaceAsync_GeneratesCorrectInterfaceFile()
    {
        // Act
        var filePath = await _sut.GenerateRepositoryInterfaceAsync(typeof(TestProduct));

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("namespace DotNet.SQLite.CrudGenerator.Repositories;");
        content.Should().Contain("public interface ITestProductRepository");
        content.Should().Contain("Task<TestProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default);");
        content.Should().Contain("Task<IEnumerable<TestProduct>> GetAllAsync(CancellationToken cancellationToken = default);");
        content.Should().Contain("Task<TestProduct> AddAsync(TestProduct entity, CancellationToken cancellationToken = default);");
        content.Should().Contain("Task<TestProduct> UpdateAsync(TestProduct entity, CancellationToken cancellationToken = default);");
        content.Should().Contain("Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);");
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method, verifying it generates the correct SQL migration file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_GeneratesCorrectSqlMigrationFile()
    {
        // Arrange
        var migrationName = "CreateTestProductTable";

        // Act
        var filePath = await _sut.GenerateMigrationAsync(typeof(TestProduct), migrationName);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain($"-- Migration: {migrationName}");
        content.Should().Contain("-- Entity: TestProduct");
        content.Should().Contain("CREATE TABLE IF NOT EXISTS TestProducts (");
        content.Should().Contain("Name TEXT");
        content.Should().Contain("Price REAL");
        content.Should().Contain("Id INTEGER PRIMARY KEY AUTOINCREMENT"); // Id should be included from GenerateColumnDefinitions
        // Add specific checks for column definitions
        content.Should().ContainEquivalentOf("Name TEXT");
        content.Should().ContainEquivalentOf("Price REAL");
    }

    /// <summary>
    /// Tests the GenerateGrpcServiceAsync method, verifying it generates the correct gRPC service file.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateGrpcServiceAsync_GeneratesCorrectProtoFile()
    {
        // Act
        var filePath = await _sut.GenerateGrpcServiceAsync(typeof(TestProduct));

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("syntax = \"proto3\";");
        content.Should().Contain("package generated.testproduct;");
        content.Should().Contain("message TestProductRequest {");
        content.Should().Contain("int32 id = 1;");
        content.Should().Contain("message TestProductResponse {");
        content.Should().Contain("string name = 2;"); // Order matters based on property order in TestProduct
        content.Should().Contain("double price = 3;"); // decimal maps to double
        content.Should().Contain("service TestProductService {");
        content.Should().Contain("rpc Get(TestProductRequest) returns (TestProductResponse);");
    }

    /// <summary>
    /// Tests the GenerateRepositoryInterfaceAsync method, verifying it throws an exception for an invalid model missing an 'Id' property.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateRepositoryInterfaceAsync_ThrowsExceptionForInvalidModelMissingId()
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateRepositoryInterfaceAsync(typeof(InvalidModelMissingId));

        // Assert
        await act.Should().ThrowAsync<GenerationException>()
            .WithMessage("Entity must have an 'Id' property. (Parameter 'InvalidModelMissingId')");
    }

    /// <summary>
    /// Tests the GenerateRepositoryInterfaceAsync method, verifying it throws an exception for an invalid model with too few properties.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateRepositoryInterfaceAsync_ThrowsExceptionForInvalidModelTooFewProperties()
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateRepositoryInterfaceAsync(typeof(InvalidModelTooFewProperties));

        // Assert
        await act.Should().ThrowAsync<GenerationException>()
            .WithMessage("Entity must have at least 2 properties. (Parameter 'InvalidModelTooFewProperties')");
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method, verifying it throws an exception for an invalid model missing an 'Id' property.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_ThrowsExceptionForInvalidModelMissingId()
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateMigrationAsync(typeof(InvalidModelMissingId), "Migration");

        // Assert
        await act.Should().ThrowAsync<GenerationException>()
            .WithMessage("Entity must have an 'Id' property. (Parameter 'InvalidModelMissingId')");
    }

    /// <summary>
    /// Tests the GenerateGrpcServiceAsync method, verifying it throws an exception for an invalid model with too few properties.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateGrpcServiceAsync_ThrowsExceptionForInvalidModelTooFewProperties()
    {
        // Act
        Func<Task> act = async () => await _sut.GenerateGrpcServiceAsync(typeof(InvalidModelTooFewProperties));

        // Assert
        await act.Should().ThrowAsync<GenerationException>()
            .WithMessage("Entity must have at least 2 properties. (Parameter 'InvalidModelTooFewProperties')");
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method, verifying the generated SQL contains the entity table name.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_ContainsEntityTableName()
    {
        // Arrange
        var migrationName = "TestMigration";

        // Act
        var filePath = await _sut.GenerateMigrationAsync(typeof(TestProduct), migrationName);
        var content = await File.ReadAllTextAsync(filePath);

        // Assert - The table name should be "TestProducts" (entity name + 's')
        content.Should().Contain("CREATE TABLE IF NOT EXISTS TestProducts (");
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method, verifying it throws ArgumentNullException for null entity type.
    /// </summary>
    [Fact]
    public async Task GenerateMigrationAsync_ThrowsArgumentNullExceptionForNullEntityType()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GenerateMigrationAsync(null!, "Migration"));
    }

    /// <summary>
    /// Tests the GenerateRepositoryInterfaceAsync method, verifying it throws ArgumentNullException for null entity type.
    /// </summary>
    [Fact]
    public async Task GenerateRepositoryInterfaceAsync_ThrowsArgumentNullExceptionForNullEntityType()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GenerateRepositoryInterfaceAsync(null!));
    }

    /// <summary>
    /// Tests the GenerateGrpcServiceAsync method, verifying it throws ArgumentNullException for null entity type.
    /// </summary>
    [Fact]
    public async Task GenerateGrpcServiceAsync_ThrowsArgumentNullExceptionForNullEntityType()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GenerateGrpcServiceAsync(null!));
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method with soft delete enabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_WithSoftDeleteEnabled()
    {
        // Arrange
        var softDeleteOptions = new SoftDeleteOptions
        {
            Enabled = true,
            ColumnName = "IsDeleted",
            ActiveValue = 0,
            DeletedValue = 1
        };
        var serviceWithSoftDelete = new GenerationService(_testOutputPath, softDeleteOptions);

        // Act
        var filePath = await serviceWithSoftDelete.GenerateMigrationAsync(typeof(TestProduct), "CreateTableWithSoftDelete");
        var content = await File.ReadAllTextAsync(filePath);

        // Assert
        content.Should().Contain("IsDeleted INTEGER NOT NULL DEFAULT 0");
        content.Should().Contain("CREATE TABLE IF NOT EXISTS TestProducts (");
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method with soft delete disabled.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_WithSoftDeleteDisabled()
    {
        // Arrange
        var softDeleteOptions = new SoftDeleteOptions { Enabled = false };
        var serviceWithoutSoftDelete = new GenerationService(_testOutputPath, softDeleteOptions);

        // Act
        var filePath = await serviceWithoutSoftDelete.GenerateMigrationAsync(typeof(TestProduct), "CreateTableNoSoftDelete");
        var content = await File.ReadAllTextAsync(filePath);

        // Assert
        content.Should().NotContain("IsDeleted");
        content.Should().Contain("CREATE TABLE IF NOT EXISTS TestProducts (");
    }

    /// <summary>
    /// Tests all three generation methods work together for a single entity.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task AllGenerationMethods_WorkTogether()
    {
        // Act
        var repoPath = await _sut.GenerateRepositoryInterfaceAsync(typeof(TestProduct));
        var migrationPath = await _sut.GenerateMigrationAsync(typeof(TestProduct), "CreateTestProductTable");
        var grpcPath = await _sut.GenerateGrpcServiceAsync(typeof(TestProduct));

        // Assert - All files should exist
        File.Exists(repoPath).Should().BeTrue();
        File.Exists(migrationPath).Should().BeTrue();
        File.Exists(grpcPath).Should().BeTrue();

        // Verify file names
        Path.GetFileName(repoPath).Should().Be("ITestProductRepository.cs");
        Path.GetFileName(grpcPath).Should().Be("TestProduct.proto");
    }

    /// <summary>
    /// Tests the GenerationService creates output directory if it doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerationService_CreatesOutputDirectory()
    {
        // Arrange - Use a new output path that doesn't exist
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "NonExistentDir", Guid.NewGuid().ToString());
        var service = new GenerationService(nonExistentPath);

        try
        {
            // Act
            var filePath = await service.GenerateMigrationAsync(typeof(TestProduct), "TestMigration");

            // Assert
            Directory.Exists(nonExistentPath).Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(nonExistentPath))
                Directory.Delete(nonExistentPath, true);
        }
    }

    /// <summary>
    /// Tests the GenerateMigrationAsync method generates correct column types for different property types.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateMigrationAsync_GeneratesCorrectColumnTypes()
    {
        // Arrange
        var migrationName = "TestColumnTypes";

        // Act
        var filePath = await _sut.GenerateMigrationAsync(typeof(TestProduct), migrationName);
        var content = await File.ReadAllTextAsync(filePath);

        // Assert
        content.Should().Contain("Id INTEGER PRIMARY KEY AUTOINCREMENT");
        content.Should().Contain("Name TEXT");
        content.Should().Contain("Price REAL");
    }

    /// <summary>
    /// Tests the GenerateGrpcServiceAsync method generates correct message types for nullable properties.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task GenerateGrpcServiceAsync_HandlesNullableProperties()
    {
        // Act
        var filePath = await _sut.GenerateGrpcServiceAsync(typeof(TestProductWithNullable));
        var content = await File.ReadAllTextAsync(filePath);

        // Assert - nullable string should be optional in proto
        content.Should().Contain("string description = 2;");
        content.Should().Contain("int32 quantity = 3;");
        content.Should().Contain("service TestProductWithNullableService {");
    }
}
