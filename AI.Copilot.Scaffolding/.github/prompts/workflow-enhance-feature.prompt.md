---
mode: 'agent'
description: "Safely enhance an existing feature without breaking current functionality. Use when: adding capabilities to existing entities, extending APIs, adding UI elements, or modifying behavior while preserving backward compatibility."
---

# Workflow: Enhance Existing Feature

Add new capabilities to an existing feature without breaking current functionality.

## Input Required
- **Feature Area**: Which existing feature (e.g., `Users`, `Documents`, `Notifications`)
- **Enhancement**: What to add or change
- **Backward Compatibility**: Must existing API contracts be preserved?

## Workflow Phases

### Phase 1: Current State Analysis
1. Read the existing files across all layers:
   - Entity: `Copilot.Domain/Entities/{Entity}.cs`
   - DTOs: `Copilot.Application/DTOs/{Area}/`
   - Handlers: `Copilot.Application/Features/{Area}/`
   - Controller: `Copilot.Api/Controllers/{Area}/`
   - Frontend: `projects/portal/src/app/pages/{scope}/{feature}/`
2. Map current API contract (endpoints, request/response shapes)
3. Identify existing unit tests to understand tested behaviors

### Phase 2: Impact Assessment
1. What changes to the entity? (new properties, new methods)
2. What changes to the database? (new columns, indexes, constraints)
3. What changes to the API? (new endpoints, modified responses)
4. What changes to the frontend? (new UI elements, modified forms)
5. Will existing tests break? How to update them?

### Phase 3: Backend Enhancement
1. **Entity changes**: Add new properties (private set), new behavior methods
   - Do NOT change existing Create() signature if backward compat needed
   - Add overloaded Create() or UpdateXxx() methods instead
2. **DTO changes**: Add new properties to existing DTOs (nullable for backward compat)
3. **Handler changes**: Modify existing or add new handlers
4. **Validator changes**: Add rules for new fields
5. **EF Config changes**: Add new column configurations, indexes
6. **Migration**: Generate for the schema change

### Phase 4: Frontend Enhancement
1. Update TypeScript interfaces to match new DTO shape
2. Update ApiService methods if endpoints changed
3. Update component templates to display new data/controls
4. Add permission checks for new capabilities

### Phase 5: Test Updates
1. Update existing tests to account for new behavior
2. Add new tests for the enhancement
3. Verify existing tests still pass

### Phase 6: Verify
```bash
cd src/backend && dotnet build Copilot.slnx && dotnet test Copilot.Tests
cd src/frontend && ng build portal
```

## Golden Rule
Enhance, don't replace. Every existing test should still pass after the enhancement.
