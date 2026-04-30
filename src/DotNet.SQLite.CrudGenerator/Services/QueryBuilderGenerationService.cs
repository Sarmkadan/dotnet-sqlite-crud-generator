#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Text;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Generates a strongly-typed, fluent query builder class for a given entity type.
/// The generated class wraps raw SQL construction behind a readable API and is
/// written as a .cs file to <see cref="OutputPath"/>.
/// </summary>
public sealed class QueryBuilderGenerationService
{
    /// <summary>Root directory where generated files are written.</summary>
    public string OutputPath { get; }

    /// <summary>
    /// Initialises the service.
    /// </summary>
    /// <param name="outputPath">Directory that will receive generated .cs files. Created if absent.</param>
    public QueryBuilderGenerationService(string outputPath = "./Generated")
    {
        OutputPath = outputPath;
        Directory.CreateDirectory(OutputPath);
    }

    /// <summary>
    /// Generates a fluent query builder class for <paramref name="entityType"/> and writes it to disk.
    /// </summary>
    /// <param name="entityType">The C# entity type to generate a query builder for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Path of the generated .cs file.</returns>
    public async Task<string> GenerateQueryBuilderAsync(Type entityType, CancellationToken cancellationToken = default)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        var content = BuildQueryBuilderSource(entityType);
        var filePath = Path.Combine(OutputPath, $"{entityType.Name}QueryBuilder.cs");
        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        return filePath;
    }

    /// <summary>
    /// Builds the full source text of the query builder class without writing it to disk.
    /// Useful for preview and testing.
    /// </summary>
    public string BuildQueryBuilderSource(Type entityType)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        var name = entityType.Name;
        var tableName = name + "s";
        var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              .Where(p => p.CanRead)
                              .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("// =============================================================================");
        sb.AppendLine("// Author: Vladyslav Zaiets | https://sarmkadan.com");
        sb.AppendLine("// CTO & Software Architect");
        sb.AppendLine("// =============================================================================");
        sb.AppendLine();
        sb.AppendLine("using System.Text;");
        sb.AppendLine();
        sb.AppendLine($"namespace DotNet.SQLite.CrudGenerator.Generated;");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// Fluent SQL query builder for the <c>{name}</c> entity.");
        sb.AppendLine($"/// Construct a <see cref=\"{name}QueryBuilder\"/>, chain filter/sort/page methods,");
        sb.AppendLine($"/// and call <see cref=\"Build\"/> to produce a parameterised SQL SELECT statement.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"public sealed class {name}QueryBuilder");
        sb.AppendLine("{");
        sb.AppendLine($"    private const string TableName = \"{tableName}\";");
        sb.AppendLine();
        sb.AppendLine("    private readonly List<string> _whereClauses = new();");
        sb.AppendLine("    private readonly List<(string Column, bool Descending)> _orderBy = new();");
        sb.AppendLine("    private readonly Dictionary<string, object?> _parameters = new();");
        sb.AppendLine("    private int? _limit;");
        sb.AppendLine("    private int? _offset;");
        sb.AppendLine("    private string _select = \"*\";");
        sb.AppendLine("    private int _paramIndex;");
        sb.AppendLine();

        // Select overload
        sb.AppendLine($"    /// <summary>Specifies the columns to select (default is <c>*</c>).</summary>");
        sb.AppendLine($"    public {name}QueryBuilder Select(params string[] columns)");
        sb.AppendLine("    {");
        sb.AppendLine("        _select = columns.Length > 0 ? string.Join(\", \", columns) : \"*\";");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generic Where
        sb.AppendLine($"    /// <summary>Adds a WHERE clause using a raw column name, operator, and value.</summary>");
        sb.AppendLine($"    /// <param name=\"column\">Column name.</param>");
        sb.AppendLine($"    /// <param name=\"op\">SQL operator, e.g. <c>=</c>, <c>&gt;</c>, <c>LIKE</c>.</param>");
        sb.AppendLine($"    /// <param name=\"value\">Parameter value (bound safely, not interpolated).</param>");
        sb.AppendLine($"    public {name}QueryBuilder Where(string column, string op, object? value)");
        sb.AppendLine("    {");
        sb.AppendLine("        var paramName = $\"@p{_paramIndex++}\";");
        sb.AppendLine("        _whereClauses.Add($\"{column} {op} {paramName}\");");
        sb.AppendLine("        _parameters[paramName] = value;");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // WhereNull / WhereNotNull
        sb.AppendLine($"    /// <summary>Adds a WHERE &lt;column&gt; IS NULL filter.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder WhereNull(string column)");
        sb.AppendLine("    {");
        sb.AppendLine("        _whereClauses.Add($\"{column} IS NULL\");");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Adds a WHERE &lt;column&gt; IS NOT NULL filter.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder WhereNotNull(string column)");
        sb.AppendLine("    {");
        sb.AppendLine("        _whereClauses.Add($\"{column} IS NOT NULL\");");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Strongly-typed convenience Where methods per property
        foreach (var prop in props)
        {
            var typeName = GetFriendlyTypeName(prop.PropertyType);
            sb.AppendLine($"    /// <summary>Adds a WHERE filter on the <c>{prop.Name}</c> column.</summary>");
            sb.AppendLine($"    public {name}QueryBuilder Where{prop.Name}(string op, {typeName} value) => Where(\"{prop.Name}\", op, value);");
            sb.AppendLine();
        }

        // OrderBy
        sb.AppendLine($"    /// <summary>Adds an ORDER BY clause.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder OrderBy(string column, bool descending = false)");
        sb.AppendLine("    {");
        sb.AppendLine("        _orderBy.Add((column, descending));");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Adds a descending ORDER BY clause.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder OrderByDescending(string column) => OrderBy(column, descending: true);");
        sb.AppendLine();

        // Limit / Offset
        sb.AppendLine($"    /// <summary>Limits the number of rows returned.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder Limit(int limit)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (limit < 0) throw new ArgumentOutOfRangeException(nameof(limit));");
        sb.AppendLine("        _limit = limit;");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Skips the specified number of rows (pagination).</summary>");
        sb.AppendLine($"    public {name}QueryBuilder Offset(int offset)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));");
        sb.AppendLine("        _offset = offset;");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Build
        sb.AppendLine($"    /// <summary>Builds the SQL SELECT statement.</summary>");
        sb.AppendLine($"    /// <returns>A tuple of (sql, parameters) ready to pass to a data reader.</returns>");
        sb.AppendLine($"    public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build()");
        sb.AppendLine("    {");
        sb.AppendLine("        var sql = new StringBuilder($\"SELECT {_select} FROM {TableName}\");");
        sb.AppendLine();
        sb.AppendLine("        if (_whereClauses.Count > 0)");
        sb.AppendLine("            sql.Append(\" WHERE \").Append(string.Join(\" AND \", _whereClauses));");
        sb.AppendLine();
        sb.AppendLine("        if (_orderBy.Count > 0)");
        sb.AppendLine("        {");
        sb.AppendLine("            var parts = _orderBy.Select(o => o.Descending ? $\"{o.Column} DESC\" : o.Column);");
        sb.AppendLine("            sql.Append(\" ORDER BY \").Append(string.Join(\", \", parts));");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (_limit.HasValue)");
        sb.AppendLine("            sql.Append($\" LIMIT {_limit.Value}\");");
        sb.AppendLine();
        sb.AppendLine("        if (_offset.HasValue)");
        sb.AppendLine("            sql.Append($\" OFFSET {_offset.Value}\");");
        sb.AppendLine();
        sb.AppendLine("        return (sql.ToString(), _parameters);");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Reset
        sb.AppendLine($"    /// <summary>Resets all clauses so the builder can be reused.</summary>");
        sb.AppendLine($"    public {name}QueryBuilder Reset()");
        sb.AppendLine("    {");
        sb.AppendLine("        _whereClauses.Clear();");
        sb.AppendLine("        _orderBy.Clear();");
        sb.AppendLine("        _parameters.Clear();");
        sb.AppendLine("        _limit = null;");
        sb.AppendLine("        _offset = null;");
        sb.AppendLine("        _select = \"*\";");
        sb.AppendLine("        _paramIndex = 0;");
        sb.AppendLine("        return this;");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(string)) return "string?";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(DateTime)) return "DateTime";
        if (type == typeof(Guid)) return "Guid";
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null) return $"{GetFriendlyTypeName(underlying)}?";
        return type.Name;
    }
}
