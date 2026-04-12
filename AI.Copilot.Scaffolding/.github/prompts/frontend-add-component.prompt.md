---
mode: 'agent'
description: "Add or modify an Angular component, service, guard, or interceptor. Use when: creating shared UI widgets, adding services, building guards, or creating directives following Copilot standalone component patterns."
---

# Frontend: Add/Modify Component or Service

## Input Required
- **Type**: `component` | `service` | `guard` | `interceptor` | `directive` | `pipe`
- **Name**: (e.g., `invoice-status-badge`)
- **Location**: `portal` | `libs/{library-name}`
- **Purpose**: Brief description

## Component Template
```typescript
import { Component, ChangeDetectionStrategy, inject, input, output, signal } from '@angular/core';

@Component({
    selector: 'app-{name}',
    standalone: true,
    imports: [],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `<!-- inline for small components -->`,
    // OR templateUrl: './{name}.component.html',
    // OR styleUrl: './{name}.component.scss',
})
export class {Name}Component {
    // Inputs (use signal-based inputs in Angular 20)
    label = input.required<string>();
    disabled = input(false);

    // Outputs
    clicked = output<void>();

    // State
    private readonly someService = inject(SomeService);
    isOpen = signal(false);
}
```

## Service Template
```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class {Name}Service {
    private readonly http = inject(HttpClient);
}
```

## Guard Template (Functional)
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const {name}Guard: CanActivateFn = (route, state) => {
    const router = inject(Router);
    // guard logic
    return true;
};
```

## Library Placement Rules
| What | Where |
|------|-------|
| Shared UI widgets (toast, dialog, badge, spinner) | `@libs/ui` |
| API calls, DTOs, utility functions | `@libs/shared` |
| Auth guards, interceptors, token management | `@libs/auth` |
| Permission guards, directives | `@libs/permissions` |
| Theme service, CSS variable management | `@libs/theme` |
| Dynamic form generation | `@libs/forms-dynamic` |
| Feature-specific components | `projects/portal/src/app/pages/` |

## Conventions
- Always export from the library's `public-api.ts`
- Use `signal()` for local state, not BehaviorSubject
- Use `input()` and `output()` (Angular 20 signal-based) for component I/O
- Use `inject()` function, never constructor injection
- Prefer inline templates for components under 30 lines of HTML
