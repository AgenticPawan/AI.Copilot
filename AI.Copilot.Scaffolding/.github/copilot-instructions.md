# VirtualStudio - GitHub Copilot Instructions

## Project Identity

Multi-tenant SaaS platform called **VirtualStudio**. DNS-based tenant isolation with database-per-tenant strategy.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, C# 13, Clean Architecture, CQRS (MediatR 12), EF Core 10 (SQL Server), FluentValidation |
| Frontend | Angular 20, Standalone components, Signals, OnPush change detection, SCSS |
| Auth | JWT + Refresh tokens (BCrypt 12 work factor), Permission-based authorization (28 policies) |
| Real-time | SignalR (3 hubs: host-notifications, tenant-notifications, transaction-stream) |
| Testing | xUnit + Moq + FluentAssertions (backend), Playwright (E2E) |
| DevOps | Docker, Kubernetes, GitHub Actions |

## Solution Structure

```
src/backend/
  VirtualStudio.Domain/           -- Entities, Enums, Common (BaseEntity, IAuditableEntity, ISoftDelete)
  VirtualStudio.Application/      -- Features/{Area}/Commands|Queries|Validators, DTOs, Interfaces, Common (Result<T>)
  VirtualStudio.Infrastructure/   -- Security (JWT, AES, BCrypt), Tenancy, Services, EmailTemplates
  VirtualStudio.Persistence/      -- Contexts (HostDbContext, TenantDbContext), Configurations, Seeders, Migrations
  VirtualStudio.Api/              -- Controllers/{Auth|Admin|Shared|TenantModules}, Middleware, Hubs, Authorization
  VirtualStudio.Migrator/         -- Console app for migrations & seeding
  VirtualStudio.Tests/            -- xUnit tests (Domain/, Application/)

src/frontend/
  projects/portal/                -- Single SPA (host/tenant detection via subdomain)
  projects/libs/auth/             -- Guards, interceptors, token management
  projects/libs/permissions/      -- Permission directives and guards
  projects/libs/theme/            -- Theme service and CSS variables
  projects/libs/ui/               -- Toast, Confirm dialog, shared UI
  projects/libs/shared/           -- ApiService, DTOs, utilities
  projects/libs/forms-dynamic/    -- Dynamic form generation
```

## Backend Coding Patterns (MUST FOLLOW)

### Entity Pattern
```csharp
public class {Entity} : BaseEntity
{
    public string Name { get; private set; } = null!;
    // All properties have PRIVATE setters

    private {Entity}() { }  // Private constructor for EF Core

    public static {Entity} Create(/* required params */)
    {
        return new {Entity} { /* set properties */ };
    }

    // Business logic via named methods
    public void Activate() => IsActive = true;
    public void UpdateName(string name) => Name = name;
}
```

### Command/Query Pattern (CQRS + MediatR)
```csharp
// Command definition - always a record implementing IRequest<Result<T>>
public record Create{Entity}Command(string Name, ...) : IRequest<Result<{Entity}Dto>>;

// Handler - constructor injection, try/catch, Result pattern
public class Create{Entity}CommandHandler : IRequestHandler<Create{Entity}Command, Result<{Entity}Dto>>
{
    // Inject: IHostDbContext or IUserAccountDbContextResolver, ILogger<T>, other services
    public async Task<Result<{Entity}Dto>> Handle(Create{Entity}Command request, CancellationToken ct)
    {
        try {
            // 1. Validate business rules (duplicate check etc.)
            // 2. Create entity via static factory: Entity.Create(...)
            // 3. Persist via DbContext: await _db.Add{Entity}Async(entity, ct);
            // 4. Save: await _db.SaveChangesAsync(ct);
            // 5. Audit log (if applicable)
            // 6. Return Result<T>.Success(dto)
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error creating {Entity}");
            throw;
        }
    }
}
```

### Validator Pattern
```csharp
public class Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>
{
    public CreateEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
```

### Controller Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
public class {Entities}Controller : ControllerBase
{
    // Inject: IMediator, ICrudNotificationService, ICurrentUserService,
    //         ITransactionStreamService, ICacheService, IOutputCacheStore, ILogger<T>

    [Authorize(Policy = "Permission:{Entity}.View")]
    [HttpGet]
    [OutputCache(PolicyName = "TenantAware")]
    public async Task<IActionResult> GetAll([FromQuery] PagedQuery query)
    {
        var result = await _mediator.Send(new Get{Entities}PagedQuery(query));
        if (!result.IsSuccess) return BadRequest(new { result.Error });
        return Ok(result.Data);
    }

