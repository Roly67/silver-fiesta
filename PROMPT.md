# File Conversion API - Ralph Wiggum Specification

## Project Overview

Build a .NET 10 Web API for converting files between formats. The initial implementation focuses on HTML to PDF conversion, with an extensible architecture to support additional format conversions in the future.

**IMPORTANT**: This project MUST follow the dotnet-coding-standards from the roly67/cc-skills plugin. Reference the standards at `~/.claude/plugins/cache/cc-skills/dotnet-coding-standards/2.3.0/` for detailed guidance.

---

## Core Requirements

### Solution Structure (Clean Architecture)

Create a Clean Architecture solution following the dependency rule where dependencies point INWARD:

```
FileConversionApi/
├── src/
│   ├── FileConversionApi.Api/              # Presentation Layer (Controllers, Middleware)
│   ├── FileConversionApi.Application/      # Application Business Rules (Commands, Queries, DTOs)
│   ├── FileConversionApi.Domain/           # Enterprise Business Rules (Entities, Value Objects)
│   └── FileConversionApi.Infrastructure/   # Interface Adapters (EF Core, External Services)
├── tests/
│   ├── FileConversionApi.UnitTests/
│   └── FileConversionApi.IntegrationTests/
├── Directory.Build.props                    # Centralized build settings
├── stylecop.json                           # StyleCop configuration
├── .editorconfig                           # Editor configuration
├── coverlet.runsettings                    # Code coverage settings
└── FileConversionApi.sln
```

### Technology Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core with PostgreSQL (Npgsql)
- PuppeteerSharp for HTML to PDF conversion
- Markdig for Markdown parsing
- MediatR for CQRS pattern
- FluentValidation for request validation
- JWT Bearer Authentication
- Serilog for logging (NOT Microsoft.Extensions.Logging config)
- Sentry for error tracking
- Swagger/OpenAPI documentation
- StyleCop.Analyzers for code style
- xUnit, Moq, FluentAssertions for testing
- Coverlet for code coverage

---

## Coding Standards (Non-Negotiable)

### File Structure

Every `.cs` file MUST start with:

```csharp
// <copyright file="{FileName}.cs" company="FileConversionApi">
// © FileConversionApi
// </copyright>

namespace FileConversionApi.{Layer}.{Feature};
```

### Core Rules

| Requirement | Rule |
|-------------|------|
| **Namespaces** | File-scoped: `FileConversionApi.[Layer].[Feature]` |
| **Documentation** | All public members require XML docs with `<summary>` |
| **Null safety** | `?? throw new ArgumentNullException(nameof(param))` in constructors |
| **Field access** | Always use `this.` prefix for instance members |
| **Build rule** | Zero warnings allowed (`TreatWarningsAsErrors=true`) |
| **Nullable refs** | Nullable reference types enabled (`<Nullable>enable</Nullable>`) |
| **StyleCop** | NO pragma suppressions allowed - fix actual issues |
| **ConfigureAwait** | Use `ConfigureAwait(false)` in Application/Infrastructure/Domain |
| **Logging** | Serilog only - no `"Logging"` section in appsettings.json |

### Directory.Build.props (Create at solution root)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)stylecop.json" Link="stylecop.json" />
  </ItemGroup>
