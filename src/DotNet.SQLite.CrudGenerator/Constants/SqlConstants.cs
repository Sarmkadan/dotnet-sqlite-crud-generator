// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Constants;

/// <summary>
/// SQL-related constants for database queries.
/// </summary>
public static class SqlConstants
{
    public const string DefaultSchema = "main";

    /// <summary>
    /// SQL keywords and operators.
    /// </summary>
    public static class Keywords
    {
        public const string Select = "SELECT";
        public const string From = "FROM";
        public const string Where = "WHERE";
        public const string OrderBy = "ORDER BY";
        public const string Insert = "INSERT INTO";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE FROM";
        public const string Join = "JOIN";
    }

    /// <summary>
    /// Table names.
    /// </summary>
    public static class Tables
    {
        public const string Users = "Users";
        public const string Products = "Products";
        public const string Orders = "Orders";
        public const string Categories = "Categories";
        public const string AuditLogs = "AuditLogs";
    }

    /// <summary>
    /// Common column names.
    /// </summary>
    public static class Columns
    {
        public const string Id = "Id";
        public const string CreatedAt = "CreatedAt";
        public const string UpdatedAt = "UpdatedAt";
        public const string IsActive = "IsActive";
    }

    /// <summary>
    /// Index names.
    /// </summary>
    public static class Indexes
    {
        public const string IdxUsersEmail = "idx_Users_Email";
        public const string IdxUsersUsername = "idx_Users_Username";
        public const string IdxProductsCategoryId = "idx_Products_CategoryId";
        public const string IdxProductsSku = "idx_Products_Sku";
        public const string IdxOrdersUserId = "idx_Orders_UserId";
        public const string IdxOrdersOrderNumber = "idx_Orders_OrderNumber";
        public const string IdxOrdersStatus = "idx_Orders_Status";
        public const string IdxAuditLogsEntityType = "idx_AuditLogs_EntityType";
        public const string IdxAuditLogsTimestamp = "idx_AuditLogs_Timestamp";
    }

    /// <summary>
    /// Query templates.
    /// </summary>
    public static class QueryTemplates
    {
        public const string SelectAll = "SELECT * FROM {0}";
        public const string SelectById = "SELECT * FROM {0} WHERE Id = @id";
        public const string CountAll = "SELECT COUNT(*) FROM {0}";
        public const string DeleteById = "DELETE FROM {0} WHERE Id = @id";
    }
}
