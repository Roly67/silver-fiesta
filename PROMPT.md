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
- ASP.NET Core Rate Limiting (built-in .NET 7+ middleware)
- Serilog for logging (NOT Microsoft.Extensions.Logging config)
- prometheus-net for Prometheus metrics
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
- `POST /api/v1/convert/markdown-to-html` - Convert Markdown to HTML (202 Accepted)
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

### Markdown to HTML Converter

The Markdown to HTML converter provides standalone HTML output with embedded styling:

1. **Parse Markdown** - Use Markdig library to convert Markdown to HTML
2. **Apply Styling** - Wrap HTML in a complete document with embedded CSS
3. **Return HTML** - Return UTF-8 encoded HTML bytes

**Output Structure:**
- Complete HTML5 document with DOCTYPE
- Embedded CSS for GitHub-flavored styling
- Responsive viewport meta tag
- Article wrapper with `markdown-body` class

**Reuses same styling as Markdown to PDF** for consistency across output formats.

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

## Rate Limiting

Protect the API from abuse and ensure fair usage using the built-in ASP.NET Core rate limiting middleware.

### Strategy

Use a **Fixed Window** rate limiter with per-user limits:
- Authenticated users: Limited by user ID
- Unauthenticated users: Limited by IP address
- Different limits for different endpoint categories

### Rate Limit Policies

| Policy | Window | Limit | Applies To |
|--------|--------|-------|------------|
| `standard` | 1 hour | 100 requests | General API endpoints |
| `conversion` | 1 hour | 50 requests | Conversion endpoints (resource-intensive) |
| `auth` | 15 minutes | 10 requests | Authentication endpoints (prevent brute force) |

### Implementation

#### 1. Configure Rate Limiting in Program.cs

```csharp
// Program.cs
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Add standard policy for general endpoints
    options.AddPolicy("standard", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKey(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Add conversion policy for resource-intensive endpoints
    options.AddPolicy("conversion", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetPartitionKey(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 50,
                Window = TimeSpan.FromHours(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Add auth policy for authentication endpoints
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Custom response for rate limit exceeded
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://httpstatuses.com/429",
            title = "Too Many Requests",
            status = 429,
            detail = "Rate limit exceeded. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? retryAfter.TotalSeconds
                : 60
        };

        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };
});

// Helper method to get partition key
static string GetPartitionKey(HttpContext context)
{
    // Use user ID for authenticated requests, IP for anonymous
    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}

// Add middleware (after UseAuthentication, before UseAuthorization)
app.UseRateLimiter();
```

#### 2. Apply Rate Limiting to Controllers

```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    // All endpoints in this controller use "auth" policy
}

// Controllers/ConvertController.cs
[ApiController]
[Route("api/v1/convert")]
[Authorize]
[EnableRateLimiting("conversion")]
public class ConvertController : ControllerBase
{
    // POST endpoints use "conversion" policy

    [HttpGet("{jobId}")]
    [EnableRateLimiting("standard")]  // Override: status checks use standard policy
    public async Task<IActionResult> GetJobStatus(Guid jobId) { }

    [HttpGet("history")]
    [EnableRateLimiting("standard")]  // Override: history uses standard policy
    public async Task<IActionResult> GetHistory() { }
}
```

#### 3. Add Rate Limit Headers

Include standard rate limit headers in responses:

```csharp
// Middleware/RateLimitHeadersMiddleware.cs
public class RateLimitHeadersMiddleware
{
    private readonly RequestDelegate next;

    public RateLimitHeadersMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await this.next(context);

        // Headers are set by the rate limiter when available
        // X-RateLimit-Limit: Maximum requests allowed
        // X-RateLimit-Remaining: Requests remaining in current window
        // X-RateLimit-Reset: Unix timestamp when the window resets
        // Retry-After: Seconds until requests are allowed (only on 429)
    }
}
```

### Configuration

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "StandardPolicy": {
      "PermitLimit": 100,
      "WindowMinutes": 60
    },
    "ConversionPolicy": {
      "PermitLimit": 50,
      "WindowMinutes": 60
    },
    "AuthPolicy": {
      "PermitLimit": 10,
      "WindowMinutes": 15
    }
  }
}
```

### Settings Class

```csharp
// Infrastructure/Options/RateLimitingSettings.cs
public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public bool EnableRateLimiting { get; set; } = true;
    public RateLimitPolicySettings StandardPolicy { get; set; } = new();
    public RateLimitPolicySettings ConversionPolicy { get; set; } = new();
    public RateLimitPolicySettings AuthPolicy { get; set; } = new();
}

