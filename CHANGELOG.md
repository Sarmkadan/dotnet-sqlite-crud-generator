// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-21

### Fixed
- Fix connection string parsing when path contains spaces or unicode characters
- Added regression test for the fix

## [2.0.1] - 2026-05-20

### Fixed
- Fix connection string parsing when path contains spaces or unicode characters
- Added regression test for the fix

## [2.0.0] - 2026-03-09

### Added
- Production-ready release with full feature set
- BenchmarkDotNet micro-benchmark suite for string extensions, cache operations, and naming conventions
- NuGet package metadata and packaging configuration (`Zaiets.dotnet.sqlite.crud.generator`)
- Docker multi-stage build and Docker Compose orchestration
- GitHub Actions CI/CD pipeline, CodeQL security scanning, and Dependabot configuration
- Comprehensive documentation: architecture overview, API reference, configuration reference, deployment guide, FAQ
- Seven usage examples covering CRUD, transactions, pagination, export, error handling, events, and advanced patterns
- Makefile targets for build, test, clean, and Docker tasks
- `.editorconfig` enforcing consistent code style

### Changed
- Hardened input validation across CLI commands and repository layer
- Optimised hot paths: `ToSnakeCase`/`ToKebabCase` rewritten with `Span<char>` (zero regex), cache hit paths return `ValueTask.FromResult`
- Repository `FindAsync` predicate compiled and cached to avoid repeated expression-tree traversal

### Fixed
- Potential null-reference dereference in `NamingConventionHelper.GetConventionInfo` when a property has no custom attributes
- Off-by-one in `StringExtensions.Repeat` for single-character inputs
- `MemoryCacheProvider.GetOrSetAsync` race condition under concurrent access resolved with lock-free `ConcurrentDictionary.GetOrAdd`

### Security
- All SQL statements use parameterised queries; raw string interpolation removed from query builder
- `RateLimitingMiddleware` enforces configurable per-IP request limits to prevent abuse

---

## [0.9.0] - 2025-09-10

### Added
- `DataExportService` with `JsonFormatter`, `CsvFormatter`, and `XmlFormatter`
- `ExternalApiClient` and `HttpClientFactory` for outbound HTTP integration
- `WebhookHandler` for publishing entity-change payloads to external endpoints
- `BackgroundTaskQueue` and `BackgroundWorkerService` for fire-and-forget async tasks
- `PerformanceMonitor` utility for lightweight wall-clock measurement

### Changed
- `DependencyInjection.AddApplicationServices` now registers export, integration, and background services automatically
- `GenerationService` reports progress via `IProgress<string>` instead of direct `Console.Write`

### Fixed
- `CsvFormatter` escaped commas and double-quotes incorrectly for string fields containing both characters
- `WebhookHandler` did not propagate `CancellationToken` to the underlying `HttpClient` call

---

## [0.8.0] - 2025-07-22

### Added
- `ErrorHandlingMiddleware` with structured JSON error responses and correlation IDs
- `LoggingMiddleware` capturing request method, path, status code, and elapsed milliseconds
- `ValidationMiddleware` running `DataAnnotations` validation before service calls
- `RateLimitingMiddleware` with configurable sliding-window token bucket
- `MemoryCacheProvider` with TTL and sliding-expiration support
- `CacheConfiguration` and `ConnectionPoolConfiguration` settings classes

### Changed
- `Repository<T>` base class uses the shared `ConnectionPool` instead of opening raw connections per call
- `DatabaseSettings` validation now rejects empty or whitespace-only file paths at startup

### Fixed
- `ConnectionPool` did not honour `MaxPoolSize` under high concurrency; replaced `List` with `ConcurrentBag`

---

## [0.7.0] - 2025-06-04

### Added
- `EventBus` publish/subscribe implementation for decoupled entity-change notifications
- `EntityChangedEvent` carrying entity name, operation type, entity ID, and timestamp
- `AuditHelper` automating audit-log entry creation on Create/Update/Delete
- `OperationType` enum (`Create`, `Read`, `Update`, `Delete`)
- `EntityStatus` enum (`Active`, `Inactive`, `Deleted`) used by soft-delete logic

### Changed
- `UserService`, `ProductService`, and `OrderService` publish `EntityChangedEvent` after each mutating operation
- `Repository.DeleteAsync` now performs a soft delete (sets `Status = Deleted`, `UpdatedAt = UtcNow`) rather than issuing `DELETE`

---

## [0.6.0] - 2025-04-29

### Added
- CLI command infrastructure: `CommandParser`, `GenerateCommand`, `MigrateCommand`, `ValidateCommand`, `ListCommand`, `StatsCommand`
- `GenerationService` producing migration SQL scripts and Protocol Buffer definitions from reflected model metadata
- `GenerateGrpcAttribute` marking entities for `.proto` output
- gRPC dependencies: `Grpc.AspNetCore`, `Grpc.Tools`
- `ReflectionHelper` and `TypeExtensions` utilities supporting the code-generation pipeline

### Changed
- `Program.cs` bootstraps the CLI dispatcher before the DI container, enabling `--help` without database initialisation

### Fixed
- `MigrateCommand` failed silently when the target migration name contained uppercase letters; now normalised before lookup

---

## [0.5.0] - 2025-03-18

### Added
- `OrderService` with `GetUserOrdersAsync` and `GetUserTotalSpentAsync`
- `AuditLog` model and `AuditLog` table schema
- `GetPagedAsync(pageNumber, pageSize)` on all services returning `(List<T>, int totalCount)`
- `FindAsync(Expression<Func<T, bool>>)` LINQ predicate support in `Repository<T>`
- `IUnitOfWork` interface and `DbContextProvider` implementation coordinating multi-repository transactions
- `DateTimeExtensions` (`ToIso8601`, `IsWeekend`, `StartOfWeek`) and `FileSystemExtensions`

