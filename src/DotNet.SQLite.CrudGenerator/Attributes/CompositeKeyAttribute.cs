#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Attributes;

/// <summary>
/// Marks a property as part of a composite primary key.
/// Apply this attribute to two or more properties on a <c>[SqliteCrudEntity]</c>-decorated class
/// to define a composite primary key instead of the default single-column <c>Id</c> key.
/// </summary>
/// <remarks>
/// <para>
/// When at least two properties in an entity carry this attribute the generator emits a
/// <c>PRIMARY KEY (col1, col2, …)</c> constraint in the <c>CREATE TABLE</c> statement and
/// builds the <c>WHERE</c> clauses for <c>SELECT</c>, <c>UPDATE</c>, and <c>DELETE</c>
/// operations accordingly.
/// </para>
/// <example>
/// <code>
/// [SqliteCrudEntity]
/// public class UserRole
/// {
///     [CompositeKey(Order = 0)]
///     public Guid UserId { get; set; }
///
///     [CompositeKey(Order = 1)]
///     public Guid RoleId { get; set; }
///
///     public DateTime AssignedAt { get; set; }
/// }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CompositeKeyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the zero-based position of this column within the composite primary key.
    /// Properties are sorted by <see cref="Order"/> ascending before the key definition is emitted.
    /// </summary>
    public int Order { get; set; }
}
