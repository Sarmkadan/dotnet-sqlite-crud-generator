# PerformanceMonitorJsonExtensions

Provides JSON serialization and deserialization helper methods for performance monitoring types (`PerformanceMonitor`, `OperationMetrics`, `PerformanceReport`, and `MemoryInfo`) to facilitate logging, storage, and transmission of performance data.

## API

### ToJson(PerformanceMonitor monitor)

Serializes a `PerformanceMonitor` instance to a JSON string.

- **Parameters**
  - `monitor`: The `PerformanceMonitor` instance to serialize.
- **Returns**
  - A JSON string representation of the `PerformanceMonitor` instance.
- **Throws**
  - `ArgumentNullException`: If `monitor` is `null`.

### FromJson(string json)

Deserializes a JSON string into a `PerformanceMonitor` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A `PerformanceMonitor` instance reconstructed from the JSON string.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.
  - `JsonException`: If the JSON is malformed or cannot be deserialized into a `PerformanceMonitor`.

### TryFromJson(string json, [MaybeNullWhen(false)] out PerformanceMonitor? monitor)

Attempts to deserialize a JSON string into a `PerformanceMonitor` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `monitor`: Output parameter that receives the deserialized `PerformanceMonitor` instance if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

### ToJson(OperationMetrics metrics)

Serializes an `OperationMetrics` instance to a JSON string.

- **Parameters**
  - `metrics`: The `OperationMetrics` instance to serialize.
- **Returns**
  - A JSON string representation of the `OperationMetrics` instance.
- **Throws**
  - `ArgumentNullException`: If `metrics` is `null`.

### FromJsonToOperationMetrics(string json)

Deserializes a JSON string into an `OperationMetrics` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - An `OperationMetrics` instance reconstructed from the JSON string.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.
  - `JsonException`: If the JSON is malformed or cannot be deserialized into an `OperationMetrics`.

### TryFromJson(string json, [MaybeNullWhen(false)] out OperationMetrics? metrics)

Attempts to deserialize a JSON string into an `OperationMetrics` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `metrics`: Output parameter that receives the deserialized `OperationMetrics` instance if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

### ToJson(PerformanceReport report)

Serializes a `PerformanceReport` instance to a JSON string.

- **Parameters**
  - `report`: The `PerformanceReport` instance to serialize.
- **Returns**
  - A JSON string representation of the `PerformanceReport` instance.
- **Throws**
  - `ArgumentNullException`: If `report` is `null`.

### FromJsonToPerformanceReport(string json)

Deserializes a JSON string into a `PerformanceReport` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A `PerformanceReport` instance reconstructed from the JSON string.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.
  - `JsonException`: If the JSON is malformed or cannot be deserialized into a `PerformanceReport`.

### TryFromJson(string json, [MaybeNullWhen(false)] out PerformanceReport? report)

Attempts to deserialize a JSON string into a `PerformanceReport` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `report`: Output parameter that receives the deserialized `PerformanceReport` instance if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

### ToJson(MemoryInfo memoryInfo)

Serializes a `MemoryInfo` instance to a JSON string.

- **Parameters**
  - `memoryInfo`: The `MemoryInfo` instance to serialize.
- **Returns**
  - A JSON string representation of the `MemoryInfo` instance.
- **Throws**
  - `ArgumentNullException`: If `memoryInfo` is `null`.

### FromJsonToMemoryInfo(string json)

Deserializes a JSON string into a `MemoryInfo` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A `MemoryInfo` instance reconstructed from the JSON string.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.
  - `JsonException`: If the JSON is malformed or cannot be deserialized into a `MemoryInfo`.

### TryFromJson(string json, [MaybeNullWhen(false)] out MemoryInfo? memoryInfo)

Attempts to deserialize a JSON string into a `MemoryInfo` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `memoryInfo`: Output parameter that receives the deserialized `MemoryInfo` instance if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

## Usage

### Logging performance metrics to a file

```csharp
using System;
using System.IO;

var monitor = new PerformanceMonitor
{
    StartTime = DateTime.UtcNow,
    EndTime = DateTime.UtcNow.AddSeconds(1.5),
    OperationName = "DatabaseQuery",
    Metrics = new OperationMetrics
    {
        DurationMs = 150,
        CpuUsage = 45.2,
        MemoryAllocated = 2_048_000
    }
};

string json = PerformanceMonitorJsonExtensions.ToJson(monitor);
File.WriteAllText("performance.json", json);
```

### Loading and processing a performance report

```csharp
using System;
using System.IO;

string json = File.ReadAllText("performance_report.json");

if (PerformanceMonitorJsonExtensions.TryFromJson(json, out var monitor))
{
    Console.WriteLine($"Operation '{monitor.OperationName}' took {monitor.Metrics.DurationMs}ms");
    
    if (PerformanceMonitorJsonExtensions.TryFromJsonToPerformanceReport(json, out var report))
    {
        Console.WriteLine($"Total operations: {report.Operations.Count}");
    }
}
```

## Notes

- All methods are static and thread-safe; they do not maintain any shared state.
- The JSON serialization uses the default `System.Text.Json` serializer with no custom options. If you require specific formatting (e.g., camelCase, indentation), wrap these helpers with your own serializer configuration.
- Malformed JSON will throw `JsonException` during deserialization; use the `TryFromJson` overloads to safely handle parsing errors without exceptions.
- The `FromJson*` methods are provided for convenience when exceptions are acceptable; prefer `TryFromJson` in performance-sensitive or high-throughput scenarios to avoid exception overhead.
- Null arguments are consistently rejected with `ArgumentNullException` across all methods.