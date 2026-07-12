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
/// Contains unit tests for the <see cref="StringExtensions"/> utility class, 
/// verifying various string manipulation and formatting operations.
/// </summary>
public sealed class StringExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToPascalCase"/> correctly converts underscore-separated words to PascalCase.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToCamelCase"/> correctly converts underscore-separated words to camelCase.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="StringExtensions.Pluralize"/> correctly pluralizes words.
    /// </summary>
    /// <param name="word">The singular word to pluralize.</param>
    /// <param name="expected">The expected plural form.</param>
    [Theory]
    [InlineData("category", "categories")]
    [InlineData("class", "classes")]
    [InlineData("product", "products")]
    public void Pluralize_WithVariousWordForms_ReturnsCorrectPlural(string word, string expected)
    {
        word.Pluralize().Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="StringExtensions.Truncate"/> correctly truncates a string and appends an ellipsis 
    /// when it exceeds the specified maximum length.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToSlug"/> correctly converts strings to lowercase, hyphenated slugs.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToSnakeCase"/> correctly converts PascalCase strings to snake_case.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToPascalCase"/> handles null or empty strings by returning the original input.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToPascalCase_WithNullOrEmpty_ReturnsOriginal(string? input)
    {
        input.ToPascalCase().Should().Be(input);
    }

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToSnakeCase"/> handles null or empty strings by returning the original input.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToSnakeCase_WithNullOrEmpty_ReturnsOriginal(string? input)
    {
        input.ToSnakeCase().Should().Be(input);
    }

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToCamelCase"/> correctly converts a single-word string to camelCase.
    /// </summary>
    [Fact]
    public void ToCamelCase_WithSingleWord_ReturnsLowercase()
    {
        // Arrange
        const string input = "User";

        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be("user");
    }
}
