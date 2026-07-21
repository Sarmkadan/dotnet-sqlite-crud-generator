#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;
using DotNet.SQLite.CrudGenerator.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="NamingConventionHelper"/> class,
/// verifying naming convention conversions and helper methods.
/// </summary>
public sealed class NamingConventionHelperTests
{
    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.ToCSharpToSqlConvention"/> correctly converts C# property names to SQL snake_case.
    /// </summary>
    [Theory]
    [InlineData("UserId", "user_id")]
    [InlineData("FirstName", "first_name")]
    [InlineData("StockQuantity", "stock_quantity")]
    [InlineData("user_id", "user_id")]
    [InlineData("first_name", "first_name")]
    public void ToCSharpToSqlConvention_WithPascalCaseInput_ConvertsToSnakeCase(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.ToCSharpToSqlConvention(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.ToCSharpToSqlConvention"/> handles null or empty strings.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToCSharpToSqlConvention_WithNullOrEmpty_ReturnsOriginal(string? input)
    {
        // Act
        var result = NamingConventionHelper.ToCSharpToSqlConvention(input);

        // Assert
        result.Should().Be(input);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.ToSqlToCSharpConvention"/> correctly converts SQL column names to C# PascalCase.
    /// </summary>
    [Theory]
    [InlineData("user_id", "UserId")]
    [InlineData("first_name", "FirstName")]
    [InlineData("stock_quantity", "StockQuantity")]
    public void ToSqlToCSharpConvention_WithSnakeCaseInput_ConvertsToPascalCase(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.ToSqlToCSharpConvention(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.ToSqlToCSharpConvention"/> handles null or empty strings.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToSqlToCSharpConvention_WithNullOrEmpty_ReturnsOriginal(string? input)
    {
        // Act
        var result = NamingConventionHelper.ToSqlToCSharpConvention(input);

        // Assert
        result.Should().Be(input);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetTableName"/> correctly pluralizes and converts type names to snake_case.
    /// </summary>
    [Theory]
    [InlineData(typeof(Product), "products")]
    [InlineData(typeof(UserProfile), "user_profiles")]
    [InlineData(typeof(Category), "categories")]
    [InlineData(typeof(OrderItem), "order_items")]
    public void GetTableName_WithPluralizeTrue_ConvertsTypeNameToPluralSnakeCase(Type type, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetTableName(type);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetTableName"/> with pluralize=false returns singular form.
    /// </summary>
    [Theory]
    [InlineData(typeof(Product), "product")]
    [InlineData(typeof(UserProfile), "user_profile")]
    [InlineData(typeof(Category), "category")]
    [InlineData(typeof(OrderItem), "order_item")]
    public void GetTableName_WithPluralizeFalse_ReturnsSingularSnakeCase(Type type, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetTableName(type, pluralize: false);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetColumnName"/> correctly converts property names to snake_case.
    /// </summary>
    [Fact]
    public void GetColumnName_WithRegularProperty_ConvertsToSnakeCase()
    {
        // Arrange
        var property = typeof(Product).GetProperty(nameof(Product.ProductId))!;

        // Act
        var result = NamingConventionHelper.GetColumnName(property);

        // Assert
        result.Should().Be("product_id");
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetColumnName"/> respects explicit Column attribute.
    /// </summary>
    [Fact]
    public void GetColumnName_WithColumnAttribute_UsesAttributeName()
    {
        // Arrange
        var property = typeof(Product).GetProperty(nameof(Product.CustomColumn))!;

        // Act
        var result = NamingConventionHelper.GetColumnName(property);

        // Assert
        result.Should().Be("custom_db_column");
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetColumnName"/> caches results.
    /// </summary>
    [Fact]
    public void GetColumnName_WithSameProperty_ReturnsCachedResult()
    {
        // Arrange
        var property = typeof(Product).GetProperty(nameof(Product.ProductId))!;

        // Act - call twice
        var result1 = NamingConventionHelper.GetColumnName(property);
        var result2 = NamingConventionHelper.GetColumnName(property);

        // Assert - both should be the same
        result1.Should().Be("product_id");
        result2.Should().Be("product_id");
        result1.Should().BeSameAs(result2); // Verify caching
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetGrpcServiceName"/> correctly handles service names.
    /// </summary>
    [Theory]
    [InlineData("ProductService", "ProductService")]
    [InlineData("UserService", "UserService")]
    [InlineData("OrderService", "OrderService")]
    public void GetGrpcServiceName_WithServiceSuffix_ReturnsAsIs(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetGrpcServiceName(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetGrpcServiceName"/> appends Service suffix when not present.
    /// </summary>
    [Theory]
    [InlineData("Product", "ProductService")]
    [InlineData("User", "UserService")]
    [InlineData("Order", "OrderService")]
    public void GetGrpcServiceName_WithoutServiceSuffix_AppendsService(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetGrpcServiceName(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetGrpcMessageName"/> correctly handles message names.
    /// </summary>
    [Theory]
    [InlineData("ProductMessage", "ProductMessage")]
    [InlineData("UserMessage", "UserMessage")]
    public void GetGrpcMessageName_WithMessageSuffix_ReturnsAsIs(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetGrpcMessageName(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetGrpcMessageName"/> appends Message suffix when not present.
    /// </summary>
    [Theory]
    [InlineData("Product", "ProductMessage")]
    [InlineData("User", "UserMessage")]
    public void GetGrpcMessageName_WithoutMessageSuffix_AppendsMessage(string input, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetGrpcMessageName(input);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetApiEndpoint"/> correctly generates API endpoints.
    /// </summary>
    [Theory]
    [InlineData(typeof(Product), "/api/v1/products")]
    [InlineData(typeof(UserProfile), "/api/v1/userprofiles")]
    [InlineData(typeof(Category), "/api/v1/categories")]
    public void GetApiEndpoint_WithDefaultApiVersion_ReturnsCorrectEndpoint(Type type, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetApiEndpoint(type);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetApiEndpoint"/> respects custom API version.
    /// </summary>
    [Theory]
    [InlineData(typeof(Product), "v2", "/api/v2/products")]
    [InlineData(typeof(UserProfile), "v3", "/api/v3/userprofiles")]
    public void GetApiEndpoint_WithCustomApiVersion_ReturnsCorrectEndpoint(Type type, string apiVersion, string expected)
    {
        // Act
        var result = NamingConventionHelper.GetApiEndpoint(type, apiVersion);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.IsValidPropertyName"/> accepts valid property names.
    /// </summary>
    [Theory]
    [InlineData("Name")]
    [InlineData("userId")]
    [InlineData("_privateField")]
    [InlineData("Property123")]
    [InlineData("_")]
    public void IsValidPropertyName_WithValidNames_ReturnsTrue(string propertyName)
    {
        // Act
        var result = NamingConventionHelper.IsValidPropertyName(propertyName);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.IsValidPropertyName"/> rejects invalid property names.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123Invalid")]
    [InlineData("-invalid")]
    [InlineData("invalid-name")]
    [InlineData("invalid name")]
    public void IsValidPropertyName_WithInvalidNames_ReturnsFalse(string? propertyName)
    {
        // Act
        var result = NamingConventionHelper.IsValidPropertyName(propertyName);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetConventionInfo"/> returns correct naming convention information.
    /// </summary>
    [Fact]
    public void GetConventionInfo_WithType_ReturnsCorrectInfo()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var result = NamingConventionHelper.GetConventionInfo(type);

        // Assert
        result.Should().NotBeNull();
        result.EntityName.Should().Be("Product");
        result.TableName.Should().Be("products");
        result.ApiEndpoint.Should().Be("/api/v1/products");
        result.GrpcServiceName.Should().Be("ProductService");
        result.Properties.Should().HaveCount(4);

        // Verify property conversions
        var productIdInfo = result.Properties.First(p => p.PropertyName == "ProductId");
        productIdInfo.ColumnName.Should().Be("product_id");
        productIdInfo.Type.Should().Be("Int32");

        var customColumnInfo = result.Properties.First(p => p.PropertyName == "CustomColumn");
        customColumnInfo.ColumnName.Should().Be("custom_db_column");
    }

    /// <summary>
    /// Test entity classes for testing GetTableName and GetApiEndpoint
    /// </summary>
    private class Product
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        [Column(Name = "custom_db_column")]
        public string? CustomColumn { get; set; }
    }

    private class UserProfile
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
    }

    private class Category
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
    }

    private class OrderItem
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
    }
}
