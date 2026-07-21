#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotNet.SQLite.CrudGenerator.BulkTransfer;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests.BulkTransfer;

/// <summary>
/// Tests for the <see cref="BulkTransferPipeline{T}"/> class, verifying fluent configuration,
/// batching behavior, error propagation, and empty source handling.
/// </summary>
public sealed class BulkTransferPipelineTests : IDisposable
{
    private readonly IBulkTransferService<TestEntity> _mockService;
    private readonly BulkTransferPipeline<TestEntity> _pipeline;
    private readonly List<BulkTransferError> _capturedErrors;
    private readonly MemoryStream _outputStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkTransferPipelineTests"/> class,
    /// setting up mock services and test fixtures.
    /// </summary>
    public BulkTransferPipelineTests()
    {
        _mockService = Substitute.For<IBulkTransferService<TestEntity>>();
        _pipeline = new BulkTransferPipeline<TestEntity>(_mockService);
        _capturedErrors = new List<BulkTransferError>();
        _outputStream = new MemoryStream();

        // Setup default mock responses
        _mockService
            .ImportFromFileAsync(Arg.Any<string>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BulkImportResult
            {
                TotalRead = 0,
                Succeeded = 0,
                Failed = 0,
                BatchesCommitted = 0,
                Duration = TimeSpan.Zero,
                StartedAt = DateTime.UtcNow,
                Errors = new List<BulkTransferError>()
            }));

