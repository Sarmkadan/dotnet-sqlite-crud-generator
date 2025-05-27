// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Represents a user entity in the system with authentication and profile information.
/// </summary>
public class User
{
    [Key]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    [JsonPropertyName("username")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password hash is required")]
    [StringLength(500, ErrorMessage = "Password hash must not exceed 500 characters")]
    [JsonPropertyName("passwordHash")]
    public required string PasswordHash { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name must not exceed 100 characters")]
    [JsonPropertyName("firstName")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name must not exceed 100 characters")]
    [JsonPropertyName("lastName")]
    public required string LastName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("emailVerified")]
    public bool EmailVerified { get; set; } = false;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Validates the user model for consistency and business rules.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
            return false;

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            return false;

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            return false;

        return true;
    }

    /// <summary>
    /// Gets the full name of the user.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Marks the user as having logged in successfully.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
