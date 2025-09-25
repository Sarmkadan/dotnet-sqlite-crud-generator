#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Tests for the <see cref="NamingConventionHelper"/> class, ensuring that
/// SQL type mappings are returned correctly for supported, nullable, and
/// unsupported .NET types.
/// </summary>
public sealed class NamingConventionHelperTests
{
    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetSqlType(Type)"/> returns the
    /// expected SQLite type string for a set of supported .NET types.
    /// </summary>
    /// <param name="type">The .NET type to map.</param>
    /// <param name="expectedSqlType">The expected SQLite type string.</param>
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

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetSqlType(Type)"/> correctly
    /// handles nullable value types by returning the underlying SQLite type.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="NamingConventionHelper.GetSqlType(Type)"/> returns the
    /// default SQLite type ("TEXT") when the provided .NET type is unsupported.
    /// </summary>
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
