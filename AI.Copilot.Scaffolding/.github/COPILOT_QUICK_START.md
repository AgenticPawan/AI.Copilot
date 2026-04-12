# VirtualStudio Copilot System — Quick Start Guide

## System Architecture

```
.github/
  copilot-instructions.md           ← BEHAVIOUR LAYER (auto-loaded by Copilot)
  
  agents/                           ← PERSONA LAYER (custom chat participants)
    🏗️ backend-architect.agent.md    → @backend-architect
    🎨 frontend-engineer.agent.md    → @frontend-engineer
    🌐 fullstack-architect.agent.md  → @fullstack-architect
    🚀 devops-engineer.agent.md      → @devops-engineer
    🧪 qa-engineer.agent.md          → @qa-engineer
  
  instructions/                     ← AUTO-APPLIED RULES (file-pattern based)
    backend.instructions.md          → Applies to **/*.cs
    frontend.instructions.md         → Applies to **/*.{ts,html,scss}
    testing.instructions.md          → Applies to **/VirtualStudio.Tests/**
    multi-tenancy.instructions.md    → Applies to tenant-related files
    security.instructions.md         → Applies to security-related files
  
  prompts/                          ← TASK + WORKFLOW LAYERS (attach in chat)
    backend-new-feature.prompt.md    → New entity + full CRUD
    backend-add-handler.prompt.md    → Add command/query to existing entity
    backend-migration.prompt.md      → EF Core migration
    frontend-new-page.prompt.md      → New Angular CRUD page
    frontend-add-component.prompt.md → Add/modify component/service
    test-unit-backend.prompt.md      → xUnit tests
    test-e2e-playwright.prompt.md    → Playwright E2E tests
    test-functional.prompt.md        → Functional test cases
    bug-fix.prompt.md                → Diagnose and fix bugs
    production-support.prompt.md     → Production issue triage
    code-review.prompt.md            → Quality and security review
    hard_critic_review.prompt.md     → Deep architecture audit
    requirements-analysis.prompt.md  → Implementation planning
    workflow-fullstack-feature.prompt.md   → Full stack delivery
    workflow-bug-resolution.prompt.md      → Bug lifecycle
    workflow-enhance-feature.prompt.md     → Enhance existing feature
    workflow-tenant-provisioning.prompt.md → New tenant setup
    modules/
      auth-session-management-module.prompt.md → Auth module spec

  skills/                           ← REASONING SKILLS (agent capabilities)
    backend-architect/SKILL.md       → Architecture reasoning skills

  workflows/                        ← CI/CD (GitHub Actions)
    saas-compliance.yml
    saas-feature-management.yml

.vscode/
  mcp.json                          ← MCP SERVERS (GitHub, Filesystem, Fetch, Angular, .NET)
  settings.json                     ← Copilot settings and auto-approve rules
```

---

## VS Code Setup (One-Time)

### 1. Workspace Instructions (Automatic)
VS Code reads `.github/copilot-instructions.md` automatically. No manual setup needed.
Verify: Open Copilot Chat → type any question → response should respect VirtualStudio patterns.

### 2. File-Based Instructions (Automatic)
Instructions in `.github/instructions/` auto-apply based on `applyTo` patterns:
- Edit a `.cs` file → backend rules loaded automatically
- Edit a `.ts` file → frontend rules loaded automatically
- Edit a test file → testing rules loaded automatically

### 3. Custom Agents
Type `@agent-name` in Copilot Chat to invoke persona-specific expertise:
```
@backend-architect     → .NET / Clean Architecture / CQRS / EF Core
@frontend-engineer     → Angular 20 / Standalone / Signals / OnPush
@fullstack-architect   → End-to-end feature delivery across all layers
@devops-engineer       → Docker / K8s / CI/CD / Migrations / Provisioning
@qa-engineer           → xUnit / Playwright / Functional tests / Tenant isolation
```

### 4. Prompt Files
Click the **attach** button (paperclip icon) in Copilot Chat → select a `.prompt.md` file to load task-specific guidance.

---

## Quick Recipes

### Add a New Feature (Full Stack)
1. Attach `workflow-fullstack-feature.prompt.md`
2. Type: `Add an Invoices feature for tenant-only scope with fields: Title (string), Amount (decimal), DueDate (DateTime), IsPaid (bool)`

### Add Just the Backend
1. Attach `backend-new-feature.prompt.md`
2. Type: `Create Invoice entity with Title, Amount, DueDate, IsPaid`

### Add Just a Frontend Page
1. Attach `frontend-new-page.prompt.md`
2. Type: `Create invoices page for tenant scope with InvoiceDto fields: id, title, amount, dueDate, isPaid`

### Add a Handler to Existing Feature
1. Attach `backend-add-handler.prompt.md`
2. Type: `Add MarkInvoiceAsPaid command to Invoices with InvoiceId parameter`

### Fix a Bug
1. Attach `bug-fix.prompt.md`
2. Type: `Users get 403 when accessing /api/documents even though they have Documents.View permission`

### Write Unit Tests
1. Attach `test-unit-backend.prompt.md`
2. Type: `Write tests for CreateInvoiceCommandHandler covering: valid creation, duplicate title, missing required fields`

### Code Review
1. Attach `code-review.prompt.md`
2. Type: `Review changes in src/backend/VirtualStudio.Application/Features/Invoices/`

### Production Incident
1. Attach `production-support.prompt.md`
2. Type: `P2 - Tenant "acmecorp" users see empty document list. Other tenants work fine.`

---

## MCP Servers

The workspace has these MCP servers configured in `.vscode/mcp.json`:

| Server | Purpose |
|--------|---------|
| `github` | GitHub Copilot MCP integration |
| `filesystem` | Read/write workspace files under `src/` |
| `fetch` | Fetch web pages for documentation lookup |
| `angular` | Playwright browser automation for Angular testing |
| `dotnet` | Context7 documentation lookup for .NET APIs |

---

## Tips for Best Results

1. **Combine agent + prompt**: `@backend-architect` + attach `backend-new-feature.prompt.md` for most focused output
2. **Always specify scope**: Say `host-only`, `tenant-only`, or `shared` in every request
3. **Reference existing patterns**: Say "follow the Tenants CRUD pattern" for consistency
4. **Use `@workspace`**: Include when Copilot needs to explore existing code
5. **Chain prompt files**: Start with `requirements-analysis.prompt.md`, then follow up with specific task prompts
6. **Instructions auto-load**: No need to manually reference — they apply based on file type
