#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Exceptions;
using Microsoft.Extensions.Logging;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for managing user operations with validation and business logic.
/// </summary>
public sealed class UserService : IService<User, int>
{
    private readonly IRepository<User, int> _userRepository;
    private readonly ILogger<UserService>? _logger;


    public UserService(IRepository<User, int> userRepository, ILogger<UserService>? logger = null)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger;
    }

    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be greater than 0", nameof(id));

        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<User> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        _logger?.LogDebug("Creating new user with email {UserEmail}", entity.Email);

        if (!Validate(entity))
        {
            _logger?.LogWarning("User validation failed for user with email {UserEmail}", entity.Email);
            throw new ValidationException("User validation failed. Check required fields.");
        }

        var existingUser = await GetByEmailAsync(entity.Email, cancellationToken);
        if (existingUser is not null)
        {
            _logger?.LogWarning("Duplicate email detected for user with email {UserEmail}", entity.Email);
            throw RepositoryException.DuplicateKey(nameof(User), nameof(User.Email), entity.Email);
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var createdUser = await _userRepository.AddAsync(entity, cancellationToken);
        _logger?.LogInformation("Successfully created user with ID {UserId} and email {UserEmail}", createdUser.Id, createdUser.Email);
        return createdUser;
    }

    public async Task<bool> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        _logger?.LogDebug("Updating user with ID {UserId}", entity.Id);

        if (entity.Id <= 0)
            throw new ArgumentException("Invalid user ID", nameof(entity));

        if (!Validate(entity))
        {
            _logger?.LogWarning("User validation failed for user with ID {UserId}", entity.Id);
            throw new ValidationException("User validation failed. Check required fields.");
        }

        var existing = await GetAsync(entity.Id, cancellationToken);
        if (existing is null)
        {
            _logger?.LogWarning("User with ID {UserId} not found for update", entity.Id);
            throw RepositoryException.EntityNotFound(nameof(User), entity.Id);
        }

        entity.UpdatedAt = DateTime.UtcNow;
        var success = await _userRepository.UpdateAsync(entity, cancellationToken);
        if (success)
            _logger?.LogInformation("Successfully updated user with ID {UserId}", entity.Id);
        return success;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be greater than 0", nameof(id));

        _logger?.LogDebug("Deleting user with ID {UserId}", id);

        var user = await GetAsync(id, cancellationToken);
        if (user is null)
        {
            _logger?.LogWarning("User with ID {UserId} not found for deletion", id);
            throw RepositoryException.EntityNotFound(nameof(User), id);
        }

        var success = await _userRepository.DeleteAsync(id, cancellationToken);
        if (success)
            _logger?.LogInformation("Successfully deleted user with ID {UserId}", id);
        return success;
    }

    /// <summary>
    /// Looks up a user by email address, using the repository's dedicated
    /// lookup when available and falling back to a generic search otherwise.
    /// </summary>
    private async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        if (_userRepository is UserRepository concreteRepository)
            return await concreteRepository.GetByEmailAsync(email, cancellationToken);

        var matches = await _userRepository.FindAsync(u => u.Email == email, cancellationToken);
        return matches?.FirstOrDefault();
    }

    public bool Validate(User entity)
    {
        if (entity is null) return false;
        return entity.Validate();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets users matching the specified predicate.
    /// </summary>
    public Task<IEnumerable<User>> FindAsync(Func<User, bool> predicate, CancellationToken cancellationToken = default)
        => _userRepository.FindAsync(predicate, cancellationToken);

    /// <summary>
    /// Counts users, optionally matching a predicate.
    /// </summary>
    public Task<int> CountAsync(Func<User, bool>? predicate = null, CancellationToken cancellationToken = default)
        => _userRepository.CountAsync(predicate, cancellationToken);

    /// <summary>
    /// Authenticates a user with email and password hash.
    /// </summary>
    public async Task<User?> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Attempting to authenticate user with email {UserEmail}", email);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordHash))
        {
            _logger?.LogWarning("Authentication attempt with empty email or password hash");
            return null;
        }

        var user = await GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            _logger?.LogDebug("User with email {UserEmail} not found", email);
            return null;
        }

        if (user.PasswordHash != passwordHash)
        {
            _logger?.LogWarning("Invalid password for user with email {UserEmail}", email);
            return null;
        }

        if (!user.IsActive)
        {
            _logger?.LogWarning("Inactive user with email {UserEmail} attempted to authenticate", email);
            return null;
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger?.LogInformation("Successfully authenticated user with email {UserEmail}", email);
        return user;
    }

    /// <summary>
    /// Resets user password (in production, this would send an email with a reset token).
    /// </summary>
    public async Task<bool> ResetPasswordAsync(int userId, string newPasswordHash, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(userId, cancellationToken);
        if (user is null)
            return false;

        user.PasswordHash = newPasswordHash;
        await UpdateAsync(user, cancellationToken);
        return true;
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(userId, cancellationToken);
        if (user is null)
            return false;

        user.Deactivate();
        await UpdateAsync(user, cancellationToken);
        return true;
    }

    /// <summary>
    /// Verifies user email address.
    /// </summary>
    public async Task<bool> VerifyEmailAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(userId, cancellationToken);
        if (user is null)
            return false;

        user.EmailVerified = true;
        await UpdateAsync(user, cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets user activity summary.
    /// </summary>
    public async Task<(int TotalUsers, int ActiveUsers, int VerifiedEmails)> GetActivitySummaryAsync(CancellationToken cancellationToken = default)
    {
        var users = await GetAllAsync(cancellationToken);
        var userList = users.ToList();
        return (
            TotalUsers: userList.Count,
            ActiveUsers: userList.Count(u => u.IsActive),
            VerifiedEmails: userList.Count(u => u.EmailVerified)
        );
    }
}