</Project>
```

---

## Layer Responsibilities

### Domain Layer (Zero External Dependencies)

Contains:
- Entities with business logic
- Value Objects (strongly-typed IDs)
- Domain Events
- Domain Exceptions
- Result pattern types
- Error definitions

**Rules**:
- NO NuGet packages except primitives
- NO EF Core attributes
- NO framework-specific code

### Application Layer

Contains:
- Commands and Command Handlers (CQRS)
- Queries and Query Handlers
- DTOs
- Interface definitions (repositories, services)
- Validation (FluentValidation)
- MediatR Pipeline Behaviors

**Rules**:
- Depends only on Domain layer
- Defines interfaces that Infrastructure implements
- Uses `ConfigureAwait(false)` on all awaits

### Infrastructure Layer

Contains:
- EF Core DbContext and configurations
- Repository implementations
- External service implementations (PuppeteerSharp)
- Email services, file storage, etc.

**Rules**:
- Implements interfaces from Application layer
- All async calls use `ConfigureAwait(false)`

### API Layer (Presentation)

Contains:
- Controllers (thin - delegate to MediatR)
- Middleware (exception handling)
- Swagger configuration
- Program.cs with DI setup

**Rules**:
- NO `ConfigureAwait(false)` in controllers
- Uses RFC 7807 Problem Details for errors

---

## Database Schema

### Users Table
- Id (Guid, PK)
- Email (string, unique, not null)
- PasswordHash (string, not null)
- ApiKey (string, unique, not null)
- CreatedAt (DateTimeOffset, not null)
- IsActive (bool, not null)

### ConversionJobs Table
- Id (Guid, PK)
- UserId (Guid, FK to Users)
- SourceFormat (string, not null)
- TargetFormat (string, not null)
- Status (int - enum: Pending=0, Processing=1, Completed=2, Failed=3)
- InputFileName (string, not null)
- OutputFileName (string, nullable)
- OutputData (byte[], nullable)
- ErrorMessage (string, nullable)
- CreatedAt (DateTimeOffset, not null)
- CompletedAt (DateTimeOffset, nullable)
- WebhookUrl (string, nullable) - URL to notify when job completes

---

## API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register new user (201 Created)
- `POST /api/v1/auth/login` - Login and receive JWT token (200 OK)
- `POST /api/v1/auth/refresh` - Refresh JWT token (200 OK)
- `GET /api/v1/auth/apikey` - Get/regenerate API key (200 OK)

### Conversions
- `POST /api/v1/convert/html-to-pdf` - Convert HTML to PDF (202 Accepted)
- `POST /api/v1/convert/markdown-to-pdf` - Convert Markdown to PDF (202 Accepted)
- `GET /api/v1/convert/{jobId}` - Get conversion job status (200 OK / 404 Not Found)
- `GET /api/v1/convert/{jobId}/download` - Download converted file (200 OK / 404 Not Found)
- `GET /api/v1/convert/history` - Get user's conversion history (200 OK, paginated)

### Health
- `GET /health` - Health check endpoint (200 OK)

---

## Converter Architecture

Implement an extensible converter system:

```csharp
// <copyright file="IFileConverter.cs" company="FileConversionApi">
// © FileConversionApi
// </copyright>

namespace FileConversionApi.Application.Interfaces;

/// <summary>
/// Interface for file format converters.
/// </summary>
public interface IFileConverter
{
    /// <summary>
    /// Gets the source format this converter handles.
    /// </summary>
    string SourceFormat { get; }

    /// <summary>
    /// Gets the target format this converter produces.
    /// </summary>
    string TargetFormat { get; }

    /// <summary>
    /// Converts the input stream to the target format.
    /// </summary>
    Task<Result<byte[]>> ConvertAsync(
        Stream input,
        ConversionOptions options,
        CancellationToken cancellationToken);
}
```

### Markdown to PDF Converter

The Markdown to PDF converter reuses the existing HTML to PDF infrastructure:

1. **Parse Markdown** - Use Markdig library to convert Markdown to HTML
2. **Apply Styling** - Wrap HTML in a styled template with professional CSS
3. **Generate PDF** - Delegate to HtmlToPdfConverter for final PDF generation

**Markdig Pipeline Configuration:**
- Advanced extensions (tables, task lists, autolinks, footnotes)
- Syntax highlighting for code blocks
- Custom container support

**Default CSS Styling:**
- Professional typography (headings, paragraphs, lists)
- Code blocks with monospace font and background
- Tables with borders and alternating row colors
- Blockquotes with left border styling

---

## Webhook Notifications

Notify external systems when conversion jobs complete or fail.

### How It Works

1. User provides optional `webhookUrl` in conversion request
2. URL is stored in `ConversionJob.WebhookUrl`
3. After job completes (success or failure), POST to webhook URL
4. Webhook failures are logged but don't fail the conversion

### Interface

```csharp
// Application/Interfaces/IWebhookService.cs
public interface IWebhookService
{
    Task SendJobCompletedAsync(ConversionJob job, CancellationToken cancellationToken);
}
```

### Implementation

Create `WebhookService` in Infrastructure layer:
- Use `IHttpClientFactory` for HTTP calls
- Inject `IOptions<WebhookSettings>` for configuration
- Implement retry logic with configurable delays
- Log all webhook attempts (success and failure)
- Fire-and-forget pattern (don't block conversion response)

### Webhook Payload

POST to the webhook URL with JSON body:

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Completed",
  "sourceFormat": "markdown",
  "targetFormat": "pdf",
  "inputFileName": "document.md",
  "outputFileName": "document.pdf",
  "errorMessage": null,
  "createdAt": "2026-01-12T06:00:00Z",
  "completedAt": "2026-01-12T06:00:05Z",
  "downloadUrl": "/api/v1/convert/550e8400-e29b-41d4-a716-446655440000/download"
}
```

