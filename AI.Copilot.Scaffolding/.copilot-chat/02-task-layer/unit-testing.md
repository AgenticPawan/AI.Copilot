# Unit Testing Prompt

**When to use**: Writing tests for command handlers, repository methods, services, and components.

---

## 🎯 Testing Philosophy

**Test Behavior, Not Implementation**:
- Focus on **what** the code should do, not **how**
- Test public API, not private implementation
- Test edge cases and error scenarios
- Avoid testing framework internals

**Test Pyramid**:
```
        /\
       /  \  E2E Tests (Playwright) - 10%
      /    \
     /______\
    /        \
   / Integr. /  Integration Tests - 20%
  /________ \
 /          \
/ Unit Tests \ Unit Tests (xUnit/Jasmine) - 70%
/____________\
```

---

## 🧪 Backend Unit Tests (xUnit + Moq)

### Structure: Arrange-Act-Assert

```csharp
[TestClass]
public class CreateUserCommandHandlerTests
{
    // ✅ Good: Descriptive test name = Behavior_Condition_ExpectedResult
    [Fact]
    public async Task Handle_WithValidEmail_CreatesUserAndReturnsSuccess()
    {
        // Arrange: Setup test fixtures and mocks
        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockValidator = new Mock<IValidator<CreateUserCommand>>();

        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());  // Valid

        mockPasswordHasher
            .Setup(p => p.Hash(It.IsAny<string>()))
            .Returns("hashed-password");

        var handler = new CreateUserCommandHandler(
            mockUserRepo.Object,
            mockPasswordHasher.Object,
            mockValidator.Object);

        var command = new CreateUserCommand("John", "Doe", "john@example.com", "P@ssw0rd!");

        // Act: Execute the behavior
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert: Verify the outcome
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("john@example.com", result.Data.EmailAddress);

        // Verify mock was called correctly
        mockUserRepo.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);

        mockPasswordHasher.Verify(
            p => p.Hash("P@ssw0rd!"),
            Times.Once);
    }

    // ✅ Good: Test error case
    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsFail()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        mockUserRepo
            .Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { });  // Existing user

        var handler = new CreateUserCommandHandler(mockUserRepo.Object, /* ... */);
        var command = new CreateUserCommand("John", "Doe", "existing@example.com", "P@ssw0rd!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already registered", result.Error);
    }

    // ✅ Good: Test validation failure
    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsFail()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<CreateUserCommand>>();
        var validationFailure = new ValidationFailure("Email", "Invalid email format");
        mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var handler = new CreateUserCommandHandler(/* ..., mockValidator.Object */);
        var command = new CreateUserCommand("John", "Doe", "invalid-email", "P@ssw0rd!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    // ✅ Good: Test with null handling
    [Fact]
    public async Task Handle_WithNullRequest_ReturnsFail()
    {
        var handler = new CreateUserCommandHandler(/* ... */);

        // Command validation should catch this, or handler checks
        // (depends on implementation)
    }

    // ✅ Good: Test CancellationToken is respected
    [Fact]
    public async Task Handle_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var handler = new CreateUserCommandHandler(/* ... */);
        var command = new CreateUserCommand("John", "Doe", "john@example.com", "P@ssw0rd!");
        var cts = new CancellationTokenSource();
        cts.Cancel();  // Cancel before execution

        // Act & Assert
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(
            () => handler.Handle(command, cts.Token));
    }
}
```

### Testing Repository Methods

```csharp
public class UserRepositoryTests
{
    private readonly TestDbContext _dbContext;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _dbContext = new TestDbContext();  // In-memory DB for testing
        _repository = new UserRepository(_dbContext);
    }

    [Fact]
    public async Task FindByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("John", "Doe", "john@example.com", "hash");
        user.Id = userId;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("john@example.com", result.EmailAddress);
    }

    [Fact]
    public async Task FindByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.FindByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_ExcludesDeletedEntities()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("John", "Doe", "john@example.com", "hash");
        user.Id = userId;
        user.IsDeleted = true;  // Soft deleted
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.Null(result);  // Soft-deleted entities are filtered
    }

    [Fact]
    public async Task FindByEmailAsync_WithCaseInsensitiveEmail_ReturnsUser()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", "hash");
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.FindByEmailAsync("JOHN@EXAMPLE.COM", CancellationToken.None);

        // Assert
        Assert.NotNull(result);  // Email comparison should be case-insensitive
    }
}
```

