---
agent: agent
description: "Generate structured functional test cases from requirements. Use when: creating QA test plans, defining acceptance criteria, documenting test scenarios for CRUD, auth, multi-tenancy, validation, and edge cases."
---

# Functional Testing: Test Case Generation

Generate structured functional test cases for {{projectName}} features.

## Input Required
- **Feature**: Feature name and description
- **Acceptance Criteria**: What defines "working correctly"
- **Scope**: `host` | `tenant` | `both`

## Test Case Template

### TC-{NNN}: {Test Case Title}
- **Precondition**: {Setup required before test}
- **Scope**: host | tenant
- **User Role**: admin | user (with specific permissions)
- **Steps**:
  1. {Action step}
  2. {Action step}
- **Expected Result**: {What should happen}
- **Priority**: P1 | P2 | P3

## Standard Test Categories

### 1. CRUD Operations
For each entity:
- **Create**: Valid data, duplicate detection, missing required fields, max length violations
- **Read**: List with pagination, search/filter, sort by columns, single entity by ID, not-found
- **Update**: Valid update, partial update, concurrent edit, stale data
- **Delete**: Soft delete, cascade effects, already-deleted entity

### 2. Authentication & Authorization
- Login with valid/invalid credentials
- Token refresh flow
- Expired token handling
- Login lockout after 5 failed attempts (30 min lock)
- Permission-based access: with permission, without permission
- Host vs Tenant portal access separation

### 3. Multi-Tenancy Isolation
- Tenant A cannot see Tenant B's data
- Host admin can see all tenants
- Tenant-specific features enabled/disabled
- Cross-tenant API calls blocked
- Tenant deactivation blocks all access

### 4. Input Validation
- Required fields empty
- Max length exceeded
- Invalid format (email, subdomain name, password policy)
- XSS/injection attempts in text fields
- File upload: invalid type, oversized, empty file

### 5. Edge Cases
- Empty database (first-time setup)
- Single item in paginated list
- Unicode characters in names
- Network timeout during save
- Concurrent operations on same entity

### 6. UI/UX (Frontend)
- Loading states displayed during API calls
- Error messages shown on failure
- Success toast on completion
- Navigation guards block unauthorized routes
- Responsive layout at different screen sizes

## Output Format
Generate test cases organized by category with unique IDs (TC-001, TC-002, ...) for traceability.
