---
name: backend-architect
description: "Backend architecture reasoning and execution skills for {{projectName}}. Use when: designing features across Clean Architecture layers, enforcing CQRS patterns, reviewing code for architecture violations, optimizing EF Core queries, or implementing multi-tenant features."
---

# Backend Architect Skills

Defines the reasoning and execution behavior for the `@backend-architect` agent.

---

## Skill: Feature Design

When designing a new feature, follow this reasoning chain:

### 1. Identify Domain Model
- What entities are needed?
- What value objects exist?
- What aggregate boundaries apply?
- Does this entity need `TenantId`?

### 2. Define CQRS Operations
- What commands (writes) are needed? → Create, Update, Delete, custom state changes
- What queries (reads) are needed? → GetById, GetPaged, custom filters
- What DTOs bridge domain to API?

### 3. Plan Validation & Authorization
- What FluentValidation rules apply to each command?
- What permission policies are needed?
- Are there business rules beyond simple validation?

### 4. Ensure Cross-Cutting Concerns
- Tenant isolation applied?
- Audit logging for state changes?
- Cache strategy (OutputCache on reads, eviction on writes)?
- SignalR notifications needed?

### Output Must Include
For each feature, generate artifacts in this order:
1. Entity (Domain layer)
2. DTOs (Application layer)
3. Commands & Queries (Application layer)
4. Handlers (Application layer)
5. Validators (Application layer)
6. DbContext interface methods + implementation (Persistence layer)
7. EF Configuration (Persistence layer)
8. Controller (Api layer)
9. Permission seeds (Persistence layer)
10. Migration command

---

## Skill: Architecture Enforcement

### Reject These Patterns
- ❌ Business logic in controllers
- ❌ Direct DbContext calls from controllers
- ❌ Public setters on entities
- ❌ Throwing exceptions for business logic failures
- ❌ Missing CancellationToken on async methods
- ❌ Separate repository classes (use DbContext methods)
- ❌ Anemic domain models (entities that are just property bags)

### Enforce These Patterns
- ✅ Rich domain models with behavior methods
- ✅ Static Create() factory, private constructor
- ✅ Result\<T\> for all handler returns
- ✅ FluentValidation for all commands
- ✅ Dependency inversion (interfaces in Application, implementations in Infrastructure/Persistence)
- ✅ CancellationToken propagation through full call chain

---

## Skill: Security Review

Always verify these items:
- [ ] Input validation exists for all external inputs
- [ ] Sensitive data is encrypted (AES-256)
- [ ] No plaintext secrets in source code
- [ ] Authorization policy on every endpoint
- [ ] No cross-tenant data access paths
- [ ] BCrypt for password hashing (never reversible encryption)

---

## Skill: EF Core Optimization

### Use
- `AsNoTracking()` for read-only queries
- Projection queries (`Select`) over loading full entities
- Indexed columns for frequently queried fields
- `HasQueryFilter` for soft delete and tenant isolation

### Avoid
- Lazy loading (disabled by default, keep it that way)
- N+1 queries — use `Include()` or projection
- Loading full entities when only IDs or names are needed

---

## Skill: CQRS Implementation

### Commands (Writes)
1. Validate input (FluentValidation pipeline)
2. Check business rules (duplicates, ownership, state)
3. Execute domain logic (entity methods)
4. Persist changes (DbContext)
5. Audit log (IAuditLogService)
6. Return `Result<T>.Success(dto)`

### Queries (Reads)
1. Build query with filters and pagination
2. Project to DTO (never return entities)
3. Apply `AsNoTracking()`
4. Return `Result<T>.Success(pagedResult)`
