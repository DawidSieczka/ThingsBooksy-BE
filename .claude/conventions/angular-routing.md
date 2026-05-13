# angular-routing.md

## Rule

Every feature in ThingsBooksy defines its routes in a single `{feature}.routes.ts` file inside `frontend/src/app/features/{feature}/`. Routes are always lazy-loaded — no feature component is eagerly imported in `app.routes.ts`. The route constant is exported as `{feature}Routes` (camelCase).

---

## Folder placement

```
frontend/src/app/
├── app.routes.ts              ← root routes, delegates to feature routes via loadChildren
└── features/
    ├── resources/
    │   ├── resources.routes.ts
    │   ├── list/
    │   └── detail/
    ├── bookings/
    │   ├── bookings.routes.ts
    │   ├── list/
    │   └── create/
    └── management-groups/
        ├── management-groups.routes.ts
        ├── list/
        └── detail/
```

The `{feature}.routes.ts` file lives at the root of the feature folder, not inside a subfolder.

---

## `{feature}.routes.ts` template

```typescript
// features/resources/resources.routes.ts
import { Routes } from '@angular/router';

export const resourcesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list/resource-list.component').then(m => m.ResourceListComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./detail/resource-detail.component').then(m => m.ResourceDetailComponent),
  },
];
```

Rules:
- Always import `Routes` from `@angular/router`. Do not import `RouterModule`.
- Export exactly one constant per file. Its name is `{feature}Routes` (see naming rules below).
- Use `loadComponent` for every leaf route — never eager `component:` imports.
- The empty-string path (`path: ''`) maps to the feature's index/list view.

---

## `app.routes.ts` integration

Root routes delegate to each feature via `loadChildren`:

```typescript
// app.routes.ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'resources',
    loadChildren: () =>
      import('./features/resources/resources.routes').then(m => m.resourcesRoutes),
  },
  {
    path: 'bookings',
    loadChildren: () =>
      import('./features/bookings/bookings.routes').then(m => m.bookingsRoutes),
  },
  {
    path: 'management-groups',
    loadChildren: () =>
      import('./features/management-groups/management-groups.routes').then(
        m => m.managementGroupsRoutes,
      ),
  },
  {
    path: '',
    redirectTo: 'resources',
    pathMatch: 'full',
  },
];
```

Rules:
- Each feature gets one entry in `app.routes.ts` using `loadChildren`.
- The URL path segment in `app.routes.ts` uses kebab-case (matches the feature folder name).
- `app.routes.ts` must never import a component class directly. All components go through `loadComponent` inside feature route files.
- `redirectTo` and `**` (wildcard) routes are always the last entries in `app.routes.ts`.

---

## Const naming convention

The exported constant is always camelCase, derived from the feature folder name.

| Feature folder | Export constant |
|---|---|
| `resources` | `resourcesRoutes` |
| `bookings` | `bookingsRoutes` |
| `users` | `usersRoutes` |
| `management-groups` | `managementGroupsRoutes` |
| `resource-categories` | `resourceCategoriesRoutes` |

Conversion algorithm for kebab-case feature names:
1. Split by `-`.
2. Capitalise the first letter of every segment except the first.
3. Join all segments.
4. Append `Routes`.

Example: `management-groups` → `['management', 'groups']` → `'management'` + `'Groups'` → `managementGroups` → `managementGroupsRoutes`.

Never use SCREAMING_SNAKE_CASE or PascalCase for the constant name.

---

## Nested routes (children)

Use `children` when a feature needs a shared layout wrapper or secondary navigation within the same URL prefix.

```typescript
// features/bookings/bookings.routes.ts
import { Routes } from '@angular/router';

export const bookingsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./bookings-shell.component').then(m => m.BookingsShellComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./list/booking-list.component').then(m => m.BookingListComponent),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./detail/booking-detail.component').then(m => m.BookingDetailComponent),
      },
      {
        path: ':id/edit',
        loadComponent: () =>
          import('./edit/booking-edit.component').then(m => m.BookingEditComponent),
      },
    ],
  },
];
```

Rules:
- Maximum nesting depth is **2** (root array + one `children` array). If deeper nesting is needed, stop and raise it as an architectural question before proceeding.
- A `children` array requires a shell/layout component at the parent level that includes `<router-outlet />`.
- Never place a `children` array inside another `children` array.

---

## Guards

Use Angular 21 functional guards. Guards live in `core/guards/` and are imported by name.

```typescript
// core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated() || router.createUrlTree(['/login']);
};
```

Applying a guard to a feature:

```typescript
// features/resources/resources.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const resourcesRoutes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./list/resource-list.component').then(m => m.ResourceListComponent),
  },
];
```

Rules:
- Always use `CanActivateFn` / `CanMatchFn` (functional) — never class-based guards that `implements CanActivate`.
- Import guards from `../../core/guards/` — never define guards inline inside a routes file.
- Apply guards at the feature root in `{feature}.routes.ts` if the entire feature requires protection, rather than repeating the guard on every child route.
- `CanMatchFn` guards belong in `app.routes.ts` on the feature entry, not inside feature route files.

---

## Resolvers

Resolvers are **out of scope** for the `fe-route-writer` agent. If a routing plan specifies resolvers, the agent will note them as a manual step in its COMPLETE block. Resolvers are implemented separately following Angular 21 functional resolver conventions (`ResolveFn`).

---

## Zakazy — zestawienie

| Zakaz | Powód |
|---|---|
| `RouterModule.forChild([...])` | Legacy NgModule pattern; standalone routing uses `Routes` array directly |
| Eager `component:` import in `app.routes.ts` | All features must be lazy-loaded |
| SCREAMING_SNAKE_CASE const names (e.g. `RESOURCES_ROUTES`) | Convention mandates camelCase `{feature}Routes` |
| PascalCase const names (e.g. `ResourcesRoutes`) | Convention mandates camelCase `{feature}Routes` |
| `children` depth greater than 2 | Exceeds permitted nesting — raise as architectural issue |
| Defining guards inline in a routes file | Guards live in `core/guards/` only |
| `provideRouter()` inside feature route files | Called once in `app.config.ts` via the root `routes` array |
| Importing `CommonModule` or `RouterModule` in standalone components for routing | Use `RouterLink`, `RouterOutlet` as standalone imports; never the full module |
