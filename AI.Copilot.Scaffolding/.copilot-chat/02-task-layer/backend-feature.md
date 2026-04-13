# Backend Feature Development Prompt

**When to use**: Adding new CRUD operations, business logic features, or API endpoints to the backend.

---

## 📋 Pre-Development Checklist

Before writing code, clarify:

1. **Scope**: Is this Host-only, Tenant-only, or both?
2. **Permissions**: What permission check is needed? (e.g., "Users.Manage", "Tenants.View")
3. **Data Model**: What are the entity fields and relationships?
4. **Audit**: Does this entity need audit tracking (CreatedByUserId, etc.)?
5. **Validation**: What business rules validate this data?
6. **API Contract**: What request/response DTOs are needed?

---

## 🎯 Development Workflow

### Step 1: Design the Domain Entity
**File**: `src/backend/{{projectName}}.Domain/Entities/{FeatureName}.cs`

```csharp
// Use factory pattern + private constructor
// Include only business logic methods, NO persistence concerns
// Use BaseEntity for audit fields (CreatedByUserId, CreatedAt, IsDeleted)

public class Document : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public Guid TenantId { get; private set; }
    public Guid CreatedByUserId { get; private set; }  // Audit

    private Document() { }

    public static Document Create(string title, string content, Guid tenantId)
    {
        return new Document { Title = title, Content = content, TenantId = tenantId };
    }

    public void Update(string title, string content) { Title = title; Content = content; }
}
```

### Step 2: Create DTOs in Application Layer

**File**: `src/backend/{{projectName}}.Application/DTOs/{FeatureName}Dto.cs`

```csharp
// Request DTO (for API input)
public class CreateDocumentRequest
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}

// Response DTO (for API output)
public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }

    public static DocumentDto FromEntity(Document entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Content = entity.Content,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt
    };
}
```

### Step 3: Create Command/Query + Handlers

**File**: `src/backend/{{projectName}}.Application/Features/{Feature}/Commands/{FeatureName}Commands.cs`

```csharp
// Define commands as records
public record CreateDocumentCommand(string Title, string Content) : IRequest<Result<DocumentDto>>;
public record UpdateDocumentCommand(Guid DocumentId, string Title, string Content) : IRequest<Result<DocumentDto>>;
public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result>;

// Define validators
public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty();
    }
}

// Implement handlers
public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Result<DocumentDto>>
{
    private readonly ITenantDbContextFactory _dbFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateDocumentCommand> _validator;

    public async Task<Result<DocumentDto>> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<DocumentDto>(string.Join(", ", validation.Errors));

        var db = await _dbFactory.CreateAsync(ct);
        var tenantId = db.TenantId;  // Multi-tenant context
        var userId = _currentUser.UserId ?? Guid.Empty;

        var document = Document.Create(request.Title, request.Content, tenantId);
        document.CreatedByUserId = userId;  // Set audit field

        await db.Documents.AddAsync(document, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success(DocumentDto.FromEntity(document));
    }
}
```

### Step 4: Create Repository Methods

**File**: `src/backend/{{projectName}}.Persistence/Repositories/{Feature}Repository.cs`

```csharp
public interface IDocumentRepository
{
    Task<Document?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Document>> FindAllByTenantAsync(Guid tenantId, CancellationToken ct);
    Task AddAsync(Document entity, CancellationToken ct);
    Task UpdateAsync(Document entity, CancellationToken ct);
    Task DeleteAsync(Document entity, CancellationToken ct);
}

public class DocumentRepository : IDocumentRepository
{
    private readonly TenantDbContext _db;

    public async Task<Document?> FindByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<Document>> FindAllByTenantAsync(Guid tenantId, CancellationToken ct) =>
        await _db.Documents
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(Document entity, CancellationToken ct) {
        await _db.Documents.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
    }
}
```

### Step 5: Register Dependencies

**File**: `src/backend/{{projectName}}.Api/Program.cs`

```csharp
// In dependency injection registration
builder.Services
    .AddScoped<IDocumentRepository, DocumentRepository>()
    .AddScoped<CreateDocumentCommandHandler>();
```

### Step 6: Create API Endpoints

**File**: `src/backend/{{projectName}}.Api/Controllers/{Feature}Controller.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    [Permission("Documents.Manage")]
    [ProduceResponseType(typeof(DocumentDto), 201)]
    public async Task<IActionResult> Create(CreateDocumentRequest request)
    {
        var command = new CreateDocumentCommand(request.Title, request.Content);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id}")]
    [Permission("Documents.View")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : NotFound();
    }

    [HttpGet]
    [Permission("Documents.View")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDocumentsQuery());
        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Permission("Documents.Manage")]
    public async Task<IActionResult> Update(Guid id, UpdateDocumentRequest request)
    {
        var command = new UpdateDocumentCommand(id, request.Title, request.Content);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    [Permission("Documents.Manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDocumentCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
```

### Step 7: Create EF Core Configuration

**File**: `src/backend/{{projectName}}.Persistence/Configurations/DocumentConfiguration.cs`

```csharp
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Content)
            .IsRequired();

        builder.Property(d => d.TenantId)
            .IsRequired();

        builder.HasIndex(d => d.TenantId);
        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.ToTable("Documents");
    }
}
```

### Step 8: Add Unit Tests

**File**: `src/backend/{{projectName}}.Tests/Features/Documents/CreateDocumentCommandHandlerTests.cs`

```csharp
public class CreateDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesDocument()
    {
        // Arrange
        var mockDb = new Mock<ITenantDbContext>();
        var mockRepo = new Mock<IDocumentRepository>();
        var mockValidator = new Mock<IValidator<CreateDocumentCommand>>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var handler = new CreateDocumentCommandHandler(mockDb.Object, mockValidator.Object, mockCurrentUser.Object);
        var cmd = new CreateDocumentCommand("Test", "Content");

        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## ✅ Checklist

- [ ] Entity created in Domain with factory pattern
- [ ] DTOs created (Request + Response)
- [ ] Command/Query with handler and validator created
- [ ] Repository methods added
- [ ] Dependencies registered in Program.cs
- [ ] API controller with proper authorization created
- [ ] EF Core entity configuration added
- [ ] Unit tests written
- [ ] Permission created (if new permission needed)
- [ ] Tested locally with Postman/curl

---

## 🚀 Quick Database Migration

```bash
cd src/backend

# Add migration
dotnet ef migrations add Add_Documents \
    --project {{projectName}}.Persistence \
    --context TenantDbContext \
    --output-dir Migrations

# Or if multiple DbContexts:
dotnet ef migrations add Add_Documents \
    --project {{projectName}}.Persistence \
    --context HostDbContext \
    --startup-project {{projectName}}.Api \
    --output-dir Migrations/Host

# Run migrations
dotnet run --project {{projectName}}.Migrator
```

---

**Tips**:
- Always use ITenantDbContextFactory or IUserAccountDbContextResolver, not DbContext directly
- Remember soft deletes: queries are auto-filtered with .HasQueryFilter(x => !x.IsDeleted)
- Audit fields (CreatedByUserId, CreatedAt) are auto-set by DbContext.SaveChangesAsync
- Test multi-tenant isolation: ensure TenantId is included in queries
