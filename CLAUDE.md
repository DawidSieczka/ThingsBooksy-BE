# CLAUDE.md — ThingsBooksy

## Commands

### Build & run
- Build solution: `dotnet build`
- Run application: `dotnet run --project src\Bootstrapper\ThingsBooksy.Bootstrapper`
- Docker (local, via WSL): `wsl docker compose up --build`

### Test
- Run all tests: `dotnet test`
- Run single test: `dotnet test <path-to-test-csproj> --filter "FullyQualifiedName~Namespace.Class.Method"`
- Prefer targeting a specific project over the full solution for faster feedback
- Integration tests: use `/integration-tests` command

### Format
- Format: `dotnet format`
- Verify only (CI): `dotnet format --verify-no-changes`
- Run `dotnet format` before suggesting any commit

### EF Core migrations (per module)
- Add: `dotnet ef migrations add {Name} --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper`
- Apply: `dotnet ef database update --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper`
- Always run `database update` after adding a migration

---

## Architecture

Modular Monolith — one deployable artifact, isolated independent modules.

- Each module: `src/Modules/{ModuleName}/` with exactly two source projects:
  - `{Module}.Api` — HTTP layer (Minimal API), endpoints, DTOs (records), module JSON config
  - `{Module}.Core` — domain entities, EF DbContext, command/query handlers, domain events, value objects
  - `{Module}.Migrations` — EF migrations (separate project, optional)
- `src/Shared/ThingsBooksy.Shared.Abstractions` — shared contracts only (events, interfaces); no module-specific types here
  - Events are organized by producing module: `Events/{ModuleName}/SomeEvent.cs` → namespace `ThingsBooksy.Shared.Abstractions.Events.{ModuleName}`
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — composes and starts all modules

---

## Coding conventions

All coding conventions live in `.claude/conventions/`. Read the relevant file before writing or reviewing code. Every agent that writes or reviews code must follow these files exactly.

| Convention file | Rule summary |
|---|---|
| `domain-entity-design.md` | `private set` on all properties, `private` ctor, `Create` factory, `Upsert` for read-models, command object as parameter, max 4 params, `Guid.CreateVersion7()` |
| `naming-commands-queries-handlers-results.md` | PascalCase, full unambiguous names, `Command`/`CommandHandler`/`Query`/`QueryHandler` suffixes, Result derived from handler name, one class per file |
| `data-provider-pattern.md` | Handlers never inject `DbContext` directly; use `IXxxDataProvider`; `AddDataProviders` called once per module in `Extensions.cs`; async eliding |
| `data-provider-query-syntax.md` | Parenthesized LINQ query syntax for joins/group by; method syntax only for simple single-table queries |
| `command-construction-in-endpoints.md` | Commands never bound directly from HTTP body; request DTO in `{Module}.Api/Requests/`; server-sourced values injected explicitly |
| `minimal-api-endpoints.md` | No `ControllerBase`; endpoints in `Expose()`; `/{module-name}/...` prefix; `AddEndpointsApiExplorer()` in `Register()` |
| `dispatcher-usage.md` | `IDispatcher.SendAsync` for commands, `IDispatcher.QueryAsync` for queries; no direct handler calls; no MediatR |
| `ef-schema-isolation.md` | `HasDefaultSchema(lowercase_snake_case)` mandatory in every `DbContext`; never `"public"`; schema = module name |
| `internals-visible-to.md` | Every `.Core` project declares 4 `InternalsVisibleTo` attributes: `.Api`, `.Migrations`, `.IntegrationTests`, `DynamicProxyGenAssembly2` |
| `integration-test-naming.md` | `{Action}{Entity}_{Condition}_{Result}` — status code or behavioral description as last segment |
| `integration-test-infrastructure.md` | TestClient (HTTP + DB methods), Factory (per entity), IntegrationTestCollection (per module); user instantiated per test |

---

## Hard rules (non-negotiable)

### Module boundaries
- Modules must **never** reference each other directly
- Inter-module communication: `IMessageBroker` (fire-and-forget events) or `IModuleClient` (request/response queries)
- If a module needs data from another, it subscribes to events and stores a local **read-model** — never queries another module's database

### Domain entities
See `.claude/conventions/domain-entity-design.md` for the full rule. Summary:
- All properties have `private set`; parameterless constructor is `private`
- Creation only via `public static SomeEntity Create(SomeCommand command)` — pass a command object, not raw primitives
- State mutations only through named domain methods (`Update`, `Delete`, `Restore`, etc.)
- Maximum 4 parameters on factory and mutation methods

### Identifiers — GUID v7
- **Always** `Guid.CreateVersion7()` — `Guid.NewGuid()` is **forbidden**
- Generate new IDs inside `Create(...)` for the entity's own ID
- Foreign-key references (e.g., `OwnerId`, `GroupId`) are accepted from outside — they point to existing entities

