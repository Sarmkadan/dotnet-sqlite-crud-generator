// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-02-15

### Added
- Event-driven architecture with EventBus for entity change notifications
- Performance monitoring and metrics collection
- Background task queue for long-running operations
- Webhook integration for external system communication
- Data export formatters (JSON, CSV, XML)
- Caching layer with configurable TTL and sliding expiration
- Rate limiting middleware for API protection
- Validation middleware for input sanitization
- Comprehensive audit logging for all entity changes
- gRPC service generation from models

### Changed
- Improved error handling with typed exception hierarchy
- Enhanced repository pattern with LINQ support
- Optimized database connection pooling
- Better transaction handling in Unit of Work pattern

### Fixed
- Memory leak in cache invalidation
- Concurrency issues in repository operations
- SQL injection vulnerability in query builder

### Security
- Added parameterized query support
- Implemented connection encryption
- Added CORS protection
- Rate limiting to prevent abuse

## [1.1.0] - 2026-01-20

### Added
- Pagination support with GetPagedAsync
- Advanced search with FindAsync and LINQ predicates
- Soft delete functionality for entity lifecycle management
- AuditLog model for change tracking
- OrderService with order management operations
- ProductService with inventory management
- CategoryRepository for hierarchical data
- Dependency injection container setup
- Database initialization with schema creation
- Performance benchmarking utilities

### Changed
- Refactored service layer for better separation of concerns
- Improved database connection management
- Enhanced entity validation

### Fixed
- Database schema initialization timing issues
- Null reference exceptions in entity mapping

## [1.0.0] - 2025-12-15

### Added
- Initial release of SQLite CRUD Generator
- Core domain models (User, Product, Order, Category, AuditLog)
- Generic Repository pattern implementation
- Service layer with business logic
- Unit of Work pattern for transaction management
- Dependency Injection container configuration
- Microsoft.Data.Sqlite integration
- Console application entry point
- CLI command infrastructure
- Basic CRUD operations for all entities
- Database connection pooling
- Error handling and custom exceptions
- Utilities for string, datetime, and reflection operations
- XML documentation for all public APIs

### Documentation
- Comprehensive README with architecture diagram
- Getting Started guide
- API reference documentation
- Architecture overview document
- Deployment guide for production environments
- FAQ with common questions and solutions

### Examples
- Basic CRUD operations example
- Transaction and Unit of Work pattern example
- Pagination and search example
- Data export example
- Error handling and events example

### Infrastructure
- Docker support with multi-stage build
- Docker Compose for local development
- GitHub Actions CI/CD pipeline
- .editorconfig for code style
- Makefile for common tasks
- .gitignore for version control

## [0.9.0] - 2025-11-01

### Added
- Pre-release version for community feedback
- Core architecture patterns
- Initial documentation

---

## Version Support

| Version | Status | .NET Support | Release Date | EOL Date |
|---------|--------|--------------|--------------|----------|
| 1.2.0 | Current | .NET 10 | 2026-02-15 | N/A |
| 1.1.0 | Stable | .NET 10 | 2026-01-20 | 2027-01-20 |
| 1.0.0 | Stable | .NET 10 | 2025-12-15 | 2026-12-15 |
| 0.9.0 | Deprecated | .NET 10 | 2025-11-01 | 2025-12-15 |

## Migration Guide

### From 1.1.0 to 1.2.0

No breaking changes. Simply update NuGet packages:

```bash
dotnet package upgrade
```

New features (EventBus, caching, audit logging) are opt-in.

### From 1.0.0 to 1.1.0

Minor breaking change: `GetAsync` renamed to `GetByIdAsync`

```csharp
// Before
var user = await userService.GetAsync(1);

// After
var user = await userService.GetByIdAsync(1);
```

## Roadmap

### Planned for v1.3.0
- [ ] Entity relationship management (foreign keys)
- [ ] Batch operation support
- [ ] Query caching strategies
- [ ] Full-text search support
- [ ] Migration scripting improvements

### Planned for v2.0.0
- [ ] Multi-database support (SQL Server, PostgreSQL, MySQL)
- [ ] ORM alternative with LINQ to SQL
- [ ] GraphQL API generation
- [ ] REST API scaffolding
- [ ] Authentication/Authorization integration
- [ ] Real-time change notifications (SignalR)

## Known Issues

- SQLite connection pooling has a 100-connection limit
- In-memory databases are not suitable for multi-threaded scenarios
- Large file uploads (>100MB) may cause memory issues

## Dependencies

- `Microsoft.Data.Sqlite` v10.0.0 - SQLite database provider
- `Microsoft.Extensions.DependencyInjection` v10.0.0 - Service container
- `Grpc.Tools` v2.65.0 - Protocol buffer compiler
- `Grpc.AspNetCore` v2.65.0 - gRPC infrastructure

## Contributors

- Vladyslav Zaiets (Author & Maintainer)

## License

MIT License - see LICENSE file for details

## Support & Communication

- **Issues**: [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions)
- **Email**: vladyslav.zaiets@sarmkadan.com
