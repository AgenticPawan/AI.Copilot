---
name: frontend-engineer
description: "Frontend architecture reasoning and execution skills for {{projectName}}. Use when: building Angular {{angularVersion}} standalone components, managing signal-based state, creating CRUD pages, configuring routes with guards, integrating APIs via ApiService, or troubleshooting change detection."
---

# Frontend Engineer Skills

Defines the reasoning and execution behavior for the `@frontend-engineer` agent.

---

## Skill: Component Design

When creating a new component, follow this reasoning chain:

### 1. Identify Component Type
- **Page component** → Lives in `projects/portal/src/app/pages/{scope}/{feature}/`
- **Shared UI widget** → Lives in `@libs/ui`
- **Feature-specific child** → Lives alongside parent page component

### 2. Determine Data Flow
- What data does this component need? → Define signals
- Where does data come from? → ApiService, parent input, route params
- What events does this emit? → Define outputs
- Does it mutate server state? → Plan API calls with loading/error handling

### 3. Configure Reactivity
- `signal()` for local mutable state (items, loading, currentPage)
- `computed()` for derived values (filteredItems, totalPages)
- `effect()` for side effects (auto-save, analytics tracking)
- `input()` / `output()` for parent-child communication (Angular {{angularVersion}} signal-based API)
- `takeUntilDestroyed()` for RxJS subscription cleanup

### 4. Apply Guards & Permissions
- Route-level: `authGuard`, `tenantGuard`/`hostGuard`, `permissionGuard()`, `featureGuard()`
- Template-level: `*hasPermission="'Entity.Action'"` directive on action buttons
- Feature flags: `featureGuard('Feature.Enabled')` for tenant-only features

### Output Must Include
For each page component, generate:
1. TypeScript interfaces (matching backend DTOs, camelCase)
2. ApiService methods (CRUD + paged queries)
3. Component class (standalone, OnPush, inject(), signals)
4. HTML template (table, search, pagination, modals, permission gates)
5. SCSS styles (BEM naming, CSS variables, Bootstrap utilities)
6. Route registration (lazy-loaded with full guard stack)
7. Sidebar navigation entry

---

## Skill: State Management

### Signal Patterns
```typescript
// ✅ Mutable state
items = signal<ItemDto[]>([]);
loading = signal(false);

// ✅ Derived state
filteredItems = computed(() =>
  this.items().filter(i => i.name.includes(this.searchTerm()))
);

// ✅ Side effects
constructor() {
  effect(() => {
    const term = this.searchTerm();
    if (term.length >= 3) this.loadItems();
  });
}
```

### Anti-Patterns to Reject
- ❌ `BehaviorSubject` for component state → Use `signal()`
- ❌ Constructor injection → Use `inject()`
- ❌ `ChangeDetectionStrategy.Default` → Always `OnPush`
- ❌ NgModules for new components → Always `standalone: true`
- ❌ Relative imports to library internals → Use `@libs/*` aliases
- ❌ Bootstrap icons (`bi-*`) → Use FontAwesome (`fa-*`)
- ❌ Missing `takeUntilDestroyed()` on subscriptions

---

## Skill: UI/UX Enforcement

### Every Data Table Must Have
- [ ] Search/filter input with debounce
- [ ] Sortable column headers
- [ ] Pagination controls with page size selector
- [ ] Loading spinner during API calls
- [ ] Empty state message
- [ ] Refresh button (`fa-sync` icon)
- [ ] CSV export button (`fa-download` icon)
- [ ] CSV import button (`fa-upload` icon)
- [ ] Create button gated by `*hasPermission` (`fa-plus` icon)
- [ ] Edit/Delete per-row actions gated by permissions

### Every Form Must Have
- [ ] Reactive validation with error messages
- [ ] Submit button with loading state
- [ ] Cancel button
- [ ] Confirm dialog for destructive actions (via `ConfirmDialogComponent`)
- [ ] Toast notification on success/failure (via `ToastService`)

---

## Skill: API Integration

### TypeScript ↔ .NET Mapping
| .NET (PascalCase JSON) | TypeScript (camelCase) |
|-------------------------|----------------------|
| `public string Title` | `title: string` |
| `public Guid Id` | `id: string` |
| `public DateTime CreatedAt` | `createdAt: string` |
| `public bool IsActive` | `isActive: boolean` |
| `public decimal Amount` | `amount: number` |
| `public Guid? TenantId` | `tenantId: string \| null` |

### API Service Pattern
```typescript
get{Entities}(params: PagedQuery): Observable<PagedResult<{Entity}Dto>> {
  return this.http.get<PagedResult<{Entity}Dto>>('/api/{entities}', { params: toHttpParams(params) });
}
```

---

## Skill: Routing & Navigation

### Guard Stack (ordered)
```typescript
canActivate: [
  maintenanceGuard,                    // 1. System availability
  authGuard,                           // 2. JWT validity
  tenantGuard,                         // 3. Scope enforcement (OR hostGuard)
  permissionGuard('Entity.View'),      // 4. Permission check
  featureGuard('Feature.Enabled'),     // 5. Feature flag (tenant-only)
]
```

### Lazy Loading
```typescript
loadComponent: () => import('./pages/tenant/{feature}/{feature}.component')
  .then(m => m.{Feature}Component)
```

---

## Skill: SCSS & Theming

### Rules
- BEM naming: `.feature-list__header--active`
- CSS variables: `var(--vs-primary)`, `var(--vs-bg-card)`, `var(--vs-text-muted)`
- Bootstrap utilities for layout: `d-flex`, `justify-content-between`, `gap-2`, `mb-3`
- `rem` units preferred over `px`
- Component styles are View Encapsulated (default)
- Do NOT use `::ng-deep` unless absolutely necessary
