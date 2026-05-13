# CLAUDE.md — ThingsBooksy

Modular Monolith (.NET 10 backend + Angular 21 frontend, single deployable). This file is an **index**, not a rulebook. Authoritative rules live in the linked files.

---

## Quick navigation

- **Architecture rules** → `.specify/memory/constitution.md`
- **Backend conventions** → `.claude/conventions/*.md` (table below)
- **Frontend conventions** → `.claude/conventions/angular-*.md` (table below)
- **Agent fleet** → `.claude/agents/*.md` (table below)
- **Bootstrapper** → `backend/src/Bootstrapper/ThingsBooksy.Bootstrapper`
- **Shared contracts** → `backend/src/Shared/ThingsBooksy.Shared.Abstractions`

---

## Hard non-negotiable rules

1. **GUID v7 only** — `Guid.CreateVersion7()`. `Guid.NewGuid()` is forbidden.
2. **No direct cross-module references** — events via `IMessageBroker`, queries via `IModuleClient`. Never query another module's DB.
3. **Tests required** — new business logic without tests does not merge.

All other rules (entity encapsulation, naming, DI pattern, schema isolation, etc.) live in `.claude/conventions/` and are authoritative there.

---

## Commands

### Backend
- Build: `dotnet build backend/ThingsBooksy.slnx`
- Run: `dotnet run --project backend/src/Bootstrapper/ThingsBooksy.Bootstrapper`
- Test all: `dotnet test backend/ThingsBooksy.slnx` (target a specific `.csproj` for faster feedback)
- Format: `dotnet format backend/ThingsBooksy.slnx` — also runs automatically on staged `.cs` files via Husky.NET pre-commit hook (`.husky/task-runner.json`). Run manually only when iterating mid-session before a commit.
- EF migration add: `dotnet ef migrations add {Name} --project backend/src/Modules/{Module}/{Module}.Migrations --startup-project backend/src/Bootstrapper/ThingsBooksy.Bootstrapper`
- EF migration apply (dev only): `dotnet ef database update --project ... --startup-project ...` — integration tests apply migrations automatically via `ThingsBooksyWebAppFactory`; manual `database update` is needed only for local Docker DB.

### Frontend
- Install: `cd frontend && npm install`
- Dev server: `cd frontend && npm start` (localhost:4200)
- Build: `cd frontend && npm run build`
- Tests: `cd frontend && npm test`
- Lint: `cd frontend && npm run lint`

### Docker
- `wsl docker compose up --build` (WSL prefix required on Windows). App: `localhost:8080`, Swagger: `localhost:8080/swagger`.

---

## Tech stack

.NET 10 / ASP.NET Core 10 / C# 13 · PostgreSQL 17 · EF Core 10 · JWT Bearer + AES-256 · Serilog · Swashbuckle · Solution: `.slnx` · Angular 21 (standalone, signals, Reactive Forms, Vitest) · Docker / WSL.

---

## Backend conventions

| File | Rule (one line) |
|---|---|
| `domain-entity-design.md` | Entity encapsulation: `private set`, private ctor, `Create`/`Upsert` factory, max 4 params |
| `naming-commands-queries-handlers-results.md` | PascalCase, full names, fixed suffixes, one class per file |
| `data-provider-pattern.md` | Handlers depend on `IXxxDataProvider`, never `DbContext`; `AddDataProviders` per module |
| `data-provider-query-syntax.md` | Parenthesized LINQ query syntax for joins/group by |
| `command-construction-in-endpoints.md` | Request DTO in `.Api/Requests/`; command constructed in endpoint, never bound from body |
| `minimal-api-endpoints.md` | Minimal API only; endpoints in `Expose()`; `/{module-name}/...` route prefix |
| `dispatcher-usage.md` | `IDispatcher.SendAsync` / `QueryAsync`; no MediatR |
| `ef-schema-isolation.md` | `HasDefaultSchema(lowercase_snake_case)` mandatory; never `"public"` |
| `internals-visible-to.md` | 4 `InternalsVisibleTo` per `.Core`: `.Api`, `.Migrations`, `.IntegrationTests`, `DynamicProxyGenAssembly2` |
| `integration-test-naming.md` | `{Action}{Entity}_{Condition}_{Result}` |
| `integration-test-infrastructure.md` | TestClient + per-entity Factory + IntegrationTestCollection per module |

---

## Frontend conventions

| File | Rule (one line) |
|---|---|
| `angular-folder-structure.md` | `core/`, `features/`, `shared/`, `src/app/api/`; `provideCore()` registers interceptors; lazy-load features |
| `angular-component-design.md` | Standalone, `inject()`, `signal()`, built-in control flow, `tb-` selector prefix |
| `angular-http-pattern.md` | Feature services return `Observable<T>`; `withFetch()` mandatory; interceptors for 401/500 |
| `angular-forms-pattern.md` | Reactive Forms, typed `FormControl<T>`, `nonNullable: true`, async validators via `timer(300) + first()` |
| `angular-styling.md` | SCSS + CSS custom properties in `_tokens.scss`; no `::ng-deep`, no hard-coded values, mobile-first |
| `angular-routing.md` | One `{feature}.routes.ts` per feature; export `{feature}Routes` (camelCase); lazy-load via `loadComponent`/`loadChildren`; max depth 2 |

---

## Workflow

- **New features / substantial changes:** SpecKit — `/speckit-specify` → `/speckit-plan` → `/speckit-tasks` → `/speckit-implement`. Helpers: `/speckit-clarify`, `/speckit-analyze`, `/speckit-checklist`, `/speckit-constitution`, `/speckit-taskstoissues`.
- **Small bugfixes:** edit directly, no SpecKit, no full agent pipeline.

