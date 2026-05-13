# angular-folder-structure.md

## Rule

All Angular application code lives under `frontend/src/app/` organized into four top-level folders: `core/`, `features/`, `shared/`, and `api/`. A `styles/` folder under `frontend/src/` holds global SCSS. No other top-level folders are created without explicit justification.

---

## Folder layout

```
frontend/src/
├── styles/
│   ├── _tokens.scss       ← design tokens (colors, spacing, typography)
│   └── _base.scss         ← global resets and base styles
└── app/
    ├── core/              ← guards, interceptors, global singleton services
    │   ├── interceptors/
    │   ├── guards/
    │   └── services/
    ├── features/          ← one folder per domain feature / backend module
    │   ├── resources/
    │   │   ├── resources.routes.ts
    │   │   ├── list/
    │   │   └── detail/
    │   ├── bookings/
    │   └── users/
    ├── shared/            ← reusable UI building blocks with no domain logic
    │   ├── components/
    │   └── pipes/
    └── api/               ← generated HTTP client (swagger-typescript-api)
```

---

### core/

Contains application-wide infrastructure that is instantiated once for the lifetime of the app.

- `interceptors/` — functional HTTP interceptors (`withInterceptors`)
- `guards/` — route guards (`CanActivateFn`, `CanMatchFn`)
- `services/` — global singleton services (auth state, user session, notifications)

Rules:
- Nothing in `core/` depends on anything in `features/`.
- `core/` services are provided at the root level (`providedIn: 'root'`) or registered once in `app.config.ts` via a `provideCore()` function.
- Do not place feature-specific logic in `core/`.

**Recommended pattern — `provideCore()`:**

```typescript
// core/index.ts
export function provideCore(): EnvironmentProviders[] {
  return [
    provideHttpClient(withInterceptors([authInterceptor])),
    // additional core providers
  ];
}
```

```typescript
// app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    ...provideCore(),
  ],
};
```

This keeps `app.config.ts` clean and makes `core/` self-registering.

---

### features/

One folder per domain feature. Features map 1-to-1 to backend modules (e.g., `resources/` mirrors the `Resources` backend module).

Each feature folder contains:
- `{feature}.routes.ts` — lazy-loaded route definitions for the feature
- One subfolder per view/page (e.g., `list/`, `detail/`, `create/`)
- Optionally: `services/`, `models/`, `store/` if the feature needs them

Rules:
- Features must not import from other feature folders. Cross-feature communication goes through `shared/` components or `core/` services.
- Lazy-load every feature via `loadChildren` or `loadComponent` in the root router.
- Create `models/` inside a feature only when needed — do not create it preemptively.

Example feature structure:

```
features/resources/
├── resources.routes.ts
├── list/
│   ├── resource-list.component.ts
│   ├── resource-list.component.html
│   └── resource-list.component.scss
└── detail/
    ├── resource-detail.component.ts
    ├── resource-detail.component.html
    └── resource-detail.component.scss
```

---

### shared/

Reusable UI components and pure pipes that have no domain knowledge. A `shared/` component must be usable in any feature without modification.

- `components/` — presentational (dumb) components: buttons, cards, modals, form fields
- `pipes/` — pure transformation pipes

Rules:
- No HTTP calls, no router navigation, no domain state inside `shared/` components.
- `shared/` components must never import from `features/` or `core/services/`.
- Keep components in `shared/` generic — accept inputs, emit outputs.

---

### api/

Auto-generated TypeScript HTTP client produced by `swagger-typescript-api`. This folder is fully regenerated on each run — do not manually edit files here.

- Generated from: `localhost:8080/swagger/v1/swagger.json` (backend must be running)
- Generation command: `npx swagger-typescript-api -p <swagger-url> -o frontend/src/app/api/ --axios false --modular`
- Add `frontend/src/app/api/` to `.gitignore` or commit as generated code — decide once and be consistent.

Rules:
- Never write business logic inside `api/`. Wrap generated services in feature-level services if adaptation is needed.
- Regenerate after every backend contract change.

---

### styles/

Global SCSS lives in `frontend/src/styles/`, not inside `app/`.

- `_tokens.scss` — CSS custom properties for design tokens (colors, spacing, typography, breakpoints)
- `_base.scss` — CSS reset, `box-sizing`, `body` defaults

Rules:
- Component-scoped styles go in the component's `.scss` file, not here.
- Import `_tokens.scss` in `styles.scss` so tokens are available globally.
- Never put component-specific rules in `_base.scss`.

---

## File naming

All Angular files use kebab-case with a type suffix separated by a dot.

| Type | Pattern | Example |
|---|---|---|
| Component | `{name}.component.ts` | `resource-list.component.ts` |
| Component template | `{name}.component.html` | `resource-list.component.html` |
| Component styles | `{name}.component.scss` | `resource-list.component.scss` |
| Component spec | `{name}.component.spec.ts` | `resource-list.component.spec.ts` |
| Service | `{name}.service.ts` | `auth.service.ts` |
| Guard | `{name}.guard.ts` | `auth.guard.ts` |
| Interceptor | `{name}.interceptor.ts` | `auth.interceptor.ts` |
| Pipe | `{name}.pipe.ts` | `date-format.pipe.ts` |
| Routes | `{feature}.routes.ts` | `resources.routes.ts` |
| Model/interface | `{name}.model.ts` | `resource.model.ts` |

Rules:
- Use full descriptive names — `resource-list.component.ts`, not `list.component.ts`.
- The class name is the PascalCase equivalent of the file name: `resource-list.component.ts` → `ResourceListComponent`.
- Folder name mirrors the primary component inside it: the `list/` folder contains `resource-list.component.ts`.
