# API Integration & Endpoint Development Prompt

**When to use**: Designing new API endpoints, modifying contracts, versioning, or integrating external services.

---

## 🎯 REST API Design Principles

### Request/Response Pattern

```csharp
// ✅ Good: Clear naming, proper status codes, nested DTOs
[HttpPost("documents")]
[Authorize]
[Permission("Documents.Manage")]
[ProduceResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status201Created)]
[ProduceResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProduceResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request)
{
    var command = new CreateDocumentCommand(request.Title, request.Content);
    var result = await _mediator.Send(command);

    return result.IsSuccess
        ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, new ApiResponse<DocumentDto>
        {
            Success = true,
            Data = result.Data
        })
        : BadRequest(new ApiError { Message = result.Error });
}
```

### HTTP Method Conventions

```csharp
// ✅ Good: Standard REST conventions
[HttpGet("{id}")]           // Retrieve single resource
public async Task<IActionResult> GetById(Guid id)

[HttpGet]                   // List all resources (with pagination)
public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)

[HttpPost]                  // Create new resource
public async Task<IActionResult> Create([FromBody] CreateRequest request)

[HttpPut("{id}")]           // Replace entire resource
public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest request)

[HttpPatch("{id}")]         // Partial update
public async Task<IActionResult> PartialUpdate(Guid id, [FromBody] JsonPatchDocument<UpdateRequest> patch)

[HttpDelete("{id}")]        // Delete resource
public async Task<IActionResult> Delete(Guid id)
```

### Status Codes

```csharp
// ✅ Good: Correct status codes
200 OK              // GET, PUT successful
201 Created         // POST successful
204 No Content      // DELETE successful, no response body
400 Bad Request     // Validation failed, malformed request
401 Unauthorized    // Missing/invalid JWT token
403 Forbidden       // User lacks permission
404 Not Found       // Resource doesn't exist
409 Conflict        // Duplicate resource, business rule violation
422 Unprocessable   // Validation error (alternative to 400)
500 Internal Error  // Unexpected server error
```

---

## 📝 Request & Response DTOs

### Request DTO Pattern

```csharp
// ✅ Good: Focused, well-documented
public class CreateDocumentRequest
{
    /// <summary>
    /// Document title (max 200 characters)
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Document content
    /// </summary>
    [Required]
    public string Content { get; set; } = null!;

    /// <summary>
    /// Optional tags (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string? Tags { get; set; }
}

// ✅ Good: Separate request from internal entity
public class UpdateDocumentRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    // Note: TenantId NOT in request (set from context)
    // Note: CreatedAt/CreatedByUserId NOT updatable (immutable)
}
```

### Response DTO Pattern

```csharp
// ✅ Good: Only expose what client needs
public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = null!;  // Include user info
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedByUserName { get; set; }

    // ❌ Don't include sensitive fields:
    // - TenantId (client shouldn't know internal structure)
    // - CreatedByUserId (expose as CreatedByUserName instead)
    // - Database-specific fields

    public static DocumentDto FromEntity(Document entity, string createdByUserName) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            Tags = entity.Tags,
            CreatedAt = entity.CreatedAt,
            CreatedByUserName = createdByUserName
        };
}
```

### Error Response Pattern

```csharp
public class ApiError
{
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Field-level validation errors
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Unique error code for client handling
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Trace ID for support reference
    /// </summary>
    public string? TraceId { get; set; }
}

// ✅ Good: Structured error responses
return BadRequest(new ApiError
{
    Message = "Validation failed",
    Code = "VALIDATION_ERROR",
    Errors = new Dictionary<string, string[]>
    {
        { "Title", new[] { "Title is required", "Title must be at least 1 character" } },
        { "Content", new[] { "Content is required" } }
    },
    TraceId = HttpContext.TraceIdentifier
});
```

---

## 🔐 Query Parameters & Filtering

### Pagination Pattern

