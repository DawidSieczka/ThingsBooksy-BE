# Agentic Fleet Improvements — odroczone z P0 (2026-05-13)

## Jak uruchomić ten plik

W przyszłej sesji wystarczy promptem: **"Wykonaj zawartość pliku `specs/005-agentic-fleet-improvements.md`"**.

Plik jest samowystarczalny — zawiera pełny kontekst, decyzje, kroki implementacyjne i kryteria weryfikacji. Każda sekcja jest niezależna — można uruchomić je sekwencyjnie lub wybiórczo, w dowolnej kolejności (chyba że oznaczono inaczej).

---

## Context

Plan P0 (`~/.claude/plans/wykonaj-ze-swojego-opisu-glittery-stream.md`) naprawił 7 bugów blokujących heavy agentic work po migracji do monorepo: ścieżki API klienta FE, hardkody Windows, Vitest, sprzeczność @Input/input(), settings.local.json, redukcja CLAUDE.md do indeksu i tekst o migracjach. Plan świadomie pominął warstwę strukturalną i nowe agenty — ten plik je rozpisuje.

Po wykonaniu P0 fleet jest gotowy do uruchamiania greenfield features. Po wykonaniu tego pliku fleet zyskuje:
- automatyczne tworzenie szkieletu nowego modułu (BE) i nowej feature (FE) — bez ręcznej pracy
- pętlę naprawczą po quality-reviewer (symetryczną do architecture-guard)
- bramkę pre-impl dla FE (odpowiednik plan-validator)
- mniej duplikacji konwencji w pamięci (MEMORY.md)
- twardą egzekucję `dotnet format` przez hook (zamiast polegania na pamięci agenta)

Wszystkie zmiany są niezależne — mogą być wprowadzane w dowolnej kolejności i pojedynczo.

---

## Sekcja A — Nowy agent: `module-scaffolder` (BE) (WYKONANE 2026-05-13)

> **Status: DONE.** Plik agenta zapisany w `.claude/agents/module-scaffolder.md` (419 linii). Decyzje:
> - **Idempotencja:** fail-fast jeśli `backend/src/Modules/{Name}/` istnieje
> - **snake_case algorytm:** PascalCase → wstaw `_` przed każdą wielką literą poza pierwszą, lowercase całość (`ManagementGroups` → `management_groups`)
> - **Krok 7 usunięty:** Bootstrapper jest self-registering przez reflection (`ModuleLoader.LoadAssemblies` + assembly prefix). Scaffolder tylko zapewnia `.csproj` w `.slnx` + `module.{name-lower}.json` w `.Api/` z `<Content Include CopyToOutputDirectory>`.
> - **Lokalizacja IntegrationTests:** `backend/src/Modules/{Name}/{Name}.IntegrationTests/` (nie `backend/tests/...` jak sugerował brief)
> - **Pipeline:** auto-detection w orchestratorze (rule w CLAUDE.md dodana)
> - **Extensions.cs:** pełna struktura z `AddDataProviders + AddPostgres + AddOutbox + AddUnitOfWork`, bez handlerów (te dorzuca `module-writer`)
> - **WebAppFactory.cs patches:** `SchemasToInclude` array + `AddInMemoryCollection` z `["{name-flat}:module:enabled"] = "true"` (flat lowercase bez separatorów)
> - **CLAUDE.md update:** wiersz w `### Known agents` + reguła w `### Orchestration rules (compact)` przed `module-writer`

---


### Problem

`module-writer` zakłada, że projekty `.Api`, `.Core`, `.Migrations`, `.IntegrationTests`, wpis w `.slnx`, referencje NuGet, rejestracja w Bootstrapperze i schemat w `ThingsBooksyWebAppFactory.SchemasToInclude` **już istnieją**. Przy tworzeniu nowego modułu cały bootstrap jest ręczny — kilkadziesiąt minut pracy, w której łatwo pominąć krok.

### Cel

