#nullable enable
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
        // AuditTrail service queries
        public const string AuditLogsDeleteOlderThan = "DELETE FROM AuditLogs WHERE Timestamp < @cutoff";
        public const string AuditLogsCountAll = "SELECT COUNT(*) FROM AuditLogs";
        public const string AuditLogsCountByOperation = "SELECT OperationType, COUNT(*) FROM AuditLogs GROUP BY OperationType";
        public const string AuditLogsCountByEntity = "SELECT EntityType, COUNT(*) FROM AuditLogs GROUP BY EntityType";
        public const string AuditLogsTimestampRange = "SELECT MIN(Timestamp), MAX(Timestamp) FROM AuditLogs";
        public const string AuditLogsSelectWithWhereAndOrder = "SELECT * FROM AuditLogs{0} ORDER BY Timestamp DESC LIMIT @limit";
        // QueryBuilder service
        public const string SelectFromTable = "SELECT {0} FROM {1}";
        public const string SelectColumnsFromTable = "SELECT {0} FROM {1}";
        // Soft delete queries
        public const string SoftDeleteSelectActive = "SELECT * FROM {0}s WHERE {1} = {2}";
        public const string SoftDeleteMarkDeleted = "UPDATE {0}s SET {1} = {2} WHERE Id = @id";
        // Repository base queries
        public const string SelectByIdWithLimit = "SELECT * FROM {0} WHERE {1} = {2} LIMIT 1";
        public const string SelectAllFromTable = "SELECT * FROM {0}";
        public const string CountAllFromTable = "SELECT COUNT(*) FROM {0}";
        public const string InsertIntoTable = "INSERT INTO {0} ({1}) VALUES ({2})";
        public const string LastInsertRowId = "SELECT last_insert_rowid();";
        public const string InsertIntoTableReturning = "INSERT INTO {0} ({1}) VALUES ({2}) RETURNING *";
        public const string UpdateTableSet = "UPDATE {0} SET {1} WHERE {2} = {3}";
        public const string DeleteFromTableWhere = "DELETE FROM {0} WHERE {1} = {2}";
    }
}
