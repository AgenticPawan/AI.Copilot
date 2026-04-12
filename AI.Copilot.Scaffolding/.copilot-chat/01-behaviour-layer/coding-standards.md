# Coding Standards: Copilot Code Quality Guide

## 📝 Backend (.NET 10) Standards

### Naming Conventions

```csharp
// Classes/Interfaces
public class UserRepository { }           // PascalCase
public interface IUserRepository { }      // I prefix for interfaces
public record CreateUserCommand { }       // Command/Query records

// Methods
public async Task<User> FindByIdAsync(Guid id) { }    // PascalCase, Async suffix for async methods
private void ValidateEmail(string email) { }

// Variables & Fields
private string _firstName;                // Underscore prefix for private fields
private readonly IRepository _repo;       // Readonly for injected dependencies
var currentUser = await GetUserAsync();   // camelCase for local variables

// Constants & Enums
private const string ConnectionString = "...";     // UPPER_SNAKE_CASE for constants
public enum UserStatus { Active, Inactive }        // PascalCase for enum values
```

### Class Organization

```csharp
public class UserRepository : IUserRepository
{
    // 1. Fields (readonly first)
    private readonly HostDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    // 2. Constructor
    public UserRepository(HostDbContext dbContext, ILogger<UserRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // 3. Public methods (query first, then mutation)
    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct) { }
    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct) { }
    public async Task AddAsync(User user, CancellationToken ct) { }
    public async Task UpdateAsync(User user, CancellationToken ct) { }

    // 4. Private methods
    private void ValidateUser(User user) { }
}
```

### Async/Await Rules

```csharp
// ✅ Good: Use Async suffix, use ConfigureAwait(false) in libraries
public async Task<User> FindByIdAsync(Guid id, CancellationToken ct)
{
    return await _dbContext.Users
        .FirstOrDefaultAsync(u => u.Id == id, ct)
        .ConfigureAwait(false);
}

// ✅ Good: Don't wait unnecessarily
public async Task ProcessUserAsync(User user)
{
    await ValidateAsync(user);        // await needed
    var result = ProcessSync(user);   // no await for sync method
}

// ❌ Bad: Fire-and-forget without proper handling
_ = ProcessAsync(user);  // Dangerous, can swallow exceptions

// ✅ Better: Handle exceptions if fire-and-forget
#pragma warning disable CS4014
ProcessAsync(user).ContinueWith(t => {
    if (t.IsFaulted) _logger.LogError(t.Exception, "Process failed");
});
#pragma warning restore CS4014
```

### Error Handling

```csharp
// ✅ Good: Result<T> pattern
public async Task<Result<UserDto>> CreateUserAsync(CreateUserCommand request)
{
    if (string.IsNullOrEmpty(request.Email))
        return Result.Failure<UserDto>("Email is required.");

    var existing = await _userRepo.FindByEmailAsync(request.Email);
    if (existing != null)
        return Result.Failure<UserDto>("Email already registered.");

    var user = User.Create(request.FirstName, request.LastName, request.Email, hash);
    await _userRepo.AddAsync(user);

    return Result.Success(UserDto.FromEntity(user));
}

// ✅ Good: Log exceptions with context
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Failed to save user {UserId}", userId);
    throw;
}

// ❌ Bad: Swallowing exceptions
try { /* code */ } catch { }

// ❌ Bad: Generic exceptions
throw new Exception("Something went wrong");

// ✅ Good: Specific exceptions with messages
throw new InvalidOperationException("User email cannot be changed when verified");
```

### LINQ Style

```csharp
// ✅ Good: Method chain style
var activeUsers = await _dbContext.Users
    .Where(u => u.IsActive && !u.IsDeleted)
    .OrderBy(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .AsNoTracking()
    .ToListAsync();

// ✅ Good: Query syntax for complex queries
var report = (from user in _dbContext.Users
              join role in _dbContext.Roles on user.RoleId equals role.Id
              where user.IsActive
              select new { user.FirstName, role.Name })
    .AsNoTracking()
    .ToList();

// ❌ Bad: Multiple queries (N+1 problem)
var users = _dbContext.Users.ToList();
foreach (var user in users)
{
    user.Role = _dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId);  // N+1!
}

// ✅ Good: Include related data
var usersWithRoles = await _dbContext.Users
    .Include(u => u.Role)
    .ToListAsync();
```

### Entity Configuration

```csharp
// ✅ Good: Fluent API in IEntityTypeConfiguration
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.EmailAddress)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.EmailAddress)
            .IsUnique();

        // Soft delete filter
        builder.HasQueryFilter(u => !u.IsDeleted);

        // Shadow properties
        builder.Property<DateTime?>("LastModifiedAt")
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
```

### Null Handling