Agent, który dla nazwy modułu (np. `Bookings`) tworzy:
1. Foldery `backend/src/Modules/Bookings/ThingsBooksy.Modules.Bookings.Api/`, `.Core/`, `.Migrations/`, `backend/tests/Modules/Bookings/ThingsBooksy.Modules.Bookings.IntegrationTests/`
2. Cztery pliki `.csproj` z poprawnymi referencjami i `<RootNamespace>` (kopia istniejącego modułu, np. `Users` lub `Resources`)
3. Wpis w `backend/ThingsBooksy.slnx` dla wszystkich czterech projektów
4. Plik `module.bookings.json` w `backend/src/Bootstrapper/` (kopia istniejącego)
5. Rejestrację `BookingsModule` w `backend/src/Bootstrapper/.../Program.cs` lub `ModuleRegistration.cs` (zależnie od mechanizmu)
6. Schema `bookings` dodany do `SchemasToInclude` w `backend/src/Shared/ThingsBooksy.Shared.IntegrationTests/ThingsBooksyWebAppFactory.cs`
7. Szkielet `BookingsModule.cs` (klasa IModule z pustymi `Register()` i `Expose()`)
8. Szkielet `BookingsDbContext.cs` z `HasDefaultSchema("bookings")` (lub odpowiedni snake_case)
9. Szkielet `Extensions.cs` w `.Core/` z `InternalsVisibleTo` dla wszystkich 4 assembly + `DynamicProxyGenAssembly2`

### Kroki implementacyjne (przyszła sesja)

1. **Sesja z `agent-architect`** — przekazać brief:

   > "Zaprojektuj agenta `module-scaffolder` dla ThingsBooksy. Wejście: nazwa modułu w PascalCase (np. `Bookings`). Działa cwd = root repo. Czyta strukturę istniejącego modułu (np. `Resources` z `backend/src/Modules/Resources/`) jako wzorzec. Generuje: 4 projekty `.csproj`, foldery, wpis w `.slnx`, `module.{name}.json`, rejestrację w Bootstrapperze, dodaje schemat w `ThingsBooksyWebAppFactory.SchemasToInclude`, szkielet `IModule`, `DbContext` z `HasDefaultSchema`, `Extensions.cs` z `InternalsVisibleTo`. Po wygenerowaniu uruchamia `dotnet build backend/ThingsBooksy.slnx` i raportuje wynik. Narzędzia: Glob, Grep, Read, Write, Edit, Bash. Plik: `.claude/agents/module-scaffolder.md`."

2. **Po stworzeniu agenta**: zaktualizować CLAUDE.md (indeks agentów) i Orchestration rules:
   - Dodać orchestration rule: "Przed pierwszym uruchomieniem `module-writer` na nowym module, jeśli folder `backend/src/Modules/{Name}/` nie istnieje, najpierw wywołać `module-scaffolder`."

3. **Test scenariuszowy**: spróbować stworzyć pusty moduł `Tasks` (lub inny nie istniejący), zweryfikować `dotnet build` zielony.

### Pliki które agent musi modyfikować
- `backend/ThingsBooksy.slnx` (dodanie projektów)
- `backend/src/Bootstrapper/.../Program.cs` lub równoważnik (rejestracja modułu — Grep za `RegisterModule` lub podobnym, zależnie od architektury)
- `backend/src/Shared/ThingsBooksy.Shared.IntegrationTests/ThingsBooksyWebAppFactory.cs` (`SchemasToInclude`)
- Plus wszystkie nowe pliki w `backend/src/Modules/{Name}/` i `backend/tests/Modules/{Name}/`

### Weryfikacja
```bash
dotnet build backend/ThingsBooksy.slnx
```
Zielono. `Get-ChildItem backend/src/Modules/{NewName}` zwraca 3 projekty (Api, Core, Migrations). `Select-String "SchemasToInclude" -Path backend/src/Shared/.../ThingsBooksyWebAppFactory.cs` zawiera nowy schemat.

---

## Sekcja B — Nowy agent: `fe-route-writer` (FE) (WYKONANE 2026-05-13)

> **Status: DONE.** Plik agenta zapisany w `.claude/agents/fe-route-writer.md` (275 linii). Dodatkowo nowa konwencja `.claude/conventions/angular-routing.md` (227 linii). Decyzje:
> - **Const naming:** `{feature}Routes` w camelCase (`resourcesRoutes`, `managementGroupsRoutes`)
> - **Nested routes:** max depth 2, dalej fail-stop
> - **Missing components:** fail-stop z listą brakujących plików (no partial mode)
> - **Resolvers:** poza scope; agent raportuje "Resolvers: not implemented (manual step required)" gdy plan ich wymaga
> - **Konwencja first:** `angular-routing.md` napisana przed plikiem agenta; agent referuje konwencję jako authoritative
> - **CLAUDE.md update:** wiersz w `### Known agents` + reguła w `### Orchestration rules (compact)` po `fe-component-writer`; konwencja `angular-routing.md` dodana do tabeli `Frontend conventions`

