---
name: integration-test-infrastructure
description: Integration test infrastructure for each module consists of three components: a TestClient (one per module, HTTP + DB methods), Factories (one per entity needing direct DB insertion), and an IntegrationTestCollection (one per module, per-module ICollectionFixture definition). Each test creates its own user to avoid shared state.
metadata:
  type: project
---

## Rule

Every module's integration test project requires three infrastructure components. They are created once per module and extended as the module grows.

---

### 1. TestClient — `Clients/{ModuleName}TestClient.cs`

One TestClient per module. It wraps `HttpClient` for HTTP calls and `ThingsBooksyWebAppFactory` for direct DB reads.

**Constructor:**
```csharp
public {ModuleName}TestClient(ThingsBooksyWebAppFactory factory, AuthenticatedUser user)
{
    _factory = factory;
    _client = user.Client;
}
```

**HTTP methods** — one per endpoint, return `Task<HttpResponseMessage>`. Callers assert the status code themselves.
```csharp
public Task<HttpResponseMessage> Create{Entity}Async({params})
    => _client.PostAsJsonAsync("/{module-name}", new { ... });

public Task<HttpResponseMessage> Get{Entity}Async(Guid id)
    => _client.GetAsync($"/{module-name}/{id}");
```

**Composite helpers** — for every POST that returns an ID, add a helper that asserts success and extracts the ID:
```csharp
public async Task<Guid> Create{Entity}AndGetIdAsync({params})
{
    var response = await Create{Entity}Async({params});
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<Create{Entity}Response>();
    return result!.Id;
}
```

**DB methods** — `internal` visibility, always call `.IgnoreQueryFilters()`:
```csharp
internal async Task<{Entity}?> Get{Entity}FromDbAsync(Guid id)
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<{ModuleName}DbContext>();
    return await db.{Entities}
        .IgnoreQueryFilters()   // always — sees soft-deleted records
        .FirstOrDefaultAsync(x => x.Id == id);
}
```

`IgnoreQueryFilters()` is mandatory on all DB methods in TestClient. Tests that verify soft-delete behavior must be able to read deleted records. Without it, a `GetAsync` returning `null` is ambiguous — the record may not exist at all, or it may be soft-deleted.

---

### 2. Factories — `Clients/{Entity}Factory.cs`

One Factory per domain entity that tests need to insert directly into the DB (preconditions in the Arrange phase that must not depend on HTTP endpoints).

**Standard entity factory:**
```csharp
public sealed class {ModuleName}{Entity}Factory
{
    private readonly ThingsBooksyWebAppFactory _factory;

    public {ModuleName}{Entity}Factory(ThingsBooksyWebAppFactory factory)
        => _factory = factory;

    public async Task<{Entity}> Create{Entity}Async(/* minimum required params */)
    {
        var entity = {Entity}.Create(/* params */);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<{ModuleName}DbContext>();
        db.{Entities}.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }
}
```

Always use `Guid.CreateVersion7()`. `Guid.NewGuid()` is forbidden (see [[domain-entity-design]]).

**User/read-model factory** — when the module stores a `UserReadModel`, the factory must also generate a signed JWT and return `AuthenticatedUser`. The JWT is built from `AuthOptions` retrieved from the DI container — never hardcode signing keys.

```csharp
public async Task<AuthenticatedUser> CreateUserAsync(string email)
{
    var userId = Guid.CreateVersion7();

    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<{ModuleName}DbContext>();
    db.UserReadModels.Add(new UserReadModel { Id = userId, Email = email });
    await db.SaveChangesAsync();

    var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.IssuerSigningKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new(JwtRegisteredClaimNames.UniqueName, userId.ToString()),
        new(JwtRegisteredClaimNames.Email, email),
        new(JwtRegisteredClaimNames.Aud, authOptions.Jwt.Audience),
        new(ClaimTypes.Role, "user"),
    };

    var token = new JwtSecurityToken(
        issuer: authOptions.Jwt.Issuer,
        claims: claims,
        notBefore: DateTime.UtcNow,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", tokenString);

    return new AuthenticatedUser(client, userId, email);
}
```

`AuthenticatedUser` is defined in `ThingsBooksy.Shared.IntegrationTests.Clients` — do not redefine it per module.

---

### 3. IntegrationTestCollection — root of each IntegrationTests project

Each module defines its own `IntegrationTestCollection` at the root of its IntegrationTests project. Do not share or import the collection from another module or from `ThingsBooksy.Shared.IntegrationTests`.

```csharp
using ThingsBooksy.Shared.IntegrationTests;
using Xunit;

namespace ThingsBooksy.Modules.{ModuleName}.IntegrationTests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<ThingsBooksyWebAppFactory>
{
}
```

Test classes reference it with `[Collection("IntegrationTestCollection")]`.

---

### Test isolation

`{ModuleName}TestClient` requires an `AuthenticatedUser`. Instantiate the user and the client **inside each test method** — never in the constructor or as a shared field.

```csharp
[Fact]
public async Task Create{Entity}_WithValidData_Returns201AndPersistsInDb()
{
    // Arrange — user and client created per test, unique email prevents collisions
    var user = await _users.CreateUserAsync("create_entity_happypath@test.com");
    var client = new {ModuleName}TestClient(Factory, user);

    // Act
    var response = await client.Create{Entity}Async(...);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

Use unique, scenario-specific email addresses (e.g., `"featurename_scenario@test.com"`) to avoid collisions even if Respawn does not clean between tests.

Never share state between tests through static fields or class-level mutable fields.

---

## Rationale

**TestClient** centralises route strings and request shapes in one place. Tests do not contain raw URL strings — a route change requires updating only the TestClient, not every test method that calls it. Returning `HttpResponseMessage` from HTTP methods gives each test full control over assertions, including asserting on error response bodies.

**Factories** decouple test preconditions from the HTTP layer. If a test for `DELETE /bookings/{id}` fails because the preceding `POST /bookings` was broken, the test failure diagnosis is misleading. Direct DB insertion via a Factory eliminates that ambiguity.

**IntegrationTestCollection per module** isolates test runs. xUnit shares the `ThingsBooksyWebAppFactory` fixture within a collection; a per-module collection means tests in different modules do not compete for the same fixture instance.

**Per-test user creation** prevents inter-test state leakage. A user created in test A and left in the DB could affect the query results in test B if Respawn has not run. Creating a fresh user per test eliminates this category of flakiness.
