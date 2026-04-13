# Architecture Guide: {{projectName}} Design Patterns

## 🏗️ Clean Architecture Layers

### Layer 1: Domain (Core Business Logic)
**Location**: `src/backend/{{projectName}}.Domain/`

**Responsibilities**:
- Define entities with business rules
- Define enums and value objects
- Define interfaces (abstractions needed by business)
- NO dependencies on external frameworks

**Templates**:

**Entity with Factory Pattern**:
```csharp
using {{projectName}}.Domain.Common;

namespace {{projectName}}.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string EmailAddress { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsEmailConfirmed { get; private set; }

    private User() { }  // Private constructor for EF

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            EmailAddress = email,
            PasswordHash = passwordHash,
            IsEmailConfirmed = false
        };
    }

    public void ConfirmEmail() => IsEmailConfirmed = true;

    public void ChangePassword(string newHash) => PasswordHash = newHash;
}
```

**BaseEntity (inherited by all entities)**:
- `Id: Guid` - Primary key
- `CreatedAt: DateTime` - Auto-set by DbContext
- `LastModifiedAt: DateTime?` - Auto-set by DbContext
- `CreatedByUserId: Guid?` - Current user ID
- `LastModifiedByUserId: Guid?` - Current user ID
- `IsDeleted: bool` - Soft delete flag
- `DeletedAt: DateTime?` - When deleted

---

### Layer 2: Application (Use Cases / Business Operations)
**Location**: `src/backend/{{projectName}}.Application/`

**Responsibilities**:
- Orchestrate business logic (Commands/Queries)
- Validate input (FluentValidation)
- Coordinate with external services
- Transform domain entities to DTOs

**Structure**:
```
Features/
  ├── Account/
  │   ├── Commands/
  │   │   ├── AccountCommands.cs          (Command definitions)
  │   │   ├── RegisterCommandHandler.cs
  │   │   └── ChangePasswordCommandHandler.cs
  │   ├── Queries/
  │   │   ├── AccountQueries.cs          (Query definitions)
  │   │   └── GetUserByIdQueryHandler.cs
  │   └── Validators/
  │       └── RegisterCommandValidator.cs
  ├── DTOs/
  │   └── UserDto.cs
  └── Interfaces/
      └── IPasswordHasher.cs
```

**Command Pattern** (CQRS Write):
```csharp
public record RegisterCommand(string FirstName, string LastName, string Email, string Password)
    : IRequest<Result<UserDto>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<RegisterCommand> _validator;

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<UserDto>(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

        var existingUser = await _userRepo.FindByEmailAsync(request.Email, ct);
        if (existingUser != null)
            return Result.Failure<UserDto>("Email already registered.");

        var passwordHash = _hasher.Hash(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, passwordHash);

        await _userRepo.AddAsync(user, ct);
        return Result.Success(UserDto.FromEntity(user));
    }
}
```

**Query Pattern** (CQRS Read):
```csharp
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepo;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _userRepo.FindByIdAsync(request.UserId, ct);
        if (user == null)
            return Result.Failure<UserDto>("User not found.");

        return Result.Success(UserDto.FromEntity(user));
    }
}
```

**DTO Pattern** (For API responses):
```csharp
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public bool IsEmailConfirmed { get; set; }

    public static UserDto FromEntity(User user) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailAddress = user.EmailAddress,
            IsEmailConfirmed = user.IsEmailConfirmed
        };
}
```

---

### Layer 3: Infrastructure (External Services)
**Location**: `src/backend/{{projectName}}.Infrastructure/`

**Responsibilities**:
- JWT token generation/validation
- Password hashing (BCrypt)
- AES encryption for sensitive data
- Tenant resolution (DNS-based)
- Security & permission services

**Key Services**:

**JWT Service** (Authentication):
```csharp
public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string[]? roles = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
```

