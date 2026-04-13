---
agent: agent
description: "Generate an EF Core migration for database schema changes. Use when: adding new entities, columns, indexes, or modifying table structure. Covers HostDbContext and TenantDbContext migrations with review steps."
---

# Backend: Database Migration

Generate EF Core migration for schema changes in {{projectName}}.

## Input Required
- **Change Description**: What changed (new entity, new column, index, etc.)
- **Target Context**: `HostDbContext` or `TenantDbContext`

## Steps

### 1. Verify Entity Configuration
Check `src/backend/{{projectName}}.Persistence/Configurations/EntityConfigurations.cs`:
- Entity has `IEntityTypeConfiguration<T>` implementation
- Table name, key, constraints, indexes are defined
- `HasQueryFilter(e => !e.IsDeleted)` is applied
- `AuditFieldConfigurations.ConfigureAuditFields<T>(builder)` is called

### 2. Verify DbContext Registration
For **HostDbContext** changes:
- DbSet property exists in `HostDbContext.cs`
- Interface method exists in `IHostDbContext.cs`

For **TenantDbContext** changes:
- DbSet property exists in `TenantDbContext.cs`
- Entity is NOT in the `modelBuilder.Ignore<T>()` list (unless host-only)
- Tenant query filter applied if entity has TenantId

### 3. Generate Migration Command
```bash
# For Host database changes
cd src/backend
dotnet ef migrations add {MigrationName} \
  --project {{projectName}}.Persistence \
  --startup-project {{projectName}}.Api \
  --context HostDbContext \
  --output-dir Migrations/Host

# For Tenant database changes
cd src/backend
dotnet ef migrations add {MigrationName} \
  --project {{projectName}}.Persistence \
  --startup-project {{projectName}}.Api \
  --context TenantDbContext \
  --output-dir Migrations/Tenant
```

### 4. Review Generated Migration
- Check the `Up()` and `Down()` methods
- Verify column types, nullable settings, default values
- Ensure indexes are correct
- Verify foreign key cascade behavior (use `DeleteBehavior.NoAction` for self-referencing)

### 5. Seeder Updates (If Needed)
Update seeders in `src/backend/{{projectName}}.Persistence/Seeders/` if the new entity needs initial data.

### 6. Apply Migration
```bash
# Via Migrator console app (preferred)
cd src/backend/{{projectName}}.Migrator
dotnet run

# Or via EF CLI
dotnet ef database update --context {Context} --project {{projectName}}.Persistence --startup-project {{projectName}}.Api
```
