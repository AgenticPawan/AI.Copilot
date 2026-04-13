---
agent: agent
description: "Analyze feature requirements and produce implementation plans. Use when: breaking down new features, estimating effort, identifying affected layers, planning database changes, or creating task lists with dependencies."
---

# Requirements Analysis & Implementation Plan

Analyze a feature requirement and produce a structured implementation plan for {{projectName}}.

## Input Required
- **Requirement**: Description of what needs to be built or changed
- **Priority**: `must-have` | `should-have` | `nice-to-have`
- **Scope**: `backend-only` | `frontend-only` | `fullstack`

## Analysis Output Format

### 1. Requirement Breakdown
Parse the requirement into:
- **Functional Requirements**: What the system must DO
- **Non-Functional Requirements**: Performance, security, UX constraints
- **Assumptions**: What we're assuming (clarify with stakeholder if unsure)
- **Out of Scope**: What this does NOT include

### 2. Impact Analysis
Identify affected areas:
- **Entities**: New or modified domain entities
- **Database**: Schema changes, migrations needed
- **API Endpoints**: New or modified endpoints
- **Frontend Pages**: New or modified components
- **Permissions**: New permission groups or policies
- **Feature Flags**: New tenant features needed
- **Infrastructure**: Encryption, caching, SignalR, email changes

### 3. Implementation Tasks
Break into ordered tasks with layer designation:

| # | Task | Layer | Files Affected | Dependencies |
|---|------|-------|---------------|-------------|
| 1 | Create Entity | Domain | Entities/{Name}.cs | None |
| 2 | Add DTOs | Application | DTOs/{Name}/ | #1 |
| 3 | Create Handlers | Application | Features/{Name}/ | #1, #2 |
| ... | ... | ... | ... | ... |

### 4. Testing Strategy
- Unit tests needed (handlers, entities, validators)
- E2E scenarios to verify
- Manual QA checkpoints
- Tenant isolation verification steps

### 5. Migration Checklist
- [ ] Database migration needed?
- [ ] Seeder data needed?
- [ ] Permission seeding needed?
- [ ] Feature flag registration needed?
- [ ] Cache strategy defined?
- [ ] SignalR notifications needed?

## Conventions
- Reference existing similar features as patterns (e.g., "follow Tenants CRUD pattern")
- Identify which prompt files to use for each task (e.g., "use backend-new-feature.prompt.md for task 1-8")
- Flag any architectural decisions that need stakeholder input