---

## Agent fleet

Main session is the orchestrator — it reads agent descriptions and orchestration rules below.

### Known agents

| Agent | When to delegate |
|---|---|
| `agent-architect` | Designing a new agent or growing the fleet |
| `convention-writer` | Interactive session to add or change a coding convention |
| `product-strategist` | Two-phase interview (business → technical) producing a handoff brief for `/speckit-specify` |
| `doc-writer` | After `product-strategist` brief — writes ADR; also for Stage 3 pivots |
| `plan-validator` | After `/speckit-tasks`, before `/speckit-implement` — GO/NO-GO + EXECUTION MAP |
| `contract-definer` | After plan-validator GO with cross-module deps — defines `IEvent` / `IModuleClient` contracts |
| `module-scaffolder` | Before `module-writer` when `backend/src/Modules/{Name}/` does not exist — creates the full empty scaffold (4 projects, `.slnx` registration, `ThingsBooksyWebAppFactory` patches) and reports `Build: PASSED` |
| `module-writer` | After contracts ready (and after `module-scaffolder` for new modules) — implements one module's assigned task IDs |
| `migration-agent` | After module-writer when `Schema changes != NONE` — generates EF migration |
| `quality-reviewer` | After migration-agent (or module-writer if no schema changes) — interactive code review |
| `integration-test-writer` | After quality-reviewer ends — writes integration tests for the module |
| `architecture-guard` | After all Wave modules tested — solution-wide cross-module check |
| `fe-api-client-writer` | Regenerates TypeScript HTTP client → `frontend/src/app/api/` |
| `html-extractor` | Analyzes a Claude Design `.html` artifact and produces a component plan |
| `fe-plan-validator` | After `html-extractor` Phase 6 approval, before any `fe-component-writer` — interactive read-only gate; produces `VERDICT GO/NO-GO` |
| `fe-component-writer` | After `fe-plan-validator` GO — implements one Angular component per call |
| `fe-route-writer` | After all `fe-component-writer` instances for a feature report `Build: PASSED` — creates `{feature}.routes.ts` and registers in `app.routes.ts` |

### Orchestration rules (compact)

- **doc-writer** runs immediately after `product-strategist` returns a brief. Do not start `/speckit-specify` until ADR is written.
- **plan-validator** NO-GO blocks `/speckit-implement`. On GO, pass EXECUTION MAP downstream.
- **contract-definer** runs only when EXECUTION MAP has cross-module deps. Block `module-writer` until contracts are ready.
- **module-scaffolder** runs before `module-writer` for any module whose `backend/src/Modules/{Name}/` directory does not exist. Pass only the module name (PascalCase). Block `module-writer` until `MODULE-SCAFFOLDER COMPLETE` with `Build: PASSED` is received.
- **module-writer** spawns one instance per module per Wave; parallelize independent modules.
- **migration-agent** runs only when `Schema changes != NONE`.
- **quality-reviewer** is interactive and read-only. Block `integration-test-writer` until session ends. If the final report has `BLOCKERS > 0` and zero `Challenged items`, repair loop: re-invoke `module-writer` for the affected module passing the violation description → tail pipeline (`migration-agent` if `Schema changes != NONE`) → re-invoke `quality-reviewer` with the same task IDs. Cap 2 iterations; on the 3rd failure escalate to the user. Skip the loop entirely when the user marked the BLOCKER as `Challenged (no resolution)`.
- **integration-test-writer** runs after `quality-reviewer` session ends. The EF schema is added to `SchemasToInclude` by `module-scaffolder` for new modules; verify it is present before invoking integration-test-writer (Respawn will not clean otherwise).
- **architecture-guard** runs after all Wave modules report `INTEGRATION-TEST-WRITER COMPLETE`. If `Challenged (no resolution) > 0`, repair loop: re-invoke `module-writer` with violation description → tail pipeline (`migration-agent` if schema → `quality-reviewer` → `integration-test-writer`) → re-invoke `architecture-guard`. Cap 2 iterations; on 3rd failure escalate to user.
- **fe-plan-validator** runs after `html-extractor` Phase 6 approval and before any `fe-component-writer` invocation. Pass the full `HTML-EXTRACTOR COMPLETE` block as inline text. If `VERDICT: NO-GO`, do not invoke `fe-component-writer` — surface unresolved BLOCKERs to the developer, correct the plan, re-run `html-extractor` Phase 6, then re-invoke `fe-plan-validator`. If `GO` (including Challenged items), proceed.
- **fe-component-writer** requires populated `frontend/src/app/api/` — run `fe-api-client-writer` first if empty. Independent components may run in parallel; Wave is done only when every instance reports `Build: PASSED`.
- **fe-route-writer** runs after every `fe-component-writer` for a feature reports `Build: PASSED`. Pass feature name + routing plan (paths, component class names, guards). Missing components cause the agent to fail-stop. One invocation per feature; independent features may run in parallel.

### Adding a new agent

1. Use `agent-architect` to design and produce the file in `.claude/agents/`.
2. After approval, update the table above with one-line "When to delegate".
3. Naming: module-specific `{module}-{purpose}.md`, cross-module `{purpose}.md`. Root only — no nested folders.

---

## Testing policy

- Unit tests: domain entities, value objects, handlers.
- Integration tests: EF Core, migrations, DB. Projects: `{Module}.Tests.Unit`, `{Module}.Tests.Integration` under `backend/tests/`.
- Test-first for new features and business logic changes.
