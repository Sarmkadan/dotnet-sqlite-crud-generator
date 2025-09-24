#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using NSubstitute;
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Services;
using DotNet.SQLite.CrudGenerator.Data;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Exceptions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Tests for the GenerationService class.
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
}
