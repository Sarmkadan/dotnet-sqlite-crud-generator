#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for string manipulation including case conversions, validation, and transformations.
/// Used throughout the codebase for naming conventions and data formatting.
/// </summary>
public static class StringExtensions
{
    // Compiled at startup — avoids per-call regex construction overhead in hot paths.
    private static readonly Regex _slugSpecialCharsRegex =
        new(@"[^a-z0-9\-]", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex _slugDoubleHyphensRegex =
        new("-{2,}", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Converts a string to PascalCase (e.g., "user_id" -> "UserId").
    /// Useful for C# property naming conventions.
    /// </summary>
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var span = input.AsSpan();

        // First pass: count non-separator characters for exact allocation.
        int outputLen = 0;
        foreach (char c in span)
            if (c != '_' && c != '-' && c != ' ') outputLen++;

        if (outputLen == 0)
            return string.Empty;

        return string.Create(outputLen, input, static (chars, src) =>
        {
            int pos = 0;
            bool capitalizeNext = true;
            foreach (char c in src.AsSpan())
            {
                if (c == '_' || c == '-' || c == ' ')
                {
                    capitalizeNext = true;
                    continue;
                }
                chars[pos++] = capitalizeNext ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
                capitalizeNext = false;
            }
        });
    }

    /// <summary>
    /// Converts a string to camelCase (e.g., "UserId" -> "userId").
    /// Useful for JSON and JavaScript naming conventions.
    /// </summary>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var pascal = input.ToPascalCase();
        if (pascal.Length == 0) return string.Empty;
        if (pascal.Length == 1) return char.ToLowerInvariant(pascal[0]).ToString();

        // Lowercase only the first character, copy the rest via Span — one allocation.
        return string.Create(pascal.Length, pascal, static (chars, src) =>
        {
            chars[0] = char.ToLowerInvariant(src[0]);
            src.AsSpan(1).CopyTo(chars[1..]);
        });
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "UserId" -> "user_id").
    /// Useful for database column naming conventions.
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var span = input.AsSpan();

        // Count lowercase→uppercase transitions; each needs an inserted underscore.
        int extra = 0;
        for (int i = 1; i < span.Length; i++)
            if (char.IsLower(span[i - 1]) && char.IsUpper(span[i]))
                extra++;

        if (extra == 0)
            return input.ToLowerInvariant();

        // One allocation: exact-sized buffer with underscores inserted inline.
        return string.Create(span.Length + extra, input, static (buf, src) =>
        {
            int pos = 0;
            var s = src.AsSpan();
            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && char.IsLower(s[i - 1]) && char.IsUpper(s[i]))
                    buf[pos++] = '_';
                buf[pos++] = char.ToLowerInvariant(s[i]);
            }
        });
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "UserId" -> "user-id").
    /// Useful for URL and configuration naming.
    /// </summary>
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Pluralizes a word (basic implementation).
    /// Note: This is a simplified version; use external library for production use.
    /// </summary>
    public static string Pluralize(this string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        if (word.EndsWith('y'))
            return string.Concat(word.AsSpan(0, word.Length - 1), "ies");

        if (word.EndsWith('s') || word.EndsWith('x') || word.EndsWith('z'))
            return word + "es";

        return word + "s";
    }

    /// <summary>
    /// Truncates a string to a maximum length, optionally adding ellipsis.
    /// </summary>
    public static string Truncate(this string input, int maxLength, bool addEllipsis = false)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        // Span slice avoids an intermediate Substring allocation when appending ellipsis.
        return addEllipsis
            ? string.Concat(input.AsSpan(0, maxLength), "...")
            : input[..maxLength];
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
        if (string.IsNullOrEmpty(input))
            return input;

        var span = input.AsSpan();

        // Count non-whitespace chars so we allocate exactly once.
        int count = 0;
        foreach (char c in span)
            if (!char.IsWhiteSpace(c)) count++;

        if (count == input.Length) return input;
        if (count == 0) return string.Empty;

        return string.Create(count, input, static (chars, src) =>
        {
            int pos = 0;
            foreach (char c in src.AsSpan())
                if (!char.IsWhiteSpace(c)) chars[pos++] = c;
        });
    }

    /// <summary>
    /// Converts a string to a slug suitable for URLs.
    /// </summary>
    public static string ToSlug(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var slug = input.ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-')
            .Trim('-');

        return _slugDoubleHyphensRegex.Replace(
            _slugSpecialCharsRegex.Replace(slug, string.Empty), "-");
    }

    /// <summary>
    /// Repeats a string the specified number of times.
    /// </summary>
    public static string Repeat(this string input, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(input))
            return string.Empty;

        return string.Create(input.Length * count, (input, count), static (chars, state) =>
        {
            var (src, cnt) = state;
            var srcSpan = src.AsSpan();
            int pos = 0;
            for (int i = 0; i < cnt; i++)
            {
                srcSpan.CopyTo(chars[pos..]);
                pos += src.Length;
            }
        });
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

        // IndexOfAny on a Span avoids the array allocation from Split.
        var span = input.AsSpan().TrimStart();
        if (span.IsEmpty) return string.Empty;

        int end = span.IndexOfAny(' ', '\t', '\n');
        return (end < 0 ? span : span[..end]).ToString();
    }
}
