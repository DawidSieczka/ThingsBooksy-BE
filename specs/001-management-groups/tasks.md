# Zadania: Moduł ManagementGroups

**Źródło**: `specs/001-management-groups/spec.md` + `specs/001-management-groups/plan.md`
**Gałąź**: `001-management-groups`

## Format: `[ID] [P?] [US?] Opis`

- **[P]** — można wykonać równolegle (inne pliki, brak zależności)
- **[US1–US4]** — powiązana historia użytkownika
- Każda faza kończy się **Checkpointem** który można zwalidować niezależnie

---

## Faza 1: Szkielet Modułu *(blokuje wszystko)*

**Cel**: Działający pusty moduł zarejestrowany w aplikacji — bez tej fazy żadna historia nie ruszy.

- [ ] T001 Utwórz projekt `ThingsBooksy.Modules.ManagementGroups.Api` (classlib, net10.0) w `src/Modules/ManagementGroups/`
- [ ] T002 Utwórz projekt `ThingsBooksy.Modules.ManagementGroups.Core` (classlib, net10.0) w `src/Modules/ManagementGroups/`
- [ ] T003 Utwórz projekt `ThingsBooksy.Modules.ManagementGroups.Migrations` (classlib, net10.0) w `src/Modules/ManagementGroups/`
- [ ] T004 Dodaj wszystkie 3 projekty do `ThingsBooksy.slnx`
- [ ] T005 [P] Dodaj referencje: `Api → Core`, `Api → Shared.Infrastructure`, `Core → Shared.Abstractions`, `Migrations → Core`
- [ ] T006 Utwórz plik `module.managementgroups.json` w `ManagementGroups.Api` z `managementgroups:module:enabled: true`
- [ ] T007 Utwórz `ManagementGroupsModule.cs` implementujący `IModule` (puste `Register` i `Expose`)
- [ ] T008 Dodaj `<ProjectReference>` do `ManagementGroups.Api` w projekcie `Bootstrapper`
- [ ] T009 Zbuduj solution i zweryfikuj brak błędów kompilacji

**Checkpoint F1**: `dotnet build` przechodzi, moduł widoczny w logach startowych aplikacji.

---

## Faza 2: Domena i Persystencja *(blokuje US1–US4)*

**Cel**: Encje domenowe, DbContext i migracja — fundament dla wszystkich historii.

- [ ] T010 [P] Utwórz encję `ManagementGroup.cs` w `Core/Domain/` z polami: `Id`, `Name`, `Description`, `OwnerId`, `CreatedAt`, `UpdatedAt`, `DeletedAt`; metody domenowe: `Update()`, `Delete()`, `Restore()`
- [ ] T011 [P] Utwórz encję `GroupMember.cs` w `Core/Domain/` z polami: `GroupId`, `UserId`, `JoinedAt`
- [ ] T012 [P] Utwórz `UserReadModel.cs` w `Core/ReadModels/` z polami: `Id` (UserId), `Email`
- [ ] T013 Utwórz `ManagementGroupsDbContext.cs` w `Core/DAL/` ze schematem `management_groups`; dodaj `DbSet` dla wszystkich 3 encji
- [ ] T014 [P] Utwórz `ManagementGroupConfiguration.cs` w `Core/DAL/Configurations/` — fluent API: PK, indeksy, unikalność nazwy globalnie, soft delete filter (`DeletedAt == null`)
- [ ] T015 [P] Utwórz `GroupMemberConfiguration.cs` w `Core/DAL/Configurations/` — klucz złożony `(GroupId, UserId)`
- [ ] T016 [P] Utwórz `UserReadModelConfiguration.cs` w `Core/DAL/Configurations/` — PK, unikalny email
- [ ] T017 Skonfiguruj projekt `Migrations` z referencją do `Core` i `ManagementGroupsDbContext` jako DbContext
- [ ] T018 Wykonaj migrację inicjalizującą: `dotnet ef migrations add Init --project .../ManagementGroups.Migrations --startup-project .../Bootstrapper`
- [ ] T019 Zastosuj migrację na bazie Dockerowej: `dotnet ef database update ...`

**Checkpoint F2**: Tabele `management_groups`, `group_members`, `user_read_models` istnieją w bazie.

