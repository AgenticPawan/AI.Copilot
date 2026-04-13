# Bug Fix & Debugging Prompt

**When to use**: Production issues, unexpected behavior, failing tests, or performance problems.

---

## 🔍 Debugging Methodology

### Step 1: Understand the Symptom
```
What exactly is happening?
- Error message or stacktrace?
- Unexpected behavior?
- Performance degradation?
- Data inconsistency?

When does it occur?
- After specific user action?
- On specific feature/route?
- For specific tenant or all tenants?
- Intermittent or consistent?

Who is affected?
- All users or specific role?
- Specific tenant or host?
- New feature or regression?
```

### Step 2: Identify Root Cause

#### 🖥️ Backend Investigation

**Check Logs**:
```bash
# Serilog outputs to:
# - Console (development)
# - File: logs/{{projectName}}-*.txt
# - Seq: http://localhost:5341 (if running)

# Look for:
# - Exception stacktraces
# - Correlation IDs (trace causally related requests)
# - User/Tenant context
```

**Debug Checklist**:
- [ ] Check exception type and message
- [ ] Verify multi-tenancy context (is TenantId correct?)
- [ ] Confirm authentication/authorization (JWT valid? Permission granted?)
- [ ] Review database state (query manually in SQL Server)
- [ ] Check service dependencies (are they injected correctly?)
- [ ] Trace request flow through middleware
- [ ] Review recent changes to affected layer

**Common Issues**:

```csharp
// ❌ NullReferenceException: Permission not checked
[Authorize]  // ← Missing [Permission(...)]
public IActionResult GetSensitiveData() { }

// ❌ Multi-tenancy bug: Missing TenantId filter
var documents = await _db.Documents  // ← Missing .Where(d => d.TenantId == tenantId)
    .ToListAsync();

// ❌ Soft delete not handled: Query includes deleted items
var users = await _db.Users.ToListAsync();  // ← BaseEntity.HasQueryFilter(x => !x.IsDeleted) should auto-filter

// ❌ Async/await error: CancellationToken ignored
public async Task<Result> ProcessAsync()
{
    return await SomeAsync();  // ← Missing CancellationToken parameter
}

// ❌ Dependency injection: Service not registered
services.AddScoped<IUserRepository, UserRepository>();  // ← UserService also needs registration
```

#### 🎨 Frontend Investigation

**Check Browser Console**:
```
- JavaScript errors
- Network errors (failed API calls)
- CORS issues
- Auth token expiration
```

**Debug Checklist**:
- [ ] Check browser Network tab (API response)
- [ ] Verify authentication token exists and is valid
- [ ] Check permissions (does user have required permission?)
- [ ] Inspect component state signals
- [ ] Check service subscriptions (are they unsubscribed?)
- [ ] Review template for syntax errors
- [ ] Verify route guards are not blocking
- [ ] Check change detection (is OnPush set? Is signal updated?)

**Common Issues**:

```typescript
// ❌ Memory leak: Subscription not unsubscribed
ngOnInit() {
    this.service.getData().subscribe(data => this.data = data);  // ← No unsubscribe!
}

// ❌ Change detection: Forgot OnPush
@Component({
    // ← Missing: changeDetection: ChangeDetectionStrategy.OnPush
    template: `{{ data }}`  // May not update
})

// ❌ Template rendering: Async operations not awaited
template: `
    <div>{{ user.name }}</div>  <!-- If user is from async operation, might be undefined -->
</div>`

// ❌ Missing track in @for loop
@for (item of items(); track item) {  <!-- Should use unique identifier -->

// ❌ Route guard: Permission not checked
canActivate: [AuthGuard]  // ← Missing PermissionGuard or permission check
```

---

## 🛠️ Fix Workflow

### Example 1: Fix a Backend Exception

```
Problem: "User not found" error when updating profile

1. Check logs for correlation ID:
   [2026-04-01 15:32:45.123] WARN: User not found for ID '...' | CorrelationId: abc-123

2. Find the handler throwing this error:
   - Search codebase for "User not found"
   - Found: UpdateUserCommandHandler

3. Analyze the code:
   var user = await _userRepo.FindByIdAsync(userId, ct);
   if (user == null)
       return Result.Failure("User not found");

4. Add debugging:
   _logger.LogInformation("Searching for user {UserId} in {TenantId}", userId, tenantId);

5. Root cause analysis:
   - Is the userId correct?
   - Is repository method filtering by TenantId?
   - Is soft-delete query filter active?

6. Fix:
   public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
       await _dbContext.Users
           .Where(u => u.TenantId == _currentTenantId)  // ← Add TenantId filter
           .FirstOrDefaultAsync(u => u.Id == id, ct);

7. Test:
   - Add unit test for this case
   - Manually test with Postman
   - Verify in specific tenant context
```

### Example 2: Fix a Frontend Bug

