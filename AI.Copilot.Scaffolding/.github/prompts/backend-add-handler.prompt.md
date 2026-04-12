---
mode: 'agent'
description: "Add a new MediatR command or query handler to an existing feature. Use when: adding operations like Archive, Export, MarkAsPaid, GetByFilter to an already-existing entity. Generates handler, validator, DbContext method, and API endpoint."
---

# Backend: Add Command or Query Handler

Add a new MediatR command or query to an existing feature area in Copilot.

## Input Required
- **Entity**: Which existing entity (e.g., `User`, `Tenant`, `Document`)
- **Operation Type**: `command` (state change) or `query` (read-only)
- **Operation Name**: (e.g., `ArchiveDocument`, `GetUsersByRole`, `ExportTenantData`)
- **Parameters**: (e.g., `Guid DocumentId, string Reason`)
- **Return Type**: (e.g., `DocumentDto`, `List<UserDto>`, `bool`, `byte[]`)

## Execution Steps

### 1. Define the Request
Add to the existing `{Area}Commands.cs` or `{Area}Queries.cs`:
```csharp
public record {OperationName}{Command|Query}({Params}) : IRequest<Result<{ReturnType}>>;
```

### 2. Create the Handler
Create `src/backend/Copilot.Application/Features/{Area}/{Commands|Queries}/{OperationName}Handler.cs`:
- Follow existing handler patterns in that feature area
- Use `IUserAccountDbContextResolver.Resolve()` for user-scoped operations
- Use `IHostDbContext` directly for host-only operations
- Wrap in try/catch with structured logging
- Return `Result<T>.Success()` or `Result<T>.Failure("message")`

### 3. Add Validator (Commands Only)
Add to existing validators file or create new:
```csharp
public class {OperationName}CommandValidator : AbstractValidator<{OperationName}Command> { }
```

### 4. Add DbContext Methods (If Needed)
If the handler needs a query not already available:
- Add method to appropriate interface (IHostDbContext, IUserAccountDbContext, ITenantDbContext)
- Implement in the DbContext class

### 5. Add API Endpoint
Add action to the existing controller:
```csharp
[Authorize(Policy = "Permission:{Entity}.{Action}")]
[Http{Verb}("{route}")]
public async Task<IActionResult> {OperationName}({Params})
{
    var result = await _mediator.Send(new {OperationName}{Command|Query}({args}));
    if (!result.IsSuccess) return BadRequest(new { result.Error });
    return Ok(result.Data);
}
```

## Conventions
- Commands that change state: Add audit logging, cache eviction, CRUD notifications
- Queries: Add `[OutputCache(PolicyName = "TenantAware")]`, use `AsNoTracking()`
- Paged queries: Accept `[FromQuery] PagedQuery`, use `ToPagedResultAsync()`
- File downloads: Return `File(bytes, contentType, fileName)`
