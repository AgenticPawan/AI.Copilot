---
name: fullstack-architect
description: "Full stack architecture reasoning and execution skills for {{projectName}}. Use when: designing end-to-end features across .NET {{dotnetVersion}} backend and Angular {{angularVersion}} frontend, ensuring cross-layer consistency, planning implementation phases, or coordinating backend-frontend integration."
---

# Full Stack Architect Skills

Defines the reasoning and execution behavior for the `@fullstack-architect` agent.

---

## Skill: End-to-End Feature Planning

When planning a new feature, follow this reasoning chain:

### 1. Scope Determination
- **Host-only** → Admin portal, uses `IHostDbContext`, `hostGuard` on routes
- **Tenant-only** → Tenant portal, uses `ITenantDbContext`, `tenantGuard` + `featureGuard` on routes
- **Shared** → Both portals, needs `IUserAccountDbContextResolver.Resolve()`, conditional guards

### 2. Domain Analysis
- Identify entities, value objects, and aggregate boundaries
- Determine relationships (one-to-many, many-to-many)
- Define which entities need `TenantId`
- Map business rules and invariants

### 3. Layer-by-Layer Implementation Plan
For each feature, generate a 10-phase plan:

| Phase | Layer | Artifacts |
|-------|-------|-----------|
| 1 | Domain | Entity with factory, behavior methods |
| 2 | Application | DTOs, Commands, Queries as records |
| 3 | Application | Handlers with Result\<T\>, try/catch, audit |
| 4 | Application | FluentValidation validators |
| 5 | Persistence | EF configuration, DbContext methods, query filters |
| 6 | Api | Controller with auth policies, cache, notifications |
| 7 | Persistence | Permission seeds, migration |
| 8 | Frontend Shared | TypeScript interfaces, ApiService methods |
| 9 | Frontend Portal | Page components (list, form), routing |
| 10 | Tests | xUnit handlers + Playwright E2E |

### 4. Cross-Layer Consistency Validation
Before completing the plan, verify:
- [ ] DTO properties (PascalCase) ↔ TypeScript interfaces (camelCase) match exactly
- [ ] API routes ↔ ApiService URLs match
- [ ] Permission constants ↔ `permissionGuard()` strings match
- [ ] Feature flag names ↔ `featureGuard()` strings match
- [ ] `PagedResult<T>` shape matches on both sides
- [ ] SignalR hub event names match between server and client

---

## Skill: Cross-Layer Impact Analysis

When modifying an existing feature:

### Questions to Answer
1. Does the entity change affect migrations? → Plan EF migration
2. Does the DTO change affect the API contract? → Check backward compatibility
3. Does the permission change affect existing users? → Plan permission seed update
4. Does the route change affect bookmarks/links? → Add redirect from old route
5. Does the database change affect tenant databases? → Plan tenant migration strategy

### Impact Matrix
| Change In | Check In |
|-----------|----------|
| Entity (Domain) | DTO, Handler, EF Config, Migration, TypeScript interface |
| DTO (Application) | Controller response, TypeScript interface, Component template |
| Permission (Persistence) | Controller attribute, Route guard, UI directive, Seed data |
| API Route (Api) | ApiService URL, Component API call |
| Feature Flag | Route guard, Seed data, Tenant settings UI |

---

## Skill: Architecture Enforcement

### Golden Rules
1. **Build backend first, then frontend** — never reference an endpoint that doesn't exist yet
2. **TypeScript interfaces must exactly mirror DTOs** — property names, types, nullability
3. **Every endpoint needs a permission** — checked at controller AND route AND UI levels
4. **Always specify scope first** — host-only, tenant-only, or shared
5. **Consider bidirectional impact** — "If I add to HostDbContext, does TenantDbContext need it?"
6. **Maintain test coverage** — every handler gets unit tests, every page gets E2E

### Backend Patterns to Enforce
- Private constructors + static Create() factories on entities
- Result\<T\> from all handlers (never throw for business failures)
- FluentValidation for every command
- CancellationToken on all async methods
- Repository methods on DbContext (no separate repository classes)

### Frontend Patterns to Enforce
- Standalone components with OnPush change detection
- inject() function (never constructor injection)
- signal() for state (never BehaviorSubject)
- @libs/* imports (never relative paths to libraries)
- Lazy-loaded routes with full guard stack

---

## Skill: Feature Delivery Workflow Coordination

### Recommended Agent Delegation
| Phase | Delegate To | Prompt File |
|-------|-------------|-------------|
| Requirements | `@fullstack-architect` | `requirements-analysis.prompt.md` |
| Backend | `@backend-architect` | `backend-new-feature.prompt.md` |
| Migration | `@devops-engineer` | `backend-migration.prompt.md` |
| Frontend | `@frontend-engineer` | `frontend-new-page.prompt.md` |
| Testing | `@qa-engineer` | `test-unit-backend.prompt.md` |
| Review | `@fullstack-architect` | `code-review.prompt.md` |

---

## Skill: Multi-Tenancy Design

### Entity Scope Decision Tree
```
Is this entity managed by host admin only?
  → YES: Host-scoped, no TenantId, use IHostDbContext
  → NO: Is this entity shared across tenants?
    → YES: Shared, TenantId (Guid?), use IUserAccountDbContextResolver
    → NO: Tenant-only, TenantId (Guid), use ITenantDbContext
```

### Database Context Selection
| Scope | Context | TenantId |
|-------|---------|----------|
| Host-only | `IHostDbContext` | null (always) |
| Tenant-only | `ITenantDbContext` | Guid (required) |
| Shared | `IUserAccountDbContextResolver.Resolve()` | Guid? (nullable) |

### Isolation Verification Checklist
- [ ] Global query filter: `HasQueryFilter(e => !e.IsDeleted)`
- [ ] Tenant query filter: `e.TenantId == _tenantId` (tenant context)
- [ ] Encrypted connection strings before storage
- [ ] No cross-tenant data access paths in handlers
- [ ] Feature flags control tenant-specific capabilities