### Changed
- `UserService.CreateAsync` validates `Username` uniqueness before insert
- `ProductService.GetByPriceRangeAsync` replaced raw SQL with an in-memory LINQ filter until indexed column support lands

### Fixed
- `Repository.GetAllAsync` returned stale data when called within the same transaction scope as a preceding insert

---

## [0.4.0] - 2025-02-24

### Added
- `UserService`, `ProductService` with business-logic methods (`AuthenticateAsync`, `CalculateTotalValueAsync`, `GetByPriceRangeAsync`)
- `IService<T>` interface defining the standard service contract
- `NamingConventionHelper` mapping C# property names to SQL column names and REST endpoint segments
- `StringExtensions`: `ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`, `Pluralize`, `ToSlug`
- `GenerationException`, `RepositoryException`, `ValidationException` typed exception hierarchy

### Changed
- Services receive `IRepository<T>` via constructor injection rather than instantiating concrete types
- `DatabaseConnection.InitializeSchema` creates indexes on `Users.Email`, `Products.CategoryId`, `Orders.UserId`

---

## [0.3.0] - 2025-02-06

### Added
- `Category` and `Order` domain models with full schema
- `IRepository<T>` generic interface (`AddAsync`, `GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, `DeleteAsync`)
- `Repository<T>` base implementation backed by `Microsoft.Data.Sqlite`
- `DbContextProvider` (Unit of Work skeleton) wrapping shared connection and transaction lifecycle
- `DependencyInjection.AddApplicationServices` extension method for `IServiceCollection`

### Changed
- `User` and `Product` models updated with `CreatedAt`, `UpdatedAt`, and `Status` fields
- `DatabaseConnection` schema initialisation consolidated into a single `InitializeSchema` call during startup

---

## [0.2.0] - 2025-01-20

### Added
- `DatabaseConnection` wrapping `SqliteConnection` with schema initialisation and thread-safe access
- `ConnectionPool` providing reusable connections up to a configurable pool size
- `DatabaseSettings` and `AppConstants`/`SqlConstants` for centralised configuration
- `User` and `Product` domain models
- `AuditLog` stub model

### Fixed
- Initial schema creation failed when the database file directory did not exist; now creates parent directories automatically

---

## [0.1.0] - 2025-01-07

### Added
- Initial project scaffolding: solution file, `src/` and `tests/` layout
- `DotNet.SQLite.CrudGenerator` console application targeting .NET 10
- `Microsoft.Data.Sqlite` and `Microsoft.Extensions.DependencyInjection` dependencies
- `Program.cs` entry point with basic DI container bootstrap
- Placeholder `IRepository<T>` interface
- MIT licence, `.gitignore`, and `.editorconfig`

---

## Version Support

| Version | Status    | .NET Support | Release Date | EOL Date   |
|---------|-----------|--------------|--------------|------------|
| 1.0.0   | Current   | .NET 10      | 2025-10-28   | N/A        |
| 0.9.0   | Supported | .NET 10      | 2025-09-10   | 2026-10-28 |
| 0.8.0   | Supported | .NET 10      | 2025-07-22   | 2026-10-28 |
| 0.7.0   | Supported | .NET 10      | 2025-06-04   | 2026-06-04 |
| 0.6.0   | EOL       | .NET 10      | 2025-04-29   | 2025-10-28 |
| 0.5.0   | EOL       | .NET 10      | 2025-03-18   | 2025-09-18 |
| 0.4.0   | EOL       | .NET 10      | 2025-02-24   | 2025-08-24 |
| 0.3.0   | EOL       | .NET 10      | 2025-02-06   | 2025-08-06 |
| 0.2.0   | EOL       | .NET 10      | 2025-01-20   | 2025-07-20 |
| 0.1.0   | EOL       | .NET 10      | 2025-01-07   | 2025-07-07 |

## Roadmap

### Planned for v1.1.0
- Entity relationship management (foreign keys, navigation properties)
- Batch insert/update operations
- Query result caching strategies

### Planned for v1.2.0
- Full-text search support via SQLite FTS5
- Migration scripting improvements (auto-detect schema drift)
- Structured logging adapter for `Microsoft.Extensions.Logging`

### Planned for v2.0.0
- Multi-database support (SQL Server, PostgreSQL, MySQL)
- GraphQL API generation
- REST API scaffolding
- Authentication/Authorization integration
- Real-time change notifications (SignalR)

## Known Issues

- SQLite connection pool has a 100-connection limit; applications requiring higher concurrency should use WAL mode
- In-memory databases are not suitable for multi-threaded production scenarios
- Large file uploads (>100 MB) during export may cause elevated memory usage; stream in chunks

## Dependencies

- `Microsoft.Data.Sqlite` v10.0.0 — SQLite database provider
- `Microsoft.Extensions.DependencyInjection` v10.0.0 — Service container
- `Grpc.Tools` v2.65.0 — Protocol buffer compiler
- `Grpc.AspNetCore` v2.65.0 — gRPC infrastructure
- `System.ComponentModel.Annotations` v5.0.0 — Validation attributes

## Contributors

- Vladyslav Zaiets (Author & Maintainer)

## License

MIT License — see [LICENSE](LICENSE) for details.

## Support & Communication

- **Issues**: [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions)
- **Email**: vladyslav.zaiets@sarmkadan.com
