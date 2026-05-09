# Specyfikacja: Testy Integracyjne

**Gałąź**: `002-integration-tests`
**Utworzono**: 2026-04-23
**Status**: Zatwierdzona

---

## Decyzje

| # | Pytanie | Decyzja |
|---|---------|---------|
| P1 | Baza danych | ✅ **Testcontainers** (PostgreSQL) |
| P2 | Czyszczenie DB | ✅ **Respawn** |
| P3 | Testowanie HTTP | ✅ **WebApplicationFactory** |
| P4 | Struktura projektów | ✅ **Shared infra + per-moduł projekty** |
| P5 | Uwierzytelnianie | ✅ **Przez endpoint sign-up/sign-in** |
| P6 | Framework | ✅ **xUnit** |
| P7 | Zakres | ✅ **Users + ManagementGroups** |
| P8 | Nazewnictwo | ✅ **`Endpoint_Scenario_ExpectedResult`** |
| P9 | Dane testowe | ✅ **SubmoduleClient pattern** (patrz niżej) |
| P10 | CI/CD | ⏸ **Tylko lokalnie** (brak GitHub Actions na razie) |

---

## Kontekst

Projekt ThingsBooksy jest modularnym monolitem z modułami `Users` i `ManagementGroups`.
Testy integracyjne mają weryfikować pełne flow HTTP → Handler → Baza danych — bez mockowania.



> 💡 **Moja rekomendacja: C — Tak, ale oddzielny job**
>
> GitHub Actions `ubuntu-latest` ma Dockera — Testcontainers działa out-of-the-box.
> Oddzielny job `integration-tests` nie blokuje szybkiego `build` job-a i można go
> uruchamiać selektywnie (np. tylko na PR do `main`). Dobra praktyka w CI.

**Twoja odpowiedź:** ___

---

## Wzorzec SubmoduleClient (P9)

Każdy moduł ma własną klasę `<Moduł>TestClient` która udostępnia dwie kategorie metod:

- **HTTP methods** — wywołania przez `HttpClient` (używane w `Arrange` i `Act`)
- **DB methods** — bezpośredni dostęp przez `DbContext` (używane wyłącznie w `Assert`)

Test ma pełną kontrolę nad setupem — świadomie decyduje co wołać, w jakiej kolejności.

```csharp
// Negative path — test celowo nie rejestruje usera
[Fact]
public async Task AddMember_WhenUserNotExists_Returns404()
{
    // Arrange — tylko owner, brak "member@test.com" w systemie
    var owner = await _users.RegisterAndLoginAsync("owner@test.com");
    var groupId = await _groups.CreateGroupAsync(owner.Client, "My Group"); // HTTP POST

    // Act
    var response = await _groups.AddMemberAsync(owner.Client, groupId, "nonexistent@test.com"); // HTTP POST

    // Assert — HTTP status
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    // Assert — bezpośrednio z DB (nie przez GET endpoint)
    var members = await _groups.GetMembersFromDbAsync(groupId); // EF Core query
    Assert.Empty(members);
}
```

---

## Wymagania Niefunkcjonalne

- **NF-001**: Każdy test musi startować z czystą bazą danych (Respawn przed każdym testem)
- **NF-002**: Testy muszą być niezależne od kolejności wykonania
- **NF-003**: Jeden kontener PostgreSQL per kolekcja testów (nie per test) — wydajność
- **NF-004**: `dotnet test` musi zakończyć się pomyślnie lokalnie przed każdym commitem

---

## Pokrycie — Scenariusze z spec.md ManagementGroups

Każdy scenariusz akceptacyjny z `specs/001-management-groups/spec.md` ma odpowiadający test integracyjny:

### Historia 1 — Tworzenie grupy
- `CreateGroup_WithValidData_Returns201AndPersistsInDb`
- `CreateGroup_WhenUnauthenticated_Returns401`
- `CreateGroup_WithoutName_Returns422`

### Historia 2 — Zarządzanie grupą
- `UpdateGroup_AsOwner_Returns200AndUpdatesDb`
- `UpdateGroup_AsNonOwner_Returns403`
- `DeleteGroup_AsOwner_Returns204AndSoftDeletesInDb`
- `RestoreGroup_AsOwner_Returns200AndClearsDeletedAt`
- `RestoreGroup_WhenNotDeleted_Returns422`

### Historia 3 — Członkowie
- `AddMember_WithValidEmail_Returns201AndPersistsInDb`
- `AddMember_WhenUserNotExists_Returns404`
- `AddMember_WhenAlreadyMember_Returns422`
- `AddMember_AsNonOwner_Returns403`
- `RemoveMember_AsOwner_Returns204AndRemovesFromDb`

### Historia 4 — Przeglądanie grup
- `GetGroups_ReturnsOnlyUserGroups_Returns200`
- `GetGroup_WhenMember_Returns200WithMembers`
- `GetGroup_WhenNotMember_Returns403`

### Przypadki brzegowe
- `AddMember_WhenOwnerAddsHimself_Returns422`
- `GetGroup_WhenSoftDeleted_Returns404`
- `AddMember_WithInvalidEmailFormat_Returns422`
- `CreateGroup_WithDuplicateName_Returns422`

### Cross-module — Users → ManagementGroups event
- `SignUp_CreatesUserReadModelInManagementGroupsSchema`

---

## Moje sugestie — Najlepsze praktyki (bez pytań)

Poniższe uważam za oczywiste dla tego projektu — jeśli się nie zgadzasz, daj znać:

1. **`CollectionFixture` dla WebApp** — jedna instancja `WebApplicationFactory` na kolekcję testów (nie per test) — 10x szybsze uruchamianie
2. **Separate `appsettings.Testing.json`** — nadpisuje connection string, wyłącza rate limiting
3. **`HttpClient` per test z osobnym userem** — każdy test tworzy własne konto przez `/users/sign-up` żeby testy nie kolidowały
4. **Assert na bazie danych też** — po `POST /management-groups` nie tylko sprawdzamy HTTP 201, ale też że rekord istnieje w DB (przez SubmoduleClient)
5. **`dotnet test` jako gate** — żaden commit nie wchodzi bez zielonych testów (lokalnie)

