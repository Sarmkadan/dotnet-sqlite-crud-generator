using DotNet.SQLite.CrudGenerator.Models;

Console.WriteLine("Testing SoftDeleteOptions...");

// Test 1: Default options (disabled)
var defaultOptions = new SoftDeleteOptions();
Console.WriteLine($"Default Enabled: {defaultOptions.Enabled}");
Console.WriteLine($"Default ColumnName: {defaultOptions.ColumnName}");
Console.WriteLine($"Default ActiveValue: {defaultOptions.ActiveValue}");
Console.WriteLine($"Default DeletedValue: {defaultOptions.DeletedValue}");

var whereClauseDisabled = defaultOptions.GetWhereClause();
Console.WriteLine($"Where clause (disabled): {whereClauseDisabled}");

var setClauseDisabled = defaultOptions.GetSetClause();
Console.WriteLine($"Set clause (disabled): '{setClauseDisabled}'");

Console.WriteLine("\n---\n");

// Test 2: Enabled options
var enabledOptions = new SoftDeleteOptions
{
    Enabled = true,
    ColumnName = "IsDeleted",
    ActiveValue = 0,
    DeletedValue = 1
};

Console.WriteLine($"Enabled Enabled: {enabledOptions.Enabled}");
Console.WriteLine($"Enabled ColumnName: {enabledOptions.ColumnName}");
Console.WriteLine($"Enabled ActiveValue: {enabledOptions.ActiveValue}");
Console.WriteLine($"Enabled DeletedValue: {enabledOptions.DeletedValue}");

var whereClauseEnabled = enabledOptions.GetWhereClause();
Console.WriteLine($"Where clause (enabled): {whereClauseEnabled}");

var setClauseEnabled = enabledOptions.GetSetClause();
Console.WriteLine($"Set clause (enabled): {setClauseEnabled}");

Console.WriteLine("\n---\n");

// Test 3: Validation
try
{
    enabledOptions.Validate();
    Console.WriteLine("Validation passed for enabled options");
}
catch (Exception ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

try
{
    var invalidOptions = new SoftDeleteOptions { Enabled = true, ColumnName = "" };
    invalidOptions.Validate();
    Console.WriteLine("ERROR: Should have thrown exception for empty ColumnName");
}
catch (Exception ex)
{
    Console.WriteLine($"Correctly caught validation error for empty ColumnName: {ex.Message}");
}

try
{
    var invalidOptions2 = new SoftDeleteOptions { Enabled = true, ColumnName = "Test", ActiveValue = 1, DeletedValue = 1 };
    invalidOptions2.Validate();
    Console.WriteLine("ERROR: Should have thrown exception for equal ActiveValue and DeletedValue");
}
catch (Exception ex)
{
    Console.WriteLine($"Correctly caught validation error for equal values: {ex.Message}");
}

Console.WriteLine("\n---\n");
Console.WriteLine("All tests completed successfully!");
