#nullable enable
using FluentAssertions;
using DotNet.SQLite.CrudGenerator.Utilities;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Edge-case tests for NamingConventionHelper - null/empty inputs, type mapping boundaries,
/// property name validation, and naming convention conversions.
/// </summary>
public sealed class NamingConventionHelperEdgeCaseTests
{
    [Theory]
    [InlineData(typeof(int), "INTEGER")]
    [InlineData(typeof(long), "INTEGER")]
    [InlineData(typeof(bool), "INTEGER")]
    [InlineData(typeof(float), "REAL")]
    [InlineData(typeof(double), "REAL")]
    [InlineData(typeof(decimal), "REAL")]
    [InlineData(typeof(string), "TEXT")]
    [InlineData(typeof(DateTime), "TEXT")]
    [InlineData(typeof(Guid), "TEXT")]
    [InlineData(typeof(byte[]), "BLOB")]
    public void GetSqlType_KnownTypes_ReturnsCorrectMapping(Type type, string expected)
    {
        NamingConventionHelper.GetSqlType(type).Should().Be(expected);
    }

    [Fact]
    public void GetSqlType_NullableInt_ReturnsInteger()
    {
        NamingConventionHelper.GetSqlType(typeof(int?)).Should().Be("INTEGER");
    }

    [Fact]
    public void GetSqlType_NullableDateTime_ReturnsText()
    {
        NamingConventionHelper.GetSqlType(typeof(DateTime?)).Should().Be("TEXT");
    }

    [Fact]
    public void GetSqlType_UnknownType_ReturnsText()
    {
        NamingConventionHelper.GetSqlType(typeof(object)).Should().Be("TEXT");
    }

    [Fact]
    public void ToCSharpToSqlConvention_NullInput_ReturnsNull()
    {
        NamingConventionHelper.ToCSharpToSqlConvention(null!).Should().BeNull();
    }

    [Fact]
    public void ToCSharpToSqlConvention_EmptyInput_ReturnsEmpty()
    {
        NamingConventionHelper.ToCSharpToSqlConvention("").Should().BeEmpty();
    }

    [Fact]
    public void ToSqlToCSharpConvention_NullInput_ReturnsNull()
    {
        NamingConventionHelper.ToSqlToCSharpConvention(null!).Should().BeNull();
    }

    [Fact]
    public void ToSqlToCSharpConvention_EmptyInput_ReturnsEmpty()
    {
        NamingConventionHelper.ToSqlToCSharpConvention("").Should().BeEmpty();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("1Name", false)]
    [InlineData("name with spaces", false)]
    [InlineData("name-with-dashes", false)]
    [InlineData("ValidName", true)]
    [InlineData("_privateName", true)]
    [InlineData("name123", true)]
    [InlineData("A", true)]
    public void IsValidPropertyName_VariousInputs_ReturnsExpected(string name, bool expected)
    {
        NamingConventionHelper.IsValidPropertyName(name).Should().Be(expected);
    }

    [Fact]
    public void IsValidPropertyName_NullInput_ReturnsFalse()
    {
        NamingConventionHelper.IsValidPropertyName(null!).Should().BeFalse();
    }

    [Fact]
    public void GetGrpcServiceName_AlreadyEndingWithService_ReturnsUnchanged()
    {
        NamingConventionHelper.GetGrpcServiceName("ProductService").Should().Be("ProductService");
    }

    [Fact]
    public void GetGrpcServiceName_WithoutServiceSuffix_AppendsSuffix()
    {
        NamingConventionHelper.GetGrpcServiceName("Product").Should().Be("ProductService");
    }

    [Fact]
    public void GetGrpcMessageName_AlreadyEndingWithMessage_ReturnsUnchanged()
    {
        NamingConventionHelper.GetGrpcMessageName("ProductMessage").Should().Be("ProductMessage");
    }

    [Fact]
    public void GetGrpcMessageName_WithoutMessageSuffix_AppendsSuffix()
    {
        NamingConventionHelper.GetGrpcMessageName("Product").Should().Be("ProductMessage");
    }

    [Fact]
    public void GetApiEndpoint_DefaultVersion_ReturnsV1()
    {
        var endpoint = NamingConventionHelper.GetApiEndpoint(typeof(TestEntity));
        endpoint.Should().StartWith("/api/v1/");
    }

    [Fact]
    public void GetApiEndpoint_CustomVersion_UsesProvidedVersion()
    {
        var endpoint = NamingConventionHelper.GetApiEndpoint(typeof(TestEntity), "v2");
        endpoint.Should().StartWith("/api/v2/");
    }

    [Fact]
    public void GetTableName_Pluralized_ReturnsSnakeCasePlural()
    {
        var tableName = NamingConventionHelper.GetTableName(typeof(TestEntity));
        tableName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetTableName_NotPluralized_ReturnsSnakeCaseSingular()
    {
        var tableName = NamingConventionHelper.GetTableName(typeof(TestEntity), pluralize: false);
        tableName.Should().Contain("test_entity");
    }

    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
