---
name: qa-engineer
description: "Testing and quality assurance reasoning and execution skills for {{projectName}}. Use when: writing xUnit unit tests, Playwright E2E tests, functional test cases, verifying tenant isolation, reviewing test coverage, or designing regression suites."
---

# QA Engineer Skills

Defines the reasoning and execution behavior for the `@qa-engineer` agent.

---

## Skill: Test Strategy Design

When designing tests for a feature, follow this reasoning chain:

### 1. Identify Test Scope
- **Unit tests** → Handler logic, entity behavior, validators (xUnit + Moq + FluentAssertions)
- **Integration tests** → API endpoint → handler → database (WebApplicationFactory)
- **E2E tests** → User workflows in browser (Playwright)
- **Functional tests** → Structured test case documents for manual/automated QA

### 2. Required Test Scenarios (EVERY feature)
| Category | Scenarios |
|----------|-----------|
| CRUD | Create valid, duplicate, empty fields; Read by ID, paged list; Update existing, non-existent; Delete soft |
| Authorization | With permission → success; without → 403; expired token → 401; missing token → 401 |
| Multi-Tenancy | Tenant A ≠ Tenant B data; Host sees all; Deactivated tenant blocked |
| Validation | Empty required fields; Max length exceeded; Invalid format; XSS attempts |
| Edge Cases | Null inputs; Unicode; Concurrent operations; Empty database; Boundary values |

### 3. Test Naming Convention
```
{MethodUnderTest}_With{Scenario}_Returns{Expected}
```
Examples:
- `Handle_WithValidInput_ReturnsSuccess`
- `Handle_WithDuplicateName_ReturnsFailure`
- `Handle_WithMissingPermission_Returns403`
- `Create_WithNullName_ThrowsArgumentException`

---

## Skill: Backend Unit Testing

### Handler Test Template
```csharp
public class {Handler}Tests
{
    private readonly Mock<IUserAccountDbContextResolver> _dbResolver = new();
    private readonly Mock<IUserAccountDbContext> _accountDb = new();
    private readonly Mock<ILogger<{Handler}>> _logger = new();
    private readonly {Handler} _handler;

    public {Handler}Tests()
    {
        _dbResolver.Setup(x => x.Resolve()).Returns(_accountDb.Object);
        _handler = new {Handler}(_dbResolver.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var command = new {Command}(/* valid params */);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
```

### Mock Setup Patterns
```csharp
// Return existing entity (found)
_accountDb.Setup(x => x.Find{Entity}ByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync({Entity}.Create(/* params */));

// Return null (not found)
_accountDb.Setup(x => x.Find{Entity}ByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(({Entity}?)null);

// Verify method was called
_accountDb.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

### Assertion Style (FluentAssertions)
```csharp
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
result.Data!.Name.Should().Be("Expected");
result.Error.Should().Contain("already exists");
```

---

## Skill: E2E Testing (Playwright)

### Test Structure
```typescript
test.describe('{Feature} - {Scope} Portal', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/login');
        await page.fill('[data-testid="username"]', 'testuser');
        await page.fill('[data-testid="password"]', 'TestPassword123');
        await page.click('[data-testid="login-button"]');
        await page.waitForURL('/dashboard');
    });

    test('should display {feature} list', async ({ page }) => {
        await page.click('[data-testid="nav-{feature}"]');
        await expect(page.locator('[data-testid="{feature}-table"]')).toBeVisible();
    });

    test('should create new {entity}', async ({ page }) => {
        await page.click('[data-testid="nav-{feature}"]');
        await page.click('[data-testid="create-button"]');
        await page.fill('[data-testid="field-name"]', 'Test Entity');
        await page.click('[data-testid="submit-button"]');
        await expect(page.locator('.toast-success')).toBeVisible();
    });
});
```

### Selector Strategy
- ALWAYS use `data-testid` attributes (stable, not tied to CSS/structure)
- NEVER use CSS class selectors (fragile, change with styling)
- NEVER use nth-child or positional selectors (fragile, change with data)

### Test Categories
1. **Smoke tests** → Critical paths (login, dashboard, main CRUD)
2. **Feature tests** → Full CRUD per feature
3. **Permission tests** → Access control verification
4. **Cross-tenant tests** → Data isolation verification
5. **Responsive tests** → Layout at different viewports

---

## Skill: Functional Test Case Generation

### Test Case Template
```
### TC-{NNN}: {Title}
- **Precondition**: {Setup required}
- **Scope**: host | tenant
- **User Role**: admin | user (with permissions)
- **Steps**:
  1. {Action}
  2. {Action}
- **Expected Result**: {Outcome}
- **Priority**: P1 | P2 | P3
```

### Standard Categories
1. CRUD Operations (create, read, update, delete for each entity)
2. Authentication & Authorization (login, tokens, permissions, lockout)
3. Multi-Tenancy Isolation (cross-tenant access prevention)
4. Input Validation (required fields, max length, format, XSS)
5. Edge Cases (empty DB, unicode, concurrency, boundary values)
6. UI/UX (loading states, error messages, navigation, responsive)

---

## Skill: Tenant Isolation Testing

### Critical Test Scenarios
```csharp
// Verify Tenant A cannot access Tenant B's data
[Fact]
public async Task GetEntities_AsTenantA_DoesNotReturnTenantBData()
{
    // Arrange: Create entities for Tenant A and Tenant B
    // Act: Query as Tenant A
    // Assert: Only Tenant A's entities returned
}

// Verify Host admin CAN see all tenants
[Fact]
public async Task GetEntities_AsHostAdmin_ReturnsAllTenantData()
{
    // Arrange: Create entities across tenants
    // Act: Query as host admin (TenantId = null)
    // Assert: All entities returned regardless of TenantId
}

// Verify deactivated tenant is blocked
[Fact]
public async Task AnyOperation_AsDeactivatedTenant_ReturnsBlocked()
{
    // Arrange: Deactivate tenant in host DB
    // Act: Attempt any operation
    // Assert: Access denied / tenant inactive error
}
```

---

## Skill: Test Coverage Analysis

### Coverage Targets
| Layer | Target | Measured By |
|-------|--------|-------------|
| Domain (Entities) | 90%+ | Entity factory methods, behavior methods |
| Application (Handlers) | 85%+ | Happy path, failure paths, edge cases |
| Application (Validators) | 95%+ | All validation rules exercised |
| Infrastructure (Services) | 70%+ | Core service logic |
| Frontend (E2E) | 80%+ page coverage | All pages visited, CRUD tested |

### Run Commands
```bash
# Backend unit tests
cd src/backend && dotnet test {{projectName}}.Tests --verbosity normal

# Backend with coverage
cd src/backend && dotnet test --collect:"XPlat Code Coverage"

# Frontend E2E
cd src/frontend && npx playwright test

# Frontend E2E with report
cd src/frontend && npx playwright test --reporter=html
```
