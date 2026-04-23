# Specyfikacja Funkcjonalności: Moduł ManagementGroups

**Gałąź**: `001-management-groups`
**Utworzono**: 2026-04-23
**Status**: Szkic

---

## Scenariusze Użytkownika i Testy *(obowiązkowe)*

### Historia Użytkownika 1 – Tworzenie Grupy Zarządzania (Priorytet: P1)

Jako zalogowany użytkownik chcę móc utworzyć własną Grupę Zarządzania (ManagementGroup),
aby mieć dedykowaną przestrzeń do zarządzania zasobami aplikacji.
Po utworzeniu grupy automatycznie staję się jej właścicielem z pełnymi uprawnieniami.

**Dlaczego ten priorytet**: Fundament całego modułu — bez możliwości tworzenia grupy
żadne inne funkcjonalności nie mają sensu.

**Niezależny Test**: Można przetestować samodzielnie wysyłając żądanie POST /management-groups
z ważnym tokenem JWT i weryfikując, że grupa pojawia się w bazie z poprawnym właścicielem.

**Scenariusze Akceptacyjne**:

1. **Dane**: Zalogowany użytkownik z ważnym JWT, **Gdy** wyśle POST /management-groups z nazwą i opisem, **Wtedy** grupa zostaje utworzona, użytkownik jest jej właścicielem, status HTTP 201
2. **Dane**: Niezalogowany użytkownik, **Gdy** spróbuje wysłać POST /management-groups, **Wtedy** otrzyma HTTP 401
3. **Dane**: Zalogowany użytkownik, **Gdy** wyśle żądanie bez wymaganej nazwy grupy, **Wtedy** otrzyma HTTP 422 z opisem błędu walidacji

---

### Historia Użytkownika 2 – Zarządzanie Grupą (edycja, usunięcie, przywrócenie) (Priorytet: P2)

Jako właściciel grupy chcę móc edytować jej dane (nazwę, opis), usunąć ją (soft delete)
oraz w razie pomyłki przywrócić usuniętą grupę.

**Dlaczego ten priorytet**: Podstawowe operacje CRUD na grupie; soft delete chroni przed
przypadkową utratą danych.

**Niezależny Test**: Wysłać PUT /management-groups/{id}, następnie DELETE /management-groups/{id},
zweryfikować że rekord w bazie ma `DeletedAt != null`, następnie POST /management-groups/{id}/restore
i zweryfikować że rekord wrócił do stanu aktywnego.

**Scenariusze Akceptacyjne**:

1. **Dane**: Właściciel grupy, **Gdy** wyśle PUT /management-groups/{id} z nowymi danymi, **Wtedy** dane grupy zostają zaktualizowane, HTTP 200
2. **Dane**: Inny użytkownik (nie właściciel), **Gdy** spróbuje edytować grupę, **Wtedy** otrzyma HTTP 403
3. **Dane**: Właściciel grupy, **Gdy** wyśle DELETE /management-groups/{id}, **Wtedy** grupa dostaje soft delete (`DeletedAt` ustawione), HTTP 204
4. **Dane**: Właściciel, **Gdy** wyśle POST /management-groups/{id}/restore na usuniętą grupę, **Wtedy** `DeletedAt` zostaje wyczyszczone, HTTP 200
5. **Dane**: Właściciel, **Gdy** spróbuje przywrócić grupę która nie była usunięta, **Wtedy** otrzyma HTTP 422

---

### Historia Użytkownika 3 – Dodawanie i Usuwanie Członków (Priorytet: P3)

Jako właściciel grupy chcę dodawać użytkowników do swojej grupy podając ich adres email
oraz usuwać ich z grupy. Tylko ja mogę zarządzać członkostwem.

**Dlaczego ten priorytet**: Rozszerza grupę o aspekt społeczny, ale wymaga działającego
modułu Users i komunikacji między modułami (event-driven).

**Niezależny Test**: POST /management-groups/{id}/members z emailem istniejącego użytkownika
→ weryfikacja pojawienia się rekordu członkostwa; DELETE /management-groups/{id}/members/{userId}
→ weryfikacja usunięcia rekordu.

**Scenariusze Akceptacyjne**:

1. **Dane**: Właściciel grupy, **Gdy** wyśle POST /management-groups/{id}/members z poprawnym emailem istniejącego użytkownika, **Wtedy** użytkownik zostaje dodany do grupy, HTTP 201
2. **Dane**: Właściciel grupy, **Gdy** poda email nieistniejącego użytkownika, **Wtedy** otrzyma HTTP 404
3. **Dane**: Właściciel grupy, **Gdy** poda email użytkownika który już jest członkiem, **Wtedy** otrzyma HTTP 422
4. **Dane**: Inny członek grupy (nie właściciel), **Gdy** spróbuje dodać kogoś do grupy, **Wtedy** otrzyma HTTP 403
5. **Dane**: Właściciel, **Gdy** wyśle DELETE /management-groups/{id}/members/{userId}, **Wtedy** użytkownik zostaje usunięty z grupy, HTTP 204

