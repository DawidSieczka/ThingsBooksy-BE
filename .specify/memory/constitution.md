# Konstytucja ThingsBooksy

## Zasady Nadrzędne

### I. Architektura Modularnego Monolitu (NIEPODWAŻALNE)
Aplikacja jest zbudowana jako **Modularny Monolit**: jeden deployowalny artefakt złożony z niezależnych, samodzielnych modułów.
- Każdy moduł znajduje się w `src/Modules/{NazwaModułu}/` i zawiera dokładnie dwa projekty: `{Nazwa}.Api` i `{Nazwa}.Core`
- Moduły **nie mogą** bezpośrednio referencjonować się nawzajem — komunikacja odbywa się wyłącznie przez `IMessageBroker` (zdarzenia) lub `IModuleClient` (zapytania)
- Współdzielona infrastruktura należy wyłącznie do `src/Shared/` — brak zależności między modułami
- Każdy moduł wystawia swój publiczny kontrakt przez `IModule` i rejestruje endpointy w `Expose(IEndpointRouteBuilder)`

### II. Uproszczone DDD (NIEPODWAŻALNE)
Projekt stosuje **Uproszczone DDD** — brak oddzielnych warstw Application i Infrastructure.
- Każdy moduł ma dokładnie dwa projekty: `{Nazwa}.Api` (warstwa HTTP, Minimal API) i `{Nazwa}.Core` (domena, persystencja, komendy, zapytania, zdarzenia)
- `Core` zawiera: encje domenowe, EF `DbContext`, handlery komend/zapytań, zdarzenia domenowe, obiekty wartości
- `Api` zawiera: rejestrację `IModule`, definicje endpointów, DTO (rekordy request/response), plik konfiguracyjny JSON modułu
- Brak MediatR — komendy/zapytania to zwykłe klasy C# dispatchowane przez `IDispatcher`

### III. Minimal API Endpoints (NIEPODWAŻALNE)
Wszystkie endpointy HTTP są definiowane przy użyciu **ASP.NET Core Minimal APIs** — brak kontrolerów MVC.
- Endpointy rejestrowane w `{NazwaModułu}Module.Expose()` dla każdego modułu
- Prefix routy wg wzorca: `/{nazwa-modułu}/...`
- Zawsze wywoływać `services.AddEndpointsApiExplorer()` aby Swagger odkrył endpointy Minimal API
- Swagger/OpenAPI musi być dostępny na `/swagger` we wszystkich środowiskach

### IV. Komunikacja Między Modułami przez Zdarzenia
Moduły komunikują się przez zdarzenia domenowe publikowane przez `IMessageBroker`.
- Publikowanie: `await _messageBroker.PublishAsync(new CośSięStałoEvent(...))`
- Subskrypcja: implementacja `IEventHandler<TEvent>` i rejestracja w `Register(IServiceCollection)` modułu
- Jeśli moduł potrzebuje danych z innego modułu, subskrybuje zdarzenia i zapisuje **read model** (lokalną kopię) — nigdy nie odpytuje bazy danych innego modułu
- Kontrakty zdarzeń w `src/Shared/ThingsBooksy.Shared.Abstractions/` — brak typów specyficznych dla modułu w treści zdarzeń

### V. Podejście Test-First
Nowe funkcjonalności i poprawki błędów wymagają testów przed implementacją.
- Testy jednostkowe logiki domenowej (encje, obiekty wartości, handlery)
- Testy integracyjne dla interakcji z bazą danych (EF Core, migracje)
- Brak testów = brak merge'a dla zmian logiki biznesowej
- Projekty testowe: `{NazwaModułu}.Tests.Unit` i `{NazwaModułu}.Tests.Integration`

### VI. Persystencja i Migracje
Każdy moduł posiada własny **EF Core DbContext** z izolacją schematu.
- Nazewnictwo schematu: `users` dla modułu Users, `{moduł}` dla pozostałych
- Migracje w dedykowanym projekcie `{NazwaModułu}.Migrations`
- Komenda migracji: `dotnet ef migrations add {Nazwa} --project src/Modules/{M}/{M}.Migrations --startup-project src/Bootstrapper/ThingsBooksy.Bootstrapper`
- Zawsze uruchamiać `dotnet ef database update` po stworzeniu migracji

### VII. Prostota i YAGNI
Nie dodawaj abstrakcji, wzorców ani pakietów, jeśli nie rozwiązują aktualnego problemu.
- Preferuj wbudowane funkcje .NET zamiast bibliotek zewnętrznych
- Brak MediatR, AutoMapper ani ciężkich frameworków — zwykły dispatch C# i ręczne mapowanie
- Konfiguracja: `module.{nazwa}.json` per moduł, scalane przez `ConfigureModules()` przy starcie

