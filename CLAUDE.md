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
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — composes and starts all modules

---

## Hard rules (non-negotiable)

### Module boundaries
- Modules must **never** reference each other directly
- Inter-module communication: `IMessageBroker` (fire-and-forget events) or `IModuleClient` (request/response queries)
- If a module needs data from another, it subscribes to events and stores a local **read-model** — never queries another module's database

### Domain entities
- All properties have `private set`
- Parameterless constructor is `private`
- Creation only via static factory method: `public static SomeEntity Create(...)`
- State mutations only through explicit domain methods (`Update`, `Delete`, `Restore`, etc.)

```csharp
internal class SomeEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private SomeEntity() { }

    public static SomeEntity Create(string name)
        => new() { Id = Guid.CreateVersion7(), Name = name };

    public void Update(string name) => Name = name;
}
```

### Identifiers — GUID v7
- **Always** `Guid.CreateVersion7()` — `Guid.NewGuid()` is **forbidden**
- Generate new IDs inside `Create(...)` for the entity's own ID
- Foreign-key references (e.g., `OwnerId`, `GroupId`) are accepted from outside — they point to existing entities

### Persistence
- Each module has its own EF Core `DbContext` and an isolated schema named after the module
- Migrations live in `{Module}.Migrations`

### HTTP / Swagger
- All endpoints use Minimal API — **no MVC controllers**
- Always call `services.AddEndpointsApiExplorer()` so Swagger picks up Minimal API endpoints
- Swagger must remain at `/swagger` in all environments
- Route prefix pattern: `/{module-name}/...`

### No heavy frameworks
- **No MediatR** — use `IDispatcher` for commands and queries
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
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — startup composition and module registration
- `src/Shared/ThingsBooksy.Shared.Abstractions` — shared event/message contracts

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
