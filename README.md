# File Conversion API

A .NET 10 Web API for converting files between different formats, built with Clean Architecture principles.

## Features

- **HTML to PDF Conversion**: Convert HTML content or URLs to PDF using PuppeteerSharp
- **JWT Authentication**: Secure API endpoints with JWT Bearer tokens
- **API Key Authentication**: Alternative authentication method using X-API-Key header
- **PostgreSQL Storage**: Store user data and conversion job history
- **Clean Architecture**: Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command Query Responsibility Segregation with MediatR
- **Structured Logging**: Serilog with console and file sinks

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL 16+
- Docker (optional)

### Running with Docker

```bash
docker-compose up -d
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

### Running Locally

1. Start PostgreSQL:
   ```bash
   docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=fileconversion -p 5432:5432 postgres:16-alpine
   ```

2. Update connection string in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Database=fileconversion;Username=postgres;Password=postgres"
     }
   }
   ```

3. Run the API:
   ```bash
   cd src/FileConversionApi.Api
   dotnet run
   ```

### Running Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Generate coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## API Endpoints

### Authentication

#### Register
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

#### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

#### Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64-encoded-refresh-token"
}
```

### Using JWT Bearer Tokens

After registering or logging in, you receive an `accessToken` that must be included in the `Authorization` header for all protected endpoints:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Lifecycle:**
- Access tokens expire after 60 minutes (configurable)
- When expired, use the refresh token to obtain a new access token
- Refresh tokens expire after 7 days (configurable)

**JWT Claims:**
- `sub`: User ID (GUID)
- `email`: User email address
- `jti`: Unique token identifier
- `exp`: Expiration timestamp

**Decoding Tokens:**
Use [jwt.io](https://jwt.io) or decode locally:
```bash
echo "<token>" | cut -d'.' -f2 | base64 -d 2>/dev/null | jq
```

### API Key Authentication

As an alternative to JWT, you can authenticate using an API key in the `X-API-Key` header:

```http
GET /api/v1/convert/history
X-API-Key: your-api-key-here
```

API keys are generated when a user registers and can be found in the user's profile.

### File Conversion

#### Convert HTML to PDF
```http
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

Or convert from URL:
```json
{
  "url": "https://example.com",
  "fileName": "example.pdf"
}
```

### Jobs

#### Get Job by ID
```http
GET /api/v1/convert/{id}
Authorization: Bearer <token>
```

#### Get Conversion History
```http
GET /api/v1/convert/history?pageNumber=1&pageSize=10
Authorization: Bearer <token>
```

#### Download Job Result
```http
GET /api/v1/convert/{id}/download
Authorization: Bearer <token>
```

### Health Check

```http
GET /health
```

## Configuration

### JWT Settings

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

### Puppeteer Settings

```json
{
  "PuppeteerSettings": {
    "ExecutablePath": "/usr/bin/chromium"
  }
}
```

## Project Structure

```
src/
  FileConversionApi.Domain/        # Domain entities, value objects, errors
  FileConversionApi.Application/   # Commands, queries, interfaces, DTOs
  FileConversionApi.Infrastructure/ # EF Core, repositories, services
  FileConversionApi.Api/           # Controllers, middleware, configuration
tests/
  FileConversionApi.UnitTests/     # Unit tests
  FileConversionApi.IntegrationTests/ # Integration tests
```

## Architecture

This project follows Clean Architecture principles:

- **Domain Layer**: Contains business entities, value objects, and domain errors. No external dependencies.
- **Application Layer**: Contains use cases (commands/queries), DTOs, and interface definitions. Depends only on Domain.
- **Infrastructure Layer**: Contains implementations for repositories, external services, and data access. Depends on Application and Domain.
- **API Layer**: Contains controllers, middleware, and API configuration. Depends on all other layers.

## Error Handling

The API uses RFC 7807 Problem Details for error responses:

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

## License

MIT