### Persistence
- Each module has its own EF Core `DbContext` and an isolated schema (lowercase snake_case, e.g. `"bookings"`, `"management_groups"`)
- `HasDefaultSchema(...)` is mandatory — never use `"public"` (see `.claude/conventions/ef-schema-isolation.md`)
- Migrations live in `{Module}.Migrations`

### HTTP / Swagger
- All endpoints use Minimal API — **no MVC controllers** (see `.claude/conventions/minimal-api-endpoints.md`)
- Always call `services.AddEndpointsApiExplorer()` so Swagger picks up Minimal API endpoints
- Swagger must remain at `/swagger` in all environments
- Route prefix pattern: `/{module-name}/...`

### No heavy frameworks
- **No MediatR** — use `IDispatcher` for commands and queries (see `.claude/conventions/dispatcher-usage.md`)
- **No AutoMapper** — map manually
- Prefer built-in .NET over external libraries (YAGNI)

---

## Tech stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Language | C# 13 |
| Database | PostgreSQL 17 (Docker) |
| ORM | Entity Framework Core 10 |
| Auth | JWT Bearer + AES-256 symmetric encryption |
| Containerization | Docker / docker-compose (WSL locally) |
| API docs | Swashbuckle / Swagger UI |
| Logging | Serilog |
| Solution format | `.slnx` (VS 2022+) |

---

## Testing

- **Test-first** for new features and business logic changes — no tests means no merge
- Unit tests: domain entities, value objects, handlers
- Integration tests: EF Core, migrations, DB interactions
- Test projects: `{Module}.Tests.Unit` and `{Module}.Tests.Integration` under `tests/`

---

## Development workflow

For **new features or substantial changes**, follow the SpecKit workflow:
1. `/speckit-specify` — write specification
2. `/speckit-plan` — implementation plan
3. `/speckit-tasks` — task breakdown
4. `/speckit-implement` — implementation

Additional SpecKit commands: `/speckit-clarify`, `/speckit-analyze`, `/speckit-checklist`, `/speckit-constitution`, `/speckit-taskstoissues`

For **small bugfixes and minor changes**, proceed directly without SpecKit.

---

## Agent fleet

Claude (main session) is the orchestrator — it reads this file to decide when to delegate to subagents.

### Known agents

| Agent | When to delegate |
|---|---|
| `agent-architect` | Designing a new agent, exploring what agents could improve the workflow, growing the fleet |
| `convention-writer` | User wants to write a new coding convention — interactive agent that challenges the proposal, refines through dialogue, and writes the rule to `.claude/conventions/` after approval |
| `product-strategist` | User wants to define or clarify a feature before implementation — conducts an interactive two-phase interview (business then technical) and produces a handoff brief for /speckit-specify |
| `doc-writer` | After product-strategist produces a handoff brief — immediately delegate to doc-writer to create the ADR. Also use when user declares a Stage 3 pivot. |
| `plan-validator` | After /speckit-tasks completes and before /speckit-implement — runs deterministic consistency checks on spec.md, plan.md, tasks.md and produces a VERDICT (GO/NO-GO) with an EXECUTION MAP grouping tasks into parallel waves |
| `contract-definer` | After plan-validator returns GO with an EXECUTION MAP containing cross-module dependencies — defines and writes all IEvent and IModuleClient contract types in ThingsBooksy.Shared.Abstractions before any module-writer is invoked |
| `module-writer` | After contract-definer reports "All contracts are ready" (or after plan-validator GO with no cross-module dependencies) — spawn one instance per module per Wave, passing module name and assigned task IDs; instances for independent modules may run in parallel |
| `migration-agent` | After module-writer reports `Schema changes` other than NONE — receives module name and Schema changes block, generates migration name, runs dotnet ef migrations add, reports schema summary |
| `quality-reviewer` | After migration-agent completes (or directly after module-writer if Schema changes: NONE) — interactive code review session, one issue at a time; ends with QUALITY-REVIEWER COMPLETE block |
| `integration-test-writer` | After quality-reviewer ends the session — spawn one instance per module, passing module name and assigned task IDs; instances for independent modules may run in parallel |
| `architecture-guard` | After integration-test-writer reports INTEGRATION-TEST-WRITER COMPLETE for all modules in the current Wave — checks the full solution for cross-module architectural violations. Interactive review, one violation at a time. Ends with ARCHITECTURE-GUARD COMPLETE block. |

### Orchestration rule — doc-writer

After `product-strategist` returns a handoff brief, immediately delegate to `doc-writer` without waiting for a separate user instruction. Pass the full handoff brief as input. Do not proceed to `/speckit-specify` until `doc-writer` confirms the ADR was written.

