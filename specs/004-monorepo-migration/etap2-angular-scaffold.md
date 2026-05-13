# Etap 2 — Angular scaffold + rozszerzenie docker-compose

## Kontekst

Ten plik zakłada, że **Etap 1 jest wdrożony** — backend jest w `backend/`, wszystkie ścieżki
zaktualizowane, budowanie działa (`dotnet build backend\`).

Etap 2 tworzy szkielet frontendu Angular i integruje go z docker-compose.

**Stan po zakończeniu Etapu 2:**
- `frontend/` — projekt Angular (ng new)
- `docker-compose.yml` — rozszerzony o serwis `frontend`
- `CLAUDE.md` — zaktualizowany o sekcję Frontend commands
- `wsl docker compose up` uruchamia zarówno backend jak i frontend

---

## Prereqs

1. Etap 1 wdrożony: `ls backend/src` powinno pokazać katalogi modułów
2. Angular CLI zainstalowany: `ng version`
   - Jeśli nie: `npm install -g @angular/cli`
3. Node.js ≥ 18: `node --version`

---

## Krok 1: Scaffold projektu Angular

Uruchom z root repo (ThingsBooksy/):

```powershell
ng new thingsbooksy-app --directory frontend --routing true --style scss
```

Odpowiedzi na pytania CLI (jeśli się pojawią):
- SSR/SSG: **No** (SPA architecture)

Po zakończeniu struktura:
```
ThingsBooksy/
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── app.component.*
│   │   │   ├── app.config.ts
│   │   │   └── app.routes.ts
│   │   ├── index.html
│   │   ├── main.ts
│   │   └── styles.scss
│   ├── angular.json
│   ├── package.json
│   ├── tsconfig.json
│   └── tsconfig.app.json
└── ...
```

---

## Krok 2: Weryfikacja frontendu standalone

```powershell
cd frontend
npm start
```

Oczekiwane: serwer na `http://localhost:4200`, strona Angular wyświetla się w przeglądarce.
Zatrzymaj (`Ctrl+C`) przed przejściem dalej.

---

## Krok 3: Dockerfile dla frontendu (development)

Utwórz plik `frontend/Dockerfile.dev`:

```dockerfile
FROM node:22-alpine
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
EXPOSE 4200
CMD ["npm", "start", "--", "--host", "0.0.0.0", "--poll", "1000"]
```

Uwaga: `--host 0.0.0.0` potrzebne, żeby Docker wystawił port na host. `--poll 1000` zapewnia
hot-reload w kontenerze z zamontowanym volume.

---

## Krok 4: Rozszerzenie docker-compose.yml

Dodaj serwis `frontend` do `docker-compose.yml`:

```yaml
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    ports:
      - "4200:4200"
    volumes:
      - ./frontend/src:/app/src
    depends_on:
      - api
```

Pełna sekcja `services` po zmianie powinna zawierać: `api`, `postgres`, `frontend`.

---

## Krok 5: Aktualizacja CLAUDE.md

Dodaj sekcję po istniejącej sekcji `### EF Core migrations`:

```markdown
### Frontend (Angular)
- Install deps: `cd frontend && npm install`
- Dev server: `cd frontend && npm start` (localhost:4200)
- Build: `cd frontend && npm run build`
- Tests: `cd frontend && npm test`
- Lint: `cd frontend && npm run lint`
```

---

## Krok 6: Weryfikacja docker-compose

```powershell
wsl docker compose up --build
```

Oczekiwane:
- `api` dostępne na `localhost:8080`, Swagger na `localhost:8080/swagger`
- `frontend` dostępne na `localhost:4200`
- Brak błędów w logach

---

## Krok 7: Commit

```powershell
git add -A
git commit -m "feat: add Angular frontend scaffold and docker-compose integration"
```

---

## Uwagi dla przyszłego Etapu 3

- Backend API kontrakt: `swagger.json` generowany przez Swashbuckle pod `localhost:8080/swagger/v1/swagger.json`
- Ten plik będzie source of truth dla agenta `fe-api-client-writer` (generuje TypeScript HTTP klienta)
- Rozważyć dodanie `@openapitools/openapi-generator-cli` lub `swagger-typescript-api` do `package.json`
- Patrz: `etap3-fe-agenty.md`