---


### Problem

`html-extractor` generuje routing plan, `fe-component-writer` implementuje komponenty, ale **nikt nie pisze** `{feature}.routes.ts` ani nie aktualizuje `frontend/src/app/app.routes.ts`. Skutek: komponenty gotowe, feature nie dostępna z poziomu routera.

### Cel

Agent, który dla listy komponentów feature'a + planu routingu z `html-extractor`:
1. Tworzy `frontend/src/app/features/{feature}/{feature}.routes.ts` z lazy-loaded child routes
2. Aktualizuje `frontend/src/app/app.routes.ts` dodając lazy-loaded entry dla feature'a (`loadChildren: () => import('./features/{feature}/{feature}.routes').then(m => m.{FEATURE}_ROUTES)`)
3. Konwencja routingu zgodna z `.claude/conventions/angular-folder-structure.md` (lazy-load wszystkich features)

### Kroki implementacyjne

1. **Sesja z `agent-architect`** — brief:

   > "Zaprojektuj agenta `fe-route-writer` dla ThingsBooksy. Wejście: nazwa feature'a (np. `resources`) + tablica { path, componentName, type: smart|dumb, guards?: string[] } z planu html-extractora. Działa cwd = root repo. Czyta `.claude/conventions/angular-folder-structure.md`. Tworzy lub edytuje `frontend/src/app/features/{feature}/{feature}.routes.ts`. Aktualizuje `frontend/src/app/app.routes.ts` (lazy-load). Sprawdza, czy każdy referencjonowany komponent istnieje w `frontend/src/app/features/{feature}/` zanim doda do tras (jeśli nie — raportuje brak). Uruchamia `cd frontend && npx ng build` i raportuje. Narzędzia: Glob, Grep, Read, Write, Edit, Bash."

2. **Orchestration rule do CLAUDE.md** (lub `.claude/agents/README.md`):
   > "Po tym, jak wszyscy `fe-component-writer` dla feature'a w Wave zaraportują `Build: PASSED`, wywołać `fe-route-writer` dla tego feature'a. Przekazać listę komponentów + plan routingu z `html-extractor`."

3. **Test scenariuszowy**: po następnej sesji `fe-component-writer` (np. dla feature `resources`), wywołać agenta i sprawdzić, czy `app.routes.ts` zyskał wpis, a `ng build` jest zielony.

### Pliki które agent musi modyfikować
- `frontend/src/app/app.routes.ts` (dodanie lazy-loaded entry)
- `frontend/src/app/features/{feature}/{feature}.routes.ts` (nowy plik)

### Weryfikacja
```bash
cd frontend && npx ng build
```
Zielono. Nowy route widoczny w `app.routes.ts` jako lazy-loaded entry. Otwarcie URL-a w przeglądarce pokazuje komponent (manual smoke test).

---

## Sekcja C — Nowy agent: `fe-plan-validator` (FE pre-impl gate) (WYKONANE 2026-05-13)

