---
mode: 'agent'
description: "Comprehensive code review for quality, security, and compliance. Use when: reviewing PRs, checking architecture compliance, verifying multi-tenancy safety, or auditing security before deployment."
---

# Code Review: Quality & Security Check

Review Copilot code changes for compliance with project patterns, security, and multi-tenancy safety.

## Input Required
- **Files to Review**: File paths or git diff
- **Change Type**: `new-feature` | `bug-fix` | `refactor` | `enhancement`

## Review Checklist

### Architecture Compliance
- [ ] Code is in the correct layer (Domain logic not in Controllers, business rules not in API)
- [ ] MediatR pattern followed (no direct service calls from controllers)
- [ ] Result<T> pattern used (no throwing exceptions for business failures)
- [ ] DTOs used for API responses (entities never exposed directly)

### Entity Rules
- [ ] Private parameterless constructor present
- [ ] Static Create() factory method used
- [ ] All setters are private
- [ ] State changes via behavior methods only
- [ ] Navigation properties: IReadOnlyCollection backed by List

### Security Review
- [ ] `[Authorize(Policy = "Permission:...")]` on all new endpoints
- [ ] No sensitive data in DTOs (passwords, connection strings, encryption keys)
- [ ] Input validation via FluentValidation for all commands
- [ ] No raw SQL queries (use EF Core LINQ)
- [ ] AES encryption used for sensitive stored data
- [ ] No hardcoded secrets or connection strings

### Multi-Tenancy Safety (CRITICAL)
- [ ] New entities have TenantId where applicable
- [ ] Query filters applied in EF configuration: `HasQueryFilter(e => !e.IsDeleted)`
- [ ] Tenant-specific queries filter by TenantId
- [ ] `IUserAccountDbContextResolver.Resolve()` used (not hardcoded to Host or Tenant)
- [ ] No cross-tenant data access possible
- [ ] Connection strings encrypted before storage

### Frontend Compliance
- [ ] Components are standalone with OnPush
- [ ] inject() function used (not constructor injection)
- [ ] Signals used for state (not BehaviorSubject)
- [ ] Imports from @libs/* (not relative paths to libraries)
- [ ] Lazy-loaded routes via loadComponent
- [ ] Permission/feature guards on routes and UI elements

### Performance
- [ ] Async/await used correctly with CancellationToken
- [ ] Paged queries for list endpoints
- [ ] OutputCache on read endpoints
- [ ] Cache eviction after mutations
- [ ] AsNoTracking() on read-only queries
- [ ] No N+1 query patterns (use Include or projection)

### Testing
- [ ] Unit tests added for new handlers
- [ ] Entity behavior tests for new domain methods
- [ ] Validator tests for new command validators
- [ ] Edge cases covered (null, empty, boundary values)
