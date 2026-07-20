#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotNet.SQLite.CrudGenerator.Models;

/// <summary>
/// Provides the configuration necessary to enable and manage soft-delete functionality
/// within generated CRUD operations.
/// </summary>
/// <remarks>
/// When soft-delete is enabled, DELETE operations are converted to UPDATE operations that
/// set the specified column to the deleted value, and SELECT operations include a WHERE
/// clause to filter out deleted records.
/// </remarks>
public sealed class SoftDeleteOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether soft-delete functionality is active for the entity.
    /// When false, standard physical deletion behavior is used.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the name of the database column used to track the deletion status of the row.
    /// </summary>
    public string ColumnName { get; set; } = "IsDeleted";

    /// <summary>
    /// Gets or sets the integer value that represents a deleted entity in the database.
    /// </summary>
    public int DeletedValue { get; set; } = 1;

    /// <summary>
    /// Gets or sets the integer value that represents an active entity in the database.
    /// </summary>
    public int ActiveValue { get; set; } = 0;

    /// <summary>
    /// Returns the SQL fragment required for a WHERE clause to filter out records marked as deleted.
    /// </summary>
    /// <returns>SQL WHERE clause fragment (e.g., "[IsDeleted] = 0")</returns>
    public string GetWhereClause()
    {
        if (!Enabled)
        {
            return "1=1"; // Always true when soft-delete is disabled
        }

        if (string.IsNullOrWhiteSpace(ColumnName))
        {
            throw new InvalidOperationException("ColumnName must be specified when soft-delete is enabled.");
        }

        return $"{ColumnName} = {ActiveValue}";
    }

    /// <summary>
    /// Returns the SQL fragment required for an UPDATE statement to mark a record as deleted.
    /// </summary>
    /// <returns>SQL SET clause fragment (e.g., "[IsDeleted] = 1")</returns>
    public string GetSetClause()
    {
        if (!Enabled)
        {
            return ""; // Empty when soft-delete is disabled
        }

        if (string.IsNullOrWhiteSpace(ColumnName))
        {
            throw new InvalidOperationException("ColumnName must be specified when soft-delete is enabled.");
        }

        return $"{ColumnName} = {DeletedValue}";
    }

    /// <summary>
    /// Performs validation on the configuration.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public void Validate()
    {
        if (Enabled)
        {
            if (string.IsNullOrWhiteSpace(ColumnName))
            {
                throw new InvalidOperationException("ColumnName must be specified when soft-delete is enabled.");
            }

            if (ActiveValue == DeletedValue)
            {
                throw new InvalidOperationException("ActiveValue and DeletedValue must be different values.");
            }
        }
    }
}