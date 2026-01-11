<div align="center">

# 📄 File Conversion API

**Transform documents at scale with a modern, secure REST API**

[![CI](https://github.com/Roly67/silver-fiesta/actions/workflows/ci.yml/badge.svg)](https://github.com/Roly67/silver-fiesta/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Roly67/silver-fiesta/graph/badge.svg)](https://codecov.io/gh/Roly67/silver-fiesta)
[![Docs](https://img.shields.io/badge/Docs-GitHub%20Pages-blue?style=flat&logo=github)](https://roly67.github.io/silver-fiesta/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

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
Convert HTML content or URLs to pixel-perfect PDFs using PuppeteerSharp with full control over page size, margins, and rendering options.

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

> **API Endpoints**
> HTTP: `http://localhost:5000`
> HTTPS: `https://localhost:5001`

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

<br />

### Health Check

```
GET /health
```

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
    "ExecutablePath": "/usr/bin/chromium"
  }
}
```

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
