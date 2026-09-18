# CLAUDE.md

## Overview
.NET 10 console app + library that generates SQLite CRUD repositories, migrations and gRPC services from C# models (layered architecture: CLI -> Services -> Repository/UnitOfWork -> Microsoft.Data.Sqlite).

## Build
- SDK: .NET 10.0.100 (`global.json`, rollForward latestMinor)
- `dotnet restore dotnet-sqlite-crud-generator.sln`
- `dotnet build dotnet-sqlite-crud-generator.sln -c Release` (or `make build`)
- Run demo app: `dotnet run --project src/DotNet.SQLite.CrudGenerator` (or `make run`)
- Main project has `TreatWarningsAsErrors=true`; warnings fail the build.

## Test
- All: `dotnet test dotnet-sqlite-crud-generator.sln -c Release` (or `make test`)
- Single test: `dotnet test tests/dotnet-sqlite-crud-generator.Tests --filter "FullyQualifiedName~StringExtensionsTests"`
- Coverage: `make test-coverage` (XPlat Code Coverage, output in TestResults/)
- Benchmarks (BenchmarkDotNet): `dotnet run -c Release --project benchmarks/dotnet-sqlite-crud-generator.Benchmarks`
- Stack: xUnit 2.9, FluentAssertions 7, Moq 4.20

## Lint / Format
- `make format` = `dotnet format --verify-no-changes`; `make format-fix` applies
- `make lint` = build with `/p:TreatWarningsAsErrors=true`
- `make analyze` = build with `EnforceCodeStyleInBuild=true`, `AnalysisLevel=latest`
- Style rules in `.editorconfig` (4 spaces, Allman braces, final newline)
- CI: `.github/workflows/ci.yml` runs restore/build Release/test on .NET 10.x

## Key directories
- `src/DotNet.SQLite.CrudGenerator/` - main project (namespace `DotNet.SQLite.CrudGenerator`)
  - `Program.cs` - entry point; DI bootstrap via `AddApplicationServices(...)` then CRUD demo
  - `CLI/` - `CommandParser` + `GenerateCommand`, `MigrateCommand`, `ValidateCommand`, `ListCommand`, `StatsCommand`, `DiffCommand`
  - `Services/` - business logic (`GenerationService`, `MigrationDiffService`, `QueryBuilderGenerationService`, `DataExportService`, `AuditTrailService`, `UserService`/`ProductService`/`OrderService`)
  - `Data/` - `Repository<T>` generic base, `DatabaseConnection`, `ConnectionPool`, `DbContextProvider`
  - `Interfaces/` - `IRepository<T>`, `IService<T>`, `IUnitOfWork`, `IBulkTransferService`
  - `Models/` - entities (`User`, `Product`, `Order`, `Category`, `AuditLog`), `SoftDeleteOptions`
  - `Configuration/` - `DependencyInjection.cs`, `DotnetSqliteCrudGeneratorOptions`, `DatabaseSettings`, cache/pool config
  - `Middleware/` - `MiddlewarePipeline` with logging, validation, rate-limiting, error-handling middleware
  - `BulkTransfer/`, `BackgroundWorkers/`, `Caching/`, `Events/`, `Formatters/` (CSV/XML/JSON), `Validation/`, `Exceptions/`, `Utilities/`, `Constants/`, `Attributes/`
- `tests/dotnet-sqlite-crud-generator.Tests/` - xUnit tests, mirrors src folders
- `benchmarks/dotnet-sqlite-crud-generator.Benchmarks/` - BenchmarkDotNet project
- `docs/` - per-class markdown docs plus `architecture.md`, `deployment.md`, `docker-guide.md`
- `Dockerfile`, `docker-compose*.yml` - container build; `Makefile` - all common tasks

## Conventions
- Every `.cs` file starts with `#nullable enable` and the author header comment block (see `Program.cs`).
- Nullable + ImplicitUsings enabled; `LangVersion=latest`; file-scoped namespaces.
- XML doc comments on public types and members (`GenerateDocumentationFile=true`; CS1591 suppressed).
- Extension helpers live in sibling files named `<Type>Extensions.cs`, `<Type>JsonExtensions.cs`, `<Type>Validation.cs`.
- Interfaces prefixed `I`; async methods suffixed `Async`; services take dependencies via constructor DI, registered in `Configuration/DependencyInjection.cs`.
- Tests: `public sealed class <Type>Tests`, method names `Method_Scenario_ExpectedResult`, Arrange/Act/Assert comments, FluentAssertions `.Should()`.
- Argument checks via `ArgumentNullException.ThrowIfNull`.
- Stray files at repo root (`}`, `fix_where.py`, `batch_fix.py`, `*_SUMMARY.md`) are leftovers, not part of the build.