---

## Faza 3: US1 — Tworzenie Grupy (P1) 🎯 MVP

**Cel**: Zalogowany użytkownik może utworzyć grupę i stać się jej właścicielem.

**Niezależny Test**: POST `/management-groups` z JWT → HTTP 201, rekord w bazie z poprawnym `OwnerId`.

- [ ] T020 [US1] Utwórz `CreateManagementGroupCommand.cs` w `Core/Commands/` (record: `Name`, `Description`, `OwnerId`)
- [ ] T021 [US1] Utwórz handler `CreateManagementGroupHandler.cs` — tworzy `ManagementGroup`, zapisuje przez DbContext; zwraca `Guid` nowej grupy; waliduje unikalność nazwy (HTTP 422 jeśli duplikat)
- [ ] T022 [US1] Zarejestruj handler w `ManagementGroupsModule.Register(IServiceCollection)`
- [ ] T023 [US1] Utwórz endpoint `POST /management-groups` w `ManagementGroupsModule.Expose()` — wymaga JWT, wyciąga `UserId` z claimsów, wywołuje handler
- [ ] T024 [US1] Zdefiniuj request DTO `CreateManagementGroupRequest` (record) i response DTO `ManagementGroupResponse`

**Checkpoint US1**: POST `/management-groups` z JWT zwraca HTTP 201; bez JWT zwraca HTTP 401; duplikat nazwy zwraca HTTP 422.

---

## Faza 4: US2 — Zarządzanie Grupą (P2)

**Cel**: Właściciel może edytować, usuwać (soft delete) i przywracać swoje grupy.

**Niezależny Test**: PUT → HTTP 200; DELETE → HTTP 204 + `DeletedAt != null`; POST restore → HTTP 200 + `DeletedAt == null`.

- [ ] T025 [US2] [P] Utwórz `UpdateManagementGroupCommand.cs` + handler — waliduje właściciela (HTTP 403), waliduje unikalność nowej nazwy (HTTP 422), wywołuje `group.Update()`
- [ ] T026 [US2] [P] Utwórz `DeleteManagementGroupCommand.cs` + handler — waliduje właściciela (HTTP 403), wywołuje `group.Delete()` (soft delete)
- [ ] T027 [US2] [P] Utwórz `RestoreManagementGroupCommand.cs` + handler — waliduje właściciela (HTTP 403), waliduje że grupa jest usunięta (HTTP 422 jeśli aktywna), wywołuje `group.Restore()`
- [ ] T028 [US2] Zarejestruj handlery w `ManagementGroupsModule.Register()`
- [ ] T029 [US2] [P] Dodaj endpoint `PUT /management-groups/{id}` do `Expose()` — wymaga JWT, owner only
- [ ] T030 [US2] [P] Dodaj endpoint `DELETE /management-groups/{id}` do `Expose()` — wymaga JWT, owner only
- [ ] T031 [US2] [P] Dodaj endpoint `POST /management-groups/{id}/restore` do `Expose()` — wymaga JWT, owner only

**Checkpoint US2**: Pełny cykl: CREATE → UPDATE → DELETE → RESTORE działa poprawnie; nie-właściciel dostaje HTTP 403.

---

## Faza 5: US3 — Zarządzanie Członkami (P3)

**Cel**: Właściciel może dodawać/usuwać członków przez email; walidacje przypadków brzegowych.

**Niezależny Test**: POST `/management-groups/{id}/members` z emailem → HTTP 201; ponownie → HTTP 422; DELETE → HTTP 204.

- [ ] T032 [US3] [P] Utwórz `AddGroupMemberCommand.cs` + handler — waliduje format emaila (HTTP 422), szuka `UserReadModel` po emailu (HTTP 404 jeśli brak), waliduje że to nie właściciel (HTTP 422), waliduje że nie jest już członkiem (HTTP 422)
- [ ] T033 [US3] [P] Utwórz `RemoveGroupMemberCommand.cs` + handler — waliduje właściciela grupy (HTTP 403), waliduje że `UserId` != `OwnerId` (HTTP 403 — właściciel nietykalny), usuwa rekord `GroupMember`
- [ ] T034 [US3] Zarejestruj handlery w `ManagementGroupsModule.Register()`
- [ ] T035 [US3] [P] Dodaj endpoint `POST /management-groups/{id}/members` do `Expose()` — wymaga JWT, owner only; body: `{ "email": "..." }`
- [ ] T036 [US3] [P] Dodaj endpoint `DELETE /management-groups/{id}/members/{userId}` do `Expose()` — wymaga JWT, owner only

