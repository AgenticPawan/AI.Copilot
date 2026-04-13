---
applyTo: "**/Security/**,**/Authorization/**,**/Middleware/**,**/JWT*,**/Encrypt*,**/BCrypt*"
description: "Security standards for {{projectName}}. Applies to security-related files. Enforces permission-based authorization, encryption, password hashing, and OWASP Top 10 protections."
---

# Security Standards — {{projectName}}

## Authentication Stack
| Layer | Technology | Details |
|-------|-----------|---------|
| Tokens | JWT | Access + Refresh token pair |
| Passwords | BCrypt | Work factor 12 |
| Encryption | AES-256 | Connection strings, documents, passwords at rest |
| Authorization | Permission-based | 28 policies across 8 groups |

## Permission Groups (28 policies)
```
Users.{Create|Edit|Delete|View}
Roles.{Create|Edit|Delete|View}
Tenants.{Create|Manage|View}
Settings.{Edit|View}
Notifications.{Send|View}
Features.Manage
AuditLogs.View
Localization.Manage
Documents.{Create|Edit|Delete|View}
Passwords.{Create|Edit|Delete|View}
FamilyMembers.{Create|Edit|Delete|View}
```

## Every Endpoint Must Have
- `[Authorize(Policy = "Permission:{Entity}.{Action}")]`
- Input validation via FluentValidation
- No sensitive data in response DTOs

## Encryption Rules
- Connection strings → AES-encrypt before storing in host DB
- Document content → AES-encrypt at rest
- Passwords → BCrypt hash (NEVER reversible encryption)
- JWT secret key → store in environment variables, NEVER in source code
- Encryption keys → rotate periodically, store securely

## OWASP Top 10 Compliance
1. **Injection**: Use EF Core parameterized queries only — NO raw SQL
2. **Broken Auth**: JWT expiry, refresh rotation, login lockout after 5 failures
3. **Sensitive Data**: AES-256 for data at rest, HTTPS for data in transit
4. **XXE**: Not applicable (JSON APIs only)
5. **Broken Access**: Permission checks on every endpoint + tenant isolation
6. **Misconfiguration**: No default passwords, encrypted appsettings
7. **XSS**: Angular auto-escaping + CSP headers
8. **Insecure Deserialization**: Model validation via FluentValidation
9. **Known Vulnerabilities**: Regular NuGet/npm updates
10. **Logging**: Structured logging via Serilog, correlation IDs, audit trails
