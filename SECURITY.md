# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
|---------|--------------------|
| 1.x.x   | :white_check_mark: |
| < 1.0   | :x:                |

## Reporting a Vulnerability

We take security vulnerabilities seriously. We appreciate your efforts to responsibly disclose your findings.

### How to Report

**Please DO NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them via one of the following methods:

1. **GitHub Security Advisories** (Preferred)
   - Go to the [Security Advisories](https://github.com/Roly67/silver-fiesta/security/advisories) page
   - Click "New draft security advisory"
   - Fill out the form with details about the vulnerability

2. **Email**
   - Send an email to the repository maintainers
   - Include "SECURITY" in the subject line

### What to Include

Please include the following information in your report:

- **Type of vulnerability** (e.g., SQL injection, XSS, authentication bypass)
- **Affected component** (e.g., API endpoint, authentication module)
- **Steps to reproduce** the vulnerability
- **Proof of concept** (if available)
- **Potential impact** of the vulnerability
- **Suggested fix** (if you have one)

### Example Report

```
Type: SQL Injection
Component: /api/v1/convert/history endpoint
Severity: High

Description:
The 'pageNumber' parameter is not properly sanitized...

Steps to Reproduce:
1. Send a GET request to /api/v1/convert/history?pageNumber=1;DROP TABLE Users
2. Observe...

Impact:
An attacker could potentially...

Suggested Fix:
Use parameterized queries for...
```

## Response Timeline

| Action | Timeline |
|--------|----------|
| Initial response | Within 48 hours |
| Vulnerability confirmation | Within 7 days |
| Patch development | Within 30 days (critical: 7 days) |
| Public disclosure | After patch release |

## What to Expect

1. **Acknowledgment**: We will acknowledge receipt of your report within 48 hours.

2. **Assessment**: Our team will investigate and validate the vulnerability. We may contact you for additional information.

3. **Resolution**: We will work on a fix and keep you informed of our progress.

4. **Disclosure**: Once the vulnerability is fixed, we will:
   - Release a security patch
   - Publish a security advisory
   - Credit you for the discovery (unless you prefer to remain anonymous)

## Disclosure Policy

- We follow a **coordinated disclosure** policy
- Please allow us reasonable time to address the vulnerability before public disclosure
- We will coordinate with you on the disclosure timeline
- We will credit reporters who follow responsible disclosure practices

## Security Best Practices

When deploying this API, please ensure:

### Authentication & Authorization

- [ ] Use strong, unique JWT secrets (minimum 32 characters)
- [ ] Rotate JWT secrets periodically
- [ ] Use HTTPS in production
- [ ] Implement rate limiting
- [ ] Review API key permissions regularly

### Configuration

```json
{
  "JwtSettings": {
    "Secret": "USE_A_STRONG_SECRET_MIN_32_CHARS_GENERATED_SECURELY",
    "TokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Database

- [ ] Use strong database passwords
- [ ] Enable SSL/TLS for database connections
- [ ] Restrict database user permissions
- [ ] Regular backups with encryption

### Docker

- [ ] Run containers as non-root user
- [ ] Use specific image tags, not `latest`
- [ ] Scan images for vulnerabilities
- [ ] Don't expose unnecessary ports

### Environment Variables

Never commit secrets to version control. Use:
- Environment variables
- Secret management services (Azure Key Vault, AWS Secrets Manager, etc.)
- Docker secrets

## Security Features

This API includes the following security features:

| Feature | Description |
|---------|-------------|
| **JWT Authentication** | Stateless token-based authentication |
| **API Key Authentication** | Alternative auth for service-to-service |
| **Password Hashing** | BCrypt with configurable work factor |
| **Input Validation** | FluentValidation on all inputs |
| **CORS** | Configurable cross-origin policies |
| **Rate Limiting** | Protect against abuse |
| **Security Headers** | HSTS, X-Content-Type-Options, etc. |

## Security Scanning

We employ automated security scanning:

- **CodeQL** - Static analysis for C# vulnerabilities
- **Dependabot** - Dependency vulnerability monitoring
- **Trivy** - Container image scanning
- **TruffleHog** - Secret detection in code

## Hall of Fame

We would like to thank the following security researchers for responsibly disclosing vulnerabilities:

*No submissions yet. Be the first!*

---

Thank you for helping keep File Conversion API and its users safe! 🛡️
