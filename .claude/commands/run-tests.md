---
model: claude-haiku-4-5-20251001
---

> **Użycie:**
> ```
> /integration-tests                                         # wszystkie testy integracyjne
> /integration-tests users                                   # tylko moduł Users
> /integration-tests mg                                      # tylko moduł ManagementGroups
> /integration-tests AddGroupMemberTests                     # konkretna klasa testowa
> /integration-tests AddMember_WhenUserNotExists_Returns404  # konkretna metoda
> ```

Run integration tests for ThingsBooksy.

**Docker requirement:** Docker runs in WSL (not Docker Desktop). Always set `$env:DOCKER_HOST = "tcp://localhost:2375"` in PowerShell before calling `dotnet test`.

**Project paths:**
- Users: `src\Modules\Users\ThingsBooksy.Modules.Users.IntegrationTests\ThingsBooksy.Modules.Users.IntegrationTests.csproj`
- ManagementGroups: `src\Modules\ManagementGroups\ThingsBooksy.Modules.ManagementGroups.IntegrationTests\ThingsBooksy.Modules.ManagementGroups.IntegrationTests.csproj`

**Always use:** `--logger "console;verbosity=normal"`

---

Based on the argument provided ($ARGUMENTS), run tests as follows:

- **No argument** — run Users project first, then ManagementGroups (two sequential `dotnet test` calls)
- **`users`** — run Users project only
- **`managementgroups`** or **`mg`** — run ManagementGroups project only
- **Any other value** — treat as a `--filter` value using `FullyQualifiedName~<value>`; if the value clearly maps to one module (e.g. contains "ManagementGroup" or "Member"), run only that module's project, otherwise run both
