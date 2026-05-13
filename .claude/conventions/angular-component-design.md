# angular-component-design.md

## Rule

Every Angular component in ThingsBooksy is a **standalone component**. Components use `inject()` for dependency injection, `signal()` for local reactive state, and the Angular 21 built-in control flow (`@if`, `@for`, `@switch`). No NgModules, no constructor-based DI, no `CommonModule`.

---

## Component skeleton

```typescript
import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'tb-resource-list',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './resource-list.component.html',
  styleUrl: './resource-list.component.scss',
})
export class ResourceListComponent {
  private readonly resourceService = inject(ResourceService);

  readonly items = signal<Resource[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
}
```

---

## Selector prefix

All component selectors use the `tb-` prefix. The selector is derived from the component's kebab-case file name:

| File name | Selector |
|---|---|
| `resource-list.component.ts` | `tb-resource-list` |
| `booking-card.component.ts` | `tb-booking-card` |
| `user-avatar.component.ts` | `tb-user-avatar` |

Never use the default `app-` prefix.

---

## Dependency injection — `inject()`

Use `inject()` at the field declaration site. Never use constructor parameters for DI.

```typescript
// CORRECT
export class ResourceListComponent {
  private readonly resourceService = inject(ResourceService);
  private readonly router = inject(Router);
}

// WRONG — constructor-based DI
export class ResourceListComponent {
  constructor(
    private readonly resourceService: ResourceService,
    private readonly router: Router,
  ) {}
}
```

Rules:
- Mark injected dependencies `private readonly`.
- Place all injections at the top of the class body, before signals and computed properties.
- `inject()` can only be called in an injection context (field initializer, constructor, or a function called from one). Never call it inside lifecycle hooks or event handlers.

---

## Local state — `signal()`

Use `signal()` for all local component state. Use `computed()` for derived values. Use `effect()` sparingly — only when a genuine side effect must react to signal changes.

```typescript
// CORRECT
readonly items = signal<Resource[]>([]);
readonly isLoading = signal(false);
readonly selectedId = signal<string | null>(null);

// Derived state
readonly hasItems = computed(() => this.items().length > 0);
readonly selectedItem = computed(() =>
  this.items().find(i => i.id === this.selectedId())
);
```

Rules:
- Always provide an explicit generic type for `signal<T>()` when the initial value is `null`, `[]`, or `{}`.
- Update signals using `.set()` for replacement or `.update()` for transforms:
  ```typescript
  this.items.set(response);
  this.isLoading.update(v => !v);
  ```
- Never mutate signal values in place (e.g., `this.items().push(x)` is forbidden — use `.update()` with a new array).

---

## Template — built-in control flow

Use Angular 21 built-in control flow. Do not import `NgIf`, `NgFor`, `AsyncPipe`, or `CommonModule`.

```html
<!-- CORRECT -->
@if (isLoading()) {
  <tb-spinner />
} @else if (error()) {
  <p class="error">{{ error() }}</p>
} @else {
  @for (item of items(); track item.id) {
    <tb-resource-card [resource]="item" />
  } @empty {
    <p>No resources found.</p>
  }
}
```

```html
<!-- WRONG — legacy structural directives -->
<div *ngIf="isLoading">...</div>
<li *ngFor="let item of items">...</li>
```

Rules:
- Always provide `track` in `@for` — use the entity's `id` field.
- Include `@empty` when an empty list has a meaningful UI state.
- Prefer `@switch` over nested `@if`/`@else if` chains with more than three branches.

---

## Smart vs. dumb components

| Type | Where | Responsibilities |
|---|---|---|
| Smart (container) | `features/{feature}/{view}/` | fetches data, holds signals, wires services |
| Dumb (presentational) | `shared/components/` | accepts `input()`, emits `output()` (signal-based), no HTTP, no router |

Rules:
- Smart components own signals and call services. They pass data down to dumb components via inputs.
- Dumb components never inject `HttpClient`, services with HTTP calls, or `Router` directly.
- Use `input()` (signal-based inputs) for dumb components in new code:
  ```typescript
  readonly resource = input.required<Resource>();
  readonly onDelete = output<string>();
  ```

---

## Lifecycle hooks

Use functional lifecycle hooks via `DestroyRef` where cleanup is needed.

```typescript
export class ResourceListComponent {
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    interval(5000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.refresh());
  }
}
```

Rules:
- Prefer `takeUntilDestroyed(this.destroyRef)` over manual `Subject` + `takeUntil` patterns.
- Keep `ngOnInit` thin — delegate to private methods.
- Do not store subscriptions in fields when `takeUntilDestroyed` is used.

---

## File structure per component

Every component consists of exactly four files:

```
resource-list/
├── resource-list.component.ts
├── resource-list.component.html
├── resource-list.component.scss
└── resource-list.component.spec.ts
```

Rules:
- Always use `templateUrl` and `styleUrl` (external files) — never inline `template` or `styles` in the decorator.
- The `.spec.ts` file is mandatory. Create it alongside the component, even if initially empty.
- The folder name matches the primary component name without the `.component.ts` suffix.

---

## Imports array

The `imports` array in `@Component` must list only what the template actually uses. Do not import speculatively.

```typescript
// CORRECT — only what the template uses
@Component({
  standalone: true,
  imports: [RouterModule, ResourceCardComponent],
  // ...
})

// WRONG — importing unused or redundant modules
@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  // CommonModule is unnecessary in Angular 21 — control flow is built in
})
```

---

## Zakazy — zestawienie

| Zakaz | Dlaczego |
|---|---|
| `constructor(private svc: SomeService)` | Użyj `inject()` |
| `*ngIf`, `*ngFor`, `NgIf`, `NgFor` | Zastąpione wbudowanym control flow |
| `CommonModule` w `imports` | Zbędny w Angular 21 — `@if`/`@for` są wbudowane |
| `NgModule` | Projekt jest w pełni standalone |
| Bezpośrednia mutacja tablicy sygnału | Użyj `.update(arr => [...arr, item])` |
| `selector: 'app-*'` | Używaj prefiksu `tb-` |
| Inline `template:` lub `styles:` | Zawsze zewnętrzne pliki `.html` i `.scss` |

---

## Rationale

`inject()` over constructor DI removes constructor boilerplate and allows injection in any class that participates in Angular's DI tree (guards, interceptors, stores), not just components. It also makes it easier to tree-shake unused services.

Signals (`signal()`, `computed()`, `effect()`) are Angular 21's primary reactive primitive. They integrate with the new built-in control flow and enable fine-grained change detection without Zone.js overhead.

The `tb-` prefix prevents selector collisions with third-party libraries and makes it immediately clear that an element belongs to the ThingsBooksy design system. The `app-` default prefix provides no such guarantee in a production codebase.

Standalone components eliminate the NgModule layer entirely, reducing indirection and making each component's dependencies explicit in its own `imports` array.