```csharp
// ✅ Good: RESTful pagination
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,           // 1-indexed
    [FromQuery] int pageSize = 10,      // Items per page
    [FromQuery] string? sortBy = "createdAt",  // Field to sort
    [FromQuery] bool descending = true)  // Sort direction
{
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 10;

    var query = new GetDocumentsQuery(page, pageSize, sortBy, descending);
    var result = await _mediator.Send(query);

    return Ok(new PaginatedResponse<DocumentDto>
    {
        Data = result.Items,
        Page = result.Page,
        PageSize = result.PageSize,
        TotalCount = result.TotalCount,
        TotalPages = result.TotalPages,
        HasNextPage = result.HasNextPage,
        HasPreviousPage = result.HasPreviousPage
    });
}

// ✅ Good: Filtering pattern
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string? searchTerm,
    [FromQuery] string? status,
    [FromQuery] DateTime? createdAfter)
{
    var query = new GetDocumentsQuery(searchTerm, status, createdAfter);
    var result = await _mediator.Send(query);
    return Ok(result);
}

// Pagination response
public class PaginatedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

## 📚 API Versioning Strategy

```csharp
// ✅ Good: URL-based versioning (clear, cacheable)
[Route("api/v1/[controller]")]
[Route("api/v2/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class DocumentsController : ControllerBase
{
    // V1 endpoint (legacy)
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAll_V1()
    {
        // Returns old format
    }

    // V2 endpoint (new)
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetAll_V2()
    {
        // Returns new format
    }
}

// OR Header-based versioning
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    [ApiVersion("1.0", Deprecated = true)]
    public IActionResult GetAll() { }

    [HttpGet]
    [ApiVersion("2.0")]
    public IActionResult GetAll_V2() { }
}

// ✅ Good: Deprecation timeline
// - Support V1 for 12 months after V2 release
// - Document sunset date in API docs
// - Return 410 Gone after sunset
```

---

## 🔗 API Documentation with OpenAPI/Swagger

```csharp
// ✅ Good: Comprehensive documentation
[HttpPost("documents")]
[Authorize]
[Permission("Documents.Manage")]
[ProduceResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
[ProduceResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
[ProduceResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
[ProduceResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> Create(
    /// <summary>
    /// Document creation request
    /// </summary>
    [FromBody] CreateDocumentRequest request)
{
    /* Implementation */
}

// Program.cs - Setup Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Copilot API",
        Version = "v1",
        Description = "Multi-tenant SaaS API",
        Contact = new OpenApiContact { Name = "Support" }
    });

    // Include JWT scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

app.UseSwaggerUI();
```

---

## 🌐 External Service Integration

### Pattern: Wrapper Interface

```csharp
// ✅ Good: Decouple from external service
public interface IExternalPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(string cardToken, decimal amount);
    Task<bool> RefundAsync(string transactionId, decimal amount);
}

public class StripePaymentService : IExternalPaymentService
{
    private readonly IStripeClient _stripeClient;

    public async Task<PaymentResult> ProcessPaymentAsync(string cardToken, decimal amount)
    {
        try
        {
            var charge = await _stripeClient.Charges.CreateAsync(new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),  // Stripe uses cents
                Currency = "usd",
                Source = cardToken
            });

            return new PaymentResult
            {
                IsSuccess = charge.Paid,
                TransactionId = charge.Id,
                Message = charge.FailureMessage
            };
        }
        catch (StripeException ex)
        {
            // Log and return user-friendly error
            _logger.LogError(ex, "Payment processing failed");
            return new PaymentResult
            {
                IsSuccess = false,
                Message = "Payment failed. Please try again."
            };
        }
    }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
}

// Register in Program.cs
builder.Services.AddSingleton<IExternalPaymentService, StripePaymentService>();
```

### Pattern: Resilience with Polly

```csharp
// ✅ Good: Retry + circuit breaker pattern
builder.Services
    .AddHttpClient<IExternalApiService, ExternalApiService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                _logger.LogWarning($"Retry {retryCount} after {timespan.TotalSeconds}s");
            });

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, timespan) =>
            {
                _logger.LogWarning($"Circuit breaker opened for {timespan.TotalSeconds}s");
            });
```

---

## 📋 Endpoint Development Checklist

- [ ] Correct HTTP method (GET, POST, PUT, PATCH, DELETE)
- [ ] Appropriate status codes (200, 201, 400, 401, 403, 404, 500)
- [ ] [Authorize] attribute added
- [ ] [Permission(...)] attribute added if needed
- [ ] ProduceResponseType attributes for documentation
- [ ] Request DTO validation (required fields, string lengths)
- [ ] Response DTO doesn't expose sensitive data
- [ ] Error response includes Message, Code, TraceId
- [ ] Pagination for list endpoints
- [ ] Query parameters for filtering/sorting
- [ ] Unit tests for success and error paths
- [ ] Swagger/OpenAPI documentation complete
- [ ] Tested with Postman/curl or Swagger UI

---

## 🧪 Testing API Endpoints

```csharp
[IntegrationTest]
public class DocumentsControllerTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DocumentsControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Post_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CreateDocumentRequest { Title = "Test", Content = "Content" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/documents", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
    }

    [Fact]
    public async Task Get_WithoutAuth_Returns401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/documents");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

---

**Last Updated**: 2026-04-01 | For Copilot v1.0
