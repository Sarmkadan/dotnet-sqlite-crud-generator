#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class NamingConventionHelperTests
{
    [Theory]
    [InlineData(typeof(int), "INTEGER")]
    [InlineData(typeof(string), "TEXT")]
    [InlineData(typeof(double), "REAL")]
    [InlineData(typeof(byte[]), "BLOB")]
    public void GetSqlType_WithSupportedTypes_ReturnsCorrectSqlType(Type type, string expectedSqlType)
    {
        // Act
        var result = NamingConventionHelper.GetSqlType(type);

        // Assert
        result.Should().Be(expectedSqlType);
    }

    [Fact]
    public void GetSqlType_WithNullableType_ReturnsCorrectSqlType()
    {
        // Arrange
        var type = typeof(int?);

        // Act
        var result = NamingConventionHelper.GetSqlType(type);

        // Assert
        result.Should().Be("INTEGER");
    }

    [Fact]
    public void GetSqlType_WithUnsupportedType_ReturnsDefaultText()
    {
        // Arrange
        var type = typeof(object);

        // Act
        var result = NamingConventionHelper.GetSqlType(type);

        // Assert
        result.Should().Be("TEXT");
    }
}