### API Request with Webhook

```json
POST /api/v1/convert/markdown-to-pdf
{
  "markdown": "# Hello World",
  "fileName": "document.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion",
  "options": { "pageSize": "A4" }
}
```

### Files to Create/Modify

| File | Action |
|------|--------|
| `Domain/Entities/ConversionJob.cs` | Add `WebhookUrl` property and update `Create()` method |
| `Infrastructure/Options/WebhookSettings.cs` | Create settings class |
| `Application/Interfaces/IWebhookService.cs` | Create interface |
| `Infrastructure/Services/WebhookService.cs` | Create implementation |
| `Infrastructure/Persistence/Configurations/ConversionJobConfiguration.cs` | Configure WebhookUrl column |
| `Application/Commands/Conversion/*Command.cs` | Add WebhookUrl property to commands |
| `Application/Commands/Conversion/*CommandHandler.cs` | Call webhook service after completion |
| `Api/Models/*Request.cs` | Add WebhookUrl to request models |
| `Infrastructure/DependencyInjection.cs` | Register webhook service with HttpClient |
| `appsettings.json` | Add WebhookSettings section |

### Registration

```csharp
// Infrastructure/DependencyInjection.cs
services.Configure<WebhookSettings>(configuration.GetSection(WebhookSettings.SectionName));
services.AddHttpClient<IWebhookService, WebhookService>();
```

---

## Error Handling

### Use Result Pattern (NOT exceptions for business logic)

```csharp
public sealed record Error(string Code, string Message);

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }
}
```

### Domain Exceptions (for truly exceptional cases)

- `EntityNotFoundException`
- `BusinessRuleException`
- `UnauthorizedException`
- `ForbiddenException`

### Global Exception Handler → RFC 7807 Problem Details

Map exceptions to appropriate HTTP status codes and Problem Details responses.

---

## Logging with Serilog

### appsettings.json (NO "Logging" section!)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithProcessId"]
  }
}
```

### Program.cs Bootstrap

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application");
    // ... builder setup
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));
    // ...
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

---

## Authentication

- JWT tokens with configurable expiration
- Refresh tokens with longer expiration
- API key authentication as alternative
- Rate limiting per user

---

## Configuration Sections

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=fileconversion;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-here-at-least-32-chars",
    "Issuer": "FileConversionApi",
    "Audience": "FileConversionApi",
    "TokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "PuppeteerSettings": {
    "ExecutablePath": null,
    "Timeout": 30000
  },
  "WebhookSettings": {
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "RetryDelayMilliseconds": 1000
  },
  "RateLimiting": {
    "RequestsPerHour": 100
  },
  "Serilog": { ... },
  "Sentry": {
    "Dsn": ""
  }
}
```

---

## Testing Requirements

- Minimum 80% code coverage
- Use xUnit, Moq, FluentAssertions
- Test naming: `MethodName_Scenario_ExpectedResult`
- Unit tests for all Application layer handlers
- Integration tests for API endpoints

---

## Docker Support

Create `docker-compose.yml` for local development with:
- API service
- PostgreSQL database

---

## Completion Criteria

The task is COMPLETE when ALL of the following are true:

1. ✅ Solution builds with ZERO errors and ZERO warnings
2. ✅ All projects follow Clean Architecture layering
3. ✅ Every `.cs` file has copyright header and file-scoped namespace
4. ✅ All public members have XML documentation
5. ✅ `this.` prefix used for all instance member access
6. ✅ `ConfigureAwait(false)` used in Application/Infrastructure layers
7. ✅ Serilog configured (NO Microsoft "Logging" section)
8. ✅ EF Core migrations created for PostgreSQL
9. ✅ All API endpoints implemented with Swagger docs
10. ✅ HTML to PDF conversion works with PuppeteerSharp
11. ✅ Markdown to PDF conversion works with Markdig + PuppeteerSharp
12. ✅ Webhook notifications work for completed/failed jobs
13. ✅ JWT + API Key authentication functional
14. ✅ Unit tests exist with 80%+ coverage
15. ✅ docker-compose.yml exists and works
16. ✅ README.md documents how to run the project

---

## Iteration Instructions

Each iteration:
1. Check what already exists in the solution
2. Run `dotnet build` to check for errors/warnings
3. Identify the next component to implement based on completion criteria
4. Implement following ALL coding standards above
5. Verify it compiles with zero warnings
6. Move to the next component

When ALL completion criteria are met, output:

<promise>FILE CONVERSION API COMPLETE</promise>

---

**Current Status:** Adding webhook notifications for completed jobs.
