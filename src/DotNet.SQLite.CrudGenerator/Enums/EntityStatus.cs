#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Enums;

/// <summary>
/// Enumeration representing the status of an entity.
/// </summary>
public enum EntityStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancelled = 3,
    Failed = 4,
    Shipped = 5,
    Delivered = 6,
    Archived = 7
}
