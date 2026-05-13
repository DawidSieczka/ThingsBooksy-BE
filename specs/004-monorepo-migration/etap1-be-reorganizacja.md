# Etap 1 — Reorganizacja BE do struktury monorepo

## Kontekst

Projekt ThingsBooksy przechodzi z layoutu single-stack do monorepo z backendem (.NET) i frontendem (Angular).
Ten plik opisuje wyłącznie Etap 1: przeniesienie backendu do katalogu `backend/` i aktualizację
wszystkich plików referencjonujących ścieżki `src/`.

**Stan po zakończeniu Etapu 1:**
- `backend/src/` — cały backend .NET
- `backend/ThingsBooksy.slnx` — plik rozwiązania
- `src/`, `ThingsBooksy.slnx` w root — usunięte przez `git mv`
- Wszystkie komendy dotnet i ścieżki agentów zaktualizowane do nowej struktury
- Budowanie, testy i Docker działają bez zmian

**Decyzja architektoniczna:** `monorepo-decision.md` w root repo.

---

## Aktualna struktura (przed zmianą)

```
ThingsBooksy/
├── ThingsBooksy.slnx          ← przenosi się do backend/
├── src/                       ← przenosi się do backend/src/
├── global.json                ← zostaje w root (Dockerfile czyta z kontekstu root)
├── Dockerfile                 ← zostaje w root, zmienią się tylko COPY paths
├── docker-compose.yml         ← BEZ ZMIAN (kontekst . pozostaje root)
├── CLAUDE.md                  ← aktualizacja ścieżek
├── .claude/agents/            ← aktualizacja ścieżek w 7+ plikach
└── .specify/memory/constitution.md  ← aktualizacja ścieżek
```

---

## Prereqs

1. Branch `main`, czyste working tree: `git status` → brak modyfikacji
2. Potwierdź: `git tag | grep pre-monorepo` — nie powinien istnieć (tag będziemy tworzyć)

---

## Krok 1: Tag bezpieczeństwa

```powershell
git tag pre-monorepo-migration
```

Punkt powrotu. Przy problemach: `git checkout pre-monorepo-migration`.

---

## Krok 2: git mv (jeden atomic commit na koniec)

```powershell
git mv src backend/src
git mv ThingsBooksy.slnx backend/ThingsBooksy.slnx
```

**Dlaczego `ThingsBooksy.slnx` nie wymaga aktualizacji wewnętrznych ścieżek:**
Plik `.slnx` referencjonuje projekty relatywnie do swojej lokalizacji. Po przeniesieniu
z `ThingsBooksy.slnx` do `backend/ThingsBooksy.slnx` relatywna ścieżka `src/...` nadal
wskazuje na `backend/src/` — bez zmian w pliku.

---

## Krok 3: Dockerfile

Plik zostaje w root. Zmiana: ścieżki `COPY` wskazują teraz na `backend/`. Wewnętrzna
struktura kontenera NIE zmienia się — `src/` w kontenerze nadal będzie `src/`.

**Plik:** `Dockerfile`

```dockerfile
# Przed (linia 5-6):
COPY ThingsBooksy.slnx global.json ./
COPY src/ src/

# Po:
COPY backend/ThingsBooksy.slnx global.json ./
COPY backend/src/ src/
```

Linie 8–13 (`dotnet restore`, `dotnet publish`) — BEZ ZMIAN. Wewnętrzna struktura kontenera
identyczna jak dotąd (`ThingsBooksy.slnx` i `src/` w `/repo/`).

---

## Krok 4: CLAUDE.md

**Plik:** `CLAUDE.md` (root)

Sekcja `### Build & run`:
```markdown
# Przed:
- Build solution: `dotnet build`
- Run application: `dotnet run --project src\Bootstrapper\ThingsBooksy.Bootstrapper`

# Po:
- Build solution: `dotnet build backend\`
- Run application: `dotnet run --project backend\src\Bootstrapper\ThingsBooksy.Bootstrapper`
```

Sekcja `### Test`:
```markdown
# Przed:
- Run all tests: `dotnet test`
- Run single test: `dotnet test <path-to-test-csproj> --filter ...`

# Po:
- Run all tests: `dotnet test backend\`
- Run single test: `dotnet test backend\<path-to-test-csproj> --filter ...`
```

Sekcja `### Format`:
```markdown
# Przed:
- Format: `dotnet format`
- Verify only (CI): `dotnet format --verify-no-changes`

# Po:
- Format: `dotnet format backend\`
- Verify only (CI): `dotnet format backend\ --verify-no-changes`
```

Sekcja `### EF Core migrations (per module)`:
```markdown
# Przed:
- Add: `dotnet ef migrations add {Name} --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper`
- Apply: `dotnet ef database update --project src\Modules\{Module}\{Module}.Migrations --startup-project src\Bootstrapper\ThingsBooksy.Bootstrapper`

# Po:
- Add: `dotnet ef migrations add {Name} --project backend\src\Modules\{Module}\{Module}.Migrations --startup-project backend\src\Bootstrapper\ThingsBooksy.Bootstrapper`
- Apply: `dotnet ef database update --project backend\src\Modules\{Module}\{Module}.Migrations --startup-project backend\src\Bootstrapper\ThingsBooksy.Bootstrapper`
```

