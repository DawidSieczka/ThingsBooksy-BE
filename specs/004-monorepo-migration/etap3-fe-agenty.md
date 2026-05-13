# Etap 3 — Projekt agentów FE

## Kontekst

Etap 1 (BE reorganizacja) i Etap 2 (Angular scaffold) są wdrożone.
Monorepo działa: `backend/` + `frontend/` (Angular 21, standalone, SCSS).

Etap 3 to 4 niezależne sesje projektowe. Każda sesja zaczyna od odpowiedniego agenta.
Wszystkie decyzje techniczne zostały podjęte — sesje nie wymagają dodatkowych pytań do użytkownika
(z wyjątkiem interaktywnej walidacji przez convention-writer i agent-architect).

---

## Podjęte decyzje techniczne

| Obszar | Decyzja | Rationale |
|---|---|---|
| Stan lokalny | `signal()` | Angular 21 domyślny kierunek, prostszy niż RxJS |
| Stan współdzielony | Proste serwisy z `signal()` na start | Nie komplikuj dopóki nie potrzeba NgRx |
| HTTP | `HttpClient` + `toSignal()` + functional interceptors | Signals bridge dla HTTP observables |
| Formularze | Reactive Forms (typowane `FormControl<T>`) | Aplikacja biznesowa |
| Testy | Vitest (domyślny Angular 21) | Karma wychodzi |
| Folder structure | Feature-based (`features/`, `core/`, `shared/`, `api/`) | Oficjalna rekomendacja |
| File naming | kebab-case (`user-profile.component.ts`) | Angular style guide |
| DI pattern | `inject()` zamiast constructor params | Nowszy, czystszy styl |
| CSS | SCSS + CSS custom properties jako design tokens | Już skonfigurowane |
| Animacje | CSS transitions dla prostych, `@angular/animations` dla złożonych | Performance |
| API client | `swagger-typescript-api` (npm-based, brak Java) | Jak NSwag dla .NET |
| Komponenty | Standalone (domyślne w Angular 21) | Brak NgModule w nowym projekcie |

---

## Docelowa struktura folderów (do zakodowania w konwencji)

```
frontend/src/app/
├── core/                  ← guards, interceptors, global services
│   ├── interceptors/
│   ├── guards/
│   └── services/
├── features/              ← jeden folder per feature/moduł BE
│   ├── resources/
│   │   ├── resources.routes.ts
│   │   ├── list/
│   │   └── detail/
│   ├── bookings/
│   └── users/
├── shared/                ← reużywalne komponenty bez logiki domenowej
│   ├── components/
│   └── pipes/
├── api/                   ← generowany klient HTTP (swagger-typescript-api)
└── styles/
    ├── _tokens.scss       ← design tokens (kolory, spacing, typografia)
    └── _base.scss
```

---

## Kolejność sesji

### Sesja A — Angular conventions (convention-writer)

**Agent:** `convention-writer`

Uruchom z następującym briefem:
> "Chcę zdefiniować konwencje dla projektu Angular 21 (frontend/ w monorepo ThingsBooksy).
> Tech stack: Angular 21, standalone components, Signals, Reactive Forms, Vitest, SCSS.
> Zacznij od folder structure i file naming — to są najważniejsze konwencje na start."

Konwencje do stworzenia w `.claude/conventions/` (analogia do konwencji BE):
- `angular-folder-structure.md` — feature-based layout, core/shared/features/api
- `angular-component-design.md` — standalone, inject(), Signals, file naming
- `angular-http-pattern.md` — HttpClient services, toSignal(), functional interceptors
- `angular-forms-pattern.md` — Reactive Forms, typowane FormControl<T>
- `angular-styling.md` — SCSS tokens, CSS custom properties, component scoping

### Sesja B — fe-api-client-writer (agent-architect)

**Agent:** `agent-architect`

Brief dla agent-architect:
> "Zaprojektuj agenta `fe-api-client-writer` dla projektu ThingsBooksy.
>
> Kontekst projektu:
> - Monorepo: backend (.NET 10 + Swashbuckle) w backend/, frontend (Angular 21) w frontend/
> - swagger.json dostępny pod localhost:8080/swagger/v1/swagger.json (gdy backend uruchomiony)
> - Generowanie klienta przez: swagger-typescript-api (npm package)
> - Wygenerowane pliki trafiają do: frontend/src/app/api/
> - Agent działa z cwd: root repozytorium
>
> Co agent robi:
> 1. Sprawdza czy backend jest uruchomiony (curl swagger.json) lub prosi o lokalny plik
> 2. Uruchamia: npx swagger-typescript-api -p <swagger-url> -o frontend/src/app/api/ --axios false --modular
> 3. Przegląda wygenerowane pliki i dostosowuje do konwencji projektu (inject(), Angular HttpClient)
> 4. Raportuje co zostało wygenerowane
>
> Narzędzia agenta: Read, Write, Edit, Bash
> Plik agenta: .claude/agents/fe-api-client-writer.md"

