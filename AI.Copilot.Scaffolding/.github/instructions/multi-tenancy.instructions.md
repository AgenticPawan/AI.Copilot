---
applyTo: "**/Tenancy/**,**/TenantDb*,**/HostDb*,**/TenantProvider*,**/TenantResolution*"
description: "Multi-tenancy safety rules for Copilot. Applies to tenant-related files. Enforces database-per-tenant isolation, encrypted connection strings, query filters, and cross-tenant access prevention."
---

# Multi-Tenancy Safety — Copilot

## Architecture
- **DNS-based resolution**: Subdomain extracted by `TenantProvider` (checks X-Forwarded-Host, Host, Origin headers)
- **Database-per-tenant**: Each tenant has a separate SQL Server database
- **Connection strings**: AES-encrypted in host database, decrypted at runtime
- **Host context**: `IsHostContext() == true` when no subdomain detected (admin portal)

## Database Contexts
| Context | Scope | Entities |
|---------|-------|----------|
| `IHostDbContext` | Host database | Tenants, Features, host Users, Permissions, Roles, global entities |
| `ITenantDbContext` | Tenant database | FamilyMembers, Documents, PasswordEntries, tenant-specific data |
| `IUserAccountDbContext` | Shared auth | Works for both host and tenant user operations |
| `IUserAccountDbContextResolver` | Dynamic | `.Resolve()` returns correct context based on current tenant |

## Safety Rules (CRITICAL)
1. **ALWAYS use global query filters**: `HasQueryFilter(e => !e.IsDeleted)` on all entities
2. **ALWAYS apply tenant filter**: TenantDbContext adds `e.TenantId == _tenantId`
3. **NEVER hardcode context selection** — use `IUserAccountDbContextResolver.Resolve()`
4. **NEVER expose connection strings** in DTOs or API responses
5. **ALWAYS encrypt** connection strings before storage using `IEncryptionService`
6. **ALWAYS validate tenant ownership** before update/delete operations
7. **NEVER allow cross-tenant data access** — every query must be tenant-scoped

## Review Checklist
- [ ] New entity has `TenantId` (Guid?) if tenant-scoped
- [ ] Query filter applied in EF configuration
- [ ] Handler uses correct DbContext resolver
- [ ] No cross-tenant data leak possible
- [ ] Connection strings encrypted before storage
- [ ] Tenant deactivation blocks access
