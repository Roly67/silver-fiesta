# Webhook Notifications for Completed Jobs

Notify external systems when conversion jobs complete or fail.

---

## Overview

Add webhook support to the File Conversion API so users can provide a callback URL that receives notifications when their conversion jobs finish (success or failure).

---

## Architecture Decision

**Approach:** Direct service injection into command handlers

After evaluating options (domain events, EF Core interception), the simplest approach is:
1. Add `WebhookUrl` field to `ConversionJob` entity
2. Create `IWebhookService` interface and implementation
3. Inject webhook service into command handlers
4. Call webhook after `SaveChangesAsync()` completes

This follows existing patterns and keeps the implementation focused.

---

## Files to Create

| File | Purpose |
|------|---------|
| `src/FileConversionApi.Application/Interfaces/IWebhookService.cs` | Service interface |
| `src/FileConversionApi.Infrastructure/Options/WebhookSettings.cs` | Configuration settings |
| `src/FileConversionApi.Infrastructure/Services/WebhookService.cs` | HTTP client implementation |
| `tests/.../WebhookServiceTests.cs` | Unit tests |

---

## Files to Modify

| File | Changes |
|------|---------|
| `src/FileConversionApi.Domain/Entities/ConversionJob.cs` | Add `WebhookUrl` property |
| `src/FileConversionApi.Infrastructure/Persistence/Configurations/ConversionJobConfiguration.cs` | Configure WebhookUrl column |
| `src/FileConversionApi.Application/Commands/Conversion/ConvertHtmlToPdfCommand.cs` | Add WebhookUrl property |
| `src/FileConversionApi.Application/Commands/Conversion/ConvertHtmlToPdfCommandHandler.cs` | Send webhook on completion |
| `src/FileConversionApi.Application/Commands/Conversion/ConvertMarkdownToPdfCommand.cs` | Add WebhookUrl property |
| `src/FileConversionApi.Application/Commands/Conversion/ConvertMarkdownToPdfCommandHandler.cs` | Send webhook on completion |
| `src/FileConversionApi.Api/Models/HtmlToPdfRequest.cs` | Add WebhookUrl property |
| `src/FileConversionApi.Api/Models/MarkdownToPdfRequest.cs` | Add WebhookUrl property |
| `src/FileConversionApi.Infrastructure/DependencyInjection.cs` | Register webhook service + HttpClient |
| `appsettings.json` | Add WebhookSettings section |

---

## Implementation Steps

### Step 1: Add WebhookUrl to Domain Entity

```csharp
// ConversionJob.cs - Add property
public string? WebhookUrl { get; private set; }

// Update Create method signature
public static ConversionJob Create(
    UserId userId,
    string sourceFormat,
    string targetFormat,
    string inputFileName,
    string? webhookUrl = null)
```

### Step 2: Create Settings Class

```csharp
// Infrastructure/Options/WebhookSettings.cs
public class WebhookSettings
{
    public const string SectionName = "WebhookSettings";

    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
```

### Step 3: Create Webhook Service Interface

```csharp
// Application/Interfaces/IWebhookService.cs
public interface IWebhookService
{
    Task SendJobCompletedAsync(ConversionJob job, CancellationToken cancellationToken);
}
```

### Step 4: Create Webhook Service Implementation

```csharp
// Infrastructure/Services/WebhookService.cs
public class WebhookService : IWebhookService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<WebhookService> logger;

    // POST to webhook URL with job details
    // Include: jobId, status, sourceFormat, targetFormat,
    //          inputFileName, outputFileName, errorMessage, completedAt
    // Fire-and-forget with logging (don't fail the request if webhook fails)
}
```

### Step 5: Update Command Handlers

After `SaveChangesAsync()` for completed/failed jobs:
```csharp
if (!string.IsNullOrWhiteSpace(job.WebhookUrl))
{
    await this.webhookService.SendJobCompletedAsync(job, cancellationToken);
}
```

### Step 6: Register Services

```csharp
// Infrastructure/DependencyInjection.cs
services.Configure<WebhookSettings>(configuration.GetSection(WebhookSettings.SectionName));
services.AddHttpClient<IWebhookService, WebhookService>();
```

### Step 7: Add EF Migration

```bash
dotnet ef migrations add AddWebhookUrlToConversionJob
```

---

## Webhook Payload Format

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

---

## API Request Examples

**HTML to PDF with webhook:**
```json
POST /api/v1/convert/html-to-pdf
{
  "htmlContent": "<h1>Hello</h1>",
  "fileName": "document.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion",
  "options": { "pageSize": "A4" }
}
```

**Markdown to PDF with webhook:**
```json
POST /api/v1/convert/markdown-to-pdf
{
  "markdown": "# Hello World",
  "fileName": "document.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion"
}
```

---

## Configuration

```json
// appsettings.json
{
  "WebhookSettings": {
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "RetryDelayMilliseconds": 1000
  }
}
```

---

## Verification

1. **Build:** `dotnet build` - Zero warnings
2. **Tests:** `dotnet test` - All pass including new webhook tests
3. **Manual test:**
   - Start API with `docker-compose up`
   - Use a webhook testing service (e.g., webhook.site)
   - Submit conversion with webhookUrl
   - Verify webhook received with correct payload
4. **Failure case:** Test with invalid webhook URL - job should still complete, webhook failure logged
