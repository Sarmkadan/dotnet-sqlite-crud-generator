// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Represents a product category with hierarchical support.
/// </summary>
public class Category
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Parent category ID must be non-negative")]
    [JsonPropertyName("parentCategoryId")]
    public int? ParentCategoryId { get; set; }

    [StringLength(100, ErrorMessage = "Slug must not exceed 100 characters")]
    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative")]
    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Validates the category model for consistency.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (DisplayOrder < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Generates a URL-friendly slug from the category name.
    /// </summary>
    public void GenerateSlug()
    {
        var slug = Name.ToLower().Replace(" ", "-").Replace("_", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\-+", "-").TrimEnd('-');
        Slug = slug;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines if this is a root category.
    /// </summary>
    public bool IsRootCategory() => ParentCategoryId is null or 0;

    /// <summary>
    /// Deactivates the category.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
