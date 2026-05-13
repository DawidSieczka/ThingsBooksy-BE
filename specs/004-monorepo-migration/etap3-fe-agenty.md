# Etap 3 — Projekt agentów FE

## Kontekst

Ten plik zakłada, że **Etap 1 i Etap 2 są wdrożone** — monorepo działa, Angular scaffold istnieje
w `frontend/`, docker-compose uruchamia oba serwisy.

Etap 3 to sesja projektowa: zaprojektowanie i dodanie nowych agentów do floty dla pracy z frontendem.
Każdy agent jest projektowany osobno przez `agent-architect`, a następnie zapisywany w `.claude/agents/`.

---

## Agenty do zaprojektowania

### 1. `fe-component-writer`

**Cel:** Implementuje komponenty Angular — generuje `.component.ts`, `.component.html`,
`.component.scss`, `.component.spec.ts` zgodnie z konwencjami projektu.

**Przed projektowaniem zapytaj użytkownika o:**
- Jakie konwencje Angular mają obowiązywać (standalone components vs NgModule, signals vs RxJS?)
- Czy są już istniejące komponenty w `frontend/src/app/` do wzorowania?
- Czy agent ma pisać testy jednostkowe (Jasmine) czy e2e (Playwright)?

**Wzorce agenta do zastosowania:**
- Analogia do `module-writer` — agent C# generuje moduł z konwencjami; `fe-component-writer` generuje komponent z konwencjami Angular
- Powinien czytać `frontend/angular.json` żeby znać konfigurację projektu
- Narzędzia: `Glob`, `Read`, `Write`, `Edit`, `Bash` (ng generate, npm test)

---

### 2. `fe-api-client-writer`

**Cel:** Generuje typowany klient HTTP TypeScript na podstawie `swagger.json` z backendu
(source of truth: Swashbuckle pod `localhost:8080/swagger/v1/swagger.json` lub plik statyczny).

**Przed projektowaniem zapytaj użytkownika o:**
- Preferowana biblioteka do generowania: `swagger-typescript-api`, `openapi-generator`, czy ręcznie?
- Czy klient ma używać `HttpClient` Angulara, czy fetch API?
- Gdzie generowane pliki trafiają (`frontend/src/app/api/` lub inaczej)?

**Wzorce agenta do zastosowania:**
- Agent powinien umieć pobrać aktualny `swagger.json` (wymagane uruchomione API lub plik statyczny)
- Generuje lub aktualizuje pliki klienta po każdej zmianie kontraktu
- Narzędzia: `Read`, `Write`, `Bash` (npm run generate lub curl swagger.json)

---

### 3. `html-extractor`

**Cel:** Nieznany z bieżącej sesji — odwołanie do wcześniejszej dyskusji o tym agencie.

**Na początku sesji zapytaj użytkownika:**
> "Widzę wzmiankę o `html-extractor` w planie monorepo, ale nie mam kontekstu z wcześniejszej
> dyskusji. Czym ma się zajmować ten agent?"

Możliwe interpretacje (do potwierdzenia):
- Wyodrębnianie kodu HTML z komponentów Angular do osobnych plików szablonów
- Parsowanie istniejącego HTML (np. mockupów Figma to HTML) na komponenty Angular
- Ekstrakcja fragmentów HTML z zewnętrznych stron do komponentów

---

## Procedura projektowania (dla każdego agenta)

Użyj `agent-architect` dla każdego agenta osobno:

```
Uruchom agent-architect z następującym briefem:

"Zaprojektuj agenta [nazwa] dla projektu ThingsBooksy (monorepo Angular + .NET).

Projekt używa:
- Angular 19+ (standalone components)
- Backend .NET 10 z Swashbuckle (swagger.json generowany automatycznie)
- Fleet Claude Code agentów w .claude/agents/
- Konwencje agentów: [opisz co agent ma robić]

Wygeneruj plik agenta gotowy do zapisu w .claude/agents/[nazwa].md"
```

Po zaakceptowaniu projektu przez `agent-architect`:
1. Agent zapisuje plik w `.claude/agents/[nazwa].md`
2. Zaktualizuj tabelę agentów w `CLAUDE.md` (sekcja `## Agent fleet`, tabela `### Known agents`)

---

## Aktualizacja CLAUDE.md po dodaniu agentów

Po zaprojektowaniu każdego agenta dodaj wiersz do tabeli w `CLAUDE.md`:

```markdown
| `fe-component-writer` | Implementuje komponenty Angular — generuje .ts, .html, .scss, .spec.ts |
| `fe-api-client-writer` | Generuje typowany klient HTTP TypeScript z swagger.json backendu |
| `html-extractor` | [opis po wyjaśnieniu z użytkownikiem] |
```

---

## Kolejność projektowania

Rekomendowana kolejność ze względu na zależności:
1. `fe-api-client-writer` — fundamentalny, każdy komponent korzysta z klienta
2. `fe-component-writer` — buduje na kliencie HTTP
3. `html-extractor` — po wyjaśnieniu zakresu

---

## Uwagi

- Konwencje Angular dla projektu nie są jeszcze zdefiniowane — przed projektowaniem agentów
  rozważ uruchomienie `convention-writer` dla Angular (analogia do istniejących `.claude/conventions/`)
- Istniejące konwencje BE są w `.claude/conventions/` — mogą służyć jako wzorzec dla FE conventions