    [Authorize(Policy = "Permission:{Entity}.Create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{Entity}Request request)
    {
        var operationId = Guid.NewGuid().ToString("N");
        // 1. Send progress "in-progress" via ITransactionStreamService
        // 2. Dispatch MediatR command
        // 3. On failure: send "failed" progress, return BadRequest
        // 4. On success: send CRUD notification, evict cache, send "completed", return Created
    }
}
```

### DbContext Pattern
Repository methods live directly on DbContext (no separate repository classes):
- `IHostDbContext` - host database operations (Tenants, Features, host Users, Permissions, Roles, global entities)
- `IUserAccountDbContext` - shared auth operations (works for both host and tenant users)
- `ITenantDbContext` - tenant-specific entities (FamilyMembers, Documents, PasswordEntries)
- `IUserAccountDbContextResolver.Resolve()` returns the correct context based on current tenant

### Multi-Tenancy Rules
- Tenant resolved from DNS subdomain via `TenantProvider` (checks X-Forwarded-Host, Host, Origin headers)
- `IsHostContext()` = true when no subdomain detected (admin/host portal)
- Connection strings AES-encrypted in host database, decrypted at runtime
- TenantDbContext applies query filters: `e.TenantId == _tenantId`
- Tenant-specific migrations under `Migrations/Tenant/`, host under `Migrations/Host/`

## Frontend Coding Patterns (MUST FOLLOW)

### Component Pattern
```typescript
@Component({
    selector: 'app-{name}',
    standalone: true,                              // ALWAYS standalone
    imports: [/* only what's needed */],
    changeDetection: ChangeDetectionStrategy.OnPush, // ALWAYS OnPush
    templateUrl: './{name}.component.html',
    styleUrl: './{name}.component.scss',
})
export class {Name}Component implements OnInit {
    private readonly api = inject(ApiService);      // Use inject() function, NOT constructor injection
    private readonly toast = inject(ToastService);

    items = signal<ItemDto[]>([]);                  // Use signals for reactive state
    totalCount = signal(0);
    loading = signal(false);
}
```

### Service Pattern
```typescript
@Injectable({ providedIn: 'root' })              // ALWAYS tree-shakable
export class {Name}Service {
    private readonly http = inject(HttpClient);   // inject() function
}
```

### Routing Pattern
- Lazy-loaded standalone components via `loadComponent`
- Multi-layer guards: `maintenanceGuard`, `authGuard`, `hostGuard`, `tenantGuard`, `permissionGuard('...')`, `featureGuard('...')`
- Host-only routes: `canActivate: [hostGuard]`
- Tenant-only routes: `canActivate: [tenantGuard, featureGuard('Feature.Enabled')]`

### Library Imports
```typescript
import { ApiService, SomeDto } from '@libs/shared';
import { ToastService, ConfirmDialogComponent } from '@libs/ui';
import { HasPermissionDirective } from '@libs/permissions';
import { AuthService } from '@libs/auth';
import { ThemeService } from '@libs/theme';
```

## Testing Patterns

### Unit Test (Backend - xUnit + Moq)
```csharp
public class {Handler}Tests
{
    private readonly Mock<IUserAccountDbContext> _accountDb = new();
    private readonly Mock<IUserAccountDbContextResolver> _dbResolver = new();
    // Mock ALL dependencies
    private readonly {Handler} _handler;

    public {Handler}Tests()
    {
        _dbResolver.Setup(x => x.Resolve()).Returns(_accountDb.Object);
        _handler = new {Handler}(/* all mocks */);
    }

    [Fact]
    public async Task {Method}_With{Scenario}_Returns{Expected}()
    {
        // Arrange - setup mocks
        // Act - call handler
        // Assert - FluentAssertions (result.IsSuccess.Should().BeTrue())
    }
}
```

## Permission System
28 permission-based policies: `Users.{Create|Edit|Delete|View}`, `Roles.{Create|Edit|Delete|View}`, `Tenants.{Create|Manage|View}`, `Settings.{Edit|View}`, `Notifications.{Send|View}`, `Features.Manage`, `AuditLogs.View`, `Localization.Manage`, `Documents.{CRUD}`, `Passwords.{CRUD}`, `FamilyMembers.{CRUD}`.

## Rules for AI
0. **Never keep logic in API controllers** - always use MediatR handlers for business logic
    0.1 **Never call DbContext directly from controllers** - always go through MediatR handlers
    0.2 **Never use services directly in controllers** - inject them into handlers, not controllers
    0.3 **Controllers should only handle HTTP concerns** - model binding, authorization, caching, and calling MediatR
    0.4 **All state-changing operations must send CRUD notifications and evict cache** via injected services in handlers
    0.5 **All operations must use Result<T> pattern** to return success/failure and error messages, never throw for expected business logic failures
    0.6 **All handlers must use CancellationToken** for async operations
    0.7 **All handlers must log exceptions** and return user-friendly error messages via Result<T>
    0.8 **All new handlers must have corresponding FluentValidation validators** for input validation
    0.9 **All new handlers must have unit tests** using xUnit and Moq to cover success and failure scenarios
    0.10 **All new features must be documented in README.md** with screenshots if applicable
    0.11 **All new features must have export/import CSV functionality** for data tables in Angular frontend
    0.12 **All new features must have a refresh button** in the Angular frontend to reload data from the server
    0.13 **All new features in Angular must use FontAwesome icons** for buttons (plus, download, upload, refresh)
1. **Permissions, Settinngs and Features should have a seperate class as a point of entry and centralization**. For example, Permissions.cs should contain all permission strings as constants, Settings.cs should contain all setting keys, Features.cs should contain all feature names. This makes it easier to manage and avoid typos.
    1.1 **Features should supports parent and child relation for better organization**. For example, you can have a parent feature "DocumentManagement" and child features "DocumentManagement.Create", "DocumentManagement.Edit", "DocumentManagement.Delete". This allows for more granular control and better UI organization in the frontend.
    
1. **Never expose domain entities directly** - always use DTOs for API responses
2. **Never use public setters on entities** - use factory methods and behavior methods
3. **Always use Result<T> pattern** in handlers - never throw for business logic failures
4. **Always add FluentValidation validators** for new commands
5. **Always use CancellationToken** in async methods
6. **Always add audit logging** for state-changing operations
7. **Always evict output cache** after mutations in controllers
8. **Always add Authorize policy** on controller actions
9. **Frontend: Always use standalone components** with OnPush change detection
10. **Frontend: Always use inject() function** not constructor injection
11. **Frontend: Always use signals** for component state
12. **Frontend: Import from @libs/* path aliases** not relative paths to libs
13. **Never create separate repository classes** - add methods to DbContext interfaces
14. **Always consider tenant isolation** - use TenantId filters where applicable
15. **Keep controllers thin** - business logic belongs in MediatR handlers
16. **Use consistent naming conventions** - {Entity} for classes, {Entity}Dto for DTOs, {Action}{Entity}Command for commands, etc.
17. **Always handle exceptions in handlers** and log them, but return user-friendly error messages via Result<T>
18. **Always write unit tests for new handlers** using xUnit and Moq
19. **For Angular, always provide export/import CSV functionality** for data tables
20. **For Angular, always include a refresh button** to reload data from the server
21. **For Angular, use FontAwesome icons** for buttons (plus, download, upload, refresh)
22. **For Angular, use Bootstrap classes** for styling buttons and layout
23. **For Angular, use the TranslatePipe** for all user-facing text to support localization
24. **For Angular, use the ToastService** to show success/error messages after operations
25. **For Angular, use the ConfirmDialogComponent** to confirm destructive actions (deletions)
26. **For Angular, use the CsvTransferService** for CSV export/import operations
27. **For Angular, use the ApiService** for all HTTP requests to the backend
28. **For Angular, use the PermissionDirective** to conditionally show/hide UI elements based on permissions
29. **For Angular, use the AuthService** to check authentication state and get current user info
30. **For Angular, use the ThemeService** to apply theming and CSS variables
31. **For Angular, always unsubscribe from observables** using takeUntilDestroyed or async pipe
32. **For Angular, always debounce search inputs** to avoid excessive API calls
33. **For Angular, always show loading indicators** when fetching data from the server
34. **For Angular, always handle API errors gracefully** and show error messages via ToastService
35. **For Angular, always use pagination** for data tables with large datasets
36. **For Angular, always use a consistent folder structure** for components, services, and pages
37. **For Angular, always use SCSS** for component styles and follow BEM naming conventions
38. **Update the README.md** with any new features or changes to existing features or any missing functionality, including screenshots if applicable.