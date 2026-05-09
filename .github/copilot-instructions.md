<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
and the project constitution at `.specify/memory/constitution.md`.
<!-- SPECKIT END -->

# Copilot instructions for ThingsBooksy-BE

Purpose: provide focused, machine-actionable guidance for Copilot sessions in this repository (build/test/lint commands, high-level architecture, and repo-specific conventions).

---

## Quick commands (use from repository root)

- Build full solution (all projects):
  - dotnet build
- Run the bootstrapper (application host that composes modules):
  - dotnet run --project src\Bootstrapper\ThingsBooksy.Bootstrapper
- Run all tests:
  - dotnet test
- Run a single test (example by project and filter):
  - dotnet test <path-to-test-csproj> --filter "FullyQualifiedName~Namespace.Class.Method"
  - Example: dotnet test tests\ThingsBooksy.Modules.Users.Tests.Integration\ThingsBooksy.Modules.Users.Tests.Integration.csproj --filter "FullyQualifiedName~UserModuleTests.CreateUser"
- Format / lint:
  - dotnet format
  - Verify formatting (CI): dotnet format --verify-no-changes
- EF Core migrations (per module):
  - dotnet ef migrations add {Name} --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper
  - dotnet ef database update --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper
- Docker compose (local, via WSL):
  - wsl docker compose up --build

Notes: prefer running dotnet commands against the specific project when possible (faster feedback).

---

## High-level architecture (short)

- Modular Monolith: source under `src/Modules/{ModuleName}/` and `src/Shared/`.
- Each module typically contains: `{Module}.Api` (Minimal API + endpoints), `{Module}.Core` (domain, EF DbContext, handlers), and `{Module}.Migrations` (optional).
- Modules must not reference each other directly. Inter-module communication uses `IMessageBroker` (events) or `IModuleClient` (queries) and local read-models.
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` composes modules at startup and is the process to run locally.
- Minimal APIs are used (no MVC controllers). Endpoints are registered via a module's `Expose(IEndpointRouteBuilder)`.

---

## Key repo conventions and patterns (extracts)

- Modular rules (see `.specify/memory/constitution.md`): modules cannot reference each other; shared contracts live under `src/Shared/ThingsBooksy.Shared.Abstractions`.
- DTOs live in the `.Api` project; domain objects and EF live in `.Core`.
- No MediatR — use repository's `IDispatcher` for sending commands/queries.
- EF Core: each module has its own DbContext and schema. Migrations are in `{Module}.Migrations`.
- Endpoint discovery: projects must call `services.AddEndpointsApiExplorer()` so Swagger picks up Minimal API endpoints; Swagger must be available at `/swagger`.
- Domain model rules:
  - Entities use private setters and private constructors; creation via static factory `Create(...)`.
  - IDs generated with GUID v7: use `Guid.CreateVersion7()` (avoid Guid.NewGuid()).
- Testing:
  - Unit and integration tests live in `tests/` and are run with `dotnet test`.
  - Run a single test by pointing dotnet test at the test project and use `--filter`.
- Formatting: `dotnet format` is the canonical formatter; CI should run `dotnet format --verify-no-changes`.

---

## Files and places worth reading for context

- `.specify/memory/constitution.md` — authoritative conventions and architecture (contains detailed rules used across repo).
- `.specify` templates and scripts — automation for feature creation and speckit tools used by this team.
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — application composition and startup logic.
- `src/Shared` — shared abstractions (contracts for events/messages).

---

## AI / Copilot session guidance

- Read `.specify/memory/constitution.md` before suggesting cross-module changes or new modules — it encodes hard rules.
- When proposing code that affects module boundaries, include: impacted modules, whether a new event contract is needed (add to shared abstractions), and migration steps (if DB changes).
- For code generation involving IDs, use `Guid.CreateVersion7()` explicitly.
- For suggested endpoint changes, ensure `AddEndpointsApiExplorer()` remains enabled and Swagger routes stay registered.
- If suggesting EF changes, include an explicit `dotnet ef migrations add` command and where to run it (project + startup-project).

---

## Detected assistant/agent configs (incorporate when relevant)

- Existing Copilot prompt and agent helpers in `.github/prompts/` and `.github/agents/` — reuse prompts for speckit tasks.
- Speckit integration files under `.specify/` (manifests, workflows, scripts) — these are used by repo automation and Copilot-based workflows.

---

If this file should be expanded to include more examples (common dotnet test filters, example migration steps, or standard test project paths), say which section to expand.