```
Problem: Component not showing data after navigation

1. Check browser console for errors
   - Are there API errors? (Network tab)
   - Are there JavaScript exceptions?

2. Add debug logging:
   ngOnInit() {
       console.log('Component init, loading data...');
       this.service.getData().subscribe(data => {
           console.log('Data loaded:', data);
           this.data.set(data);
       });
   }

3. Root cause analysis:
   - Is the API being called? (Check Network tab)
   - Does the service return data? (Check Response)
   - Is change detection running? (Check signal was updated)

4. Fix - Example 1 (Missing signal update):
   this.service.getData().subscribe(data => {
       this.data.set(data);  // ← Ensure signal is set
   });

5. Fix - Example 2 (Unmount before data loads):
   this.service.getData()
       .pipe(takeUntil(this.destroy$))  // ← Unsubscribe on destroy
       .subscribe(data => this.data.set(data));

6. Test:
   - Manual browser test
   - Add unit test
   - Verify in different states (loading, error, success)
```

---

## 🧪 Write a Test for the Bug

Always write a test that reproduces the bug **before** fixing it:

```csharp
[Fact]
public async Task FindByIdAsync_WithWrongTenant_ReturnsNull()
{
    // This test reproduces the bug: query not filtering by tenant

    // Arrange
    var tenant1Id = Guid.NewGuid();
    var tenant2Id = Guid.NewGuid();
    var user = User.Create("John", "Doe", "john@example.com", "hash");
    user.TenantId = tenant1Id;

    var dbContext = new TestDbContext();
    await dbContext.Users.AddAsync(user);
    await dbContext.SaveChangesAsync();

    var repo = new UserRepository(dbContext);

    // Act - Query from different tenant
    var currentTenantId = tenant2Id;  // Different tenant!
    var result = await repo.FindByIdAsync(user.Id, CancellationToken.None);

    // Assert - Should NOT find user from different tenant
    Assert.Null(result);  // ← Currently fails, confirming bug exists
}
```

After this test fails (confirming the bug), fix the code:

```csharp
public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
    await _dbContext.Users
        .Where(u => u.TenantId == _currentTenantId)  // ← Fix added
        .FirstOrDefaultAsync(u => u.Id == id, ct);
```

Now the test passes! ✅

---

## 📋 Fix Verification Checklist

After fixing, verify:

- [ ] **Root cause addressed**: Does the fix actually solve the root cause?
- [ ] **No regressions**: Are other tests still passing?
- [ ] **Properly tested**: Is there a test case for this bug?
- [ ] **Logging added**: Can future issues be traced?
- [ ] **Edge cases**: Are similar bugs possible elsewhere?
- [ ] **Documentation**: Is fix explanation clear for future developers?
- [ ] **Performance**: Does fix not introduce new performance issues?
- [ ] **Security**: Does fix not introduce new security concerns?

---

## 🚀 Common Fixes Reference

### Backend

**Multi-tenancy Issues**:
```csharp
// ❌ Bug: Query includes data from other tenants
var docs = await _db.Documents.ToListAsync();

// ✅ Fix: Add TenantId filter
var docs = await _db.Documents
    .Where(d => d.TenantId == _currentTenantId)
    .ToListAsync();
```

**Authorization Issues**:
```csharp
// ❌ Bug: No permission check
[Authorize]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id) { }

// ✅ Fix: Add permission check
[HttpDelete("{id}")]
[Permission("Documents.Manage")]  // ← Required
public async Task<IActionResult> Delete(Guid id) { }
```

**Null Reference Issues**:
```csharp
// ❌ Bug: Not checking null
var user = await _userRepo.FindByIdAsync(id, ct);
var firstName = user.FirstName;  // ← NullReferenceException

// ✅ Fix: Check null
var user = await _userRepo.FindByIdAsync(id, ct);
if (user == null) return Result.Failure("User not found");
var firstName = user.FirstName;
```

### Frontend

**Unsubscribe Issues**:
```typescript
// ❌ Bug: Memory leak
this.service.getData().subscribe(data => this.data.set(data));

// ✅ Fix: Unsubscribe
this.service.getData()
    .pipe(takeUntil(this.destroy$))
    .subscribe(data => this.data.set(data));
```

**Change Detection Issues**:
```typescript
// ❌ Bug: Not using OnPush
@Component({
    template: `{{ data }}`
})

// ✅ Fix: Use OnPush + signal
@Component({
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `{{ data() }}`
})
export class MyComponent {
    data = signal(null);
}
```

**Async Handling Issues**:
```typescript
// ❌ Bug: Error not handled
this.service.getData().subscribe(data => this.data = data);

// ✅ Fix: Handle errors
this.service.getData().subscribe({
    next: data => this.data = data,
    error: err => console.error('Error:', err),
    complete: () => console.log('Completed')
});
```

---

## 💡 Prevention Tips

1. **Add logging** at important decision points
2. **Write tests** before shipping features
3. **Review multi-tenancy** logic carefully
4. **Check permissions** on every endpoint
5. **Use compiler warnings** (treat as errors)
6. **Monitor performance** in production
7. **Set up error tracking** (Sentry, Application Insights)
8. **Document assumptions** in code comments
9. **Code review** with focus on edge cases
10. **Run full test suite** before committing

---

**Last Updated**: 2026-04-01 | For {{projectName}} v1.0
