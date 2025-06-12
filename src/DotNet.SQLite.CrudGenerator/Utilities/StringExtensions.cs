// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for string manipulation including case conversions, validation, and transformations.
/// Used throughout the codebase for naming conventions and data formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to PascalCase (e.g., "user_id" -> "UserId").
    /// Useful for C# property naming conventions.
    /// </summary>
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
                sb.Append(char.ToUpper(word[0]) + word.Substring(1).ToLower());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to camelCase (e.g., "UserId" -> "userId").
    /// Useful for JSON and JavaScript naming conventions.
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var pascalCase = input.ToPascalCase();
        return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "UserId" -> "user_id").
    /// Useful for database column naming conventions.
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = Regex.Replace(input, "([a-z])([A-Z])", "$1_$2");
        return result.ToLower();
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "UserId" -> "user-id").
    /// Useful for URL and configuration naming.
    /// </summary>
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.ToSnakeCase().Replace("_", "-");
    }

    /// <summary>
    /// Pluralizes a word (basic implementation).
    /// Note: This is a simplified version; use external library for production use.
    /// </summary>
    public static string Pluralize(this string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        return word.EndsWith("y")
            ? word.Substring(0, word.Length - 1) + "ies"
            : word.EndsWith("s") || word.EndsWith("x") || word.EndsWith("z") ? word + "es" : word + "s";
    }

    /// <summary>
    /// Truncates a string to a maximum length, optionally adding ellipsis.
    /// </summary>
    public static string Truncate(this string input, int maxLength, bool addEllipsis = false)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        var truncated = input.Substring(0, maxLength);
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>
    /// Checks if a string is null, empty, or consists only of whitespace.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    /// <summary>
    /// Removes all whitespace from a string.
    /// </summary>
    public static string RemoveWhitespace(this string input)
    {
        return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    /// <summary>
    /// Converts a string to a slug suitable for URLs.
    /// </summary>
    public static string ToSlug(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var slug = input
            .ToLower()
            .Replace(" ", "-")
            .Replace("_", "-")
            .Trim('-');

        return Regex.Replace(slug, @"[^a-z0-9\-]", "")
            .Replace("--", "-");
    }

    /// <summary>
    /// Repeats a string the specified number of times.
    /// </summary>
    public static string Repeat(this string input, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
            sb.Append(input);

        return sb.ToString();
    }

    /// <summary>
    /// Checks if a string matches a pattern (basic regex).
    /// </summary>
    public static bool MatchesPattern(this string input, string pattern)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern))
            return false;

        try
        {
            return Regex.IsMatch(input, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the first word of a string.
    /// </summary>
    public static string FirstWord(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var words = input.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 0 ? words[0] : string.Empty;
    }
}
