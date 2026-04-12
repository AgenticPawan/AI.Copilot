---
mode: 'agent'
description: "Automate new tenant provisioning. Use when: onboarding a new tenant — database creation, migration, seeding, host registration with encrypted connection string, DNS configuration, and verification."
---

# Workflow: New Tenant Provisioning

Automate the provisioning of a new tenant in VirtualStudio.

## Input Required
- **Tenant Name**: Subdomain-safe name (e.g., `acmecorp`)
- **Display Name**: Human-readable name (e.g., `Acme Corporation`)
- **Admin Email**: Tenant admin's email address
- **Database Server**: SQL Server instance for the tenant database

## Workflow Steps

### Step 1: Create Tenant Database
```sql
CREATE DATABASE VS_Tenant_{TenantName};
```

### Step 2: Build Connection String
```
Server={DbServer};Database=VS_Tenant_{TenantName};Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;
```

### Step 3: Run Tenant Migrations
Use the Migrator console app or EF CLI:
```bash
cd src/backend/VirtualStudio.Migrator
dotnet run -- --tenant-connection "{ConnectionString}"
```
This runs all `TenantDbContext` migrations against the new database.

### Step 4: Seed Tenant Data
The Migrator seeds:
- Default permissions (28 tenant-applicable permissions)
- Default roles (Admin, User with appropriate permission assignments)
- Admin user (with provided email, auto-generated temporary password)
- Default app settings
- Feature flags (all enabled by default)

### Step 5: Register in Host Database
Create tenant record via API or direct handler:
```
POST /api/tenants
{
    "tenancyName": "{TenantName}",
    "displayName": "{DisplayName}",
    "connectionString": "{ConnectionString}",
    "adminEmail": "{AdminEmail}",
    "adminPassword": "{TempPassword}"
}
```
Connection string is AES-encrypted before storage.

### Step 6: DNS Configuration
Configure subdomain routing:
- `{TenantName}.yourdomain.com` → Application ingress
- The `TenantResolutionMiddleware` extracts subdomain and resolves the tenant

### Step 7: Verification Checklist
- [ ] Navigate to `{TenantName}.yourdomain.com` → Shows tenant login page
- [ ] Login with admin credentials → Dashboard loads
- [ ] Verify tenant-specific data isolation → No host data visible
- [ ] Verify feature flags → Enabled features accessible
- [ ] Verify role permissions → Admin has full access
- [ ] Test a CRUD operation → Data persists to tenant database