public class RateLimitPolicySettings
{
    public int PermitLimit { get; set; } = 100;
    public int WindowMinutes { get; set; } = 60;
}
```

### Files to Create/Modify

| File | Action |
|------|--------|
| `Infrastructure/Options/RateLimitingSettings.cs` | Create settings class |
| `Api/Program.cs` | Add rate limiting services and middleware |
| `Api/Controllers/AuthController.cs` | Add `[EnableRateLimiting("auth")]` attribute |
| `Api/Controllers/ConvertController.cs` | Add `[EnableRateLimiting("conversion")]` attribute |
| `appsettings.json` | Add RateLimiting configuration section |
| `appsettings.Development.json` | Add RateLimiting with higher limits for dev |

### Response Headers

When rate limiting is active, responses include:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1704067200
```

When rate limit is exceeded (HTTP 429):

```json
{
  "type": "https://httpstatuses.com/429",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please try again later.",
  "retryAfter": 3600
}
```

### Testing Rate Limits

Unit tests should verify:
- Rate limit headers are present in responses
- 429 response after exceeding limit
- Different limits apply to different policies
- Authenticated vs anonymous partitioning works correctly

---

## Job Cleanup/Expiry

Automatically clean up old conversion jobs to prevent database bloat.

### Strategy

Use a background service (`IHostedService`) that periodically deletes expired jobs based on configurable retention policies.

### Configuration

```json
{
  "JobCleanup": {
    "Enabled": true,
    "RunIntervalMinutes": 60,
    "CompletedJobRetentionDays": 7,
    "FailedJobRetentionDays": 30,
    "BatchSize": 100
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `Enabled` | true | Enable/disable cleanup service |
| `RunIntervalMinutes` | 60 | How often to run cleanup (minutes) |
| `CompletedJobRetentionDays` | 7 | Days to keep completed jobs |
| `FailedJobRetentionDays` | 30 | Days to keep failed jobs (longer for debugging) |
| `BatchSize` | 100 | Max jobs to delete per run (prevents long-running transactions) |

### Settings Class

```csharp
// Infrastructure/Options/JobCleanupSettings.cs
public class JobCleanupSettings
{
    public const string SectionName = "JobCleanup";

    public bool Enabled { get; set; } = true;
    public int RunIntervalMinutes { get; set; } = 60;
    public int CompletedJobRetentionDays { get; set; } = 7;
    public int FailedJobRetentionDays { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
}
```

### Background Service

```csharp
// Infrastructure/Services/JobCleanupService.cs
public class JobCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOptions<JobCleanupSettings> settings;
    private readonly ILogger<JobCleanupService> logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Value.Enabled)
        {
            logger.LogInformation("Job cleanup service is disabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredJobsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(settings.Value.RunIntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupExpiredJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var completedCutoff = DateTimeOffset.UtcNow.AddDays(-settings.Value.CompletedJobRetentionDays);
        var failedCutoff = DateTimeOffset.UtcNow.AddDays(-settings.Value.FailedJobRetentionDays);

        // Delete expired completed jobs
        var completedDeleted = await dbContext.ConversionJobs
            .Where(j => j.Status == ConversionStatus.Completed && j.CompletedAt < completedCutoff)
            .Take(settings.Value.BatchSize)
            .ExecuteDeleteAsync(cancellationToken);

        // Delete expired failed jobs
        var failedDeleted = await dbContext.ConversionJobs
            .Where(j => j.Status == ConversionStatus.Failed && j.CompletedAt < failedCutoff)
            .Take(settings.Value.BatchSize)
            .ExecuteDeleteAsync(cancellationToken);

        if (completedDeleted > 0 || failedDeleted > 0)
        {
            logger.LogInformation(
                "Job cleanup completed: {CompletedDeleted} completed jobs, {FailedDeleted} failed jobs deleted",
                completedDeleted,
                failedDeleted);
        }
    }
}
```

### Registration

```csharp
// Infrastructure/DependencyInjection.cs
services.Configure<JobCleanupSettings>(configuration.GetSection(JobCleanupSettings.SectionName));
services.AddHostedService<JobCleanupService>();
```

### Files to Create/Modify

| File | Action |
|------|--------|
| `Infrastructure/Options/JobCleanupSettings.cs` | Create settings class |
| `Infrastructure/Services/JobCleanupService.cs` | Create background service |
| `Infrastructure/DependencyInjection.cs` | Register settings and hosted service |
| `appsettings.json` | Add JobCleanup configuration section |
| `appsettings.Development.json` | Add JobCleanup with shorter retention for dev |

### Testing

Unit tests should verify:
- Cleanup runs only when enabled
- Jobs older than retention period are deleted
- Jobs newer than retention period are retained
- Batch size limits are respected
- Completed and failed jobs have different retention periods
- Service handles empty database gracefully

---

## Health Check Improvements

Enhance the health endpoint to provide detailed component health status for monitoring and Kubernetes readiness probes.

### Strategy

Extend the existing `/health` endpoint to report detailed status of all critical system components with proper HTTP status codes.

### Health Components

| Component | Check | Description |
|-----------|-------|-------------|
| Database | EF Core connection | Verify PostgreSQL connectivity |
| Chromium | PuppeteerSharp browser launch | Verify PDF generation capability |
| Disk Space | Available storage | Ensure sufficient temp storage |

### Response Format

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123456",
      "description": "PostgreSQL connection successful"
    },
    "chromium": {
      "status": "Healthy",
      "duration": "00:00:00.1000000",
      "description": "Chromium browser available"
    },
    "diskSpace": {
      "status": "Healthy",
      "duration": "00:00:00.0001234",
      "description": "1.5 GB available",
      "data": {
        "availableBytes": 1610612736,
        "minimumRequiredBytes": 104857600
      }
    }
  }
}
```

