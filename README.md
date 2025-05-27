# SQLite CRUD Generator

A comprehensive .NET 10 source generator and CRUD framework for SQLite databases that creates CRUD operations, migrations, and gRPC services from C# models.

## Features

- **Source Generation**: Automatically generates repository interfaces and implementations from entity models
- **CRUD Operations**: Full Create, Read, Update, Delete operations for any entity
- **SQLite Integration**: Native Microsoft.Data.Sqlite support with connection pooling
- **Service Layer**: Well-structured service classes with business logic separation
- **Unit of Work Pattern**: Transaction support and coordinated repository access
- **Dependency Injection**: Complete Microsoft.Extensions.DependencyInjection setup
- **Code Generation**: Generate migrations and gRPC service definitions
- **Audit Logging**: Track all entity changes with comprehensive audit logs
- **Entity Validation**: Built-in validation and error handling

## Project Structure

```
src/DotNet.SQLite.CrudGenerator/
├── Models/              # Domain entities (User, Product, Order, Category, AuditLog)
├── Services/            # Business logic (UserService, ProductService, OrderService, GenerationService)
├── Data/                # Data access layer (Repository, UnitOfWork, DatabaseConnection)
├── Interfaces/          # Contracts (IRepository, IService, IUnitOfWork)
├── Exceptions/          # Custom exceptions (RepositoryException, ValidationException, GenerationException)
├── Configuration/       # DI setup and database settings
├── Constants/           # Application and SQL constants
├── Enums/               # EntityStatus, OperationType
├── Attributes/          # GenerateGrpc attribute
└── Program.cs           # Application entry point
```

## Core Components

### Domain Models
- **User**: User accounts with authentication and profile information
- **Product**: Product inventory with pricing and stock management
- **Order**: Customer orders with status tracking
- **Category**: Product categories with hierarchical support
- **AuditLog**: Entity change tracking for compliance

### Services
- **UserService**: User management, authentication, profile operations
- **ProductService**: Product management, inventory, pricing calculations
- **OrderService**: Order lifecycle, status management, business metrics
- **GenerationService**: Source code generation for repositories and migrations

### Data Access
- **DatabaseConnection**: SQLite connection management and initialization
- **Repository Pattern**: Generic base repository with LINQ-like queries
- **Concrete Repositories**: UserRepository, ProductRepository, OrderRepository, CategoryRepository, AuditLogRepository
- **DbContextProvider**: Unit of work implementation with transaction support

## Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 or VS Code

### Installation

```bash
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator
dotnet build
```

### Running the Application

```bash
dotnet run --project src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
```

This will:
1. Initialize the SQLite database with all required tables and indexes
2. Demonstrate CRUD operations on all entities
3. Generate code artifacts (repository interfaces, migrations, gRPC definitions)
4. Display comprehensive statistics and metrics

## Usage Examples

### Creating Entities

```csharp
// Create a user
var user = new User
{
    Username = "john.doe",
    Email = "john@example.com",
    PasswordHash = "hashed_password",
    FirstName = "John",
    LastName = "Doe"
};

var createdUser = await userService.CreateAsync(user);
```

### Querying Data

```csharp
// Get all products
var products = await productService.GetAllAsync();

// Get products in a category
var categoryProducts = await productService.GetByCategoryAsync(1);

// Get low stock products
var lowStock = await productService.GetLowStockProductsAsync();
```

### Managing Inventory

```csharp
// Add stock
var product = await productService.RestockProductAsync(1, 100);

// Sell items
var product = await productService.SellProductAsync(1, 5);

// Get inventory stats
var stats = await productService.GetInventoryStatsAsync();
```

### Order Management

```csharp
// Get pending orders
var pending = await orderService.GetPendingOrdersAsync();

// Ship an order
await orderService.ShipOrderAsync(orderId, "tracking-number");

// Get order metrics
var metrics = await orderService.GetMetricsAsync();
```

## Database Schema

The application automatically creates the following tables:
- Users (username, email, authentication)
- Products (inventory, pricing, categories)
- Orders (customer orders, status tracking)
- Categories (product classification)
- AuditLogs (change tracking)

## Code Architecture

### Layered Architecture
1. **Models Layer**: Domain entities with validation
2. **Service Layer**: Business logic and orchestration
3. **Repository Layer**: Data access abstraction
4. **Database Layer**: SQLite connection and queries

### Design Patterns
- Repository Pattern
- Unit of Work Pattern
- Dependency Injection
- SOLID Principles
- Async/Await for I/O operations

## Dependencies

- Microsoft.Data.Sqlite (10.0.0)
- Microsoft.Extensions.DependencyInjection (10.0.0)
- Microsoft.Extensions.Configuration (10.0.0)
- Grpc.AspNetCore (2.65.0)
- Grpc.Tools (2.65.0)

## Testing

The application includes comprehensive examples demonstrating:
- CRUD operations on all entities
- Service layer business logic
- Inventory calculations
- Order metrics
- User authentication
- Code generation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Vladyslav Zaiets**  
CTO & Software Architect  
https://sarmkadan.com

## Contributing

This is an educational and reference implementation. For improvements or suggestions, please open an issue or pull request.

## Roadmap

### Phase 1 ✓ (Complete)
- Core architecture and CRUD foundation
- Domain models and services
- Repository pattern implementation
- Dependency injection setup

### Phase 2 (Planned)
- Source generators for automatic code generation
- Advanced query capabilities
- Performance optimizations
- Enhanced gRPC support

### Phase 3 (Planned)
- Migration system with versioning
- Soft delete support
- Query caching
- Distributed transaction support
