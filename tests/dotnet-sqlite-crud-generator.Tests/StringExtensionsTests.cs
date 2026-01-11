// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void ToPascalCase_WithUnderscoreSeparatedWords_CapitalizesEachWordSegment()
    {
        // Arrange
        const string input = "user_profile_id";

        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be("UserProfileId");
    }

    [Fact]
    public void ToCamelCase_WithUnderscoreSeparatedWords_MakesFirstSegmentLowercase()
    {
        // Arrange
        const string input = "first_name";

        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be("firstName");
    }

    [Theory]
    [InlineData("category", "categories")]
    [InlineData("class", "classes")]
    [InlineData("product", "products")]
    public void Pluralize_WithVariousWordForms_ReturnsCorrectPlural(string word, string expected)
    {
        word.Pluralize().Should().Be(expected);
    }

    [Fact]
    public void Truncate_WhenStringExceedsMaxLengthWithEllipsis_TruncatesAndAppendsEllipsis()
    {
        // Arrange
        const string input = "This is a long product description";

        // Act
        var result = input.Truncate(20, addEllipsis: true);

        // Assert
        result.Should().Be("This is a long produ...");
    }

    [Fact]
    public void ToSlug_WithMixedCaseAndSpaces_ReturnsLowercaseHyphenatedSlug()
    {
        // Arrange
        const string input = "My Product Name 2024";

        // Act
        var result = input.ToSlug();

        // Assert
        result.Should().Be("my-product-name-2024");
    }

    [Fact]
    public void ToSnakeCase_WithPascalCaseInput_InsertsUnderscoresAtWordBoundaries()
    {
        // Arrange
        const string input = "StockQuantity";

        // Act
        var result = input.ToSnakeCase();

        // Assert
        result.Should().Be("stock_quantity");
    }
}
