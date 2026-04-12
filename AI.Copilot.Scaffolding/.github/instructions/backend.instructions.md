---
applyTo: "**/*.cs"
description: "Backend .NET coding standards for Copilot. Applies automatically to all C# files. Enforces Clean Architecture, CQRS, entity patterns, Result<T>, multi-tenancy, and security requirements."
---

# .NET Backend Standards — Copilot

## Architecture Layers (NEVER violate)
- **Domain** (`Copilot.Domain/`) = Entities with private constructors, static Create() factories, private setters, behavior methods. NO framework dependencies.
- **Application** (`Copilot.Application/`) = CQRS handlers returning `Result<T>`, FluentValidation validators, DTOs, interfaces.
- **Infrastructure** (`Copilot.Infrastructure/`) = JWT, BCrypt, AES, tenant resolution, external integrations.
- **Persistence** (`Copilot.Persistence/`) = EF Core DbContexts with repository methods ON context (NO separate repository classes).
- **Api** (`Copilot.Api/`) = Thin controllers: authorize → dispatch MediatR → check Result → return HTTP status.

## Entity Rules
- Private parameterless constructor for EF Core
- Static `Create()` factory method
- ALL properties have `private set`
- State changes via named behavior methods (e.g., `Activate()`, `UpdateName()`)
- Inherit from `BaseEntity`

## Handler Rules
- Return `Result<T>` — never throw for business logic failures
- Accept `CancellationToken` on all async methods
- Use `IUserAccountDbContextResolver.Resolve()` for user-scoped operations
- Use `IHostDbContext` for host-only operations
- Wrap in try/catch with structured `ILogger<T>` logging
- Add audit logging for state-changing operations

## Controller Rules
- `[Authorize(Policy = "Permission:{Entity}.{Action}")]` on EVERY action
- `[OutputCache(PolicyName = "TenantAware")]` on GET endpoints
- Dispatch MediatR command/query only — NO business logic
- Evict cache + send CRUD notifications after mutations
- Send progress via `ITransactionStreamService` for create/update

## Multi-Tenancy
- Tenant-scoped entities MUST have `TenantId` (Guid?) property
- TenantDbContext applies query filter: `e.TenantId == _tenantId`
- Host data has `TenantId = null`
- Connection strings are AES-encrypted before storage
- ALWAYS consider: "Can Tenant A access Tenant B's data through this code path?"

## Security
- FluentValidation for all commands
- No raw SQL — use EF Core LINQ
- No sensitive data in DTOs
- No hardcoded secrets
- BCrypt for passwords (work factor 12)
- AES-256 for sensitive data at rest

## Performance
- `AsNoTracking()` for read-only queries
- Projections (`Select`) over full entity loading
- `ToPagedResultAsync()` for list endpoints
- Avoid N+1 queries — use `Include()` or projection