### Sesja C — html-extractor (agent-architect)

**Agent:** `agent-architect`

Brief dla agent-architect:
> "Zaprojektuj agenta `html-extractor` dla projektu ThingsBooksy.
>
> Co robi:
> - Analizuje jeden plik .html wygenerowany przez Claude Design (artefakt designu)
> - Wyciąga: kolory, typografię, spacing, animacje, responsywność, elementy UI (formularze, przyciski, karty)
> - Proponuje ekstrakcję design tokens do frontend/src/styles/_tokens.scss
> - Przeprowadza interview z developerem (tabelaryczny format - wszystkie funkcje naraz):
>   | Funkcjonalność | Endpoint | Status (✅/❌/⚠️) | Uwagi |
> - Przeprowadza audyt dostępności (kontrast, ARIA, focus)
> - Proponuje dekompozycję HTML na drzewo komponentów Angular (które fragmenty → które komponenty)
> - Mapuje zaimplementowane funkcje na konkretne API endpointy z swagger.json
> - Tworzy inwentarz assetów (ikony, obrazy, fonty)
> - Decyduje: CSS transitions vs @angular/animations
> - Planuje routing Angular (które sekcje → które routes)
> - Identyfikuje stany UI (loading, error, empty state) dla każdego elementu interaktywnego
>
> Output agenta (po pełnej analizie i zatwierdzeniu przez developera):
> Strukturowany plan dla fe-component-writer zawierający:
> - Listę komponentów do stworzenia (nazwa, ścieżka, typ: smart/dumb)
> - Design tokens do wygenerowania w _tokens.scss
> - Mapowanie komponent → endpoint API
> - Plan routingu
> - Specyfikację stanów UI
> - Uwagi dostępnościowe
>
> Checklist zakończenia (agent nie kończy bez wszystkich pozycji):
> - [ ] Wszystkie kolory/spacing wyekstrahowane jako tokeny
> - [ ] Drzewo komponentów zaproponowane i zatwierdzone
> - [ ] Każda interaktywna funkcja ma status backend
> - [ ] Plan routingu gotowy
> - [ ] Audyt dostępności wykonany
>
> Język agenta: angielski
> Narzędzia: Read, Glob, WebSearch (opcjonalnie dla ikon/fontów)
> Plik agenta: .claude/agents/html-extractor.md"

### Sesja D — fe-component-writer (agent-architect)

**Agent:** `agent-architect`

Brief dla agent-architect:
> "Zaprojektuj agenta `fe-component-writer` dla projektu ThingsBooksy.
>
> Kontekst:
> - Angular 21, standalone components, Signals, Reactive Forms, Vitest
> - Konwencje w .claude/conventions/angular-*.md
> - Input: plan z html-extractor (lista komponentów, design tokens, API mapping)
> - API klient dostępny w frontend/src/app/api/ (wygenerowany przez fe-api-client-writer)
>
> Co agent robi:
> - Implementuje jeden komponent na wywołanie (nazwa jako argument)
> - Czyta konwencje Angular z .claude/conventions/
> - Generuje: .component.ts, .component.html, .component.scss, .component.spec.ts
> - Używa inject() zamiast constructor, signal() dla stanu, HttpClient przez api/ serwisy
> - Pisze testy Vitest dla logiki komponentu
> - Uruchamia: ng build --watch (weryfikacja kompilacji)
>
> Narzędzia: Glob, Read, Write, Edit, Bash (ng build, npm test)
> Plik agenta: .claude/agents/fe-component-writer.md"

---

## Po każdej sesji

Zaktualizuj tabelę agentów w `CLAUDE.md` (sekcja `### Known agents`):

```markdown
| `fe-api-client-writer` | Generuje TypeScript klienta HTTP z swagger.json przez swagger-typescript-api → frontend/src/app/api/ |
| `html-extractor` | Analizuje HTML z Claude Design: design tokens, dekompozycja komponentów, backend readiness interview, plan dla fe-component-writer |
| `fe-component-writer` | Implementuje komponenty Angular (standalone, Signals, Reactive Forms) na podstawie planu z html-extractor |
```
