---
applyTo: "**/*.{ts,html,scss}"
description: "Angular 20 frontend coding standards for Copilot. Applies automatically to TypeScript, HTML, and SCSS files. Enforces standalone components, OnPush, signals, inject(), @libs/* imports, and UI conventions."
---

# Angular Frontend Standards — Copilot

## Component Rules (MANDATORY)
- `standalone: true` — ALWAYS, no NgModules
- `ChangeDetectionStrategy.OnPush` — ALWAYS
- `inject()` function — NEVER constructor injection
- `signal()` for all reactive state — NOT BehaviorSubject
- `input()` / `output()` for component I/O (Angular 20 signal-based API)
- `takeUntilDestroyed()` for subscription cleanup
- `loadComponent` for lazy-loading in routes

## Import Rules
| Need | Import From |
|------|-------------|
| API calls, DTOs, utilities | `@libs/shared` |
| Auth guards, token management | `@libs/auth` |
| Permission guards, directives | `@libs/permissions` |
| Toast, Confirm dialog, UI widgets | `@libs/ui` |
| Theme service, CSS variables | `@libs/theme` |
| Dynamic forms | `@libs/forms-dynamic` |

**NEVER**: Import from relative paths to library internals. ALWAYS use `@libs/*` aliases.

## UI Conventions
- **Icons**: FontAwesome (`fa-plus`, `fa-download`, `fa-upload`, `fa-sync`) — NEVER Bootstrap icons (`bi-*`)
- **Styling**: Bootstrap utility classes + SCSS with BEM naming
- **Toasts**: `ToastService` for success/error messages
- **Confirm**: `ConfirmDialogComponent` for destructive actions
- **Permission**: `HasPermissionDirective` (`*hasPermission="'Entity.Action'"`) on UI elements
- **Loading**: Show spinner/indicator during API calls (`loading` signal)
- **Pagination**: All data tables must be paginated
- **CSV**: Export/import via `CsvTransferService` for data tables
- **Refresh**: Always include a manual refresh button (`fa-sync`)
- **Debounce**: Debounce search inputs to prevent excessive API calls
- **i18n**: Use `TranslatePipe` for all user-facing text

## Routing Pattern
```typescript
{
    path: '{feature}',
    canActivate: [authGuard, tenantGuard, permissionGuard('Entity.View'), featureGuard('Feature.Enabled')],
    loadComponent: () => import('./pages/tenant/{feature}/{feature}.component').then(m => m.{Feature}Component),
}
```

## SCSS Rules
- Use CSS variables: `var(--vs-primary)`, `var(--vs-bg-card)`
- BEM naming: `.block__element--modifier`
- Prefer `rem` over `px`
- Component styles are View Encapsulated

## Service Pattern
```typescript
@Injectable({ providedIn: 'root' })
export class {Name}Service {
    private readonly http = inject(HttpClient);
}
```