### VIII. Formatowanie Kodu
Kod jest formatowany narzędziem **`dotnet format`** wbudowanym w .NET SDK.
- Uruchamiaj **przed każdym commitem**: `dotnet format`
- Formatowanie obejmuje: wcięcia, spacje, organizację `using`, styl kodu zgodny z `.editorconfig`
- Jeśli plik `.editorconfig` nie istnieje w katalogu głównym — należy go utworzyć
- CI/CD powinno weryfikować formatowanie: `dotnet format --verify-no-changes`

### IX. Encje Domenowe — Hermetyzacja (NIEPODWAŻALNE)
Encje domenowe **muszą** chronić swój stan przez pełną hermetyzację.
- **Prywatne settery**: wszystkie właściwości encji mają `private set` — stan zmienia się wyłącznie przez metody encji
- **Prywatny konstruktor**: konstruktor encji jest `private` — jedynym sposobem tworzenia instancji jest statyczna metoda fabryczna `Create(...)`
- **Metody domenowe**: wszelkie modyfikacje stanu (tworzenie, aktualizacja, usunięcie, przywrócenie) realizowane są przez publiczne metody encji (`Create`, `Update`, `Delete`, `Restore` itp.)
- Przykład wzorca:
  ```csharp
  internal class SomeEntity
  {
      public Guid Id { get; private set; }
      public string Name { get; private set; } = null!;

      private SomeEntity() { }

      public static SomeEntity Create(string name)
          => new() { Id = Guid.CreateVersion7(), Name = name };

      public void Update(string name) => Name = name;
  }
  ```

### X. Identyfikatory — GUID v7 (NIEPODWAŻALNE)
Wszystkie identyfikatory **generowane przez aplikację** muszą używać `Guid.CreateVersion7()`.
- `Guid.NewGuid()` jest **zabronione** — zamień każde wystąpienie na `Guid.CreateVersion7()`
- GUID v7 jest time-ordered (lepsza wydajność indeksów w PostgreSQL) i monotoniczny
- Identyfikatory nowych encji generowane są wewnątrz metody `Create(...)` encji
- Identyfikatory wskazujące **relację do istniejącej encji** (np. `OwnerId`, `GroupId`) mogą być przyjmowane z zewnątrz — są to referencje, nie nowe identyfikatory

## Stos Technologiczny

| Warstwa | Technologia |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Język | C# 13 |
| Baza danych | PostgreSQL 17 (Docker) |
| ORM | Entity Framework Core 10 |
| Autentykacja | JWT Bearer + szyfrowanie symetryczne AES-256 |
| Konteneryzacja | Docker / docker-compose (WSL lokalnie) |
| Dokumentacja API | Swashbuckle / Swagger UI |
| Logowanie | Serilog |
| Format rozwiązania | `.slnx` (VS 2022+) |

## Proces Deweloperski

1. **Nowy moduł**: utwórz `{Nazwa}.Api` + `{Nazwa}.Core` + `{Nazwa}.Migrations`, zarejestruj w `Bootstrapper`, dodaj `module.{nazwa}.json`
2. **Nowa funkcjonalność**: specyfikacja (`/speckit.specify`), plan (`/speckit.plan`), zadania (`/speckit.tasks`), implementacja (`/speckit.implement`)
3. **Zmiana bazy danych**: dodaj migrację EF, zaktualizuj bazę, zweryfikuj w Dockerze
4. **Przed commitem**: upewnij się że projekt buduje się, Swagger pokazuje wszystkie endpointy, Docker compose startuje poprawnie

## Docker i Konfiguracja Lokalna

- Lokalny Docker działa przez **WSL** — poprzedzaj każdą komendę docker słowem `wsl`: `wsl docker compose up --build`
- Środowisko: `ASPNETCORE_ENVIRONMENT=Docker` aktywuje `appsettings.Docker.json`
- Connection string do PostgreSQL w `appsettings.Docker.json` musi zawierać nazwę użytkownika i hasło
- Aplikacja dostępna na `localhost:8080`, Swagger pod `localhost:8080/swagger`

## Zarządzanie

- Niniejsza konstytucja ma pierwszeństwo przed wszystkimi innymi praktykami i konwencjami
- Zmiany wymagają aktualizacji tego pliku z uzasadnieniem i inkrementacją wersji
- Wszystkie PR muszą weryfikować zgodność z zasadami Modularnego Monolitu i Uproszczonego DDD
- Każde odejście od wymogu `AddEndpointsApiExplorer()` jest zabronione — zgodność ze Swaggerem jest obowiązkowa

**Wersja**: 1.1.0 | **Ratyfikacja**: 2026-04-23 | **Ostatnia zmiana**: 2026-04-23
