---
agent: agent
description: "Write xUnit unit tests with Moq and FluentAssertions. Use when: testing MediatR handlers, entity behavior, validators, or services. Generates tests for happy path, duplicates, validation failures, not-found, and tenant isolation."
---

# Unit Testing: Backend (xUnit + Moq + FluentAssertions)

Write unit tests for {{projectName}} backend code.

## Input Required
- **Target**: Class/method to test (e.g., `CreateTenantCommandHandler`, `User.RecordFailedLogin`)
- **Test Scope**: `handler` | `entity` | `service` | `validator`

## Handler Test Template
```csharp
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using {{projectName}}.Application.Common;
using {{projectName}}.Application.Interfaces;
using {{projectName}}.Domain.Entities;

namespace {{projectName}}.Tests.Application;

public class {Handler}Tests
{
    // Mock ALL constructor dependencies
    private readonly Mock<IUserAccountDbContext> _accountDb = new();
    private readonly Mock<IUserAccountDbContextResolver> _dbResolver = new();
    private readonly Mock<ILogger<{Handler}>> _logger = new();
    // Add other mocks as needed: IEncryptionService, IAuditLogService, etc.
    private readonly {Handler} _handler;

    public {Handler}Tests()
    {
        // Setup common mocks
        _dbResolver.Setup(x => x.Resolve()).Returns(_accountDb.Object);
        _handler = new {Handler}(/* inject all mocks */);
    }

    [Fact]
    public async Task Handle_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var command = new {Command}(/* valid params */);
        _accountDb.Setup(x => x.Find{Entity}ByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(/* test entity */);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicate_ReturnsFailure()
    {
        // Arrange - setup mock to return existing entity (duplicate)
        // Act
        // Assert - result.IsSuccess.Should().BeFalse()
    }

    [Fact]
    public async Task Handle_WithInvalidState_ReturnsFailure()
    {
        // Test business rule violations
    }
}
```

## Entity Test Template
```csharp
public class {Entity}Tests
{
    [Fact]
    public void Create_WithValidParams_SetsPropertiesCorrectly()
    {
        var entity = {Entity}.Create(/* params */);

        entity.Should().NotBeNull();
        entity.Name.Should().Be("expected");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void {BusinessMethod}_ChangesStateCorrectly()
    {
        var entity = {Entity}.Create(/* params */);
        entity.{BusinessMethod}(/* params */);

        entity.{Property}.Should().Be(expectedValue);
    }
}
```

## Validator Test Template
```csharp
public class {Validator}Tests
{
    private readonly {Validator} _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_Passes()
    {
        var command = new {Command}(/* valid params */);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithInvalidName_Fails(string name)
    {
        var command = new {Command}(Name: name, /* other params */);
        var result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}
```

## Test Categories to Cover
1. **Happy path** - valid input produces expected output
2. **Business rule violations** - duplicates, invalid state transitions, unauthorized
3. **Edge cases** - null inputs, empty strings, boundary values, max lengths
4. **Entity behavior** - factory methods, state change methods, computed properties
5. **Tenant isolation** - operations respect tenant boundaries

## File Location
Place tests in `src/backend/{{projectName}}.Tests/{Layer}/`:
- `Application/` for handler and validator tests
- `Domain/` for entity tests
- `Infrastructure/` for service tests

## Run Tests
```bash
cd src/backend && dotnet test {{projectName}}.Tests
```