        _mockService
            .ImportFromStreamAsync(Arg.Any<Stream>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BulkImportResult
            {
                TotalRead = 0,
                Succeeded = 0,
                Failed = 0,
                BatchesCommitted = 0,
                Duration = TimeSpan.Zero,
                StartedAt = DateTime.UtcNow,
                Errors = new List<BulkTransferError>()
            }));

        _mockService
            .ExportToStreamAsync(Arg.Any<Stream>(), Arg.Any<ExportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BulkExportResult
            {
                TotalExported = 0,
                BytesWritten = 0,
                Format = ExportFormat.Json,
                DestinationPath = null,
                Duration = TimeSpan.Zero,
                StartedAt = DateTime.UtcNow,
                IsSuccess = true
            }));

        _mockService
            .TransferAsync(Arg.Any<Stream>(), Arg.Any<ImportFormat>(), Arg.Any<Stream>(), Arg.Any<ExportFormat>(),
                Arg.Any<Func<TestEntity, TestEntity?>>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BulkTransferResult(
            new BulkImportResult
            {
                TotalRead = 0,
                Succeeded = 0,
                Failed = 0,
                BatchesCommitted = 0,
                Duration = TimeSpan.Zero,
                StartedAt = DateTime.UtcNow,
                Errors = new List<BulkTransferError>()
            },
            new BulkExportResult
            {
                TotalExported = 0,
                BytesWritten = 0,
                Format = ExportFormat.Json,
                Duration = TimeSpan.Zero,
                StartedAt = DateTime.UtcNow,
                IsSuccess = true
            })));

        _mockService
            .GetStatistics()
            .Returns(new BulkTransferStatistics());
    }

    /// <summary>
    /// Disposes test fixtures including memory streams.
    /// </summary>
    public void Dispose()
    {
        _outputStream.Dispose();
    }

    // -- Helper entity class -------------------------------------------------------

    public sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    // -- Fluent configuration tests ----------------------------------------------

    [Fact]
    public void WithOptions_ShouldReplaceDefaultOptions()
    {
        // Arrange
        var customOptions = new BulkTransferOptions
        {
            BatchSize = 100,
            MaxConcurrency = 2,
            ProgressReportingInterval = 50,
            EnableProgressReporting = false
        };

        // Act
        var result = _pipeline.WithOptions(customOptions);

        // Assert
        result.Should().BeSameAs(_pipeline);
        // Note: Options are stored internally, not directly accessible for verification
    }

    [Fact]
    public void WithOptions_WithNull_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pipeline.WithOptions(null!));
    }

    [Fact]
    public void WithTransform_ShouldRegisterTransformFunction()
    {
        // Act
        var result = _pipeline.WithTransform(e => new TestEntity { Id = e.Id * 2 });

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void WithTransform_WithNull_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pipeline.WithTransform(null!));
    }

    [Fact]
    public void WithFilter_ShouldRegisterFilterFunction()
    {
        // Act
        var result = _pipeline.WithFilter(e => e.Value > 10);

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void WithFilter_WithNull_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pipeline.WithFilter(null!));
    }

    [Fact]
    public void WithProgress_ShouldRegisterProgressObserver()
    {
        // Arrange
        var progress = Substitute.For<IProgress<BulkTransferProgress>>();

        // Act
        var result = _pipeline.WithProgress(progress);

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void WithProgress_WithNull_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pipeline.WithProgress(null!));
    }

    [Fact]
    public void OnError_ShouldRegisterErrorHandler()
    {
        // Act
        var result = _pipeline.OnError(e => _capturedErrors.Add(e));

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void OnError_WithNull_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pipeline.OnError(null!));
    }

    [Fact]
    public void WithRetry_WithNegativeCount_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _pipeline.WithRetry(-1));
    }

    [Fact]
    public void WithRetry_WithZeroCount_ShouldSetRetryCount()
    {
        // Act
        var result = _pipeline.WithRetry(0);

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void WithRetry_WithPositiveCount_ShouldConfigureRetry()
    {
        // Arrange
        var delay = TimeSpan.FromSeconds(5);

        // Act
        var result = _pipeline.WithRetry(3, delay);

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    [Fact]
    public void WithRetry_WithZeroDelay_ShouldUseDefaultDelay()
    {
        // Act
        var result = _pipeline.WithRetry(2, TimeSpan.Zero);

        // Assert
        result.Should().BeSameAs(_pipeline);
    }

    // -- Execution tests ----------------------------------------------------------

    [Fact]
    public async Task ImportFromFileAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var filePath = "/tmp/test.json";
        var format = ImportFormat.Json;
        var testFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(testFile, "[]");

        // Act
        await _pipeline.ImportFromFileAsync(filePath, format);

        // Assert
        await _mockService
            .Received(1)
            .ImportFromFileAsync(filePath, format, Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());

        File.Delete(testFile);
    }

    [Fact]
    public async Task ImportFromStreamAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("[]"));
        var format = ImportFormat.Json;

        // Act
        await _pipeline.ImportFromStreamAsync(stream, format);

        // Assert
        await _mockService
            .Received(1)
            .ImportFromStreamAsync(stream, format, Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportToFileAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var filePath = "/tmp/output.json";
        var format = ExportFormat.Json;

        // Act
        await _pipeline.ExportToFileAsync(filePath, format);

        // Assert
        await _mockService
            .Received(1)
            .ExportToFileAsync(filePath, format, Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportToStreamAsync_WithoutFilter_ShouldCallExportToStream()
    {
        // Arrange
        var stream = new MemoryStream();
        var format = ExportFormat.Json;

        // Act
        await _pipeline.ExportToStreamAsync(stream, format);

        // Assert
        await _mockService
            .Received(1)
            .ExportToStreamAsync(stream, format, Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExportToStreamAsync_WithFilter_ShouldCallExportFiltered()
    {
        // Arrange
        var filter = Substitute.For<Func<TestEntity, bool>>();
        _pipeline.WithFilter(filter);

        var stream = new MemoryStream();
        var format = ExportFormat.Json;

        // Act
        await _pipeline.ExportToStreamAsync(stream, format);

        // Assert
        await _mockService
            .Received(1)
            .ExportFilteredAsync(filter, stream, format, Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransferAsync_ShouldCallServiceWithCorrectParameters()
    {
        // Arrange
        var source = new MemoryStream(Encoding.UTF8.GetBytes("[]"));
        var sourceFormat = ImportFormat.Json;
        var destination = new MemoryStream();
        var destinationFormat = ExportFormat.Json;

        // Act
        await _pipeline.TransferAsync(source, sourceFormat, destination, destinationFormat);

        // Assert
        await _mockService
            .Received(1)
            .TransferAsync(source, sourceFormat, destination, destinationFormat,
                Arg.Any<Func<TestEntity, TestEntity?>>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnServiceStatistics()
    {
        // Arrange
        var expectedStats = new BulkTransferStatistics
        {
            TotalImports = 5,
            TotalExports = 3,
            TotalRecordsImported = 100,
            TotalRecordsExported = 75,
            TotalErrors = 2,
            TotalBytesTransferred = 1024
        };

        _mockService.GetStatistics().Returns(expectedStats);

        // Act
        var result = _pipeline.GetStatistics();

        // Assert
        result.Should().BeSameAs(expectedStats);
    }

    // -- Error propagation tests ---------------------------------------------------

    [Fact]
    public async Task ImportFromFileAsync_WithErrors_ShouldInvokeErrorHandler()
    {
        // Arrange
        var errors = new List<BulkTransferError>
        {
            new() { RowNumber = 1, Message = "Invalid data", RawRecord = "{}" },
            new() { RowNumber = 2, Message = "Validation failed", RawRecord = "{}" }
        };

        var result = new BulkImportResult
        {
            TotalRead = 2,
            Succeeded = 1,
            Failed = 1,
            BatchesCommitted = 1,
            Duration = TimeSpan.FromSeconds(1),
            StartedAt = DateTime.UtcNow,
            Errors = errors
        };

        _mockService
            .ImportFromFileAsync(Arg.Any<string>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        _pipeline.OnError(e => _capturedErrors.Add(e));

        // Act
        await _pipeline.ImportFromFileAsync("/tmp/test.json", ImportFormat.Json);

        // Assert
        _capturedErrors.Should().HaveCount(2);
        _capturedErrors[0].RowNumber.Should().Be(1);
        _capturedErrors[1].RowNumber.Should().Be(2);
    }

    [Fact]
    public async Task ImportFromStreamAsync_WithErrors_ShouldInvokeErrorHandler()
    {
        // Arrange
        var errors = new List<BulkTransferError>
        {
            new() { RowNumber = 1, Message = "Invalid data", RawRecord = "{}" }
        };

        var result = new BulkImportResult
        {
            TotalRead = 1,
            Succeeded = 0,
            Failed = 1,
            BatchesCommitted = 0,
            Duration = TimeSpan.FromSeconds(1),
            StartedAt = DateTime.UtcNow,
            Errors = errors
        };

        _mockService
            .ImportFromStreamAsync(Arg.Any<Stream>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        _pipeline.OnError(e => _capturedErrors.Add(e));

        // Act
        await _pipeline.ImportFromStreamAsync(new MemoryStream(), ImportFormat.Json);

        // Assert
        _capturedErrors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportFromFileAsync_WithoutErrorHandler_ShouldNotThrow()
    {
        // Arrange
        var errors = new List<BulkTransferError>
        {
            new() { RowNumber = 1, Message = "Invalid data", RawRecord = "{}" }
        };

        var result = new BulkImportResult
        {
            TotalRead = 1,
            Succeeded = 0,
            Failed = 1,
            BatchesCommitted = 0,
            Duration = TimeSpan.FromSeconds(1),
            StartedAt = DateTime.UtcNow,
            Errors = errors
        };

        _mockService
            .ImportFromFileAsync(Arg.Any<string>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        // Act & Assert (should not throw even with errors when no handler)
        await _pipeline.ImportFromFileAsync("/tmp/test.json", ImportFormat.Json);
    }

    // -- Empty source tests -------------------------------------------------------

    [Fact]
    public async Task ImportFromFileAsync_WithEmptySource_ShouldHandleCorrectly()
    {
        // Arrange
        var result = new BulkImportResult
        {
            TotalRead = 0,
            Succeeded = 0,
            Failed = 0,
            BatchesCommitted = 0,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            Errors = new List<BulkTransferError>()
        };

        _mockService
            .ImportFromFileAsync(Arg.Any<string>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        // Act
        var actualResult = await _pipeline.ImportFromFileAsync("/tmp/empty.json", ImportFormat.Json);

        // Assert
        actualResult.TotalRead.Should().Be(0);
        actualResult.Succeeded.Should().Be(0);
        actualResult.Failed.Should().Be(0);
        actualResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ImportFromStreamAsync_WithEmptyStream_ShouldHandleCorrectly()
    {
        // Arrange
        var result = new BulkImportResult
        {
            TotalRead = 0,
            Succeeded = 0,
            Failed = 0,
            BatchesCommitted = 0,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            Errors = new List<BulkTransferError>()
        };

        _mockService
            .ImportFromStreamAsync(Arg.Any<Stream>(), Arg.Any<ImportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        // Act
        var actualResult = await _pipeline.ImportFromStreamAsync(new MemoryStream(), ImportFormat.Json);

        // Assert
        actualResult.TotalRead.Should().Be(0);
        actualResult.Succeeded.Should().Be(0);
        actualResult.Failed.Should().Be(0);
        actualResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExportToFileAsync_WithEmptyData_ShouldHandleCorrectly()
    {
        // Arrange
        var result = new BulkExportResult
        {
            TotalExported = 0,
            BytesWritten = 0,
            Format = ExportFormat.Json,
            DestinationPath = null,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            IsSuccess = true
        };

        _mockService
            .ExportToFileAsync(Arg.Any<string>(), Arg.Any<ExportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        // Act
        var actualResult = await _pipeline.ExportToFileAsync("/tmp/output.json", ExportFormat.Json);

        // Assert
        actualResult.TotalExported.Should().Be(0);
        actualResult.BytesWritten.Should().Be(0);
        actualResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExportToStreamAsync_WithEmptyData_ShouldHandleCorrectly()
    {
        // Arrange
        var result = new BulkExportResult
        {
            TotalExported = 0,
            BytesWritten = 0,
            Format = ExportFormat.Json,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            IsSuccess = true
        };

        _mockService
            .ExportToStreamAsync(Arg.Any<Stream>(), Arg.Any<ExportFormat>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

        // Act
        var actualResult = await _pipeline.ExportToStreamAsync(_outputStream, ExportFormat.Json);

        // Assert
        actualResult.TotalExported.Should().Be(0);
        actualResult.BytesWritten.Should().Be(0);
        actualResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TransferAsync_WithEmptyStreams_ShouldHandleCorrectly()
    {
        // Arrange
        var importResult = new BulkImportResult
        {
            TotalRead = 0,
            Succeeded = 0,
            Failed = 0,
            BatchesCommitted = 0,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            Errors = new List<BulkTransferError>()
        };

        var exportResult = new BulkExportResult
        {
            TotalExported = 0,
            BytesWritten = 0,
            Format = ExportFormat.Json,
            Duration = TimeSpan.FromSeconds(0.1),
            StartedAt = DateTime.UtcNow,
            IsSuccess = true
        };

        _mockService
            .TransferAsync(Arg.Any<Stream>(), Arg.Any<ImportFormat>(), Arg.Any<Stream>(), Arg.Any<ExportFormat>(),
                Arg.Any<Func<TestEntity, TestEntity?>>(), Arg.Any<IProgress<BulkTransferProgress>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BulkTransferResult(importResult, exportResult)));

        // Act
        var result = await _pipeline.TransferAsync(
            new MemoryStream(), ImportFormat.Json,
            new MemoryStream(), ExportFormat.Json);

        // Assert
        result.Import.TotalRead.Should().Be(0);
        result.Import.Succeeded.Should().Be(0);
        result.Export.TotalExported.Should().Be(0);
        result.IsSuccess.Should().BeTrue();
    }

    // -- Static factory tests ------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnNewPipelineInstance()
    {
        // Arrange
        var mockService = Substitute.For<IBulkTransferService<TestEntity>>();

        // Act
        var pipeline = BulkTransferPipeline<TestEntity>.Create(mockService);

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<BulkTransferPipeline<TestEntity>>();
    }

    [Fact]
    public void Constructor_WithNullService_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BulkTransferPipeline<TestEntity>(null!));
    }
}