```csharp
// ✅ Good: Nullable reference types enabled (#nullable enable)
public class User
{
    public string FirstName { get; set; } = null!;   // Never null
    public string? MiddleName { get; set; }          // Can be null
    public string? GetFullName() => string.IsNullOrEmpty(MiddleName)
        ? $"{FirstName}"
        : $"{FirstName} {MiddleName}";
}

// ✅ Good: Null coalescing, null-conditional
var email = user?.EmailAddress ?? "no-email@example.com";
var role = user?.Role?.Name ?? "No Role";

// ❌ Bad: Not checking null
var email = user.EmailAddress;  // Potential NullReferenceException
```

### Dependency Injection

```csharp
// ✅ Good: Register in Program.cs
builder.Services
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IUserService, UserService>()
    .AddSingleton<IEncryptionService, EncryptionService>()
    .AddTransient<IEmailService, EmailService>();

// ✅ Good: Constructor injection
public class UserService
{
    private readonly IUserRepository _userRepo;
    private readonly IEncryptionService _encryption;

    public UserService(IUserRepository userRepo, IEncryptionService encryption)
    {
        _userRepo = userRepo;
        _encryption = encryption;
    }
}

// ❌ Bad: Service locator pattern
var service = ServiceLocator.GetService<IUserService>();
```

---

## 🎨 Frontend (Angular 20) Standards

### TypeScript Naming

```typescript
// Classes
export class UserService { }
export class UserDto { }

// Interfaces
export interface IUser { }

// Functions and variables
export function getUserName(): string { }
export const users: UserDto[] = [];
let currentUser: UserDto | null = null;

// Constants
export const DEFAULT_PAGE_SIZE = 10;
export const USER_ROLES = { ADMIN: 'admin', USER: 'user' } as const;

// Private/protected
private _cache: Map<string, User> = new Map();
protected readonly destroy$ = new Subject<void>();
```

### Component Structure

```typescript
@Component({
    selector: 'app-user-list',
    standalone: true,
    imports: [CommonModule, FormsModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `...`,
    styles: [`...`]
})
export class UserListComponent implements OnInit, OnDestroy {
    // 1. Input/Output
    @Input() title: string = 'Users';
    @Output() userSelected = new EventEmitter<UserDto>();

    // 2. ViewChild/ViewChildren
    @ViewChild('userForm') userForm!: NgForm;

    // 3. Injected services
    private readonly userService = inject(UserService);
    private readonly route = inject(ActivatedRoute);
    private readonly cdr = inject(ChangeDetectorRef);

    // 4. Signals
    users = signal<UserDto[]>([]);
    isLoading = signal(false);
    selectedUser = signal<UserDto | null>(null);

    // 5. Observables
    private readonly destroy$ = new Subject<void>();
    users$ = this.userService.getAll();

    // 6. Lifecycle hooks
    ngOnInit() {
        this.loadUsers();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // 7. Public methods
    onUserSelect(user: UserDto) {
        this.selectedUser.set(user);
        this.userSelected.emit(user);
    }

    // 8. Private methods
    private loadUsers() {
        this.isLoading.set(true);
        this.userService.getAll()
            .pipe(
                finalize(() => this.isLoading.set(false)),
                takeUntil(this.destroy$)
            )
            .subscribe(users => this.users.set(users));
    }
}
```

### Service Pattern

```typescript
@Injectable({ providedIn: 'root' })
export class UserService {
    private readonly api = inject(ApiService);
    private readonly http = inject(HttpClient);
    private readonly cache = new Map<string, UserDto>();

    getAll(): Observable<UserDto[]> {
        return this.http.get<UserDto[]>('/api/users');
    }

    getById(id: string): Observable<UserDto> {
        if (this.cache.has(id)) {
            return of(this.cache.get(id)!);
        }

        return this.http.get<UserDto>(`/api/users/${id}`).pipe(
            tap(user => this.cache.set(id, user))
        );
    }

    create(user: Omit<UserDto, 'id'>): Observable<UserDto> {
        return this.http.post<UserDto>('/api/users', user);
    }

    update(id: string, user: Partial<UserDto>): Observable<UserDto> {
        return this.http.put<UserDto>(`/api/users/${id}`, user);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`/api/users/${id}`);
    }
}
```

### Reactive Forms Best Practices

```typescript
export class UserFormComponent {
    private readonly fb = inject(FormBuilder);
    private readonly userService = inject(UserService);

    form = this.fb.group({
        firstName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        role: ['', Validators.required]
    });

    onSubmit() {
        if (this.form.invalid) return;

        this.userService.create(this.form.value)
            .subscribe(
                user => console.log('Created:', user),
                error => console.error('Error:', error)
            );
    }

    get firstName() {
        return this.form.get('firstName');
    }
}
```

### Unsubscribe Pattern

