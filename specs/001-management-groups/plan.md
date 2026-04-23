# Plan Implementacji: Moduł ManagementGroups

**Gałąź**: `001-management-groups` | **Data**: 2026-04-23 | **Spec**: `specs/001-management-groups/spec.md`

---

## Podsumowanie

Implementacja nowego modułu `ManagementGroups` w architekturze Modularnego Monolitu (Simplified DDD).
Moduł umożliwia zalogowanemu użytkownikowi tworzenie i zarządzanie grupami zarządzania,
w tym dodawanie/usuwanie członków przez email oraz soft delete z możliwością przywrócenia.
Komunikacja z modułem Users odbywa się wyłącznie przez `IMessageBroker` (read model userId↔email).

---

## Kontekst Techniczny

**Język/Wersja**: C# 13 / .NET 10
**Główne zależności**: ASP.NET Core Minimal API, EF Core 10, Npgsql, Swashbuckle
**Baza danych**: PostgreSQL 17 (schemat `management_groups`)
**Testy**: xUnit (jednostkowe + integracyjne — w kolejnym kroku)
**Platforma docelowa**: Docker (Linux container), lokalna ekspozycja `localhost:8080`
**Typ projektu**: Moduł w Modularnym Monolicie
**Ograniczenia**: Brak bezpośrednich referencji do innych modułów; tylko `Shared.Abstractions` i `Shared.Infrastructure`

---

## Weryfikacja Konstytucji

| Zasada | Status | Uwagi |
|--------|--------|-------|
| Dwa projekty per moduł (Api + Core) | ✅ | `ManagementGroups.Api` + `ManagementGroups.Core` |
| Brak bezpośrednich referencji między modułami | ✅ | Komunikacja przez `IMessageBroker` / read model |
| Minimal API (brak kontrolerów MVC) | ✅ | Endpointy w `ManagementGroupsModule.Expose()` |
| AddEndpointsApiExplorer() | ✅ | Już obecne w `Shared.Infrastructure` |
| Własny DbContext + schema isolation | ✅ | Schema `management_groups` |
| Soft delete zamiast hard delete | ✅ | Pole `DeletedAt` na encji |

---

## Struktura Projektu

### Dokumentacja (ta funkcjonalność)

```text
specs/001-management-groups/
├── spec.md          ✅ gotowe
├── plan.md          ✅ ten plik
└── tasks.md         ⏳ do wygenerowania (/speckit.tasks)
```

### Kod źródłowy (nowe pliki)

```text
src/Modules/ManagementGroups/
├── ThingsBooksy.Modules.ManagementGroups.Api/
│   ├── ThingsBooksy.Modules.ManagementGroups.Api.csproj
│   ├── ManagementGroupsModule.cs          # IModule — Register + Expose
│   ├── module.managementgroups.json       # enabled flag + JWT config
│   └── Endpoints/
│       ├── CreateManagementGroupEndpoint.cs
│       ├── UpdateManagementGroupEndpoint.cs
│       ├── DeleteManagementGroupEndpoint.cs
│       ├── RestoreManagementGroupEndpoint.cs
│       ├── GetManagementGroupEndpoint.cs
│       ├── GetManagementGroupsEndpoint.cs
│       ├── AddMemberEndpoint.cs
│       └── RemoveMemberEndpoint.cs
│
└── ThingsBooksy.Modules.ManagementGroups.Core/
    ├── ThingsBooksy.Modules.ManagementGroups.Core.csproj
    ├── Domain/
    │   ├── ManagementGroup.cs             # Encja + metody domenowe
    │   └── GroupMember.cs                 # Encja relacji
    ├── Commands/
    │   ├── CreateManagementGroup.cs
    │   ├── UpdateManagementGroup.cs
    │   ├── DeleteManagementGroup.cs
    │   ├── RestoreManagementGroup.cs
    │   ├── AddGroupMember.cs
    │   └── RemoveGroupMember.cs
    ├── Queries/
    │   ├── GetManagementGroup.cs
    │   └── GetManagementGroups.cs
    ├── DAL/
    │   ├── ManagementGroupsDbContext.cs
    │   ├── Configurations/
    │   │   ├── ManagementGroupConfiguration.cs
    │   │   └── GroupMemberConfiguration.cs
    │   └── Repositories/
    │       └── ManagementGroupRepository.cs
    ├── Events/
    │   └── Handlers/
    │       └── UserSignedUpHandler.cs     # Subskrybuje SignedUp z modułu Users
    └── ReadModels/
        └── UserReadModel.cs               # Lokalna kopia danych z Users
```

### Migracje

```text
src/Modules/ManagementGroups/
└── ThingsBooksy.Modules.ManagementGroups.Migrations/
    ├── ThingsBooksy.Modules.ManagementGroups.Migrations.csproj
    └── Migrations/
        └── {timestamp}_Init.cs
```

