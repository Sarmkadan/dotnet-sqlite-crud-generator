// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Enums;

/// <summary>
/// Enumeration representing the type of operation performed on an entity.
/// </summary>
public enum OperationType
{
    Create = 1,
    Read = 2,
    Update = 3,
    Delete = 4,
    Export = 5,
    Import = 6,
    Bulk = 7
}
