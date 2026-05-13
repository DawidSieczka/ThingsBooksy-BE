# angular-http-pattern.md

## Rule

Feature services are the single HTTP boundary between Angular components and the backend. Every feature service wraps generated `api/` clients, always returns `Observable<T>`, and never leaks raw DTOs or HTTP internals to components. Interceptors handle infrastructure-level errors only — domain errors are handled by feature services or components.

---

## HttpClient configuration

`provideHttpClient()` must always include `withFetch()`. Register it inside `provideCore()` in `core/index.ts`, never directly in `app.config.ts`.

```typescript
// core/index.ts
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { EnvironmentProviders } from '@angular/core';
import { authInterceptor } from './interceptors/auth.interceptor';
import { errorInterceptor } from './interceptors/error.interceptor';

export function provideCore(): EnvironmentProviders[] {
  return [
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, errorInterceptor]),
    ),
  ];
}
```

```typescript
// app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideCore } from './core';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    ...provideCore(),
  ],
};
```

Rules:
- `withFetch()` is mandatory in every `provideHttpClient()` call. Omitting it is a convention violation.
- All interceptors are registered through `provideCore()` — never added individually in `app.config.ts`.
- Do not call `provideHttpClient()` outside `provideCore()`.

---

## Feature service — location and naming

The primary feature service lives directly in `features/{feature}/`, not in a subfolder.

```
features/resources/
├── resources.routes.ts
├── resources.service.ts       ← primary feature service, lives here
├── list/
├── detail/
└── services/                  ← additional specialised services only if needed
    └── resource-search.service.ts
```

Rules:
- One primary service per feature, named `{feature}.service.ts` (e.g., `resources.service.ts`).
- Additional specialised services (e.g., a dedicated search or export service) go in `features/{feature}/services/`. Do not create `services/` preemptively — create it only when a second service is needed.
- Feature services are `providedIn: 'root'` unless they require feature-scoped lifetime (rare).

---

## Return type — always `Observable<T>`

Feature services always return `Observable<T>`. Returning a `Promise` from a service method is forbidden.

```typescript
// CORRECT — service returns Observable
getResources(): Observable<Resource[]> {
  return this.api.resourcesGet().pipe(
    map(dto => dto.items.map(mapToResource)),
  );
}

// WRONG — service converts to Promise
async getResources(): Promise<Resource[]> {
  return firstValueFrom(this.api.resourcesGet());
}
```

Rationale: the service does not know how the caller will consume the data. The component decides whether to use `toSignal()`, `firstValueFrom()`, subscribe directly, or compose with RxJS operators. Converting in the service removes that flexibility and prevents cancellation.

The component is responsible for unwrapping:

```typescript
// Component using toSignal (preferred for read data)
readonly resources = toSignal(this.resourceService.getResources(), { initialValue: [] });

// Component using firstValueFrom (for imperative flows, e.g. form submit)
async onSubmit(): Promise<void> {
  await firstValueFrom(this.resourceService.createResource(this.form.value));
}
```

---

## What a feature service may and may not do

### Allowed

- **Map DTO → view model**: translate raw API types to component-friendly shapes.
- **Compose multiple calls**: use `combineLatest`, `switchMap`, `forkJoin` to merge responses.
- **Local reactive cache with `signal()`**: store fetched data in a `signal()` when the feature needs shared reactive state across multiple components within the same feature.

```typescript
@Injectable({ providedIn: 'root' })
export class ResourcesService {
  private readonly api = inject(ResourcesApi);

  // Mapping DTO → view model
  getResources(): Observable<Resource[]> {
    return this.api.resourcesGet().pipe(
      map(dto => dto.items.map(mapToResource)),
    );
  }

  // Composing two calls
  getResourceWithOwner(id: string): Observable<ResourceWithOwner> {
    return this.api.resourcesIdGet({ id }).pipe(
      switchMap(resource =>
        this.api.usersIdGet({ id: resource.ownerId }).pipe(
          map(owner => ({ ...mapToResource(resource), owner: mapToUser(owner) })),
        ),
      ),
    );
  }

  // Local reactive cache
  private readonly _resources = signal<Resource[]>([]);
  readonly resources = this._resources.asReadonly();

  loadResources(): Observable<void> {
    return this.api.resourcesGet().pipe(
      map(dto => dto.items.map(mapToResource)),
      tap(items => this._resources.set(items)),
      map(() => void 0),
    );
  }
}
```

### Forbidden

- **Cross-feature state**: state shared between different features belongs in `core/services/`, not a feature service.
- **Side-effects unrelated to HTTP**: navigation, toast notifications, and analytics events must not originate inside a feature service.
- **Raw DTO exposure**: never return a generated `api/` type directly to a component; always map to a view model or a typed interface in `features/{feature}/models/`.

---

## Error handling — split between interceptors and feature services

### Interceptor responsibilities (infrastructure errors)

| HTTP status | Interceptor action |
|---|---|
| **401** | Calls `AuthService` to trigger logout. **Swallows** the error — the component never sees it. |
| **500, 503** | Shows a global toast notification. **Re-throws** with `throwError(() => error)` — component may show a local error state. |
| **All other 4xx** | Passes through untouched. |
| **422, 404 with domain payload** | Passes through untouched. |

