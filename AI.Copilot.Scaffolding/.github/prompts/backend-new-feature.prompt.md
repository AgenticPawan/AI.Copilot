---
mode: 'agent'
description: "Add a new backend feature end-to-end. Use when: creating a new entity with full CRUD (Entity → DTO → Command/Query → Handler → Validator → DbContext → Controller → Permissions → Migration). Generates production-ready code following Copilot Clean Architecture patterns."
---

# Backend Feature: Add New Entity & CRUD

You are building a new backend feature for the Copilot multi-tenant SaaS platform.

## Input Required
Provide the following:
- **Entity Name**: (e.g., `Invoice`)
- **Properties**: (e.g., `string Title, decimal Amount, DateTime DueDate, bool IsPaid`)
- **Scope**: `host-only` | `tenant-only` | `shared`
- **Permissions Group**: (e.g., `Invoices` with Create, Edit, Delete, View)

## Step-by-Step Execution

### Step 1: Domain Entity
Create `src/backend/Copilot.Domain/Entities/{Entity}.cs`:
- Inherit from `BaseEntity`
- Private parameterless constructor
- Static `Create()` factory method with required parameters
- Private setters on ALL properties
- Business logic methods for state changes (Update, Activate, etc.)
- Navigation properties as `IReadOnlyCollection<T>` backed by `List<T>` if applicable
- Add `TenantId` (Guid?) property if scope is `tenant-only` or `shared`

### Step 2: DTOs
Create `src/backend/Copilot.Application/DTOs/{Entity}/{Entity}Dtos.cs`:
- `{Entity}Dto` - read model for API responses (include Id, CreatedAt, all display properties)
- `Create{Entity}Request` - write model for create (only user-provided fields, no Id)
- `Update{Entity}Request` - write model for update (mutable fields + Id setter)

### Step 3: Commands & Queries
Create in `src/backend/Copilot.Application/Features/{Entity}/`:

**Commands/{Entity}Commands.cs** - record definitions:
```
Create{Entity}Command(...) : IRequest<Result<{Entity}Dto>>
Update{Entity}Command(...) : IRequest<Result<{Entity}Dto>>
Delete{Entity}Command(Guid Id) : IRequest<Result<bool>>
```

**Commands/Create{Entity}CommandHandler.cs**:
- Inject appropriate DbContext (IHostDbContext or resolve via IUserAccountDbContextResolver)
- Check for duplicates, create via factory, persist, audit log, return Result.Success

**Commands/{Entity}CrudCommandHandlers.cs** (Update + Delete in one file):
- Update: find entity, call domain methods, save, audit log
- Delete: soft-delete (entity.IsDeleted = true or remove method), save, audit log

**Queries/{Entity}Queries.cs** + **Queries/{Entity}QueryHandlers.cs**:
- `Get{Entity}ByIdQuery(Guid Id)` → single entity
- `Get{Entities}PagedQuery(PagedQuery Paging)` → paged list using `.ToPagedResultAsync()`

### Step 4: Validators
Create `src/backend/Copilot.Application/Features/{Entity}/Validators/{Entity}Validators.cs`:
- `Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>`
- `Update{Entity}CommandValidator : AbstractValidator<Update{Entity}Command>`
- Rules: NotEmpty, MaximumLength, regex patterns where applicable

### Step 5: DbContext Interface Methods
Add to the appropriate interface in `src/backend/Copilot.Application/Interfaces/`:
- For host entities → `IHostDbContext.cs`
- For tenant entities → `ITenantDbContext.cs` (or create if needed)
- Methods: `Find{Entity}ByIdAsync`, `Add{Entity}Async`, `{Entities}Queryable` (IQueryable)

### Step 6: DbContext Implementation
Add implementations to the appropriate DbContext:
- Add `DbSet<{Entity}> {Entities}` property
- Implement interface methods (FindById uses FirstOrDefaultAsync with !IsDeleted filter)
- Add `IQueryable<{Entity}> {Entities}Queryable => {Entities}.Where(x => !x.IsDeleted)`

### Step 7: EF Configuration
Add to `src/backend/Copilot.Persistence/Configurations/EntityConfigurations.cs`:
- `{Entity}Configuration : IEntityTypeConfiguration<{Entity}>`
- Table name, key, property constraints, indexes, `HasQueryFilter(e => !e.IsDeleted)`
- Call `AuditFieldConfigurations.ConfigureAuditFields<{Entity}>(builder)`

### Step 8: API Controller
Create `src/backend/Copilot.Api/Controllers/{Area}/{Entities}Controller.cs`:
- `[ApiController]`, `[Route("api/[controller]")]`
- Inject: IMediator, ICrudNotificationService, ICurrentUserService, ITransactionStreamService, ICacheService, IOutputCacheStore, ILogger
- CRUD endpoints with `[Authorize(Policy = "Permission:{Entity}.{Action}")]`
- OutputCache on GET endpoints
- Cache eviction + CRUD notifications on mutations
- Progress tracking via ITransactionStreamService on create/update

### Step 9: Permission Registration
- Add new enum values to `PermissionGroup` if creating a new group
- Add permission seed data in `PermissionSeeder.cs`
- Register authorization policies in `Program.cs`

### Step 10: Migration
Remind the user to:
```bash
cd src/backend
dotnet ef migrations add Add{Entity} --project Copilot.Persistence --startup-project Copilot.Api --context {Host|Tenant}DbContext --output-dir Migrations/{Host|Tenant}
```

## Output Checklist
After generating, verify:
- [ ] Entity has private constructor + static Create() factory
- [ ] All properties have private setters
- [ ] DTOs don't expose sensitive data (connection strings, hashes)
- [ ] Validator exists for every command
- [ ] Controller actions have Authorize policies
- [ ] CancellationToken passed through all async methods
- [ ] Audit logging on state changes
- [ ] Cache eviction after mutations
- [ ] TenantId considered for multi-tenant scope
