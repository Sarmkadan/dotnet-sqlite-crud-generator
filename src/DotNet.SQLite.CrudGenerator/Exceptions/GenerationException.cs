// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Exceptions;

/// <summary>
/// Exception thrown when code generation fails.
/// </summary>
public class GenerationException : Exception
{
    public GenerationException(string message) : base(message) { }

    public GenerationException(string message, Exception innerException)
        : base(message, innerException) { }

    public string? GenerationType { get; set; }
    public string? SourceEntity { get; set; }
    public int? LineNumber { get; set; }

    /// <summary>
    /// Creates a generation exception for missing required configuration.
    /// </summary>
    public static GenerationException MissingConfiguration(string configName)
    {
        return new GenerationException($"Required configuration '{configName}' is missing.")
        {
            GenerationType = "Configuration"
        };
    }

    /// <summary>
    /// Creates a generation exception for invalid models.
    /// </summary>
    public static GenerationException InvalidModel(string modelName, string reason)
    {
        return new GenerationException($"Model '{modelName}' is invalid: {reason}")
        {
            GenerationType = "Model",
            SourceEntity = modelName
        };
    }

    /// <summary>
    /// Creates a generation exception for code generation errors.
    /// </summary>
    public static GenerationException CodeGenerationFailed(string generationType, string sourceEntity, Exception innerException)
    {
        return new GenerationException($"Failed to generate {generationType} for {sourceEntity}", innerException)
        {
            GenerationType = generationType,
            SourceEntity = sourceEntity
        };
    }
}
