# Decyzja: migracja do monorepo (BE + Angular FE)

## Kontekst

Projekt ThingsBooksy to modularny monolit .NET 10 z rozbudowanym fleetem agentów Claude Code
(product-strategist → speckit → plan-validator → module-writer → migration-agent →
quality-reviewer → integration-test-writer → architecture-guard).

Frontend (Angular) jeszcze nie istnieje. Pytanie: jak zorganizować dwa stacki i agentów?

## Odrzucone alternatywy

### ThingsBooksy-Orchestrator (repo pośrednie)
Osobne repo z agentami "biznesowymi" nad BE i FE, z BE i FE jako podfolderami w `.gitignore`.

**Dlaczego odrzucone:**
- Claude Code agent ma `cwd` przypiętny do jednego repo — nie może wywołać agentów z podfolderu
- Orchestrator nie miałby dostępu do kodu (BE/FE w `.gitignore`) — tylko raporty tekstowe od subagentów
- Nested repos bez traceability: `git clone` daje puste foldery `backend/` i `frontend/`
- Trzy repozytoria do utrzymania bez żadnej przewagi nad monorepo
- Hybrydowe rozwiązanie gorsze od każdej czystej alternatywy

### Dwa osobne repozytoria + ręczna synchronizacja OpenAPI
- Przy każdej ficzerze wymagającej zmian w BE i FE: ręczne kopiowanie `swagger.json` do FE sesji
- Brak atomic commitów cross-stack
- Użytkownik staje się Orchestratorem — tarcie przy każdym feature'rze

## Wybrane rozwiązanie: monorepo

Jeden git repo, dwa katalogi `backend/` i `frontend/`.

### Dlaczego monorepo wygrywa

| Kryterium | Monorepo | Dwa repo |
|---|---|---|
| Dostęp agenta do całego kodu | Pełny przez Read/Glob/Grep | Tylko w obrębie własnego repo |
| Atomic commit BE + FE | Tak | Nie |
| Synchronizacja kontraktu API | Automatyczna (jeden repo) | Ręczna (kopiowanie swagger.json) |
| Konfiguracja fleet agentów | Jeden .claude/agents/ | Dwa oddzielne, brak komunikacji |
| Onboarding | git clone = cały projekt | Trzy clone + ręczny setup |
| Historia git | Jeden timeline | Rozjeżdża się bez submodules |

### Wady monorepo

- CI/CD wymaga filtrowania (uruchamiaj tylko BE albo tylko FE build przy zmianie w danym katalogu)
- `git log` miesza commity BE i FE — trzeba używać `git log backend/` lub `git log frontend/`
- Pierwszy setup frontendu (Angular) wymaga konfiguracji monorepo tooling (opcjonalnie Nx lub Turborepo dla zaawansowanych przypadków — niepotrzebne na start)

## Docelowa struktura katalogów

```
ThingsBooksy/
├── backend/
│   ├── src/
│   │   ├── Bootstrapper/
│   │   ├── Modules/
│   │   └── Shared/
│   └── ThingsBooksy.slnx
├── frontend/
│   ├── src/
│   ├── angular.json
│   └── package.json
├── docker-compose.yml          ← obsługuje oba serwisy (context: ./backend)
├── .claude/
│   └── agents/                 ← jeden fleet dla całego repo
│       ├── module-writer.md    ← istniejące BE agenty bez zmian
│       ├── migration-agent.md
│       ├── ...
│       ├── fe-component-writer.md    ← nowe FE agenty z prefiksem fe-
│       └── fe-api-client-writer.md
├── .specify/                   ← zostaje w root
├── specs/                      ← zostaje w root
├── CLAUDE.md                   ← zostaje w root, wymaga aktualizacji ścieżek
└── .gitignore                  ← rozszerzony o Node/Angular
```

## Plan migracji — kroki

### Etap 0: Przygotowanie (PRZED jakimkolwiek ruchem)
1. Zmerge'ować lub zamknąć aktywny branch `003-resource-schema`
   (alternatywnie: rebase po migracji, ale to ryzykowniejsze)
2. `git checkout main`
3. `git tag pre-monorepo-migration`  ← punkt powrotu

### Etap 1: Reorganizacja BE (jeden atomic commit)
```powershell
git mv src backend/src
git mv ThingsBooksy.slnx backend/ThingsBooksy.slnx
# jeśli istnieje: git mv Dockerfile backend/Dockerfile
```

W tym samym commicie zaktualizować:
- `docker-compose.yml`: `context: .` → `context: ./backend`
- `CLAUDE.md`: wszystkie ścieżki dotnet (`dotnet build` → `dotnet build backend/`)
- `.specify/memory/constitution.md`: ścieżki EF migrations (`src/Modules/...` → `backend/src/Modules/...`)

### Etap 2: Frontend scaffold
```powershell
ng new thingsbooksy-app --directory frontend --routing true --style scss
```

### Etap 3: Rozszerzenie docker-compose
Dodać serwis `frontend` (ng serve lub nginx) do `docker-compose.yml`.

### Etap 4: Rebase feature brancha (jeśli nie był zmerge'owany)
```powershell
git checkout 003-resource-schema
git rebase main
```
Git przenosi zmiany na nowe ścieżki dzięki `git mv` — spodziewaj się manualnych
konfliktów w plikach modyfikowanych przez branch.

## Ryzyka migracji

| Ryzyko | Waga | Mitygacja |
|---|---|---|
| Konflikty przy rebase `003-resource-schema` | Wysoka | Zmerge'ować branch przed migracją |
| Zepsute ścieżki w CLAUDE.md | Średnia | Aktualizacja w tym samym commicie co git mv |
| docker-compose context | Średnia | Zmienić na `context: ./backend` |
| Ścieżki EF migrations w CLAUDE.md i constitution.md | Niska | Aktualizacja w Etapie 1 |
| Wewnętrzne ścieżki ThingsBooksy.slnx | Brak | Relative do lokalizacji pliku — bez zmian |
| Namespace C# | Brak | Bez zmian |

## Co się NIE zmienia

- Historia git (zachowana przez `git mv`)
- `.specify/`, `specs/`, `.github/` (zostają w root)
- Wewnętrzne ścieżki w `ThingsBooksy.slnx`
- Namespace'y C#
- Connection strings, konfiguracja PostgreSQL
- Konwencja nazewnicza istniejących agentów BE

## Następne kroki po migracji

1. Dodać agenty FE (`fe-component-writer`, `fe-api-client-writer`, `html-extractor`)
   — patrz wcześniejsza dyskusja o `html-extractor`
2. Zdefiniować granicę BE/FE: `swagger.json` generowany przez Swashbuckle jako
   source of truth dla typów klienta Angular
3. Opcjonalnie dodać `ui-critic` (ocena UX mockupów) i `frontend-reviewer` (code review Angular)
