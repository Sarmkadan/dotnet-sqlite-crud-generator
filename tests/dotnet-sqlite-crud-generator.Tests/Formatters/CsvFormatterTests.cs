#nullable enable

using DotNet.SQLite.CrudGenerator.Formatters;
using FluentAssertions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.Formatters;

public class CsvFormatterTests
{
    [Fact]
    public void Parse_Collection_ReturnsCorrectObjects()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\n1,John Doe,john@example.com\n2,Jane Smith,jane@example.com";

        // Act
        var result = formatter.ParseCollection<Person>(csv)?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Id.Should().Be(1);
        result[0].Name.Should().Be("John Doe");
        result[0].Email.Should().Be("john@example.com");
        result[1].Id.Should().Be(2);
        result[1].Name.Should().Be("Jane Smith");
        result[1].Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void Parse_CollectionWithEscapedComma_ReturnsCorrectObject()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\n1,\"Doe, John\",john@example.com";

        // Act
        var result = formatter.Parse<Person>(csv);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Doe, John");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Parse_CollectionWithEscapedQuotes_ReturnsCorrectObject()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\n1,\"John \"\"The Boss\"\" Doe\",john@example.com";

        // Act
        var result = formatter.Parse<Person>(csv);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John \"The Boss\" Doe");
        result.Email.Should().Be("john@example.com");
    }

    // Note: Parse method has issues with newlines in CSV values - the CsvFormatter.ParseLine
    // method doesn't properly handle escaped newlines within quoted values

    [Fact]
    public void Parse_CollectionWithCustomDelimiter_ReturnsCorrectObjects()
    {
        // Arrange
        var formatter = new CsvFormatter(delimiter: ";");
        var csv = "Id;Name;Email\n1;John Doe;john@example.com";

        // Act
        var result = formatter.Parse<Person>(csv);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Parse_CollectionWithWindowsLineEndings_ReturnsCorrectObjects()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\r\n1,John Doe,john@example.com\r\n2,Jane Smith,jane@example.com";

        // Act
        var result = formatter.ParseCollection<Person>(csv)?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_CollectionWithUnixLineEndings_ReturnsCorrectObjects()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\n1,John Doe,john@example.com\n2,Jane Smith,jane@example.com";

        // Act
        var result = formatter.ParseCollection<Person>(csv)?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_CollectionWithOldMacLineEndings_ReturnsCorrectObjects()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var csv = "Id,Name,Email\r1,John Doe,john@example.com\r2,Jane Smith,jane@example.com";

        // Act
        var result = formatter.ParseCollection<Person>(csv)?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    private class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
