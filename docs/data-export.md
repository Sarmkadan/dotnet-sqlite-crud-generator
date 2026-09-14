# Data Export

`DataExportService` (in `src/DotNet.SQLite.CrudGenerator/Services/DataExportService.cs`)
serializes collections of entity objects into a variety of interchange formats and
persists or streams the resulting output. It is the public entry point over the
formatters in `src/DotNet.SQLite.CrudGenerator/Formatters/`.

## Supported formats

| Format | Method(s) | Notes |
| --- | --- | --- |
| JSON | `ExportAsJsonAsync<T>` | Pretty-printed, camelCase property names, enums as strings, `DateTime` in round-trip `"O"` format, circular references preserved. |
| CSV | `ExportAsCsvAsync<T>` | Header row + one row per item. Values containing the delimiter, quotes, or newlines are quoted and escaped. |
| XML | `ExportAsXmlAsync<T>` | Collection wrapped in a `<root>` element; each item serialized with `XmlSerializer`. |
| JSON Lines | `ExportAsJsonLinesAsync<T>`, `ExportAsJsonLinesToFileAsync<T>`, `ExportAsJsonLinesToStreamAsync<T>` | One JSON object per line. |
| Markdown | `ExportAsMarkdownAsync<T>`, `ExportAsMarkdownToFileAsync<T>` | GitHub-flavored table. Newest addition. |

The generic `ExportToFileAsync<T>` / `ExportToStreamAsync<T>` overloads accept an
`ExportFormat` enum (`Json`, `Csv`, `Xml`) and therefore cover only JSON, CSV, and
XML. Markdown and JSON Lines have their own dedicated methods instead.

## API

All export methods are `async` and require `T : class`. They throw
`ArgumentNullException` if `items` is `null`; the file-based methods also throw
`ArgumentException` if `filePath` is null, empty, or whitespace.

### ExportAsJsonAsync\<T\>(IEnumerable\<T\> items)
Returns a `Task<string>` with the JSON representation of the collection.

### ExportAsCsvAsync\<T\>(IEnumerable\<T\> items)
Returns a `Task<string>` with the CSV representation.

### ExportAsXmlAsync\<T\>(IEnumerable\<T\> items)
Returns a `Task<string>` with the XML representation.

### ExportAsJsonLinesAsync\<T\>(IEnumerable\<T\> items)
Returns a `Task<string>` with one JSON object per line.

### ExportAsJsonLinesToFileAsync\<T\>(IEnumerable\<T\> items, string filePath)
Writes JSON Lines to a file, creating the parent directory if needed.

### ExportAsJsonLinesToStreamAsync\<T\>(IEnumerable\<T\> items, Stream stream)
Writes JSON Lines to a stream, leaving the stream open.

### ExportAsMarkdownAsync\<T\>(IEnumerable\<T\> items)
Returns a `Task<string>` with a Markdown table. Columns are the public instance
properties of `T`; `null` values become empty cells; literal `|` characters in
values are escaped as `\|`. Returns an empty string if `T` has no public
instance properties.

### ExportAsMarkdownToFileAsync\<T\>(IEnumerable\<T\> items, string filePath)
Writes the Markdown table to a file, creating the parent directory if needed.

### ExportToFileAsync\<T\>(IEnumerable\<T\> items, string filePath, ExportFormat format)
Writes the collection to a file in the given format. Returns `true` on success.
Returns `false` (and logs to stderr) if the path is not rooted or an exception
occurs. Only `Json`, `Csv`, and `Xml` are supported; any other value throws
`ArgumentException`.

### ExportToStreamAsync\<T\>(IEnumerable\<T\> items, Stream stream, ExportFormat format)
Writes the collection to a stream in the given format, leaving the stream open.
Only `Json`, `Csv`, and `Xml` are supported; any other value throws
`ArgumentException`.

### GenerateExportReport\<T\>(IEnumerable\<T\> items, string entityName)
Returns an `ExportReport` with `EntityName`, `ItemCount`, `ExportedAt`
(`DateTime.UtcNow`), `AvailableFormats` (`["json", "csv", "xml"]`), and a
`SampleItem` (the first item, or `null` for an empty collection). Its
`ToString()` renders a one-line summary.

## Examples

The examples below use a simple `Product` entity:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool InStock { get; set; }
}
```

### JSON

```csharp
var service = new DataExportService();
var products = new[]
{
    new Product { Id = 1, Name = "Widget", Price = 9.99m, InStock = true },
    new Product { Id = 2, Name = "Gadget", Price = 19.50m, InStock = false },
};

string json = await service.ExportAsJsonAsync(products);
Console.WriteLine(json);
```

```json
[
  {
    "id": 1,
    "name": "Widget",
    "price": 9.99,
    "inStock": true
  },
  {
    "id": 2,
    "name": "Gadget",
    "price": 19.5,
    "inStock": false
  }
]
```

### CSV

```csharp
string csv = await service.ExportAsCsvAsync(products);
Console.WriteLine(csv);
```

```csv
Id,Name,Price,InStock
1,Widget,9.99,True
2,Gadget,19.5,False
```

### XML

```csharp
string xml = await service.ExportAsXmlAsync(products);
Console.WriteLine(xml);
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <Product xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <Id>1</Id>
    <Name>Widget</Name>
    <Price>9.99</Price>
    <InStock>true</InStock>
  </Product>
  <Product xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <Id>2</Id>
    <Name>Gadget</Name>
    <Price>19.50</Price>
    <InStock>false</InStock>
  </Product>
</root>
```

### JSON Lines

```csharp
string jsonLines = await service.ExportAsJsonLinesAsync(products);
Console.WriteLine(jsonLines);
```

```json
{"id":1,"name":"Widget","price":9.99,"inStock":true}
{"id":2,"name":"Gadget","price":19.5,"inStock":false}
```

Write to a file (parent directories are created automatically):

```csharp
await service.ExportAsJsonLinesToFileAsync(products, "/tmp/exports/products.jsonl");
```

### Markdown

```csharp
string markdown = await service.ExportAsMarkdownAsync(products);
Console.WriteLine(markdown);
```

```markdown
| Id | Name | Price | InStock |
| --- | --- | --- | --- |
| 1 | Widget | 9.99 | True |
| 2 | Gadget | 19.5 | False |
```

Write to a file:

```csharp
await service.ExportAsMarkdownToFileAsync(products, "/tmp/exports/products.md");
```

### Generic file export (JSON / CSV / XML only)

```csharp
bool ok = await service.ExportToFileAsync(products, "/tmp/exports/products.json", ExportFormat.Json);
Console.WriteLine(ok ? "Exported" : "Export failed");
```

### Export report

```csharp
ExportReport report = service.GenerateExportReport(products, "Product");
Console.WriteLine(report.ToString());
// Exported 2 Product items in json, csv, xml format(s) at 2026-09-14T12:00:00.0000000Z
```

## Notes

- The generic `ExportToFileAsync` / `ExportToStreamAsync` overloads require a
  rooted file path and support only the `ExportFormat` enum values (`Json`,
  `Csv`, `Xml`). Use the dedicated Markdown / JSON Lines methods for those
  formats.
- `GenerateExportReport` reports `AvailableFormats` as `["json", "csv", "xml"]`;
  it does not currently list `markdown` or `jsonlines`.
- Markdown output reflects public instance properties in declaration order and
  escapes `|` inside cell values so the table stays well-formed.
- The file-based methods create missing parent directories before writing.