### Orchestration rule — plan-validator

After `plan-validator` returns NO-GO, do not proceed to `/speckit-implement`. Surface the BLOCKING issues to the user and wait for corrections before re-running. If VERDICT is GO, pass the EXECUTION MAP to `/speckit-implement` as context for parallel task scheduling.

### Orchestration rule — contract-definer

After `plan-validator` returns GO, inspect the EXECUTION MAP for cross-module dependencies. If any exist, delegate to `contract-definer` before invoking any `module-writer`. Pass the full EXECUTION MAP and the `.specify/` path as input. Do not start module implementation until `contract-definer` reports "All contracts are ready." If the EXECUTION MAP contains no cross-module dependencies, skip `contract-definer` and proceed directly to `module-writer`.

### Orchestration rule — migration-agent

After `module-writer` completes for a module, inspect its `Schema changes` field. If the value is anything other than `NONE`, delegate to `migration-agent` before invoking `quality-reviewer`. Pass the module name and the full Schema changes block as input. Do not invoke `quality-reviewer` until `migration-agent` reports `MIGRATION-AGENT COMPLETE`. If `Schema changes: NONE`, skip `migration-agent` entirely and proceed directly to `quality-reviewer`.

### Orchestration rule — quality-reviewer

After `migration-agent` reports `MIGRATION-AGENT COMPLETE` (or directly after `module-writer` if `Schema changes: NONE`), delegate to `quality-reviewer`. Pass the module name and the `Written files` list from the MODULE-WRITER COMPLETE block. Wait for the user to complete the interactive review session (signalled by `QUALITY-REVIEWER COMPLETE`). Do not invoke `integration-test-writer` until the review session ends.

### Orchestration rule — integration-test-writer

After `quality-reviewer` reports `QUALITY-REVIEWER COMPLETE` for a module, immediately delegate to `integration-test-writer`, passing the same module name and task IDs. Multiple instances may run in parallel for independent modules. Do not mark a Wave as complete until `integration-test-writer` reports `Test run: PASSED {n}/{n}` with no failures.

### Orchestration rule — architecture-guard

After `integration-test-writer` reports `INTEGRATION-TEST-WRITER COMPLETE` for **all** modules in the current Wave, delegate to `architecture-guard`. Pass the list of Wave module names as input. Wait for the user to complete the interactive review session (signalled by `ARCHITECTURE-GUARD COMPLETE`). Do not mark the Wave as complete until the block is received.

**If `Challenged (no resolution)` is greater than zero — repair loop:**

1. For each unresolved BLOCKER, identify the affected module from the violation location.
2. Re-invoke `module-writer` for that module, passing the violation description instead of task IDs. Instruct it to fix only the reported violation — no new features.
3. Run the full tail pipeline for that module: `migration-agent` (if Schema changes) → `quality-reviewer` → `integration-test-writer`.
4. Re-invoke `architecture-guard` with the same Wave module list.
5. If `architecture-guard` still reports the same BLOCKER after 2 repair iterations, stop and surface the issue to the user — do not loop further.

**Manual prerequisite for new modules:** Before invoking `integration-test-writer` on a module that did not previously have integration tests, manually add the module's EF Core schema name to `SchemasToInclude` in `ThingsBooksyWebAppFactory.cs` (in `src/Shared/ThingsBooksy.Shared.IntegrationTests/`). Failing to do so causes silent test pollution — Respawn will not clean the new module's data between tests.

### Naming convention

All agents live in `.claude/agents/` (root only — nested directories are not supported).
- Module-specific: `{module}-{purpose}.md` — e.g. `users-code-reviewer.md`
- Cross-module: `shared-{purpose}.md` or `{purpose}.md`

### Adding a new agent

1. Use `agent-architect` to design and produce the agent file
2. After approval, `agent-architect` will propose an update to the table above
3. Apply the update so future sessions know the agent exists

---

## Docker & local environment

- All `docker` commands must be prefixed with `wsl`: `wsl docker compose up --build`
- Environment variable `ASPNETCORE_ENVIRONMENT=Docker` activates `appsettings.Docker.json`
- App: `localhost:8080`, Swagger: `localhost:8080/swagger`
- Module config: `module.{name}.json` per module, merged at startup via `ConfigureModules()`

---

## Key files

- `.specify/memory/constitution.md` — authoritative architecture rules (full version of what is summarized here; read it before proposing cross-module changes or new modules)
- `.claude/conventions/` — coding conventions (DataProvider pattern, naming, command construction, endpoint structure, test patterns); read before writing or reviewing code
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — startup composition and module registration
- `src/Shared/ThingsBooksy.Shared.Abstractions` — shared event/message contracts

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at specs/003-resource-schema/plan.md
<!-- SPECKIT END -->
