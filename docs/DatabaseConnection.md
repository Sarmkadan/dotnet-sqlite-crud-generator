# DatabaseConnection

Provides a managed connection to a SQLite database, including asynchronous initialization, opening/closing semantics, and change-tracking support for entity persistence.

## API

### `public DatabaseConnection`

Initializes a new instance of the `DatabaseConnection` class. The connection is not immediately opened; call `OpenAsync` or `InitializeAsync` to establish a database session.

### `public async Task OpenAsync`

Asynchronously opens the database connection if it is not already open. This method must be called before performing any database operations.

### `public void Close`

Closes the database connection if it is open. Any pending changes or open transactions are discarded. Subsequent operations will require reopening the connection.

### `public async Task InitializeDatabaseAsync`

Asynchronously initializes the database schema, creating tables and indexes as required by the application’s entity model. This method should be called once per database lifecycle, typically during application startup.

### `public Task InitializeAsync`

Asynchronously prepares the connection for use, performing any necessary initialization steps such as schema validation or connection setup. This method is idempotent and safe to call multiple times.

### `public async Task<int> SaveChangesAsync`

Asynchronously persists all pending changes to the database and returns the total number of affected rows across all operations. This method commits the current transaction and begins a new one.

### `public void Dispose`

Releases all resources used by the `DatabaseConnection`, including the underlying SQLite connection and any change tracking state. If the connection is open, it is closed first. This method is safe to call multiple times.

### `public async ValueTask DisposeAsync`

Asynchronously releases all resources used by the `DatabaseConnection`, including the underlying SQLite connection and any change tracking state. If the connection is open, it is closed first. This method is safe to call multiple times and preferred in asynchronous contexts.

## Usage
