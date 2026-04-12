# Frontend Feature Development Prompt

**When to use**: Adding new Angular components, services, pages, or features to the portal.

---

## 📋 Pre-Development Checklist

Before writing code, clarify:

1. **Scope**: Is this Host portal (admin.*) or Tenant portal (*.*)?
2. **Permissions**: What permission check is needed for display/access?
3. **Data Source**: Which backend API endpoint(s) will this consume?
4. **Routing**: What URL route and nested routes are needed?
5. **State**: How should data be loaded and cached?
6. **Real-time**: Does this need SignalR notifications?

---

## 🎯 Development Workflow

### Step 1: Create the Service

**File**: `src/frontend/projects/libs/shared/src/lib/{feature}.service.ts`

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentDto {
    id: string;
    title: string;
    content: string;
    tenantId: string;
    createdAt: string;
}

export interface CreateDocumentRequest {
    title: string;
    content: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
    private readonly http = inject(HttpClient);

    getAll(): Observable<DocumentDto[]> {
        return this.http.get<DocumentDto[]>('/api/documents');
    }

    getById(id: string): Observable<DocumentDto> {
        return this.http.get<DocumentDto>(`/api/documents/${id}`);
    }

    create(request: CreateDocumentRequest): Observable<DocumentDto> {
        return this.http.post<DocumentDto>('/api/documents', request);
    }

    update(id: string, request: CreateDocumentRequest): Observable<DocumentDto> {
        return this.http.put<DocumentDto>(`/api/documents/${id}`, request);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`/api/documents/${id}`);
    }
}
```

### Step 2: Create the Component

**File**: `src/frontend/projects/portal/src/app/pages/{feature}/{feature}.component.ts`

```typescript
import { Component, ChangeDetectionStrategy, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DocumentService, DocumentDto } from '@libs/shared';
import { PermissionDirective } from '@libs/permissions';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-documents',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, PermissionDirective],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="container">
            <h1>Documents</h1>

            @if (isLoading()) {
                <p>Loading...</p>
            } @else {
                <button (click)="openCreateDialog()" [appPermission]="'Documents.Manage'">
                    + New Document
                </button>

                @if ((documents() ?? []).length === 0) {
                    <p>No documents found</p>
                } @else {
                    <table>
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Created</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            @for (doc of documents(); track doc.id) {
                                <tr>
                                    <td>{{ doc.title }}</td>
                                    <td>{{ doc.createdAt | date }}</td>
                                    <td>
                                        <button (click)="onEdit(doc)">Edit</button>
                                        <button (click)="onDelete(doc.id)" [appPermission]="'Documents.Manage'">Delete</button>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                }
            }
        </div>
    `,
    styles: [`
        :host { display: block; padding: 20px; }
        .container { max-width: 1200px; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
        button { padding: 8px 12px; margin-right: 8px; }
    `]
})
export class DocumentsComponent implements OnInit, OnDestroy {
    private readonly documentService = inject(DocumentService);
    private readonly destroy$ = new Subject<void>();

    documents = signal<DocumentDto[]>([]);
    isLoading = signal(false);

    ngOnInit() {
        this.loadDocuments();
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadDocuments() {
        this.isLoading.set(true);
        this.documentService.getAll()
            .pipe(
                finalize(() => this.isLoading.set(false)),
                takeUntil(this.destroy$)
            )
            .subscribe({
                next: docs => this.documents.set(docs),
                error: err => console.error('Error loading documents:', err)
            });
    }

    openCreateDialog() {
        // TODO: Implement modal/dialog for create
    }

    onEdit(document: DocumentDto) {
        // TODO: Navigate to edit page or open dialog
    }

    onDelete(id: string) {
        if (!confirm('Are you sure?')) return;

        this.documentService.delete(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => this.loadDocuments(),
                error: err => console.error('Error deleting:', err)
            });
    }
}
```

### Step 3: Create Edit/Detail Component (if needed)

**File**: `src/frontend/projects/portal/src/app/pages/{feature}/{feature}-edit.component.ts`

```typescript
@Component({
    selector: 'app-document-edit',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="form-container">
            <h2>{{ isNew ? 'New Document' : 'Edit Document' }}</h2>

            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <div class="form-group">
                    <label>Title</label>
                    <input type="text" formControlName="title" />
                </div>

                <div class="form-group">
                    <label>Content</label>
                    <textarea formControlName="content"></textarea>
                </div>

                <button type="submit" [disabled]="form.invalid || isSubmitting()">
                    {{ isSubmitting() ? 'Saving...' : 'Save' }}
                </button>
                <button type="button" (click)="onCancel()">Cancel</button>
            </form>
        </div>
    `,
    styles: [`
        :host { display: block; }
        .form-container { max-width: 600px; margin: 20px auto; }
        .form-group { margin-bottom: 20px; display: flex; flex-direction: column; }
        label { font-weight: bold; margin-bottom: 5px; }
        input, textarea { padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
        button { padding: 10px 16px; margin-right: 8px; cursor: pointer; }
    `]
})
export class DocumentEditComponent implements OnInit {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly documentService = inject(DocumentService);
    private readonly fb = inject(FormBuilder);
    private readonly destroy$ = new Subject<void>();

    isNew = signal(true);
    isSubmitting = signal(false);
    documentId?: string;

