# ThingsBooksy Constitution

## Core Principles

### I. Modular Monolith Architecture (NON-NEGOTIABLE)
The application is built as a **Modular Monolith**: a single deployable artifact composed of independent, self-contained modules.
- Each module lives in `src/Modules/{ModuleName}/` and contains exactly two source projects: `{Name}.Api` and `{Name}.Core`
- Modules **must never** directly reference each other — communication happens exclusively through `IMessageBroker` (events) or `IModuleClient` (queries)
- Shared infrastructure belongs exclusively to `src/Shared/` — no cross-module dependencies
- Each module exposes its public contract via `IModule` and registers endpoints in `Expose(IEndpointRouteBuilder)`

### II. Simplified DDD (NON-NEGOTIABLE)
The project applies **Simplified DDD** — no separate Application or Infrastructure layers.
- Each module has exactly two projects: `{Name}.Api` (HTTP layer, Minimal API) and `{Name}.Core` (domain, persistence, commands, queries, events)
- `Core` contains: domain entities, EF `DbContext`, command/query handlers, domain events, value objects
- `Api` contains: `IModule` registration, endpoint definitions, DTOs (request/response records), module JSON config file
- No MediatR — commands/queries are plain C# classes dispatched via `IDispatcher`

### III. Minimal API Endpoints (NON-NEGOTIABLE)
All HTTP endpoints are defined using **ASP.NET Core Minimal APIs** — no MVC controllers.
- Endpoints registered in `{ModuleName}Module.Expose()` for each module
- Route prefix pattern: `/{module-name}/...`
- Always call `services.AddEndpointsApiExplorer()` so Swagger discovers Minimal API endpoints
- Swagger/OpenAPI must be available at `/swagger` in all environments

### IV. Inter-Module Communication via Events
Modules communicate through domain events published via `IMessageBroker`.
- Publishing: `await _messageBroker.PublishAsync(new SomethingHappenedEvent(...))`
- Subscribing: implement `IEventHandler<TEvent>` and register it in the module's `Register(IServiceCollection)`
- If a module needs data from another module, it subscribes to events and stores a **read model** (local copy) — it never queries another module's database
- Event contracts live in `src/Shared/ThingsBooksy.Shared.Abstractions/` — no module-specific types in event bodies

### V. Test-First Approach
New features and bug fixes require tests before implementation.
- Unit tests for domain logic (entities, value objects, handlers)
- Integration tests for database interactions (EF Core, migrations)
- No tests = no merge for business logic changes
- Test projects: `{ModuleName}.Tests.Unit` and `{ModuleName}.Tests.Integration`

### VI. Persistence and Migrations
Each module has its own **EF Core DbContext** with schema isolation.
- Schema naming: lowercase snake_case of the module name — `"bookings"`, `"management_groups"`, `"users"`, etc.
- Every `DbContext` must call `modelBuilder.HasDefaultSchema(...)` — using `"public"` or omitting the call is forbidden (causes cross-module table collisions and silent Respawn data loss in tests)
- Migrations live in a dedicated `{ModuleName}.Migrations` project
- Migration command: `dotnet ef migrations add {Name} --project src/Modules/{M}/{M}.Migrations --startup-project src/Bootstrapper/ThingsBooksy.Bootstrapper`
- Always run `dotnet ef database update` after adding a migration

### VII. Simplicity and YAGNI
Do not add abstractions, patterns, or packages unless they solve a current problem.
- Prefer built-in .NET features over external libraries
- No MediatR, AutoMapper, or heavy frameworks — plain C# dispatch and manual mapping
- Configuration: `module.{name}.json` per module, merged by `ConfigureModules()` at startup

### VIII. Code Formatting
Code is formatted with the **`dotnet format`** tool built into the .NET SDK.
- Run **before every commit**: `dotnet format`
- Formatting covers: indentation, whitespace, `using` organization, code style aligned with `.editorconfig`
- If `.editorconfig` does not exist at the root — create it
- CI/CD should verify formatting: `dotnet format --verify-no-changes`

### IX. Domain Entities — Encapsulation (NON-NEGOTIABLE)
Domain entities **must** protect their state through full encapsulation.
- **Private setters**: all entity properties have `private set` — state changes only through entity methods
- **Private constructor**: the entity constructor is `private` — the only way to create an instance is via the static factory method `Create(...)`
- **Command objects as parameters**: `Create` and `Update` receive the command object as the first parameter, not individual primitives. Resolved external data (foreign keys, timestamps) is added as extra parameters after the command. Maximum **4 parameters total**.
- **Domain methods**: all state mutations are performed through public entity methods (`Update`, `Delete`, `Restore`, etc.)
- **Read-models** use `internal static Upsert(TEvent)` as factory — not `Create` — to signal that the method covers both creation and update semantics.
- Full rules: `.claude/conventions/domain-entity-design.md`
- Pattern example:
  ```csharp
  internal class Booking
  {
      public Guid Id { get; private set; }
      public string Name { get; private set; } = null!;
      public Guid OwnerId { get; private set; }

      private Booking() { }

      public static Booking Create(CreateBookingCommand command)
          => new() { Id = Guid.CreateVersion7(), Name = command.Name, OwnerId = command.OwnerId };

      public void Update(UpdateBookingCommand command) => Name = command.Name;
  }
  ```

### X. Identifiers — GUID v7 (NON-NEGOTIABLE)
All identifiers **generated by the application** must use `Guid.CreateVersion7()`.
- `Guid.NewGuid()` is **forbidden** — replace every occurrence with `Guid.CreateVersion7()`
- GUID v7 is time-ordered (better index performance in PostgreSQL) and monotonic
- New entity IDs are generated inside the entity's `Create(...)` method
- Identifiers referencing **an existing entity** (e.g. `OwnerId`, `GroupId`) may be accepted from outside — they are references, not new identifiers

