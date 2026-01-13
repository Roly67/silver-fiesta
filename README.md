<div align="center">

# 📄 File Conversion API

**Transform documents at scale with a modern, secure REST API**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

[![CI](https://github.com/Roly67/silver-fiesta/actions/workflows/ci.yml/badge.svg)](https://github.com/Roly67/silver-fiesta/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Roly67/silver-fiesta/graph/badge.svg)](https://codecov.io/gh/Roly67/silver-fiesta)
[![Docs](https://img.shields.io/badge/Docs-GitHub%20Pages-blue?style=flat&logo=github)](https://roly67.github.io/silver-fiesta/)

<br />

[Getting Started](#-getting-started) · [API Reference](#-api-reference) · [Architecture](#-architecture) · [Configuration](#-configuration)

<br />

---

</div>

## ✨ Features

<table>
<tr>
<td width="50%">

### 🔄 Document Conversion
Convert HTML, Markdown, or URLs to PDF. Convert Markdown to HTML. Transform images between PNG, JPEG, and WebP formats with resize and quality options.

</td>
<td width="50%">

### 🔐 Dual Authentication
Secure your endpoints with JWT Bearer tokens for user sessions or API keys for service-to-service communication.

</td>
</tr>
<tr>
<td width="50%">

### 🏗️ Clean Architecture
Four-layer architecture (Domain, Application, Infrastructure, API) with CQRS pattern powered by MediatR.

</td>
<td width="50%">

### 📊 Job Tracking
Monitor conversion progress, access history, and download results with comprehensive job management.

</td>
</tr>
<tr>
<td width="50%">

### 🔔 Webhook Notifications
Receive HTTP callbacks when conversion jobs complete or fail. Configure per-request webhook URLs for real-time integration.

</td>
<td width="50%">

### 🚦 Rate Limiting
Built-in rate limiting with per-user and per-endpoint policies. Protects against abuse with configurable limits and proper 429 responses.

</td>
</tr>
<tr>
<td width="50%">

### 🧹 Auto-Cleanup
Background service automatically deletes expired jobs. Configurable retention periods for completed and failed jobs prevent database bloat.

</td>
<td width="50%">

### 🐳 Multi-Arch Docker
Production-ready containers for both AMD64 and ARM64 architectures, available from GitHub Container Registry.

</td>
</tr>
<tr>
<td width="50%">

### 🖼️ Image Conversions
Convert images between PNG, JPEG, and WebP formats using ImageSharp. Supports resize, quality settings, and maintains aspect ratio.

</td>
<td width="50%">

### 📈 Prometheus Metrics
Built-in `/metrics` endpoint exposes conversion statistics, HTTP request metrics, and system health for Grafana dashboards.

</td>
</tr>
<tr>
<td width="50%">

### 🩺 Enhanced Health Checks
Detailed `/health` endpoint reports database connectivity, Chromium availability, and disk space status with degraded state detection.

</td>
<td width="50%">

### 📋 Production Ready
Comprehensive logging with Serilog, Swagger documentation, and zero-warning builds with StyleCop analyzers.

</td>
</tr>
<tr>
<td width="50%">

### 📦 Batch Conversions
Process up to 20 conversion requests in a single API call with partial success support and per-item error reporting.

</td>
<td width="50%">

### 🔗 PDF Operations
Merge multiple PDFs, split by page ranges, add watermarks, and protect with passwords using PdfSharpCore.

</td>
</tr>
<tr>
<td width="50%">

### 👑 Admin API
Role-based admin endpoints for user management, job statistics, and system monitoring. Disable users, reset API keys, and grant admin privileges.

</td>
<td width="50%">

### 📝 Conversion Templates
Save and reuse conversion settings with named templates. Define page sizes, margins, watermarks, and other options for consistent output.

</td>
</tr>
<tr>
<td width="50%">

### 🔭 OpenTelemetry Tracing
Distributed tracing with OpenTelemetry for end-to-end request visibility. Export traces to Jaeger, Zipkin, or any OTLP-compatible backend.

</td>
<td width="50%">

</td>
</tr>
</table>

<br />

## 🚀 Getting Started

### Prerequisites

| Requirement | Version |
|------------|---------|
| .NET SDK | 10.0+ |
| PostgreSQL | 16+ |
| Docker | Optional |

### Quick Start with Docker

```bash
docker-compose up -d
```

> **API Endpoint:** `http://localhost:5000`
>
> The development setup uses HTTP only. For production, configure HTTPS with proper certificates.

**Pull from GitHub Container Registry:**

```bash
# Multi-arch image (amd64/arm64)
docker pull ghcr.io/roly67/silver-fiesta:latest
```

<details>
<summary><strong>📋 Manual Setup</strong></summary>

<br />

**1. Start PostgreSQL**
```bash
docker run -d \
  --name postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=fileconversion \
  -p 5432:5432 \
  postgres:16-alpine
```

**2. Configure Connection String**

Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=fileconversion;Username=postgres;Password=postgres"
  }
}
```

**3. Run the API**
```bash
cd src/FileConversionApi.Api
dotnet run
```

</details>

<details>
<summary><strong>🧪 Running Tests</strong></summary>

<br />

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

</details>

<br />

---

<br />

## 📡 API Reference

### Authentication

<table>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/auth/register</code></td>
<td>Create a new account</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/auth/login</code></td>
<td>Authenticate and receive tokens</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/auth/refresh</code></td>
<td>Refresh an expired access token</td>
</tr>
</table>

<details>
<summary><strong>Register / Login Request</strong></summary>

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

</details>

<details>
<summary><strong>Token Response</strong></summary>

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

</details>

<br />

### 🔑 Using JWT Bearer Tokens

Include the access token in the `Authorization` header for protected endpoints:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

| Token Type | Lifetime | Refresh Strategy |
|-----------|----------|------------------|
| Access Token | 60 min | Use refresh token endpoint |
| Refresh Token | 7 days | Re-authenticate |

<details>
<summary><strong>JWT Claims Reference</strong></summary>

| Claim | Description |
|-------|-------------|
| `sub` | User ID (GUID) |
| `email` | User email address |
| `jti` | Unique token identifier |
| `exp` | Expiration timestamp |

**Decode tokens locally:**
```bash
echo "<token>" | cut -d'.' -f2 | base64 -d 2>/dev/null | jq
```

</details>

<br />

### 🔐 API Key Authentication

Alternative to JWT for service-to-service communication:

```
X-API-Key: your-api-key-here
```

<br />

### File Conversion

<table>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/html-to-pdf</code></td>
<td>Convert HTML/URL to PDF</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/markdown-to-pdf</code></td>
<td>Convert Markdown to PDF</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/markdown-to-html</code></td>
<td>Convert Markdown to HTML</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/image</code></td>
<td>Convert image formats (PNG, JPEG, WebP)</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/pdf/merge</code></td>
<td>Merge multiple PDFs into one</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/pdf/split</code></td>
<td>Split PDF into multiple files</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/convert/batch</code></td>
<td>Batch convert multiple files</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/convert/{id}</code></td>
<td>Get job status</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/convert/{id}/download</code></td>
<td>Download result</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/convert/history</code></td>
<td>List conversion history</td>
</tr>
</table>

<details>
<summary><strong>Convert HTML to PDF</strong></summary>

```json
POST /api/v1/convert/html-to-pdf
Authorization: Bearer <token>
Content-Type: application/json

{
  "htmlContent": "<html><body><h1>Hello World</h1></body></html>",
  "fileName": "document.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion",
  "options": {
    "pageSize": "A4",
    "landscape": false,
    "marginTop": 10,
    "marginBottom": 10,
    "marginLeft": 10,
    "marginRight": 10,
    "waitForJavaScript": true,
    "javaScriptTimeout": 30000
  }
}
```

**Convert from URL:**
```json
{
  "url": "https://example.com",
  "fileName": "example.pdf"
}
```

</details>

<details>
<summary><strong>Convert Markdown to PDF</strong></summary>

```json
POST /api/v1/convert/markdown-to-pdf
Authorization: Bearer <token>
Content-Type: application/json

{
  "markdown": "# Hello World\n\nThis is **bold** and *italic* text.",
  "fileName": "document.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion",
  "options": {
    "pageSize": "A4",
    "landscape": false,
    "marginTop": 25,
    "marginBottom": 25,
    "marginLeft": 20,
    "marginRight": 20
  }
}
```

**Supported Markdown features:**
- Headings, paragraphs, lists
- Bold, italic, strikethrough
- Code blocks with syntax highlighting
- Tables, blockquotes
- Links and images

</details>

<details>
<summary><strong>PDF Watermarking</strong></summary>

Add watermarks to PDF output by including the `watermark` option in any PDF conversion request:

```json
{
  "htmlContent": "<html><body><h1>My Document</h1></body></html>",
  "fileName": "document.pdf",
  "options": {
    "watermark": {
      "text": "CONFIDENTIAL",
      "fontSize": 48,
      "fontFamily": "Helvetica",
      "color": "#FF0000",
      "opacity": 0.3,
      "rotation": -45,
      "position": "Center",
      "allPages": true
    }
  }
}
```

**Watermark Options:**
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `text` | string | required | The watermark text |
| `fontSize` | int | 48 | Font size in points |
| `fontFamily` | string | Helvetica | Font family name |
| `color` | string | #808080 | Color in hex format |
| `opacity` | double | 0.3 | Opacity (0.0 to 1.0) |
| `rotation` | double | -45 | Rotation angle in degrees |
| `position` | string | Center | Position on page |
| `allPages` | bool | true | Apply to all pages |
| `pageNumbers` | int[] | null | Specific pages (if allPages is false) |

**Position Values:**
`Center`, `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight`, `Tile`

</details>

<details>
<summary><strong>PDF Password Protection</strong></summary>

Encrypt PDF output with passwords and set permissions by including the `passwordProtection` option in any PDF conversion request:

```json
{
  "htmlContent": "<html><body><h1>Secure Document</h1></body></html>",
  "fileName": "document.pdf",
  "options": {
    "passwordProtection": {
      "userPassword": "viewerpass123",
      "ownerPassword": "adminpass456",
      "allowPrinting": true,
      "allowCopyingContent": false,
      "allowModifying": false,
      "allowAnnotations": false
    }
  }
}
```

**Password Protection Options:**
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `userPassword` | string | required | Password to open/view the PDF |
| `ownerPassword` | string | null | Password for full access (defaults to userPassword) |
| `allowPrinting` | bool | true | Allow printing the document |
| `allowCopyingContent` | bool | true | Allow copying text/images |
| `allowModifying` | bool | false | Allow modifying the document |
| `allowAnnotations` | bool | false | Allow adding annotations |

**Notes:**
- The `userPassword` is required to enable encryption
- If `ownerPassword` is not specified, it defaults to the `userPassword`
- Owner password grants full access regardless of permission settings
- Can be combined with watermarking for additional document protection

</details>

<details>
<summary><strong>PDF Merge</strong></summary>

Merge multiple PDF documents into a single PDF:

```json
POST /api/v1/convert/pdf/merge
Authorization: Bearer <token>
Content-Type: application/json

{
  "pdfDocuments": [
    "base64-encoded-pdf-1",
    "base64-encoded-pdf-2",
    "base64-encoded-pdf-3"
  ],
  "fileName": "merged.pdf",
  "webhookUrl": "https://example.com/webhooks/conversion"
}
```

**Notes:**
- At least two PDF documents are required
- PDFs are merged in the order provided
- Output is a single merged PDF file

</details>

<details>
<summary><strong>PDF Split</strong></summary>

Split a PDF document into multiple PDFs:

```json
POST /api/v1/convert/pdf/split
Authorization: Bearer <token>
Content-Type: application/json

{
  "pdfData": "base64-encoded-pdf",
  "fileName": "document.pdf",
  "options": {
    "pageRanges": ["1-3", "5", "7-10"],
    "splitIntoSinglePages": false
  },
  "webhookUrl": "https://example.com/webhooks/conversion"
}
```

**Split Options:**
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `pageRanges` | string[] | null | Page ranges to extract (e.g., "1-3", "5") |
| `splitIntoSinglePages` | bool | false | Split into individual pages |

**Notes:**
- Output is a ZIP file containing the split PDFs
- If no options specified, defaults to splitting into single pages
- Page numbers are 1-based
- Page ranges use format "start-end" (e.g., "1-5") or single page (e.g., "3")

</details>

<details>
<summary><strong>Convert Markdown to HTML</strong></summary>

```json
POST /api/v1/convert/markdown-to-html
Authorization: Bearer <token>
Content-Type: application/json

{
  "markdown": "# Hello World\n\nThis is **bold** and *italic* text.",
  "fileName": "document.html",
  "webhookUrl": "https://example.com/webhooks/conversion"
}
```

Returns styled HTML with professional CSS including:
- Typography and code syntax highlighting
- Table styling and blockquotes
- Responsive design

</details>

<details>
<summary><strong>Convert Image Formats</strong></summary>

```json
POST /api/v1/convert/image
Authorization: Bearer <token>
Content-Type: application/json

{
  "imageData": "base64-encoded-image-data",
  "sourceFormat": "png",
  "targetFormat": "jpeg",
  "fileName": "photo.png",
  "webhookUrl": "https://example.com/webhooks/conversion",
  "options": {
    "imageWidth": 800,
    "imageHeight": 600,
    "imageQuality": 85
  }
}
```

**Supported formats:** PNG, JPEG, WebP, GIF, BMP

**Options:**
| Option | Description |
|--------|-------------|
| `imageWidth` | Target width in pixels (maintains aspect ratio) |
| `imageHeight` | Target height in pixels (maintains aspect ratio) |
| `imageQuality` | Quality 1-100 (for JPEG/WebP) |

</details>

<details>
<summary><strong>Batch Conversion</strong></summary>

Convert multiple files in a single request (max 20 items):

```json
POST /api/v1/convert/batch
Authorization: Bearer <token>
Content-Type: application/json

{
  "items": [
    {
      "type": "html-to-pdf",
      "htmlContent": "<html><body>Document 1</body></html>",
      "fileName": "doc1.html"
    },
    {
      "type": "markdown-to-pdf",
      "markdown": "# Document 2",
      "fileName": "doc2.md"
    },
    {
      "type": "image",
      "imageData": "base64-encoded-data",
      "sourceFormat": "png",
      "targetFormat": "jpeg"
    }
  ],
  "webhookUrl": "https://example.com/webhook"
}
```

**Response (200 OK):**
```json
{
  "totalItems": 3,
  "successCount": 3,
  "failureCount": 0,
  "results": [
    {
      "index": 0,
      "success": true,
      "job": { "id": "...", "status": "Completed", ... }
    },
    {
      "index": 1,
      "success": true,
      "job": { "id": "...", "status": "Completed", ... }
    },
    {
      "index": 2,
      "success": true,
      "job": { "id": "...", "status": "Completed", ... }
    }
  ]
}
```

**Supported types:** `html-to-pdf`, `markdown-to-pdf`, `markdown-to-html`, `image`

</details>

<br />

### Health & Monitoring

<table>
<tr>
<td><code>GET</code></td>
<td><code>/health</code></td>
<td>Detailed health status (DB, Chromium, disk)</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/metrics</code></td>
<td>Prometheus metrics endpoint</td>
</tr>
</table>

<details>
<summary><strong>Health Check Response</strong></summary>

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "PostgreSQL connection successful"
    },
    "chromium": {
      "status": "Healthy",
      "description": "Chromium is available for PDF generation"
    },
    "disk_space": {
      "status": "Healthy",
      "description": "Disk space: 50.2 GB free (62.5%)"
    }
  }
}
```

**Status values:** `Healthy`, `Degraded`, `Unhealthy`

</details>

<details>
<summary><strong>Prometheus Metrics</strong></summary>

The `/metrics` endpoint exposes:

| Metric | Type | Description |
|--------|------|-------------|
| `conversion_requests_total` | Counter | Total conversion requests by format and status |
| `conversion_duration_seconds` | Histogram | Conversion duration by format |
| `http_requests_total` | Counter | HTTP requests by method, path, status |
| `http_request_duration_seconds` | Histogram | HTTP request duration |

**Example Prometheus scrape config:**
```yaml
scrape_configs:
  - job_name: 'fileconversion-api'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'
```

</details>

<br />

### Admin API

<table>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/admin/users</code></td>
<td>List all users (paginated)</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/admin/users/{id}</code></td>
<td>Get user details</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/admin/users/{id}/disable</code></td>
<td>Disable a user</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/admin/users/{id}/enable</code></td>
<td>Enable a user</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/admin/users/{id}/reset-api-key</code></td>
<td>Reset user's API key</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/admin/users/{id}/grant-admin</code></td>
<td>Grant admin privileges</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/admin/users/{id}/revoke-admin</code></td>
<td>Revoke admin privileges</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/admin/stats</code></td>
<td>Get job statistics</td>
</tr>
</table>

<details>
<summary><strong>Admin Endpoints (requires Admin role)</strong></summary>

**Get Job Statistics:**
```json
GET /api/v1/admin/stats
Authorization: Bearer <admin-token>

Response:
{
  "totalJobs": 1500,
  "completedJobs": 1200,
  "failedJobs": 50,
  "pendingJobs": 250,
  "totalUsers": 45,
  "successRate": 80.0
}
```

**List Users:**
```json
GET /api/v1/admin/users?page=1&pageSize=20
Authorization: Bearer <admin-token>

Response:
{
  "items": [
    {
      "id": "...",
      "email": "user@example.com",
      "isActive": true,
      "isAdmin": false,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3
}
```

</details>

<br />

### Conversion Templates

<table>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/templates</code></td>
<td>List user's templates (optional ?targetFormat= filter)</td>
</tr>
<tr>
<td><code>GET</code></td>
<td><code>/api/v1/templates/{id}</code></td>
<td>Get template details</td>
</tr>
<tr>
<td><code>POST</code></td>
<td><code>/api/v1/templates</code></td>
<td>Create new template</td>
</tr>
<tr>
<td><code>PUT</code></td>
<td><code>/api/v1/templates/{id}</code></td>
<td>Update template</td>
</tr>
<tr>
<td><code>DELETE</code></td>
<td><code>/api/v1/templates/{id}</code></td>
<td>Delete template</td>
</tr>
</table>

<details>
<summary><strong>Template Examples</strong></summary>

**Create Template:**
```json
POST /api/v1/templates
Authorization: Bearer <token>

{
  "name": "A4 Landscape Report",
  "description": "Standard report format with company watermark",
  "targetFormat": "pdf",
  "options": {
    "pageSize": "A4",
    "landscape": true,
    "marginTop": 25,
    "marginBottom": 25,
    "watermark": {
      "text": "CONFIDENTIAL",
      "opacity": 0.2
    }
  }
}
```

**List Templates:**
```json
GET /api/v1/templates?targetFormat=pdf
Authorization: Bearer <token>

Response:
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "A4 Landscape Report",
    "description": "Standard report format with company watermark",
    "targetFormat": "pdf",
    "options": { ... },
    "createdAt": "2026-01-13T12:00:00Z"
  }
]
```

**Supported target formats:** `pdf`, `html`, `png`, `jpeg`, `webp`, `gif`, `bmp`

</details>

<br />

---

<br />

## 🏛️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                          API Layer                              │
│              Controllers · Middleware · Configuration           │
├─────────────────────────────────────────────────────────────────┤
│                      Application Layer                          │
│            Commands · Queries · DTOs · Interfaces               │
├─────────────────────────────────────────────────────────────────┤
│                     Infrastructure Layer                        │
│          EF Core · Repositories · External Services             │
├─────────────────────────────────────────────────────────────────┤
│                        Domain Layer                             │
│            Entities · Value Objects · Domain Errors             │
└─────────────────────────────────────────────────────────────────┘
```

<details>
<summary><strong>Project Structure</strong></summary>

```
src/
├── FileConversionApi.Domain/          # Entities, value objects, errors
├── FileConversionApi.Application/     # Commands, queries, interfaces, DTOs
├── FileConversionApi.Infrastructure/  # EF Core, repositories, services
└── FileConversionApi.Api/             # Controllers, middleware, config

tests/
├── FileConversionApi.UnitTests/       # Unit tests
└── FileConversionApi.IntegrationTests/# Integration tests
```

</details>

<details>
<summary><strong>Layer Responsibilities</strong></summary>

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **Domain** | Business entities, value objects, domain errors | None |
| **Application** | Use cases (CQRS), DTOs, interface definitions | Domain |
| **Infrastructure** | Repositories, external services, data access | Application, Domain |
| **API** | Controllers, middleware, configuration | All layers |

</details>

<br />

---

<br />

## ⚙️ Configuration

<details>
<summary><strong>JWT Settings</strong></summary>

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "FileConversionApi",
    "Audience": "FileConversionApi",
    "TokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

</details>

<details>
<summary><strong>Puppeteer Settings</strong></summary>

```json
{
  "PuppeteerSettings": {
    "ExecutablePath": null,
    "Timeout": 30000
  }
}
```

> **Note:** When `ExecutablePath` is `null`, PuppeteerSharp automatically downloads a compatible Chromium version. This is the recommended approach for Docker deployments.

</details>

<details>
<summary><strong>Webhook Settings</strong></summary>

```json
{
  "WebhookSettings": {
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "RetryDelayMilliseconds": 1000
  }
}
```

> **Note:** Webhook notifications are sent when conversion jobs complete or fail. Failed webhook calls are retried automatically.

</details>

<details>
<summary><strong>Rate Limiting Settings</strong></summary>

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

| Policy | Limit | Window | Endpoints |
|--------|-------|--------|-----------|
| `standard` | 100 requests | 1 hour | GET endpoints (status, download, history) |
| `conversion` | 50 requests | 1 hour | POST conversion endpoints |
| `auth` | 10 requests | 15 min | Authentication endpoints |

> **Note:** Rate limits are applied per-user for authenticated requests and per-IP for anonymous requests. When rate limited, the API returns HTTP 429 with a `Retry-After` header.

</details>

<details>
<summary><strong>Job Cleanup Settings</strong></summary>

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
| `Enabled` | true | Enable/disable the cleanup service |
| `RunIntervalMinutes` | 60 | How often to run cleanup |
| `CompletedJobRetentionDays` | 7 | Days to keep completed jobs |
| `FailedJobRetentionDays` | 30 | Days to keep failed jobs (longer for debugging) |
| `BatchSize` | 100 | Max jobs to delete per run |

> **Note:** The cleanup service runs as a background hosted service. Failed jobs are retained longer to allow for debugging issues.

</details>

<details>
<summary><strong>OpenTelemetry Settings</strong></summary>

```json
{
  "OpenTelemetry": {
    "EnableTracing": true,
    "ServiceName": "FileConversionApi",
    "OtlpEndpoint": "http://localhost:4317",
    "ExportToConsole": false,
    "SamplingRatio": 1.0
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `EnableTracing` | true | Enable/disable distributed tracing |
| `ServiceName` | FileConversionApi | Service name in traces |
| `OtlpEndpoint` | null | OTLP exporter endpoint (e.g., Jaeger, Zipkin) |
| `ExportToConsole` | false | Output traces to console (dev only) |
| `SamplingRatio` | 1.0 | Trace sampling ratio (0.0-1.0) |

**Instrumented operations:**
- HTTP requests (ASP.NET Core)
- Outbound HTTP calls (HttpClient)
- Database queries (Entity Framework Core)
- Conversion operations (custom spans)

> **Note:** To view traces, run Jaeger locally: `docker run -d -p 4317:4317 -p 16686:16686 jaegertracing/all-in-one:latest` and set `OtlpEndpoint` to `http://localhost:4317`.

</details>

<br />

---

<br />

## 🚨 Error Handling

The API uses [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details for standardized error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": ["Email is required"]
  }
}
```

<br />

---

<br />

<div align="center">

## 📄 License

MIT © 2026

<br />

**Built with ❤️ using .NET 10 and Clean Architecture**

</div>
