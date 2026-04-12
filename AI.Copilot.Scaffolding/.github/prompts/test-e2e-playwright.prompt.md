---
mode: 'agent'
description: "Write Playwright end-to-end tests for Angular features. Use when: testing user flows, CRUD operations, permission enforcement, or cross-tenant isolation in the browser. Uses data-testid selectors."
---

# E2E Testing: Frontend (Playwright)

Write Playwright end-to-end tests for Copilot portal features.

## Input Required
- **Feature**: Which page/flow to test (e.g., `login`, `tenant-management`, `document-upload`)
- **Scope**: `host` | `tenant`
- **Test Scenarios**: List of user flows to verify

## Test Structure
Tests are in `src/frontend/e2e/`:

```typescript
import { test, expect } from '@playwright/test';

test.describe('{Feature} - {Scope} Portal', () => {
    test.beforeEach(async ({ page }) => {
        // Navigate and authenticate
        // For host: go to admin/host URL
        // For tenant: go to {subdomain}.{domain} URL
        await page.goto('/login');
        await page.fill('[data-testid="username"]', 'testuser');
        await page.fill('[data-testid="password"]', 'TestPassword123');
        await page.click('[data-testid="login-button"]');
        await page.waitForURL('/dashboard');
    });

    test('should display {feature} list', async ({ page }) => {
        await page.click('[data-testid="nav-{feature}"]');
        await expect(page.locator('[data-testid="{feature}-table"]')).toBeVisible();
    });

    test('should create new {entity}', async ({ page }) => {
        await page.click('[data-testid="nav-{feature}"]');
        await page.click('[data-testid="create-button"]');
        // Fill form fields
        await page.fill('[data-testid="field-name"]', 'Test Entity');
        await page.click('[data-testid="submit-button"]');
        // Verify success toast or redirect
        await expect(page.locator('.toast-success')).toBeVisible();
    });

    test('should enforce permission access', async ({ page }) => {
        // Login as user without permission
        // Verify restricted elements are hidden or route is blocked
    });
});
```

## Conventions
- Use `data-testid` attributes for selectors (add to templates if missing)
- Test user flows, not implementation details
- Include tests for: CRUD operations, validation errors, permission enforcement, empty states, pagination
- Use `test.describe.configure({ mode: 'serial' })` for dependent tests

## Run Tests
```bash
cd src/frontend && npx playwright test
```
