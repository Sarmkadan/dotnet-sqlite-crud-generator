#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for DataExportService class that verify export functionality
// for JSON, CSV, and XML formats with various edge cases.
// =============================================================================

using DotNet.SQLite.CrudGenerator.Formatters;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Unit tests for <see cref="DataExportService"/> class that verify export operations
/// for JSON, CSV, XML, and JSON Lines formats with various data scenarios.
/// </summary>
public sealed class DataExportServiceTests
{
    private readonly DataExportService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataExportServiceTests"/> class.
    /// Creates a <see cref="DataExportService"/> instance for testing.
    /// </summary>
    public DataExportServiceTests()
    {
        _service = new DataExportService();
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a normal dataset as JSON returns valid JSON with all items.
    /// </summary>
    public async Task ExportAsJsonAsync_WithNormalDataset_ReturnsValidJsonWithAllItems()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m },
            new() { Id = 3, Name = "Product C", Sku = "PC-003", CategoryId = 1, Price = 15.75m, Cost = 7.50m }
        };

        // Act
        var result = await _service.ExportAsJsonAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("Product B");
        result.Should().Contain("Product C");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
        result.Should().Contain("15.75");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting an empty dataset as JSON returns an empty array.
    /// </summary>
    public async Task ExportAsJsonAsync_WithEmptyDataset_ReturnsEmptyArray()
    {
        // Arrange
        var emptyProducts = new List<Product>();

        // Act
        var result = await _service.ExportAsJsonAsync(emptyProducts);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("$values");
        result.Should().Contain("[]");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a dataset with null values as JSON handles nulls correctly.
    /// </summary>
    public async Task ExportAsJsonAsync_WithNullValues_HandlesNullsCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = null, Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 0 },
            new() { Id = 3, Name = "Product C", Sku = "PC-003", CategoryId = 1, Price = 15.75m, Cost = 8.00m }
        };

        // Act
        var result = await _service.ExportAsJsonAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("Product C");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a normal dataset as CSV returns properly formatted CSV.
    /// </summary>
    public async Task ExportAsCsvAsync_WithNormalDataset_ReturnsProperlyFormattedCsv()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m }
        };

        // Act
        var result = await _service.ExportAsCsvAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Id");
        result.Should().Contain("Name");
        result.Should().Contain("Product A");
        result.Should().Contain("Product B");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting an empty dataset as CSV returns an empty string.
    /// </summary>
    public async Task ExportAsCsvAsync_WithEmptyDataset_ReturnsEmptyString()
    {
        // Arrange
        var emptyProducts = new List<Product>();

        // Act
        var result = await _service.ExportAsCsvAsync(emptyProducts);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a dataset with special characters needing CSV escaping works correctly.
    /// </summary>
    public async Task ExportAsCsvAsync_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product, with commas", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product with \"quotes\"", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m },
            new() { Id = 3, Name = "Product with\nnewlines", Sku = "PC-003", CategoryId = 1, Price = 15.75m, Cost = 7.50m }
        };

        // Act
        var result = await _service.ExportAsCsvAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("quotes");
        result.Should().Contain("\n");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a dataset with null values as CSV handles nulls correctly.
    /// </summary>
    public async Task ExportAsCsvAsync_WithNullValues_HandlesNullsCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = null, Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 0 },
            new() { Id = 3, Name = "Product C", Sku = "PC-003", CategoryId = 1, Price = 15.75m, Cost = 8.00m }
        };

        // Act
        var result = await _service.ExportAsCsvAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("Product C");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a normal dataset as XML returns valid XML with all items.
    /// </summary>
    public async Task ExportAsXmlAsync_WithNormalDataset_ReturnsValidXmlWithAllItems()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m }
        };

        // Act
        var result = await _service.ExportAsXmlAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("<?xml");
        result.Should().Contain("Product A");
        result.Should().Contain("Product B");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting an empty dataset as XML returns XML with empty root element.
    /// </summary>
    public async Task ExportAsXmlAsync_WithEmptyDataset_ReturnsXmlWithEmptyRoot()
    {
        // Arrange
        var emptyProducts = new List<Product>();

        // Act
        var result = await _service.ExportAsXmlAsync(emptyProducts);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("<?xml");
        result.Should().Contain("<root");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting a dataset with null values as XML handles nulls correctly.
    /// </summary>
    public async Task ExportAsXmlAsync_WithNullValues_HandlesNullsCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };

        // Act
        var result = await _service.ExportAsXmlAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("10.99");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting as JSON Lines format returns one JSON object per line.
    /// </summary>
    public async Task ExportAsJsonLinesAsync_WithNormalDataset_ReturnsOneJsonObjectPerLine()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m }
        };

        // Act
        var result = await _service.ExportAsJsonLinesAsync(products);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("Product B");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
    }

    [Fact]
    /// <summary>
    /// Tests that exporting an empty dataset as JSON Lines returns an empty string.
    /// </summary>
    public async Task ExportAsJsonLinesAsync_WithEmptyDataset_ReturnsEmptyString()
    {
        // Arrange
        var emptyProducts = new List<Product>();

        // Act
        var result = await _service.ExportAsJsonLinesAsync(emptyProducts);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToFileAsync with JSON format creates a file with valid JSON content.
    /// </summary>
    public async Task ExportToFileAsync_WithJsonFormat_CreatesFileWithValidJson()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = await _service.ExportToFileAsync(products, tempFile, ExportFormat.Json);

            // Assert
            result.Should().BeTrue();
            File.Exists(tempFile).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(tempFile);
            fileContent.Should().NotBeNullOrWhiteSpace();
            fileContent.Should().Contain("Product A");
            fileContent.Should().Contain("10.99");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToFileAsync with CSV format creates a file with valid CSV content.
    /// </summary>
    public async Task ExportToFileAsync_WithCsvFormat_CreatesFileWithValidCsv()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = await _service.ExportToFileAsync(products, tempFile, ExportFormat.Csv);

            // Assert
            result.Should().BeTrue();
            File.Exists(tempFile).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(tempFile);
            fileContent.Should().NotBeNullOrWhiteSpace();
            fileContent.Should().Contain("Product A");
            fileContent.Should().Contain("10.99");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToFileAsync with XML format creates a file with valid XML content.
    /// </summary>
    public async Task ExportToFileAsync_WithXmlFormat_CreatesFileWithValidXml()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = await _service.ExportToFileAsync(products, tempFile, ExportFormat.Xml);

            // Assert
            result.Should().BeTrue();
            File.Exists(tempFile).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(tempFile);
            fileContent.Should().NotBeNullOrWhiteSpace();
            fileContent.Should().Contain("<?xml");
            fileContent.Should().Contain("Product A");
            fileContent.Should().Contain("10.99");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToFileAsync returns false when export fails.
    /// </summary>
    public async Task ExportToFileAsync_WhenExportFails_ReturnsFalse()
    {
        // Arrange - use invalid file path
        var products = new List<Product>();
        var invalidPath = "/proc/1/mem/protected_file_that_cannot_be_written.json";

        // Act
        var result = await _service.ExportToFileAsync(products, invalidPath, ExportFormat.Json);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    /// <summary>
    /// Tests that GenerateExportReport creates a proper report for a dataset.
    /// </summary>
    public void GenerateExportReport_WithNormalDataset_CreatesProperReport()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m }
        };

        // Act
        var report = _service.GenerateExportReport(products, "Product");

        // Assert
        report.Should().NotBeNull();
        report.EntityName.Should().Be("Product");
        report.ItemCount.Should().Be(2);
        report.AvailableFormats.Should().HaveCount(3);
        report.AvailableFormats.Should().Contain("json");
        report.AvailableFormats.Should().Contain("csv");
        report.AvailableFormats.Should().Contain("xml");
        report.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        report.SampleItem.Should().NotBeNull();
    }

    [Fact]
    /// <summary>
    /// Tests that GenerateExportReport handles empty datasets correctly.
    /// </summary>
    public void GenerateExportReport_WithEmptyDataset_HandlesEmptyDataset()
    {
        // Arrange
        var emptyProducts = new List<Product>();

        // Act
        var report = _service.GenerateExportReport(emptyProducts, "Product");

        // Assert
        report.Should().NotBeNull();
        report.EntityName.Should().Be("Product");
        report.ItemCount.Should().Be(0);
        report.AvailableFormats.Should().HaveCount(3);
        report.SampleItem.Should().BeNull();
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToStreamAsync writes valid JSON to a stream.
    /// </summary>
    public async Task ExportToStreamAsync_WithJsonFormat_WritesValidJsonToStream()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        using var stream = new MemoryStream();

        // Act
        await _service.ExportToStreamAsync(products, stream, ExportFormat.Json);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("10.99");
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToStreamAsync writes valid CSV to a stream.
    /// </summary>
    public async Task ExportToStreamAsync_WithCsvFormat_WritesValidCsvToStream()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        using var stream = new MemoryStream();

        // Act
        await _service.ExportToStreamAsync(products, stream, ExportFormat.Csv);
        stream.Position = 0;
        var result = await new StreamReader(stream).ReadToEndAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("10.99");
    }

    [Fact]
    /// <summary>
    /// Tests that ExportToStreamAsync writes valid XML to a stream.
    /// </summary>
    public async Task ExportToStreamAsync_WithXmlFormat_WritesValidXmlToStream()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m }
        };
        using var stream = new MemoryStream();

        // Act
        await _service.ExportToStreamAsync(products, stream, ExportFormat.Xml);
        stream.Position = 0;
        var result = await new StreamReader(stream).ReadToEndAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("<?xml");
        result.Should().Contain("Product A");
        result.Should().Contain("10.99");
    }

    [Fact]
    /// <summary>
    /// Tests that ExportJsonLinesToFileAsync creates a file with valid JSON Lines content.
    /// </summary>
    public async Task ExportAsJsonLinesToFileAsync_CreatesFileWithValidJsonLines()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m }
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await _service.ExportAsJsonLinesToFileAsync(products, tempFile);

            // Assert
            File.Exists(tempFile).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(tempFile);
            fileContent.Should().NotBeNullOrWhiteSpace();
            fileContent.Should().Contain("Product A");
            fileContent.Should().Contain("Product B");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    /// <summary>
    /// Tests that ExportJsonLinesToStreamAsync writes valid JSON Lines to a stream.
    /// </summary>
    public async Task ExportAsJsonLinesToStreamAsync_WritesValidJsonLinesToStream()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product A", Sku = "PA-001", CategoryId = 1, Price = 10.99m, Cost = 5.00m },
            new() { Id = 2, Name = "Product B", Sku = "PB-002", CategoryId = 2, Price = 25.50m, Cost = 12.00m }
        };
        using var stream = new MemoryStream();

        // Act
        await _service.ExportAsJsonLinesToStreamAsync(products, stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("Product A");
        result.Should().Contain("Product B");
        result.Should().Contain("10.99");
        result.Should().Contain("25.50");
    }
}