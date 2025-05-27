// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Data.Sqlite;

namespace DotNet.SQLite.CrudGenerator.Data;

/// <summary>
/// Manages SQLite database connections with connection pooling support.
/// </summary>
public class DatabaseConnection : IAsyncDisposable, IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;
    private bool _disposed;

    public DatabaseConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _connectionString = connectionString;
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
            await Connection.OpenAsync(cancellationToken);
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
    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await OpenAsync(cancellationToken);

        using var command = Connection.CreateCommand();
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
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
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
                FOREIGN KEY (UserId) REFERENCES Users(Id)
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
        ";

        await command.ExecuteNonQueryAsync(cancellationToken);
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
        if (_connection != null)
            await _connection.DisposeAsync();

        _disposed = true;
    }
}
