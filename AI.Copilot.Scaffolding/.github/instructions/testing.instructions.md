---
applyTo: "**/VirtualStudio.Tests/**"
description: "Testing standards for VirtualStudio. Applies to test files. Enforces xUnit + Moq + FluentAssertions patterns, required test scenarios, and naming conventions."
---

# Testing Standards — VirtualStudio

## Backend Unit Tests (xUnit + Moq + FluentAssertions)

### Naming Convention
```
{MethodUnderTest}_With{Scenario}_Returns{Expected}
```

### Handler Test Template
```csharp
public class Create{Entity}CommandHandlerTests
{
    private readonly Mock<IUserAccountDbContextResolver> _dbResolver = new();
    private readonly Mock<IUserAccountDbContext> _accountDb = new();
    private readonly Mock<ILogger<Create{Entity}CommandHandler>> _logger = new();
    private readonly Create{Entity}CommandHandler _handler;

    public Create{Entity}CommandHandlerTests()
    {
        _dbResolver.Setup(x => x.Resolve()).Returns(_accountDb.Object);
        _handler = new(_dbResolver.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsSuccess() { }

    [Fact]
    public async Task Handle_WithDuplicate_ReturnsFailure() { }

    [Fact]
    public async Task Handle_WithInvalidInput_ReturnsFailure() { }
}
```

### Required Scenarios (EVERY handler)
1. **Happy path** → `Result<T>.IsSuccess == true`, data populated
2. **Duplicate detection** → `Result<T>.IsSuccess == false`, descriptive error message
3. **Entity not found** → appropriate failure message for update/delete
4. **Business rule violation** → failure before database write
5. **Tenant isolation** → operations scoped to correct TenantId

### Assertion Style (FluentAssertions)
```csharp
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
result.Data!.Name.Should().Be("Expected");
result.Error.Should().Contain("already exists");
```

### Mock Setup Patterns
```csharp
// Return existing entity
_accountDb.Setup(x => x.FindUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(User.Create("John", "Doe", "john@test.com", "hash"));

// Return null (not found)
_accountDb.Setup(x => x.FindUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((User?)null);

// Verify method was called
_accountDb.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

## Test File Locations
```
VirtualStudio.Tests/Domain/        → Entity behavior tests
VirtualStudio.Tests/Application/   → Handler and validator tests
VirtualStudio.Tests/Infrastructure/ → Service tests
```
