# Workflow Patterns: Reusable Development Processes

Multi-step workflows for common development scenarios. Follow these workflows when tackling complex tasks.

---

## 🚀 New Feature Development Workflow

**Timeline**: 2-3 days | **Personas**: Backend Architect → Frontend Specialist → QA Engineer

### Phase 1: Design & Plan (Async - Async - Async)

**Step 1: Requirements Clarification**
```
☐ Define feature scope and user story
☐ Identify host vs tenant vs both
☐ List required permissions
☐ Map data flow and relationships
☐ Estimate complexity (S/M/L/XL)
```

**Step 2: Backend Architecture Design**
```
☐ Identify new entities or modifications
☐ Design commands/queries needed
☐ Define API endpoints (REST contract)
☐ Plan database changes (migrations)
☐ Consider multi-tenancy implications
☐ Identify new services/repositories

Use: architecture-guide.md → Design backend layers
Persona: Backend Architect
```

**Step 3: Frontend Design**
```
☐ Sketch component structure
☐ Plan routing and navigation
☐ Identify required services
☐ Plan permission checks
☐ Identify reusable components

Use: frontend-feature.md → Design component hierarchy
Persona: Frontend Specialist
```

**Step 4: Test Strategy**
```
☐ Identify test scenarios
☐ Edge cases to cover
☐ Multi-tenant test cases
☐ Permission scenarios
☐ Error handling scenarios

Use: unit-testing.md
Persona: QA Engineer
```

### Phase 2: Backend Implementation

**Step 5: Create Domain Entity**
```bash
cd src/backend/Copilot.Domain/Entities
# File: src/backend/Copilot.Domain/Entities/Feature.cs

☐ Define entity with private constructor
☐ Add factory Create() method
☐ Add business logic methods
☐ Define interfaces if needed
```

**Step 6: Create Application Layer (CQRS)**
```bash
cd src/backend/Copilot.Application

☐ Create DTOs (Request + Response)
☐ Create Command/Query definitions
☐ Create Validator
☐ Create Handler implementation
☐ Add to MediatR

Use: backend-feature.md → Step 2-3
```

**Step 7: Create Persistence Layer**
```bash
cd src/backend/Copilot.Persistence

☐ Create IRepository interface
☐ Implement Repository class
☐ Add EF Core configuration (IEntityTypeConfiguration)
☐ Create migration
☐ Test query performance

Use: architecture-guide.md → Persistence section
```

**Step 8: Create API Endpoints**
```bash
cd src/backend/Copilot.Api/Controllers

☐ Create Controller class
☐ Add GET/POST/PUT/DELETE endpoints
☐ Add [Authorize] and [Permission] attributes
☐ Add ProduceResponseType for SW agger
☐ Test with Postman/curl

Use: api-integration.md
Persona: Backend Architect + Security Specialist
```

**Step 9: Backend Unit Tests**
```bash
cd src/backend/Copilot.Tests

☐ Test Command handler (happy path + errors)
☐ Test Repository methods
☐ Test Controller integration
☐ Test multi-tenant isolation
☐ Verify test coverage 80%+

Use: unit-testing.md
Persona: QA Engineer
Command: dotnet test Copilot.slnx
```

### Phase 3: Frontend Implementation

**Step 10: Create Service**
```bash
cd src/frontend/projects/libs/shared/src/lib

☐ Define service interface
☐ Implement HTTP methods (CRUD)
☐ Handle error responses
☐ Add to shared library exports

Use: frontend-feature.md → Step 1
```

**Step 11: Create Components**
```bash
cd src/frontend/projects/portal/src/app/pages

☐ Create list component
☐ Create detail/edit component
☐ Use standalone + OnPush
☐ Use signals for state
☐ Add proper unsubscribe logic

Use: frontend-feature.md → Step 2-3
```

**Step 12: Setup Routing**
```bash
cd src/frontend/projects/portal/src/app

☐ Add routes for feature
☐ Add route guards (AuthGuard, PermissionGuard)
☐ Add permissions check
☐ Add to navigation menu

Use: frontend-feature.md → Step 4-5
```

**Step 13: Frontend Unit Tests**
```bash
cd src/frontend/projects/portal/src

☐ Test service (HTTP calls)
☐ Test component lifecycle
☐ Test user interactions
☐ Test error handling
☐ Verify subscription cleanup

Use: unit-testing.md
Persona: QA Engineer
Command: npm test
```

### Phase 4: Integration & QA

**Step 14: End-to-End Testing**
```bash
cd src/frontend

☐ Test full user workflow
☐ Test both host and tenant portals (if applicable)
☐ Test permission checks
☐ Test error scenarios
☐ Test in multiple browsers

Command: npx playwright test
Persona: QA Engineer
```

**Step 15: Documentation & Code Review**
```
☐ Update README if needed
☐ Document API in Swagger
☐ Add inline comments for complex logic
☐ Create ADR if architectural decision
☐ Prepare for code review

Use: README.md, api-integration.md
Persona: Documentation Writer
```

