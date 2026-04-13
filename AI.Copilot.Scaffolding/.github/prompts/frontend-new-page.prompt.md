---
agent: agent
description: "Create a new Angular page with full CRUD operations. Use when: building a new feature page with data table, search, pagination, create/edit/delete modals, CSV export/import, permission-gated UI, and API integration."
---

# Frontend Feature: New Page with CRUD

Create a new Angular page in the {{projectName}} portal with full CRUD operations.

## Input Required
- **Feature Name**: (e.g., `invoices`)
- **Entity DTO**: (e.g., `InvoiceDto` with fields: id, title, amount, dueDate, isPaid)
- **Scope**: `host-only` | `tenant-only` | `shared`
- **Required Permission**: (e.g., `Invoices.View`, `Invoices.Create`)
- **Feature Flag** (tenant-only): (e.g., `Invoices.Enabled`)

## Step-by-Step Execution

### Step 1: DTO & API Service Methods
Add to `src/frontend/projects/libs/shared/src/lib/`:

**models/ or existing dto file** - Add TypeScript interfaces:
```typescript
export interface {Entity}Dto {
    id: string;
    // mirror backend DTO properties (camelCase)
    createdAt: string;
}
export interface Create{Entity}Request { /* writable fields only */ }
export interface Update{Entity}Request { /* mutable fields + id */ }
```

**api.service.ts** - Add API methods:
```typescript
// GET with paging
get{Entities}(params: PagedQuery): Observable<PagedResult<{Entity}Dto>> { }
// GET by id
get{Entity}(id: string): Observable<{Entity}Dto> { }
// POST
create{Entity}(request: Create{Entity}Request): Observable<{Entity}Dto> { }
// PUT
update{Entity}(id: string, request: Update{Entity}Request): Observable<{Entity}Dto> { }
// DELETE
delete{Entity}(id: string): Observable<void> { }
```

Export from `@libs/shared` public-api.ts.

### Step 2: List Component
Create `src/frontend/projects/portal/src/app/pages/{scope}/{feature}/{feature}.component.ts`:
```typescript
@Component({
    selector: 'app-{feature}',
    standalone: true,
    imports: [FormsModule, DatePipe, RouterLink, ConfirmDialogComponent, HasPermissionDirective],
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './{feature}.component.html',
    styleUrl: './{feature}.component.scss',
})
export class {Feature}Component implements OnInit {
    private readonly api = inject(ApiService);
    private readonly toast = inject(ToastService);

    items = signal<{Entity}Dto[]>([]);
    totalCount = signal(0);
    currentPage = signal(1);
    pageSize = 10;
    searchFilter = '';
    loading = signal(false);

    // Modal state
    showCreateModal = signal(false);
    showEditModal = signal(false);
    showDeleteConfirm = signal(false);
    selectedItem = signal<{Entity}Dto | null>(null);
}
```

### Step 3: Create/Edit Form Component (if complex)
For simple forms, use modal in list component.
For complex forms, create `{feature}-form.component.ts`:
```typescript
@Component({
    selector: 'app-{feature}-form',
    standalone: true,
    imports: [ReactiveFormsModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class {Feature}FormComponent {
    // Use reactive forms for complex validation
}
```

### Step 4: Template
Create `.component.html` with:
- Search/filter bar
- Data table with sortable columns
- Pagination controls
- Action buttons gated by `*hasPermission="'{Entity}.Create'"` directive
- Edit/Delete buttons per row
- Modal dialogs for create/edit

### Step 5: Routing
Add to `src/frontend/projects/portal/src/app/app.routes.ts`:
```typescript
// Inside the shell children array:
{
    path: '{feature}',
    canActivate: [{scope}Guard],  // hostGuard or tenantGuard
    loadComponent: () => import('./pages/{scope}/{feature}/{feature}.component')
        .then(m => m.{Feature}Component)
},
{
    path: '{feature}/create',
    canActivate: [{scope}Guard, permissionGuard('{Entity}.Create')],
    loadComponent: () => import('./pages/{scope}/{feature}/{feature}-form.component')
        .then(m => m.{Feature}FormComponent)
},
```

For tenant-only features, add `featureGuard('{Feature}.Enabled')`.

### Step 6: Navigation
Add menu item to the shell/sidebar component with permission check.

## Output Checklist
- [ ] Component is standalone with OnPush
- [ ] Uses inject() function (not constructor injection)
- [ ] Uses signals for all reactive state
- [ ] Imports from @libs/* path aliases
- [ ] Route has appropriate guards (auth + scope + permission + feature)
- [ ] UI elements gated by HasPermissionDirective
- [ ] Loading states handled
- [ ] Error handling with ToastService
- [ ] Lazy-loaded via loadComponent