### XI. DataProvider Pattern (NON-NEGOTIABLE)
Handlers (command and query) must **never** inject `DbContext` directly.
- Each handler depends on a dedicated `IXxxDataProvider` interface — named by stripping `Handler` and appending `DataProvider`
- Both interface and implementation live in `{Module}.Core/Features/{Feature}/DataProviders/`
- Each module calls `AddDataProviders([typeof(Extensions).Assembly])` once from its own `Extensions.cs` — no manual `.AddScoped` per provider
- Data provider methods that are a single awaitable expression return `Task` directly — no `async`/`await`
- Full rules: `.claude/conventions/data-provider-pattern.md` and `.claude/conventions/data-provider-query-syntax.md`

### XII. Command Construction in Endpoints (NON-NEGOTIABLE)
Commands must **never** be bound directly from the HTTP request body.
- Each endpoint with a body declares a request DTO `record` in `{Module}.Api/Requests/` containing only client-permitted fields
- The command is constructed explicitly in the endpoint lambda; server-sourced values (route params, JWT-derived IDs, `Guid.CreateVersion7()`) are injected at the call site
- This prevents clients from overriding server-sourced fields via JSON body injection
- Full rules: `.claude/conventions/command-construction-in-endpoints.md`

### XIII. Naming — Commands, Queries, Handlers, Results
All commands, queries, handlers, and result types follow mechanical naming rules.
- PascalCase everywhere; full unambiguous names including module/aggregate context
- Suffixes: `Command`, `CommandHandler`, `Query`, `QueryHandler`
- Result class name derived from handler name by stripping `Handler` — nothing else
- One class per file; file name equals class name exactly
- Full rules: `.claude/conventions/naming-commands-queries-handlers-results.md`

### XIV. Module Internal Visibility
Every `.Core` project must declare four `InternalsVisibleTo` attributes in `Extensions.cs`.
- `ThingsBooksy.Modules.{ModuleName}.Api`
- `ThingsBooksy.Modules.{ModuleName}.Migrations`
- `ThingsBooksy.Modules.{ModuleName}.IntegrationTests`
- `DynamicProxyGenAssembly2`
- Missing any one causes compilation errors or runtime mock failures
- Full rules: `.claude/conventions/internals-visible-to.md`

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Language | C# 13 |
| Database | PostgreSQL 17 (Docker) |
| ORM | Entity Framework Core 10 |
| Authentication | JWT Bearer + AES-256 symmetric encryption |
| Containerization | Docker / docker-compose (WSL locally) |
| API Docs | Swashbuckle / Swagger UI |
| Logging | Serilog |
| Solution Format | `.slnx` (VS 2022+) |

## Development Workflow

1. **New module**: create `{Name}.Api` + `{Name}.Core` + `{Name}.Migrations`, register in `Bootstrapper`, add `module.{name}.json`
2. **New feature**: specify (`/speckit-specify`), plan (`/speckit-plan`), tasks (`/speckit-tasks`), implement (`/speckit-implement`)
3. **Database change**: add EF migration, update database, verify in Docker
4. **Before commit**: ensure the project builds, Swagger shows all endpoints, Docker Compose starts correctly

## Docker and Local Environment

- Local Docker runs via **WSL** — prefix every docker command with `wsl`: `wsl docker compose up --build`
- Environment: `ASPNETCORE_ENVIRONMENT=Docker` activates `appsettings.Docker.json`
- PostgreSQL connection string in `appsettings.Docker.json` must include username and password
- App available at `localhost:8080`, Swagger at `localhost:8080/swagger`

## Coding Conventions

Detailed, example-rich rules for recurring code patterns live in `.claude/conventions/`. Every agent that writes or reviews code must read the relevant files before acting. The constitution states *what* is non-negotiable; the convention files state *how* to implement it correctly.

| File | Covers |
|---|---|
| `domain-entity-design.md` | Entity structure, member order, factory/mutation signatures, read-model `Upsert` |
| `naming-commands-queries-handlers-results.md` | Naming rules for all CQRS types and result records |
| `data-provider-pattern.md` | IDataProvider per handler, registration via `AddDataProviders`, async eliding |
| `data-provider-query-syntax.md` | Parenthesized LINQ query syntax for joins and group-by |
| `command-construction-in-endpoints.md` | Request DTOs in `Requests/`, explicit command construction, no direct body binding |
| `minimal-api-endpoints.md` | No controllers, `Expose()` method, route prefix, `AddEndpointsApiExplorer()` |
| `dispatcher-usage.md` | `IDispatcher.SendAsync` / `QueryAsync`, no MediatR, no direct handler calls |
| `ef-schema-isolation.md` | `HasDefaultSchema` mandatory, lowercase snake_case, never `"public"` |
| `internals-visible-to.md` | Four `InternalsVisibleTo` declarations per `.Core` project |
| `integration-test-naming.md` | `{Action}{Entity}_{Condition}_{Result}` test method naming |
| `integration-test-infrastructure.md` | TestClient, Factory, IntegrationTestCollection patterns |

## Governance

- This constitution takes precedence over all other practices and conventions
- Changes require updating this file with a justification and a version increment
- All PRs must verify compliance with Modular Monolith and Simplified DDD principles
- Any deviation from the `AddEndpointsApiExplorer()` requirement is forbidden — Swagger compliance is mandatory

**Version**: 1.2.0 | **Ratified**: 2026-04-23 | **Last Amended**: 2026-05-12