Sekcja `## Key files` (na końcu CLAUDE.md):
```markdown
# Przed:
- `src/Bootstrapper/ThingsBooksy.Bootstrapper` — startup composition and module registration
- `src/Shared/ThingsBooksy.Shared.Abstractions` — shared event/message contracts

# Po:
- `backend/src/Bootstrapper/ThingsBooksy.Bootstrapper` — startup composition and module registration
- `backend/src/Shared/ThingsBooksy.Shared.Abstractions` — shared event/message contracts
```

---

## Krok 5: constitution.md

**Plik:** `.specify/memory/constitution.md`

Wykonaj replace dla następujących wzorców (forward slash):

| Przed | Po |
|---|---|
| `src/Modules/` | `backend/src/Modules/` |
| `src/Shared/` | `backend/src/Shared/` |
| `src/Bootstrapper/` | `backend/src/Bootstrapper/` |

Dotyczy konkretnie tych linii (numery mogą się nieznacznie różnić po edycji):
- L7: `src/Modules/{ModuleName}/` (wzorzec modułu)
- L9: `src/Shared/` (shared infrastructure)
- L31: `src/Shared/ThingsBooksy.Shared.Abstractions/` (event contracts)
- L45: komenda EF migrations (dwie ścieżki w jednej linii)

---

## Krok 6: Pliki agentów w .claude/agents/

**Agenty z hardkodowanymi ścieżkami `src/` (7 krytycznych):**

| Plik | Liczba referencji |
|---|---|
| `architecture-guard.md` | ~18 |
| `contract-definer.md` | ~12 |
| `module-writer.md` | ~10 |
| `integration-test-writer.md` | ~8 |
| `quality-reviewer.md` | ~7 |
| `migration-agent.md` | ~2 |
| `plan-validator.md` | sprawdzić |

**Wzorce do zamiany w każdym pliku** (czytaj plik, potem edytuj):

Ścieżki forward-slash (instrukcje tekstowe):
- `src/Modules/` → `backend/src/Modules/`
- `src/Shared/` → `backend/src/Shared/`
- `src/Bootstrapper/` → `backend/src/Bootstrapper/`

Ścieżki backslash (komendy PowerShell/dotnet):
- `src\Modules\` → `backend\src\Modules\`
- `src\Shared\` → `backend\src\Shared\`
- `src\Bootstrapper\` → `backend\src\Bootstrapper\`

Glob patterns w instrukcjach agentów:
- `src/**/*.cs` → `backend/src/**/*.cs`
- `src/Modules/**` → `backend/src/Modules/**`
- `src/Shared/**` → `backend/src/Shared/**`

Komendy dotnet (standalone — bez specyficznego projektu):
- `dotnet build` (gdy standalone polecenie rozwiązania) → `dotnet build backend\`
- `dotnet test` (gdy standalone) → `dotnet test backend\`
- `dotnet format` (gdy standalone) → `dotnet format backend\`

**Uwaga:** Nie zamieniaj automatycznie `dotnet build` jeśli następuje bezpośrednio po nim
ścieżka do projektu — edytuj tylko standalone wywołania. Czytaj kontekst każdego pliku.

Komendy EF migrations w agentach (migration-agent.md):
- Analogicznie do zmian w CLAUDE.md — dodaj prefiks `backend\` do obu ścieżek `--project` i `--startup-project`

---

## Krok 7: .gitignore

Dodaj na końcu pliku `.gitignore`:

```gitignore
# Node / Angular (frontend)
frontend/node_modules/
frontend/.angular/
frontend/dist/
frontend/.env
```

---

## Krok 8: Weryfikacja

```powershell
# Build backendu
dotnet build backend\

# Testy
dotnet test backend\

# Format check
dotnet format backend\ --verify-no-changes

# Docker (opcjonalnie)
wsl docker compose build
```

Oczekiwane rezultaty:
- `dotnet build backend\` → `Build succeeded`
- `dotnet test backend\` → wszystkie testy passed
- `dotnet format backend\ --verify-no-changes` → brak błędów formatowania
- Docker build → `Successfully built`

---

## Krok 9: Atomic commit

```powershell
git add -A
git commit -m "chore: migrate to monorepo structure — move backend to backend/"
```

---

## Pułapki do uniknięcia

1. **Nie ruszaj `global.json`** — zostaje w root; Dockerfile kopiuje go z kontekstu root
2. **Nie zmieniaj `docker-compose.yml`** — context pozostaje `.` (root), Dockerfile jest w root
3. **Wewnętrzne ścieżki w kontenerze** — po `COPY backend/src/ src/` kontener widzi `src/` bez `backend/`; `dotnet restore ThingsBooksy.slnx` w Dockerfile BEZ zmian
4. **Ścieżki w `.slnx`** — relatywne do lokalizacji pliku, nie wymagają edycji
5. **Namespace'y C#** — bez zmian