**Password Hasher** (Encryption):
```csharp
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

**Encryption Service** (AES):
```csharp
public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
```

**Tenant Resolution**:
```csharp
public interface ITenantResolver
{
    Task<Tenant?> ResolveAsync(string host);  // Extract subdomain, lookup tenant
}
```

**Permission Service**:
```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission, Guid? tenantId = null);
    Task<string[]> GetUserPermissionsAsync(Guid userId, Guid? tenantId = null);
}
```

---

### Layer 4: Persistence (Data Access)
**Location**: `src/backend/{{projectName}}.Persistence/`

**Responsibilities**:
- EF Core DbContext configuration
- Database migrations
- Entity configurations
- Repository implementations

**DbContext Separation**:

**HostDbContext** (Shared host database):
```csharp
public class HostDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Exclude soft-deleted entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.Name)
                .HasQueryFilter($"{nameof(BaseEntity.IsDeleted)} == false");
        }

        // Entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HostDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        HandleAuditFields();
        return await base.SaveChangesAsync(ct);
    }

    private void HandleAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var userId = _currentUser?.UserId;  // Injected ICurrentUserService

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = DateTime.UtcNow;
                entry.Entity.LastModifiedByUserId = userId;
            }
        }
    }
}
```

**TenantDbContext** (Per-tenant database):
```csharp
public class TenantDbContext : DbContext
{
    private readonly Guid _tenantId;

    public DbSet<Document> Documents { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }

    // Same query filters, audit handling as above
}
```

**Repository Pattern**:
```csharp
public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<IEnumerable<User>> FindAllAsync(CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(User user, CancellationToken ct);
}

public class UserRepository : IUserRepository
{
    private readonly HostDbContext _dbContext;

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
        await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
}
```

---

### Layer 5: API (Controllers / Endpoints)
**Location**: `src/backend/{{projectName}}.Api/`

**Responsibilities**:
- HTTP endpoints (REST)
- Request/Response handling
- Authorization attributes
- Error responses

**Controller Pattern**:
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Require JWT
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    [AllowAnonymous]  // Registration is public
    [ProduceResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProduceResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> Register(RegisterCommand request)
    {
        var result = await _mediator.Send(request);

        if (!result.IsSuccess)
            return BadRequest(new ApiError { Message = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpGet("{id}")]
    [Permission("Users.View")]  // Custom authorization
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));

        if (!result.IsSuccess)
            return NotFound(new ApiError { Message = result.Error });

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Permission("Users.Manage")]
    public async Task<IActionResult> Update(Guid id, UpdateUserCommand request)
    {
        request.UserId = id;
        var result = await _mediator.Send(request);

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    [Permission("Users.Manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteUserCommand(id));

        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
```

**Response Patterns**:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}

public class ApiError
{
    public string Message { get; set; } = null!;
    public Dictionary<string, string[]>? Errors { get; set; }
}
```

---

### Middleware (Cross-Cutting Concerns)
**Location**: `src/backend/{{projectName}}.Api/Middleware/`

**Exception Handling Middleware**:
- Catches all exceptions
- Logs with correlation ID
- Returns ApiError response

**Tenant Resolution Middleware**:
- Extracts subdomain from request
- Resolves tenant
- Injects tenant context into request

**JWT Validation Middleware**:
- Extracts bearer token
- Validates and sets claims principal

---

## 🔄 Multi-Tenancy Flow

```
Browser Request (acme.{{projectName}}.com)
         ↓
TenantResolutionMiddleware
         ↓↓
ExtractSubdomain("acme") → ITenantResolver.ResolveAsync("acme")
         ↓ (Look up in HostDbContext.Tenants)
Tenant { Id: Guid, TenancyName: "acme", ConnectionString: "encrypted" }
         ↓↓↓
SetContextTenantId(tenantId) → HttpContext.Items["TenantId"]
         ↓
ITenantDbContextFactory.CreateAsync(tenantId)
         ↓↓↓↓
IEncryptionService.Decrypt(connectionString)
         ↓
TenantDbContext(decrypted connection string)
         ↓
Use TenantDbContext in repository/queries
         ↓
Request processed with tenant-isolated data
```

**Key Classes**:
- `TenantResolutionMiddleware` - Extracts subdomain, resolves tenant
- `ITenantDbContextFactory` - Creates DbContext for specific tenant
- `IEncryptionService` - Decrypts connection strings
- `IUserAccountDbContextResolver` - Returns Host or Tenant DbContext based on context

---

## 🎨 Frontend Architecture (Angular 20)

### Project Structure
```
projects/
├── portal/
│   ├── src/
│   │   ├── app/
│   │   │   ├── app.component.ts       (Root component)
│   │   │   ├── pages/
│   │   │   │   ├── admin/             (Host portal pages)
│   │   │   │   │   ├── dashboard/
│   │   │   │   │   ├── users/
│   │   │   │   │   └── tenants/
│   │   │   │   └── tenant/            (Tenant portal pages)
│   │   │   │       ├── dashboard/
│   │   │   │       └── documents/
│   │   │   ├── core/                  (Guards, interceptors)
│   │   │   └── shared/                (Reusable components)
│   │   └── index.html
│   └── angular.json
└── libs/
    ├── auth/                          (Authentication)
    ├── permissions/                   (Authorization)
    ├── theme/                         (Theming)
    ├── ui/                            (Layout components)
    ├── shared/                        (Services, models)
    └── forms-dynamic/                 (Dynamic forms)