### HTTP Status Codes

| Status | HTTP Code | Description |
|--------|-----------|-------------|
| Healthy | 200 | All components healthy |
| Degraded | 200 | Some non-critical components degraded |
| Unhealthy | 503 | Critical component failure |

### Implementation

#### 1. Database Health Check

```csharp
// Infrastructure/HealthChecks/DatabaseHealthCheck.cs
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
        }
    }
}
```

#### 2. Chromium Health Check

```csharp
// Infrastructure/HealthChecks/ChromiumHealthCheck.cs
public class ChromiumHealthCheck : IHealthCheck
{
    private readonly IBrowserFactory browserFactory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Quick browser availability check
            using var browser = await browserFactory.CreateBrowserAsync(cancellationToken);
            return HealthCheckResult.Healthy("Chromium browser available");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Chromium browser unavailable", ex);
        }
    }
}
```

#### 3. Disk Space Health Check

```csharp
// Infrastructure/HealthChecks/DiskSpaceHealthCheck.cs
public class DiskSpaceHealthCheck : IHealthCheck
{
    private const long MinimumRequiredBytes = 100 * 1024 * 1024; // 100 MB

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var tempPath = Path.GetTempPath();
        var driveInfo = new DriveInfo(Path.GetPathRoot(tempPath)!);
        var availableBytes = driveInfo.AvailableFreeSpace;

        var data = new Dictionary<string, object>
        {
            ["availableBytes"] = availableBytes,
            ["minimumRequiredBytes"] = MinimumRequiredBytes
        };

        if (availableBytes < MinimumRequiredBytes)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Low disk space: {availableBytes / 1024 / 1024} MB available",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"{availableBytes / 1024 / 1024 / 1024.0:F1} GB available",
            data: data));
    }
}
```

### Registration

```csharp
// Program.cs or Infrastructure/DependencyInjection.cs
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck<ChromiumHealthCheck>("chromium", tags: new[] { "ready" })
    .AddCheck<DiskSpaceHealthCheck>("diskSpace", tags: new[] { "ready" });

// In Program.cs middleware
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthCheckResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Liveness: just check app is running
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthCheckResponse
});
```

### Health Check Response Writer

