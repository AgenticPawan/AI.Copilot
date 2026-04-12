---
mode: 'agent'
description: "Complete bug resolution lifecycle. Use when: handling bugs from triage through root cause analysis, fix implementation, regression testing, and verification. Ensures multi-tenant safety of fixes."
---

# Workflow: Bug Resolution Pipeline

Complete bug lifecycle from triage to verified fix.

## Input Required
- **Bug Report**: What's wrong, error messages, affected scope
- **Severity**: P1 | P2 | P3 | P4

## Workflow Phases

### Phase 1: Triage & Reproduce
Use `production-support.prompt.md`:
1. Classify severity and impact scope
2. Identify affected layer (backend/frontend/database/infra)
3. Reproduce the issue locally
4. Capture: error message, stack trace, affected tenant/user, request details

### Phase 2: Root Cause Analysis
Use `bug-fix.prompt.md`:
1. Trace the request through the middleware pipeline
2. Identify the exact line/method where behavior diverges from expectation
3. Check for multi-tenancy issues (wrong context, missing TenantId filter)
4. Check for recent regressions (`git log --oneline -20`)
5. Document: Root cause, why it wasn't caught, how to prevent recurrence

### Phase 3: Implement Fix
1. Apply the minimal fix following existing code patterns
2. **Do NOT** refactor surrounding code unless directly related
3. Ensure fix respects tenant isolation
4. Invalidate relevant caches if data was affected

### Phase 4: Regression Test
Use `test-unit-backend.prompt.md`:
1. Write a unit test that **fails without the fix** and **passes with the fix**
2. Name it descriptively: `Handle_With{BugScenario}_Returns{Expected}()`
3. Add edge case variations if applicable

### Phase 5: Verify
```bash
# Run all backend tests
cd src/backend && dotnet test VirtualStudio.Tests

# Build frontend if changed
cd src/frontend && ng build portal
```

### Phase 6: Document
- Root cause summary
- Files changed
- Tests added
- Any follow-up items (monitoring, data fixes, etc.)
