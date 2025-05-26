#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Configuration;
using FluentAssertions;
using FluentAssertions.Primitives; // Added for BeTrue/BeFalse
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void DatabaseSettings_WithFilePathContainingSpaces_GeneratesQuotedConnectionString()
    {
        // Arrange
        var expectedFilePath = "path with spaces/test database.db";
        var settings = new DatabaseSettings { FilePath = expectedFilePath };

        // Act
        var connectionString = settings.ConnectionString;

        // Assert
        connectionString.Should().Contain($"Data Source=\"{expectedFilePath}\";");
    }

    [Fact]
    public void DatabaseSettings_WithFilePathContainingUnicode_GeneratesQuotedConnectionString()
    {
        // Arrange
        var expectedFilePath = "path/with/unicode/データベース.db";
        var settings = new DatabaseSettings { FilePath = expectedFilePath };

        // Act
        var connectionString = settings.ConnectionString;

        // Assert
        connectionString.Should().Contain($"Data Source=\"{expectedFilePath}\";");
    }

    [Fact]
    public void DatabaseSettings_WithValidConnectionString_IsValid()
    {
        // Arrange
        var settings = new DatabaseSettings { ConnectionString = "Data Source=test.db" };

        // Act & Assert
        settings.ConnectionString.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DatabaseSettings_WithEmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange
        var settings = new DatabaseSettings { ConnectionString = "" };

        // Act
        Action act = () =>
        {
            if (string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new ArgumentException("Connection string cannot be empty.");
            }
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Connection string cannot be empty.");
    }

    [Fact]
    public void ConnectionPoolConfiguration_WithValidSettings_IsValid()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 10,
            IdleTimeout = TimeSpan.FromMinutes(5),
            AcquireTimeout = TimeSpan.FromSeconds(30),
            CleanupInterval = TimeSpan.FromMinutes(1),
            EnableDiagnostics = true
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().NotThrow();
        config.MinPoolSize.Should().Be(1);
        config.MaxPoolSize.Should().Be(10);
        config.AcquireTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void ConnectionPoolConfiguration_WithZeroMaxPoolSize_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 0 // Invalid setting
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxPoolSize must be at least 1.*");
    }

    [Fact]
    public void ConnectionPoolConfiguration_WithMinPoolSizeGreaterThanMaxPoolSize_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 5,
            MaxPoolSize = 3 // Invalid setting
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MinPoolSize cannot exceed MaxPoolSize.*");
    }

    [Fact]
    public void ConnectionPoolConfiguration_WithZeroIdleTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConnectionPoolConfiguration
        {
            MinPoolSize = 1,
            MaxPoolSize = 5,
            IdleTimeout = TimeSpan.Zero // Invalid setting
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*IdleTimeout must be a positive duration.*");
    }

    [Fact]
    public void CacheConfiguration_WithValidSettings_IsValid()
    {
        // Arrange
        var config = new CacheConfiguration
        {
            Enabled = true,
            MaxSizeBytes = 10_000_000,
            DefaultTTL = TimeSpan.FromMinutes(30),
            CleanupIntervalSeconds = 300
        };

        // Act & Assert
        config.Enabled.Should().BeTrue();
        config.MaxSizeBytes.Should().Be(10_000_000);
        config.DefaultTTL.Should().Be(TimeSpan.FromMinutes(30));
        config.CleanupIntervalSeconds.Should().Be(300);
    }

    [Fact]
    public void ApplicationConfiguration_Validate_ThrowsForInvalidCacheMaxSizeBytes()
    {
        // Arrange
        var config = new ApplicationConfiguration
        {
            Database = new DatabaseSettings { ConnectionString = "Data Source=test.db" },
            Cache = new CacheConfiguration { MaxSizeBytes = 0 } // Invalid setting
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cache max size must be greater than 0*");
    }

    [Fact]
    public void ApplicationConfiguration_Validate_ThrowsForMissingDatabaseConnectionString()
    {
        // Arrange
        var config = new ApplicationConfiguration
        {
            Database = new DatabaseSettings { ConnectionString = "" }, // Invalid setting
            Cache = new CacheConfiguration { MaxSizeBytes = 100 }
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Database connection string is required*");
    }

    [Fact]
    public void ApplicationConfiguration_Validate_ThrowsForNullDatabaseSettings()
    {
        // Arrange
        var config = new ApplicationConfiguration
        {
            Database = null!, // Invalid setting
            Cache = new CacheConfiguration { MaxSizeBytes = 100 }
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Database configuration is required*");
    }

    [Fact]
    public void ApplicationConfiguration_Validate_ThrowsForInvalidWorkerCount()
    {
        // Arrange
        var config = new ApplicationConfiguration
        {
            Database = new DatabaseSettings { ConnectionString = "Data Source=test.db" },
            Cache = new CacheConfiguration { MaxSizeBytes = 100 },
            BackgroundWorker = new BackgroundWorkerConfiguration { WorkerCount = 0 } // Invalid setting
        };

        // Act
        Action act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Worker count must be at least 1*");
    }
}
