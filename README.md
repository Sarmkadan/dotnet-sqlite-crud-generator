## QueryBuilderBenchmarks

The `QueryBuilderBenchmarks` class provides a set of benchmarks for measuring the performance of the query builder. It allows you to generate query builders for different scenarios, such as generating a query builder for a user, order, or category.

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Benchmarks;

public class Demo
{
    public async Task RunAsync()
    {
        var queryBuilderBenchmarks = new QueryBuilderBenchmarks();
        await queryBuilderBenchmarks.Setup();
        await queryBuilderBenchmarks.GenerateQueryBuilderAsync();
        queryBuilderBenchmarks.BuildQueryBuilderSource();
        await queryBuilderBenchmarks.GenerateQueryBuilderForUserAsync();
        await queryBuilderBenchmarks.GenerateQueryBuilderForOrderAsync();
        await queryBuilderBenchmarks.GenerateQueryBuilderForCategoryAsync();
        await queryBuilderBenchmarks.Cleanup();
        queryBuilderBenchmarks.Dispose();
    }
}
```

## NamingConventionBenchmarks

The `NamingConventionBenchmarks` class provides a set of benchmarks for testing naming conventions. It allows you to test the conversion of C# property names to SQL column names, as well as the validation of property names against a set of rules.

```csharp
using System;
using DotNet.SQLite.CrudGenerator.Benchmarks;

public class Demo
{
    public void Run()
    {
        var namingConventionBenchmarks = new NamingConventionBenchmarks();
        Console.WriteLine(namingConventionBenchmarks.GetTableName);
        Console.WriteLine(namingConventionBenchmarks.GetColumnNamePlain("MyProperty"));
        Console.WriteLine(namingConventionBenchmarks.GetColumnNameDecimal("MyDecimalProperty"));
        var conventionInfo = namingConventionBenchmarks.GetConventionInfo();
        Console.WriteLine(conventionInfo.Name);
        Console.WriteLine(namingConventionBenchmarks.IsValidPropertyNameValid("ValidProperty"));
        Console.WriteLine(namingConventionBenchmarks.IsValidPropertyNameInvalid("Invalid Property"));
        Console.WriteLine(namingConventionBenchmarks.GetApiEndpoint("MyApiEndpoint"));
        Console.WriteLine(namingConventionBenchmarks.CSharpToSql("MyProperty"));
    }
}
```

## BackgroundWorkerServiceExtensions

The `BackgroundWorkerServiceExtensions` class provides extension methods for managing background task queues and scheduled tasks. It allows you to start/stop task runners, enqueue tasks, monitor queue length, and retrieve task statistics.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

public class Demo
{
    public async Task RunAsync()
    {
        // Start scheduled task runner
        var taskRunner = await BackgroundWorkerServiceExtensions.StartScheduledTaskAsync();
        
        // Enqueue a background task
        await BackgroundWorkerServiceExtensions.EnqueueTaskAsync(
            async () => 
            {
                await Task.Delay(1000);
                Console.WriteLine("Task completed");
            },
            CancellationToken.None);
        
        // Get current task statistics
        var stats = BackgroundWorkerServiceExtensions.GetTaskStatistics();
        Console.WriteLine($"Queue length: {BackgroundWorkerServiceExtensions.GetQueueLength()}");
        Console.WriteLine($"Worker count: {BackgroundWorkerServiceExtensions.GetWorkerCount()}");
        
        // Stop all scheduled tasks
        await BackgroundWorkerServiceExtensions.StopScheduledTasksAsync();
    }
}
```

## UserServiceExtensions

`UserServiceExtensions` provides a collection of helper methods for retrieving and querying `User` entities. It includes operations to fetch users by email or username, check for existence, obtain active or verified users, filter users by creation date, and count active or verified users.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;

public class Demo
{
    public async Task RunAsync()
    {
        var email = "user@example.com";
        var username = "johndoe";

        // Retrieve a user by email or username
        var userByEmail = await UserServiceExtensions.GetByEmailAsync(email);
        var userByUsername = await UserServiceExtensions.GetByUsernameAsync(username);

        // Check whether a user exists for a given email
        bool exists = await UserServiceExtensions.ExistsByEmailAsync(email);

        // Get collections of users
        IEnumerable<User> activeUsers = await UserServiceExtensions.GetActiveUsersAsync();
        IEnumerable<User> verifiedUsers = await UserServiceExtensions.GetVerifiedUsersAsync();
        IEnumerable<User> recentUsers = await UserServiceExtensions.GetUsersByCreationDateAsync();

        // Count users
        int activeCount = await UserServiceExtensions.CountActiveUsersAsync();
        int verifiedCount = await UserServiceExtensions.CountVerifiedUsersAsync();

        // Example output
        Console.WriteLine($"User by email: {userByEmail?.Id}");
        Console.WriteLine($"User by username: {userByUsername?.Id}");
        Console.WriteLine($"Exists by email: {exists}");
        Console.WriteLine($"Active users count: {activeCount}");
        Console.WriteLine($"Verified users count: {verifiedCount}");
    }
}
```

## DataExportServiceExtensions

The `DataExportServiceExtensions` class provides extension methods for exporting data in various formats. It allows you to export data as JSON, CSV, or byte arrays, and also supports exporting to multiple files. 

```csharp
using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.Services;

public class Demo
{
    public async Task RunAsync()
    {
        var data = new[] { new { Id = 1, Name = "John" }, new { Id = 2, Name = "Jane" } };

        // Export data as JSON
        var jsonData = await DataExportServiceExtensions.ExportAsJsonAsync(data);
        Console.WriteLine(jsonData);

        // Export data as CSV
        var csvData = await DataExportServiceExtensions.ExportAsCsvAsync(data);
        Console.WriteLine(csvData);

        // Export data to multiple files
        var exportResult = await DataExportServiceExtensions.ExportToMultipleFilesAsync(data);
        foreach (var file in exportResult)
        {
            Console.WriteLine($"{file.Key}: {file.Value}");
        }
    }
}
``` 
```