### Using InlineData for Parametrized Tests

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public async Task Validate_WithEmptyFirstName_ReturnsFail(string firstName)
{
    var validator = new CreateUserCommandValidator();
    var command = new CreateUserCommand(firstName, "Doe", "john@example.com", "P@ssw0rd!");

    var result = await validator.ValidateAsync(command);

    Assert.False(result.IsValid);
}
```

---

## 🎨 Frontend Unit Tests (Jasmine)

### Test Component Behavior

```typescript
describe('DocumentsComponent', () => {
    let component: DocumentsComponent;
    let fixture: ComponentFixture<DocumentsComponent>;
    let documentService: jasmine.SpyObj<DocumentService>;

    beforeEach(async () => {
        // Create spy object with mock methods
        const spy = jasmine.createSpyObj('DocumentService', [
            'getAll',
            'delete'
        ]);

        await TestBed.configureTestingModule({
            imports: [DocumentsComponent],
            providers: [{ provide: DocumentService, useValue: spy }]
        }).compileComponents();

        documentService = TestBed.inject(DocumentService) as jasmine.SpyObj<DocumentService>;
        fixture = TestBed.createComponent(DocumentsComponent);
        component = fixture.componentInstance;
    });

    // ✅ Good: Test data loading on init
    it('should load documents on init', fakeAsync(() => {
        // Arrange
        const mockDocs: DocumentDto[] = [
            { id: '1', title: 'Doc1', content: 'Cont1', tenantId: '1', createdAt: '' },
            { id: '2', title: 'Doc2', content: 'Cont2', tenantId: '1', createdAt: '' }
        ];
        documentService.getAll.and.returnValue(of(mockDocs));

        // Act
        fixture.detectChanges();  // Trigger ngOnInit
        tick();  // Process async operations

        // Assert
        expect(documentService.getAll).toHaveBeenCalled();
        expect(component.documents()).toEqual(mockDocs);
        expect(component.isLoading()).toBe(false);
    }));

    // ✅ Good: Test error handling
    it('should handle error when loading documents fails', fakeAsync(() => {
        // Arrange
        documentService.getAll.and.returnValue(
            throwError(() => new Error('Network error'))
        );

        spyOn(console, 'error');

        // Act
        fixture.detectChanges();
        tick();

        // Assert
        expect(console.error).toHaveBeenCalledWith(
            'Error loading documents:',
            jasmine.any(Error)
        );
    }));

    // ✅ Good: Test user interaction
    it('should delete document when user confirms', () => {
        // Arrange
        spyOn(window, 'confirm').and.returnValue(true);
        documentService.delete.and.returnValue(of(void 0));
        documentService.getAll.and.returnValue(of([]));

        // Act
        component.onDelete('1');

        // Assert
        expect(window.confirm).toHaveBeenCalledWith('Are you sure?');
        expect(documentService.delete).toHaveBeenCalledWith('1');
    });

    // ✅ Good: Test user interaction cancelled
    it('should not delete when user cancels confirmation', () => {
        // Arrange
        spyOn(window, 'confirm').and.returnValue(false);

        // Act
        component.onDelete('1');

        // Assert
        expect(documentService.delete).not.toHaveBeenCalled();
    });

    // ✅ Good: Test signal updates
    it('should update documents signal when data arrives', fakeAsync(() => {
        // Arrange
        const mockDocs = [{ id: '1', title: 'Test', /*...*/ }];
        documentService.getAll.and.returnValue(of(mockDocs));

        // Act
        component.ngOnInit();
        tick();

        // Assert
        expect(component.documents()).toEqual(mockDocs);
    }));

    // ✅ Good: Test cleanup
    it('should unsubscribe on destroy', () => {
        // Arrange
        spyOn(component['destroy$'], 'next');
        spyOn(component['destroy$'], 'complete');

        // Act
        component.ngOnDestroy();

        // Assert
        expect(component['destroy$'].next).toHaveBeenCalled();
        expect(component['destroy$'].complete).toHaveBeenCalled();
    });
});
```

### Test Reactive Forms

```typescript
describe('DocumentEditComponent', () => {
    it('should validate form correctly', () => {
        // Arrange
        const component = new DocumentEditComponent();
        component.form.patchValue({
            title: '',  // Invalid: required
            content: 'Some content'
        });

        // Act & Assert
        expect(component.form.invalid).toBe(true);
        expect(component.form.get('title')?.hasError('required')).toBe(true);
    });

    it('should enable submit button when form is valid', () => {
        // Arrange
        const form = component.form;
        form.patchValue({
            title: 'Valid Title',
            content: 'Valid content'
        });

        // Act & Assert
        expect(form.valid).toBe(true);
        expect(component.isSubmitting()).toBe(false);
    });

    it('should call service.create when submitting new document', fakeAsync(() => {
        // Arrange
        const spy = spyOn(documentService, 'create').and.returnValue(of({ id: '1' }));
        component.form.patchValue({ title: 'New', content: 'Content' });

        // Act
        component.onSubmit();
        tick();

        // Assert
        expect(spy).toHaveBeenCalledWith(component.form.value);
    }));
});
```

### Test Service

```typescript
describe('DocumentService', () => {
    let service: DocumentService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [DocumentService]
        });

        service = TestBed.inject(DocumentService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();  // Ensure all HTTP calls were matched
    });

    it('should fetch documents from API', () => {
        // Arrange
        const mockDocs = [{ id: '1', title: 'Test' }];

        // Act
        service.getAll().subscribe(docs => {
            // Assert
            expect(docs).toEqual(mockDocs);
        });

        // Assert HTTP call
        const req = httpMock.expectOne('/api/documents');
        expect(req.request.method).toBe('GET');
        req.flush(mockDocs);
    });

    it('should create document via POST', () => {
        // Arrange
        const newDoc = { title: 'New Doc', content: 'Content' };
        const mockResponse = { id: '1', ...newDoc };

        // Act
        service.create(newDoc).subscribe(doc => {
            // Assert
            expect(doc).toEqual(mockResponse);
        });

        // Assert HTTP call
        const req = httpMock.expectOne('/api/documents');
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(newDoc);
        req.flush(mockResponse);
    });
});
```

---

## 📋 Test Coverage Goals

| Layer | Coverage Target | Focus |
|-------|-----------------|-------|
| Commands/Queries | 80%+ | Happy path, error cases, validation |
| Repositories | 85%+ | CRUD operations, complex queries |
| Services | 80%+ | Business logic, error handling |
| Components | 70%+ | User interactions, data binding |
| Utilities | 90%+ | Edge cases, all branches |

---

## 🎯 Testing Checklist

- [ ] Test happy path (normal operation)
- [ ] Test error cases (exceptions, validation failures)
- [ ] Test null/undefined handling
- [ ] Test edge cases (empty collections, boundary values)
- [ ] Test with CancellationToken
- [ ] Test async operations with fakeAsync/tick
- [ ] Test component lifecycle (ngOnInit, ngOnDestroy)
- [ ] Test unsubscribe/cleanup
- [ ] Mock external dependencies
- [ ] Verify mocks were called correctly (Verify/expect)

---

## 📊 Running Tests

```bash
# Backend - Run all tests
cd src/backend
dotnet test VirtualStudio.slnx

# Backend - Run specific test
dotnet test --filter "CreateUserCommandHandlerTests"

# Frontend - Run tests
cd src/frontend
npm test

# Frontend - Run specific test file
npm test -- documents.component.spec.ts

# Frontend - Coverage
npm test -- --code-coverage
```

---

**Last Updated**: 2026-04-01 | For VirtualStudio v1.0
