# {{projectName}} AI Development System

> **Note**: This folder contains reference documentation for the {{projectName}} AI-assisted development system. The actual Copilot configuration files live in `.github/` and `.vscode/`. See [Quick Start](../.github/COPILOT_QUICK_START.md).

---

## System Overview

{{projectName}} uses a layered AI assistance architecture with GitHub Copilot:

| Layer | Location | Auto-Loaded? | Purpose |
|-------|----------|-------------|---------|
| **Behaviour** | `.github/copilot-instructions.md` | ✅ Yes | Global coding rules, patterns, constraints |
| **Instructions** | `.github/instructions/*.instructions.md` | ✅ Yes (by file type) | File-pattern-based rules for .cs, .ts, tests |
| **Agents** | `.github/agents/*.agent.md` | Via `@agent-name` | Persona-specific expertise (5 agents) |
| **Prompts** | `.github/prompts/*.prompt.md` | Via attach (📎) | Task-specific recipes and workflows |
| **Skills** | `.github/skills/*/SKILL.md` | Via agent reference | Agent reasoning and execution skills |
| **MCP** | `.vscode/mcp.json` | ✅ Yes | External tool integration (5 servers) |

---

## Agents (Custom Chat Participants)

| Agent | Icon | Expertise |
|-------|------|-----------|
| `@backend-architect` | 🏗️ | .NET 10, Clean Architecture, CQRS, EF Core, Multi-Tenancy |
| `@frontend-engineer` | 🎨 | Angular 20, Standalone, Signals, OnPush, @libs/* |
| `@fullstack-architect` | 🌐 | End-to-end feature delivery across all layers |
| `@devops-engineer` | 🚀 | Docker, K8s, CI/CD, Migrations, Provisioning |
| `@qa-engineer` | 🧪 | xUnit, Playwright, Functional Tests, Tenant Isolation |

---

## Instructions (Auto-Applied by File Type)

| File | `applyTo` Pattern | Enforces |
|------|-------------------|----------|
| `backend.instructions.md` | `**/*.cs` | Clean Architecture, CQRS, Result\<T\>, tenant isolation |
| `frontend.instructions.md` | `**/*.{ts,html,scss}` | Standalone, OnPush, signals, @libs/*, FontAwesome |
| `testing.instructions.md` | `**/{{projectName}}.Tests/**` | xUnit patterns, required scenarios, FluentAssertions |
| `multi-tenancy.instructions.md` | Tenant-related files | Database-per-tenant, query filters, encrypted connections |
| `security.instructions.md` | Security-related files | Permissions, AES, BCrypt, OWASP Top 10 |

---

## Prompts (Attach via 📎 in Chat)

### Task Prompts
| Prompt | Use When |
|--------|----------|
| `backend-new-feature` | Creating a new entity with full CRUD across all backend layers |
| `backend-add-handler` | Adding a command/query to an existing feature |
| `backend-migration` | Generating EF Core migrations |
| `frontend-new-page` | Building a new Angular CRUD page |
| `frontend-add-component` | Creating shared components, services, guards |
| `test-unit-backend` | Writing xUnit tests for handlers/entities/validators |
| `test-e2e-playwright` | Writing Playwright E2E tests |
| `test-functional` | Generating functional test case documentation |
| `bug-fix` | Diagnosing and fixing bugs |
| `production-support` | Triaging production incidents |
| `code-review` | Reviewing code for quality and security |
| `hard_critic_review` | Deep architecture audit with SMART criteria |
| `requirements-analysis` | Breaking down requirements into implementation plans |

### Workflow Prompts (Multi-Phase)
| Workflow | Phases |
|----------|--------|
| `workflow-fullstack-feature` | Requirements → Backend → Frontend → Tests → Migration → Review |
| `workflow-bug-resolution` | Triage → Root Cause → Fix → Regression Test → Verify |
| `workflow-enhance-feature` | Analyze → Impact → Backend → Frontend → Tests → Verify |
| `workflow-tenant-provisioning` | Database → Migrate → Seed → Register → DNS → Verify |

---

## MCP Servers

| Server | Package | Purpose |
|--------|---------|---------|
| `github` | GitHub Copilot MCP | Repository context, issues, PRs |
| `filesystem` | `@modelcontextprotocol/server-filesystem` | Read/write workspace files |
| `fetch` | `@modelcontextprotocol/server-fetch` | Fetch web pages for docs |
| `angular` | `@playwright/mcp` | Browser automation for Angular testing |
| `dotnet` | `@upstash/context7-mcp` | .NET documentation and API context |

---

## Reference Documentation (This Folder)

The files in `.copilot-chat/` provide supplementary architecture reference:

| Folder | Contents |
|--------|----------|
| `01-behaviour-layer/` | System prompt, architecture guide, coding standards |
| `02-task-layer/` | Task-specific guides for backend, frontend, testing, API |
| `03-persona-layer/` | AI persona definitions and expertise areas |
| `04-workflow-layer/` | Reusable workflow patterns |

---

**Last Updated**: 2026-04-01 | **Version**: 1.0 | **For**: {{projectName}} v1.0
