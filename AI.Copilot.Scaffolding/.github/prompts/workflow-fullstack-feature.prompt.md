---
agent: agent
description: "Complete end-to-end full stack feature delivery. Use when: building a feature from scratch across all layers — requirements → domain → application → persistence → API → frontend → tests → migration → code review."
---

# Workflow: Full Stack Feature Delivery

Complete end-to-end feature development across all layers of {{projectName}}.

## Input Required
- **Feature Requirement**: What needs to be built
- **Scope**: `host-only` | `tenant-only` | `shared`

## Workflow Phases

### Phase 1: Requirements Analysis
Use the approach from `requirements-analysis.prompt.md`:
1. Break requirement into functional/non-functional items
2. Identify all affected layers (Domain, Application, Persistence, Api, Frontend)
3. List new entities, DTOs, handlers, endpoints, components needed
4. Identify permissions and feature flags needed
5. Output: Implementation task list with dependencies

### Phase 2: Backend Implementation
Use `backend-new-feature.prompt.md` for each new entity:
1. **Domain Entity** → `{{projectName}}.Domain/Entities/`
2. **DTOs** → `{{projectName}}.Application/DTOs/{Feature}/`
3. **Command/Query definitions** → `{{projectName}}.Application/Features/{Feature}/Commands/` and `Queries/`
4. **Handlers** → Same directories (one file per handler or grouped CRUD)
5. **Validators** → `{{projectName}}.Application/Features/{Feature}/Validators/`
6. **DbContext interface methods** → `{{projectName}}.Application/Interfaces/IHostDbContext.cs` or `ITenantDbContext.cs`
7. **EF Configuration** → `{{projectName}}.Persistence/Configurations/EntityConfigurations.cs`
8. **DbContext implementation** → `{{projectName}}.Persistence/Contexts/`
9. **API Controller** → `{{projectName}}.Api/Controllers/{Area}/`
10. **Permission seeds** → `{{projectName}}.Persistence/Seeders/PermissionSeeder.cs`

### Phase 3: Database Migration
Use `backend-migration.prompt.md`:
1. Generate migration for Host or Tenant context
2. Review generated SQL
3. Update seeders if initial data needed

### Phase 4: Frontend Implementation
Use `frontend-new-page.prompt.md`:
1. **TypeScript interfaces** → Add to `@libs/shared`
2. **ApiService methods** → Add to `@libs/shared/api.service.ts`
3. **List component** → `projects/portal/src/app/pages/{scope}/{feature}/`
4. **Form component** (if needed) → Same directory
5. **Route registration** → `app.routes.ts` with guards
6. **Navigation** → Add to shell sidebar

### Phase 5: Testing
Use `test-unit-backend.prompt.md` + `test-functional.prompt.md`:
1. **Unit tests** for each handler → `{{projectName}}.Tests/Application/`
2. **Entity tests** → `{{projectName}}.Tests/Domain/`
3. **Validator tests** → `{{projectName}}.Tests/Application/`
4. **Functional test cases** → Document for manual QA

### Phase 6: Code Review
Use `code-review.prompt.md`:
1. Architecture compliance check
2. Security review
3. Multi-tenancy safety verification
4. Performance check

### Phase 7: Verify Build
```bash
# Backend
cd src/backend && dotnet build {{projectName}}.slnx && dotnet test {{projectName}}.Tests

# Frontend
cd src/frontend && ng build portal
```

## Completion Criteria
- [ ] All backend layers implemented following patterns
- [ ] EF migration generated and reviewed
- [ ] Frontend CRUD page functional
- [ ] Routes with proper guards
- [ ] Unit tests passing
- [ ] Code review checklist clean
- [ ] Build succeeds for both backend and frontend
