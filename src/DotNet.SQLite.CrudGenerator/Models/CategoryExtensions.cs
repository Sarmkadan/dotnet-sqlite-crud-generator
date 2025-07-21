#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Provides useful extension methods for the Category class to facilitate common operations
/// such as hierarchical traversal, path generation, and category filtering.
/// </summary>
public static class CategoryExtensions
{
    /// <summary>
    /// Gets the full hierarchical path of the category from root to this category.
    /// </summary>
    /// <param name="category">The category to get the path for</param>
    /// <param name="allCategories">All available categories to build the hierarchy</param>
    /// <returns>An enumerable representing the path from root to this category</returns>
    public static IEnumerable<Category> GetPath(this Category category, IEnumerable<Category> allCategories)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));

        if (allCategories is null)
            throw new ArgumentNullException(nameof(allCategories));

        var path = new List<Category>();
        var current = category;

        while (current != null)
        {
            path.Add(current);
            if (current.IsRootCategory())
                break;

            current = allCategories.FirstOrDefault(c => c.Id == current.ParentCategoryId);
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Gets the full path as a string with category names separated by the specified separator.
    /// </summary>
    /// <param name="category">The category to get the path for</param>
    /// <param name="allCategories">All available categories to build the hierarchy</param>
    /// <param name="separator">The separator to use between category names (default: " → ")</param>
    /// <returns>A formatted string representing the full category path</returns>
    public static string GetPathString(this Category category, IEnumerable<Category> allCategories, string separator = " → ")
    {
        var path = category.GetPath(allCategories);
        return string.Join(separator, path.Select(c => c.Name));
    }

    /// <summary>
    /// Gets all descendant categories (children, grandchildren, etc.) recursively.
    /// </summary>
    /// <param name="category">The parent category</param>
    /// <param name="allCategories">All available categories to search through</param>
    /// <returns>An enumerable of all descendant categories</returns>
    public static IEnumerable<Category> GetDescendants(this Category category, IEnumerable<Category> allCategories)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));

        if (allCategories is null)
            throw new ArgumentNullException(nameof(allCategories));

        var children = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();

        foreach (var child in children)
        {
            yield return child;
            foreach (var descendant in child.GetDescendants(allCategories))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Gets all ancestor categories (parent, grandparent, etc.) up to the root.
    /// </summary>
    /// <param name="category">The category to get ancestors for</param>
    /// <param name="allCategories">All available categories to search through</param>
    /// <returns>An enumerable of all ancestor categories</returns>
    public static IEnumerable<Category> GetAncestors(this Category category, IEnumerable<Category> allCategories)
    {
        if (category is null)
            throw new ArgumentNullException(nameof(category));

        if (allCategories is null)
            throw new ArgumentNullException(nameof(allCategories));

        var current = category;

        while (!current.IsRootCategory())
        {
            var parent = allCategories.FirstOrDefault(c => c.Id == current.ParentCategoryId);
            if (parent == null)
                break;

            yield return parent;
            current = parent;
        }
    }
}