**Checkpoint US3**: Właściciel może dodać członka przez email; nie może dodać siebie; nie może usunąć siebie; nie-właściciel dostaje HTTP 403.

---

## Faza 6: US4 — Przeglądanie Grup (P4)

**Cel**: Zalogowany użytkownik może przeglądać swoje grupy i szczegóły każdej z nich.

**Niezależny Test**: GET `/management-groups` zwraca tylko aktywne grupy użytkownika; GET `/{id}` zwraca szczegóły z listą członków; soft-deleted zwraca HTTP 404.

- [ ] T037 [US4] [P] Utwórz `GetManagementGroupsQuery.cs` + handler — zwraca grupy gdzie `OwnerId == userId` LUB `GroupMember.UserId == userId`; pomija soft-deleted
- [ ] T038 [US4] [P] Utwórz `GetManagementGroupQuery.cs` + handler — zwraca szczegóły z listą członków; HTTP 404 jeśli nie istnieje lub soft-deleted; HTTP 403 jeśli requestujący nie jest właścicielem/członkiem
- [ ] T039 [US4] Zarejestruj handlery w `ManagementGroupsModule.Register()`
- [ ] T040 [US4] [P] Dodaj endpoint `GET /management-groups` do `Expose()` — wymaga JWT
- [ ] T041 [US4] [P] Dodaj endpoint `GET /management-groups/{id}` do `Expose()` — wymaga JWT

**Checkpoint US4**: Lista grup nie zawiera usuniętych; GET na usuniętą grupę zwraca HTTP 404; nieautoryzowany dostęp zwraca HTTP 403.

---

## Faza 7: Komunikacja z Modułem Users

**Cel**: ManagementGroups zbiera dane o użytkownikach przez zdarzenia, bez bezpośredniej referencji do Users.

- [ ] T042 Zweryfikuj że zdarzenie `UserSignedUp` (lub równoważne) jest zdefiniowane w `Shared.Abstractions`; jeśli nie — utwórz je tam
- [ ] T043 Utwórz `UserSignedUpHandler.cs` w `Core/Events/Handlers/` implementujący `IEventHandler<UserSignedUp>` — zapisuje `UserReadModel` (Id, Email) do bazy
- [ ] T044 Zarejestruj `UserSignedUpHandler` w `ManagementGroupsModule.Register()`
- [ ] T045 Zweryfikuj że moduł Users publikuje zdarzenie `UserSignedUp` przez `IMessageBroker` po rejestracji użytkownika

**Checkpoint F7**: Po rejestracji nowego użytkownika w module Users, rekord pojawia się w tabeli `user_read_models` modułu ManagementGroups.

---

## Faza 8: Finalizacja

- [ ] T046 [P] Zweryfikuj że Swagger pokazuje wszystkie 8 endpointów modułu ManagementGroups
- [ ] T047 [P] Smoke test pełnego flow: rejestracja → login → tworzenie grupy → dodanie członka → edycja → soft delete → restore
- [ ] T048 Zaktualizuj `specs/001-management-groups/plan.md` — oznacz ukończone fazy
- [ ] T049 Commit i push gałęzi `001-management-groups`

---

## Zależności Między Fazami

```
Faza 1 (Szkielet)
    └── Faza 2 (Domena + Persystencja)
            ├── Faza 3 (US1 - Tworzenie)      🎯 MVP
            ├── Faza 4 (US2 - Zarządzanie)
            ├── Faza 5 (US3 - Członkowie)     ← wymaga też Fazy 7
            ├── Faza 6 (US4 - Przeglądanie)
            └── Faza 7 (Events - Users)
Faza 8 (Finalizacja) ← po wszystkich powyższych
```

**Minimalny MVP**: Faza 1 + Faza 2 + Faza 3 = działający POST `/management-groups`
