// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Contributing to dotnet-sqlite-crud-generator

Thank you for your interest in contributing to dotnet-sqlite-crud-generator! We welcome contributions from the community.

## Development Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A code editor with C# support (e.g., Visual Studio, VS Code with C# Dev Kit, Rider)

## Building Locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Build a specific project
dotnet build src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --logger trx

# Run a specific test project
dotnet test tests/dotnet-sqlite-crud-generator.Tests/dotnet-sqlite-crud-generator.Tests.csproj
```

Test results are written to `TestResults/` in TRX format.

## Running Benchmarks

```bash
cd benchmarks/dotnet-sqlite-crud-generator.Benchmarks
dotnet run --configuration Release
```

## How to Contribute

1. **Fork the repository** on GitHub.
2. **Clone your fork** locally: `git clone https://github.com/your-username/dotnet-sqlite-crud-generator.git`
3. **Create a new branch** for your feature or bug fix: `git checkout -b feature/your-feature-name`
4. **Make your changes** and ensure they adhere to the project's coding standards.
5. **Run the tests** to confirm nothing is broken: `dotnet test`
6. **Commit your changes** with clear, descriptive commit messages following [Conventional Commits](https://www.conventionalcommits.org/).
7. **Push to your fork** and submit a **Pull Request** to the `main` branch.

## Pull Request Guidelines

- Keep PRs focused — one feature or fix per PR.
- Include or update tests for any changed behaviour.
- Update documentation if public APIs or behaviour change.
- All CI checks must pass before a PR can be merged.
- Provide a clear description of the problem being solved and the approach taken.

## Code Style

- Follow the `.editorconfig` settings already present in the repository.
- Use `file-scoped` namespace declarations (C# 10+).
- Prefer `var` only when the type is evident from the right-hand side.
- Provide XML documentation comments for all public APIs.
- Keep lines to a reasonable length; prefer clarity over brevity.
- **IMPORTANT**: Keep all existing author headers intact. Do not remove them from any file.

## Reporting Issues

If you encounter a bug or have a feature request, please use [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues).
When reporting a bug, include:
- .NET SDK version (`dotnet --version`)
- Operating system and version
- Steps to reproduce
- Expected vs actual behaviour

## License

By contributing to this project, you agree that your contributions will be licensed under its MIT License.