```typescript
export class MyComponent implements OnDestroy {
    private readonly destroy$ = new Subject<void>();
    private subscription?: Subscription;

    // ✅ Good: takeUntil pattern
    ngOnInit() {
        this.userService.users$
            .pipe(takeUntil(this.destroy$))
            .subscribe(users => console.log(users));
    }

    // ✅ Good: Manual unsubscribe
    ngOnInit() {
        this.subscription = this.userService.users$
            .subscribe(users => console.log(users));
    }

    ngOnDestroy() {
        this.subscription?.unsubscribe();  // Manual
        this.destroy$.next();              // Emit complete for takeUntil
        this.destroy$.complete();
    }

    // ❌ Bad: Memory leak
    ngOnInit() {
        this.userService.users$.subscribe(users => console.log(users));
        // No unsubscribe!
    }
}
```

### Import Aliases

```typescript
// ✅ Good: Use path aliases
import { UserService } from '@libs/shared';
import { AuthGuard } from '@libs/auth';
import { PermissionDirective } from '@libs/permissions';
import { ThemeService } from '@libs/theme';

// ❌ Bad: Relative imports
import { UserService } from '../../../libs/shared/src/lib/user.service';
```

### Template Best Practices

```html
<!-- ✅ Good: OnPush with signals -->
@if (isLoading()) {
    <p>Loading...</p>
}

<!-- ✅ Good: Template reference variables -->
@for (user of users(); track user.id) {
    <app-user-card
        [user]="user"
        (userSelected)="onUserSelect($event)"

 />
}

<!-- ✅ Good: Event binding with $event -->
<button (click)="delete(user.id)">Delete</button>

<!-- ❌ Bad: Unnecessary ngIf with async pipe -->
<div *ngIf="(users$ | async) as users">
    <!-- Better: use signal or property binding -->
</div>

<!-- ❌ Bad: Complex logic in template -->
<div *ngIf="user.role === 'admin' && user.isActive && !user.isDeleted">
    <!-- Move to component property -->
</div>
```

---

## 🧪 Testing Standards

### Backend Unit Test Structure

```csharp
public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _mockUserRepo;
    private readonly IPasswordHasher _mockHasher;
    private readonly IValidator<CreateUserCommand> _validator;

    public CreateUserCommandHandlerTests()
    {
        _mockUserRepo = new Mock<IUserRepository>().Object;
        _mockHasher = new Mock<IPasswordHasher>().Object;
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(_mockUserRepo, _mockHasher, _validator);
        var request = new CreateUserCommand("John", "Doe", "john@example.com", "ValidPass123");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsFail()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { });  // Simulate existing user

        var handler = new CreateUserCommandHandler(mockRepo.Object, _mockHasher, _validator);
        var request = new CreateUserCommand("John", "Doe", "existing@example.com", "Pass123");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already registered", result.Error);
    }
}
```

### Frontend Test Structure

```typescript
describe('UserListComponent', () => {
    let component: UserListComponent;
    let fixture: ComponentFixture<UserListComponent>;
    let userService: jasmine.SpyObj<UserService>;

    beforeEach(async () => {
        const spy = jasmine.createSpyObj('UserService', ['getAll']);

        await TestBed.configureTestingModule({
            imports: [UserListComponent],
            providers: [{ provide: UserService, useValue: spy }]
        }).compileComponents();

        userService = TestBed.inject(UserService) as jasmine.SpyObj<UserService>;
        fixture = TestBed.createComponent(UserListComponent);
        component = fixture.componentInstance;
    });

    it('should load users on init', fakeAsync(() => {
        // Arrange
        const mockUsers = [{ id: '1', firstName: 'John' }];
        userService.getAll.and.returnValue(of(mockUsers));

        // Act
        fixture.detectChanges();
        tick();

        // Assert
        expect(userService.getAll).toHaveBeenCalled();
        expect(component.users()).toEqual(mockUsers);
    }));

    it('should emit userSelected on selection', () => {
        // Arrange
        spyOn(component.userSelected, 'emit');
        const user = { id: '1', firstName: 'John' };

        // Act
        component.onUserSelect(user);

        // Assert
        expect(component.userSelected.emit).toHaveBeenCalledWith(user);
    });
});
```

---

## 📋 Code Review Checklist

### Backend
- [ ] Entity follows factory pattern with private constructor
- [ ] Command/Query has validator with meaningful messages
- [ ] Handler properly injects dependencies and logs errors
- [ ] Repository method has clear intent and is tested
- [ ] API endpoint has proper authorization attributes
- [ ] Uses Result<T> for error handling
- [ ] No null reference exceptions possible
- [ ] Audit fields (CreatedBy, ModifiedBy) are set correctly
- [ ] Multi-tenant queries have proper TenantId filtering
- [ ] Async/await properly used with CancellationToken

### Frontend
- [ ] Component uses OnPush change detection
- [ ] Services are injectable with providedIn: 'root'
- [ ] Unsubscribe logic in ngOnDestroy
- [ ] Proper null/undefined checking
- [ ] Template does not contain complex logic
- [ ] Path aliases used for imports
- [ ] No hardcoded API endpoints
- [ ] Permissions checked before showing admin features
- [ ] Error handling with user-friendly messages
- [ ] Unit tests cover happy path and error cases

---

**Last Updated**: 2026-04-01 | For Copilot v1.0