    form = this.fb.group({
        title: ['', [Validators.required, Validators.maxLength(200)]],
        content: ['', Validators.required]
    });

    ngOnInit() {
        this.route.params
            .pipe(takeUntil(this.destroy$))
            .subscribe(params => {
                if (params['id']) {
                    this.documentId = params['id'];
                    this.isNew.set(false);
                    this.loadDocument(params['id']);
                }
            });
    }

    private loadDocument(id: string) {
        this.documentService.getById(id)
            .pipe(takeUntil(this.destroy$))
            .subscribe(doc => {
                this.form.patchValue({
                    title: doc.title,
                    content: doc.content
                });
            });
    }

    onSubmit() {
        if (this.form.invalid) return;

        this.isSubmitting.set(true);

        const operation = this.isNew()
            ? this.documentService.create(this.form.value)
            : this.documentService.update(this.documentId!, this.form.value);

        operation
            .pipe(
                finalize(() => this.isSubmitting.set(false)),
                takeUntil(this.destroy$)
            )
            .subscribe({
                next: () => this.router.navigate(['/documents']),
                error: err => console.error('Error saving:', err)
            });
    }

    onCancel() {
        this.router.navigate(['/documents']);
    }
}
```

### Step 4: Set Up Routing

**File**: `src/frontend/projects/portal/src/app/routes.ts`

```typescript
export const ROUTES: Routes = [
    {
        path: 'documents',
        canActivate: [AuthGuard],
        children: [
            { path: '', component: DocumentsComponent },
            {
                path: 'new',
                component: DocumentEditComponent,
                canActivate: [PermissionGuard],
                data: { permission: 'Documents.Manage' }
            },
            {
                path: ':id',
                component: DocumentEditComponent,
                canActivate: [PermissionGuard],
                data: { permission: 'Documents.View' }
            }
        ]
    }
];
```

### Step 5: Add to Navigation (if needed)

**File**: `src/frontend/projects/portal/src/app/layout/sidebar.component.ts`

```typescript
export const MENU_ITEMS = [
    {
        label: 'Documents',
        route: '/documents',
        permission: 'Documents.View'  // Only show if user has permission
    },
    // ... other menu items
];
```

### Step 6: Export from Shared Library (if service in shared lib)

**File**: `src/frontend/projects/libs/shared/src/index.ts`

```typescript
export * from './lib/document.service';
export * from './lib/models/document.model';
```

### Step 7: Create Unit Tests

**File**: `src/frontend/projects/portal/src/app/pages/documents/documents.component.spec.ts`

```typescript
describe('DocumentsComponent', () => {
    let component: DocumentsComponent;
    let fixture: ComponentFixture<DocumentsComponent>;
    let documentService: jasmine.SpyObj<DocumentService>;

    beforeEach(async () => {
        const spy = jasmine.createSpyObj('DocumentService', ['getAll', 'delete']);

        await TestBed.configureTestingModule({
            imports: [DocumentsComponent],
            providers: [{ provide: DocumentService, useValue: spy }]
        }).compileComponents();

        documentService = TestBed.inject(DocumentService) as jasmine.SpyObj<DocumentService>;
        fixture = TestBed.createComponent(DocumentsComponent);
        component = fixture.componentInstance;
    });

    it('should load documents on init', fakeAsync(() => {
        const mockDocs: DocumentDto[] = [
            { id: '1', title: 'Test', content: 'Content', tenantId: '1', createdAt: '' }
        ];
        documentService.getAll.and.returnValue(of(mockDocs));

        fixture.detectChanges();
        tick();

        expect(documentService.getAll).toHaveBeenCalled();
        expect(component.documents()).toEqual(mockDocs);
    }));

    it('should delete document when confirmed', fakeAsync(() => {
        spyOn(window, 'confirm').and.returnValue(true);
        documentService.delete.and.returnValue(of(void 0));

        component.onDelete('1');
        tick();

        expect(documentService.delete).toHaveBeenCalledWith('1');
    }));
});
```

---

## ✅ Checklist

- [ ] Service created in @libs/shared with API endpoints
- [ ] List component created with signals and OnPush
- [ ] Detail/Edit component created with reactive forms
- [ ] Routes defined with permission guards
- [ ] Navigation added to sidebar (if applicable)
- [ ] Services exported from shared library
- [ ] Unit tests written for service and components
- [ ] Permission checks added to UI elements
- [ ] Error handling with user-friendly messages
- [ ] Tested in both Host and Tenant portals (if applicable)

---

## 🎯 Best Practices

1. **Always use OnPush** change detection for performance
2. **Use signals** for component state (not RxJS subjects for state)
3. **Use takeUntil(destroy$)** to automatically unsubscribe
4. **Import from @libs/** using path aliases
5. **Check permissions** before showing/enabling features
6. **Handle errors** gracefully with console/toast messages
7. **Test async operations** with fakeAsync/tick
8. **Use track function** in @for loops for performance
9. **Lazy load routes** if feature is large
10. **Mock services** in tests with jasmine.createSpyObj

---

**Tips**:
- Use `npm start` to watch for changes during development
- Use `ng generate component` to auto-create with placeholders
- Use RxJS `tap()`/`finalize()` for side effects
- Remember TenantId in filters is automatic from backend
- Consider using lazy loading for large feature modules