---

## Fazy Implementacji

### Faza 0 — Szkielet Modułu

- [ ] Utworzenie projektów `ManagementGroups.Api` i `ManagementGroups.Core`
- [ ] Dodanie projektów do `ThingsBooksy.slnx`
- [ ] Referencje: `Api → Core`, `Api → Shared.Infrastructure`, `Core → Shared.Abstractions`
- [ ] Rejestracja modułu w `Bootstrapper` (ProjectReference + `LoadModules`)
- [ ] Plik `module.managementgroups.json` z `enabled: true`
- [ ] Pusty `ManagementGroupsModule.cs` implementujący `IModule`

### Faza 1 — Domena i Persystencja

- [ ] Encja `ManagementGroup` (właściwości, soft delete, metody domenowe: `Delete()`, `Restore()`, `Update()`)
- [ ] Encja `GroupMember`
- [ ] `ManagementGroupsDbContext` ze schematem `management_groups`
- [ ] Konfiguracje EF Core (fluent API)
- [ ] Projekt `ManagementGroups.Migrations` + dodanie do `.slnx`
- [ ] Migracja inicjalizująca (`dotnet ef migrations add Init ...`)
- [ ] `ManagementGroupRepository` (CRUD + soft delete aware)

### Faza 2 — Komendy i Zapytania

- [ ] `CreateManagementGroupCommand` + handler
- [ ] `UpdateManagementGroupCommand` + handler
- [ ] `DeleteManagementGroupCommand` + handler (soft delete)
- [ ] `RestoreManagementGroupCommand` + handler
- [ ] `AddGroupMemberCommand` + handler (walidacja email → `UserReadModel`)
- [ ] `RemoveGroupMemberCommand` + handler
- [ ] `GetManagementGroupQuery` + handler
- [ ] `GetManagementGroupsQuery` + handler

### Faza 3 — Komunikacja z Modułem Users

- [ ] Definicja `UserReadModel` (UserId, Email) w `ManagementGroups.Core`
- [ ] `UserSignedUpHandler` (implementuje `IEventHandler<UserSignedUp>`) — zapisuje read model
- [ ] Rejestracja handlera w `ManagementGroupsModule.Register()`
- [ ] Weryfikacja że `UserSignedUp` event jest zdefiniowany w `Shared.Abstractions`

### Faza 4 — Endpointy HTTP

- [ ] POST `/management-groups` — tworzenie grupy (JWT required)
- [ ] GET `/management-groups` — lista grup użytkownika (JWT required)
- [ ] GET `/management-groups/{id}` — szczegóły grupy (JWT required, member/owner only)
- [ ] PUT `/management-groups/{id}` — edycja grupy (JWT required, owner only)
- [ ] DELETE `/management-groups/{id}` — soft delete (JWT required, owner only)
- [ ] POST `/management-groups/{id}/restore` — przywrócenie (JWT required, owner only)
- [ ] POST `/management-groups/{id}/members` — dodanie członka przez email (JWT required, owner only)
- [ ] DELETE `/management-groups/{id}/members/{userId}` — usunięcie członka (JWT required, owner only)

### Faza 5 — Weryfikacja

- [ ] Build projektu bez błędów
- [ ] Migracja zastosowana na bazie Dockerowej
- [ ] Swagger pokazuje wszystkie 8 endpointów modułu
- [ ] Manualne smoke testy (rejestracja → login → tworzenie grupy → dodanie członka → soft delete → restore)

---

## Model Danych

### Tabela `management_groups.management_groups`

| Kolumna | Typ | Uwagi |
|---------|-----|-------|
| `id` | UUID | PK |
| `name` | VARCHAR(200) | NOT NULL |
| `description` | TEXT | nullable |
| `owner_id` | UUID | FK (brak hard FK do tabeli users — inny moduł) |
| `created_at` | TIMESTAMPTZ | NOT NULL |
| `updated_at` | TIMESTAMPTZ | NOT NULL |
| `deleted_at` | TIMESTAMPTZ | nullable — soft delete |

### Tabela `management_groups.group_members`

| Kolumna | Typ | Uwagi |
|---------|-----|-------|
| `group_id` | UUID | PK, FK → management_groups.id |
| `user_id` | UUID | PK |
| `joined_at` | TIMESTAMPTZ | NOT NULL |

### Tabela `management_groups.user_read_models`

| Kolumna | Typ | Uwagi |
|---------|-----|-------|
| `id` | UUID | PK (UserId z modułu Users) |
| `email` | VARCHAR(256) | NOT NULL, UNIQUE |

---

## Śledzenie Złożoności

> Brak naruszeń konstytucji — architektura zgodna z zasadami projektu.
