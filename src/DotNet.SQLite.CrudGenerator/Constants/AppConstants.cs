// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    public const string ApplicationName = "SQLite CRUD Generator";
    public const string Version = "1.0.0";
    public const string Author = "Vladyslav Zaiets";
    public const string WebsiteUrl = "https://sarmkadan.com";

    /// <summary>
    /// Database operation constants.
    /// </summary>
    public static class Database
    {
        public const string DefaultDatabaseFileName = "crudgenerator.db";
        public const int DefaultConnectionTimeout = 30;
        public const int DefaultBatchSize = 100;
    }

    /// <summary>
    /// Validation constants.
    /// </summary>
    public static class Validation
    {
        public const int MinPasswordLength = 8;
        public const int MaxUsernameLength = 100;
        public const int MaxEmailLength = 255;
        public const int MaxProductNameLength = 255;
        public const int MaxSkuLength = 100;
    }

    /// <summary>
    /// Business logic constants.
    /// </summary>
    public static class Business
    {
        public const int DefaultReorderLevel = 10;
        public const string DefaultProductUnit = "piece";
        public const decimal MaxProductPrice = 999999.99m;
        public const int DefaultOrderItemCount = 1;
    }

    /// <summary>
    /// Error message constants.
    /// </summary>
    public static class ErrorMessages
    {
        public const string EntityNotFound = "The requested entity was not found.";
        public const string InvalidInput = "The provided input is invalid.";
        public const string OperationFailed = "The operation failed. Please try again.";
        public const string UnauthorizedOperation = "You are not authorized to perform this operation.";
        public const string DuplicateEntity = "An entity with the same unique identifier already exists.";
        public const string InvalidEntityState = "The entity is in an invalid state for this operation.";
    }

    /// <summary>
    /// Success message constants.
    /// </summary>
    public static class SuccessMessages
    {
        public const string EntityCreated = "Entity created successfully.";
        public const string EntityUpdated = "Entity updated successfully.";
        public const string EntityDeleted = "Entity deleted successfully.";
        public const string OperationCompleted = "Operation completed successfully.";
    }
}