**Step 16: Security Review**
```
☐ Permission checks in place
☐ No sensitive data in responses
☐ Input validation implemented
☐ Multi-tenant isolation verified
☐ Authentication required for endpoints

Use: system-prompt.md → Security by Default
Persona: Security Specialist
```

---

## 🐛 Production Bug Fix Workflow

**Timeline**: 1-2 hours (depending on complexity) | **Personas**: Debugging Expert → Backend Architect/Frontend Specialist

### Phase 1: Triage & Understanding

**Step 1: Collect Information**
```
☐ What is the exact symptom?
☐ When did it start?
☐ Current error or logs?
☐ User impact (1 user vs all users vs specific tenant)?
☐ Reproducible or intermittent?

Use: bug-fix.md → Step 1
```

**Step 2: Reproduce the Issue**
```
☐ Can you reproduce locally?
☐ What are exact reproduction steps?
☐ Is it specific to a tenant or role?
☐ Does it happen with test data or specific data?

Persona: Debugging Expert
```

**Step 3: Check Logs**
```bash
# Backend logs
tail -f logs/Copilot-*.txt

# Browser console
F12 → Console tab

☐ Any exceptions in logs?
☐ What's the correlation ID?
☐ Are there warnings before the error?

Use: bug-fix.md → Backend Investigation
```

### Phase 2: Root Cause Analysis

**Step 4: Diagnose Backend Issue (if applicable)**
```
☐ Search for error message in code
☐ Trace through the request flow
☐ Check database state (query in SQL Server)
☐ Verify multi-tenant context
☐ Review recent changes to affected layer

Use: bug-fix.md → Common Fixes Reference
```

**Step 5: Diagnose Frontend Issue (if applicable)**
```
☐ Check Network tab (API response)
☐ Verify authentication token
☐ Check component state (debug signals)
☐ Check browser console for errors
☐ Review change detection strategy

Use: bug-fix.md → Frontend Investigation
```

**Step 6: Form Hypothesis**
```
☐ What do I think is causing this?
☐ What evidence supports this?
☐ What would disprove this?
☐ How confident am I? (1-10)

Persona: Debugging Expert
```

### Phase 3: Fix & Verify

**Step 7: Write Failing Test First**
```csharp
[Fact]
public async Task Bug_WhenCondition_ThenErrorOccurs()
{
    // This test reproduces the bug
    // It should FAIL before fix
    // It should PASS after fix
}
```

Use: unit-testing.md → Write a Test for the Bug
Persona: QA Engineer

**Step 8: Implement Fix**
```
☐ Make minimal change to fix root cause
☐ Don't refactor surrounding code
☐ Don't add unnecessary features
☐ Follow project coding standards

Use: coding-standards.md
```

**Step 9: Verify Test Now Passes**
```bash
# Backend
dotnet test --filter BugNameTests

# Frontend
npm test -- bug-name.spec.ts
```

**Step 10: Regression Testing**
```bash
# Run full test suite
dotnet test Copilot.slnx
npm test

☐ No new test failures?
☐ Related functionality still works?
☐ Tested in both host and tenant (if applicable)?

Persona: QA Engineer
```

**Step 11: Manual Verification**
```
☐ Reproduce original issue - FIXED?
☐ Test related features - INTACT?
☐ Test similar code paths - OK?
☐ Performance impact - ACCEPTABLE?

Persona: QA Engineer/Performance Optimizer
```

### Phase 4: Deployment

**Step 12: Review & Commit**
```bash
git status
git diff
# Review changes before commit

git add .
git commit -m "Fix: [Brief description of bug and fix]

Fixes #[Github issue number]
"
```

**Step 13: Communicate**
```
☐ Document the root cause
☐ Update any relevant documentation
☐ Notify affected users
☐ Plan preventive measures

Persona: Documentation Writer
```

---

## 📈 Tenant Provisioning Workflow

**Timeline**: 15-30 minutes | **Personas**: DevOps Engineer → Backend Architect

### Phase 1: Prerequisites

```
☐ New tenant name (e.g., "acme")
☐ Display name (e.g., "Acme Corp")
☐ Admin email address
☐ Admin password (strong)
☐ Database server details (host, credentials)
```

### Phase 2: Database Setup

**Step 1: Create Tenant Database**
```sql
-- In SQL Server Management Studio
CREATE DATABASE VS_Acme;

-- Run migrations for tenant database
```

**Step 2: Configure Connection String**
```csharp
// The connection string will be encrypted automatically
"Server=SQLSERVER;Database=VS_Acme;Trusted_Connection=true;TrustServerCertificate=true"
```

### Phase 3: Tenant Provisioning

**Step 3: Run Provisioning Script**
```powershell
cd scripts

.\provision-tenant.ps1 `
  -ApiBaseUrl "https://localhost:5001" `
  -TenancyName "acme" `
  -DisplayName "Acme Corp" `
  -ConnectionString "Server=SQLSERVER;Database=VS_Acme;..." `
  -AdminEmail "admin@acme.com" `
  -AdminPassword "P@ssw0rd!" `
  -AccessToken "your-jwt-token"

