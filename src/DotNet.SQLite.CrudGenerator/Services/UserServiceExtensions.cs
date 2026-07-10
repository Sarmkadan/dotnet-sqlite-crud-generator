#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Data;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Extension methods for UserService providing additional utility functionality.
/// </summary>
public static class UserServiceExtensions
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="email">The email address to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found, otherwise null.</returns>
    public static async Task<User?> GetByEmailAsync(this UserService service, string email, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or whitespace", nameof(email));

        var userRepository = (UserRepository)service.GetFieldValue("_userRepository");
        return await userRepository.GetByEmailAsync(email, cancellationToken);
    }

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="username">The username to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found, otherwise null.</returns>
    public static async Task<User?> GetByUsernameAsync(this UserService service, string username, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or whitespace", nameof(username));

        var users = await service.FindAsync(u => u.Username?.Equals(username, StringComparison.OrdinalIgnoreCase) == true, cancellationToken);
        return users.FirstOrDefault();
    }

    /// <summary>
    /// Checks if a user with the specified email exists.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if user exists, otherwise false.</returns>
    public static async Task<bool> ExistsByEmailAsync(this UserService service, string email, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or whitespace", nameof(email));

        var user = await service.GetByEmailAsync(email, cancellationToken);
        return user is not null;
    }

    /// <summary>
    /// Gets all active users.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of active users.</returns>
    public static async Task<IEnumerable<User>> GetActiveUsersAsync(this UserService service, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        return await service.FindAsync(u => u.IsActive, cancellationToken);
    }

    /// <summary>
    /// Gets all verified users.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of verified users.</returns>
    public static async Task<IEnumerable<User>> GetVerifiedUsersAsync(this UserService service, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        return await service.FindAsync(u => u.EmailVerified, cancellationToken);
    }

    /// <summary>
    /// Gets all users created within a specific time range.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="startDate">The start date of the range (inclusive).</param>
    /// <param name="endDate">The end date of the range (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of users created within the specified range.</returns>
    public static async Task<IEnumerable<User>> GetUsersByCreationDateAsync(
        this UserService service,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date", nameof(startDate));

        return await service.FindAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate, cancellationToken);
    }

    /// <summary>
    /// Gets the count of active users.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of active users.</returns>
    public static async Task<int> CountActiveUsersAsync(this UserService service, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var activeUsers = await service.GetActiveUsersAsync(cancellationToken);
        return activeUsers.Count();
    }

    /// <summary>
    /// Gets the count of verified users.
    /// </summary>
    /// <param name="service">The UserService instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of verified users.</returns>
    public static async Task<int> CountVerifiedUsersAsync(this UserService service, CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var verifiedUsers = await service.GetVerifiedUsersAsync(cancellationToken);
        return verifiedUsers.Count();
    }

    /// <summary>
    /// Helper method to get private field value via reflection.
    /// </summary>
    private static object GetFieldValue(this object obj, string fieldName)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(obj) ?? throw new InvalidOperationException($"Field {fieldName} not found");
    }
}