> **Status: DONE.** Plik agenta zapisany w `.claude/agents/fe-plan-validator.md` (305 linii). Decyzje:
> - **Input format:** inline w prompcie od orchestratora (cały `HTML-EXTRACTOR COMPLETE` block jako tekst); brak zmian w `html-extractor`
> - **Missing resources:** halt-fast jak `plan-validator` (`api/` lub `_tokens.scss` brakuje → natychmiastowe NO-GO z "run fe-api-client-writer first / create _tokens.scss first")
> - **EXECUTION MAP ownership:** `html-extractor` jest właścicielem; walidator tylko weryfikuje spójność (CHECK 6) — żadnej drugiej mapy
> - **CHECK 3 (component spec):** lekkie sprawdzenie obecności pól (WARNING gdy brakuje), nie pełna walidacja (`html-extractor` jest gateway'em)
> - **Tryb:** interaktywny (jak `quality-reviewer`), model `claude-sonnet-4-6`. Jedno finding na raz. Akceptuje "Challenged (no resolution)".
> - **CLAUDE.md update:** wiersz w `### Known agents` między `html-extractor` a `fe-component-writer` + reguła w `### Orchestration rules (compact)` przed `fe-component-writer`

---


### Problem

Backend ma `plan-validator` jako bramkę przed `module-writer` — wykrywa cross-module dependencies, missing entities, niezgodne FR-traceability. **Frontend tej bramki nie ma.** `html-extractor` produkuje plan; `fe-component-writer` implementuje. Jeśli plan referencjonuje endpoint, którego nie ma w `frontend/src/app/api/`, lub token niezadeklarowany w `_tokens.scss`, agent dowiaduje się o tym dopiero w trakcie pisania komponentu.

### Cel

Agent, który dla `HTML-EXTRACTOR COMPLETE` block sprawdza:
1. Czy każdy endpoint w `API mapping` istnieje w `frontend/src/app/api/` (Grep service classes i method names)
2. Czy każdy token wymieniony w `Design tokens` jest zadeklarowany w `frontend/src/styles/_tokens.scss` (lub wymieniony jako "do dodania")
3. Czy każdy komponent w `Implementation order` ma zdefiniowany typ (smart/dumb), inputs, outputs, UI states
4. Czy ścieżka folderu dla każdego komponentu jest zgodna z konwencją `frontend/src/app/features/{feature}/...` lub `frontend/src/app/shared/components/...`
5. Czy nie ma duplikatów selektorów (`tb-*`)

Wynik: blok `FE-PLAN-VALIDATOR VERDICT` (GO / NO-GO) z listą BLOCKERs/WARNINGs analogiczną do `plan-validator`.

### Kroki implementacyjne

1. **Sesja z `agent-architect`** — brief:

   > "Zaprojektuj agenta `fe-plan-validator` jako frontendowy odpowiednik backendowego `plan-validator`. Wejście: blok `HTML-EXTRACTOR COMPLETE`. Read-only. Czyta `frontend/src/app/api/` (sprawdzenie endpointów), `frontend/src/styles/_tokens.scss` (tokens), istniejące `frontend/src/app/features/` i `shared/components/` (duplikaty selektorów). Wzorzec wyjścia: `## VERDICT`, `## ISSUES`, `## EXECUTION MAP` — komponenty pogrupowane w Wave'y wg zależności. Narzędzia: Glob, Grep, Read."

2. **Orchestration rule** (CLAUDE.md):
   > "Po tym, jak `html-extractor` zaraportuje `HTML-EXTRACTOR COMPLETE` i developer zatwierdzi plan, wywołać `fe-plan-validator`. Jeśli `VERDICT: NO-GO`, wstrzymać i poprawić plan. Jeśli `GO`, przekazać EXECUTION MAP do `fe-component-writer` jako kolejność implementacji."

3. **Test scenariuszowy**: dla istniejącego planu html-extractora, agent powinien znaleźć przynajmniej "tokens not declared" jeśli istnieje świeży plan z nowymi tokenami.

### Pliki które agent czyta (nigdy nie modyfikuje)
- `frontend/src/app/api/**/*.ts`
- `frontend/src/styles/_tokens.scss`
- `frontend/src/app/features/**/*.component.ts`
- `frontend/src/app/shared/components/**/*.component.ts`

### Weryfikacja
Uruchomienie agenta dla aktualnego planu html-extractora (jeśli istnieje) i potwierdzenie, że wykrywa znane wcześniej problemy (np. brakujące tokeny). Brak modyfikacji plików.

---

## Sekcja D — Repair loop dla `quality-reviewer`

### Problem

`architecture-guard` ma repair loop: jeśli BLOCKER nie został rozwiązany, automatycznie wywołuje `module-writer` z opisem violation, czeka na poprawkę, ponawia review. **`quality-reviewer` tej pętli nie ma** — gdy znajdzie BLOCKER, developer musi ręcznie zlecić poprawkę i ponownie uruchomić review. Niespójność z `architecture-guard`.

### Cel

Dodać do CLAUDE.md (lub `.claude/agents/README.md`) Orchestration rule:

> Po tym, jak `quality-reviewer` zaraportuje `QUALITY-REVIEWER COMPLETE` z `BLOCKERS > 0` (i bez `Challenged items`):
> 1. Dla każdego BLOCKER zidentyfikuj plik i zasadę z opisu finding'u.
> 2. Re-wywołaj `module-writer` przekazując: nazwę modułu + opis violation (zamiast task IDs).
> 3. Po raporcie `MODULE-WRITER COMPLETE`, jeśli `Schema changes != NONE` → `migration-agent`.
> 4. Re-wywołaj `quality-reviewer` z tymi samymi task IDs.
> 5. Cap: 2 iteracje. Jeśli po 2 nadal są BLOCKERs, wstrzymaj i zaraportuj do użytkownika.

### Kroki implementacyjne

1. Edytować `CLAUDE.md` w sekcji "Orchestration rule — quality-reviewer" (po Fix 6 — może to być `.claude/agents/README.md` jeśli zdecydowano się przenieść):
   - Dodać blok repair loop analogiczny do `architecture-guard` (linie 233–242 w obecnym CLAUDE.md).

2. **Nie wymaga zmian w pliku agenta** — `quality-reviewer.md` zostaje read-only; pętla jest po stronie orkiestratora (main session).

### Pliki do edycji
- `CLAUDE.md` (lub `.claude/agents/README.md` po reorganizacji P0)

### Weryfikacja
Następna sesja, w której quality-reviewer znajdzie BLOCKER, powinna pokazać orkiestratora automatycznie wywołującego module-writer z violation description.

---

## Sekcja E — Pre-commit hook dla `dotnet format` (WYKONANE 2026-05-13)

> **Status: backend hook done, frontend prettier deferred.**
> Husky.NET 0.9.1 zainstalowany, `.config/dotnet-tools.json` z `husky`, `.husky/pre-commit` woła `dotnet husky run`, `.husky/task-runner.json` ma task `format-backend-staged` (uruchamia `dotnet format --include ${staged}` na staged `.cs` w `backend/`) + `restage-formatted` (git add po formacie). Frontend prettier hook nie został dodany — wymaga cross-shell wrappera (bash/pwsh) który czyta staged `.ts/.html/.scss` i wywołuje `frontend/node_modules/.bin/prettier`. Do zrobienia osobno: `tools/format-frontend.{sh,ps1}` + drugi task w `task-runner.json`.

---


### Problem

CLAUDE.md i `git` skill mówią: "Run `dotnet format` before suggesting any commit". To wymóg w pamięci — gdy agent zapomni, niesformatowany kod ląduje w commit'cie. CI łapie to (`dotnet format --verify-no-changes`), ale dopiero po push'u. Hook gitowy załatwiłby to lokalnie i automatycznie.

### Cel

Pre-commit hook (Husky.NET lub native git hook), który:
1. Wykrywa, czy w stage'u są `.cs` files w `backend/`
2. Jeśli tak: uruchamia `dotnet format backend/ThingsBooksy.slnx --include $(git diff --cached --name-only --diff-filter=ACMR | grep '\.cs$')`
3. Re-stage'uje sformatowane pliki
4. Pozwala commit'owi przejść tylko jeśli format kończy bez błędów

Analogicznie dla `frontend/`: hook na `.ts/.html/.scss` files uruchamiający `prettier --write`.

### Kroki implementacyjne

1. **Decyzja techniczna**: Husky.NET (dotnet tool) vs natywny git hook vs pre-commit framework (Python). Rekomendacja: **Husky.NET** — naturalnie integruje się z `dotnet` ecosystem, nie wymaga Pythona.

2. **Setup Husky.NET**:
   ```bash
   dotnet new tool-manifest # jeśli nie ma
   dotnet tool install Husky
   dotnet husky install
   dotnet husky add pre-commit -c "dotnet format backend/ThingsBooksy.slnx --include-private --verify-no-changes || (dotnet format backend/ThingsBooksy.slnx && git add -u && exit 1)"
   ```

3. **Hook FE**: `dotnet husky add pre-commit` może mieć multi-step config. Dodać entry dla `frontend/`:
   ```bash
   cd frontend && npx prettier --write $(git diff --cached --name-only --diff-filter=ACMR | grep -E '\.(ts|html|scss)$')
   ```

4. **Commit hook config**: plik `.husky/pre-commit` lub `.husky/task-runner.json` (zależnie od wersji).

5. **Update CLAUDE.md** (komendy): zmienić "Run dotnet format backend/ThingsBooksy.slnx before suggesting any commit" → "Format is enforced by pre-commit hook (Husky.NET); agents nie muszą wywoływać explicitly".

6. **Update agentów**: w `module-writer.md`, `integration-test-writer.md`, `fe-component-writer.md` — usunąć wymóg ręcznego `dotnet format` (Phase 3 / Phase 4). Zostawić `dotnet build` (sprawdzanie kompilacji to nie to samo co formatowanie).

### Pliki do edycji / dodać
- `.config/dotnet-tools.json` (Husky.NET)
- `.husky/pre-commit` (skrypt)
- `.husky/task-runner.json` (jeśli Husky.NET używa tego mechanizmu)
- `CLAUDE.md` (komendy)
- `.claude/agents/module-writer.md` (usunąć `dotnet format` z Phase 3)
- `.claude/agents/integration-test-writer.md` (usunąć `dotnet format`)
- `.claude/agents/fe-component-writer.md` (usunąć wzmianki o ręcznym formacie)

### Weryfikacja
1. Wprowadź ręczną niespójność formatowania w pliku `.cs`. `git commit` powinien sam to naprawić lub odrzucić.
2. To samo dla `.ts` w frontend.

---

## Sekcja F — CLAUDE.md jako "indeks" — głębszy poziom dedup'u

### Problem

P0 zredukował CLAUDE.md do ~80 linii (indeks + 3 hard rules + brief commands + tech stack). To wciąż zawiera **Orchestration rules** w postaci zwartego bloku ~25 linii. Te reguły są też w description front-matter każdego agenta. Drugorzędna duplikacja: każdy agent ma sekcję "When to delegate" w swoim description, a CLAUDE.md powtarza to w skrócie w tabeli `### Known agents`.

### Cel

Po P0, jeśli okaże się, że duplikacja "tabela agentów w CLAUDE.md vs description w agent files" generuje drift, zrobić jeden z dwóch ruchów:

**Opcja A — single source of truth w agent files**:
- Usunąć tabelę `### Known agents` z CLAUDE.md.
- Wprowadzić w CLAUDE.md tylko link: "Agent fleet → `.claude/agents/` (read individual files; description frontmatter = źródło prawdy)".
- Dodać do `agent-architect` zadanie: po stworzeniu nowego agenta automatycznie wygenerować/aktualizować `.claude/agents/INDEX.md` (zbiorczy snapshot).

**Opcja B — auto-generated index**:
- Hook (pre-commit lub osobny skrypt) skanuje `.claude/agents/*.md`, czyta `description:` z frontmatter, generuje `.claude/agents/INDEX.md`. CLAUDE.md linkuje tylko do INDEX.md.

### Kroki implementacyjne

1. Wybrać Opcję A lub B (B jest bezobsługowa, ale wymaga skryptu).
2. Dla **Opcji A**: edycja CLAUDE.md, brief dla `agent-architect`.
3. Dla **Opcji B**: napisać skrypt PowerShell `tools/generate-agent-index.ps1` (parsuje YAML frontmatter wszystkich `.claude/agents/*.md` i generuje `.claude/agents/INDEX.md`). Dodać wywołanie do pre-commit hook'a (Sekcja E).

### Pliki do edycji
- `CLAUDE.md`
- (opcja B) `tools/generate-agent-index.ps1` (nowy)
- (opcja B) `.husky/pre-commit` (dodanie wywołania skryptu)
- (opcja A) `.claude/agents/agent-architect.md` (rozszerzenie o aktualizację indeksu)

### Weryfikacja
Po dodaniu nowego agenta przez `agent-architect`, pojawia się w `.claude/agents/INDEX.md` bez ręcznej edycji CLAUDE.md.

---

## Sekcja G — Czyszczenie MEMORY.md

### Problem

`~/.claude/projects/.../memory/MEMORY.md` zawiera wpisy, które są skopiowane z `.claude/conventions/`:
- `Architectural Conventions` (project_conventions.md)
- `Domain Entity Design` (domain_entity_design.md)
- `Command/Query/Handler Naming` (project_conventions.md)
- `DataProvider Pattern` (project_conventions.md)
- `DataProvider Query Syntax` (data-provider-query-syntax.md)

To duplikat — konwencje są źródłem prawdy, MEMORY ma pamiętać preferencje/kontekst, nie reguły kodu. Drift gwarantowany.

### Cel

Wyczyścić MEMORY.md z wpisów dublujących konwencje. Zachować:
- Preferencje (np. agent design preferences)
- Plan i status projektu (Agent Fleet Plan, Monorepo Migration Plan)
- Feedback (deferred-work files pattern, EF Core query filter pattern)

### Kroki implementacyjne

1. Otworzyć `~/.claude/projects/C--Users-dsieczka-Desktop-github-ThingsBooksy/memory/MEMORY.md`.
2. Usunąć wpisy: `Architectural Conventions`, `Domain Entity Design`, `Command/Query/Handler Naming`, `DataProvider Pattern`, `DataProvider Query Syntax` — wraz z plikami w katalogu memory/.
3. Pozostawić: `Agent Fleet Plan`, `Agent Design Preferences`, `EF Core query filter pattern` (to specyficzny gotcha, nie konwencja), `Monorepo Migration Plan`, `Deferred-work files pattern`.
4. Dodać krótki wpis `Conventions reference` z pojedynczym linkiem do `.claude/conventions/` jako wskazówka "tam jest źródło prawdy o regułach kodu".

### Pliki do edycji
- `~/.claude/projects/C--Users-dsieczka-Desktop-github-ThingsBooksy/memory/MEMORY.md`
- Pliki w tym samym folderze — usunąć: `project_conventions.md`, `domain_entity_design.md`, `data-provider-query-syntax.md`

### Weryfikacja
MEMORY.md zawiera < 8 wpisów. Każdy wpis to preferencja, plan lub gotcha — żaden nie powtarza treści konwencji.

---

## Sekcja H — Likwidacja stale worktrees

### Problem

`.claude/worktrees/` zawiera trzy katalogi: `agent-acbaee79ebb071723`, `agent-a221b37034e575054`, `agent-a787222c01f2976a5`. Hash w nazwie sugeruje, że to artefakty po runach agentów z `isolation: "worktree"`. Nie wiadomo, czy aktywne ani co zawierają.

### Cel

Sprawdzić status każdej worktree i usunąć martwe.

### Kroki implementacyjne

```powershell
git worktree list
```

Dla każdej worktree pod `.claude/worktrees/`:
1. Jeśli nie ma jej w `git worktree list` — usunąć folder ręcznie.
2. Jeśli jest, ale gałąź jest zmerge'owana z main — `git worktree remove <path>`.
3. Jeśli jest aktywna (uncommitted changes lub niemerge'owana gałąź) — zostawić, zapytać użytkownika.

### Weryfikacja
```powershell
Get-ChildItem .claude/worktrees/ -Directory
```
Wyświetla tylko aktywne worktrees (lub jest pusty). `git worktree list` jest spójny z zawartością folderu.

---

## Sekcja I — Kolejność wykonania (sugerowana)

1. **G — MEMORY.md cleanup** (najmniej ryzykowne, czysto pamięciowe)
2. **H — Stale worktrees** (czyszczenie środowiska)
3. **D — Repair loop dla quality-reviewer** (5 linii w CLAUDE.md, brak nowych plików)
4. **E — Pre-commit hook** (twarda automatyka, eliminuje klasę błędów)
5. **A — module-scaffolder agent** (wymaga sesji agent-architect)
6. **B — fe-route-writer agent** (wymaga sesji agent-architect)
7. **C — fe-plan-validator agent** (wymaga sesji agent-architect)
8. **F — CLAUDE.md głębszy dedup** (najostatniej — po nabraniu pewności że pozostałe zmiany się stabilizowały)

Sekcje A, B, C są niezależne — można je robić w dowolnej kolejności lub równolegle.

---

## Linkowane dokumenty

- Plan P0: `~/.claude/plans/wykonaj-ze-swojego-opisu-glittery-stream.md`
- Monorepo migration plan: `specs/004-monorepo-migration/`
- Konstytucja architektury: `.specify/memory/constitution.md`
- Konwencje BE i FE: `.claude/conventions/`
- Agent fleet: `.claude/agents/`
