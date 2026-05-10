# ThingsBooksy

> **Business description coming soon.**

---

## Architecture

ThingsBooksy is a **Modular Monolith** — a single deployable artifact composed of isolated, independently-developed modules. Each module owns its domain, its database schema, and its HTTP surface. Modules never reference each other directly; all cross-module communication goes through events (fire-and-forget) or queries (request/response) via shared infrastructure abstractions.

```
┌─────────────────────────────────────────────────────┐
│              ThingsBooksy.Bootstrapper               │
│  Discovers and composes all modules at startup via   │
│  reflection (IModule interface)                      │
└──────────────────┬──────────────────────────────────┘
                   │
         ┌─────────┴──────────┐
         ▼                    ▼
┌──────────────────┐  ┌──────────────────────┐
│  Users           │  │  ManagementGroups     │
│  .Api            │  │  .Api                 │
│  .Core           │  │  .Core                │
│  .Migrations     │  │  .Migrations          │
│  .Contracts      │  │                       │
└────────┬─────────┘  └──────────┬────────────┘
         │    UserSignedUp       │
         │ ─────────────────────►│
         │    (IMessageBroker)
         └────────────┬──────────┘
                      │
           ┌──────────▼──────────┐
           │  Shared.Abstractions│
           │  Events / Contracts │
           │  IDispatcher        │
           │  IMessageBroker     │
           └─────────────────────┘
```

Each module follows a consistent project structure:

```
src/Modules/{ModuleName}/
├── {ModuleName}.Api               # Minimal API endpoints, DTOs, module config
├── {ModuleName}.Core              # Domain entities, EF DbContext, handlers
├── {ModuleName}.Migrations        # EF Core migrations (optional)
├── {ModuleName}.Contracts         # Contracts exposed to other modules (optional)
└── {ModuleName}.IntegrationTests  # Integration tests
```

---

## Modules

| Module | Description |
|---|---|
| **Users** | Authentication and user account management |
| **ManagementGroups** | Creation and management of groups and their memberships |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Language | C# 13 |
| Database | PostgreSQL 17 (Docker) |
| ORM | Entity Framework Core 10 |
| Auth | JWT Bearer + AES-256 |
| API docs | Swashbuckle / Swagger UI (`/swagger`) |
| Logging | Serilog |
| Containerization | Docker / docker-compose |

---

## AI

ThingsBooksy uses a fleet of purpose-built Claude Code agents that automate the full feature-delivery pipeline — from product discovery through architecture validation.

### Development Workflow

```
product-strategist
       │  produces handoff brief
       ▼
  doc-writer  (writes ADR)
       │
       ▼
/speckit-specify → /speckit-plan → /speckit-tasks
       │
       ▼
 plan-validator  (GO / NO-GO + Execution Map)
       │ GO
       ▼
contract-definer  (only if cross-module deps exist)
       │
       ▼
 module-writer  ──► migration-agent ──► quality-reviewer ──► integration-test-writer
  (per module,                                                  (per module, parallel)
   parallel by Wave)
       │
       ▼
architecture-guard  (solution-wide check after each Wave)
```

If `architecture-guard` finds unresolved violations, a repair loop is triggered: `module-writer` → `migration-agent` → `quality-reviewer` → `integration-test-writer` → `architecture-guard`, up to 2 iterations before surfacing the issue to the developer.

---

### Agent Reference

| Agent | Responsibility |
|---|---|
| **agent-architect** | Designs new agents for the fleet. Challenges bad ideas, proposes improvements to the agent ecosystem. |
| **product-strategist** | Conducts a two-phase business + technical interview to clarify a feature before any code is written. Produces a handoff brief for `/speckit-specify`. |
| **doc-writer** | Writes Architecture Decision Records (ADRs) in `.specify/decisions/` immediately after `product-strategist` delivers a brief. |
| **plan-validator** | Runs deterministic consistency checks across `spec.md`, `plan.md`, and `tasks.md`. Returns a `GO`/`NO-GO` verdict and an Execution Map that groups tasks into parallel Waves. |
| **contract-definer** | Defines and writes all `IEvent` and `IModuleClient` contract types in `Shared.Abstractions` when cross-module dependencies exist — before any module implementation begins. |
| **module-writer** | Implements a single module's assigned tasks: entities, DbContext, command/query handlers, Minimal API endpoints, and DTOs. One instance per module, parallel across independent modules. |
| **migration-agent** | Runs `dotnet ef migrations add` for a module after `module-writer` reports schema changes. |
| **quality-reviewer** | Reviews a module's written files against a 10-point architectural checklist. Interactive session — one issue at a time. |
| **integration-test-writer** | Writes integration tests for a module using `WebApplicationFactory`, `TestClient`, and xUnit. Runs the test suite and reports pass/fail. |
| **architecture-guard** | Performs a solution-wide scan for cross-module violations: direct assembly references, orphaned events, missing registrations, and more. Interactive review — one violation at a time. |