```csharp
// Api/HealthChecks/HealthCheckResponseWriter.cs
public static class HealthCheckResponseWriter
{
    public static async Task WriteHealthCheckResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            entries = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.ToString(),
                    description = entry.Value.Description,
                    data = entry.Value.Data.Count > 0 ? entry.Value.Data : null,
                    exception = entry.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### Endpoints

| Endpoint | Purpose | Use Case |
|----------|---------|----------|
| `/health` | Full health check | Monitoring dashboards |
| `/health/live` | Liveness probe | K8s liveness (is app running?) |
| `/health/ready` | Readiness probe | K8s readiness (can accept traffic?) |

### Configuration

```json
{
  "HealthChecks": {
    "DiskSpaceMinimumMB": 100,
    "ChromiumTimeoutSeconds": 30
  }
}
```

### Files to Create/Modify

| File | Action |
|------|--------|
| `Infrastructure/HealthChecks/DatabaseHealthCheck.cs` | Create DB health check |
| `Infrastructure/HealthChecks/ChromiumHealthCheck.cs` | Create Chromium health check |
| `Infrastructure/HealthChecks/DiskSpaceHealthCheck.cs` | Create disk space health check |
| `Api/HealthChecks/HealthCheckResponseWriter.cs` | Create JSON response writer |
| `Api/Program.cs` | Register health checks and endpoints |
| `Infrastructure/DependencyInjection.cs` | Register health check services |

### Testing

Unit tests should verify:
- Database health check returns healthy when connected
- Database health check returns unhealthy on connection failure
- Chromium health check returns healthy when browser available
- Chromium health check returns unhealthy when browser unavailable
- Disk space check returns healthy with sufficient space
- Disk space check returns unhealthy with low space
- Response writer formats output correctly
- Liveness endpoint returns 200 without running checks
- Readiness endpoint runs all tagged checks

---

## Prometheus Metrics

Expose application metrics in Prometheus format for monitoring dashboards and alerting.

### Strategy

Use the `prometheus-net.AspNetCore` library to expose a `/metrics` endpoint with standard and custom metrics.

### Metrics to Expose

| Metric | Type | Description |
|--------|------|-------------|
| `http_requests_total` | Counter | Total HTTP requests by method, endpoint, status |
| `http_request_duration_seconds` | Histogram | Request duration distribution |
| `conversion_jobs_total` | Counter | Total conversion jobs by format and status |
| `conversion_job_duration_seconds` | Histogram | Conversion duration by format |
| `conversion_jobs_active` | Gauge | Currently processing jobs |
| `database_connections_active` | Gauge | Active database connections |

### Endpoint

```
GET /metrics
```

Returns Prometheus text format:

```
# HELP http_requests_total Total number of HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="POST",endpoint="/api/v1/convert/html-to-pdf",status="202"} 150
http_requests_total{method="GET",endpoint="/api/v1/convert/{jobId}",status="200"} 450

# HELP conversion_jobs_total Total conversion jobs processed
# TYPE conversion_jobs_total counter
conversion_jobs_total{source_format="html",target_format="pdf",status="completed"} 145
conversion_jobs_total{source_format="markdown",target_format="pdf",status="completed"} 89
conversion_jobs_total{source_format="html",target_format="pdf",status="failed"} 5

# HELP conversion_job_duration_seconds Conversion job duration in seconds
# TYPE conversion_job_duration_seconds histogram
conversion_job_duration_seconds_bucket{source_format="html",target_format="pdf",le="1"} 50
conversion_job_duration_seconds_bucket{source_format="html",target_format="pdf",le="5"} 120
conversion_job_duration_seconds_bucket{source_format="html",target_format="pdf",le="10"} 145
conversion_job_duration_seconds_bucket{source_format="html",target_format="pdf",le="+Inf"} 150
```

### Implementation

#### 1. Install Package

```bash
dotnet add src/FileConversionApi.Api package prometheus-net.AspNetCore
```

#### 2. Create Custom Metrics Service

```csharp
// Infrastructure/Metrics/MetricsService.cs
public interface IMetricsService
{
    void RecordConversionStarted(string sourceFormat, string targetFormat);
    void RecordConversionCompleted(string sourceFormat, string targetFormat, double durationSeconds);
    void RecordConversionFailed(string sourceFormat, string targetFormat);
}

public class PrometheusMetricsService : IMetricsService
{
    private static readonly Counter ConversionJobsTotal = Metrics.CreateCounter(
        "conversion_jobs_total",
        "Total conversion jobs processed",
        new CounterConfiguration
        {
            LabelNames = new[] { "source_format", "target_format", "status" }
        });

