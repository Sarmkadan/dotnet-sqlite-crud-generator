#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Manages SQLite database connections with connection pooling support.
/// </summary>
public sealed class DatabaseConnection : IAsyncDisposable, IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private bool _disposed;
    private readonly ILogger<DatabaseConnection>? _logger;

    public DatabaseConnection(string connectionString, ILogger<DatabaseConnection>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates the database connection.
    /// </summary>
    public SqliteConnection Connection
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseConnection));

            _connection ??= new SqliteConnection(_connectionString);
            return _connection;
        }
    }

    /// <summary>
    /// Opens the database connection asynchronously.
    /// </summary>
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (Connection.State != System.Data.ConnectionState.Open)
        {
            _logger?.LogInformation("Opening database connection to {DatabasePath}", _connectionString);
            await Connection.OpenAsync(cancellationToken);
            _logger?.LogDebug("Database connection opened successfully");
        }
    }

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    public void Close()
    {
        if (Connection.State != System.Data.ConnectionState.Closed)
            Connection.Close();
    }

    /// <summary>
    /// Ensures the database is initialized with required tables.
    /// </summary>
    public async Task InitializeDatabaseAsync(bool dropExistingTables = false, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Initializing database with tables");
        await OpenAsync(cancellationToken);

        using var command = Connection.CreateCommand();

        if (dropExistingTables)
        {
            _logger?.LogWarning("Dropping existing database tables");
            // Drop all tables in reverse order of creation to respect foreign key constraints
            command.CommandText = @"
                PRAGMA foreign_keys = OFF;
                DROP TABLE IF EXISTS AuditLogs;
                DROP TABLE IF EXISTS Orders;
                DROP TABLE IF EXISTS Products;
                DROP TABLE IF EXISTS Categories;
                DROP TABLE IF EXISTS Users;
                PRAGMA foreign_keys = ON;
            ";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                PhoneNumber TEXT,
                Bio TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                EmailVerified INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                LastLoginAt TEXT
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                ParentCategoryId INTEGER,
                Slug TEXT,
                DisplayOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Sku TEXT NOT NULL UNIQUE,
                CategoryId INTEGER NOT NULL,
                Price REAL NOT NULL,
                Cost REAL NOT NULL,
                StockQuantity INTEGER NOT NULL DEFAULT 0,
                ReorderLevel INTEGER NOT NULL DEFAULT 10,
                Unit TEXT NOT NULL DEFAULT 'piece',
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                OrderNumber TEXT NOT NULL UNIQUE,
                Status INTEGER NOT NULL DEFAULT 0,
                TotalAmount REAL NOT NULL,
                TaxAmount REAL NOT NULL DEFAULT 0,
                DiscountAmount REAL NOT NULL DEFAULT 0,
                Notes TEXT,
                ShippingAddress TEXT,
                BillingAddress TEXT,
                ItemCount INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                ShippedAt TEXT,
                DeliveredAt TEXT,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EntityType TEXT NOT NULL,
                EntityId INTEGER NOT NULL,
                OperationType INTEGER NOT NULL,
                ChangedByUserId INTEGER NOT NULL,
                OldValues TEXT,
                NewValues TEXT,
                ChangeReason TEXT,
                IpAddress TEXT,
                Timestamp TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_Users_Email ON Users(Email);
            CREATE INDEX IF NOT EXISTS idx_Users_Username ON Users(Username);
            CREATE INDEX IF NOT EXISTS idx_Products_CategoryId ON Products(CategoryId);
            CREATE INDEX IF NOT EXISTS idx_Products_Sku ON Products(Sku);
            CREATE INDEX IF NOT EXISTS idx_Orders_UserId ON Orders(UserId);
            CREATE INDEX IF NOT EXISTS idx_Orders_OrderNumber ON Orders(OrderNumber);
            CREATE INDEX IF NOT EXISTS idx_Orders_Status ON Orders(Status);
            CREATE INDEX IF NOT EXISTS idx_AuditLogs_EntityType ON AuditLogs(EntityType);
            CREATE INDEX IF NOT EXISTS idx_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IF NOT EXISTS idx_AuditLogs_EntityType_Timestamp ON AuditLogs(EntityType, Timestamp DESC);
CREATE INDEX IF NOT EXISTS idx_AuditLogs_Timestamp_Id ON AuditLogs(Timestamp DESC, Id DESC);
        ";

        await command.ExecuteNonQueryAsync(cancellationToken);
            _logger?.LogInformation("Database tables created successfully");
    }

    /// <summary>
    /// Convenience alias for <see cref="InitializeDatabaseAsync"/> matching the naming
    /// convention used by callers that treat the connection as a lightweight unit of work.
    /// </summary>
    public Task InitializeAsync(bool dropExistingTables = false, CancellationToken cancellationToken = default)
        => InitializeDatabaseAsync(dropExistingTables, cancellationToken);

    /// <summary>
    /// Compatibility method for callers using a unit-of-work style pattern. Repository
    /// operations on this connection are committed immediately, so there are no pending
    /// changes to flush; this simply ensures the connection is open.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await OpenAsync(cancellationToken);
        return 0;
    }

    /// <summary>
    /// Disposes the database connection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Close();
        _connection?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Asynchronously disposes the database connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        Close();
        if (_connection is not null)
            await _connection.DisposeAsync();

        _disposed = true;
    }
}
