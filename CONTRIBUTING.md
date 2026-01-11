# Contributing to File Conversion API

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing](#testing)
- [Architecture Guidelines](#architecture-guidelines)

## Code of Conduct

Please be respectful and constructive in all interactions. We're all here to build something great together.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/silver-fiesta.git
   cd silver-fiesta
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/Roly67/silver-fiesta.git
   ```
4. **Create a branch** for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Development Setup

### Prerequisites

- .NET 10 SDK
- PostgreSQL 16+
- Docker (recommended)
- IDE: Visual Studio 2022, JetBrains Rider, or VS Code with C# extension

### Local Setup

1. **Start PostgreSQL**:
   ```bash
   docker run -d \
     --name postgres \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=fileconversion \
     -p 5432:5432 \
     postgres:16-alpine
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the API**:
   ```bash
   cd src/FileConversionApi.Api
   dotnet run
   ```

4. **Run tests**:
   ```bash
   dotnet test
   ```

### Using Docker

```bash
docker-compose up -d
```

## Coding Standards

### General Rules

- **Follow Clean Architecture** - Respect layer boundaries
- **Use file-scoped namespaces** - One type per file
- **Zero warnings policy** - All code must compile without warnings
- **XML documentation** - All public members must have documentation

### C# Style Guide

```csharp
// Use file-scoped namespaces
namespace FileConversionApi.Domain.Entities;

/// <summary>
/// XML documentation for all public members.
/// </summary>
public class Example
{
    // Use 'this.' prefix for instance members
    private readonly string field;

    public Example(string field)
    {
        this.field = field;
    }

    // Use expression-bodied members when appropriate
    public string Field => this.field;
}
```

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `ConversionJob` |
| Interfaces | IPascalCase | `IUserRepository` |
| Methods | PascalCase | `GetUserAsync` |
| Properties | PascalCase | `Email` |
| Private fields | camelCase | `emailService` |
| Constants | PascalCase | `MaxRetryCount` |
| Parameters | camelCase | `userId` |
| Local variables | camelCase | `result` |

### Async/Await

- Use `Async` suffix for async methods
- Use `ConfigureAwait(false)` in Application and Infrastructure layers
- Prefer `ValueTask` for hot paths that often complete synchronously

```csharp
public async Task<Result<User>> GetUserAsync(UserId id, CancellationToken cancellationToken)
{
    var user = await this.dbContext.Users
        .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
        .ConfigureAwait(false);

    return user is null
        ? Result<User>.Failure(UserErrors.NotFound)
        : Result<User>.Success(user);
}
```

### Error Handling

Use the Result pattern instead of exceptions for expected failures:

```csharp
// Good - Using Result pattern
public Result<User> CreateUser(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        return Result<User>.Failure(UserErrors.InvalidEmail);
    }

    return Result<User>.Success(new User(email));
}

// Avoid - Throwing exceptions for validation
public User CreateUser(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        throw new ArgumentException("Invalid email"); // Don't do this
    }

    return new User(email);
}
```

## Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation changes |
| `style` | Code style changes (formatting, etc.) |
| `refactor` | Code refactoring |
| `test` | Adding or updating tests |
| `ci` | CI/CD changes |
| `chore` | Maintenance tasks |
| `deps` | Dependency updates |

### Examples

```bash
feat(auth): add refresh token endpoint
fix(conversion): handle empty HTML content
docs(readme): update API examples
test(auth): add login command handler tests
ci: add security scanning workflow
```

## Pull Request Process

1. **Update your branch** with the latest upstream changes:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Ensure all checks pass**:
   ```bash
   dotnet build
   dotnet test
   dotnet format --verify-no-changes
   ```

3. **Push your changes**:
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Create a Pull Request** on GitHub:
   - Fill out the PR template completely
   - Link any related issues
   - Request review from maintainers

5. **Address review feedback**:
   - Make requested changes
   - Push additional commits
   - Re-request review when ready

### PR Requirements

- [ ] All CI checks pass
- [ ] Code coverage maintained (80%+ target)
- [ ] No new warnings
- [ ] Documentation updated if needed
- [ ] Tests added for new functionality

## Testing

### Test Structure

```
tests/
├── FileConversionApi.UnitTests/           # Unit tests
│   ├── Domain/                            # Domain layer tests
│   ├── Application/                       # Application layer tests
│   └── Infrastructure/                    # Infrastructure tests
└── FileConversionApi.IntegrationTests/    # Integration tests
```

### Writing Tests

```csharp
public class UserTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashedpassword";

        // Act
        var user = User.Create(email, passwordHash);

        // Assert
        user.Email.Should().Be(email);
        user.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        // Arrange & Act
        var act = () => User.Create(email!, "hash");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/FileConversionApi.UnitTests

# Run tests matching a filter
dotnet test --filter "FullyQualifiedName~UserTests"
```

## Architecture Guidelines

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│              API Layer                  │  ← Controllers, Middleware
├─────────────────────────────────────────┤
│          Application Layer              │  ← Commands, Queries, DTOs
├─────────────────────────────────────────┤
│         Infrastructure Layer            │  ← Repositories, Services
├─────────────────────────────────────────┤
│            Domain Layer                 │  ← Entities, Value Objects
└─────────────────────────────────────────┘
```

### Dependency Rules

- **Domain** → No dependencies
- **Application** → Domain only
- **Infrastructure** → Application, Domain
- **API** → All layers

### CQRS Pattern

```csharp
// Command (write operation)
public record CreateUserCommand(string Email, string Password) : IRequest<Result<Guid>>;

// Query (read operation)
public record GetUserQuery(Guid Id) : IRequest<Result<UserDto>>;
```

### Adding New Features

1. **Domain** - Add entities/value objects if needed
2. **Application** - Create command/query with handler and validator
3. **Infrastructure** - Implement any new repository methods
4. **API** - Add controller endpoint

## Questions?

- Open a [Discussion](https://github.com/Roly67/silver-fiesta/discussions)
- Check existing [Issues](https://github.com/Roly67/silver-fiesta/issues)

---

Thank you for contributing! 🎉
