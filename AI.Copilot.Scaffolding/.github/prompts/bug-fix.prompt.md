---
agent: agent
description: "Systematic bug diagnosis and resolution. Use when: debugging errors, unexpected behavior, or crashes in backend, frontend, or database layers. Follows structured investigation through middleware → handler → DbContext pipeline."
---

# Bug Fix: Diagnose & Resolve

Systematic approach to finding and fixing bugs in {{projectName}}.

## Input Required
- **Symptom**: What is happening? (error message, unexpected behavior, crash)
- **Expected Behavior**: What should happen?
- **Reproduction Steps**: How to trigger the bug
- **Layer Suspicion**: `backend` | `frontend` | `database` | `infra` | `unknown`

## Diagnosis Process

### Phase 1: Understand the Symptom
1. **Read the error message carefully** - extract: exception type, stack trace location, inner exceptions
2. **Check correlation** - Is this tenant-specific? User-specific? Endpoint-specific? Time-dependent?
3. **Check recent changes** - `git log --oneline -20` to see what changed recently

### Phase 2: Backend Investigation
If backend issue:
1. **Check API logs** - Look at Serilog structured logs for the correlation ID
2. **Trace the request flow**:
   - `Middleware` → `Controller` → `MediatR Pipeline` → `ValidationBehavior` → `Handler` → `DbContext`
3. **Common backend issues**:
   - `ValidationException` → Check FluentValidation rules in `Validators/`
   - `UnauthorizedAccessException` → Check `[Authorize]` policies and PermissionChecker
   - `DbUpdateException` → Check EF configurations, constraints, query filters
   - `NullReferenceException` → Check `IUserAccountDbContextResolver.Resolve()` returns correct context
   - Tenant data leak → Check query filters, `TenantId` assignments
   - `InvalidOperationException` in DI → Check service lifetimes (Singleton vs Scoped)

### Phase 3: Frontend Investigation
If frontend issue:
1. **Check browser console** - JavaScript errors, network failures (4xx/5xx)
2. **Check Angular change detection** - Components use OnPush, signals must be updated correctly
3. **Common frontend issues**:
   - Component not updating → Not using signals or OnPush not triggered
   - API errors not shown → Check HttpErrorInterceptor, error handling in subscribe
   - Route not accessible → Check guards (auth, permission, feature, host/tenant)
   - Styles not applied → Component encapsulation, SCSS import paths

### Phase 4: Database Investigation
If database issue:
1. **Check migration status** - Are all migrations applied?
2. **Check data integrity** - IsDeleted flags, TenantId values, FK references
3. **Check query filters** - EF global filters might hide/expose wrong data
4. **Tenant isolation check** - `SELECT * FROM {Table} WHERE TenantId = @TenantId`

## Fix Implementation
1. Write the fix following existing code patterns
2. Add/update unit tests to cover the bug scenario (regression test)
3. Test locally: `dotnet test` (backend), `ng serve` (frontend)
4. Verify fix doesn't affect other tenants/features

## Fix Checklist
- [ ] Root cause identified and documented
- [ ] Fix follows existing code patterns
- [ ] Regression test added
- [ ] No tenant isolation violations
- [ ] No new security vulnerabilities introduced
- [ ] Related cache entries invalidated if data fix
