// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Contributing to SQLite CRUD Generator

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Focus on code, not personalities
- Report inappropriate behavior appropriately

## How to Contribute

### Reporting Bugs

1. **Check existing issues**: Search [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues) to avoid duplicates
2. **Create detailed report**: Include reproduction steps, expected behavior, actual behavior
3. **Provide environment**: .NET version, OS, relevant package versions
4. **Add examples**: Code snippets demonstrating the issue

**Bug Report Template**:
```markdown
### Description
Clear description of the bug

### Reproduction Steps
1. Step 1
2. Step 2
3. ...

### Expected Behavior
What should happen

### Actual Behavior
What actually happens

### Environment
- .NET Version: 10.0.x
- OS: Windows/Linux/macOS
- Package Version: 1.x.x

### Code Example
```csharp
// Minimal reproducible example
```
```

### Suggesting Features

1. **Check discussions**: Visit [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions) first
2. **Describe use case**: Explain the problem and proposed solution
3. **Provide examples**: Show how the feature would be used
4. **Consider impact**: How does it affect existing code?

**Feature Request Template**:
```markdown
### Problem
Describe the problem you're trying to solve

### Proposed Solution
Describe your solution

### Alternative Solutions
Other approaches you considered

### Example Usage
```csharp
// How the feature would be used
```

### Impact
- Breaking changes: Yes/No
- Performance impact: None/Minor/Significant
- Documentation needed: Yes/No
```

### Submitting Code Changes

#### Prerequisites

- .NET 10 SDK installed
- Git configured
- GitHub account
- Understanding of C# and .NET

#### Step 1: Fork and Clone

```bash
# Fork the repository on GitHub (button in top-right)
git clone https://github.com/YOUR_USERNAME/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator
git remote add upstream https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
```

#### Step 2: Create Feature Branch

```bash
# Update main branch
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/your-feature-name
```

**Branch Naming Convention**:
- `feature/description` - New feature
- `fix/description` - Bug fix
- `docs/description` - Documentation
- `refactor/description` - Code refactoring
- `perf/description` - Performance improvement

#### Step 3: Make Changes

Follow the code style guidelines:

**C# Code Style**:
- Use 4-space indentation
- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Add XML documentation to public members
- Maximum line length: 120 characters
- Use var only when type is obvious from context

**Example**:
```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Features;

/// <summary>Provides functionality for feature X</summary>
public class FeatureService
{
    private readonly IRepository<Entity> _repository;

    /// <summary>Initializes a new instance of FeatureService</summary>
    /// <param name="repository">Entity repository</param>
    public FeatureService(IRepository<Entity> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>Performs operation on entity</summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>Operation result</returns>
    public async Task<Entity> GetAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));

        return await _repository.GetByIdAsync(id);
    }
}
```

**Guidelines**:
- One concern per class
- Methods under 30 lines when possible
- Private fields start with underscore
- Use async/await for I/O operations
- Validate inputs at method entry
- Use meaningful variable names
- Add comments only for non-obvious logic

#### Step 4: Commit Changes

```bash
# Stage changes
git add src/DotNet.SQLite.CrudGenerator/FeatureService.cs
git add tests/FeatureServiceTests.cs

# Commit with meaningful message
git commit -m "feat: Add feature X

- Implements functionality for X
- Includes unit tests
- Updates documentation

Closes #123"
```

**Commit Message Format**:
```
<type>: <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style (formatting)
- `refactor`: Code refactoring
- `perf`: Performance improvement
- `test`: Test addition/modification
- `ci`: CI/CD changes

**Guidelines**:
- First line under 72 characters
- Reference issues: `Closes #123`, `Fixes #456`
- Explain what and why, not how
- Use imperative mood: "Add" not "Added"

#### Step 5: Add Tests

Add unit tests for new features:

```csharp
public class FeatureServiceTests
{
    [Fact]
    public async Task GetAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<Entity>>();
        var service = new FeatureService(mockRepository.Object);
        var expected = new Entity { Id = 1, Name = "Test" };

        mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(expected);

        // Act
        var result = await service.GetAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
        mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<Entity>>();
        var service = new FeatureService(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAsync(-1));
    }
}
```

#### Step 6: Update Documentation

- Update README.md if user-facing changes
- Update API docs if public API changes
- Add examples if introducing new patterns
- Update CHANGELOG.md

#### Step 7: Push and Create Pull Request

```bash
# Push to your fork
git push origin feature/your-feature-name

# Visit GitHub and create PR
# Fill out PR template completely
```

**Pull Request Template**:
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Changes Made
- Point 1
- Point 2
- Point 3

## Testing Done
- [ ] Unit tests added/updated
- [ ] Manual testing completed
- [ ] Edge cases tested

## Breaking Changes
- None / List any breaking changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] Tests pass locally
- [ ] No new warnings
- [ ] Commits are clean
```

#### Step 8: Code Review

- Address reviewer comments
- Push updates to same branch
- Don't force push after review starts
- Request re-review when ready

## Development Workflow

### Local Setup

```bash
# Clone and setup
git clone https://github.com/sarmkadan/dotnet-sqlite-crud-generator.git
cd dotnet-sqlite-crud-generator

# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Run examples
dotnet run --project src/DotNet.SQLite.CrudGenerator
```

### Daily Workflow

```bash
# Start day
git fetch upstream
git rebase upstream/main

# Make changes
# ... edit files ...

# Test
dotnet build
dotnet test

# Commit
git commit -am "feat: description"

# Push before sleep
git push origin feature/branch-name
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "Namespace.TestClass.TestMethod"

# With coverage
dotnet test /p:CollectCoverage=true
```

## Project Structure

```
src/DotNet.SQLite.CrudGenerator/
├── Models/              # Domain entities
├── Services/            # Business logic
├── Data/                # Repository implementations
├── Interfaces/          # Contracts
├── Configuration/       # DI setup
├── Middleware/          # Request pipeline
├── Utilities/           # Helper functions
└── Program.cs           # Entry point

docs/                    # Documentation
examples/                # Example code
.github/workflows/       # CI/CD
tests/                   # Unit tests (if added)
```

## Release Process

Maintainers follow semantic versioning:
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

## Questions?

- **Usage Questions**: [GitHub Discussions](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/discussions)
- **Bug Reports**: [GitHub Issues](https://github.com/sarmkadan/dotnet-sqlite-crud-generator/issues)
- **Security Issues**: Email vladyslav.zaiets@sarmkadan.com (don't use issues for security)

## License

By contributing, you agree your contributions are licensed under the MIT License.

## Recognition

Contributors are recognized in:
- GitHub Contributors page
- CHANGELOG.md release notes
- Project README

Thank you for contributing to making this project better! 🎉
