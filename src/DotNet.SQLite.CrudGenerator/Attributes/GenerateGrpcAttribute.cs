// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

namespace DotNet.SQLite.CrudGenerator.Attributes;

/// <summary>
/// Marks a class for gRPC service generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateGrpcAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to generate async methods.
    /// </summary>
    public bool GenerateAsync { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate CRUD operations.
    /// </summary>
    public bool GenerateCrud { get; set; } = true;

    /// <summary>
    /// Gets or sets the service name. If not specified, the class name + "Service" is used.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets whether to generate streaming methods.
    /// </summary>
    public bool GenerateStreaming { get; set; } = false;

    /// <summary>
    /// Gets or sets the namespace for the generated service.
    /// </summary>
    public string? Namespace { get; set; }
}