    private static readonly Histogram ConversionDuration = Metrics.CreateHistogram(
        "conversion_job_duration_seconds",
        "Conversion job duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "source_format", "target_format" },
            Buckets = new[] { 0.5, 1, 2, 5, 10, 30, 60 }
        });

    private static readonly Gauge ActiveJobs = Metrics.CreateGauge(
        "conversion_jobs_active",
        "Currently processing conversion jobs");

    public void RecordConversionStarted(string sourceFormat, string targetFormat)
    {
        ActiveJobs.Inc();
    }

    public void RecordConversionCompleted(string sourceFormat, string targetFormat, double durationSeconds)
    {
        ActiveJobs.Dec();
        ConversionJobsTotal.WithLabels(sourceFormat, targetFormat, "completed").Inc();
        ConversionDuration.WithLabels(sourceFormat, targetFormat).Observe(durationSeconds);
    }

    public void RecordConversionFailed(string sourceFormat, string targetFormat)
    {
        ActiveJobs.Dec();
        ConversionJobsTotal.WithLabels(sourceFormat, targetFormat, "failed").Inc();
    }
}
```

#### 3. Register Metrics in Program.cs

```csharp
// Program.cs
using Prometheus;

// Add metrics service
builder.Services.AddSingleton<IMetricsService, PrometheusMetricsService>();

// Add HTTP metrics middleware (after app.Build())
app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("endpoint", context => context.Request.Path);
});

// Map metrics endpoint (no auth required for scraping)
app.MapMetrics();
```

#### 4. Instrument Command Handlers

```csharp
// Application/Commands/Conversion/ConvertHtmlToPdfCommandHandler.cs
public async Task<Result<ConversionJobDto>> Handle(
    ConvertHtmlToPdfCommand request,
    CancellationToken cancellationToken)
{
    var stopwatch = Stopwatch.StartNew();
    this.metricsService.RecordConversionStarted("html", "pdf");

    try
    {
        // ... conversion logic ...

        stopwatch.Stop();
        this.metricsService.RecordConversionCompleted("html", "pdf", stopwatch.Elapsed.TotalSeconds);
        return result;
    }
    catch (Exception)
    {
        this.metricsService.RecordConversionFailed("html", "pdf");
        throw;
    }
}
```

### Configuration

```json
{
  "Metrics": {
    "Enabled": true,
    "Endpoint": "/metrics",
    "IncludeHttpMetrics": true,
    "IncludeProcessMetrics": true
  }
}
```

### Files to Create/Modify

| File | Action |
|------|--------|
| `Application/Interfaces/IMetricsService.cs` | Create metrics interface |
| `Infrastructure/Metrics/PrometheusMetricsService.cs` | Create Prometheus implementation |
| `Infrastructure/Options/MetricsSettings.cs` | Create settings class |
| `Api/Program.cs` | Register metrics middleware and endpoint |
| `Infrastructure/DependencyInjection.cs` | Register metrics service |
| `Application/Commands/Conversion/*Handler.cs` | Add metrics instrumentation |
| `Api/FileConversionApi.Api.csproj` | Add prometheus-net.AspNetCore package |
| `appsettings.json` | Add Metrics configuration section |

### Testing

Unit tests should verify:
- MetricsService correctly increments counters
- MetricsService correctly records histograms
- ActiveJobs gauge increments on start and decrements on complete/fail
- Metrics endpoint returns 200 OK
- Metrics output is in valid Prometheus format

### Grafana Dashboard

Example queries for Grafana:
- Request rate: `rate(http_requests_total[5m])`
- Error rate: `rate(http_requests_total{status=~"5.."}[5m])`
- Conversion success rate: `rate(conversion_jobs_total{status="completed"}[5m])`
- P95 conversion duration: `histogram_quantile(0.95, rate(conversion_job_duration_seconds_bucket[5m]))`

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
- Rate limiting per user (see Rate Limiting section)

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
12. ✅ Markdown to HTML conversion works with Markdig
13. ✅ Webhook notifications work for completed/failed jobs
14. ✅ JWT + API Key authentication functional
15. ✅ Rate limiting implemented with per-user and per-endpoint policies
16. ✅ Job cleanup service auto-deletes expired jobs
17. ✅ Health checks report detailed component status (DB, Chromium, disk)
18. ✅ Prometheus metrics endpoint exposes conversion and HTTP metrics
19. ✅ Unit tests exist with 80%+ coverage
20. ✅ docker-compose.yml exists and works
21. ✅ README.md documents how to run the project

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

**Current Status:** Implementing Prometheus metrics feature.
