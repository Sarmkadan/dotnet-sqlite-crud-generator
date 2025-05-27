// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for managing user operations with validation and business logic.
/// </summary>
public class UserService : IService<User, int>
{
    private readonly IRepository<User, int> _userRepository;

    public UserService(IRepository<User, int> userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("User validation failed. Check required fields.");

        var existingUser = await ((UserRepository)_userRepository).GetByEmailAsync(entity.Email, cancellationToken);
        if (existingUser != null)
            throw RepositoryException.DuplicateKey(nameof(User), nameof(User.Email), entity.Email);

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id <= 0)
            throw new ArgumentException("Invalid user ID", nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("User validation failed. Check required fields.");

        var existing = await GetAsync(entity.Id, cancellationToken);
        if (existing == null)
            throw RepositoryException.EntityNotFound(nameof(User), entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;
        return await _userRepository.UpdateAsync(entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be greater than 0", nameof(id));

        var user = await GetAsync(id, cancellationToken);
        if (user == null)
            throw RepositoryException.EntityNotFound(nameof(User), id);

        return await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public bool Validate(User entity)
    {
        if (entity == null) return false;
        return entity.Validate();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Authenticates a user with email and password hash.
    /// </summary>
    public async Task<User?> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordHash))
            return null;

        var user = await ((UserRepository)_userRepository).GetByEmailAsync(email, cancellationToken);
        if (user == null || user.PasswordHash != passwordHash || !user.IsActive)
            return null;

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);
        return user;
    }

    /// <summary>
    /// Resets user password (in production, this would send an email with a reset token).
    /// </summary>
    public async Task<bool> ResetPasswordAsync(int userId, string newPasswordHash, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(userId, cancellationToken);
        if (user == null)
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
        if (user == null)
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
        if (user == null)
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