# Script performs:
# ☐ Create tenant in host database
# ☐ Encrypt connection string
# ☐ Create admin user in tenant database
# ☐ Seed default roles and permissions
```

### Phase 4: Verification

**Step 4: Verify Tenant**
```bash
# In browser, visit:
https://acme.localhost:4200

# Login with:
# Email: admin@acme.com
# Password: P@ssw0rd!

# Verify:
☐ Portal loads
☐ Admin can login
☐ Default features visible
☐ Can create sample data
```

---

## 🎓 Code Review Workflow

**Timeline**: 30 minutes per PR | **Personas**: Mentor → Security Specialist → Performance Optimizer

### Phase 1: Initial Review

**Step 1: Understand the Change**
```
☐ What problem does this solve?
☐ What user story/issue is it related to?
☐ What files were changed?
☐ How much code changed?
```

**Step 2: Read the Code**
```
☐ Follow the code from entry point
☐ Understand the data flow
☐ Check error handling
☐ Review test coverage
```

### Phase 2: Correctness Check

**Step 3: Verify Behavior**
```
☐ Does it solve the stated problem?
☐ Are all edge cases handled?
☐ Are error messages user-friendly?
☐ Does it follow project patterns?

Use: architecture-guide.md, coding-standards.md
Persona: Mentor
```

**Step 4: Security Review**
```
☐ Permission checks in place?
☐ Input validation for all user input?
☐ No SQL injection risks?
☐ No XSS vulnerabilities?
☐ Multi-tenant isolation?
☐ Sensitive data not logged?

Use: system-prompt.md → Security by Default
Persona: Security Specialist
```

**Step 5: Performance Review**
```
☐ N+1 queries?
☐ Unnecessary change detection?
☐ Memory leaks possible?
☐ Inefficient algorithms?

Use: bug-fix.md → Performance Issues
Persona: Performance Optimizer
```

### Phase 3: Feedback

**Step 6: Provide Constructive Comments**
```
Format for each comment:
- What: Brief description
- Why: Explain the concern
- How: Suggest improvement with example

Types of issues:
- 🔴 BLOCKING: Must fix before merge
- 🟡 IMPORTANT: Should fix before merge
- 🟢 NICE-TO-HAVE: Can fix in follow-up

Persona: Mentor
```

**Step 7: Checklist for Approval**
```
☐ Solves the stated problem
☐ Follows coding standards
☐ Tests included and passing
☐ No security issues
☐ No major performance issues
☐ Code is readable/maintainable
☐ Documentation updated
☐ No merge conflicts

Approval: ✅ APPROVED
```

---

## 📊 Performance Investigation Workflow

**Timeline**: 1-4 hours | **Personas**: Performance Optimizer → Backend Architect → Debugging Expert

### Phase 1: Identify Bottleneck

**Step 1: Measure Baseline**
```
☐ What's the current response time?
☐ What's acceptable?
☐ When did it get slow?
☐ Is it specific to certain queries?
```

**Step 2: Profile the Operation**
```bash
# Backend - EF Core logging
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
// Enable SQL logging to see queries

# Frontend - Chrome DevTools
F12 → Network tab
- Look for slow API responses
- Look for waterfall (serial vs parallel)

# Frontend - Performance tab
F12 → Performance tab
- Record and analyze rendering
- Look for long tasks
- Check JavaScript execution time
```

### Phase 2: Root Cause Analysis

**Step 3: Analyze Queries**
```
☐ Are there N+1 queries?
☐ Could we use Include()?
☐ Could we use Select() to project fewer fields?
☐ Is there a missing database index?

Use: architecture-guide.md → Repository Pattern
Persona: Performance Optimizer
```

**Step 4: Analyze Rendering**
```
☐ Is change detection running too often?
☐ Are we using OnPush + signals?
☐ Are we rendering unnecessary components?
☐ Are there untracked loops?

Use: frontend-feature.md
Persona: Frontend Specialist
```

### Phase 3: Optimization

**Step 5: Implement Fix**
```
Example optimizations:
- Add .AsNoTracking() to read queries
- Use .Select() for projection
- Add database index
- Use lazy loading or virtual scrolling
- Use OnPush change detection
- Implement pagination
```

**Step 6: Measure Improvement**
```bash
# Baseline again
Original: 2500ms
After fix: 250ms
Improvement: 90% faster ✅

# Verify with load testing
# Could be single slow query, but may show N+1 under load
```

**Step 7: Verify No Regression**
```bash
dotnet test
npm test

☐ All tests pass?
☐ Functionality intact?
☐ Still correct results?
```

---

## 🏗️ Architecture Refactor Workflow

**Timeline**: 2-5 days | **Personas**: Backend Architect → QA Engineer

### Before Refactoring

```
☐ Document current architecture
☐ Identify pain points
☐ Make case for refactoring
☐ Get team agreement
☐ Have good test coverage
```

### During Refactoring

```
☐ Make small changes at a time
☐ Run tests after each change
☐ Git commit frequently
☐ Create feature branch
☐ Keep out of way of other features
```

### After Refactoring

```
☐ All tests pass
☐ No performance regression
☐ Code review completed
☐ Documentation updated
☐ Deploy with confidence
```

---

**Last Updated**: 2026-04-01 | For Copilot v1.0