---

### Historia Użytkownika 4 – Przeglądanie Swoich Grup (Priorytet: P4)

Jako zalogowany użytkownik chcę widzieć listę wszystkich grup, których jestem właścicielem
lub członkiem, wraz ze szczegółami każdej z nich.

**Dlaczego ten priorytet**: Funkcjonalność read-only, wartościowa ale drugorzędna wobec
operacji zapisu.

**Niezależny Test**: GET /management-groups → lista grup użytkownika;
GET /management-groups/{id} → szczegóły jednej grupy z listą członków.

**Scenariusze Akceptacyjne**:

1. **Dane**: Zalogowany użytkownik posiadający 3 grupy, **Gdy** wyśle GET /management-groups, **Wtedy** otrzyma listę swoich 3 grup (bez usuniętych), HTTP 200
2. **Dane**: Zalogowany użytkownik, **Gdy** wyśle GET /management-groups/{id} dla grupy do której należy, **Wtedy** otrzyma pełne dane grupy z listą członków, HTTP 200
3. **Dane**: Zalogowany użytkownik, **Gdy** wyśle GET /management-groups/{id} dla grupy do której nie należy, **Wtedy** otrzyma HTTP 403

---

### Przypadki Brzegowe

- Właściciel nie może dodać siebie jako członka → **HTTP 422** ("Owner is already a member of this group")
- Właściciel nie może być usunięty z grupy przez nikogo → **HTTP 403** (właściciel nie figuruje w tabeli members)
- Soft-deleted grupa jest niewidoczna dla wszystkich zapytań GET → **HTTP 404** (jakby nie istniała)
- Email w błędnym formacie przy dodawaniu członka → **HTTP 422** (walidacja po stronie API, przed zapytaniem do bazy)
- Nazwy grup są unikalne **globalnie** (w obrębie wszystkich użytkowników) → duplikat nazwy zwraca **HTTP 422**

---

## Wymagania *(obowiązkowe)*

### Wymagania Funkcjonalne

- **FR-001**: System MUSI umożliwiać zalogowanemu użytkownikowi tworzenie grup zarządzania
- **FR-002**: Twórca grupy MUSI automatycznie zostać jej właścicielem
- **FR-003**: Właściciel MUSI mieć wyłączność na edycję i usunięcie grupy
- **FR-004**: Usunięcie grupy MUSI być realizowane jako soft delete (pole `DeletedAt`)
- **FR-005**: System MUSI umożliwiać przywrócenie soft-deleted grupy przez właściciela
- **FR-006**: Właściciel MUSI móc dodawać członków wyłącznie przez podanie adresu email
- **FR-007**: Właściciel MUSI mieć wyłączność na zarządzanie członkostwem w grupie
- **FR-008**: System MUSI walidować czy podany email należy do istniejącego użytkownika
- **FR-009**: Użytkownik MUSI móc posiadać wiele grup jednocześnie
- **FR-010**: Lista grup zalogowanego użytkownika NIE MOŻE zawierać soft-deleted grup
- **FR-011**: Niezalogowani użytkownicy NIE MOGĄ uzyskać dostępu do żadnego endpointu modułu

### Kluczowe Encje

- **ManagementGroup**: Główna encja — `Id`, `Name`, `Description`, `OwnerId` (UserId), `CreatedAt`, `UpdatedAt`, `DeletedAt`
- **GroupMember**: Relacja — `GroupId`, `UserId`, `JoinedAt`; właściciel NIE jest duplikowany w tej tabeli

---

## Kryteria Sukcesu *(obowiązkowe)*

- **SC-001**: Użytkownik może przejść pełny flow: rejestracja → logowanie → tworzenie grupy → dodanie członka → usunięcie grupy → przywrócenie
- **SC-002**: Wszystkie endpointy wymagające autoryzacji zwracają HTTP 401 dla niezalogowanych i HTTP 403 dla nieuprawnionych
- **SC-003**: Soft delete działa poprawnie — usunięta grupa nie pojawia się na listach, ale można ją przywrócić
- **SC-004**: Swagger dokumentuje wszystkie endpointy modułu ManagementGroups
- **SC-005**: Moduł nie zawiera bezpośrednich referencji do projektów modułu Users

---

## Założenia

- Uwierzytelnianie i rejestracja użytkowników są realizowane przez istniejący moduł Users
- ManagementGroups komunikuje się z Users przez zdarzenia (`IMessageBroker`) lub read model — nie przez bezpośrednie referencje
- Moduł użyje własnego EF Core DbContext ze schematem `management_groups`
- Aplikacja w momencie implementacji tego modułu działa w Dockerze z PostgreSQL
- Soft delete nie kaskaduje na członków grupy — po przywróceniu grupy, lista członków pozostaje nienaruszona
- Właściciel nie figuruje w tabeli `GroupMember` — jego tożsamość określa pole `OwnerId` w `ManagementGroup`