```typescript
// core/interceptors/error.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, EMPTY, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.logout();
        return EMPTY; // swallow — component never sees this
      }

      if (error.status === 500 || error.status === 503) {
        notifications.showError('Wystąpił błąd serwera. Spróbuj ponownie.');
        return throwError(() => error); // re-throw — component may handle locally
      }

      return throwError(() => error); // all other errors pass through
    }),
  );
};
```

### Feature service responsibilities (domain errors)

Domain errors (422 validation failure, 404 with contextual payload) are handled in the feature service using `catchError`:

```typescript
// features/resources/resources.service.ts
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ResourcesApi } from '../../api';
import { Resource, ResourceValidationError } from './models/resource.model';
import { mapToResource } from './models/resource.mapper';

@Injectable({ providedIn: 'root' })
export class ResourcesService {
  private readonly api = inject(ResourcesApi);

  createResource(command: CreateResourceCommand): Observable<Resource> {
    return this.api.resourcesPost({ body: command }).pipe(
      map(mapToResource),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 422) {
          const validationError: ResourceValidationError = {
            type: 'validation',
            fields: error.error?.errors ?? {},
          };
          return throwError(() => validationError);
        }
        return throwError(() => error);
      }),
    );
  }
}
```

The component then handles the typed error:

```typescript
async onSubmit(): Promise<void> {
  this.isLoading.set(true);
  try {
    await firstValueFrom(this.resourceService.createResource(this.form.value));
    this.router.navigate(['/resources']);
  } catch (err) {
    if (isResourceValidationError(err)) {
      this.fieldErrors.set(err.fields);
    } else {
      this.error.set('Nie udało się zapisać zasobu.');
    }
  } finally {
    this.isLoading.set(false);
  }
}
```

### Summary — who handles what

| Error type | Handler | Action |
|---|---|---|
| 401 Unauthorized | `errorInterceptor` | logout + swallow |
| 500, 503 Server Error | `errorInterceptor` | global toast + re-throw |
| 422 Unprocessable Entity | feature service `catchError` | typed domain error |
| 404 with domain payload | feature service `catchError` | typed domain error |
| Other 4xx | component or feature service | local error state |

---

## View models and mapping

Never expose generated `api/` types to components. Map at the feature service boundary.

Place model interfaces and mapper functions in `features/{feature}/models/`:

```
features/resources/
├── models/
│   ├── resource.model.ts    ← interfaces: Resource, ResourceValidationError
│   └── resource.mapper.ts   ← pure mapping functions: mapToResource()
```

```typescript
// features/resources/models/resource.model.ts
export interface Resource {
  id: string;
  name: string;
  description: string;
  ownerId: string;
  createdAt: Date;
}

export interface ResourceValidationError {
  type: 'validation';
  fields: Record<string, string[]>;
}

export function isResourceValidationError(err: unknown): err is ResourceValidationError {
  return typeof err === 'object' && err !== null && (err as ResourceValidationError).type === 'validation';
}
```

```typescript
// features/resources/models/resource.mapper.ts
import { ResourceDto } from '../../../api';
import { Resource } from './resource.model';

export function mapToResource(dto: ResourceDto): Resource {
  return {
    id: dto.id,
    name: dto.name,
    description: dto.description ?? '',
    ownerId: dto.ownerId,
    createdAt: new Date(dto.createdAt),
  };
}
```

Rules:
- Mapper functions are pure functions — no side effects, no injected services.
- Create `models/` only when the feature has at least one view model. Do not create it preemptively.
- One file for interfaces (`{entity}.model.ts`), one for mappers (`{entity}.mapper.ts`).

---

## Auth interceptor

The auth interceptor attaches the JWT bearer token to every outgoing request:

```typescript
// core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();

  if (!token) {
    return next(req);
  }

  return next(req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  }));
};
```

Rules:
- Use functional interceptors (`HttpInterceptorFn`) — never class-based `HttpInterceptor`.
- Token is read from `AuthService` via `inject()` — never stored in the interceptor itself.
- If no token is present, forward the request unmodified.

---

## Zakazy — zestawienie

| Zakaz | Powód |
|---|---|
| `provideHttpClient()` bez `withFetch()` | `withFetch()` jest obowiązkowe |
| `provideHttpClient()` poza `provideCore()` | Tylko jedna rejestracja, przez `provideCore()` |
| Typ zwracany `Promise<T>` w serwisie | Serwis zawsze zwraca `Observable<T>` |
| `firstValueFrom()` wewnątrz serwisu | To odpowiedzialność komponentu |
| Bezpośrednie eksponowanie typów z `api/` do komponentów | Zawsze mapuj do view model |
| Stan cross-feature w feature service | Przenies do `core/services/` |
| Side-effecty niezwiązane z HTTP w serwisie | Toasty, nawigacja, analytyka — poza serwisem |
| Klasowy `HttpInterceptor` (`implements HttpInterceptor`) | Używaj `HttpInterceptorFn` (funkcyjnego) |
| Rejestracja interceptorów bezpośrednio w `app.config.ts` | Rejestruj przez `provideCore()` |