```

### Standalone Component Pattern (Angular 20)
```typescript
import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

@Component({
    selector: 'app-users-list',
    standalone: true,  // No NgModule needed
    imports: [CommonModule, HttpClientModule],
    changeDetection: ChangeDetectionStrategy.OnPush,  // Zoneless performance
    template: `
        <div>
            <h1>Users</h1>
            @for (user of users(); track user.id) {
                <app-user-card [user]="user" />
            }
        </div>
    `,
    styles: [`
        :host { display: block; }
    `]
})
export class UsersListComponent {
    private readonly userService = inject(UserService);
    users = signal<UserDto[]>([]);

    constructor() {
        this.loadUsers();
    }

    private loadUsers() {
        this.userService.getAll().subscribe(
            users => this.users.set(users)
        );
    }
}
```

### Service Pattern
```typescript
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@libs/shared';

@Injectable({ providedIn: 'root' })
export class UserService {
    private readonly api = inject(ApiService);

    getAll(): Observable<UserDto[]> {
        return this.api.get<UserDto[]>('/api/users');
    }

    getById(id: string): Observable<UserDto> {
        return this.api.get<UserDto>(`/api/users/${id}`);
    }

    create(user: CreateUserRequest): Observable<UserDto> {
        return this.api.post<UserDto>('/api/users', user);
    }

    update(id: string, user: UpdateUserRequest): Observable<UserDto> {
        return this.api.put<UserDto>(`/api/users/${id}`, user);
    }

    delete(id: string): Observable<void> {
        return this.api.delete<void>(`/api/users/${id}`);
    }
}
```

### Route & Guard Pattern
```typescript
const routes: Routes = [
    {
        path: 'admin',
        canActivate: [AdminGuard],
        canActivateChild: [AuthGuard],
        children: [
            { path: 'users', component: UsersListComponent },
            { path: 'tenants', component: TenantsListComponent },
        ]
    },
    {
        path: 'tenant',
        canActivate: [TenantGuard],
        children: [
            { path: 'documents', component: DocumentsListComponent },
        ]
    }
];
```

---

## 🔐 Security Architecture

### Authentication Flow
1. User login → API /auth/login
2. Backend validates credentials, generates JWT + Refresh token
3. JWT stored in localStorage (HttpOnly would be better in production)
4. JWT interceptor adds Authorization header to all requests
5. Expired JWT → Use refresh token to get new JWT

### Authorization Flow
1. Endpoint has `[Permission("Users.Manage")]` attribute
2. Token contains user claims
3. PermissionRequirement handler checks claim against permission
4. If authorized, endpoint executes; else 403 Forbidden

### Data Encryption
1. Tenant connection strings encrypted with AES
2. Sensitive settings encrypted before storage
3. Decrypted on demand with IEncryptionService

---

## 📚 Data Flow Diagram

```
User Input → Angular Component
    ↓
API Service (with Auth interceptor)
    ↓
API Endpoint (Controller action)
    ↓
MediatR CQRS Handler (authorize, validate)
    ↓
Repository Layer (multi-tenant aware)
    ↓
DbContext (Host or Tenant)
    ↓
SQL Server
```

---

## 🚀 Feature Development Checklist

- [ ] **Domain** - Create entity with factory pattern
- [ ] **Application** - Create Command/Query with handler and validator
- [ ] **Infrastructure** - Add any external service integration
- [ ] **Persistence** - Create repository method and EF configuration
- [ ] **Api** - Create controller endpoint with authorization
- [ ] **Frontend** - Create service, component, route, guard
- [ ] **Tests** - Unit tests for handler, repository, component
- [ ] **Permissions** - Check if new permission needed
- [ ] **Multi-tenancy** - Verify host/tenant data isolation
- [ ] **Documentation** - Update README/wiki if needed

---

**Last Updated**: 2026-04-01 | For {{projectName}} v1.0
