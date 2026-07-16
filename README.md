// existing content ...

## AuditHelper

`AuditHelper` is a utility class for tracking and logging audit information. Records entity changes, user actions, and system events. Provides methods to log entity changes, property changes, and other operations.

Below is a realistic example of using `AuditHelper` in a console application:

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Utilities;

class Program
{
    static void Main(string[] args)
    {
        // Log entity change
        AuditHelper.LogEntityChange("Product", "12345", "CREATE");

        // Log property change
        AuditHelper.LogPropertyChange("Product", "12345", "Price", 10.99m, 9.99m);

        // Get audit trail for an entity
        var auditTrail = AuditHelper.GetEntityAuditTrail("Product", "12345");
        foreach (var entry in auditTrail)
        {
            Console.WriteLine($"Timestamp: {entry.Timestamp}, Operation: {entry.Operation}");
        }

        // Get operation log
        var operationLog = AuditHelper.GetOperationLog("CREATE");
        foreach (var entry in operationLog)
        {
            Console.WriteLine($"Timestamp: {entry.Timestamp}, Entity: {entry.EntityType}");
        }

        // Get user audit trail
        var userAuditTrail = AuditHelper.GetUserAuditTrail("user123");
        foreach (var entry in userAuditTrail)
        {
            Console.WriteLine($"Timestamp: {entry.Timestamp}, Operation: {entry.Operation}");
        }

        // Get audit entries in a date range
        var auditEntries = AuditHelper.GetAuditEntriesInRange(DateTime.Now.AddDays(-7), DateTime.Now);
        foreach (var entry in auditEntries)
        {
            Console.WriteLine($"Timestamp: {entry.Timestamp}, Entity: {entry.EntityType}");
        }

        // Get audit statistics
        var statistics = AuditHelper.GetStatistics();
        Console.WriteLine($"Total Entries: {statistics.TotalEntries}");
        Console.WriteLine($"Operations: {string.Join(", ", statistics.Operations.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        // Clear audit log
        AuditHelper.ClearAuditLog();

        // Export audit log to CSV
        var csv = AuditHelper.ExportToCsv();
        Console.WriteLine(csv);
    }
}

public class AuditLogEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }
}

public class AuditStatistics
{
    public int TotalEntries { get; set; }
    public int EntryCount { get; set; }
    public Dictionary<string, int> Operations { get; set; } = new();
}
```
// ... rest of README content ...
